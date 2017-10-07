/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Pixelaria.Data.Persistence.PixelariaFileBlocks;
using Pixelaria.Utils;

namespace Pixelaria.Data.Persistence
{
    /// <summary>
    /// Encapsulates a Pixelaria .plx file
    /// </summary>
    public class PixelariaFile : GenericFile<FileBlock>
    {
        /// <summary>
        /// The version of this Pixelaria file
        /// </summary>
        protected int version = 9;

        /// <summary>
        /// After discarding temporary blocks, file contents must be re-loaded from the stream
        /// after a Save() call to properly pick the pieces back again.
        /// 
        /// Also required when the file is first created, since no loading has taken place yet.
        /// </summary>
        private bool _requiresReload = true;

        /// <summary>
        /// The Bundle binded to this PixelariaFile.
        /// 
        /// Null, if file is being loaded, not saved to.
        /// </summary>
        [CanBeNull]
        protected Bundle bundle;

        /// <summary>
        /// Gets or sets the version of this PixelariaFile
        /// </summary>
        public int Version { get => version; set => version = value; }

        /// <summary>
        /// Gets or sets a value specifying whether to reset the bundle when loading from disk
        /// </summary>
        public bool ResetBundleOnLoad { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the PixelariaFile class
        /// </summary>
        public PixelariaFile()
        {
            fileHeader = new PixelariaFileHeader();
            blockList = new List<FileBlock>();

            if (bundle != null)
                AddDefaultBlocks();
        }

        /// <summary>
        /// Initializes a new instance of the PixelariaFile class
        /// </summary>
        /// <param name="bundle">The bundle to bind to this PixelariaFile</param>
        /// <param name="filePath">The path to the .plx file to manipulate</param>
        public PixelariaFile([CanBeNull] Bundle bundle, string filePath)
            : this()
        {
            this.filePath = filePath;
            this.bundle = bundle;
            
        }

        /// <summary>
        /// Initializes a new instance of the PixelariaFile class
        /// </summary>
        /// <param name="bundle">The bundle to bind to this PixelariaFile</param>
        /// <param name="stream">The stream used to load/save the PixelariaFile</param>
        public PixelariaFile([CanBeNull] Bundle bundle, Stream stream)
            : this()
        {
            filePath = "";
            this.bundle = bundle;
            this.stream = stream;
        }

        /// <summary>
        /// From the underlying stream, constructs and returns a Bundle object.
        /// </summary>
        public Bundle ConstructBundle()
        {
            if (_requiresReload)
            {
                stream.Position = 0;
                Load();
            }

            // Piece stuff together
            var headerBlock = (PixelariaFileHeader)fileHeader;

            var locBundle = new Bundle(headerBlock.BundleName) {ExportPath = headerBlock.BundleExportPath};
            
            var legacyAnimBlocks = Blocks.OfType<AnimationBlock>().ToArray();
            var animBlocks = Blocks.OfType<AnimationHeaderBlock>().ToArray();
            var sheetBlocks = Blocks.OfType<AnimationSheetBlock>().ToArray();

            // For frames, we group them by animation ID to make things easier when adding them to the respective animations
            var frameBlocks =
                Blocks
                    .OfType<FrameBlock>()
                    .GroupBy(block => block.ReadAnimationId())
                    .ToDictionary(blocks => blocks.Key, blocks => blocks.ToArray());

            // Start by creating animations
            foreach (var block in legacyAnimBlocks)
            {
                if (block.StreamAnimation != null)
                    locBundle.AddAnimation(block.StreamAnimation);
            }

            foreach (var animBlock in animBlocks)
            {
                var anim = animBlock.Animation;

                // Get frames matching the animation's ID
                if (frameBlocks.TryGetValue(anim.ID, out FrameBlock[] frames))
                {
                    foreach (var block in frames)
                    {
                        var frameInfo = block.LoadFrameFromBuffer(anim.Width, anim.Height);
                        var frame = frameInfo.Frame;

                        frame.Animation = anim;
                        anim.Frames.Add(frame);

                        // Add layers now
                        Debug.Assert(frameInfo.Layers != null, "frameInfo.Layers != null");

                        foreach (var frameLayer in frameInfo.Layers)
                        {
                            // Create bitmap from byte[]
                            var bitmap = PersistenceHelper.LoadImageFromBytes(frameLayer.ImageData);

                            var layer = new Frame.FrameLayer(bitmap, frameLayer.Name);
                            frame.Layers.Add(layer);
                            layer.Frame = frame;
                            layer.Index = frame.LayerCount - 1;
                        }

                        // If the block version is prior to 2, update the frame's hash value due to the new way the hash is calculated
                        if (block.BlockVersion < 2)
                            frameInfo.Frame.UpdateHash();
                        else
                        {
                            frameInfo.Frame.SetHash(frameInfo.HashBytes);
                        }
                    }
                }

                locBundle.AddAnimation(anim);
            }

            // Tie in animation sheets now
            foreach (var sheetBlock in sheetBlocks)
            {
                var sheetEntries = sheetBlock.LoadAnimationSheetsFromBuffer();
                foreach (var sheetEntry in sheetEntries)
                {
                    var sheet = sheetEntry.Sheet;
                    var animIds = sheetEntry.AnimationIds;

                    // Try to verify and correct clashing animation sheet IDs
                    if (locBundle.GetAnimationSheetByID(sheet.ID) != null)
                    {
                        Logging.Warning(
                            $"Animation sheet had invalid duplicated ID {sheet.ID}. Attempting to recover by re-setting to -1...",
                            "PixelariaFile");
                        
                        sheet.ID = -1; // Let bundle figure out a new proper unique ID
                    }

                    foreach (var animId in animIds)
                    {
                        var anim = locBundle.GetAnimationByID(animId);

                        Debug.Assert(anim != null, "anim != null");
                        sheet.AddAnimation(anim);
                    }

                    locBundle.AddAnimationSheet(sheet);
                }
            }

            return locBundle;
        }

        /// <summary>
        /// <para>Adds the default block definitions to this PixelariaFile.</para>
        /// <para>The default blocks added are:</para>
        /// <list type="bullet">
        /// <item><description>AnimationBlock</description></item>
        /// <item><description>AnimationSheetBlock</description></item>
        /// <item><description>ProjectTreeBlock</description></item>
        /// </list>
        /// </summary>
        public void AddDefaultBlocks()
        {
            Debug.Assert(bundle != null, "bundle != null");

            foreach (var animation in bundle.Animations)
            {
                AddBlock(new AnimationHeaderBlock(animation));
            }

            if (GetBlocksByType(typeof(AnimationSheetBlock)).Length == 0)
            {
                AddBlock(new AnimationSheetBlock());
            }
            if (GetBlocksByType(typeof(ProjectTreeBlock)).Length == 0)
            {
                AddBlock(new ProjectTreeBlock());
            }
        }

        // 
        // AddBlock override
        // 
        public override void AddBlock([NotNull] FileBlock block)
        {
            base.AddBlock(block);

            block.OwningFile = this;
        }

        /// <summary>
        /// Prepares the blocks with the currently loaded bundle
        /// </summary>
        public void PrepareBlocksWithBundle()
        {
            // Prepare header
            Debug.Assert(bundle != null, "bundle != null");

            ((PixelariaFileHeader)fileHeader).BundleName = bundle.Name;
            ((PixelariaFileHeader)fileHeader).BundleExportPath = bundle.ExportPath;

            // Clear disposable blocks
            for (int i = 0; i < blockList.Count; i++)
            {
                if (blockList[i].RemoveOnPrepare)
                {
                    RemoveBlock(blockList[i]);
                    i--;
                }
            }

            // Re-load default blocks
            AddDefaultBlocks();

            // No for-loop because the block list may be modified during preparation
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < blockList.Count; i++)
            {
                blockList[i].PrepareFromBundle(bundle);
            }
        }

        // 
        // SaveHeader override
        // 
        protected override void SaveHeader()
        {
            // Prepare header
            fileHeader.Version = version;

            Debug.Assert(bundle != null, "bundle != null");

            ((PixelariaFileHeader) fileHeader).BundleName = bundle.Name;
            ((PixelariaFileHeader) fileHeader).BundleExportPath = bundle.ExportPath;

            base.SaveHeader();
        }

        // 
        // SaveBlocks override
        // 
        protected override void SaveBlocks()
        {
            // Prepare the blocks prior to saving them
            PrepareBlocksWithBundle();
            base.SaveBlocks();

            _requiresReload = true;
        }

        // 
        // LoadBlocksFromStream override
        // 
        protected override void LoadBlocksFromStream()
        {
            // Setup the bundle information from the file header
            bundle = new Bundle(((PixelariaFileHeader) fileHeader).BundleName)
            {
                ExportPath = ((PixelariaFileHeader) fileHeader).BundleExportPath
            };
            
            base.LoadBlocksFromStream();

            _requiresReload = false;
        }

        // 
        // AddBlockFromStream override
        // 
        protected override void AddBlockFromStream()
        {
            AddBlock(FileBlock.FromStream(stream, this));
        }

        /// <summary>
        /// Represents the header for a pixelaria file
        /// </summary>
        public class PixelariaFileHeader : FileHeader
        {
            /// <summary>
            /// Gets or sets the name of the bundle
            /// </summary>
            public string BundleName { get; set; }

            /// <summary>
            /// Gets or sets the export path for the bundle
            /// </summary>
            public string BundleExportPath { get; set; }

            /// <summary>
            /// Initializes a new instance of the PixelariaFileHeader class
            /// </summary>
            public PixelariaFileHeader()
            {
                magicNumberBytes = new[] { (byte)'P', (byte)'X', (byte)'L' };
                expectedMagicNumberBytes = new[] { (byte)'P', (byte)'X', (byte)'L' };
            }

            // 
            // Save To Stream override
            // 
            public override void SaveToSteam(Stream stream)
            {
                base.SaveToSteam(stream);

                var writer = new BinaryWriter(stream, Encoding.UTF8);

                writer.Write(BundleName);
                writer.Write(BundleExportPath);
            }

            // 
            // Load From Stream override
            // 
            public override void LoadFromSteam(Stream stream)
            {
                base.LoadFromSteam(stream);

                var reader = new BinaryReader(stream, Encoding.UTF8);

                BundleName = reader.ReadString();
                BundleExportPath = reader.ReadString();
            }
        }
    }
}
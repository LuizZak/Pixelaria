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
using System.IO;
using System.Text;
using Pixelaria.Data.Persistence.PixelariaFileBlocks;

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
        /// The Bundle binded to this PixelariaFile
        /// </summary>
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
        /// Gets the Bundle binded to this PixelariaFile
        /// </summary>
        public Bundle LoadedBundle => bundle;

        /// <summary>
        /// Initializes a new instance of the PixelariaFile class
        /// </summary>
        public PixelariaFile()
        {
            fileHeader = new PixelariaFileHeader();
        }

        /// <summary>
        /// Initializes a new instance of the PixelariaFile class
        /// </summary>
        /// <param name="bundle">The bundle to bind to this PixelariaFile</param>
        /// <param name="filePath">The path to the .plx file to manipulate</param>
        public PixelariaFile(Bundle bundle, string filePath)
            : this()
        {
            this.filePath = filePath;
            this.bundle = bundle;
            blockList = new List<FileBlock>();

            AddDefaultBlocks();
        }

        /// <summary>
        /// Initializes a new instance of the PixelariaFile class
        /// </summary>
        /// <param name="bundle">The bundle to bind to this PixelariaFile</param>
        /// <param name="stream">The stream used to load/save the PixelariaFile</param>
        public PixelariaFile(Bundle bundle, Stream stream)
            : this()
        {
            filePath = "";
            this.bundle = bundle;
            this.stream = stream;
            blockList = new List<FileBlock>();

            AddDefaultBlocks();
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
        public override void AddBlock(FileBlock block)
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
            ((PixelariaFileHeader)fileHeader).BundleName = bundle.Name;
            ((PixelariaFileHeader)fileHeader).BundleExportPath = bundle.ExportPath;

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
        }

        // 
        // LoadBlocksFromStream override
        // 
        protected override void LoadBlocksFromStream()
        {
            if (ResetBundleOnLoad)
            {
                // Reset the bundle beforehands
                bundle.Clear();
            }

            // Setup the bundle information from the file header
            bundle.Name = ((PixelariaFileHeader)fileHeader).BundleName;
            bundle.ExportPath = ((PixelariaFileHeader)fileHeader).BundleExportPath;

            base.LoadBlocksFromStream();
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

                BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);

                writer.Write(BundleName);
                writer.Write(BundleExportPath);
            }

            // 
            // Load From Stream override
            // 
            public override void LoadFromSteam(Stream stream)
            {
                base.LoadFromSteam(stream);

                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);

                BundleName = reader.ReadString();
                BundleExportPath = reader.ReadString();
            }
        }
    }
}
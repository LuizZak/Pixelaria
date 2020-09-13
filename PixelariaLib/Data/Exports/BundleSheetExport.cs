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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using PixelariaLib.Utils;

namespace PixelariaLib.Data.Exports
{
    /// <summary>
    /// Describes an exported Bundle Sheet containing a sequence of animations, and data pertaining to the animations
    /// </summary>
    public class BundleSheetExport : IDisposable
    {
        /// <summary>
        /// The frame sheet
        /// </summary>
        public Image Sheet { get; private set; }

        /// <summary>
        /// Gets the number of frames on this <see cref="BundleSheetExport"/>
        /// </summary>
        public int FrameCount => FrameRects.Length;

        /// <summary>
        /// Gets the number of reused frames on this <see cref="BundleSheetExport"/>
        /// </summary>
        public int ReusedFrameCount { get; private set; }

        /// <summary>
        /// Gets the <see cref="FrameRect"/> for the frame at the given index on this <see cref="BundleSheetExport"/>
        /// </summary>
        /// <param name="i">An index</param>
        /// <returns>The <see cref="FrameRect"/> stored at that index</returns>
        public FrameRect this[int i] => FrameRects[i];

        /// <summary>
        /// The atlas that originated this bundle sheet export
        /// </summary>
        public TextureAtlas Atlas { get; private set; }

        /// <summary>
        /// Gets the array of <see cref="FrameRect"/> objects inside this <see cref="BundleSheetExport"/>
        /// </summary>
        public FrameRect[] FrameRects { get; private set; }

        /// <summary>
        /// Gets the array of frames reused for all the frame rectangle bounds
        /// </summary>
        public int[] ReuseCounts { get; private set; }

        /// <summary>
        /// Gets or sets the export settings to be used when exporting the Bundle Sheet
        /// </summary>
        public AnimationExportSettings ExportSettings { get; private set; }

        /// <summary>
        /// The list of animations in this <see cref="BundleSheetExport"/>
        /// </summary>
        public Animation[] Animations { get; private set; }

        /// <summary>
        /// Default constructor for the <see cref="BundleSheetExport"/> class
        /// </summary>
        private BundleSheetExport()
        {

        }

        ~BundleSheetExport()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of this bundle sheet and all resources allocated by it
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            Atlas.Dispose();
            Sheet.Dispose();
        }

        /// <summary>
        /// Saves the contents of this <see cref="BundleSheetExport"/> to disk, using the given path
        /// as a base path, and saving the .png and .json as that base path
        /// </summary>
        /// <param name="basePath">The base path to export the animations as</param>
        public void SaveToDisk([NotNull] string basePath)
        {
            SaveToDisk(Path.GetFullPath(basePath) + ".png", Path.GetFullPath(basePath) + ".json");
        }

        /// <summary>
        /// Saves the contents of this <see cref="BundleSheetExport"/> to the disk
        /// </summary>
        /// <param name="sheetPath">The path to the sprite sheet to save</param>
        /// <param name="jsonPath">The path to the JSON file containing the data for the animations</param>
        public void SaveToDisk([NotNull] string sheetPath, [NotNull] string jsonPath)
        {
            // Save the sprite sheet first
            Sheet.Save(sheetPath, ImageFormat.Png);

            // Early quit - The json generation is disabled
            if (!ExportSettings.ExportJson)
                return;

            SaveDescriptorToDisk(sheetPath, jsonPath);
        }

        /// <summary>
        /// Saves the animation descriptor to the given path.
        /// The descriptor is a JSON file containing information about the animations and frames exported.
        /// </summary>
        /// <param name="sheetPath">The path of the animation sheet image to reference in the descriptor file</param>
        /// <param name="descriptorPath">The path of the descriptor file</param>
        public void SaveDescriptorToDisk([NotNull] string sheetPath, [NotNull] string descriptorPath)
        {
            var animations = new List<Dictionary<string, object>>();

            foreach (var anim in Animations)
            {
                var animation = new Dictionary<string, object>
                {
                    ["name"] = anim.Name,
                    ["width"] = anim.Width,
                    ["height"] = anim.Height,
                    ["fps"] = anim.PlaybackSettings.FPS,
                    ["frameskip"] = anim.PlaybackSettings.FrameSkip
                };

                // Write down frame bounds now
                var frameBounds = new List<Dictionary<string, object>>();

                for (int i = 0; i < anim.FrameCount; i++)
                {
                    var frame = anim.GetFrameAtIndex(i);

                    if (!ContainsFrame(frame))
                        continue;

                    var rect = GetFrameRectForFrame(frame);
                    Debug.Assert(rect != null, "rect != null");

                    var bounds = new Dictionary<string, object>
                    {
                        ["sheet"] = new Dictionary<string, object>
                        {
                            ["x"] = rect.SheetArea.X - (ExportSettings.UsePaddingOnJson ? ExportSettings.XPadding / 2 : 0),
                            ["y"] = rect.SheetArea.Y - (ExportSettings.UsePaddingOnJson ? ExportSettings.YPadding / 2 : 0),
                            ["width"] = rect.SheetArea.Width + (ExportSettings.UsePaddingOnJson ? ExportSettings.XPadding : 0),
                            ["height"] = rect.SheetArea.Height + (ExportSettings.UsePaddingOnJson ? ExportSettings.YPadding : 0)
                        },
                        ["frame"] = new Dictionary<string, object>
                        {
                            ["x"] = rect.FrameArea.X - (ExportSettings.UsePaddingOnJson ? ExportSettings.XPadding / 2 : 0),
                            ["y"] = rect.FrameArea.Y - (ExportSettings.UsePaddingOnJson ? ExportSettings.YPadding / 2 : 0),
                            ["width"] = rect.FrameArea.Width,
                            ["height"] = rect.FrameArea.Height
                        }
                    };

                    frameBounds.Add(bounds);
                }

                animation["frames"] = frameBounds;

                animations.Add(animation);
            }

            // Root node for JSON
            var root = new Dictionary<string, object>
            {
                ["sprite_image"] = Utilities.GetRelativePath(sheetPath, Path.GetDirectoryName(descriptorPath)),
                ["animations"] = animations
            };
            
            var json = JsonConvert.SerializeObject(root);
            File.WriteAllText(descriptorPath, json, Encoding.UTF8);
        }

        /// <summary>
        /// Returns whether or not the given frame is inside this <see cref="BundleSheetExport"/>
        /// </summary>
        /// <param name="frame">The frame to search for</param>
        /// <returns>True whether the given frame is inside this <see cref="BundleSheetExport"/>, false otherwise</returns>
        public bool ContainsFrame(IFrame frame)
        {
            // Returns true if any of the sequence's frames returns true to an expression
            return FrameRects.Any(frameRect => ReferenceEquals(frameRect.Frame, frame));
        }

        /// <summary>
        /// Returns the FrameRect object that represents the given Frame. If no <see cref="FrameRect"/> represents the given frame,
        /// null is returned.
        /// </summary>
        /// <param name="frame">The Frame to get the corresponding <see cref="FrameRect"/></param>
        /// <returns>The <see cref="FrameRect"/> object that represents the given Frame. If no <see cref="FrameRect"/> represents the given frame, null is returned.</returns>
        [CanBeNull]
        public FrameRect GetFrameRectForFrame(IFrame frame)
        {
            return FrameRects.FirstOrDefault(frameRect => ReferenceEquals(frameRect.Frame, frame));
        }

        /// <summary>
        /// Returns a list of all unique frame regions in the sheet area, and the respective <see cref="FrameRect"/> instances that share the regions.
        /// </summary>
        /// <returns>A list of <see cref="FramesEntry"/> which describe collective <see cref="FrameRect"/> instances that share the same region in the sprite sheet image.</returns>
        public IReadOnlyList<FramesEntry> GetUniqueFrameRegions()
        {
            var map = new Dictionary<Rectangle, List<FrameRect>>();

            foreach (var frameRect in FrameRects)
            {
                if (map.TryGetValue(frameRect.SheetArea, out var list))
                {
                    list.Add(frameRect);
                }
                else
                {
                    map[frameRect.SheetArea] = new List<FrameRect> {frameRect};
                }
            }

            return map.Select(keyValuePair => new FramesEntry(keyValuePair.Key, keyValuePair.Value)).ToList();
        }

        private int HashCodeForRectangle(Rectangle rect)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 3 + rect.X;
                hash = hash * 3 + rect.Y;
                hash = hash * 3 + rect.Width;
                hash = hash * 3 + rect.Height;
                return hash;
            }
        }

        /// <summary>
        /// Creates a new <see cref="BundleSheetExport"/> from a <see cref="TextureAtlas"/>
        /// </summary>
        /// <param name="atlas">The <see cref="TextureAtlas"/> to import</param>
        /// <returns>A new <see cref="BundleSheetExport"/> created from the given TextureAtlas</returns>
        public static BundleSheetExport FromAtlas([NotNull] TextureAtlas atlas)
        {
            //
            // 1. Generate final export image
            //
            var image = atlas.GenerateSheet();

            //
            // 2. Copy the frame bounds from the atlas to the bundle sheet
            //
            var frameRectList = new List<FrameRect>();
            for (int i = 0; i < atlas.FrameCount; i++)
            {
                frameRectList.Add(new FrameRect(atlas.GetFrame(i), atlas.GetFrameBoundsRectangle(i), atlas.GetFrameOriginsRectangle(i)));
            }

            return new BundleSheetExport
            {
                Sheet = image,
                Atlas = atlas,
                ExportSettings = atlas.ExportSettings,
                Animations = atlas.GetAnimationsOnAtlas(),
                ReusedFrameCount = atlas.Information.ReusedFrameOriginsCount,
                ReuseCounts = atlas.FrameList.Select(atlas.ReuseCountForFrame).ToArray(),
                FrameRects = frameRectList.ToArray()
            };
        }

        /// <summary>
        /// Describes a frame on this <see cref="BundleSheetExport"/>
        /// </summary>
        public class FrameRect
        {
            /// <summary>
            /// Gets the Frame represented by this FrameRect
            /// </summary>
            public IFrame Frame { get; }

            /// <summary>
            /// Gets the area the frame occupies inside the sheet
            /// </summary>
            public Rectangle SheetArea { get; }

            /// <summary>
            /// Gets the area of the frame that is used on the sheet
            /// </summary>
            public Rectangle FrameArea { get; }

            /// <summary>
            /// Creates a new FrameRect using the given parameters
            /// </summary>
            /// <param name="frame">The frame to represent on this FrameRect</param>
            /// <param name="sheetArea">The area the frame occupies inside the sheet</param>
            /// <param name="frameArea">The area of the frame that is used on the sheet</param>
            public FrameRect([NotNull] IFrame frame, Rectangle sheetArea, Rectangle frameArea)
            {
                // Clip the area to the frame size
                if (sheetArea.Width > frame.Width)
                    sheetArea.Width = frame.Width;
                if (sheetArea.Height > frame.Height)
                    sheetArea.Height = frame.Height;

                if (frameArea.Width > frame.Width)
                    frameArea.Width = frame.Width;
                if (frameArea.Height > frame.Height)
                    frameArea.Height = frame.Height;

                Frame = frame;
                SheetArea = sheetArea;
                FrameArea = frameArea;
            }
        }

        /// <summary>
        /// Represents a unique sheet region that one or more frames share
        /// </summary>
        public readonly struct FramesEntry
        {
            /// <summary>
            /// The shared sheet area
            /// </summary>
            public Rectangle SheetArea { get; }

            /// <summary>
            /// A list of frames that share this region
            /// </summary>
            public IReadOnlyList<FrameRect> FrameRects { get; }

            public FramesEntry(Rectangle sheetArea, IReadOnlyList<FrameRect> frameRects)
            {
                FrameRects = frameRects;
                SheetArea = sheetArea;
            }
        }
    }

    /// <summary>
    /// The delegate used to report progress of bundle export
    /// </summary>
    /// <param name="progressArgs">The current export progress summary</param>
    public delegate void BundleExportProgressEventHandler(BundleExportProgressEventArgs progressArgs);

    /// <summary>
    /// Contains information about the progress of a bundle export
    /// </summary>
    public class BundleExportProgressEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the current export stage for the bundle
        /// </summary>
        public BundleExportStage ExportStage { get; }

        /// <summary>
        /// Gets the current stage export progress.
        /// The progress ranges from 0 - 100
        /// </summary>
        public int StageProgress { get; }

        /// <summary>
        /// Gets the total export progress.
        /// The progress ranges from 0 - 100
        /// </summary>
        public int TotalProgress { get; }

        /// <summary>
        /// Gets the description of the current stage
        /// </summary>
        public string StageDescription { get; }

        /// <summary>
        /// Initializes a new instance of the BundleExportProgressEventArgs
        /// </summary>
        /// <param name="exportStage">The current export stage for the bundle</param>
        /// <param name="stageProgress">The progress of the current export stage</param>
        /// <param name="totalProgress">The total progress of the export</param>
        /// <param name="stageDescription">An optional description for the current stage</param>
        public BundleExportProgressEventArgs(BundleExportStage exportStage, int stageProgress, int totalProgress, string stageDescription = "")
        {
            ExportStage = exportStage;
            StageProgress = stageProgress;
            TotalProgress = totalProgress;
            StageDescription = stageDescription;
        }
    }

    /// <summary>
    /// Specific progress handler for sheet generation phase of bundle exporting
    /// </summary>
    public class SheetGenerationBundleExportProgressEventArgs : BundleExportProgressEventArgs
    {
        /// <summary>
        /// Gets the animation provider associated with this export progress event argument
        /// </summary>
        public IAnimationProvider Provider { get; }

        public SheetGenerationBundleExportProgressEventArgs(IAnimationProvider provider, BundleExportStage exportStage, int stageProgress, int totalProgress, string stageDescription = "") 
            : base(exportStage, stageProgress, totalProgress, stageDescription)
        {
            Provider = provider;
        }
    }

    /// <summary>
    /// Describes the stage a bundle export is currently at
    /// </summary>
    public enum BundleExportStage
    {
        /// <summary>
        /// Specifies that the progress is currently at the texture atlas generation
        /// </summary>
        TextureAtlasGeneration,
        /// <summary>
        /// Specifies that the progress is currently at the saving to disk stage
        /// </summary>
        SavingToDisk,
        /// <summary>
        /// Specifies that the export process is over
        /// </summary>
        Ended
    }
}
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
using System.Drawing;
using System.Drawing.Imaging;
using FastBitmapLib;
using JetBrains.Annotations;
using PixLib.Utils;

namespace PixLib.Data.Exports
{
    /// <summary>
    /// Describes a texture atlas that is used to pack frames of an animation together
    /// </summary>
    public class TextureAtlas : IDisposable
    {
        /// <summary>
        /// Creates a new TextureAtlas, preparing the atlas to export using the given export settings
        /// </summary>
        /// <param name="settings">The export settings to use when packing the frames</param>
        /// <param name="name">An optional name for the TextureAtlas to be used on progress report</param>
        public TextureAtlas(AnimationSheetExportSettings settings, string name = "")
        {
            _boundsMap = new FrameBoundsMap();
            _animationList = new List<Animation>();
            FrameList = new List<IFrame>();
            SheetExportSettings = settings;
            Information = new TextureAtlasInformation();

            Name = name;
        }
        
        ~TextureAtlas()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of the data stored by this TextureAtlas
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

            // Clear the lists
            _boundsMap = new FrameBoundsMap();
            FrameList.Clear();
        }

        /// <summary>
        /// Inserts a frame into this TextureAtlas
        /// </summary>
        /// <param name="frame">The frame to pack</param>
        public void InsertFrame(IFrame frame)
        {
            if (FrameList.ContainsReference(frame))
            {
                return;
            }

            if (frame.Animation != null && !_animationList.Contains(frame.Animation))
            {
                _animationList.Add(frame.Animation);
            }

            _boundsMap.RegisterFrames(new[] { frame }, new Rectangle(0, 0, frame.Width, frame.Height));

            FrameList.Add(frame);
        }

        /// <summary>
        /// Inserts the frames of a given animation into this texture atlas
        /// </summary>
        /// <param name="animation">The animatio to insert the frames to</param>
        public void InsertFramesFromAnimation([NotNull] Animation animation)
        {
            foreach (var frame in animation.Frames)
            {
                InsertFrame(frame);
            }
        }

        /// <summary>
        /// Generates the Sheet for this TextureAtlas
        /// </summary>
        /// <returns>A composed texture atlas of all the frames imported on this TextureAtlas</returns>
        public Image GenerateSheet()
        {
            //
            // 1. Create the sheet bitmap
            //
            var image = new Bitmap(AtlasWidth, AtlasHeight, PixelFormat.Format32bppArgb);

            using (var fastTarget = image.FastLock())
            {
                //
                // 2. Draw the frames on the sheet image
                //
                // Keep track of frames that were already drawn
                var renderedFrames = new List<IFrame>();
                for (int i = 0; i < FrameCount; i++)
                {
                    var frame = GetFrame(i);

                    if (SheetExportSettings.ReuseIdenticalFramesArea)
                    {
                        if (renderedFrames.Contains(frame))
                            continue;

                        renderedFrames.Add(frame);
                    }

                    var frameBounds = GetFrameBoundsRectangle(i);
                    var originBounds = GetFrameOriginsRectangle(i);

                    using (var composed = frame.GetComposedBitmap())
                    {
                        fastTarget.CopyRegion(composed, originBounds, frameBounds);
                    }
                }
            }

            return image;
        }

        /// <summary>
        /// Gets a list of animations whose frames are on this atlas
        /// </summary>
        /// <returns>The array of animations on this TextureAtlas</returns>
        public Animation[] GetAnimationsOnAtlas()
        {
            return _animationList.ToArray();
        }

        /// <summary>
        /// Returns the frame that lies on the given index
        /// </summary>
        /// <param name="frameIndex">The index for frame</param>
        /// <returns>The frame that relies on that index</returns>
        public IFrame GetFrame(int frameIndex)
        {
            return FrameList[frameIndex];
        }

        /// <summary>
        /// Gets the bounding rectangle for the given frame index. The bounds rectangle represents the
        /// area on the atlas sheet the frame occupies. It is relative to the atlas sheet
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the bounding rectangle from</param>
        /// <returns>The bounding rectangle for the given frame</returns>
        public Rectangle GetFrameBoundsRectangle(int frameIndex)
        {
            var frame = FrameList[frameIndex];
            return _boundsMap.GetSheetBoundsForFrame(frame).GetValueOrDefault();
        }

        /// <summary>
        /// Sets the bounding rectangle for the given frame index. The bounds rectangle represents the
        /// area on the atlas sheet the frame occupies. It is relative to the atlas sheet
        /// </summary>
        /// <param name="frameIndex">The index of the frame to set the bounding rectangle</param>
        /// <param name="moveSharedFrames">Whether to also move frames that are marked as sharing the region of this frame</param>
        /// <param name="rectangle">The bounds Rectangle for the frame index</param>
        public void SetFrameBoundsRectangle(int frameIndex, Rectangle rectangle, bool moveSharedFrames = false)
        {
            var frame = FrameList[frameIndex];

            if (!moveSharedFrames)
            {
                _boundsMap.SplitSharedSheetBoundsForFrame(frame);
            }

            _boundsMap.SetSheetBoundsForFrame(frame, rectangle);
        }

        /// <summary>
        /// Gets the origin rectangle for the given frame index. The origin rectangle represents the area
        /// of the Frame image that is used on the drawn atlas sheet. It is relative to the Frame area
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the origin rectangle from</param>
        /// <returns>The origin rectangle for the given frame</returns>
        public Rectangle GetFrameOriginsRectangle(int frameIndex)
        {
            var frame = FrameList[frameIndex];
            return _boundsMap.GetLocalBoundsForFrame(frame).GetValueOrDefault();
        }

        /// <summary>
        /// Sets the origin rectangle for the given frame index. The origin rectangle represents the area
        /// of the Frame image that is used on the drawn atlas sheet. It is relative to the Frame area
        /// </summary>
        /// <param name="frameIndex">The index of the frame to set the origin rectangle</param>
        /// <param name="rectangle">The origin Rectangle for the frame index</param>
        public void SetFrameOriginsRectangle(int frameIndex, Rectangle rectangle)
        {
            var frame = FrameList[frameIndex];
            _boundsMap.SetLocalBoundsForFrame(frame, rectangle);
        }

        /// <summary>
        /// Sets the frame bounds map for this texture atlas, replacing the current underlying frame bounds object
        /// </summary>
        public void SetFrameBoundsMap(FrameBoundsMap map)
        {
            _boundsMap = map;
        }

        /// <summary>
        /// Returns the reuse count for a given frame on this texture atlas
        /// </summary>
        public int ReuseCountForFrame([NotNull] IFrame frame)
        {
            var index = _boundsMap.SheetIndexForFrame(frame);
            return _boundsMap.CountOfFramesAtSheetBoundsIndex(index);
        }

        /// <summary>
        /// Fetches a copy of the current underlying frame bounds map object
        /// </summary>
        public FrameBoundsMap GetFrameBoundsMap()
        {
            return _boundsMap.Copy();
        }

        /// <summary>
        /// List of animations that have their frames placed on this texture atlas
        /// </summary>
        private readonly List<Animation> _animationList;

        /// <summary>
        /// The information about this texture atlas
        /// </summary>
        public TextureAtlasInformation Information;

        /// <summary>
        /// Gets or sets the name of this TextureAtlas. Used on progress reports
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the number of frames in this TextureAtlas
        /// </summary>
        public int FrameCount => FrameList.Count;

        /// <summary>
        /// The current frame bounds map for this texture altas, mapping frames to sheet rectangles that may be shared amongst frames
        /// </summary>
        private FrameBoundsMap _boundsMap;

        /// <summary>
        /// Gets the internal list of frames for this texture atlas
        /// </summary>
        public List<IFrame> FrameList { get; }
        
        /// <summary>
        /// Gets an array of unique rectangle bounds containeg within this texture atlas, ignoring duplicated frame bounds that where resued for similar frames.
        /// The array is not ordered in any particular way.
        /// </summary>
        public Rectangle[] UniqueBounds => _boundsMap.SheetBounds;
        
        /// <summary>
        /// Gets this atlas' width
        /// </summary>
        public int AtlasWidth => AtlasRectangle.Width;

        /// <summary>
        /// Gets this atlas' height
        /// </summary>
        public int AtlasHeight => AtlasRectangle.Height;

        /// <summary>
        /// Gets or sets this atlas' area rectangle
        /// </summary>
        public Rectangle AtlasRectangle { get; set; }

        /// <summary>
        /// Gets this atlas' export settings
        /// </summary>
        public AnimationSheetExportSettings SheetExportSettings { get; set; }
    }
}
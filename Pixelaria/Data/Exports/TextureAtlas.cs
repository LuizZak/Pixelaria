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

using Pixelaria.Utils;

namespace Pixelaria.Data.Exports
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
        public TextureAtlas(AnimationExportSettings settings, string name = "")
        {
            _animationList = new List<Animation>();
            _frameList = new List<IFrame>();
            _boundsList = new List<Rectangle>();
            _originsList = new List<Rectangle>();
            _reuseCount = new List<int>();
            _exportSettings = settings;
            Information = new TextureAtlasInformation();

            Name = name;
        }

        /// <summary>
        /// Disposes of the data stored by this TextureAtlas
        /// </summary>
        public void Dispose()
        {
            // Clear the lists
            _frameList.Clear();
            _boundsList.Clear();
            _originsList.Clear();
            _reuseCount.Clear();
        }

        /// <summary>
        /// Inserts a frame into this TextureAtlas
        /// </summary>
        /// <param name="frame">The frame to pack</param>
        public void InsertFrame(IFrame frame)
        {
            if (_frameList.ContainsReference(frame))
            {
                return;
            }

            if (frame.Animation != null && !_animationList.Contains(frame.Animation))
            {
                _animationList.Add(frame.Animation);
            }

            _frameList.Add(frame);
            _boundsList.Add(new Rectangle());
            _originsList.Add(new Rectangle(0, 0, frame.Width, frame.Height));
            _reuseCount.Add(0);
        }

        /// <summary>
        /// Inserts the frames of a given animation into this texture atlas
        /// </summary>
        /// <param name="animation">The animatio to insert the frames to</param>
        public void InsertFramesFromAnimation(Animation animation)
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
            Bitmap image = new Bitmap(AtlasWidth, AtlasHeight, PixelFormat.Format32bppArgb);

            //
            // 2. Draw the frames on the sheet image
            //
            // Keep track of frames that were already drawn
            List<IFrame> renderedFrames = new List<IFrame>();
            for (int i = 0; i < FrameCount; i++)
            {
                IFrame frame = GetFrame(i);

                if (_exportSettings.ReuseIdenticalFramesArea)
                {
                    if (renderedFrames.Contains(frame))
                        continue;

                    renderedFrames.Add(frame);
                }

                Rectangle frameBounds = GetFrameBoundsRectangle(i);
                Rectangle originBounds = GetFrameOriginsRectangle(i);

                // TODO: Deal with disposal of GetComposedBitmap()'s return
                Bitmap frameBitmap = frame.GetComposedBitmap();

                FastBitmap.CopyRegion(frameBitmap, image, originBounds, frameBounds);
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
            return _frameList[frameIndex];
        }

        /// <summary>
        /// Gets the bounding rectangle for the given frame index. The bounds rectangle represents the
        /// area on the atlas sheet the frame occupies. It is relative to the atlas sheet
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the bounding rectangle from</param>
        /// <returns>The bounding rectangle for the given frame</returns>
        public Rectangle GetFrameBoundsRectangle(int frameIndex)
        {
            return _boundsList[frameIndex];
        }

        /// <summary>
        /// Sets the bounding rectangle for the given frame index. The bounds rectangle represents the
        /// area on the atlas sheet the frame occupies. It is relative to the atlas sheet
        /// </summary>
        /// <param name="frameIndex">The index of the frame to set the bounding rectangle</param>
        /// <param name="rectangle">The bounds Rectangle for the frame index</param>
        public void SetFrameBoundsRectangle(int frameIndex, Rectangle rectangle)
        {
            _boundsList[frameIndex] = rectangle;
        }

        /// <summary>
        /// Gets the origin rectangle for the given frame index. The origin rectangle represents the area
        /// of the Frame image that is used on the drawn atlas sheet. It is relative to the Frame area
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the origin rectangle from</param>
        /// <returns>The origin rectangle for the given frame</returns>
        public Rectangle GetFrameOriginsRectangle(int frameIndex)
        {
            return _originsList[frameIndex];
        }

        /// <summary>
        /// Sets the origin rectangle for the given frame index. The origin rectangle represents the area
        /// of the Frame image that is used on the drawn atlas sheet. It is relative to the Frame area
        /// </summary>
        /// <param name="frameIndex">The index of the frame to set the origin rectangle</param>
        /// <param name="rectangle">The origin Rectangle for the frame index</param>
        public void SetFrameOriginsRectangle(int frameIndex, Rectangle rectangle)
        {
            _originsList[frameIndex] = rectangle;
        }

        /// <summary>
        /// List of animations that have their frames placed on this texture atlas
        /// </summary>
        private readonly List<Animation> _animationList;

        /// <summary>
        /// List of frames to pack
        /// </summary>
        private readonly List<IFrame> _frameList;

        /// <summary>
        /// List of frame bounds.
        /// The bounds rectangle represents the rectangle of the frame image that is represented on the exported sheet image
        /// </summary>
        private readonly List<Rectangle> _boundsList;

        /// <summary>
        /// List of frame origins.
        /// The origin rectangle represents a rectangle on the exported sheet image that the corresponding frame occupies
        /// </summary>
        private readonly List<Rectangle> _originsList;

        /// <summary>
        /// List of frames reused for each frame index. This list contains all the frames on the sheet, with indices that may repeat over.
        /// </summary>
        private readonly List<int> _reuseCount;

        /// <summary>
        /// Total area of this TextureAtlas
        /// </summary>
        private Rectangle _atlasRectangle;

        /// <summary>
        /// Export settings used by this TextureAtlas
        /// </summary>
        private AnimationExportSettings _exportSettings;

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
        public int FrameCount { get { return _frameList.Count; } }

        /// <summary>
        /// Gets the internal list of frames for this texture atlas
        /// </summary>
        public List<IFrame> FrameList { get { return _frameList; } }

        /// <summary>
        /// Gets the list of frame bounds.
        /// The bounds rectangle represents the rectangle of the frame image that is represented on the exported sheet image
        /// </summary>
        public List<Rectangle> BoundsList { get { return _boundsList; } }

        /// <summary>
        /// Gets the list of frame origins.
        /// The origin rectangle represents a rectangle on the exported sheet image that the corresponding frame occupies
        /// </summary>
        public List<Rectangle> OriginsList { get { return _originsList; } }

        /// <summary>
        /// Gets the list of frames reused for each frame index. This list contains all the frames on the sheet, with indices that may repeat over.
        /// </summary>
        public List<int> ReuseCount { get { return _reuseCount; } }

        /// <summary>
        /// Gets this atlas' width
        /// </summary>
        public int AtlasWidth { get { return _atlasRectangle.Width; } }

        /// <summary>
        /// Gets this atlas' height
        /// </summary>
        public int AtlasHeight { get { return _atlasRectangle.Height; } }

        /// <summary>
        /// Gets or sets this atlas' area rectangle
        /// </summary>
        public Rectangle AtlasRectangle { get { return _atlasRectangle; } set { _atlasRectangle = value; } }

        /// <summary>
        /// Gets this atlas' export settings
        /// </summary>
        public AnimationExportSettings ExportSettings { get { return _exportSettings; } set { _exportSettings = value; } }
    }
}
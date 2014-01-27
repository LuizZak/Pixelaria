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

using System.Drawing;

using Pixelaria.Data;

namespace Pixelaria.Importers
{
    /// <summary>
    /// Defines a default importer behavior that must be implemented by importers in the program
    /// </summary>
    public interface IDefaultImporter
    {
        /// <summary>
        /// Imports an animation from an image on disk
        /// </summary>
        /// <param name="animationName">The name to use when creating the animation</param>
        /// <param name="sheetPath">The path to the sheet file</param>
        /// <param name="settings">The sheet import settings</param>
        /// <returns>The final imported animation</returns>
        Animation ImportAnimationFromPath(string animationName, string sheetPath, SheetSettings settings);

        /// <summary>
        /// Imports an animation from an animation sheet image
        /// </summary>
        /// <param name="animationName">The name to use when creating the animation</param>
        /// <param name="sheet">The sheet image</param>
        /// <param name="settings">The sheet import settings</param>
        /// <returns>The final imported animation</returns>
        Animation ImportAnimationFromImage(string animationName, Image sheet, SheetSettings settings);

        /// <summary>
        /// Generates and returns an array of rectangles that represents the frames of the animation described by the given sheet settings.
        /// The rectangles are relatives to the image used as sheet.
        /// </summary>
        /// <param name="sheet">The image to use as sheet</param>
        /// <param name="settings">The sheet settings to use to calculate the rectangle frames</param>
        /// <returns>An array of rectangles that represents the frames of the animation described by the given sheet settings</returns>
        Rectangle[] GenerateFrameBounds(Image sheet, SheetSettings settings);
    }

    /// <summary>
    /// Struct used as import settings when feeding to the IDefaultImporter methods
    /// </summary>
    public struct SheetSettings
    {
        /// <summary>
        /// Whether the frames of the animation have a constant size
        /// </summary>
        public bool ConstantSize;

        /// <summary>
        /// The X offset to start loading the frames from
        /// </summary>
        public int OffsetX;

        /// <summary>
        /// The Y offset to start loading the frames from
        /// </summary>
        public int OffsetY;

        /// <summary>
        /// The width of the frames on the sheet
        /// </summary>
        public int FrameWidth;

        /// <summary>
        /// The height of the frames on the sheet
        /// </summary>
        public int FrameHeight;

        /// <summary>
        /// The number of frames to import
        /// </summary>
        public int FrameCount;

        /// <summary>
        /// The first frame to import
        /// </summary>
        public int FirstFrame;

        /// <summary>
        /// Whether to flip the frame import ordering and import the animation backwards
        /// </summary>
        public bool FlipFrames;

        /// <summary>
        /// Default struct constructor
        /// </summary>
        /// <param name="constantSize">Whether the frames of the animation have a constant size</param>
        /// <param name="frameWidth">The width of the frames on the sheet</param>
        /// <param name="frameHeight">The height of the frames on the sheet</param>
        /// <param name="frameCount">The number of frames to import. Leave -1 to import all frames</param>
        /// <param name="firstFrame">The first frame to import. Leave 0 to start from the first frame</param>
        /// <param name="flipFrames">Whether to flip the frame import ordering and import the animation backwards</param>
        /// <param name="offsetX">The X offset to start loading the frames from</param>
        /// <param name="offsetY">The Y offset to start loading the frames from</param>
        public SheetSettings(bool constantSize, int frameWidth, int frameHeight, int frameCount = -1, int firstFrame = 0, bool flipFrames = false, int offsetX = 0, int offsetY = 0)
        {
            this.ConstantSize = constantSize;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
            this.FrameCount = frameCount;
            this.FirstFrame = firstFrame;
            this.FlipFrames = flipFrames;
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
        }

        /// <summary>
        /// Compares this struct with the given sheet struct and returns true if all properties match
        /// </summary>
        /// <param name="obj">The other SheetSettings to compare to</param>
        /// <returns>Whether this SheetSettings object matches the passed object</returns>
        public bool Equals(SheetSettings obj)
        {
            return this.ConstantSize == obj.ConstantSize && this.OffsetX == obj.OffsetX && this.OffsetY == obj.OffsetY &&
                   this.FrameWidth == obj.FrameWidth && this.FrameHeight == obj.FrameHeight && this.FrameCount == obj.FrameCount &&
                   this.FirstFrame == obj.FirstFrame && this.FlipFrames == obj.FlipFrames;
        }
    }
}
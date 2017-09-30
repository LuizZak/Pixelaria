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
using System.Drawing;
using FastBitmapLib;
using Pixelaria.Data;

namespace Pixelaria.Controllers.Importers
{
    /// <summary>
    /// Default importer that uses PNG as the texture format
    /// </summary>
    public class DefaultPngImporter : IDefaultImporter
    {
        /// <summary>
        /// Imports an animation from an animation sheet
        /// </summary>
        /// <param name="animationName">The name to use when creating the animation</param>
        /// <param name="sheetPath">The path to the sheet file</param>
        /// <param name="settings">The sheet import settings</param>
        /// <returns>The final imported animation</returns>
        public Animation ImportAnimationFromPath(string animationName, string sheetPath, SheetSettings settings)
        {
            return ImportAnimationFromImage(animationName, Image.FromFile(sheetPath), settings);
        }

        /// <summary>
        /// Imports an animation from an animation sheet image
        /// </summary>
        /// <param name="animationName">The name to use when creating the animation</param>
        /// <param name="sheet">The sheet image</param>
        /// <param name="settings">The sheet import settings</param>
        /// <returns>The final imported animation</returns>
        public Animation ImportAnimationFromImage(string animationName, Image sheet, SheetSettings settings)
        {
            Rectangle[] frameBounds = GenerateFrameBounds(sheet, settings);

            int frameWidth = (settings.FrameWidth > sheet.Width ? sheet.Width : settings.FrameWidth);
            int frameHeight = (settings.FrameHeight > sheet.Height ? sheet.Height : settings.FrameHeight);
            Animation animation = new Animation(animationName, frameWidth, frameHeight);

            foreach (Rectangle rect in frameBounds)
            {
                // Create the frame
                Frame frame = animation.CreateFrame(settings.FlipFrames ? 0 : -1);

                frame.SetFrameBitmap(FastBitmap.SliceBitmap((Bitmap)sheet, rect));
                frame.UpdateHash();
            }

            return animation;
        }

        /// <summary>
        /// Generates and returns an array of rectangles that represents the frames of the animation described by the given sheet settings.
        /// The rectangles are relatives to the image used as sheet.
        /// </summary>
        /// <param name="sheet">The image to use as sheet</param>
        /// <param name="settings">The sheet settings to use to calculate the rectangle frames</param>
        /// <returns>An array of rectangles that represents the frames of the animation described by the given sheet settings</returns>
        public Rectangle[] GenerateFrameBounds(Image sheet, SheetSettings settings)
        {
            List<Rectangle> frameBounds = new List<Rectangle>();

            Image texture = sheet;

            // Trim out the dimensions:
            int frameWidth = (settings.FrameWidth > texture.Width ? texture.Width : settings.FrameWidth);
            int frameHeight = (settings.FrameHeight > texture.Height ? texture.Height : settings.FrameHeight);

            // Calculate cells dimensions
            int xCells = texture.Width / frameWidth;
            int yCells = texture.Height / frameHeight;

            // No frame count set? Calculate by the image size
            if (settings.FrameCount == -1)
                settings.FrameCount = (xCells * yCells) - settings.FirstFrame;

            // Frame count larger than frames on image? Trim the variable
            if (settings.FirstFrame + settings.FrameCount > xCells * yCells)
                settings.FrameCount = (xCells * yCells) - settings.FirstFrame;

            int x = settings.OffsetX + settings.FirstFrame * frameWidth;
            int y = settings.OffsetY;

            if (x > texture.Width - frameWidth)
            {
                x = 0;

                // Break the loop once the maximum number of frames has been reached
                if (y >= texture.Height - frameHeight)
                {
                    return frameBounds.ToArray();
                }
                y += frameHeight;
            }

            // Import the frames now
            for (int cell = settings.FirstFrame; cell < settings.FirstFrame + settings.FrameCount; cell++)
            {
                // Create the frame
                // Calculate the offset onto the input image:
                //int x = cell % x_cells * frameWidth;
                //int y = cell / x_cells * frameHeight;
                frameBounds.Add(new Rectangle(x, y, frameWidth, frameHeight));

                x += frameWidth;

                if (x > texture.Width - frameWidth)
                {
                    x = 0;

                    // Break the loop once the maximum number of frames has been reached
                    if (y >= texture.Height - frameHeight)
                    {
                        break;
                    }
                    y += frameHeight;
                }
            }

            return frameBounds.ToArray();
        }
    }
}
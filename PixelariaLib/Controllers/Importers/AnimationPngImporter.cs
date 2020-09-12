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
using System.Linq;
using FastBitmapLib;
using JetBrains.Annotations;
using PixelariaLib.Controllers.DataControllers;
using PixelariaLib.Data;

namespace PixelariaLib.Controllers.Importers
{
    /// <summary>
    /// Default importer that uses PNG as the texture format
    /// </summary>
    public class AnimationPngImporter : IAnimationImporter
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
            return ImportAnimationFromImage(animationName, new Bitmap(Image.FromFile(sheetPath)), settings);
        }

        /// <summary>
        /// Imports an animation from an animation sheet image
        /// </summary>
        /// <param name="animationName">The name to use when creating the animation</param>
        /// <param name="sheet">The sheet image</param>
        /// <param name="settings">The sheet import settings</param>
        /// <returns>The final imported animation</returns>
        public Animation ImportAnimationFromImage(string animationName, Bitmap sheet, SheetSettings settings)
        {
            var frameSequence = GenerateFrameSequence(sheet, settings);

            int frameWidth = settings.FrameWidth > sheet.Width ? sheet.Width : settings.FrameWidth;
            int frameHeight = settings.FrameHeight > sheet.Height ? sheet.Height : settings.FrameHeight;
            var animation = new Animation(animationName, frameWidth, frameHeight);

            var controller = new AnimationController(null, animation);

            for (int i = 0; i < frameSequence.FrameCount; i++)
            {
                var bitmap = frameSequence.GetComposedBitmapForFrame(i);

                // Create the frame
                var frameId = controller.CreateFrame(settings.FlipFrames ? 0 : -1);
                var frame = controller.GetFrameController(frameId);

                frame.SetFrameBitmap(bitmap);
            }

            return animation;
        }

        /// <summary>
        /// Generates and returns an array of rectangles that represents the frames of the animation described by the given sheet settings.
        /// The rectangles are all sourced from a rectangle of size <see cref="textureSize"/>.
        /// </summary>
        /// <param name="textureSize">The size of the sheet texture to slice</param>
        /// <param name="settings">The sheet settings to use to calculate the rectangle frames</param>
        /// <returns>An array of rectangles that represents the frames of the animation described by the given sheet settings</returns>
        public Rectangle[] GenerateFrameBounds(Size textureSize, SheetSettings settings)
        {
            var frameBounds = new List<Rectangle>();
            
            // Trim out the dimensions:
            int frameWidth = settings.FrameWidth > textureSize.Width ? textureSize.Width : settings.FrameWidth;
            int frameHeight = settings.FrameHeight > textureSize.Height ? textureSize.Height : settings.FrameHeight;

            // Calculate cells dimensions
            int xCells = textureSize.Width / frameWidth;
            int yCells = textureSize.Height / frameHeight;

            // No frame count set? Calculate by the image size
            if (settings.FrameCount == -1)
                settings.FrameCount = xCells * yCells - settings.FirstFrame;

            // Frame count larger than frames on image? Trim the variable
            if (settings.FirstFrame + settings.FrameCount > xCells * yCells)
                settings.FrameCount = xCells * yCells - settings.FirstFrame;

            int x = settings.OffsetX + settings.FirstFrame * frameWidth;
            int y = settings.OffsetY;

            if (x > textureSize.Width - frameWidth)
            {
                x = 0;

                // Break the loop once the maximum number of frames has been reached
                if (y >= textureSize.Height - frameHeight)
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

                if (x > textureSize.Width - frameWidth)
                {
                    x = 0;

                    // Break the loop once the maximum number of frames has been reached
                    if (y >= textureSize.Height - frameHeight)
                    {
                        break;
                    }
                    y += frameHeight;
                }
            }

            return frameBounds.ToArray();
        }
    
        /// <summary>
        /// From a given image and sheet settings pair, generates a frame sequence where each extracted frame
        /// represents the proper frame square within the input image.
        /// </summary>
        /// <param name="sheet">The image to use as sheet</param>
        /// <param name="settings">The sheet settings to use to calculate the rectangle frames</param>
        /// <returns>A bitmap frame sequence for the animation</returns>
        public IBitmapFrameSequence GenerateFrameSequence(Bitmap sheet, SheetSettings settings)
        {
            return new FrameSequencer(sheet, GenerateFrameBounds(sheet.Size, settings));
        }
    }

    /// <summary>
    /// A simple frame sequencer that exposes bitmap frames from a sequence of slice rectangles
    /// of a source bitmap
    /// </summary>
    public class FrameSequencer : IBitmapFrameSequence
    {
        private readonly Bitmap _sourceBitmap;
        private readonly Rectangle[] _rectangles;

        public int Width => Size.Width;
        public int Height => Size.Height;
        public Size Size { get; }
        public int FrameCount { get; }

        public FrameSequencer([NotNull] Bitmap sourceBitmap, [NotNull] Rectangle[] rectangles)
        {
            _sourceBitmap = sourceBitmap;
            _rectangles = rectangles;

            // Frame size is the largest size from the input rectangles source
            Size = _rectangles.Aggregate(Size.Empty, (size, rectangle) => new Size(Math.Max(size.Width, rectangle.Size.Width),
                Math.Max(size.Height, rectangle.Size.Height)));

            FrameCount = rectangles.Length;
        }

        public Bitmap GetComposedBitmapForFrame(int frameIndex)
        {
            var rect = _rectangles[frameIndex];

            // Make quick check to see if slice isn't smaller than frame size
            // If it is, paste it into a larger transparent background image
            if (rect.Size == Size)
                return FastBitmap.SliceBitmap(_sourceBitmap, rect);

            var newBitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            FastBitmap.CopyRegion(_sourceBitmap, newBitmap, rect, new Rectangle(Point.Empty, Size));

            return newBitmap;
        }
    }
}
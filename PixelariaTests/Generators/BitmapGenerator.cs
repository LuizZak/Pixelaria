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

// TODO: Reduce copying this file around in PixelariaTests and PixLibTests

using System;
using System.Drawing;
using System.Drawing.Imaging;
using FastBitmapLib;
using JetBrains.Annotations;
using PixLib.Controllers.DataControllers;

namespace PixelariaTests.Generators
{
    public static class BitmapGenerator
    {
        /// <summary>
        /// Random number generator used to randomize seeds for image generation when none are provided
        /// </summary>
        private static readonly Random SeedRandom = new Random();

        /// <summary>
        /// Generates a frame image with a given set of parameters.
        /// The seed is used to randomize the frame, and any call with the same width, height and seed will generate the same image
        /// </summary>
        /// <param name="width">The width of the image to generate</param>
        /// <param name="height">The height of the image to generate</param>
        /// <param name="seed">The seed for the image, used to seed the random number generator that will generate the image contents</param>
        /// <returns>An image with the passed parameters</returns>
        public static Bitmap GenerateRandomBitmap(int width, int height, int seed = -1)
        {
            if (seed == -1)
            {
                seed = SeedRandom.Next();
            }

            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            var fastBitmap = new FastBitmap(bitmap);
            fastBitmap.Lock();

            // Plot the image with random pixels now
            var r = new Random(seed);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    uint pixelColor = (uint)(r.NextDouble() * 0xFFFFFFFF);
                    fastBitmap.SetPixel(x, y, pixelColor);
                }
            }

            fastBitmap.Unlock();

            return bitmap;
        }

        /// <summary>
        /// Generates a bitmap that is guaranteed to be considered different from another bitmap.
        /// The bitmap retains the original bitmap's size and bitdepth
        /// </summary>
        /// <param name="bitmap">A valid Bitmap</param>
        /// <returns>A new Bitmap, that is considered to be different from the provided bitmap</returns>
        public static Bitmap GenerateDifferentFrom([NotNull] Bitmap bitmap)
        {
            var bit = new Bitmap(bitmap);
            var c = Color.FromArgb((bitmap.GetPixel(0, 0).ToArgb() + 1) % 0xFFFFFFF);

            bit.SetPixel(0, 0, c);

            return bit;
        }

        /// <summary>
        /// Randomizes the contents of this frame's bitmap based on a given seed
        /// </summary>
        /// <param name="frame">The frame to randomize</param>
        /// <param name="seed">The seed to use when randomizing this frame. Leave -1 to use a random seed</param>
        public static void RandomizeBitmap([NotNull] this FrameController frame, int seed = -1)
        {
            frame.SetFrameBitmap(GenerateRandomBitmap(frame.Width, frame.Height, seed));
        }
    }
}
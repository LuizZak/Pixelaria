using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pixelaria.Utils;

namespace PixelariaTests.Generators
{
    /// <summary>
    /// Contains methods related to image generation used in unit tests
    /// </summary>
    public static class FrameImageGenerator
    {
        /// <summary>
        /// Generates a frame image with a given set of parameters.
        /// The seed is used to randomize the frame, and any call with the same width, height and seed will generate the same image
        /// </summary>
        /// <param name="width">The width of the frame to generate</param>
        /// <param name="height">The height of the frame to generate</param>
        /// <param name="seed">The seed for the frame's image, used to seed the random number generator that will generate the image contents</param>
        /// <returns>An image with the passed parameters</returns>
        public static Bitmap GenerateFrameImage(int width, int height, int seed)
        {
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            FastBitmap fastBitmap = new FastBitmap(bitmap);
            fastBitmap.Lock();

            // Plot the image with random pixels now
            Random r = new Random(seed);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelColor = r.Next(0xFFFFFF);
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
        public static Bitmap GenerateDifferentFrom(Bitmap bitmap)
        {
            Bitmap bit = new Bitmap(bitmap);
            Color c = Color.FromArgb((bitmap.GetPixel(0, 0).ToArgb() + 1) % 0xFFFFFFF);

            bit.SetPixel(0, 0, c);

            return bit;
        }
    }
}
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using FastBitmapLib;
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Properties;
using PixCore.Unsafe;

namespace PixCore.Imaging
{
    /// <summary>
    /// Contains static image-related utility methods
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        /// Checkers pattern used as the default tile
        /// </summary>
        private static readonly Image CheckersPattern = Resources.checkers_pattern;
        
        /// <summary>
        /// Returns an Image object that represents the default tile image to be used in the background of controls
        /// that display any type of transparency
        /// </summary>
        /// <returns>
        /// An Image object that represents the default tile image to be used in the background of controlsthat display
        /// any type of transparency
        /// </returns>
        public static Image GetDefaultTile()
        {
            return CheckersPattern;
        }

        /// <summary>
        /// Returns a Rectangle that specifies the minimum image area, clipping out all the alpha pixels
        /// </summary>
        /// <param name="image">The image to find the mimimum image area</param>
        /// <returns>A Rectangle that specifies the minimum image area, clipping out all the alpha pixels</returns>
        public static Rectangle FindMinimumImageArea([NotNull] Bitmap image)
        {
            int minImageX = 0;
            int minImageY = 0;

            int maxImageX = 0;
            int maxImageY = 0;

            int width = image.Width;
            int height = image.Height;

            using (var fastBitmap = image.FastLock())
            {
                // Scan vertically - 1st pass
                int x;
                int y = 0;

                for (x = 0; x < width; x++)
                {
                    for (y = 0; y < height; y++)
                    {
                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            minImageX = x;
                            goto skipx;
                        }
                    }
                }
                skipx:

                // All pixels scanned, none are opaque
                if (x == width && y == height)
                {
                    return Rectangle.Empty;
                }

                // Scan horizontally - 1st pass
                for (y = 0; y < height; y++)
                {
                    for (x = minImageX; x < width; x++)
                    {
                        minImageY = y;

                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            goto skipy;
                        }
                    }
                }
                skipy:

                // Scan vertically - 2nd pass
                for (x = width - 1; x >= minImageX; x--)
                {
                    for (y = height - 1; y >= minImageY; y--)
                    {
                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            maxImageX = x;
                            goto skipw;
                        }
                    }
                }
                skipw:

                // Scan horizontally - 2nd pass
                for (y = height - 1; y >= minImageY; y--)
                {
                    for (x = minImageX; x <= maxImageX; x++)
                    {
                        if (fastBitmap.GetPixelInt(x, y) >> 24 != 0)
                        {
                            maxImageY = y;
                            goto skiph;
                        }
                    }
                }
                skiph:
                ;
            }

            return new Rectangle(minImageX, minImageY, maxImageX - minImageX + 1, maxImageY - minImageY + 1);
        }

        /// <summary>
        /// Draws a bitmap on top of a target bitmap using color blending.
        /// If the two images don't have the same dimensions or don't have a 32bpp ARGB, an exception is raised
        /// </summary>
        /// <param name="target">The bitmap to draw the foreground onto</param>
        /// <param name="foreBitmap">The foreground bitmap to draw onto the target bitmap</param>
        /// <param name="highQuality">Whether to use high quality image composition. This composition mode is considerably slower than low quality</param>
        /// <exception cref="Exception">The size of the bitmaps must be equal and both bitmaps must have a 32bpp ARGB pixel format</exception>
        public static void FlattenBitmaps([NotNull] Bitmap target, [NotNull] Bitmap foreBitmap, bool highQuality)
        {
            if (target.Size != foreBitmap.Size || target.PixelFormat != PixelFormat.Format32bppArgb || foreBitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception("The size of the bitmaps must be equal and both bitmaps must have a 32bpp ARGB pixel format");
            }

            if (!highQuality)
            {
                using (var gfx = Graphics.FromImage(target))
                {
                    gfx.CompositingMode = CompositingMode.SourceOver;
                    gfx.InterpolationMode = InterpolationMode.NearestNeighbor;
                    gfx.SmoothingMode = SmoothingMode.HighSpeed;

                    gfx.DrawImage(foreBitmap, 0, 0);

                    gfx.Flush();
                }

                return;
            }

            int width = target.Width;
            int height = target.Height;

            using (FastBitmap fastTarget = target.FastLock(), fastForeBitmap = foreBitmap.FastLock())
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        fastTarget.SetPixel(x, y, ColorUtils.FlattenColor(fastTarget.GetPixelUInt(x, y), fastForeBitmap.GetPixelUInt(x, y)));
                    }
                }
            }
        }

        /// <summary>
        /// The hashing algorithm used for hashing the bitmaps
        /// </summary>
        private static readonly HashAlgorithm ShaM = new SHA256Managed();

        /// <summary>
        /// Returns a hash for the given Bitmap object
        /// </summary>
        /// <param name="bitmap">The bitmap to get the hash of</param>
        /// <returns>The hash of the given bitmap</returns>
        public static unsafe byte[] GetHashForBitmap([NotNull] Bitmap bitmap)
        {
            using (var fastBitmap = bitmap.FastLock())
            {
                var bytes = new byte[fastBitmap.Height * fastBitmap.Width * (Image.GetPixelFormatSize(bitmap.PixelFormat) / 8)];
                var scByte = (byte*)fastBitmap.Scan0;
                fixed (byte* pByte = bytes)
                {
                    FastBitmap.memcpy(pByte, scByte, (ulong)bytes.Length);
                }

                using (var stream = new MemoryStream(bytes, false))
                {
                    var hash = GetHashForStream(stream);
                    return hash;
                }
            }
        }

        /// <summary>
        /// Returns a hash for the given Stream object
        /// </summary>
        /// <param name="stream">The stream to get the hash of</param>
        /// <returns>The hash of the given stream</returns>
        public static byte[] GetHashForStream([NotNull] Stream stream)
        {
            // Compute a hash for the image
            return ShaM.ComputeHash(stream);
        }

        /// <summary>
        /// Returns the memory usage of the given image, in bytes
        /// </summary>
        /// <returns>Total memory usage, in bytes</returns>
        [Pure]
        public static long MemoryUsageOfImage([NotNull] Image image)
        {
            return image.Width * image.Height * Image.GetPixelFormatSize(image.PixelFormat) / 8;
        }
        
        /// <summary>
        /// Returns whether the two given images are identical to the pixel level.
        /// If the image dimensions are mis-matched, or any of the references is null, the method returns false.
        /// </summary>
        /// <param name="image1">The first image to compare</param>
        /// <param name="image2">The second image to compare</param>
        /// <returns>True whether the two images are identical, false otherwise</returns>
        [Pure]
        public static bool ImagesAreIdentical(Image image1, Image image2)
        {
            if (image1 == null || image2 == null)
                return false;

            if (image1 == image2)
                return true;

            if (image1.Size != image2.Size)
                return false;

            if (image1.PixelFormat != image2.PixelFormat)
                return false;

            Bitmap bit1 = null;
            Bitmap bit2 = null;

            try
            {
                var bitmap1 = image1 as Bitmap;
                var bitmap2 = image2 as Bitmap;

                bit1 = bitmap1 ?? new Bitmap(image1);
                bit2 = bitmap2 ?? new Bitmap(image2);

                return CompareMemCmp(bit1, bit2);
            }
            finally
            {
                if (bit1 != null && bit1 != image1)
                    bit1.Dispose();
                if (bit2 != null && bit2 != image2)
                    bit2.Dispose();
            }
        }

        /// <summary>
        /// Compares the memory portions of the two Bitmaps 
        /// </summary>
        /// <param name="b1">The first bitmap to compare</param>
        /// <param name="b2">The second bitmap to compare</param>
        /// <returns>Whether the two bitmaps are identical</returns>
        [Pure]
        private static bool CompareMemCmp(Bitmap b1, Bitmap b2)
        {
            var bd1 = b1.LockBits(new Rectangle(Point.Empty, b1.Size), ImageLockMode.ReadOnly, b1.PixelFormat);
            var bd2 = b2.LockBits(new Rectangle(Point.Empty, b2.Size), ImageLockMode.ReadOnly, b2.PixelFormat);

            int len = bd1.Stride * b1.Height;

            try
            {
                return UnsafeNativeMethods.memcmp(bd1.Scan0, bd2.Scan0, len) == 0;
            }
            finally
            {
                b1.UnlockBits(bd1);
                b2.UnlockBits(bd2);
            }
        }
    }
}

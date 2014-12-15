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
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Encapsulates a Bitmap for fast bitmap pixel operations using 32bpp images
    /// </summary>
    public unsafe class FastBitmap
    {
        /// <summary>
        /// The Bitmap object encapsulated on this FastBitmap
        /// </summary>
        private readonly Bitmap _bitmap;

        /// <summary>
        /// The BitmapData resulted from the lock operation
        /// </summary>
        private BitmapData _bitmapData;

        /// <summary>
        /// The stride of the bitmap
        /// </summary>
        private int _strideWidth;

        /// <summary>
        /// The first pixel of the bitmap
        /// </summary>
        private int *_scan0;

        /// <summary>
        /// Whether the current bitmap is locked
        /// </summary>
        private bool _locked;

        /// <summary>
        /// The width of this FastBitmap
        /// </summary>
        private int _width;

        /// <summary>
        /// The height of this FastBitmap
        /// </summary>
        private int _height;

        /// <summary>
        /// Gets the width of this FastBitmap object
        /// </summary>
        public int Width { get { return _width; } }

        /// <summary>
        /// Gets the height of this FastBitmap object
        /// </summary>
        public int Height { get { return _height; } }

        /// <summary>
        /// Gets the pointer to the first pixel of the bitmap
        /// </summary>
        public IntPtr Scan0 { get { return _bitmapData.Scan0; } }

        /// <summary>
        /// Gets the stride width of the bitmap
        /// </summary>
        public int Stride { get { return _strideWidth; } }

        /// <summary>
        /// Gets an array of 32-bit ARGB values that represent this FastBitmap
        /// </summary>
        public int[] DataArray
        {
            get
            {
                // Declare an array to hold the bytes of the bitmap
                int bytes = Math.Abs(_bitmapData.Stride) * _bitmap.Height;
                int[] argbValues = new int[bytes / 4];

                // Copy the RGB values into the array
                Marshal.Copy(_bitmapData.Scan0, argbValues, 0, bytes / 4);

                return argbValues;
            }
        }

        /// <summary>
        /// Creates a new instance of the FastBitmap class
        /// </summary>
        /// <param name="bitmap">The Bitmap object to encapsulate on this FastBitmap object</param>
        public FastBitmap(Bitmap bitmap)
        {
            this._bitmap = bitmap;

            this._width = bitmap.Width;
            this._height = bitmap.Height;
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations. If the bitmap is already locked,
        /// an exception is thrown
        /// </summary>
        public void Lock()
        {
            if (_locked)
            {
                throw new Exception("Unlock must be called before a Lock operation");
            }

            Lock(ImageLockMode.ReadWrite);
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations
        /// </summary>
        /// <param name="lockMode">The lock mode to use on the bitmap</param>
        private void Lock(ImageLockMode lockMode)
        {
            Rectangle rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);

            Lock(lockMode, rect);
        }

        /// <summary>
        /// Locks the bitmap to start the bitmap operations
        /// </summary>
        /// <param name="lockMode">The lock mode to use on the bitmap</param>
        /// <param name="rect">The rectangle to lock</param>
        private void Lock(ImageLockMode lockMode, Rectangle rect)
        {
            // Lock the bitmap's bits
            _bitmapData = _bitmap.LockBits(rect, lockMode, _bitmap.PixelFormat);

            _scan0 = (int*)_bitmapData.Scan0;
            _strideWidth = _bitmapData.Stride / 4;

            _locked = true;
        }

        /// <summary>
        /// Unlocks the bitmap and applies the changes made to it. If the bitmap was not locked
        /// beforehand, an exception is thrown
        /// </summary>
        public void Unlock()
        {
            if (!_locked)
            {
                throw new Exception("Lock must be called before an Unlock operation");
            }

            _bitmap.UnlockBits(_bitmapData);

            _locked = false;
        }

        /// <summary>
        /// Sets the pixel color at the given coordinates. If the bitmap was not locked beforehands,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to set</param>
        /// <param name="y">The Y coordinate of the pixel to set</param>
        /// <param name="color">The new color of the pixel to set</param>
        public void SetPixel(int x, int y, Color color)
        {
            SetPixel(x, y, color.ToArgb());
        }

        /// <summary>
        /// Sets the pixel color at the given coordinates. If the bitmap was not locked beforehands,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to set</param>
        /// <param name="y">The Y coordinate of the pixel to set</param>
        /// <param name="color">The new color of the pixel to set</param>
        public void SetPixel(int x, int y, int color)
        {
            if (!_locked)
            {
                throw new Exception("The FastBitmap must be locked before any pixel operations are made");
            }

            if (x < 0 || x >= _width)
            {
                throw new Exception("The X component must be >= 0 and < width");
            }
            if (y < 0 || y >= _height)
            {
                throw new Exception("The Y component must be >= 0 and < height");
            }

            *(_scan0 + x + y * _strideWidth) = color;
        }

        /// <summary>
        /// Sets the pixel color at the given coordinates. If the bitmap was not locked beforehands,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to set</param>
        /// <param name="y">The Y coordinate of the pixel to set</param>
        /// <param name="color">The new color of the pixel to set</param>
        public void SetPixel(int x, int y, uint color)
        {
            if (!_locked)
            {
                throw new Exception("The FastBitmap must be locked before any pixel operations are made");
            }

            if (x < 0 || x >= _width)
            {
                throw new Exception("The X component must be >= 0 and < width");
            }
            if (y < 0 || y >= _height)
            {
                throw new Exception("The Y component must be >= 0 and < height");
            }

            *(uint*)(_scan0 + x + y * _strideWidth) = color;
        }

        /// <summary>
        /// Gets the pixel color at the given coordinates. If the bitmap was not locked beforehands,
        /// an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to get</param>
        /// <param name="y">The Y coordinate of the pixel to get</param>
        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(GetPixelInt(x, y));
        }

        /// <summary>
        /// Gets the pixel color at the given coordinates as an integer value. If the bitmap
        /// was not locked beforehands, an exception is thrown
        /// </summary>
        /// <param name="x">The X coordinate of the pixel to get</param>
        /// <param name="y">The Y coordinate of the pixel to get</param>
        public int GetPixelInt(int x, int y)
        {
            if (!_locked)
            {
                throw new Exception("The FastBitmap must be locked before any pixel operations are made");
            }

            if (x < 0 || x >= _width)
            {
                throw new Exception("The X component must be >= 0 and < width");
            }
            if (y < 0 || y >= _height)
            {
                throw new Exception("The Y component must be >= 0 and < height");
            }

            return *(_scan0 + x + y * _strideWidth);
        }

        /// <summary>
        /// Clears the bitmap with the given color
        /// </summary>
        /// <param name="color">The color to clear the bitmap with</param>
        public void Clear(Color color)
        {
            Clear(color.ToArgb());
        }

        /// <summary>
        /// Clears the bitmap with the given color
        /// </summary>
        /// <param name="color">The color to clear the bitmap with</param>
        public void Clear(int color)
        {
            // Clear all the pixels
            int count = _width * _height;
            int* curScan = _scan0;

            int rem = count % 8;

            count /= 8;

            while (count-- > 0)
            {
                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;

                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;
                *(curScan++) = color;
            }
            while (rem-- > 0)
            {
                *(curScan++) = color;
            }
        }

        /// <summary>
        /// Copies a region of the source bitmap into this fast bitmap
        /// </summary>
        /// <param name="source">The source image to copy</param>
        /// <param name="srcRect">The region on the source bitmap that will be copied over</param>
        /// <param name="destRect">The region on this fast bitmap that will be changed</param>
        public void CopyRegion(Bitmap source, Rectangle srcRect, Rectangle destRect)
        {
            // Check if the rectangle configuration doesn't generate invalid states or does not affect the target image
            if (srcRect.Width <= 0 || srcRect.Height <= 0 || destRect.Width <= 0 || destRect.Height <= 0 || destRect.X > _width || destRect.Y > _height)
                return;

            FastBitmap fastSource = new FastBitmap(source);
            fastSource.Lock();

            int copyWidth = Math.Min(srcRect.Width, destRect.Width);
            int copyHeight = Math.Min(srcRect.Height, destRect.Height);

            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                {
                    int destX = destRect.X + x;
                    int destY = destRect.Y + y;

                    int srcX = x + srcRect.X;
                    int srcY = y + srcRect.Y;

                    if (destX >= 0 && destY >= 0 && destX < _width && destY < _height && srcX >= 0 && srcY >= 0 && srcX < fastSource._width && srcY < fastSource._height)
                    {
                        SetPixel(destX, destY, fastSource.GetPixelInt(srcX, srcY));
                    }
                }
            }

            fastSource.Unlock();
        }

        /// <summary>
        /// Performs a copy operation of the pixels from the Source bitmap to the Target bitmap.
        /// If the dimensions or pixel depths of both images don't match, the copy is not performed
        /// </summary>
        /// <param name="source">The bitmap to copy the pixels from</param>
        /// <param name="target">The bitmap to copy the pixels to</param>
        /// <returns>Whether the copy proceedure was successful</returns>
        public static bool CopyPixels(Bitmap source, Bitmap target)
        {
            if (source.Width != target.Width || source.Height != target.Height || source.PixelFormat != target.PixelFormat)
                return false;

            FastBitmap fastSource = new FastBitmap(source);
            FastBitmap fastTarget = new FastBitmap(target);

            fastSource.Lock(ImageLockMode.ReadOnly);
            fastTarget.Lock();

            // Simply copy the argb values array
            int *s0s = fastSource._scan0;
            int *s0t = fastTarget._scan0;

            const int bpp = 1; // Bytes per pixel

            int count = fastSource._width * fastSource._height * bpp;
            int rem = count % 8;

            count /= 8;

            while (count-- > 0)
            {
                *(s0t++) = *(s0s++);
                *(s0t++) = *(s0s++);
                *(s0t++) = *(s0s++);
                *(s0t++) = *(s0s++);

                *(s0t++) = *(s0s++);
                *(s0t++) = *(s0s++);
                *(s0t++) = *(s0s++);
                *(s0t++) = *(s0s++);
            }
            while (rem-- > 0)
            {
                *(s0t++) = *(s0s++);
            }

            fastSource.Unlock();
            fastTarget.Unlock();

            return true;
        }

        /// <summary>
        /// Clears the given bitmap with the given color
        /// </summary>
        /// <param name="bitmap">The bitmap to clear</param>
        /// <param name="color">The color to clear the bitmap with</param>
        public static void ClearBitmap(Bitmap bitmap, Color color)
        {
            ClearBitmap(bitmap, color.ToArgb());
        }

        /// <summary>
        /// Clears the given bitmap with the given color
        /// </summary>
        /// <param name="bitmap">The bitmap to clear</param>
        /// <param name="color">The color to clear the bitmap with</param>
        public static void ClearBitmap(Bitmap bitmap, int color)
        {
            FastBitmap fb = new FastBitmap(bitmap);
            fb.Lock();
            fb.Clear(color);
            fb.Unlock();
        }

        /// <summary>
        /// Copies a region of the source bitmap to a target bitmap
        /// </summary>
        /// <param name="source">The source image to copy</param>
        /// <param name="target">The target image to be altered</param>
        /// <param name="srcRect">The region on the source bitmap that will be copied over</param>
        /// <param name="destRect">The region on the target bitmap that will be changed</param>
        public static void CopyRegion(Bitmap source, Bitmap target, Rectangle srcRect, Rectangle destRect)
        {
            // Check if the rectangle configuration doesn't generate invalid states or does not affect the target image
            if (srcRect.Width <= 0 || srcRect.Height <= 0 || destRect.Width <= 0 || destRect.Height <= 0 || destRect.X > target.Width || destRect.Y > target.Height)
                return;

            FastBitmap fastTarget = new FastBitmap(target);
            fastTarget.Lock();

            fastTarget.CopyRegion(source, srcRect, destRect);

            fastTarget.Unlock();
        }
    }
}
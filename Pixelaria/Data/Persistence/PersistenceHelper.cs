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
using System.IO;
using FastBitmapLib;
using JetBrains.Annotations;

namespace Pixelaria.Data.Persistence
{
    /// <summary>
    /// Static class containing various helper methods related to stream persistence
    /// </summary>
    public static class PersistenceHelper
    {
        /// <summary>
        /// Saves the given bitmap onto the given stram. The save procedure stores a long value just before the bitmap specifying the size of the bitmap's contents
        /// </summary>
        /// <param name="bitmap">The bitmap to save</param>
        /// <param name="stream">The stream to save the bitmap to</param>
        public static void SaveImageToStream([NotNull] Bitmap bitmap, [NotNull] Stream stream)
        {
            // Save the image to a temporary memory stream so the write doesn't mess the original stream
            using(var memStream = new MemoryStream())
            {
                bitmap.Save(memStream, ImageFormat.Png);

                // Write the bitmap size and contents to the target stream
                stream.Write(BitConverter.GetBytes(memStream.Length), 0, 8);
                stream.Write(memStream.GetBuffer(), 0, (int)memStream.Length);
            }
        }

        /// <summary>
        /// Loads a bitmap image from the given stream. The stream must contain a long value at its current position specifying the size of the image on the stream
        /// </summary>
        /// <param name="stream">The stream to load the image from</param>
        /// <returns>A bitmap generated from the stream</returns>
        public static Bitmap LoadImageFromStream([NotNull] Stream stream)
        {
            var reader = new BinaryReader(stream);
            
            // Read the size of the frame texture
            long textSize = reader.ReadInt64();
            
            return LoadImageFromBytes(reader.ReadBytes((int)textSize));
        }

        /// <summary>
        /// Loads a bitmap image from the given bytes source. Bytes array must be as big as needed to load the underlying image from it.
        /// </summary>
        /// <param name="bytes">The bytes to load the image from</param>
        /// <returns>A bitmap generated from the passed bytes</returns>
        public static Bitmap LoadImageFromBytes([NotNull] byte[] bytes)
        {
            Bitmap bitmap;

            using (var memStream = new MemoryStream(bytes))
            {
                var img = (Bitmap)Image.FromStream(memStream);

                // Bitmap's copy-constructor is used here because images loaded from
                // streams are read-only and cannot be directly edited
                bitmap = new Bitmap(img.Width, img.Height, img.PixelFormat);
                FastBitmap.CopyPixels(img, bitmap);

                img.Dispose();
            }

            return bitmap;
        }
    }
}
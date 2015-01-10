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
using System.Drawing.Imaging;
using System.IO;

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
        public static void SaveImageToStream(Bitmap bitmap, Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            // Save the space for the image size on the stream
            long sizeOffset = stream.Position;
            writer.Write((long)0);

            // Save the frame image
            bitmap.Save(stream, ImageFormat.Png);

            // Skip back to the image size offset and save the size
            long streamEnd = stream.Position;
            stream.Position = sizeOffset;
            writer.Write(streamEnd - sizeOffset - 8);

            // Skip back to the end to keep saving
            stream.Position = streamEnd;
        }

        /// <summary>
        /// Loads a bitmap image from the given stream. The stream must contain a long value at its current position specifying the size of the image on the stream
        /// </summary>
        /// <param name="stream">The stream to load the image from</param>
        /// <returns>A bitmap generated from the stream</returns>
        public static Bitmap LoadImageFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            // Read the size of the frame texture
            long textSize = reader.ReadInt64();

            MemoryStream memStream = new MemoryStream();

            long pos = stream.Position;

            byte[] buff = new byte[textSize];
            stream.Read(buff, 0, buff.Length);
            stream.Position = pos + textSize;

            memStream.Write(buff, 0, buff.Length);

            Image img = Image.FromStream(memStream);

            // The Bitmap constructor is used here because images loaded from streams are read-only and cannot be directly edited
            Bitmap bitmap = new Bitmap(img);

            img.Dispose();

            memStream.Dispose();

            return bitmap;
        }
    }
}
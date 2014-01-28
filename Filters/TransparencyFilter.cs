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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

using Pixelaria.Utils;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a Transparency filter
    /// </summary>
    public class TransparencyFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return Transparency != 1; } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Transparency"; } }

        /// <summary>
        /// Gets or sets the Transparency component as a floating point value ranging from [0 - 1]
        /// </summary>
        public float Transparency { get; set; }

        /// <summary>
        /// Applies this TransparencyFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public unsafe void ApplyToBitmap(Bitmap bitmap)
        {
            if (Transparency == 1)
                return;

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int width = bitmap.Width;
            int height = bitmap.Height;

            int bpp = 4;

            byte* scan0b = (byte*)data.Scan0;
            byte* endPixel = scan0b + bpp * width * height;

            scan0b += 3;

            while(scan0b < endPixel)
            {
                *scan0b = (byte)(*scan0b * Transparency);
                /*byte a = *scan0b;

                a = (byte)(a * Transparency);

                *scan0b = a;*/

                scan0b += 4;
            }

            bitmap.UnlockBits(data);
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(Transparency);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            Transparency = reader.ReadSingle();
        }
    }
}
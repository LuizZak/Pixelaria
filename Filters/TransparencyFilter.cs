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
        public void ApplyToBitmap(Bitmap bitmap)
        {
            if (Transparency == 1)
                return;

            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.Lock();

            // Multiply all the alpha pixels
            for (int y = 0; y < fastBitmap.Height; y++)
            {
                for (int x = 0; x < fastBitmap.Width; x++)
                {
                    // Get the Alpha component of the current pixel
                    int color = fastBitmap.GetPixelInt(x, y);
                    int a = (color >> 24) & 0xFF;

                    a = (int)(a * Transparency);

                    // Re-apply the pixel back
                    fastBitmap.SetPixel(x, y, (a << 24) | (color & 0xFFFFFF));
                }
            }

            fastBitmap.Unlock();
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
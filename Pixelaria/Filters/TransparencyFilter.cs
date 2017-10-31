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
using System.Reflection;
using FastBitmapLib;
using JetBrains.Annotations;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a Transparency filter
    /// </summary>
    internal class TransparencyFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => Transparency < 1;

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Transparency";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// Gets or sets the Transparency component as a floating point value ranging from [0 - 1]
        /// </summary>
        public float Transparency { get; set; }

        /// <summary>
        /// Initializes a new instance of the TransparencyFilter class
        /// </summary>
        public TransparencyFilter()
        {
            Transparency = 1;
        }

        /// <summary>
        /// Applies this TransparencyFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public unsafe void ApplyToBitmap(Bitmap bitmap)
        {
            // 
            // !!!   ATENTION: UNSAFE POINTER HANDLING    !!!
            // !!! WATCH IT WHEN MESSING WITH THIS METHOD !!!
            // 

            if (!Modifying || bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            if (Transparency <= 0)
                Transparency = 0;

            using (var fastBitmap = bitmap.FastLock())
            {
                // ReSharper disable once InconsistentNaming
                byte* scan0b = (byte*)fastBitmap.Scan0;

                const int loopUnroll = 8;
                int count = bitmap.Width * bitmap.Height;
                int rem = count % loopUnroll;
                count /= loopUnroll;

                // Pre-align to the alpha offset
                scan0b += 3;

                // Unrolled loop for faster operations
                while (count-- > 0)
                {
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;

                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                    *scan0b = (byte)(*scan0b * Transparency); scan0b += 4;
                }
                while (rem-- > 0)
                {
                    *scan0b = (byte)(*scan0b * Transparency);
                    scan0b += 4;
                }
            }
        }

        /// <summary>
        /// Array of property infosfrom this <see cref="IFilter"/> that can be inspected and set using reflection.
        /// 
        /// Used by export pipeline UI to streamling process of creating pipeline nodes based off of filters.
        /// </summary>
        public PropertyInfo[] InspectableProperties()
        {
            return new []
            {
                GetType().GetProperty(nameof(Transparency))
            };
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Transparency);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            Transparency = reader.ReadSingle();
        }

        public bool Equals(IFilter filter)
        {
            var other = filter as TransparencyFilter;

            return other != null && Math.Abs(Transparency - other.Transparency) < float.Epsilon && Version == other.Version;
        }
    }
}
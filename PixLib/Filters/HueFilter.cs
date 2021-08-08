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
using System.Reflection;
using FastBitmapLib;
using JetBrains.Annotations;
using PixCore.Colors;

namespace PixLib.Filters
{
    /// <summary>
    /// Implements a Hue alteration filter
    /// </summary>
    public class HueFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => !(Hue == 0 && Relative);

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Hue";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// HUE value ranging from 0 - 360
        /// </summary>
        public int Hue { get; set; }

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative { get; set; }

        /// <summary>
        /// Applies this HueFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this HueFilter to</param>
        public unsafe void ApplyToBitmap(Bitmap bitmap)
        {
            // 
            // !!!   ATENTION: UNSAFE POINTER HANDLING    !!!
            // !!! WATCH IT WHEN MESSING WITH THIS METHOD !!!
            // 

            if (!Modifying || bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            // Lock the bitmap
            using (var fastBitmap = bitmap.FastLock())
            {
                int* scan0 = (int*) fastBitmap.Scan0;
                int count = bitmap.Width * bitmap.Height;

                float hueF = Hue / 360.0f;

                while (count-- > 0)
                {
                    var ahsl = AhslColor.FromArgb(*scan0);

                    var newHue = Relative ? (ahsl.FloatHue + hueF) % 1.0f : hueF;

                    * scan0++ = new AhslColor(ahsl.FloatAlpha, newHue, ahsl.FloatSaturation, ahsl.FloatLightness).ToArgb();
                }
            }
        }

        /// <summary>
        /// Array of property infos from this <see cref="IFilter"/> that can be inspected and set using reflection.
        /// 
        /// Used by export pipeline UI to streamlining process of creating pipeline nodes based off of filters.
        /// </summary>
        public PropertyInfo[] InspectableProperties()
        {
            return new[]
            {
                GetType().GetProperty(nameof(Hue)),
                GetType().GetProperty(nameof(Relative))
            };
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Hue);
            writer.Write(Relative);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            Hue = reader.ReadInt32();
            Relative = reader.ReadBoolean();
        }

        public bool Equals(IFilter filter)
        {
            return filter is HueFilter other && Hue == other.Hue && Relative == other.Relative && Version == other.Version;
        }
    }
}
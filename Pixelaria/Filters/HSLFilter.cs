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
using Pixelaria.Utils;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a Hue alteration filter
    /// </summary>
    internal class HueFilter : IFilter
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
        /// Array of property infosfrom this <see cref="IFilter"/> that can be inspected and set using reflection.
        /// 
        /// Used by export pipeline UI to streamling process of creating pipeline nodes based off of filters.
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
            var other = filter as HueFilter;

            return other != null && Hue == other.Hue && Relative == other.Relative && Version == other.Version;
        }
    }

    /// <summary>
    /// Implements a Hue alteration filter
    /// </summary>
    internal class SaturationFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => !(Saturation == 0 && Relative) && !(Saturation == 100 && Multiply);

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Saturation";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// Saturation value ranging from 0 - 100
        /// </summary>
        public int Saturation { get; set; }

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative { get; set; }

        /// <summary>
        /// Gets or sets whether to keep the grays
        /// </summary>
        public bool KeepGrays { get; set; }

        /// <summary>
        /// Gets or sets whether to multiply the current values instead of adding to them
        /// </summary>
        public bool Multiply { get; set; }

        /// <summary>
        /// Initializes a new instance of the SaturationFilter class
        /// </summary>
        public SaturationFilter()
        {
            Saturation = 100;
            Relative = false;
            KeepGrays = true;
            Multiply = false;
        }

        /// <summary>
        /// Applies this SaturationFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this SaturationFilter to</param>
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

                float satF = Saturation / 100.0f;

                while (count-- > 0)
                {
                    var ahsl = AhslColor.FromArgb(*scan0);
                    float s = ahsl.FloatSaturation;

                    if (!KeepGrays || ahsl.FloatSaturation > 0)
                    {
                        if (Multiply)
                        {
                            s = ahsl.FloatSaturation * satF;
                        }
                        else
                        {
                            s = Relative ? ahsl.FloatSaturation + satF : satF;
                        }
                    }

                    *scan0++ = new AhslColor(ahsl.FloatAlpha, ahsl.FloatHue, s, ahsl.FloatLightness).ToArgb();
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
            return new[]
            {
                GetType().GetProperty(nameof(Saturation)),
                GetType().GetProperty(nameof(Relative)),
                GetType().GetProperty(nameof(KeepGrays)),
                GetType().GetProperty(nameof(Multiply))
            };
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Saturation);
            writer.Write(Relative);
            writer.Write(Multiply);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            Saturation = reader.ReadInt32();
            Relative = reader.ReadBoolean();
            Multiply = reader.ReadBoolean();
        }

        public bool Equals(IFilter filter)
        {
            var other = filter as SaturationFilter;

            return other != null && Saturation == other.Saturation && Relative == other.Relative &&
                   KeepGrays == other.KeepGrays && Multiply == other.Multiply && Version == other.Version;
        }
    }

    /// <summary>
    /// Implements a Lightness alteration filter
    /// </summary>
    internal class LightnessFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => !(Lightness == 0 && Relative) && !(Lightness == 100 && Multiply);

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Lightness";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// Lightness value ranging from 0 - 100
        /// </summary>
        public int Lightness { get; set; }

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative { get; set; }

        /// <summary>
        /// Gets or sets whether to multiply the current values instead of adding to them
        /// </summary>
        public bool Multiply { get; set; }

        /// <summary>
        /// Initializes a new instance of the LightnessFilter class
        /// </summary>
        public LightnessFilter()
        {
            Lightness = 100;
            Relative = false;
            Multiply = false;
        }

        /// <summary>
        /// Applies this LightnessFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this LightnessFilter to</param>
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

                float lightF = Lightness / 100.0f;

                while (count-- > 0)
                {
                    var ahsl = AhslColor.FromArgb(*scan0);
                    float l;

                    if (Multiply)
                    {
                        l = ahsl.FloatLightness * lightF;
                    }
                    else
                    {
                        l = Relative ? ahsl.FloatLightness + lightF : lightF;
                    }

                    *scan0++ = new AhslColor(ahsl.FloatAlpha, ahsl.FloatHue, ahsl.FloatSaturation, l).ToArgb();
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
            return new[]
            {
                GetType().GetProperty(nameof(Lightness)),
                GetType().GetProperty(nameof(Relative)),
                GetType().GetProperty(nameof(Multiply))
            };
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Lightness);
            writer.Write(Relative);
            writer.Write(Multiply);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            Lightness = reader.ReadInt32();
            Relative = reader.ReadBoolean();
            Multiply = reader.ReadBoolean();
        }

        public bool Equals(IFilter filter)
        {
            var other = filter as LightnessFilter;

            return other != null && Lightness == other.Lightness && Relative == other.Relative &&
                   Multiply == other.Multiply && Version == other.Version;
        }
    }
}
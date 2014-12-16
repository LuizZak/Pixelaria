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

using Pixelaria.Utils;
using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.ColorControls;

namespace Pixelaria.Filters
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
        public bool Modifying { get { return !(Hue == 0 && Relative); } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Hue"; } }

        /// <summary>
        /// HUE value ranging from 0 - 360
        /// </summary>
        public int Hue;

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative;

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

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            // Lock the bitmap
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int* scan0 = (int*)data.Scan0;
            int count = bitmap.Width * bitmap.Height;

            while (count-- > 0)
            {
                int c = *(scan0);

                AHSL ahsl = AHSL.FromArgb(c);

                ahsl.H = (Relative ? (ahsl.H + Hue) % 360 : Hue);

                *scan0 = ahsl.ToArgb();

                scan0++;
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

            writer.Write(Hue);
            writer.Write(Relative);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            Hue = reader.ReadInt32();
            Relative = reader.ReadBoolean();
        }
    }

    /// <summary>
    /// Implements a Hue alteration filter
    /// </summary>
    public class SaturationFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return !(Saturation == 0 && Relative) && !(Saturation == 100 && Multiply); } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Saturation"; } }

        /// <summary>
        /// Saturation value ranging from 0 - 100
        /// </summary>
        public int Saturation;

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative;

        /// <summary>
        /// Gets or sets whether to keep the grays
        /// </summary>
        public bool KeepGrays;

        /// <summary>
        /// Gets or sets whether to multiply the current values instead of adding to them
        /// </summary>
        public bool Multiply;

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

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            // Lock the bitmap
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int* scan0 = (int*)data.Scan0;
            int count = bitmap.Width * bitmap.Height;

            while (count-- > 0)
            {
                int c = *(scan0);

                AHSL ahsl = AHSL.FromArgb(c);

                if (!KeepGrays || ahsl.S > 0)
                {
                    if (Multiply)
                    {
                        ahsl.S = (int)(ahsl.S * (float)Math.Max(0, Saturation) / 100);
                    }
                    else
                    {
                        ahsl.S = (Relative ? Math.Min(100, (ahsl.S + Saturation)) : Saturation);
                    }
                }

                *scan0 = ahsl.ToArgb();

                scan0++;
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

            writer.Write(Saturation);
            writer.Write(Relative);
            writer.Write(Multiply);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            Saturation = reader.ReadInt32();
            Relative = reader.ReadBoolean();
            Multiply = reader.ReadBoolean();
        }
    }

    /// <summary>
    /// Implements a Lightness alteration filter
    /// </summary>
    public class LightnessFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return !(Lightness == 0 && Relative) && !(Lightness == 100 && Multiply); } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Lightness"; } }

        /// <summary>
        /// Lightness value ranging from 0 - 100
        /// </summary>
        public int Lightness;

        /// <summary>
        /// Gets or sets whether the changes made by this HSL filter are relative to current values
        /// </summary>
        public bool Relative;

        /// <summary>
        /// Gets or sets whether to multiply the current values instead of adding to them
        /// </summary>
        public bool Multiply;

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

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                return;

            // Lock the bitmap
            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);

            int* scan0 = (int*)data.Scan0;
            int count = bitmap.Width * bitmap.Height;

            while (count-- > 0)
            {
                int c = *(scan0);

                AHSL ahsl = AHSL.FromArgb(c);

                if (Multiply)
                {
                    ahsl.L = (int)(ahsl.L * (float)Math.Max(0, Lightness) / 100);
                }
                else
                {
                    ahsl.L = (Relative ? Math.Min(100, (ahsl.L + Lightness)) : Lightness);
                }

                *scan0 = ahsl.ToArgb();

                scan0++;
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

            writer.Write(Lightness);
            writer.Write(Relative);
            writer.Write(Multiply);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            Lightness = reader.ReadInt32();
            Relative = reader.ReadBoolean();
            Multiply = reader.ReadBoolean();
        }
    }
}
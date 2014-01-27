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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;

using Pixelaria.Filters;

using Pixelaria.Utils;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a Fade filter
    /// </summary>
    public class FadeFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return FadeFactor > 0; } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Fade Color"; } }

        /// <summary>
        /// Gets or sets the color to fade the image with
        /// </summary>
        public Color FadeColor { get; set; }

        /// <summary>
        /// The factor to use when fade the two colors together
        /// </summary>
        public float FadeFactor { get; set; }

        /// <summary>
        /// Whether to fade the alpha as well
        /// </summary>
        public bool FadeAlpha { get; set; }

        /// <summary>
        /// Initializes a new instance of the BlendFilter class
        /// </summary>
        public FadeFilter()
        {
            FadeColor = Color.White;
            FadeFactor = 0.5f;
            FadeAlpha = false;
        }

        /// <summary>
        /// Applies this FadeFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public void ApplyToBitmap(Bitmap bitmap)
        {
            FastBitmap fb = new FastBitmap(bitmap);

            fb.Lock();

            for (int y = 0; y < fb.Height; y++)
            {
                for (int x = 0; x < fb.Width; x++)
                {
                    Color pix = fb.GetPixel(x, y);

                    fb.SetPixel(x, y, pix.Fade(FadeColor, FadeFactor, FadeAlpha));
                }
            }

            fb.Unlock();
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(FadeColor.ToArgb());
            writer.Write(FadeFactor);
            writer.Write(FadeAlpha);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            FadeColor = Color.FromArgb(reader.ReadInt32());
            FadeFactor = reader.ReadSingle();
            FadeAlpha = reader.ReadBoolean();
        }
    }
}
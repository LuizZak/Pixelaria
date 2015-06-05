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
using System.Drawing.Drawing2D;
using System.IO;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a Scaling filter
    /// </summary>
    public class ScaleFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => Math.Abs(ScaleX - 1) > float.Epsilon || Math.Abs(ScaleY - 1) > float.Epsilon;

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Scale";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

        /// <summary>
        /// Gets or sets the X scale component as a floating point value
        /// </summary>
        public float ScaleX { get; set; }

        /// <summary>
        /// Gets or sets the Y scale component as a floating point value
        /// </summary>
        public float ScaleY { get; set; }

        /// <summary>
        /// Gets or sets whether to center the scaled image
        /// </summary>
        public bool Centered { get; set; }

        /// <summary>
        /// Gets or sets whether to use nearest-neighbor quality
        /// </summary>
        public bool PixelQuality { get; set; }

        /// <summary>
        /// Initializes a new instance of the ScaleFilter class
        /// </summary>
        public ScaleFilter()
        {
            ScaleX = 1;
            ScaleY = 1;
        }

        /// <summary>
        /// Applies this ScaleFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public void ApplyToBitmap(Bitmap bitmap)
        {
            if (!Modifying)
                return;

            Bitmap bit = (Bitmap)bitmap.Clone();

            Graphics g = Graphics.FromImage(bitmap);

            g.Clear(Color.Transparent);

            g.InterpolationMode = PixelQuality ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic;

            RectangleF rec = new RectangleF(0, 0, bitmap.Width, bitmap.Height);

            rec.Width *= ScaleX;
            rec.Height *= ScaleY;

            if (Centered)
            {
                rec.X = (float)Math.Round(bitmap.Width / 2.0f - rec.Width / 2);
                rec.Y = (float)Math.Round(bitmap.Height / 2.0f - rec.Height / 2);
            }

            g.DrawImage(bit, rec, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);

            g.Dispose();
            bit.Dispose();
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(ScaleX);
            writer.Write(ScaleY);

            writer.Write(Centered);
            writer.Write(PixelQuality);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream(Stream stream, int version)
        {
            BinaryReader reader = new BinaryReader(stream);

            ScaleX = reader.ReadSingle();
            ScaleY = reader.ReadSingle();

            Centered = reader.ReadBoolean();
            PixelQuality = reader.ReadBoolean();
        }
    }
}
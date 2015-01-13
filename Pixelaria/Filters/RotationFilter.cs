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
    /// Implements a Rotation filter
    /// </summary>
    public class RotationFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return Math.Abs((Rotation % 360)) > float.Epsilon; } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Rotation"; } }

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version
        {
            get { return 1; }
        }

        /// <summary>
        /// Gets or sets the Rotation component as a floating point value ranging from [0 - 360]
        /// </summary>
        public float Rotation { get; set; }

        /// <summary>
        /// Gets or sets whether to rotate around the center of the image
        /// </summary>
        public bool RotateAroundCenter { get; set; }

        /// <summary>
        /// Gets or sets whether to use nearest-neighbor quality
        /// </summary>
        public bool PixelQuality { get; set; }

        /// <summary>
        /// Applies this RotationFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this RotationFilter to</param>
        public void ApplyToBitmap(Bitmap bitmap)
        {
            if (!Modifying)
                return;

            Bitmap bit = (Bitmap)bitmap.Clone();

            Point pivot = new Point();

            Graphics gfx = Graphics.FromImage(bitmap);

            gfx.Clear(Color.Transparent);

            if (RotateAroundCenter)
            {
                gfx.TranslateTransform(bitmap.Width / 2.0f, bitmap.Height / 2.0f);
            }

            gfx.InterpolationMode = (PixelQuality ? InterpolationMode.NearestNeighbor : InterpolationMode.HighQualityBicubic);

            gfx.RotateTransform(Rotation);

            if (RotateAroundCenter)
            {
                gfx.TranslateTransform(-bitmap.Width / 2.0f, -bitmap.Height / 2.0f);
            }

            gfx.DrawImage(bit, pivot);

            gfx.Flush();
            gfx.Dispose();

            bit.Dispose();
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(Rotation);
            writer.Write(RotateAroundCenter);
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

            Rotation = reader.ReadSingle();
            RotateAroundCenter = reader.ReadBoolean();
            PixelQuality = reader.ReadBoolean();
        }
    }
}
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

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements an Offseting filter
    /// </summary>
    public class OffsetFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return OffsetX != 0 || OffsetY != 0; } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Offset"; } }

        /// <summary>
        /// Gets or sets the X offset component as a floating point value
        /// </summary>
        public float OffsetX { get; set; }

        /// <summary>
        /// Gets or sets the Y offset component as a floating point value
        /// </summary>
        public float OffsetY { get; set; }

        /// <summary>
        /// Gets or sets whether to wrap the image around the X axis
        /// </summary>
        public bool WrapHorizontal { get; set; }

        /// <summary>
        /// Gets or sets whether to wrap the image around the Y axis
        /// </summary>
        public bool WrapVertical { get; set; }

        /// <summary>
        /// Applies this OffsetFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public void ApplyToBitmap(Bitmap bitmap)
        {
            if (OffsetX == 0 && OffsetY == 0)
                return;

            Bitmap bit = bitmap.Clone() as Bitmap;

            Graphics g = Graphics.FromImage(bitmap);

            g.Clear(Color.Transparent);

            RectangleF rec = new RectangleF(0, 0, bitmap.Width, bitmap.Height);

            rec.X += OffsetX;
            rec.Y += OffsetY;

            g.DrawImage(bit, rec, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);

            // Draw wrap-arounds
            if (WrapHorizontal && OffsetX != 0)
            {
                RectangleF wrapRec = rec;

                if (OffsetX > 0)
                {
                    wrapRec.X -= bitmap.Width;
                }
                else
                {
                    wrapRec.X += bitmap.Width;
                }

                g.DrawImage(bit, wrapRec, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }

            if (WrapVertical && OffsetY != 0)
            {
                RectangleF wrapRec = rec;

                if (OffsetY > 0)
                {
                    wrapRec.Y -= bitmap.Height;
                }
                else
                {
                    wrapRec.Y += bitmap.Height;
                }

                g.DrawImage(bit, wrapRec, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }

            // Diagonal wrap-arounds
            if (WrapVertical && WrapHorizontal && OffsetX != 0 && OffsetY != 0)
            {
                RectangleF wrapRec = rec;

                if (OffsetX > 0)
                {
                    wrapRec.X -= bitmap.Width;
                }
                else
                {
                    wrapRec.X += bitmap.Width;
                }

                if (OffsetY > 0)
                {
                    wrapRec.Y -= bitmap.Height;
                }
                else
                {
                    wrapRec.Y += bitmap.Height;
                }

                g.DrawImage(bit, wrapRec, new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }

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

            writer.Write(OffsetX);
            writer.Write(OffsetY);
            writer.Write(WrapHorizontal);
            writer.Write(WrapVertical);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            OffsetX = reader.ReadSingle();
            OffsetY = reader.ReadSingle();
            WrapHorizontal = reader.ReadBoolean();
            WrapVertical = reader.ReadBoolean();
        }
    }
}
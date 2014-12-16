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

using Pixelaria.Utils;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a stroke filter
    /// </summary>
    public class StrokeFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying { get { return StrokeRadius > 0; } }

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name { get { return "Stroke"; } }

        /// <summary>
        /// Gets or sets the color of this stroke
        /// </summary>
        public Color StrokeColor { get; set; }

        /// <summary>
        /// Gets or sets the radius of this stroke
        /// </summary>
        public int StrokeRadius { get; set; }

        /// <summary>
        /// Gets or sets whether to knockout the original image and leave only the outline
        /// </summary>
        public bool KnockoutImage { get; set; }

        /// <summary>
        /// Gets or sets whether to smooth out the outline
        /// </summary>
        public bool Smooth { get; set; }

        /// <summary>
        /// Initializes a new instance of the StrokeFilter class
        /// </summary>
        public StrokeFilter()
        {
            this.StrokeColor = Color.Red;
            this.StrokeRadius = 1;
            this.KnockoutImage = false;
            this.Smooth = false;
        }

        /// <summary>
        /// Applies this OffsetFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public void ApplyToBitmap(Bitmap bitmap)
        {
            if(!Modifying)
                return;

            Bitmap bmo = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

            FastBitmap fbi = new FastBitmap(bitmap);
            FastBitmap fbo = new FastBitmap(bmo);

            fbi.Lock();
            fbo.Lock();

            int w = bitmap.Width;
            int h = bitmap.Height;

            int strokeColorInt = StrokeColor.ToArgb();

            int strokeRadius = StrokeRadius + (Smooth ? 1 : 0);

            // Apply the stroke
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int a = (fbi.GetPixelInt(x, y) >> 24) & 0xFF;

                    // If the pixel has an alpha of 0, it can't apply any stroke
                    if (a == 0)
                        continue;

                    for (int sy = Math.Max(y - strokeRadius, 0); sy <= Math.Min(y + strokeRadius, h - 1); sy++)
                    {
                        for (int sx = Math.Max(x - strokeRadius, 0); sx <= Math.Min(x + strokeRadius, w - 1); sx++)
                        {
                            // Don't apply any stroke on top of fully opaque pixels
                            if ((sx == x && sy == y) || ((fbi.GetPixelInt(sx, sy) >> 24) & 0xFF) == 255)
                                continue;

                            double dx = sx - x;
                            double dy = sy - y;
                            double dis = Math.Sqrt(dx * dx + dy * dy);

                            if (dis > strokeRadius)
                                continue;

                            dx = 1 - dx / strokeRadius;
                            dy = 1 - dy / strokeRadius;

                            dx = Math.Max(0, Math.Min(1, dx));

                            int outA = 255;
                            int outC = (outA << 24) + (strokeColorInt & 0xFFFFFF);

                            if(Smooth)
					        {
						        float oldA = ((fbo.GetPixelInt(sx, sy) >> 24) & 0xFF) / 255.0f;

                                outC = (Math.Min(255, (int)((oldA + (1 - (dis) / strokeRadius)) * 0xFF)) << 24) + (strokeColorInt & 0xFFFFFF);
					        }

                            fbo.SetPixel(sx, sy, outC);
                        }
                    }
                }
            }

            fbo.Unlock();
            fbi.Unlock();

            // Draw the underlying image
            if (!KnockoutImage)
            {
                Graphics gphTemp = Graphics.FromImage(bmo);
                gphTemp.CompositingMode = CompositingMode.SourceOver;
                gphTemp.DrawImage(bitmap, Point.Empty);
                gphTemp.Dispose();
            }

            // Draw to the image now
            Graphics gphOut = Graphics.FromImage(bitmap);
            gphOut.CompositingMode = CompositingMode.SourceCopy;
            gphOut.DrawImage(bmo, Point.Empty);
            gphOut.Dispose();

            bmo.Dispose();
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(StrokeColor.ToArgb());
            writer.Write(StrokeRadius);
            writer.Write(KnockoutImage);
            writer.Write(Smooth);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            StrokeColor = Color.FromArgb(reader.ReadInt32());
            StrokeRadius = reader.ReadInt32();
            KnockoutImage = reader.ReadBoolean();
            Smooth = reader.ReadBoolean();
        }
    }
}
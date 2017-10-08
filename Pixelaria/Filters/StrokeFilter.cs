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
using FastBitmapLib;
using JetBrains.Annotations;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Implements a stroke filter
    /// </summary>
    internal class StrokeFilter : IFilter
    {
        /// <summary>
        /// Gets a value indicating whether this IFilter instance will modify any of the pixels
        /// of the bitmap it is applied on with the current settings
        /// </summary>
        public bool Modifying => StrokeRadius > 0;

        /// <summary>
        /// Gets the unique display name of this filter
        /// </summary>
        public string Name => "Stroke";

        /// <summary>
        /// Gets the version of the filter to be used during persistence operations
        /// </summary>
        public int Version => 1;

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
            StrokeColor = Color.Red;
            StrokeRadius = 1;
            KnockoutImage = false;
            Smooth = false;
        }

        /// <summary>
        /// Applies this OffsetFilter to a Bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap to apply this TransparencyFilter to</param>
        public unsafe void ApplyToBitmap(Bitmap bitmap)
        {
            // 
            // !!!   ATENTION: UNSAFE POINTER HANDLING    !!!
            // !!! WATCH IT WHEN MESSING WITH THIS METHOD !!!
            // 

            if(!Modifying)
                return;

            using (var bmo = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat))
            {
                using (var fbi = bitmap.FastLock())
                using (var fbo = bmo.FastLock())
                {
                    int w = bitmap.Width;
                    int h = bitmap.Height;

                    int strokeColorInt = StrokeColor.ToArgb() & 0xFFFFFF;

                    int strokeRadius = StrokeRadius + (Smooth ? 1 : 0);
                    int strokeRadiusSqrd = strokeRadius * strokeRadius;

                    int* scan0I = (int*) fbi.Scan0;
                    int* scan0O = (int*) fbo.Scan0;

                    int strideWidthI = fbi.Stride;

                    bool smooth = Smooth;
                
                    // Apply the stroke
                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int a = (*(scan0I + strideWidthI * y + x) >> 24) & 0xFF;

                            // If the pixel has an alpha of 0, it can't apply any stroke
                            if (a == 0)
                                continue;

                            int minY = Math.Max(y - strokeRadius, 0), maxY = Math.Min(y + strokeRadius, h - 1);
                            int minX = Math.Max(x - strokeRadius, 0), maxX = Math.Min(x + strokeRadius, w - 1);

                            for (int sy = minY; sy <= maxY; sy++)
                            {
                                // We cache the Y pointer offset here so we don't need to recalculate for each column of pixels over and over
                                int yMod = strideWidthI * sy;

                                for (int sx = minX; sx <= maxX; sx++)
                                {
                                    // Don't apply any stroke on top of fully opaque pixels
                                    if ((*(scan0I + yMod + sx) & 0xFF000000) == 0xFF000000 || (sx == x && sy == y))
                                        continue;

                                    int dx = sx - x;
                                    int dy = sy - y;
                                    int dis = dx * dx + dy * dy;

                                    if (dis > strokeRadiusSqrd)
                                        continue;

                                    const int outA = 255;
                                    int outC = (outA << 24) + strokeColorInt;

                                    if (smooth)
                                    {
                                        float oldA = ((*(scan0O + yMod + sx) >> 24) & 0xFF) / 255.0f;

                                        outC = (Math.Min(255,
                                                    (int) ((oldA + (1 - Math.Sqrt(dis) / strokeRadius)) * 0xFF)) << 24) +
                                               strokeColorInt;
                                    }

                                    *(scan0O + yMod + sx) = outC;
                                }
                            }
                        }
                    }
                }

                // Draw the underlying image
                if (!KnockoutImage)
                {
                    using (var gphTemp = Graphics.FromImage(bmo))
                    {
                        gphTemp.CompositingMode = CompositingMode.SourceOver;
                        gphTemp.DrawImage(bitmap, Point.Empty);
                        gphTemp.Flush();
                    }
                }

                // Draw to the image now
                using (var gphOut = Graphics.FromImage(bitmap))
                {
                    gphOut.CompositingMode = CompositingMode.SourceCopy;
                    gphOut.DrawImage(bmo, Point.Empty);
                    gphOut.Flush();
                }
            }
        }

        /// <summary>
        /// Saves the properties of this filter to the given stream
        /// </summary>
        /// <param name="stream">A Stream to save the data to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(StrokeColor.ToArgb());
            writer.Write(StrokeRadius);
            writer.Write(KnockoutImage);
            writer.Write(Smooth);
        }

        /// <summary>
        /// Loads the properties of this filter from the given stream
        /// </summary>
        /// <param name="stream">A Stream to load the data from</param>
        /// <param name="version">The version of the filter data that is stored on the stream</param>
        public void LoadFromStream([NotNull] Stream stream, int version)
        {
            var reader = new BinaryReader(stream);

            StrokeColor = Color.FromArgb(reader.ReadInt32());
            StrokeRadius = reader.ReadInt32();
            KnockoutImage = reader.ReadBoolean();
            Smooth = reader.ReadBoolean();
        }

        public bool Equals(IFilter filter)
        {
            var other = filter as StrokeFilter;

            return other != null && StrokeColor == other.StrokeColor && StrokeRadius == other.StrokeRadius &&
                   KnockoutImage == other.KnockoutImage && Smooth == other.Smooth && Version == other.Version;
        }
    }
}
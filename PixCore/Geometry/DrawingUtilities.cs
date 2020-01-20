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
using System.Windows.Forms;
using JetBrains.Annotations;

namespace PixCore.Geometry
{
    public static class DrawingUtilities
    {
        /// <summary>
        /// Returns the distance between two points objects
        /// </summary>
        /// <param name="point">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>The distance between the two points</returns>
        [Pure]
        public static float Distance(in this PointF point, in PointF point2)
        {
            float dx = point.X - point2.X;
            float dy = point.Y - point2.Y;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the squared distance between two points objects
        /// </summary>
        /// <param name="point">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>The squared distance between the two points</returns>
        [Pure]
        public static float DistanceSquared(in this PointF point, in PointF point2)
        {
            float dx = point.X - point2.X;
            float dy = point.Y - point2.Y;

            return dx * dx + dy * dy;
        }

        /// <summary>
        /// Returns the distance between two points objects
        /// </summary>
        /// <param name="point">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>The distance between the two points</returns>
        [Pure]
        public static float Distance(in this Point point, in Point point2)
        {
            float dx = point.X - point2.X;
            float dy = point.Y - point2.Y;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the center-point of a rectangle
        /// </summary>
        [Pure]
        public static Point Center(in this Rectangle rectangle)
        {
            return new Point((rectangle.Left + rectangle.Right) / 2, (rectangle.Top + rectangle.Bottom) / 2);
        }

        /// <summary>
        /// Returns the center-point of a rectangle
        /// </summary>
        [Pure]
        public static PointF Center(in this RectangleF rectangle)
        {
            return new PointF((rectangle.Left + rectangle.Right) / 2, (rectangle.Top + rectangle.Bottom) / 2);
        }

        // 
        // C# assembly implementation
        // 
        public static RectangleF RectangleFit(RectangleF availableBounds, SizeF rectangleSize, ImageLayout imageLayout)
        {
            var rectangle = availableBounds;
            if (rectangleSize.IsEmpty) 
                return rectangle;

            switch (imageLayout)
            {
                case ImageLayout.None:
                    rectangle.Size = rectangleSize;
                    break;

                case ImageLayout.Tile:
                    rectangle.Size = rectangleSize;
                    break;

                case ImageLayout.Center:
                    rectangle.Size = rectangleSize;
                    var size = availableBounds.Size;
                    if (size.Width > rectangle.Width)
                    {
                        rectangle.X = (size.Width - rectangle.Width) / 2;
                    }
                    if (size.Height > rectangle.Height)
                    {
                        rectangle.Y = (size.Height - rectangle.Height) / 2;
                    }
                    break;

                case ImageLayout.Stretch:
                    rectangle.Size = availableBounds.Size;
                    break;

                case ImageLayout.Zoom:
                    var size2 = rectangleSize;
                    float num = availableBounds.Width / size2.Width;
                    float num2 = availableBounds.Height / size2.Height;

                    if (size2.Width <= availableBounds.Width && size2.Height <= availableBounds.Height)
                    {
                        return new RectangleF(availableBounds.Width / 2 - size2.Width / 2, availableBounds.Height / 2 - size2.Height / 2, size2.Width, size2.Height);
                    }

                    if (num >= num2)
                    {
                        rectangle.Height = availableBounds.Height;
                        rectangle.Width = (int)((size2.Width * num2) + 0.5);

                        if (availableBounds.X >= 0)
                        {
                            rectangle.X = (availableBounds.Width - rectangle.Width) / 2;
                        }
                    }
                    else
                    {
                        rectangle.Width = availableBounds.Width;
                        rectangle.Height = (int)((size2.Height * num) + 0.5);

                        if (availableBounds.Y >= 0)
                        {
                            rectangle.Y = (availableBounds.Height - rectangle.Height) / 2;
                        }
                    }
                    break;
            }

            return rectangle;
        }
    }
}
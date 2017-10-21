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
using JetBrains.Annotations;

namespace Pixelaria.Utils
{
    public static class DrawingUtilities
    {
        /// <summary>
        /// Adds a rounded rectangle to this GraphicsPath
        /// </summary>
        /// <param name="gfxPath">The GraphicsPath to add the rounded rectangle to</param>
        /// <param name="bounds">The bounds of the rounded rectangle</param>
        /// <param name="cornerRadius">The radius of the corners</param>
        public static void AddRoundedRectangle([NotNull] this GraphicsPath gfxPath, RectangleF bounds, int cornerRadius)
        {
            gfxPath.AddArc(bounds.X, bounds.Y, cornerRadius, cornerRadius, 180, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y, cornerRadius, cornerRadius, 270, 90);
            gfxPath.AddArc(bounds.X + bounds.Width - cornerRadius, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            gfxPath.AddArc(bounds.X, bounds.Y + bounds.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            gfxPath.CloseAllFigures();
        }

        /// <summary>
        /// Performs a set of changes on an action block that reset the Graphics object to its previous
        /// Graphics State afterwards.
        /// </summary>
        public static void WithTemporaryState([NotNull] this Graphics g, [NotNull, InstantHandle] Action action)
        {
            var compositing = g.CompositingMode;
            var interpolationMode = g.InterpolationMode;
            var pixelOffsetMode = g.PixelOffsetMode;
            var smoothingMode = g.SmoothingMode;
            var textRenderingHint = g.TextRenderingHint;
            var compositingQuality = g.CompositingQuality;
            int textContrast = g.TextContrast;
            var transform = g.Transform;
            
            try
            {
                action();
            }
            finally
            {
                g.CompositingMode = compositing;
                g.InterpolationMode = interpolationMode;
                g.PixelOffsetMode = pixelOffsetMode;
                g.SmoothingMode = smoothingMode;
                g.TextRenderingHint = textRenderingHint;
                g.CompositingQuality = compositingQuality;
                g.TextContrast = textContrast;
                g.Transform = transform;
            }
        }

        /// <summary>
        /// Performs a set of changes on an action block that reset the Graphics object to its previous
        /// Graphics State afterwards.
        /// </summary>
        public static void WithTemporaryClippingState([NotNull] this Graphics g, [NotNull] Region clip, [NotNull, InstantHandle] Action action)
        {
            var compositing = g.CompositingMode;
            var interpolationMode = g.InterpolationMode;
            var pixelOffsetMode = g.PixelOffsetMode;
            var smoothingMode = g.SmoothingMode;
            var textRenderingHint = g.TextRenderingHint;
            var compositingQuality = g.CompositingQuality;
            int textContrast = g.TextContrast;
            var transform = g.Transform;
            var region = g.Clip;

            try
            {
                action();
            }
            finally
            {
                g.CompositingMode = compositing;
                g.InterpolationMode = interpolationMode;
                g.PixelOffsetMode = pixelOffsetMode;
                g.SmoothingMode = smoothingMode;
                g.TextRenderingHint = textRenderingHint;
                g.CompositingQuality = compositingQuality;
                g.TextContrast = textContrast;
                g.Transform = transform;
                g.Clip = region;
                region.Dispose();
                clip.Dispose();
            }
        }

        [Pure]
        public static RectangleF Inflated(this RectangleF rectangle, SizeF size)
        {
            var rec = rectangle;
            rec.Inflate(size);
            return rec;
        }

        [Pure]
        public static RectangleF Inflated(this RectangleF rectangle, float x, float y)
        {
            var rec = rectangle;
            rec.Inflate(x, y);
            return rec;
        }

        [Pure]
        public static Rectangle Inflated(this Rectangle rectangle, Size size)
        {
            var rec = rectangle;
            rec.Inflate(size);
            return rec;
        }

        [Pure]
        public static Rectangle Inflated(this Rectangle rectangle, int x, int y)
        {
            var rec = rectangle;
            rec.Inflate(x, y);
            return rec;
        }

        [Pure]
        public static RectangleF OffsetBy(this RectangleF rectangle, float x, float y)
        {
            var rec = rectangle;
            rec.Offset(x, y);
            return rec;
        }

        [Pure]
        public static Rectangle OffsetBy(this Rectangle rectangle, int x, int y)
        {
            var rec = rectangle;
            rec.Offset(x, y);
            return rec;
        }

        [Pure]
        public static RectangleF OffsetBy(this RectangleF rectangle, PointF point)
        {
            var rec = rectangle;
            rec.Offset(point);
            return rec;
        }

        [Pure]
        public static Rectangle OffsetBy(this Rectangle rectangle, Point point)
        {
            var rec = rectangle;
            rec.Offset(point);
            return rec;
        }

        /// <summary>
        /// Returns the points that form the corners of a rectangle, in clockwise
        /// order, starting from the top-left corner.
        /// </summary>
        [Pure]
        public static Point[] Points(this Rectangle rectangle)
        {
            var tl = new Point(rectangle.Left, rectangle.Top);
            var tr = new Point(rectangle.Right, rectangle.Top);
            var br = new Point(rectangle.Right, rectangle.Bottom);
            var bl = new Point(rectangle.Left, rectangle.Bottom);

            return new[] {tl, tr, br, bl};
        }

        /// <summary>
        /// Returns the points that form the corners of a rectangle, in clockwise
        /// order, starting from the top-left corner.
        /// </summary>
        [Pure]
        public static PointF[] Points(this RectangleF rectangle)
        {
            var tl = new PointF(rectangle.Left, rectangle.Top);
            var tr = new PointF(rectangle.Right, rectangle.Top);
            var br = new PointF(rectangle.Right, rectangle.Bottom);
            var bl = new PointF(rectangle.Left, rectangle.Bottom);

            return new[] { tl, tr, br, bl };
        }

        /// <summary>
        /// Returns the smallest rectangle able to contain all points in an enumerable of points.
        /// 
        /// If enumerable is empty, returns Rectangle.Empty.
        /// </summary>
        [Pure]
        public static Rectangle Area([NotNull] this IEnumerable<Point> points)
        {
            var rect = Rectangle.Empty;
            var hasPoints = false;

            foreach (var point in points)
            {
                if (!hasPoints)
                {
                    rect = new Rectangle(point, Size.Empty);
                    hasPoints = true;
                }
                else
                {
                    rect = Rectangle.Union(rect, new Rectangle(point, Size.Empty));
                }
            }
            
            return rect;
        }

        /// <summary>
        /// Returns the smallest rectangle able to contain all points in an enumerable of points.
        /// 
        /// If enumerable is empty, returns Rectangle.Empty.
        /// </summary>
        [Pure]
        public static RectangleF Area([NotNull] this IEnumerable<PointF> points)
        {
            var rect = RectangleF.Empty;
            var hasPoints = false;

            foreach (var point in points)
            {
                if (!hasPoints)
                {
                    rect = new RectangleF(point, SizeF.Empty);
                    hasPoints = true;
                }
                else
                {
                    rect = RectangleF.Union(rect, new RectangleF(point, SizeF.Empty));
                }
            }

            return rect;
        }

        /// <summary>
        /// Transforms a single point by multiplying it by the matrix's value
        /// </summary>
        [Pure]
        public static Point Transform([NotNull] this Matrix matrix, Point point)
        {
            var pts = new[] {point};
            matrix.TransformPoints(pts);
            return pts[0];
        }

        /// <summary>
        /// Transforms a single point by multiplying it by the matrix's value
        /// </summary>
        [Pure]
        public static PointF Transform([NotNull] this Matrix matrix, PointF point)
        {
            var pts = new[] { point };
            matrix.TransformPoints(pts);
            return pts[0];
        }

        /// <summary>
        /// Returns an inverted copy of a matrix
        /// </summary>
        [Pure]
        public static Matrix Inverted([NotNull] this Matrix matrix)
        {
            var clone = matrix.Clone();
            clone.Invert();
            return clone;
        }

        [Pure]
        public static PointF Normalized(this PointF point)
        {
            var dx = point.X;
            var dy = point.Y;

            var dis = (float)Math.Sqrt(dx * dx + dy * dy);

            return new PointF(dx / dis, dy / dis);
        }

        [Pure]
        public static Point Rounded(this PointF point)
        {
            return Point.Round(point);
        }

        [Pure]
        public static PointF Multiplied(this PointF point, SizeF size)
        {
            return new PointF(point.X * size.Width, point.Y * size.Height);
        }

        [Pure]
        public static PointF Multiplied(this PointF point, float length)
        {
            return new PointF(point.X * length, point.Y * length);
        }


        /// <summary>
        /// Returns the distance between two points objects
        /// </summary>
        /// <param name="point">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>The distance between the two points</returns>
        [Pure]
        public static float Distance(this PointF point, PointF point2)
        {
            var dx = point.X - point2.X;
            var dy = point.Y - point2.Y;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the distance between two points objects
        /// </summary>
        /// <param name="point">The first point</param>
        /// <param name="point2">The second point</param>
        /// <returns>The distance between the two points</returns>
        [Pure]
        public static float Distance(this Point point, Point point2)
        {
            var dx = point.X - point2.X;
            var dy = point.Y - point2.Y;

            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Returns the center-point of a rectangle
        /// </summary>
        [Pure]
        public static Point Center(this Rectangle rectangle)
        {
            return new Point((rectangle.Left + rectangle.Right) / 2, (rectangle.Top + rectangle.Bottom) / 2);
        }

        /// <summary>
        /// Returns the center-point of a rectangle
        /// </summary>
        [Pure]
        public static PointF Center(this RectangleF rectangle)
        {
            return new PointF((rectangle.Left + rectangle.Right) / 2, (rectangle.Top + rectangle.Bottom) / 2);
        }
    }
}
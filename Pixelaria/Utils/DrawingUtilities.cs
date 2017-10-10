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
            GraphicsState state = null;
            try
            {
                state = g.Save();
                action();
            }
            finally
            {
                if (state != null)
                    g.Restore(state);
            }
        }

        public static RectangleF Inflated(this RectangleF rectangle, SizeF size)
        {
            var rec = rectangle;
            rec.Inflate(size);
            return rec;
        }

        public static RectangleF Inflated(this RectangleF rectangle, float x, float y)
        {
            var rec = rectangle;
            rec.Inflate(x, y);
            return rec;
        }

        public static Rectangle Inflated(this Rectangle rectangle, Size size)
        {
            var rec = rectangle;
            rec.Inflate(size);
            return rec;
        }

        public static Rectangle Inflated(this Rectangle rectangle, int x, int y)
        {
            var rec = rectangle;
            rec.Inflate(x, y);
            return rec;
        }

        public static RectangleF OffsetBy(this RectangleF rectangle, float x, float y)
        {
            var rec = rectangle;
            rec.Offset(x, y);
            return rec;
        }

        public static Rectangle OffsetBy(this Rectangle rectangle, int x, int y)
        {
            var rec = rectangle;
            rec.Offset(x, y);
            return rec;
        }

        public static RectangleF OffsetBy(this RectangleF rectangle, PointF point)
        {
            var rec = rectangle;
            rec.Offset(point);
            return rec;
        }

        public static Rectangle OffsetBy(this Rectangle rectangle, Point point)
        {
            var rec = rectangle;
            rec.Offset(point);
            return rec;
        }
    }
}
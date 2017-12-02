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
using JetBrains.Annotations;

namespace PixUI.Utils
{
    /// <summary>
    /// Contains static utility methods
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Adds a disposable object into a collection of disposable (usually a CompositeDisposable)
        /// </summary>
        public static void AddToDisposable<T>(this IDisposable disposable, [NotNull] T target) where T : ICollection<IDisposable>, IDisposable
        {
            target.Add(disposable);
        }
        
        /// <summary>
        /// Returns the smallest Rectangle object that encloses all points provided
        /// </summary>
        /// <param name="pointList">An array of points to convert</param>
        /// <returns>The smallest Rectangle object that encloses all points provided</returns>
        [Pure]
        public static Rectangle GetRectangleArea([NotNull] Point[] pointList)
        {
            int minX = pointList[0].X;
            int minY = pointList[0].Y;

            int maxX = pointList[0].X;
            int maxY = pointList[0].Y;

            foreach (var p in pointList)
            {
                minX = Math.Min(p.X, minX);
                minY = Math.Min(p.Y, minY);

                maxX = Math.Max(p.X, maxX);
                maxY = Math.Max(p.Y, maxY);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Returns the smallest Rectangle object that encloses all points provided
        /// </summary>
        /// <param name="pointList">An array of points to convert</param>
        /// <returns>The smallest Rectangle object that encloses all points provided</returns>
        [Pure]
        public static RectangleF GetRectangleArea([NotNull] IReadOnlyList<PointF> pointList)
        {
            float minX = pointList[0].X;
            float minY = pointList[0].Y;

            float maxX = pointList[0].X;
            float maxY = pointList[0].Y;

            foreach (var p in pointList)
            {
                minX = Math.Min(p.X, minX);
                minY = Math.Min(p.Y, minY);

                maxX = Math.Max(p.X, maxX);
                maxY = Math.Max(p.Y, maxY);
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
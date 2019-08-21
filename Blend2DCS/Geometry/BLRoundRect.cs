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
using System.Runtime.InteropServices;

namespace Blend2DCS.Geometry
{
    /// <summary>
    /// Rounded rectangle specified as [x, y, w, h, rx, ry] using <see cref="double"/> as a storage type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct BLRoundRect: IEquatable<BLRoundRect>
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
        public double RadiusX;
        public double RadiusY;

        public BLRoundRect(double x, double y, double width, double height, double radiusX, double radiusY)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public bool Equals(BLRoundRect other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Width.Equals(other.Width) && Height.Equals(other.Height) && RadiusX.Equals(other.RadiusX) && RadiusY.Equals(other.RadiusY);
        }

        public override bool Equals(object obj)
        {
            return obj is BLRoundRect other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Width.GetHashCode();
                hashCode = (hashCode * 397) ^ Height.GetHashCode();
                hashCode = (hashCode * 397) ^ RadiusX.GetHashCode();
                hashCode = (hashCode * 397) ^ RadiusY.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BLRoundRect left, BLRoundRect right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BLRoundRect left, BLRoundRect right)
        {
            return !left.Equals(right);
        }
    }
}

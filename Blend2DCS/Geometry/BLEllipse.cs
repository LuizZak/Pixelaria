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
    /// Ellipse specified as [cx, cy, rx, ry] using <see cref="double"/> as a storage type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct BLEllipse: IEquatable<BLEllipse>
    {
        public double X;
        public double Y;
        public double RadiusX;
        public double RadiusY;

        public BLEllipse(double x, double y, double radiusX, double radiusY)
        {
            X = x;
            Y = y;
            RadiusX = radiusX;
            RadiusY = radiusY;
        }

        public bool Equals(BLEllipse other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && RadiusX.Equals(other.RadiusX) && RadiusY.Equals(other.RadiusY);
        }

        public override bool Equals(object obj)
        {
            return obj is BLEllipse other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ RadiusX.GetHashCode();
                hashCode = (hashCode * 397) ^ RadiusY.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BLEllipse left, BLEllipse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BLEllipse left, BLEllipse right)
        {
            return !left.Equals(right);
        }
    }
}

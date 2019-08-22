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
    /// Circle specified as [cx, cy, r] using <see cref="double"/> as a storage type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct BLCircle: IEquatable<BLCircle>
    {
        public double X;
        public double Y;
        public double Radius;

        public BLCircle(double x, double y, double radius)
        {
            X = x;
            Y = y;
            Radius = radius;
        }

        public bool Equals(BLCircle other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Radius.Equals(other.Radius);
        }

        public override bool Equals(object obj)
        {
            return obj is BLCircle other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Radius.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BLCircle left, BLCircle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BLCircle left, BLCircle right)
        {
            return !left.Equals(right);
        }
    }
}

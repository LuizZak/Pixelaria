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
    /// Line specified as [x0, y0, x1, y1] using <see cref="double"/> as a storage type.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct BLLine: IEquatable<BLLine>
    {
        public double X0;
        public double Y0;
        public double X1;
        public double Y1;

        public BLLine(double x0, double y0, double x1, double y1)
        {
            X0 = x0;
            Y0 = y0;
            X1 = x1;
            Y1 = y1;
        }

        public bool Equals(BLLine other)
        {
            return X0.Equals(other.X0) && Y0.Equals(other.Y0) && X1.Equals(other.X1) && Y1.Equals(other.Y1);
        }

        public override bool Equals(object obj)
        {
            return obj is BLLine other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X0.GetHashCode();
                hashCode = (hashCode * 397) ^ Y0.GetHashCode();
                hashCode = (hashCode * 397) ^ X1.GetHashCode();
                hashCode = (hashCode * 397) ^ Y1.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(BLLine left, BLLine right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BLLine left, BLLine right)
        {
            return !left.Equals(right);
        }
    }
}

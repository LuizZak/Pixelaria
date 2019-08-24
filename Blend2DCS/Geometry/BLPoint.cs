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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Blend2DCS.Geometry
{
    /// <summary>
    /// Point specified as [x, y] using <see cref="double"/> as a storage type.
    /// </summary>
    [DebuggerDisplay("[{X}, {Y}]")]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct BLPoint: IEquatable<BLPoint>
    {
        public double X;
        public double Y;

        public BLPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool Equals(BLPoint other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            return obj is BLPoint other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public static bool operator ==(BLPoint left, BLPoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BLPoint left, BLPoint right)
        {
            return !left.Equals(right);
        }
    }
}

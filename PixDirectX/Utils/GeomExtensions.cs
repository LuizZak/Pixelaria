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

using PixCore.Geometry;
using SharpDX.Mathematics.Interop;

namespace PixDirectX.Utils
{
    /// <summary>
    /// Useful conversion methods from PixCore's to SharpDX's geometry types
    /// </summary>
    public static class GeomExtensions
    {
        /// <summary>
        /// Converts a <see cref="RawVector2"/> to an equivalent <see cref="Vector"/> value.
        /// </summary>
        public static unsafe Vector ToVector(this RawVector2 vec)
        {
            return *(Vector*)&vec;
        }

        /// <summary>
        /// Converts a <see cref="Vector"/> to an equivalent <see cref="RawVector2"/> value.
        /// </summary>
        public static unsafe RawVector2 ToRawVector2(this Vector vec)
        {
            return *(RawVector2*)&vec;
        }

        /// <summary>
        /// Converts a <see cref="RawRectangleF"/> to an equivalent <see cref="AABB"/> value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static AABB ToAABB(this RawRectangleF rec)
        {
            return new AABB(rec.Left, rec.Top, rec.Bottom, rec.Right);
        }

        /// <summary>
        /// Converts a <see cref="AABB"/> to an equivalent <see cref="RawRectangleF"/> value.
        /// </summary>
        public static RawRectangleF ToRawRectangleF(this AABB rec)
        {
            return new RawRectangleF(rec.Left, rec.Top, rec.Right, rec.Bottom);
        }
        
        /// <summary>
        /// Converts a <see cref="Matrix2D"/> to an equivalent <see cref="RawMatrix3x2"/> value.
        /// </summary>
        public static unsafe RawMatrix3x2 ToRawMatrix3X2(this Matrix2D matrix)
        {
            return *(RawMatrix3x2*)&matrix;
        }
    }
}
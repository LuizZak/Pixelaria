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

using JetBrains.Annotations;
using PixCore.Geometry;
using System.Drawing;
using System.Numerics;
using Vortice;
using Matrix = System.Drawing.Drawing2D.Matrix;
using PixVector = PixCore.Geometry.Vector;

namespace PixDirectX.Utils
{
    /// <summary>
    /// Useful conversion methods from PixCore to SharpDX and GDI+ geometry types
    /// </summary>
    public static class GeomExtensions
    {
        #region Vector / RawVector2

        /// <summary>
        /// Converts a <see cref="RawVector2"/> to an equivalent <see cref="Vector"/> value.
        /// </summary>
        public static unsafe PixVector ToVector(this Vector2 vec)
        {
            //return *(PixVector*)&vec;
            return new PixVector(vec.X, vec.Y);
        }

        /// <summary>
        /// Converts a <see cref="Vector"/> to an equivalent <see cref="RawVector2"/> value.
        /// </summary>
        public static unsafe Vector2 ToVector2(this PixVector vec)
        {
            //return *(Vector2*)&vec;
            return new Vector2(vec.X, vec.Y);
        }

        #endregion

        #region AABB / RawRectF

        /// <summary>
        /// Converts a <see cref="RawRectF"/> to an equivalent <see cref="AABB"/> value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static AABB ToAABB(this RawRectF rec)
        {
            return new AABB(rec.Left, rec.Top, rec.Bottom, rec.Right);
        }

        /// <summary>
        /// Converts a <see cref="AABB"/> to an equivalent <see cref="RectangleF"/> value.
        /// </summary>
        public static RawRectF ToRawRectF(this AABB rec)
        {
            return new RectangleF(rec.Left, rec.Top, rec.Width, rec.Height);
        }
        
        #endregion

        #region Matrix2D / Matrix3x2

        /// <summary>
        /// Converts a <see cref="Matrix2D"/> to an equivalent <see cref="RawMatrix3x2"/> value.
        /// </summary>
        public static unsafe Matrix3x2 ToRawMatrix3X2(this Matrix2D matrix)
        {
            //return *(Matrix3x2*)&matrix;
            return new Matrix3x2(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32);
        }

        /// <summary>
        /// Converts a <see cref="Matrix3x2"/> to an equivalent <see cref="Matrix2D"/> value.
        /// </summary>
        public static unsafe Matrix2D ToMatrix2D(this Matrix3x2 matrix)
        {
            //return *(Matrix2D*)&matrix;
            return new Matrix2D(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32);
        }

        #endregion

        #region Matrix2D / Matrix

        /// <summary>
        /// Converts a <see cref="Matrix2D"/> to an equivalent <see cref="Matrix"/> value.
        /// </summary>
        public static Matrix ToMatrix(this Matrix2D matrix)
        {
            return new Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, matrix.M31, matrix.M32);
        }

        /// <summary>
        /// Converts a <see cref="Matrix"/> to an equivalent <see cref="Matrix2D"/> value.
        /// </summary>
        public static unsafe Matrix2D ToMatrix2D([NotNull] this Matrix matrix)
        {
            return new Matrix2D(matrix.Elements);
        }

        #endregion
    }
}
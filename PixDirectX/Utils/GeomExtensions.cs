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

using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using SharpDX;
using SharpDX.Mathematics.Interop;
using SkiaSharp;
using Matrix = System.Drawing.Drawing2D.Matrix;

namespace PixDirectX.Utils
{
    /// <summary>
    /// Useful conversion methods from PixCore to SharpDX, GDI+, and Skia geometry types
    /// </summary>
    public static class GeomExtensions
    {
        #region Vector / RawVector2

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

        #endregion

        #region Vector / SKPoint

        /// <summary>
        /// Converts a <see cref="SKPoint"/> to an equivalent <see cref="Vector"/> value.
        /// </summary>
        public static unsafe Vector ToVector(this SKPoint vec)
        {
            return *(Vector*)&vec;
        }

        /// <summary>
        /// Converts a <see cref="Vector"/> to an equivalent <see cref="SKPoint"/> value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static unsafe SKPoint ToSKPoint(this Vector vec)
        {
            return *(SKPoint*)&vec;
        }

        #endregion

        #region AABB / RawRectangleF

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

        #endregion

        #region AABB / SKRect

        /// <summary>
        /// Converts a <see cref="SKRect"/> to an equivalent <see cref="AABB"/> value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static AABB ToAABB(this SKRect rec)
        {
            return new AABB(rec.Left, rec.Top, rec.Bottom, rec.Right);
        }

        /// <summary>
        /// Converts a <see cref="AABB"/> to an equivalent <see cref="SKRect"/> value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static SKRect ToSKRect(this AABB rec)
        {
            return new SKRect(rec.Left, rec.Top, rec.Right, rec.Bottom);
        }

        #endregion

        #region Matrix2D / RawMatrix3x2 / Matrix3x2

        /// <summary>
        /// Converts a <see cref="Matrix2D"/> to an equivalent <see cref="RawMatrix3x2"/> value.
        /// </summary>
        public static unsafe RawMatrix3x2 ToRawMatrix3X2(this Matrix2D matrix)
        {
            return *(RawMatrix3x2*)&matrix;
        }

        /// <summary>
        /// Converts a <see cref="RawMatrix3x2"/> to an equivalent <see cref="Matrix2D"/> value.
        /// </summary>
        public static unsafe Matrix2D ToMatrix2D(this RawMatrix3x2 matrix)
        {
            return *(Matrix2D*)&matrix;
        }

        /// <summary>
        /// Converts a <see cref="Matrix3x2"/> to an equivalent <see cref="Matrix2D"/> value.
        /// </summary>
        public static unsafe Matrix2D ToMatrix2D(this Matrix3x2 matrix)
        {
            return *(Matrix2D*)&matrix;
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
        public static Matrix2D ToMatrix2D([NotNull] this Matrix matrix)
        {
            return new Matrix2D(matrix.Elements);
        }

        #endregion


        #region Matrix2D / SKMatrix

        /// <summary>
        /// Converts a <see cref="Matrix2D"/> to an equivalent <see cref="SKMatrix"/> value.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static SKMatrix ToSKMatrix(this Matrix2D matrix)
        {
            var m = new SKMatrix
            {
                TransX = matrix.TranslationVector.X,
                TransY = matrix.TranslationVector.Y,
                ScaleX = matrix.ScaleVector.X,
                ScaleY = matrix.ScaleVector.Y,
                SkewX = matrix.SkewVector.X,
                SkewY = matrix.SkewVector.Y,
                Persp2 = 1
            };
            
            return m;
        }

        /// <summary>
        /// Converts a <see cref="SKMatrix"/> to an equivalent <see cref="Matrix2D"/> value.
        /// </summary>
        public static unsafe Matrix2D ToMatrix2D(this SKMatrix matrix)
        {
            return new Matrix2D(matrix.ScaleX, matrix.SkewX, matrix.SkewY, matrix.ScaleY, matrix.TransX, matrix.TransY);
        }

        #endregion
    }
}
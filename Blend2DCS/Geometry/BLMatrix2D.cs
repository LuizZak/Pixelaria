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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Blend2DCS.Geometry
{
    [DebuggerDisplay("[{M00}, {M01}, {M10}, {M11}, {M20}, {M21}]")]
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct BLMatrix2D
    {
        public double M00;
        public double M01;
        public double M10;
        public double M11;
        public double M20;
        public double M21;

        public BLMatrix2D(double m00, double m01, double m10, double m11, double m20, double m21)
        {
            M00 = m00;
            M01 = m01;
            M10 = m10;
            M11 = m11;
            M20 = m20;
            M21 = m21;
        }

        public override string ToString()
        {
            return $"{{ {M00}, {M01}, {M10}, {M11}, {M20}, {M21} }}";
        }

        public static BLMatrix2D Identity()
        {
            return new BLMatrix2D(1, 0, 0, 1, 0, 0);
        }
    }

    /// 2D matrix operation.
    public enum BLMatrix2DOp: uint
    {
        /// <summary>
        /// Reset matrix to identity (argument ignored, should be nullptr).
        /// </summary>
        Reset = 0,
        /// <summary>
        /// Assign (copy) the other matrix.
        /// </summary>
        Assign = 1,

        /// <summary>
        /// Translate the matrix by [x, y].
        /// </summary>
        Translate = 2,
        /// <summary>
        /// Scale the matrix by [x, y].
        /// </summary>
        Scale = 3,
        /// <summary>
        /// Skew the matrix by [x, y].
        /// </summary>
        Skew = 4,
        /// <summary>
        /// Rotate the matrix by the given angle about [0, 0].
        /// </summary>
        Rotate = 5,
        /// <summary>
        /// Rotate the matrix by the given angle about [x, y].
        /// </summary>
        RotatePt = 6,
        /// <summary>
        /// Transform this matrix by other `BLMatrix2D`.
        /// </summary>
        Transform = 7,

        /// <summary>
        /// Post-translate the matrix by [x, y].
        /// </summary>
        PostTranslate = 8,
        /// <summary>
        /// Post-scale the matrix by [x, y].
        /// </summary>
        PostScale = 9,
        /// <summary>
        /// Post-skew the matrix by [x, y].
        /// </summary>
        PostSkew = 10,
        /// <summary>
        /// Post-rotate the matrix about [0, 0].
        /// </summary>
        PostRotate = 11,
        /// <summary>
        /// Post-rotate the matrix about a reference BLPoint.
        /// </summary>
        PostRotatePt = 12,
        /// <summary>
        /// Post-transform this matrix by other `BLMatrix2D`.
        /// </summary>
        PostTransform = 13
    }
}

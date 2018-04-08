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

// This source code is partially based on SharpDX's Matrix3x2.cs & MathUtils.cs implementations, the license of which is stated bellow:


// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace PixCore.Geometry
{
    /// <summary>
    /// Plain Matrix3x2.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public readonly struct Matrix2D
    {
        private static readonly Matrix2D _identity = new Matrix2D(1, 0, 0, 1, 0, 0);

        /// <summary>
        /// Gets the identity matrix.
        /// </summary>
        /// <value>The identity matrix.</value>
        public static ref readonly Matrix2D Identity => ref _identity;

        /// <summary>
        /// Element (1,1)
        /// </summary>
        public readonly float M11;

        /// <summary>
        /// Element (1,2)
        /// </summary>
        public readonly float M12;

        /// <summary>
        /// Element (2,1)
        /// </summary>
        public readonly float M21;

        /// <summary>
        /// Element (2,2)
        /// </summary>
        public readonly float M22;

        /// <summary>
        /// Element (3,1)
        /// </summary>
        public readonly float M31;

        /// <summary>
        /// Element (3,2)
        /// </summary>
        public readonly float M32;

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix2D"/> struct.
        /// </summary>
        /// <param name="value">The value that will be assigned to all components.</param>
        public Matrix2D(float value)
        {
            M11 = M12 = 
            M21 = M22 = 
            M31 = M32 = value; 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix2D"/> struct.
        /// </summary>
        /// <param name="m11">The value to assign at row 1 column 1 of the matrix.</param>
        /// <param name="m12">The value to assign at row 1 column 2 of the matrix.</param>
        /// <param name="m21">The value to assign at row 2 column 1 of the matrix.</param>
        /// <param name="m22">The value to assign at row 2 column 2 of the matrix.</param>
        /// <param name="m31">The value to assign at row 3 column 1 of the matrix.</param>
        /// <param name="m32">The value to assign at row 3 column 2 of the matrix.</param>
        public Matrix2D(float m11, float m12, float m21, float m22, float m31, float m32)
        {
            M11 = m11; M12 = m12;
            M21 = m21; M22 = m22;
            M31 = m31; M32 = m32;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix2D"/> struct.
        /// </summary>
        /// <param name="values">The values to assign to the components of the matrix. This must be an array with six elements.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="values"/> contains more or less than six elements.</exception>
        public Matrix2D([NotNull] float[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));
            if (values.Length != 6)
                throw new ArgumentOutOfRangeException(nameof(values), @"There must be six input values for Matrix3x2.");

            M11 = values[0];
            M12 = values[1];

            M21 = values[2];
            M22 = values[3];

            M31 = values[4];
            M32 = values[5];
        }

        /// <summary>
        /// Gets or sets the first row in the matrix; that is M11 and M12.
        /// </summary>
        public Vector Row1 => new Vector(M11, M12);

        /// <summary>
        /// Gets or sets the second row in the matrix; that is M21 and M22.
        /// </summary>
        public Vector Row2 => new Vector(M21, M22);

        /// <summary>
        /// Gets or sets the third row in the matrix; that is M31 and M32.
        /// </summary>
        public Vector Row3 => new Vector(M31, M32);

        /// <summary>
        /// Gets or sets the first column in the matrix; that is M11, M21, and M31.
        /// </summary>
        public float[] Column1 => new[] {M11, M21, M31};

        /// <summary>
        /// Gets or sets the second column in the matrix; that is M12, M22, and M32.
        /// </summary>
        public float[] Column2 => new[] {M12, M22, M32};

        /// <summary>
        /// Gets or sets the translation of the matrix; that is M31 and M32.
        /// </summary>
        public Vector TranslationVector => new Vector(M31, M32);

        /// <summary>
        /// Gets or sets the scale of the matrix; that is M11 and M22.
        /// </summary>
        public Vector ScaleVector => new Vector(M11, M22);

        /// <summary>
        /// Gets a value indicating whether this instance is an identity matrix.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an identity matrix; otherwise, <c>false</c>.
        /// </value>
        public bool IsIdentity => Equals(Identity);

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="index">The zero-based index of the component to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="index"/> is out of the range [0, 5].</exception>
        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return M11;
                    case 1: return M12;
                    case 2: return M21;
                    case 3: return M22;
                    case 4: return M31;
                    case 5: return M32;
                    default: throw new ArgumentOutOfRangeException(nameof(index), @"Indices for Matrix3x2 run from 0 to 5, inclusive.");
                }
            }
        }

        /// <summary>
        /// Gets or sets the component at the specified index.
        /// </summary>
        /// <value>The value of the matrix component, depending on the index.</value>
        /// <param name="row">The row of the matrix to access.</param>
        /// <param name="column">The column of the matrix to access.</param>
        /// <returns>The value of the component at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="row"/> or <paramref name="column"/>is out of the range [0, 3].</exception>
        public float this[int row, int column]
        {
            get
            {
                if (row < 0 || row > 2)
                    throw new ArgumentOutOfRangeException(nameof(row), @"Rows and columns for matrices run from 0 to 2, inclusive.");
                if (column < 0 || column > 1)
                    throw new ArgumentOutOfRangeException(nameof(column), @"Rows and columns for matrices run from 0 to 1, inclusive.");

                return this[row * 2 + column];
            }
        }

        /// <summary>
        /// Creates an array containing the elements of the matrix.
        /// </summary>
        /// <returns>A sixteen-element array containing the components of the matrix.</returns>
        public float[] ToArray()
        {
            return new[] { M11, M12, M21, M22, M31, M32 };
        }

        /// <summary>
        /// Determines the sum of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <param name="result">When the method completes, contains the sum of the two matrices.</param>
        public static void Add(in Matrix2D left, in Matrix2D right, out Matrix2D result)
        {
            float m11 = left.M11 + right.M11;
            float m12 = left.M12 + right.M12;
            float m21 = left.M21 + right.M21;
            float m22 = left.M22 + right.M22;
            float m31 = left.M31 + right.M31;
            float m32 = left.M32 + right.M32;

            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Determines the sum of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <returns>The sum of the two matrices.</returns>
        public static Matrix2D Add(in Matrix2D left, in Matrix2D right)
        {
            Add(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Determines the difference between two matrices.
        /// </summary>
        /// <param name="left">The first matrix to subtract.</param>
        /// <param name="right">The second matrix to subtract.</param>
        /// <param name="result">When the method completes, contains the difference between the two matrices.</param>
        public static void Subtract(in Matrix2D left, in Matrix2D right, out Matrix2D result)
        {
            float m11 = left.M11 - right.M11;
            float m12 = left.M12 - right.M12;
            float m21 = left.M21 - right.M21;
            float m22 = left.M22 - right.M22;
            float m31 = left.M31 - right.M31;
            float m32 = left.M32 - right.M32;
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Determines the difference between two matrices.
        /// </summary>
        /// <param name="left">The first matrix to subtract.</param>
        /// <param name="right">The second matrix to subtract.</param>
        /// <returns>The difference between the two matrices.</returns>
        public static Matrix2D Subtract(in Matrix2D left, in Matrix2D right)
        {
            Subtract(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled matrix.</param>
        public static void Multiply(in Matrix2D left, float right, out Matrix2D result)
        {
            float m11 = left.M11 * right;
            float m12 = left.M12 * right;
            float m21 = left.M21 * right;
            float m22 = left.M22 * right;
            float m31 = left.M31 * right;
            float m32 = left.M32 * right;
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2D Multiply(in Matrix2D left, float right)
        {
            Multiply(in left, right, out var result);
            return result;
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <param name="result">The product of the two matrices.</param>
        public static void Multiply(in Matrix2D left, in Matrix2D right, out Matrix2D result)
        {
            float m11 = left.M11 * right.M11 + left.M12 * right.M21;
            float m12 = left.M11 * right.M12 + left.M12 * right.M22;
            float m21 = left.M21 * right.M11 + left.M22 * right.M21;
            float m22 = left.M21 * right.M12 + left.M22 * right.M22;
            float m31 = left.M31 * right.M11 + left.M32 * right.M21 + right.M31;
            float m32 = left.M31 * right.M12 + left.M32 * right.M22 + right.M32;
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Determines the product of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix2D Multiply(in Matrix2D left, in Matrix2D right)
        {
            Multiply(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by the given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <param name="result">When the method completes, contains the scaled matrix.</param>
        public static void Divide(in Matrix2D left, float right, out Matrix2D result)
        {
            float inv = 1.0f / right;

            float m11 = left.M11 * inv;
            float m12 = left.M12 * inv;
            float m21 = left.M21 * inv;
            float m22 = left.M22 * inv;
            float m31 = left.M31 * inv;
            float m32 = left.M32 * inv;
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Determines the quotient of two matrices.
        /// </summary>
        /// <param name="left">The first matrix to divide.</param>
        /// <param name="right">The second matrix to divide.</param>
        /// <param name="result">When the method completes, contains the quotient of the two matrices.</param>
        public static void Divide(in Matrix2D left, in Matrix2D right, out Matrix2D result)
        {
            float m11 = left.M11 / right.M11;
            float m12 = left.M12 / right.M12;
            float m21 = left.M21 / right.M21;
            float m22 = left.M22 / right.M22;
            float m31 = left.M31 / right.M31;
            float m32 = left.M32 / right.M32;
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <param name="result">When the method completes, contains the negated matrix.</param>
        public static void Negate(in Matrix2D value, out Matrix2D result)
        {
            float m11 = -value.M11;
            float m12 = -value.M12;
            float m21 = -value.M21;
            float m22 = -value.M22;
            float m31 = -value.M31;
            float m32 = -value.M32;
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The matrix to be negated.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix2D Negate(in Matrix2D value)
        {
            Negate(in value, out var result);
            return result;
        }

        /// <summary>
        /// Performs a linear interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the linear interpolation of the two matrices.</param>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static void Lerp(in Matrix2D start, in Matrix2D end, float amount, out Matrix2D result)
        {
            float m11 = Lerp(start.M11, end.M11, amount);
            float m12 = Lerp(start.M12, end.M12, amount);
            float m21 = Lerp(start.M21, end.M21, amount);
            float m22 = Lerp(start.M22, end.M22, amount);
            float m31 = Lerp(start.M31, end.M31, amount);
            float m32 = Lerp(start.M32, end.M32, amount);
            
            result = new Matrix2D(m11, m12, m21, m22, m31, m32);
        }

        /// <summary>
        /// Performs a linear interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The linear interpolation of the two matrices.</returns>
        /// <remarks>
        /// Passing <paramref name="amount"/> a value of 0 will cause <paramref name="start"/> to be returned; a value of 1 will cause <paramref name="end"/> to be returned. 
        /// </remarks>
        public static Matrix2D Lerp(in Matrix2D start, in Matrix2D end, float amount)
        {
            Lerp(in start, in end, amount, out var result);
            return result;
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <param name="result">When the method completes, contains the cubic interpolation of the two matrices.</param>
        public static void SmoothStep(in Matrix2D start, in Matrix2D end, float amount, out Matrix2D result)
        {
            amount = SmoothStep(amount);
            Lerp(in start, in end, amount, out result);
        }

        /// <summary>
        /// Performs a cubic interpolation between two matrices.
        /// </summary>
        /// <param name="start">Start matrix.</param>
        /// <param name="end">End matrix.</param>
        /// <param name="amount">Value between 0 and 1 indicating the weight of <paramref name="end"/>.</param>
        /// <returns>The cubic interpolation of the two matrices.</returns>
        public static Matrix2D SmoothStep(in Matrix2D start, in Matrix2D end, float amount)
        {
            SmoothStep(in start, in end, amount, out var result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for both axes.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(in Vector scale, out Matrix2D result)
        {
            Scaling(scale.X, scale.Y, out result);
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis and y-axis.
        /// </summary>
        /// <param name="scale">Scaling factor for both axes.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix2D Scaling(in Vector scale)
        {
            Scaling(in scale, out var result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(float x, float y, out Matrix2D result)
        {
            result = new Matrix2D(x, Identity.M12, Identity.M21, y, Identity.M31, Identity.M32);
        }

        /// <summary>
        /// Creates a matrix that scales along the x-axis and y-axis.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix2D Scaling(float x, float y)
        {
            Scaling(x, y, out var result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that uniformly scales along both axes.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along both axes.</param>
        /// <param name="result">When the method completes, contains the created scaling matrix.</param>
        public static void Scaling(float scale, out Matrix2D result)
        {
            result = new Matrix2D(scale, Identity.M12, Identity.M21, scale, Identity.M31, Identity.M32);
        }

        /// <summary>
        /// Creates a matrix that uniformly scales along both axes.
        /// </summary>
        /// <param name="scale">The uniform scale that is applied along both axes.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix2D Scaling(float scale)
        {
            Scaling(scale, out var result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that is scaling from a specified center.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="center">The center of the scaling.</param>
        /// <returns>The created scaling matrix.</returns>
        public static Matrix2D Scaling(float x, float y, in Vector center)
        {
            return new Matrix2D(x, 0, 0, y, center.X - x * center.X, center.Y - y * center.Y);
        }

        /// <summary>
        /// Creates a matrix that is scaling from a specified center.
        /// </summary>
        /// <param name="x">Scaling factor that is applied along the x-axis.</param>
        /// <param name="y">Scaling factor that is applied along the y-axis.</param>
        /// <param name="center">The center of the scaling.</param>
        /// <param name="result">The created scaling matrix.</param>
        public static void Scaling(float x, float y, in Vector center, out Matrix2D result)
        {
            result = new Matrix2D(x, 0, 0, y, center.X - x * center.X, center.Y - y * center.Y);
        }

        /// <summary>
        /// Calculates the determinant of this matrix.
        /// </summary>
        /// <returns>Result of the determinant.</returns>
        public float Determinant()
        {
            return M11 * M22 - M12 * M21;
        }

        /// <summary>
        /// Creates a matrix that rotates.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void Rotation(float angle, out Matrix2D result)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            
            result = new Matrix2D(cos, sin, -sin, cos, Identity.M31, Identity.M32);
        }

        /// <summary>
        /// Creates a matrix that rotates.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix2D Rotation(float angle)
        {
            Rotation(angle, out var result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates about a specified center.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
        /// <param name="center">The center of the rotation.</param>
        /// <returns>The created rotation matrix.</returns>
        public static Matrix2D Rotation(float angle, in Vector center)
        {
            Rotation(angle, center, out var result);
            return result;
        }

        /// <summary>
        /// Creates a matrix that rotates about a specified center.
        /// </summary>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
        /// <param name="center">The center of the rotation.</param>
        /// <param name="result">When the method completes, contains the created rotation matrix.</param>
        public static void Rotation(float angle, Vector center, out Matrix2D result)
        {
            result = Translation(-center) * Rotation(angle) * Translation(center);
        }

        /// <summary>
        /// Creates a transformation matrix.
        /// </summary>
        /// <param name="xScale">Scaling factor that is applied along the x-axis.</param>
        /// <param name="yScale">Scaling factor that is applied along the y-axis.</param>
        /// <param name="angle">Angle of rotation in radians. Angles are measured clockwise when looking along the rotation axis.</param>
        /// <param name="xOffset">X-coordinate offset.</param>
        /// <param name="yOffset">Y-coordinate offset.</param>
        /// <param name="result">When the method completes, contains the created transformation matrix.</param>
        public static void Transformation(float xScale, float yScale, float angle, float xOffset, float yOffset, out Matrix2D result)
        {
            result = Scaling(xScale, yScale) * Rotation(angle) * Translation(xOffset, yOffset);
        }

        /// <summary>
        /// Creates a transformation matrix.
        /// </summary>
        /// <param name="xScale">Scaling factor that is applied along the x-axis.</param>
        /// <param name="yScale">Scaling factor that is applied along the y-axis.</param>
        /// <param name="angle">Angle of rotation in radians.</param>
        /// <param name="xOffset">X-coordinate offset.</param>
        /// <param name="yOffset">Y-coordinate offset.</param>
        /// <returns>The created transformation matrix.</returns>
        public static Matrix2D Transformation(float xScale, float yScale, float angle, float xOffset, float yOffset)
        {
            Transformation(xScale, yScale, angle, xOffset, yOffset, out var result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for both coordinate planes.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void Translation(in Vector value, out Matrix2D result)
        {
            Translation(value.X, value.Y, out result);
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="value">The offset for both coordinate planes.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix2D Translation(in Vector value)
        {
            Translation(in value, out var result);
            return result;
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <param name="result">When the method completes, contains the created translation matrix.</param>
        public static void Translation(float x, float y, out Matrix2D result)
        {
            result = new Matrix2D(Identity.M11, Identity.M12, Identity.M21, Identity.M22, x, y);
        }

        /// <summary>
        /// Creates a translation matrix using the specified offsets.
        /// </summary>
        /// <param name="x">X-coordinate offset.</param>
        /// <param name="y">Y-coordinate offset.</param>
        /// <returns>The created translation matrix.</returns>
        public static Matrix2D Translation(float x, float y)
        {
            Translation(x, y, out var result);
            return result;
        }

        /// <summary>
        /// Transforms a vector by this matrix.
        /// </summary>
        /// <param name="matrix">The matrix to use as a transformation matrix.</param>
        /// <param name="point">The original vector to apply the transformation.</param>
        /// <returns>The result of the transformation for the input vector.</returns>
        public static Vector TransformPoint(Matrix2D matrix, in Vector point)
        {
            float x = point.X * matrix.M11 + point.Y * matrix.M21 + matrix.M31;
            float y = point.X * matrix.M12 + point.Y * matrix.M22 + matrix.M32;
            return new Vector(x, y);
        }

        /// <summary>
        /// Transforms a vector by this matrix.
        /// </summary>
        /// <param name="matrix">The matrix to use as a transformation matrix.</param>
        /// <param name="point">The original vector to apply the transformation.</param>
        /// <param name="result">The result of the transformation for the input vector.</param>
        /// <returns></returns>
        public static void TransformPoint(ref Matrix2D matrix, in Vector point, out Vector result)
        {
            float x = point.X * matrix.M11 + point.Y * matrix.M21 + matrix.M31;
            float y = point.X * matrix.M12 + point.Y * matrix.M22 + matrix.M32;
            result = new Vector(x, y);
        }

        /// <summary>
        /// Calculates the inverse of this matrix instance.
        /// </summary>
        public Matrix2D Inverted()
        {
            Invert(in this, out var matrix);
            return matrix;
        }

        /// <summary>
        /// Calculates the inverse of the specified matrix.
        /// </summary>
        /// <param name="value">The matrix whose inverse is to be calculated.</param>
        /// <returns>the inverse of the specified matrix.</returns>
        public static Matrix2D Invert(Matrix2D value)
        {
            Invert(in value, out var result);
            return result;
        }

        /// <summary>
        /// Creates a skew matrix.
        /// </summary>
        /// <param name="angleX">Angle of skew along the X-axis in radians.</param>
        /// <param name="angleY">Angle of skew along the Y-axis in radians.</param>
        /// <returns>The created skew matrix.</returns>
        public static Matrix2D Skew(float angleX, float angleY)
        {
            Skew(angleX, angleY, out var result);
            return result;
        }

        /// <summary>
        /// Creates a skew matrix.
        /// </summary>
        /// <param name="angleX">Angle of skew along the X-axis in radians.</param>
        /// <param name="angleY">Angle of skew along the Y-axis in radians.</param>
        /// <param name="result">When the method completes, contains the created skew matrix.</param>
        public static void Skew(float angleX, float angleY, out Matrix2D result)
        {
            result = new Matrix2D(Identity.M11, (float) Math.Tan(angleX), (float) Math.Tan(angleY), Identity.M22, Identity.M31, Identity.M32);
        }

        /// <summary>
        /// Calculates the inverse of the specified matrix.
        /// </summary>
        /// <param name="value">The matrix whose inverse is to be calculated.</param>
        /// <param name="result">When the method completes, contains the inverse of the specified matrix.</param>
        public static void Invert(in Matrix2D value, out Matrix2D result)
        {
            float determinant = value.Determinant();

            if (IsZero(determinant))
            {
                result = Identity;
                return;
            }

            float invdet = 1.0f / determinant;
            float offsetX = value.M31;
            float offsetY = value.M32;

            result = new Matrix2D(
                value.M22 * invdet,
                -value.M12 * invdet,
                -value.M21 * invdet,
                value.M11 * invdet,
                (value.M21 * offsetY - offsetX * value.M22) * invdet,
                (offsetX * value.M12 - value.M11 * offsetY) * invdet);
        }

        /// <summary>
        /// Adds two matrices.
        /// </summary>
        /// <param name="left">The first matrix to add.</param>
        /// <param name="right">The second matrix to add.</param>
        /// <returns>The sum of the two matrices.</returns>
        public static Matrix2D operator +(Matrix2D left, Matrix2D right)
        {
            Add(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Assert a matrix (return it unchanged).
        /// </summary>
        /// <param name="value">The matrix to assert (unchanged).</param>
        /// <returns>The asserted (unchanged) matrix.</returns>
        public static Matrix2D operator +(Matrix2D value)
        {
            return value;
        }

        /// <summary>
        /// Subtracts two matrices.
        /// </summary>
        /// <param name="left">The first matrix to subtract.</param>
        /// <param name="right">The second matrix to subtract.</param>
        /// <returns>The difference between the two matrices.</returns>
        public static Matrix2D operator -(Matrix2D left, Matrix2D right)
        {
            Subtract(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Negates a matrix.
        /// </summary>
        /// <param name="value">The matrix to negate.</param>
        /// <returns>The negated matrix.</returns>
        public static Matrix2D operator -(Matrix2D value)
        {
            Negate(in value, out var result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by a given value.
        /// </summary>
        /// <param name="right">The matrix to scale.</param>
        /// <param name="left">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2D operator *(float left, Matrix2D right)
        {
            Multiply(in right, left, out var result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by a given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2D operator *(Matrix2D left, float right)
        {
            Multiply(in left, right, out var result);
            return result;
        }

        /// <summary>
        /// Multiplies two matrices.
        /// </summary>
        /// <param name="left">The first matrix to multiply.</param>
        /// <param name="right">The second matrix to multiply.</param>
        /// <returns>The product of the two matrices.</returns>
        public static Matrix2D operator *(Matrix2D left, Matrix2D right)
        {
            Multiply(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Scales a matrix by a given value.
        /// </summary>
        /// <param name="left">The matrix to scale.</param>
        /// <param name="right">The amount by which to scale.</param>
        /// <returns>The scaled matrix.</returns>
        public static Matrix2D operator /(Matrix2D left, float right)
        {
            Divide(in left, right, out var result);
            return result;
        }

        /// <summary>
        /// Divides two matrices.
        /// </summary>
        /// <param name="left">The first matrix to divide.</param>
        /// <param name="right">The second matrix to divide.</param>
        /// <returns>The quotient of the two matrices.</returns>
        public static Matrix2D operator /(Matrix2D left, Matrix2D right)
        {
            Divide(in left, in right, out var result);
            return result;
        }

        /// <summary>
        /// Tests for equality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has the same value as <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Matrix2D left, Matrix2D right)
        {
            return left.Equals(ref right);
        }

        /// <summary>
        /// Tests for inequality between two objects.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns><c>true</c> if <paramref name="left"/> has a different value than <paramref name="right"/>; otherwise, <c>false</c>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Matrix2D left, Matrix2D right)
        {
            return !left.Equals(ref right);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]",
                M11, M12, M21, M22, M31, M32);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            if (format == null)
                return ToString();

            return string.Format(format, CultureInfo.CurrentCulture, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]",
                M11.ToString(format, CultureInfo.CurrentCulture), M12.ToString(format, CultureInfo.CurrentCulture),
                M21.ToString(format, CultureInfo.CurrentCulture), M22.ToString(format, CultureInfo.CurrentCulture),
                M31.ToString(format, CultureInfo.CurrentCulture), M32.ToString(format, CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(IFormatProvider formatProvider)
        {
            return string.Format(formatProvider, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]",
                M11.ToString(formatProvider), M12.ToString(formatProvider),
                M21.ToString(formatProvider), M22.ToString(formatProvider),
                M31.ToString(formatProvider), M32.ToString(formatProvider));
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (format == null)
                return ToString(formatProvider);

            return string.Format(format, formatProvider, "[M11:{0} M12:{1}] [M21:{2} M22:{3}] [M31:{4} M32:{5}]",
                M11.ToString(format, formatProvider), M12.ToString(format, formatProvider),
                M21.ToString(format, formatProvider), M22.ToString(format, formatProvider),
                M31.ToString(format, formatProvider), M32.ToString(format, formatProvider));
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = M11.GetHashCode();
                hashCode = (hashCode * 397) ^ M12.GetHashCode();
                hashCode = (hashCode * 397) ^ M21.GetHashCode();
                hashCode = (hashCode * 397) ^ M22.GetHashCode();
                hashCode = (hashCode * 397) ^ M31.GetHashCode();
                hashCode = (hashCode * 397) ^ M32.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix2D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix2D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix2D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ref Matrix2D other)
        {
            return NearEqual(other.M11, M11) &&
                   NearEqual(other.M12, M12) &&
                   NearEqual(other.M21, M21) &&
                   NearEqual(other.M22, M22) &&
                   NearEqual(other.M31, M31) &&
                   NearEqual(other.M32, M32);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Matrix2D"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="Matrix2D"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Matrix2D"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Matrix2D other)
        {
            return Equals(ref other);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (!(value is Matrix2D matrix))
                return false;
            
            return Equals(ref matrix);
        }

        /// <summary>
        /// Checks if a and b are almost equals, taking into account the magnitude of floating point numbers. See Remarks.
        /// See remarks.
        /// </summary>
        /// <param name="a">The left value to compare.</param>
        /// <param name="b">The right value to compare.</param>
        /// <returns><c>true</c> if a almost equal to b, <c>false</c> otherwise</returns>
        /// <remarks>
        /// The code is using the technique described by Bruce Dawson in 
        /// <a href="http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/">Comparing Floating point numbers 2012 edition</a>. 
        /// </remarks>
        private static unsafe bool NearEqual(float a, float b)
        {
            // Check if the numbers are really close -- needed
            // when comparing numbers near zero.
            if (IsZero(a - b))
                return true;

            // Original from Bruce Dawson: http://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/
            int aInt = *(int*)&a;
            int bInt = *(int*)&b;

            // Different signs means they do not match.
            if (aInt < 0 != bInt < 0)
                return false;

            // Find the difference in ULPs.
            int ulp = Math.Abs(aInt - bInt);

            // Choose of maxUlp = 4
            // according to http://code.google.com/p/googletest/source/browse/trunk/include/gtest/internal/gtest-internal.h
            const int maxUlp = 4;
            return ulp <= maxUlp;
        }

        /// <summary>
        /// Interpolates between two values using a linear function by a given amount.
        /// </summary>
        /// <remarks>
        /// See http://www.encyclopediaofmath.org/index.php/Linear_interpolation and
        /// http://fgiesen.wordpress.com/2012/08/15/linear-interpolation-past-present-and-future/
        /// </remarks>
        /// <param name="from">Value to interpolate from.</param>
        /// <param name="to">Value to interpolate to.</param>
        /// <param name="amount">Interpolation amount.</param>
        /// <returns>The result of linear interpolation of values based on the amount.</returns>
        private static float Lerp(float from, float to, float amount)
        {
            return (1 - amount) * from + amount * to;
        }

        /// <summary>
        /// Performs smooth (cubic Hermite) interpolation between 0 and 1.
        /// </summary>
        /// <remarks>
        /// See https://en.wikipedia.org/wiki/Smoothstep
        /// </remarks>
        /// <param name="amount">Value between 0 and 1 indicating interpolation amount.</param>
        private static float SmoothStep(float amount)
        {
            return amount <= 0 ? 0
                : amount >= 1 ? 1
                : amount * amount * (3 - 2 * amount);
        }

        /// <summary>
        /// Determines whether the specified value is close to zero (0.0f).
        /// </summary>
        /// <param name="a">The floating value.</param>
        /// <returns><c>true</c> if the specified value is close to zero (0.0f); otherwise, <c>false</c>.</returns>
        private static bool IsZero(float a)
        {
            const float zeroTolerance = 1e-6f;

            return Math.Abs(a) < zeroTolerance;
        }
    }
}

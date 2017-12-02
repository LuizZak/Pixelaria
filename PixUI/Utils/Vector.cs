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
using System.Drawing;
using JetBrains.Annotations;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Point = System.Drawing.Point;

namespace PixUI.Utils
{
    /// <summary>
    /// Represents a 2D vector with X and Y components.
    /// </summary>
    [DebuggerDisplay("{X}, {Y}")]
    public struct Vector : IEquatable<Vector>, IComparable<Vector>
    {
        /// <summary>
        /// A Zero vector (0, 0)
        /// </summary>
        public static readonly Vector Zero = new Vector(0, 0);

        /// <summary>
        /// A Unit vector (1, 1)
        /// </summary>
        public static readonly Vector Unit = new Vector(1, 1);

        /// <summary>
        /// X coordinate
        /// </summary>
        public readonly float X;
        /// <summary>
        /// Y coordinate
        /// </summary>
        public readonly float Y;

        /// <summary>
        /// Creates a vector w/ both axis set to <see cref="value"/>
        /// </summary>
        public Vector(float value)
        {
            X = value;
            Y = value;
        }

        public Vector(float px, float py)
        {
            X = px;
            Y = py;
        }

        public Vector(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public Vector(PointF point)
        {
            X = point.X;
            Y = point.Y;
        }

        [Pure]
        public float Dot(Vector v)
        {
            return X * v.X + Y * v.Y;
        }

        [Pure]
        public float Cross(Vector v)
        {
            return X * v.Y - Y * v.X;
        }

        [Pure]
        public float Length()
        {
            return X * X + Y * Y;
        }

        [Pure]
        public float Magnitude()
        {
            return (float)Math.Sqrt(Length());
        }

        [Pure]
        public float Distance(Vector v)
        {
            return (this - v).Magnitude();
        }
        [Pure]
        public float DistanceSquared(Vector v)
        {
            return (this - v).Length();
        }

        [Pure]
        public Vector Normalized()
        {
            float m = Magnitude();
            if (Math.Abs(m) < float.Epsilon)
            {
                m = 0.0001f;
            }
            return this * (1.0f / m);
        }

        public void Normalize()
        {
            this = Normalized();
        }

        public void Negate()
        {
            this = -this;
        }

        /// <summary>
        /// Returns a copy of this Vector instance with the X and Y
        /// coordinates clamped to be within the min and max limits
        /// of a given AABB.
        /// </summary>
        public Vector LimitedWithin(AABB aabb)
        {
            return Max(aabb.Minimum, Min(aabb.Maximum, this));
        }
        
        public override string ToString()
        {
            return X + " : " + Y;
        }

        /// <summary>
        /// Returns a vector with the smallest x and y coordinate values across
        /// the two given vectors
        /// </summary>
        [Pure]
        public static Vector Min(Vector lhs, Vector rhs)
        {
            return new Vector(Math.Min(lhs.X, rhs.X), Math.Min(lhs.Y, rhs.Y));
        }

        /// <summary>
        /// Returns a vector with the largest x and y coordinate values across
        /// the two given vectors
        /// </summary>
        [Pure]
        public static Vector Max(Vector lhs, Vector rhs)
        {
            return new Vector(Math.Max(lhs.X, rhs.X), Math.Max(lhs.Y, rhs.Y));
        }

        /// <summary>
        /// Returns a vector with the x and y coordinates floored to the
        /// nearest integer number.
        /// </summary>
        [Pure]
        public static Vector Floor(Vector vec)
        {
            return new Vector((float)Math.Floor(vec.X), (float)Math.Floor(vec.Y));
        }

        /// <summary>
        /// Returns a vector with the x and y coordinates rounded to the
        /// nearest integer number.
        /// </summary>
        [Pure]
        public static Vector Round(Vector vec)
        {
            return new Vector((float)Math.Round(vec.X), (float)Math.Round(vec.Y));
        }

        /// <summary>
        /// Returns a vector with the x and y coordinates rounded to the
        /// next largest integer number.
        /// </summary>
        [Pure]
        public static Vector Ceiling(Vector vec)
        {
            return new Vector((float)Math.Ceiling(vec.X), (float)Math.Ceiling(vec.Y));
        }

        /// <summary>
        /// Returns a vector with absoluite x and y coordinates 
        /// </summary>
        [Pure]
        public static Vector Abs(Vector vec)
        {
            return new Vector(Math.Abs(vec.X), Math.Abs(vec.Y));
        }

        /// <summary>
        /// Performs a linear interpolation between <see cref="start"/> and <see cref="end"/>
        /// with a specified factor.
        /// </summary>
        /// <param name="start">Start of linear interpolation</param>
        /// <param name="end">End of linear interpolation</param>
        /// <param name="factor">A factor, usually between zero and one, that controls where the final interpolad point lands</param>
        public static Vector Lerp(Vector start, Vector end, float factor)
        {
            return start + (end - start) * factor;
        }

        public static bool operator <(Vector lhs, Vector rhs)
        {
            return lhs.X < rhs.X && lhs.Y < rhs.Y;
        }

        public static bool operator >(Vector lhs, Vector rhs)
        {
            return lhs.X > rhs.X && lhs.Y > rhs.Y;
        }

        public static bool operator >=(Vector lhs, Vector rhs)
        {
            return lhs.X >= rhs.X && lhs.Y >= rhs.Y;
        }

        public static bool operator <=(Vector lhs, Vector rhs)
        {
            return lhs.X <= rhs.X && lhs.Y <= rhs.Y;
        }

        public static bool operator ==(Vector lhs, Vector rhs)
        {
            return Math.Abs(lhs.X - rhs.X) < float.Epsilon && Math.Abs(lhs.Y - rhs.Y) < float.Epsilon;
        }

        public static bool operator !=(Vector lhs, Vector rhs)
        {
            return !(lhs == rhs);
        }
        
        public static Vector operator *([NotNull] Matrix m, Vector vec)
        {
            return m.Transform(vec);
        }

        public static Vector operator *(Vector vec, [NotNull] Matrix m)
        {
            return m.Transform(vec);
        }

        public static Vector operator +(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X + vec2.X, vec1.Y + vec2.Y);
        }

        public static Vector operator -(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X - vec2.X, vec1.Y - vec2.Y);
        }

        public static Vector operator *(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X * vec2.X, vec1.Y * vec2.Y);
        }

        public static Vector operator /(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X / vec2.X, vec1.Y / vec2.Y);
        }

        public static Vector operator %(Vector vec1, Vector vec2)
        {
            return new Vector(vec1.X % vec2.X, vec1.Y % vec2.Y);
        }

        public static Vector operator -(Vector vec)
        {
            return new Vector(-vec.X, -vec.Y);
        }

        public static Vector operator *(Vector vec, float factor)
        {
            return new Vector(vec.X * factor, vec.Y * factor);
        }

        public static Vector operator /(Vector vec, float factor)
        {
            return new Vector(vec.X / factor, vec.Y / factor);
        }

        public static Vector operator /(float factor, Vector vec)
        {
            return new Vector(factor / vec.X, factor / vec.Y);
        }

        public static implicit operator PointF(Vector vec)
        {
            return new PointF(vec.X, vec.Y);
        }

        public static implicit operator Vector(Size size)
        {
            return new Vector(size.Width, size.Height);
        }

        public static implicit operator Vector(SizeF size)
        {
            return new Vector(size.Width, size.Height);
        }

        public static implicit operator Vector(PointF vec)
        {
            return new Vector(vec.X, vec.Y);
        }

        public static implicit operator Vector(Point vec)
        {
            return new Vector(vec.X, vec.Y);
        }

        public static implicit operator RawVector2(Vector vec)
        {
            return new RawVector2(vec.X, vec.Y);
        }

        public static implicit operator Vector2(Vector vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        /*
         * TODO: This one seems like a bit of a stretch. Maybe it's ok?
         * 
        public static implicit operator Vector((float x, float y) vec)
        {
            return new Vector(vec.x, vec.y);
        }
        */

        public int CompareTo(Vector other)
        {
            int xComparison = X.CompareTo(other.X);
            return xComparison != 0 ? xComparison : Y.CompareTo(other.Y);
        }

        public bool Equals(Vector other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vector && Equals((Vector)obj);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }
    }

    public static class VectorExtensions
    {
        /// <summary>
        /// Transforms a set of points by multiplying them by a given matrix.
        /// </summary>
        [Pure]
        public static Vector[] Transform([NotNull] this Matrix matrix, [NotNull] Vector[] elements)
        {
            var result = new Vector[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                result[i] = matrix * elements[i];
            }
            return result;
        }

        /// <summary>
        /// Transforms a set of points by multiplying them by a given matrix.
        /// </summary>
        [Pure]
        public static Vector[] Transform([NotNull] this Vector[] elements, [NotNull] Matrix matrix)
        {
            var result = new Vector[elements.Length];
            for (int i = 0; i < elements.Length; i++)
            {
                result[i] = matrix * elements[i];
            }
            return result;
        }

        /// <summary>
        /// Gets the minimum area capable of containing a set of points
        /// </summary>
        [Pure]
        public static AABB Area([NotNull] this Vector[] elements)
        {
            return new AABB(elements);
        }
    }
}

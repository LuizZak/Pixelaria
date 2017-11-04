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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using JetBrains.Annotations;
using SharpDX.Mathematics.Interop;

namespace Pixelaria.Utils
{
    /// <summary>
    /// An axis-aligned bounding box.
    /// </summary>
    [DebuggerDisplay("Minimum: {Minimum}, Maximum: {Maximum}")]
    // ReSharper disable once InconsistentNaming
    public struct AABB : IEquatable<AABB>
    {
        /// <summary>
        /// Gets an AABB that has zero bounds area.
        /// 
        /// AABB has minimum and maximum == Vector.Zero, and is Valid.
        /// </summary>
        public static readonly AABB Empty = new AABB(Vector.Zero, Vector.Zero);

        public readonly Vector Minimum;
        public readonly Vector Maximum;
        public readonly State Validity;

        public float Width => Maximum.X - Minimum.X;
        public float Height => Maximum.Y - Minimum.Y;

        public Vector Size => new Vector(Width, Height);

        public Vector Center => (Minimum + Maximum) / 2;

        /// <summary>
        /// Returns the area of this AABB by multiplying its width by its height
        /// </summary>
        public float Area => Size.X * Size.Y;

        /// <summary>
        /// Alias for Minimum.Y
        /// </summary>
        public float Top => Minimum.Y;

        /// <summary>
        /// Alias for Minimum.X
        /// </summary>
        public float Left => Minimum.X;

        /// <summary>
        /// Alias for Maximum.X
        /// </summary>
        public float Right => Maximum.X;

        /// <summary>
        /// Alias for Maximum.Y
        /// </summary>
        public float Bottom => Maximum.Y;

        /// <summary>
        /// Returns an array of vectors that represent this AABB's corners in clockwise
        /// order, starting from the top-left corner.
        /// 
        /// Always contains 4 elements.
        /// 
        /// If this AABB is Invalid, returns an array of (0, 0) values.
        /// </summary>
        public Vector[] Corners
        {
            get
            {
                if (Validity != State.Valid)
                    return new[] {Vector.Zero, Vector.Zero, Vector.Zero, Vector.Zero};

                return new[]
                {
                    Minimum, new Vector(Maximum.X, Minimum.Y), Maximum, new Vector(Minimum.X, Maximum.Y)
                };
            }
        }

        public AABB(float left, float top, float bottom, float right)
            : this(new Vector(left, top), new Vector(right, bottom))
        {

        }

        public AABB(Vector minimum, Vector maximum)
        {
            Minimum = Vector.Min(minimum, maximum);
            Maximum = Vector.Max(minimum, maximum);
            Validity = State.Valid;
        }

        public AABB(RectangleF rectangle)
        {
            Minimum = new Vector(rectangle.Left, rectangle.Top);
            Maximum = new Vector(rectangle.Right, rectangle.Bottom);

            Validity = State.Valid;
        }

        /// <summary>
        /// Initializes an AABB with the minimum bounds
        /// capable of fitting the given array of points.
        /// </summary>
        public AABB([NotNull] IEnumerable<Vector> points)
        {
            Minimum = Vector.Zero;
            Maximum = Vector.Zero;

            var isSet = false;
            foreach (var point in points)
            {
                if (isSet)
                {
                    Minimum = Vector.Min(Minimum, point);
                    Maximum = Vector.Max(Maximum, point);
                }
                else
                {
                    Minimum = point;
                    Maximum = point;
                }

                isSet = true;
            }

            Validity = isSet ? State.Valid : State.Invalid;
        }

        /// <summary>
        /// Sets this aabb's minimum and maximum to the given values
        /// </summary>
        public void Set(Vector minimum, Vector maximum)
        {
            this = new AABB(minimum, maximum);
        }

        /// <summary>
        /// Returns an AABB that matches this AABB's top-left location with a new size
        /// </summary>
        [Pure]
        public AABB WithSize(Vector size)
        {
            return new AABB(Minimum, Minimum + size);
        }

        /// <summary>
        /// Returns an AABB that matches this AABB's top-left location with a new size
        /// </summary>
        [Pure]
        public AABB WithSize(float width, float height)
        {
            return WithSize(new Vector(width, height));
        }

        /// <summary>
        /// Returns a copy of this AABB with the minimum and maximum coordinates
        /// offset by a given ammount.
        /// </summary>
        [Pure]
        public AABB OffsetBy(Vector vector)
        {
            if (Validity == State.Invalid)
                return this;

            return new AABB(Minimum + vector, Maximum + vector);
        }

        /// <summary>
        /// Returns a copy of this AABB with the minimum and maximum coordinates
        /// offset by a given ammount.
        /// </summary>
        [Pure]
        public AABB OffsetBy(float x, float y)
        {
            return OffsetBy(new Vector(x, y));
        }

        /// <summary>
        /// Returns a copy of this AABB with the minimum and maximum coordinates
        /// set to a given ammount.
        /// </summary>
        [Pure]
        public AABB OffsetTo(Vector vector)
        {
            if (Validity == State.Invalid)
                return this;

            return new AABB(vector, vector + Size);
        }

        /// <summary>
        /// Returns a copy of this AABB with the minimum and maximum coordinates
        /// set to a given ammount.
        /// </summary>
        [Pure]
        public AABB OffsetTo(float x, float y)
        {
            return OffsetTo(new Vector(x, y));
        }

        /// <summary>
        /// Returns an AABB which is an inflated version of this AABB 
        /// (i.e. bounds are larger by <see cref="size"/>, but center 
        /// remains the same)
        /// </summary>
        [Pure]
        public AABB Inflated(Vector size)
        {
            return new AABB(Minimum - size / 2, Maximum + size / 2);
        }

        /// <summary>
        /// Inflates this AABB
        /// </summary>
        public void Inflate(Vector size)
        {
            this = Inflated(size);
        }

        /// <summary>
        /// Returns an AABB which is an inflated version of this AABB 
        /// (i.e. bounds are larger by (x, y), but center 
        /// remains the same)
        /// </summary>
        [Pure]
        public AABB Inflated(float x, float y)
        {
            return Inflated(new Vector(x, y));
        }

        /// <summary>
        /// Inflates this AABB
        /// </summary>
        public void Inflate(float x, float y)
        {
            this = Inflated(x, y);
        }

        /// <summary>
        /// Expands this AABB to include a given point
        /// </summary>
        public void ExpandToInclude(Vector point)
        {
            this = ExpandedToInclude(point);
        }

        /// <summary>
        /// Returns an AABB which is the minimum AABB that can fit
        /// this AABB and the given point.
        /// </summary>
        [Pure]
        public AABB ExpandedToInclude(Vector point)
        {
            if(Validity == State.Invalid)
                return new AABB(point, point);

            return new AABB(Vector.Min(point, Minimum), Vector.Max(point, Maximum));
        }

        /// <summary>
        /// Returns an AABB which is the minimum AABB that can fit
        /// this AABB and a second AABB.
        /// 
        /// If this AABB is valid and <see cref="aabb"/> is not, does nothing.
        /// If <see cref="aabb"/> is valid and this one is not, returns <see cref="aabb"/>.
        /// 
        /// Does nothing, if this and the other AABB are both invalid.
        /// </summary>
        [Pure]
        public AABB Union(AABB aabb)
        {
            return Union(this, aabb);
        }

        /// <summary>
        /// Returns an AABB which is the minimum AABB that can fit
        /// two given AABBs.
        /// 
        /// If one of the AABBs is valid and the other is not, returns the first valid AABB.
        /// 
        /// Does nothing, if both AABBs are invalid.
        /// </summary>
        public static AABB Union(AABB left, AABB right)
        {
            if (left.Validity == State.Invalid && right.Validity == State.Invalid)
                return left;

            if (left.Validity == State.Invalid)
                return right;
            if (right.Validity == State.Invalid)
                return left;

            return new AABB(Vector.Min(left.Minimum, right.Minimum), Vector.Max(left.Maximum, right.Maximum));
        }

        /// <summary>
        /// Returns whether a given point rests inside the boundaries
        /// of this AABB.
        /// 
        /// Always returns false, if this AABB is Invalid.
        /// </summary>
        public bool Contains(Vector point)
        {
            if (Validity != State.Valid)
                return false;

            return point >= Minimum && point <= Maximum;
        }

        /// <summary>
        /// Returns whether a given AABB rests completely inside the 
        /// boundaries of this AABB
        /// 
        /// Always returns false, if this (or the other) AABB is Invalid.
        /// </summary>
        public bool Contains(AABB other)
        {
            if (Validity != State.Valid || other.Validity != State.Valid)
                return false;

            return other.Minimum >= Minimum && other.Maximum <= Maximum;
        }

        /// <summary>
        /// Returns whether a given AABB intersects this AABB
        /// 
        /// Always returns false, if this (or the other) AABB is Invalid.
        /// </summary>
        public bool Intersects(AABB other)
        {
            if (Validity != State.Valid || other.Validity != State.Valid)
                return false;

            // X overlap check.
            return Minimum.X <= other.Maximum.X &&
                   Maximum.X >= other.Minimum.X &&
                   Minimum.Y <= other.Maximum.Y &&
                   Maximum.Y >= other.Minimum.Y;
        }

        /// <summary>
        /// Applies the given Matrix on all corners of this AABB, returning
        /// a new minimaml AABB capable of containing the transformed points.
        /// </summary>
        [Pure]
        public AABB TransformedBounds([NotNull] Matrix matrix)
        {
            return Corners.Transform(matrix).Area();
        }

        [Pure]
        public AABB Inset(InsetBounds inset)
        {
            return inset.Inset(this);
        }

        public bool Equals(AABB other)
        {
            return Minimum.Equals(other.Minimum) && Maximum.Equals(other.Maximum) && Validity == other.Validity;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AABB && Equals((AABB) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Minimum.GetHashCode();
                hashCode = (hashCode * 397) ^ Maximum.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Validity;
                return hashCode;
            }
        }

        /// <summary>
        /// Returns an AABB that represents a given rectangle using x, y + width, height
        /// </summary>
        public static AABB FromRectangle(float x, float y, float width, float height)
        {
            return new AABB(x, y, y + height, x + width);
        }

        /// <summary>
        /// Returns an AABB that represents a given rectangle using x, y + width, height
        /// </summary>
        public static AABB FromRectangle(Vector position, Vector size)
        {
            return FromRectangle(position.X, position.Y, size.X, size.Y);
        }

        /// <summary>
        /// Returns an AABB with the coordinate values rounded to the nearest integer.
        /// </summary>
        public static AABB Rounded(AABB aabb)
        {
            return new AABB(Vector.Round(aabb.Minimum), Vector.Round(aabb.Maximum));
        }

        public static implicit operator AABB(RectangleF rect)
        {
            return new AABB(rect);
        }

        public static implicit operator AABB(Rectangle rect)
        {
            return new AABB(rect);
        }

        public static explicit operator RectangleF(AABB aabb)
        {
            return new RectangleF(aabb.Left, aabb.Top, aabb.Width, aabb.Height);
        }

        public static explicit operator Rectangle(AABB v)
        {
            return Rectangle.Round((RectangleF) v);
        }

        public static implicit operator RawRectangleF(AABB bounds)
        {
            return new RawRectangleF(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        }

        /// <summary>
        /// State of an AABB instance.
        /// </summary>
        public enum State
        {
            Invalid,
            Valid
        }
    }

    /// <summary>
    /// Specifies left-top-bottom-right regions to expand AABB and RectangleF's with.
    /// </summary>
    public struct InsetBounds
    {
        public static readonly InsetBounds Empty = new InsetBounds(0, 0, 0, 0);

        public readonly float Left;
        public readonly float Top;
        public readonly float Bottom;
        public readonly float Right;

        public InsetBounds(float left, float top, float bottom, float right)
        {
            Bottom = bottom;
            Right = right;
            Left = left;
            Top = top;
        }

        [Pure]
        public AABB Inset(AABB aabb)
        {
            return aabb.OffsetBy(Left, Top).WithSize(aabb.Width - Left - Right, aabb.Height - Top - Bottom);
        }
    }
}

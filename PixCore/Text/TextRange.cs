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

namespace PixCore.Text
{
    /// <summary>
    /// Specifies a range on a text string
    /// </summary>
    [DebuggerDisplay("{Start} , {Length}")]
    public readonly struct TextRange : IEquatable<TextRange>
    {
        public TextRange(int start, int length) : this()
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }

        /// <summary>
        /// End of range; Equivalent to <see cref="Start"/> + <see cref="Length"/>.
        /// </summary>
        public int End => Start + Length;

        /// <summary>
        /// Returns true if <see cref="value"/> is &gt;= Start and &lt;= End
        /// </summary>
        public bool Contains(int value)
        {
            return value >= Start && value <= End;
        }

        /// <summary>
        /// Returns whether this range intersects another range.
        /// </summary>
        public bool Intersects(TextRange other)
        {
            return Start < other.End && other.Start < End;
        }

        /// <summary>
        /// Returns the intersection between this text range and another text range.
        /// 
        /// Intersection between the two ranges must have at least length 1 to be valid.
        /// 
        /// Returns null, if the ranges do not intersect.
        /// </summary>
        public TextRange? Intersection(TextRange other)
        {
            // Not intersecting
            if (End <= other.Start || other.End <= Start)
                return null;

            int start = Math.Max(Start, other.Start);
            int end = Math.Min(End, other.End);

            return new TextRange(start, end - start);
        }

        /// <summary>
        /// Returns the overlap between this text range and another text range.
        /// 
        /// Overlap between the two ranges can occur with 0-length overlaps; final overlap will
        /// also have 0 <see cref="Length"/>.
        /// 
        /// Returns null, if the ranges do not overlap.
        /// </summary>
        public TextRange? Overlap(TextRange other)
        {
            // Not intersecting
            if (End < other.Start || other.End < Start)
                return null;

            int start = Math.Max(Start, other.Start);
            int end = Math.Min(End, other.End);

            return new TextRange(start, end - start);
        }

        /// <summary>
        /// Returns the result of the union operation between this and another text range.
        /// </summary>
        public TextRange Union(TextRange other)
        {
            return FromOffsets(Math.Min(Start, other.Start), Math.Max(End, other.End));
        }

        public bool Equals(TextRange other)
        {
            return Start == other.Start && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TextRange range && Equals(range);
        }
        
        public static bool operator ==(TextRange lhs, TextRange rhs)
        {
            return lhs.Start == rhs.Start && lhs.Length == rhs.Length;
        }

        public static bool operator !=(TextRange lhs, TextRange rhs)
        {
            return lhs.Start != rhs.Start || lhs.Length != rhs.Length;
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ Length;
            }
        }

        public override string ToString()
        {
            return $"[ {Start} : {Length} ]";
        }

        /// <summary>
        /// Given two string offsets, returns a <see cref="TextRange"/> where <see cref="Start"/> is
        /// the minimum of the two offsets, and <see cref="Length"/> is the different between the two
        /// offsets.
        /// </summary>
        public static TextRange FromOffsets(int offset1, int offset2)
        {
            int min = Math.Min(offset1, offset2);
            int max = Math.Max(offset1, offset2);

            return new TextRange(min, max - min);
        }
    }
}
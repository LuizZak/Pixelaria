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
using System.Drawing;
using PixCore.Geometry;

namespace PixCore.Text.Attributes
{
    public struct BackgroundColorAttribute : ITextAttribute, IEquatable<BackgroundColorAttribute>
    {
        public Color BackColor { get; }
        public Vector Inflation { get; }

        public BackgroundColorAttribute(Color backColor)
        {
            BackColor = backColor;
            Inflation = Vector.Zero;
        }

        public BackgroundColorAttribute(Color backColor, Vector inflation)
        {
            BackColor = backColor;
            Inflation = inflation;
        }

        public bool Equals(BackgroundColorAttribute other)
        {
            return BackColor.Equals(other.BackColor) && Inflation.Equals(other.Inflation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is BackgroundColorAttribute attribute && Equals(attribute);
        }

        public static bool operator ==(BackgroundColorAttribute lhs, BackgroundColorAttribute rhs)
        {
            return lhs.BackColor == rhs.BackColor && lhs.Inflation == rhs.Inflation;
        }

        public static bool operator !=(BackgroundColorAttribute lhs, BackgroundColorAttribute rhs)
        {
            return lhs.BackColor != rhs.BackColor || lhs.Inflation != rhs.Inflation;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BackColor.GetHashCode() * 397) ^ Inflation.GetHashCode();
            }
        }

        public object Clone()
        {
            return new BackgroundColorAttribute(BackColor, Inflation);
        }
    }
}
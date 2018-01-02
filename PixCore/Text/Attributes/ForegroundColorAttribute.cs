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

namespace PixCore.Text.Attributes
{
    public struct ForegroundColorAttribute : ITextAttribute, IEquatable<ForegroundColorAttribute>
    {
        public Color ForeColor { get; }

        public ForegroundColorAttribute(Color foreColor)
        {
            ForeColor = foreColor;
        }

        public bool Equals(ForegroundColorAttribute other)
        {
            return ForeColor.Equals(other.ForeColor);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ForegroundColorAttribute attribute && Equals(attribute);
        }

        public static bool operator ==(ForegroundColorAttribute lhs, ForegroundColorAttribute rhs)
        {
            return lhs.ForeColor == rhs.ForeColor;
        }

        public static bool operator !=(ForegroundColorAttribute lhs, ForegroundColorAttribute rhs)
        {
            return lhs.ForeColor != rhs.ForeColor;
        }

        public override int GetHashCode()
        {
            return ForeColor.GetHashCode();
        }

        public object Clone()
        {
            return new ForegroundColorAttribute(ForeColor);
        }
    }
}
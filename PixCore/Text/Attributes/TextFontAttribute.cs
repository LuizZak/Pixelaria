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
    public readonly struct TextFontAttribute : ITextAttribute, IEquatable<TextFontAttribute>
    {
        public Font Font { get; }

        public TextFontAttribute(Font font)
        {
            Font = font;
        }

        public void Consume(ITextAttributeConsumer consumer)
        {
            consumer.Consume(this);
        }

        public bool Equals(TextFontAttribute other)
        {
            return Font.Equals(other.Font);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TextFontAttribute attribute && Equals(attribute);
        }
        
        public static bool operator ==(TextFontAttribute lhs, TextFontAttribute rhs)
        {
            return Equals(lhs.Font, rhs.Font);
        }

        public static bool operator !=(TextFontAttribute lhs, TextFontAttribute rhs)
        {
            return !Equals(lhs.Font, rhs.Font);
        }
        
        public override int GetHashCode()
        {
            return Font.GetHashCode();
        }

        public object Clone()
        {
            return this;
        }
    }
}
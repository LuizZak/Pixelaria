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

using PixCore.Text.Attributes;

namespace PixCore.Text
{
    /// <summary>
    /// An interface for interpreting all available <see cref="ITextAttribute"/> instances.
    /// </summary>
    public interface ITextAttributeConsumer
    {
        /// <summary>
        /// Consume a <see cref="TextFontAttribute"/>.
        /// </summary>
        void Consume(TextFontAttribute textFontAttribute);

        /// <summary>
        /// Consume a <see cref="ForegroundColorAttribute"/>.
        /// </summary>
        void Consume(ForegroundColorAttribute foreColorAttribute);

        /// <summary>
        /// Consume a <see cref="BackgroundColorAttribute"/>.
        /// </summary>
        void Consume(BackgroundColorAttribute backColorAttribute);

        /// <summary>
        /// Called to consume a non-specific <see cref="ITextAttribute"/> object.
        /// </summary>
        void ConsumeOther(ITextAttribute attribute);
    }
}

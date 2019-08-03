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

using System.Drawing;
using JetBrains.Annotations;
using PixCore.Text;

namespace PixRendering
{
    /// <summary>
    /// Interface for objects that are capable of figuring out sizes of text strings
    /// </summary>
    public interface ITextSizeProvider
    {
        /// <summary>
        /// Calculates the text size for a given pair of string/font
        /// </summary>
        SizeF CalculateTextSize([NotNull] string text, [NotNull] System.Drawing.Font font);

        /// <summary>
        /// Calculates the text size for a given pair of attributed string/font
        /// </summary>
        SizeF CalculateTextSize([NotNull] IAttributedText text, [NotNull] System.Drawing.Font font);

        /// <summary>
        /// Calculates the text size for a given pair of attributed string/font/font size
        /// </summary>
        SizeF CalculateTextSize([NotNull] IAttributedText text, [NotNull] string font, float fontSize);
    }
}
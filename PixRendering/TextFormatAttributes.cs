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

using JetBrains.Annotations;

namespace PixRendering
{
    /// <summary>
    /// Default struct for bundling text formatting attributes to feed to calls to <see cref="ITextMetricsProvider"/>
    /// </summary>
    public struct TextFormatAttributes
    {
        [NotNull]
        public string Font { get; set; }
        public float FontSize { get; set; }
        public HorizontalTextAlignment HorizontalTextAlignment { get; set; }
        public VerticalTextAlignment VerticalTextAlignment { get; set; }
        public TextWordWrap WordWrap { get; set; }

        /// <summary>
        /// Gets or sets the ellipsis trimming for this text layout.
        /// </summary>
        public TextEllipsisTrimming? TextEllipsisTrimming { get; set; }

        public TextFormatAttributes([NotNull] string font, float fontSize)
        {
            Font = font;
            FontSize = fontSize;
            HorizontalTextAlignment = HorizontalTextAlignment.Leading;
            VerticalTextAlignment = VerticalTextAlignment.Near;
            WordWrap = TextWordWrap.None;
            TextEllipsisTrimming = null;
        }
    }

    /// <summary>
    /// Horizontal text alignment.
    /// </summary>
    public enum HorizontalTextAlignment
    {
        Leading,
        Trailing,
        Center,
        Justified
    }

    /// <summary>
    /// Vertical text alignment.
    /// </summary>
    public enum VerticalTextAlignment
    {
        Near,
        Far,
        Center
    }

    /// <summary>
    /// Text word wrap.
    /// </summary>
    public enum TextWordWrap
    {
        None,
        ByCharacter,
        ByWord
    }
}

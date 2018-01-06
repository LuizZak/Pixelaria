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

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Default struct for bundling text attributes to feed to calls to <see cref="ITextMetricsProvider"/>
    /// </summary>
    public struct TextLayoutAttributes
    {
        [NotNull]
        public string Font { get; set; }
        public float FontSize { get; set; }
        public HorizontalTextAlignment HorizontalTextAlignment { get; set; }
        public VerticalTextAlignment VerticalTextAlignment { get; set; }
        public TextWordWrap WordWrap { get; set; }

        /// <summary>
        /// Total available width to draw text onto
        /// </summary>
        public float AvailableWidth { get; set; }

        /// <summary>
        /// Total available height to draw text onto
        /// </summary>
        public float AvailableHeight { get; set; }

        public TextLayoutAttributes([NotNull] string font, float fontSize,
            HorizontalTextAlignment horizontal = HorizontalTextAlignment.Leading,
            VerticalTextAlignment vertical = VerticalTextAlignment.Near)
        {
            Font = font;
            FontSize = fontSize;
            HorizontalTextAlignment = horizontal;
            VerticalTextAlignment = vertical;
            AvailableWidth = float.PositiveInfinity;
            AvailableHeight = float.PositiveInfinity;
            WordWrap = TextWordWrap.ByWord;
        }
    }
    
    /// <summary>
    /// Horizontal text alignment.
    /// </summary>
    public enum HorizontalTextAlignment
    {
        Leading,
        Center,
        Trailing
    }

    /// <summary>
    /// Vertical text alignment.
    /// </summary>
    public enum VerticalTextAlignment
    {
        Near,
        Center,
        Far
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
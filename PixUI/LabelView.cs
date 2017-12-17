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
using JetBrains.Annotations;
using PixCore.Geometry;
using PixUI.Controls;
using PixUI.Text;

namespace PixUI
{
    /// <summary>
    /// A basic view that represents a text label
    /// </summary>
    public sealed class LabelView : BaseView
    {
        [NotNull]
        public static ILabelViewSizeProvider DefaultLabelViewSizeProvider = new DefaultSizer();
        
        private AABB _bounds = AABB.Empty;
        private AABB _textBounds = AABB.Empty;
        
        [NotNull]
        private readonly AttributedText _attributedText = new AttributedText();

        [NotNull]
        private Font _font = new Font(FontFamily.GenericSansSerif.Name, 10);

        private InsetBounds _textInsetBounds;

        /// <summary>
        /// Gets or sets the background color that is drawn around the label.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the string this label represents
        /// </summary>
        [NotNull]
        public string Text
        {
            get => _attributedText.String;
            set
            {
                if (!_attributedText.HasAttributes && _attributedText.String == value)
                    return;

                AttributedText.SetText(value);
            }
        }

        /// <summary>
        /// Gets the attributed text for this label view
        /// </summary>
        [NotNull]
        public IAttributedText AttributedText => _attributedText;

        /// <summary>
        /// Gets or sets the text color of this label view
        /// </summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>
        /// Gets or sets the size provider for this label view.
        /// 
        /// If null, label view defaults to using <see cref="DefaultLabelViewSizeProvider"/>
        /// </summary>
        [CanBeNull]
        public ILabelViewSizeProvider SizeProvider { get; set; }

        /// <summary>
        /// Gets or sets the text font for the text of this label view
        /// </summary>
        [NotNull]
        public Font TextFont
        {
            get => _font;
            set
            {
                _font = value;

                CalculateBounds();
            }
        }

        /// <summary>
        /// An AABB that expands the four corners of the text bounds from
        /// within this view.
        /// 
        /// The final text label has the size of its text string expanded by this
        /// value.
        /// </summary>
        public InsetBounds TextInsetBounds
        {
            get => _textInsetBounds;
            set
            {
                if (TextInsetBounds.Equals(value))
                    return;
                
                _textInsetBounds = value;
                CalculateBounds();
            }
        }

        /// <summary>
        /// Gets or sets if this label should be visible on screen.
        /// </summary>
        public bool Visible { get; set; } = true;

        public override AABB Bounds => _bounds;

        /// <summary>
        /// Returns the exact AABB from within this label view where the text will
        /// appear in when insetted by <see cref="TextInsetBounds"/>.
        /// </summary>
        public AABB TextBounds
        {
            get
            {
                var final = _bounds;
                final = _textInsetBounds.Inset(final);
                return final;
            }
        }

        public LabelView()
        {
            _attributedText.Modified += AttributedTextModified;
        }

        private void AttributedTextModified(object sender, EventArgs eventArgs)
        {
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            _textBounds = new RectangleF(PointF.Empty, (SizeProvider ?? DefaultLabelViewSizeProvider).CalculateTextSize(this));
            Size = _textBounds.Size;

            Size += new Vector(TextInsetBounds.Left + TextInsetBounds.Right,
                TextInsetBounds.Top + TextInsetBounds.Bottom);

            _bounds = new AABB(Vector.Zero, Size);
        }

        private class DefaultSizer : ILabelViewSizeProvider
        {
            public SizeF CalculateTextSize(LabelView label)
            {
                return CalculateTextSize(new AttributedText(label.Text), label.TextFont);
            }

            /// <summary>
            /// Calculates the text size for a given pair of string/font
            /// </summary>
            public SizeF CalculateTextSize(string text, Font font)
            {
                return CalculateTextSize(new AttributedText(text), font);
            }

            public SizeF CalculateTextSize(IAttributedText text, Font font)
            {
                using (var dummy = new Bitmap(1, 1))
                using (var graphics = Graphics.FromImage(dummy))
                {
                    return graphics.MeasureString(text.String, font);
                }
            }

            public SizeF CalculateTextSize(IAttributedText text, string fontName, float size)
            {
                using (var dummy = new Bitmap(1, 1))
                using (var graphics = Graphics.FromImage(dummy))
                using (var font = new Font(fontName, size))
                {
                    return graphics.MeasureString(text.String, font);
                }
            }
        }
    }

    /// <summary>
    /// Interface for objects that are capable of figuring out sizes of text in label views
    /// </summary>
    public interface ILabelViewSizeProvider
    {
        /// <summary>
        /// Calculates the text size on a given label view
        /// </summary>
        SizeF CalculateTextSize([NotNull] LabelView label);

        /// <summary>
        /// Calculates the text size for a given pair of string/font
        /// </summary>
        SizeF CalculateTextSize([NotNull] string text, [NotNull] Font font);

        /// <summary>
        /// Calculates the text size for a given pair of attributed string/font
        /// </summary>
        SizeF CalculateTextSize([NotNull] IAttributedText text, [NotNull] Font font);
        
        /// <summary>
        /// Calculates the text size for a given pair of attributed string/font/font size
        /// </summary>
        SizeF CalculateTextSize([NotNull] IAttributedText text, [NotNull] string font, float fontSize);
    }

    /// <summary>
    /// Provides an interface for objects to request metrics about positions of glyphs in a string.
    /// </summary>
    public interface ILabelViewTextMetricsProvider
    {
        /// <summary>
        /// Gets the bounding box for a single character at a given absolute string offset
        /// </summary>
        AABB LocationOfCharacter(int offset, [NotNull] IAttributedText text, TextAttributes textAttributes);

        /// <summary>
        /// Gets the bounding box for a set of characters at a given absolute string offset + length
        /// </summary>
        AABB[] LocationOfCharacters(int offset, int length, [NotNull] IAttributedText text, TextAttributes textAttributes);
    }

    /// <summary>
    /// Default struct for bundling text attributes to feed to calls to <see cref="ILabelViewTextMetricsProvider"/>
    /// </summary>
    public struct TextAttributes
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

        public TextAttributes([NotNull] string font, float fontSize,
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
}

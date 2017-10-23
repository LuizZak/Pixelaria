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
using System.Drawing;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Pixelaria.Views.ModelViews.PipelineView
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
        /// Gets or sets the attributed text for this label view
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
            _textBounds = new RectangleF(PointF.Empty, (SizeProvider ?? DefaultLabelViewSizeProvider).CalculateTextBounds(this));
            Size = _textBounds.Size;

            Size += new Vector(TextInsetBounds.Left + TextInsetBounds.Right,
                TextInsetBounds.Top + TextInsetBounds.Bottom);

            _bounds = new AABB(Vector.Zero, Size);
        }

        private class DefaultSizer : ILabelViewSizeProvider
        {
            public SizeF CalculateTextBounds(LabelView label)
            {
                return CalculateTextBounds(new AttributedText(label.Text), label.TextFont);
            }

            /// <summary>
            /// Calculates the text size for a given pair of string/font
            /// </summary>
            public SizeF CalculateTextBounds(string text, Font font)
            {
                return CalculateTextBounds(new AttributedText(text), font);
            }

            public SizeF CalculateTextBounds(IAttributedText text, Font font)
            {
                using (var dummy = new Bitmap(1, 1))
                using (var graphics = Graphics.FromImage(dummy))
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
        SizeF CalculateTextBounds([NotNull] LabelView label);

        /// <summary>
        /// Calculates the text size for a given pair of string/font
        /// </summary>
        SizeF CalculateTextBounds([NotNull] string text, [NotNull] Font font);

        /// <summary>
        /// Calculates the text size for a given pair of attributed string/font
        /// </summary>
        SizeF CalculateTextBounds([NotNull] IAttributedText text, [NotNull] Font font);
    }
}

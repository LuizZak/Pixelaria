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
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixRendering;
using Font = System.Drawing.Font;

namespace PixUI.Controls
{
    /// <summary>
    /// A basic textual label.
    /// 
    /// By default, label controls don't have interaction enabled and pass through all mouse events.
    /// </summary>
    public class LabelViewControl : ControlView
    {
        /// <summary>
        /// Cached text layout instance refreshed on redraw, and reset every time text settings change
        /// </summary>
        [CanBeNull]
        private ITextLayout _textLayout;

        private readonly LabelViewBacking _labelViewBacking;

        private HorizontalTextAlignment _horizontalTextAlignment = HorizontalTextAlignment.Leading;
        private VerticalTextAlignment _verticalTextAlignment = VerticalTextAlignment.Near;
        private TextWordWrap _textWordWrap = TextWordWrap.None;
        private bool _autoResize = true;

        /// <summary>
        /// Whether to automatically resize this label view's bounds
        /// whenever its properties are updated.
        /// </summary>
        public bool AutoResize
        {
            get => _autoResize;
            set
            {
                _autoResize = value;

                if (_autoResize)
                    CalculateBounds();
            }
        }

        internal override Vector IntrinsicSize => _labelViewBacking?.CalculateSize(LabelView.defaultTextSizeProvider) ?? Vector.Zero;

        [NotNull]
        public Font TextFont
        {
            get => _labelViewBacking.TextFont;
            set
            {
                _labelViewBacking.TextFont = value;
                ResetTextFormat();
            }
        }

        [NotNull]
        public string Text
        {
            get => _labelViewBacking.Text;
            set
            {
                if (AutoResize)
                    Layout();

                _labelViewBacking.Text = value;

                ResetTextFormat();
            }
        }

        /// <summary>
        /// Gets the attributed text for this label view control
        /// </summary>
        [NotNull]
        public IAttributedText AttributedText => _labelViewBacking.AttributedText;

        public HorizontalTextAlignment HorizontalTextAlignment
        {
            get => _horizontalTextAlignment;
            set
            {
                _horizontalTextAlignment = value;
                ResetTextFormat();
                Invalidate();
            }
        }

        public VerticalTextAlignment VerticalTextAlignment
        {
            get => _verticalTextAlignment;
            set
            {
                _verticalTextAlignment = value;
                ResetTextFormat();
                Invalidate();
            }
        }

        public TextWordWrap TextWordWrap
        {
            get => _textWordWrap;
            set
            {
                _textWordWrap = value;
                ResetTextFormat();
                Invalidate();
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="LabelViewControl"/>.
        /// </summary>
        /// <returns></returns>
        public static LabelViewControl Create([NotNull] string text = "")
        {
            var instance = new LabelViewControl();

            instance.Initialize(text);

            return instance;
        }
        
        protected LabelViewControl()
        {
            _labelViewBacking = new LabelViewBacking();
        }

        protected virtual void Initialize([NotNull] string text)
        {
            InteractionEnabled = false;
            _labelViewBacking.BoundsInvalidated += (sender, args) =>
            {
                SetNeedsLayout();
                CalculateBounds();
                Invalidate();
            };

            Text = text;
        }

        protected override void Dispose(bool disposing)
        {
            _labelViewBacking?.Dispose();

            _textLayout?.Dispose();
            _textLayout = null;

            base.Dispose(disposing);
        }

        /// <summary>
        /// Forces this <see cref="LabelViewControl"/> to resize itself to fit its text.
        /// </summary>
        public void AutoSize()
        {
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            if (AutoResize)
                Size = _labelViewBacking.CalculateSize(LabelView.defaultTextSizeProvider);
        }
        
        public override void RenderForeground(ControlRenderingContext context)
        {
            RefreshTextFormat();

            Debug.Assert(_textLayout != null, "_textLayout != null");

            context.Renderer.SetFillColor(ForeColor);
            context.Renderer.DrawAttributedText(AttributedText, TextFormatAttributes(), Bounds);
        }

        public void WithTextLayout([NotNull, InstantHandle] Action<ITextLayout> perform)
        {
            RefreshTextFormat();

            perform(_textLayout);
        }

        /// <summary>
        /// Returns the text layout attributes for this label view control.
        /// </summary>
        public TextLayoutAttributes TextLayoutAttributes()
        {
            return new TextLayoutAttributes(TextFont.Name, TextFont.Size, HorizontalTextAlignment, VerticalTextAlignment, TextWordWrap)
            {
                AvailableWidth = Width,
                AvailableHeight = Height
            };
        }

        public TextFormatAttributes TextFormatAttributes()
        {
            return new TextFormatAttributes(TextFont.Name, TextFont.Size)
            {
                HorizontalTextAlignment = HorizontalTextAlignment,
                VerticalTextAlignment = VerticalTextAlignment,
                WordWrap = TextWordWrap
            };
        }

        private void RefreshTextFormat()
        {
            Debug.Assert(TextLayoutRenderer != null, "TextLayoutRenderer != null");

            // Render text
            if (_textLayout != null)
                return;

            _textLayout = TextLayoutRenderer.CreateTextLayout(AttributedText, TextLayoutAttributes());
        }

        private void ResetTextFormat()
        {
            _textLayout?.Dispose();
            _textLayout = null;
        }
    }
}
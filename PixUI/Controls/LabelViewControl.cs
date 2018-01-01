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
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using PixUI.Rendering;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Font = System.Drawing.Font;
using Factory = SharpDX.DirectWrite.Factory;

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
        /// Cached text format instance refreshed on redraw, and reset every time text settings change
        /// </summary>
        [CanBeNull]
        private TextFormat _textFormat;
        /// <summary>
        /// Cached text layout instance refreshed on redraw, and reset every time text settings change
        /// </summary>
        [CanBeNull]
        private TextLayout _textLayout;

        /// <summary>
        /// The backing label view for this label view control
        /// </summary>
        private readonly LabelView _labelView;

        private HorizontalTextAlignment _horizontalTextAlignment = HorizontalTextAlignment.Leading;
        private VerticalTextAlignment _verticalTextAlignment = VerticalTextAlignment.Near;
        private TextWordWrap _textWordWrap = TextWordWrap.None;

        /// <summary>
        /// Whether to automatically resize this label view's bounds
        /// whenever its properties are updated.
        /// </summary>
        public bool AutoResize { get; set; } = true;
        
        [NotNull]
        public Font TextFont
        {
            get => _labelView.TextFont;
            set
            {
                _labelView.TextFont = value;
                CalculateBounds();
                ResetTextFormat();
            }
        }

        [NotNull]
        public string Text
        {
            get => _labelView.Text;
            set
            {
                _labelView.Text = value;

                CalculateBounds();
                ResetTextFormat();
            }
        }

        /// <summary>
        /// Gets the attributed text for this label view control
        /// </summary>
        [NotNull]
        public IAttributedText AttributedText => _labelView.AttributedText;

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
            _labelView = new LabelView();
        }

        protected void Initialize([NotNull] string text)
        {
            InteractionEnabled = false;

            Text = text;
        }

        protected override void Dispose(bool disposing)
        {
            _textFormat?.Dispose();
            _textFormat = null;

            _textLayout?.Dispose();
            _textLayout = null;

            base.Dispose(disposing);
        }

        private void CalculateBounds()
        {
            if (AutoResize)
                Size = _labelView.Size;
        }
        
        public override void RenderForeground(ControlRenderingContext context)
        {
            RefreshTextFormat(context.State.DirectWriteFactory);

            Debug.Assert(_textLayout != null, "_textLayout != null");

            context.Renderer.WithPreparedTextLayout(ForeColor.ToColor4(), AttributedText, _textLayout, (layout, renderer) =>
            {
                // Render background segments
                var backSegments =
                    AttributedText.GetTextSegments()
                        .Where(seg => seg.HasAttribute<BackgroundColorAttribute>());

                foreach (var segment in backSegments)
                {
                    var attr = segment.GetAttribute<BackgroundColorAttribute>();

                    using (var brush = new SolidColorBrush(context.RenderTarget, attr.BackColor.ToColor4()))
                    {
                        var metrics = layout.HitTestTextRange(segment.TextRange.Start, segment.TextRange.Length, 0, 0);

                        foreach (var metric in metrics)
                        {
                            var aabb = AABB.FromRectangle(metric.Left, metric.Top, metric.Width, metric.Height);
                            context.RenderTarget.FillRectangle(aabb.Inflated(attr.Inflation).ToRawRectangleF(), brush);
                        }
                    }
                }

                layout.Draw(renderer, Bounds.Minimum.X, Bounds.Minimum.Y);
            });
        }

        public void WithTextLayout([NotNull, InstantHandle] Action<TextLayout> perform)
        {
            RefreshTextFormat(DirectWriteFactory);

            perform(_textLayout);
        }

        private void RefreshTextFormat(Factory factory)
        {
            // Render text
            if (_textFormat != null && _textLayout != null)
                return;

            var horizontalAlign =
                Direct2DHelpers.DirectWriteAlignmentFor(HorizontalTextAlignment);
            var verticalAlign =
                Direct2DHelpers.DirectWriteAlignmentFor(VerticalTextAlignment);
            var wordWrap =
                Direct2DHelpers.DirectWriteWordWrapFor(TextWordWrap);

            _textFormat =
                new TextFormat(factory, _labelView.TextFont.Name, _labelView.TextFont.Size)
                {
                    TextAlignment = horizontalAlign,
                    ParagraphAlignment = verticalAlign,
                    WordWrapping = wordWrap
                };

            _textLayout = new TextLayout(factory, Text, _textFormat, Bounds.Width, Bounds.Height);
        }

        private void ResetTextFormat()
        {
            _textFormat?.Dispose();
            _textFormat = null;

            _textLayout?.Dispose();
            _textLayout = null;
        }
    }

    /// <summary>
    /// Text alignment of a <see cref="LabelViewControl"/>.
    /// </summary>
    public enum HorizontalTextAlignment
    {
        Leading,
        Center,
        Trailing
    }

    /// <summary>
    /// Vertical text alignment of a <see cref="LabelViewControl"/>.
    /// </summary>
    public enum VerticalTextAlignment
    {
        Near,
        Center,
        Far
    }
    
    /// <summary>
    /// Text word wrap of a <see cref="LabelViewControl"/>.
    /// </summary>
    public enum TextWordWrap
    {
        None,
        ByCharacter,
        ByWord
    }
}
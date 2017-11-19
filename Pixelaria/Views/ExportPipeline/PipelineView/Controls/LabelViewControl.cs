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

using Font = System.Drawing.Font;

using JetBrains.Annotations;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A basic textual label.
    /// 
    /// By default, label controls don't have interaction enabled and pass through all mouse events.
    /// </summary>
    internal class LabelViewControl : ControlView
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
                ResetTestFormat();
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
                ResetTestFormat();
            }
        }

        /// <summary>
        /// Gets the attributed text for this label view control
        /// </summary>
        [NotNull]
        public IAttributedText AttributedText => _labelView.AttributedText;

        public HorizontalTextAlignment HorizontalTextAlignment { get; set; } = HorizontalTextAlignment.Leading;

        public VerticalTextAlignment VerticalTextAlignment { get; set; } = VerticalTextAlignment.Near;

        public TextWordWrap TextWordWrap { get; set; } = TextWordWrap.None;

        public LabelViewControl()
        {
            _labelView = new LabelView();
            InteractionEnabled = false;
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
            // Render text
            if (_textFormat == null)
            {
                var horizontalAlign = 
                    Direct2DRenderer.DirectWriteAlignmentFor(HorizontalTextAlignment);
                var verticalAlign = 
                    Direct2DRenderer.DirectWriteAlignmentFor(VerticalTextAlignment);
                var wordWrap =
                    Direct2DRenderer.DirectWriteWordWrapFor(TextWordWrap);
                
                _textFormat =
                    new TextFormat(context.State.DirectWriteFactory, _labelView.TextFont.Name, _labelView.TextFont.Size)
                    {
                        TextAlignment = horizontalAlign,
                        ParagraphAlignment = verticalAlign,
                        WordWrapping = wordWrap
                    };
                
                _textLayout = new TextLayout(context.State.DirectWriteFactory, Text, _textFormat, Bounds.Width, Bounds.Height);
            }

            using (var brush = new SolidColorBrush(context.RenderTarget, ForeColor.ToColor4()))
            {
                context.RenderTarget.DrawText(_labelView.Text, _textFormat, Bounds, brush, DrawTextOptions.Clip);
            }
        }
        
        private void ResetTestFormat()
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
    internal enum HorizontalTextAlignment
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
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
using System.Windows.Forms;
using FontFamily = System.Drawing.FontFamily;

using JetBrains.Annotations;

using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A basic Button control
    /// </summary>
    internal class ButtonControl : ControlView
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

        private ButtonState _state = ButtonState.Normal;

        private Color _normalColor = Color.FromKnownColor(KnownColor.Control);
        private Color _highlightColor = Color.LightGray;
        private Color _selectedColor = Color.Gray;

        private bool _mouseDown;

        /// <summary>
        /// Gets or sets the default state color for this Button when it's not being
        /// hovered or pressed down.
        /// </summary>
        public Color NormalColor
        {
            get => _normalColor;
            set
            {
                _normalColor = value;
                UpdateColors();
            }
        }

        /// <summary>
        /// Gets or sets the color to use when hovering over this button without pressing
        /// it down.
        /// </summary>
        public Color HighlightColor
        {
            get => _highlightColor;
            set
            {
                _highlightColor = value;
                UpdateColors();
            }
        }

        /// <summary>
        /// Gets or sets the color to use when hovering over this button while pressing it
        /// down.
        /// </summary>
        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                UpdateColors();
            }
        }

        /// <summary>
        /// Gets the current button state
        /// </summary>
        public ButtonState State
        {
            get => _state;
            private set
            {
                _state = value;
                UpdateColors();
            }
        }

        /// <summary>
        /// Gets or sets the horizontal text alignment for the control
        /// </summary>
        public HorizontalTextAlignment HorizontalTextAlignment
        {
            get => _textAlignment;
            set
            {
                _textAlignment = value;
                ResetTestFormat();
            }
        }

        /// <summary>
        /// Text label for the button
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                ResetTestFormat();
            }
        }

        /// <summary>
        /// Color for button's label
        /// </summary>
        public Color TextColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets the inset region to retract the text label of this button by.
        /// </summary>
        public InsetBounds TextInset { get; set; }

        /// <summary>
        /// Gets or sets an image resource to draw besides the text label.
        /// 
        /// If an image is set, it is rendered to the left of the button's text.
        /// </summary>
        public ImageResource Image { get; set; }

        /// <summary>
        /// OnClick event for button
        /// </summary>
        public EventHandler Clicked;

        private string _text = "Button";
        private HorizontalTextAlignment _textAlignment;

        public ButtonControl()
        {
            CornerRadius = 3;
        }

        protected override void Dispose(bool disposing)
        {
            _textFormat?.Dispose();
            _textLayout?.Dispose();

            base.Dispose(disposing);
        }

        public override void RenderForeground(ControlRenderingContext context)
        {
            // Render text
            if (_textFormat == null)
            {
                TextAlignment textAlign;

                switch (HorizontalTextAlignment)
                {
                    case HorizontalTextAlignment.Left:
                        textAlign = TextAlignment.Leading;
                        break;
                    case HorizontalTextAlignment.Right:
                        textAlign = TextAlignment.Trailing;
                        break;
                    case HorizontalTextAlignment.Center:
                        textAlign = TextAlignment.Center;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string fontName = FontFamily.GenericSansSerif.Name;
                const float fontSize = 12.0f;

                _textFormat = new TextFormat(context.State.DirectWriteFactory, fontName, fontSize)
                {
                    TextAlignment = textAlign,
                    ParagraphAlignment = ParagraphAlignment.Center
                };

                // Get a text layout with the proper size for the button
                var available = Bounds;
                available = available.Inset(new InsetBounds(Image.Width, 0, 0, 0));
                available = available.Inset(TextInset);

                _textLayout = new TextLayout(context.State.DirectWriteFactory, Text, _textFormat, available.Width, available.Height);
            }

            using (var brush = new SolidColorBrush(context.RenderTarget, TextColor.ToColor4()))
            {
                var bounds = Bounds.Inset(TextInset);
                context.RenderTarget.DrawTextLayout(bounds.Minimum, _textLayout, brush);
                //context.RenderTarget.DrawText(Text, _textFormat, bounds, brush);
            }
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();

            State = ButtonState.Highlight;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            State = ButtonState.Selected;

            _mouseDown = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown)
            {
                State = Bounds.Contains(e.Location) ? ButtonState.Selected : ButtonState.Highlight;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            State = ButtonState.Highlight;

            _mouseDown = false;
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();

            State = ButtonState.Normal;
        }

        private void UpdateColors()
        {
            switch (_state)
            {
                case ButtonState.Normal:
                    BackColor = NormalColor;
                    break;
                case ButtonState.Selected:
                    BackColor = SelectedColor;
                    break;
                case ButtonState.Highlight:
                    BackColor = HighlightColor;
                    break;
            }
        }

        private void ResetTestFormat()
        {
            _textFormat?.Dispose();
            _textFormat = null;

            _textLayout?.Dispose();
            _textLayout = null;
        }

        /// <summary>
        /// Describes possible button states
        /// </summary>
        public enum ButtonState
        {
            Normal,
            Highlight,
            Selected
        }
    }

    /// <summary>
    /// Specifies a horizontal text alignment for a button or label control
    /// </summary>
    internal enum HorizontalTextAlignment
    {
        Center,
        Left,
        Right
    }
}
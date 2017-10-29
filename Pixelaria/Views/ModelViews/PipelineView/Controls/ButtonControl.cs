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
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using FontFamily = System.Drawing.FontFamily;

namespace Pixelaria.Views.ModelViews.PipelineView.Controls
{
    /// <summary>
    /// A basic Button control
    /// </summary>
    internal class ButtonControl : ControlView
    {
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
        /// Text label for the button
        /// </summary>
        public string Text { get; set; } = "Button";

        /// <summary>
        /// Color for button's label
        /// </summary>
        public Color TextColor { get; set; } = Color.Black;

        /// <summary>
        /// OnClick event for button
        /// </summary>
        public EventHandler Clicked;

        public ButtonControl()
        {
            CornerRadius = 3;
        }

        public override void RenderForeground(Direct2DRenderingState state)
        {
            // Render text
            using (var brush = new SolidColorBrush(state.D2DRenderTarget, TextColor.ToColor4()))
            {
                using (var textFormat = new TextFormat(state.DirectWriteFactory, FontFamily.GenericSansSerif.Name, 12) { TextAlignment = TextAlignment.Center, ParagraphAlignment = ParagraphAlignment.Center })
                {
                    state.D2DRenderTarget.DrawText(Text, textFormat, Bounds, brush);
                }
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

            BackColor = NormalColor;
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
}
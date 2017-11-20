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

using Pixelaria.Utils;
using Pixelaria.Utils.Layouting;
using SharpDX.Direct2D1;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A basic Button control
    /// </summary>
    internal class ButtonControl : ControlView
    {
        /// <summary>
        /// The textual label for the button
        /// </summary>
        private readonly LabelViewControl _label = new LabelViewControl();
        
        private ButtonState _state = ButtonState.Normal;

        private ImageResource? _image;
        private InsetBounds _textInset;
        private InsetBounds _imageInset;

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
        /// Gets or sets the horizontal text alignment for the control's contents
        /// </summary>
        public HorizontalTextAlignment HorizontalTextAlignment
        {
            get => _label.HorizontalTextAlignment;
            set => _label.HorizontalTextAlignment = value;
        }

        /// <summary>
        /// Gets or sets the vertical text alignment for the control's contents
        /// </summary>
        public VerticalTextAlignment VerticalTextAlignment
        {
            get => _label.VerticalTextAlignment;
            set => _label.VerticalTextAlignment = value;
        }

        /// <summary>
        /// Text label for the button
        /// </summary>
        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        /// <summary>
        /// Color for button's label
        /// </summary>
        public Color TextColor
        {
            get => _label.ForeColor;
            set => _label.ForeColor = value;
        }

        /// <summary>
        /// Font for button's label
        /// </summary>
        public Font TextFont
        {
            get => _label.TextFont;
            set => _label.TextFont = value;
        }

        /// <summary>
        /// Gets or sets the inset region to retract the text label of this button by.
        /// </summary>
        public InsetBounds TextInset
        {
            get => _textInset;
            set
            {
                _textInset = value;
                Layout();
            }
        }

        /// <summary>
        /// Gets or sets the inset region to retract the image of this button by.
        /// </summary>
        public InsetBounds ImageInset
        {
            get => _imageInset;
            set
            {
                _imageInset = value;
                Layout();
            }
        }

        /// <summary>
        /// Gets or sets an image resource to draw besides the text label.
        /// 
        /// If an image is set, it is rendered to the left of the button's text.
        /// </summary>
        public ImageResource? Image
        {
            get => _image;
            set
            {
                _image = value;
                Layout();
            }
        }

        /// <summary>
        /// OnClick event for button
        /// </summary>
        public EventHandler Clicked;

        public ButtonControl()
        {
            CornerRadius = 3;

            _label.Text = "Button";

            _label.BackColor = Color.Transparent;
            _label.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            _label.VerticalTextAlignment = VerticalTextAlignment.Center;
            _label.TextWordWrap = TextWordWrap.ByWord;
            _label.StrokeColor = Color.Transparent;
            _label.AutoResize = false;
            
            AddChild(_label);

            Layout();
        }
        
        protected override void OnResize()
        {
            base.OnResize();

            Layout();
        }

        public override void RenderForeground(ControlRenderingContext context)
        {
            // Render image, if available
            if (!Image.HasValue)
                return;

            var image = Image.Value;
            var bitmapBounds = BoundsForImage(image);

            var bitmap = context.Renderer.ImageResources.BitmapForResource(image);
            context.RenderTarget.DrawBitmap(bitmap, bitmapBounds, 1, BitmapInterpolationMode.Linear);
        }

        private AABB BoundsForText()
        {
            var bounds = Bounds.Inset(TextInset);

            if (!Image.HasValue)
                return bounds;

            var image = Image.Value;
            var imgBounds = BoundsForImage(image);

            bounds = bounds.Inset(new InsetBounds(imgBounds.Right, 0, 0, 0));

            return bounds;
        }

        private AABB BoundsForImage(ImageResource image)
        {
            var bitmapBounds = AABB.FromRectangle(Vector.Zero, image.Size);
            var bounds = Bounds.Inset(ImageInset);

            bitmapBounds = bitmapBounds.OffsetBy(bounds.Minimum.X, 0);
            bitmapBounds = LayoutingHelper.CenterWithinContainer(bitmapBounds, bounds, LayoutDirection.Vertical);

            return bitmapBounds;
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

        private void Layout()
        {
            _label.SetFrame(BoundsForText());
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
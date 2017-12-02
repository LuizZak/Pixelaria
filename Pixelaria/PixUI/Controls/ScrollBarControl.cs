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
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;

namespace Pixelaria.PixUI.Controls
{
    /// <summary>
    /// A scroll bar control that allows scrolling 
    /// </summary>
    internal sealed class ScrollBarControl : ControlView
    {
        private Vector _dragStart = Vector.Zero;
        private float _scrollStart;

        private readonly ButtonControl _decreaseScroll = new ButtonControl();
        private readonly ButtonControl _increaseScroll = new ButtonControl();

        private readonly ControlView _scrollBarKnob = new ControlView();
        private readonly ControlView _scrollBarArea = new ControlView();

        private IScrollBarControlStyle _scrollBarStyle = new DarkScrollBarControlStyle();
        private ScrollBarOrientation _orientation = ScrollBarOrientation.Vertical;
        private float _contentSize;
        private float _visibleSize;
        private float _scroll;
        
        public IScrollBarControlStyle ScrollBarStyle
        {
            get => _scrollBarStyle;
            set
            {
                _scrollBarStyle = value;
                SetupScrollBarColors();
            }
        }

        /// <summary>
        /// Size of content to scroll through
        /// </summary>
        public float ContentSize
        {
            get => _contentSize;
            set
            {
                _contentSize = value;
                UpdateScrollBarPosition();
            }
        }

        /// <summary>
        /// The size of the content which is visible when scrolled
        /// </summary>
        public float VisibleSize
        {
            get => _visibleSize;
            set
            {
                _visibleSize = value;
                UpdateScrollBarPosition();
            }
        }

        /// <summary>
        /// Gets or sets the scroll value.
        /// 
        /// Scroll value must be between 0 and ContentSize - VisibleSize.
        /// </summary>
        public float Scroll
        {
            get => _scroll;
            set
            {
                _scroll = Math.Max(0, Math.Min(ContentSize - VisibleSize, value));

                UpdateScrollBarPosition();
            }
        }

        /// <summary>
        /// Event called when the user scrolls the scroll bar.
        /// 
        /// This is not called when <see cref="Scroll"/> value is programatically set.
        /// </summary>
        public EventHandler ScrollChanged;

        /// <summary>
        /// Gets or sets the orientation of this scroll bar.
        /// 
        /// Defaults to <see cref="ScrollBarOrientation.Vertical"/>.
        /// </summary>
        public ScrollBarOrientation Orientation
        {
            get => _orientation;
            set
            {
                _orientation = value;

                Layout();
                UpdateScrollBarPosition();
            }
        }

        public ScrollBarControl()
        {
            AddChild(_decreaseScroll);
            AddChild(_increaseScroll);
            AddChild(_scrollBarArea);
            AddChild(_scrollBarKnob);

            _scrollBarKnob.InteractionEnabled = false;

            _scrollBarArea.BackColor = Color.Transparent;
            _scrollBarArea.StrokeColor = Color.Transparent;

            _decreaseScroll.Text = "-";
            _increaseScroll.Text = "+";

            Layout();

            const float scrollSpeed = 25;
            
            SetupRepeatFire(_increaseScroll, () =>
            {
                Scroll += scrollSpeed;
                ScrollChanged?.Invoke(this, EventArgs.Empty);
            });

            SetupRepeatFire(_decreaseScroll, () =>
            {
                Scroll -= scrollSpeed;
                ScrollChanged?.Invoke(this, EventArgs.Empty);
            });
            
            var mouseDrag = new DragMouseEventRecognizer();
            _scrollBarArea.AddMouseRecognizer(mouseDrag);

            mouseDrag.DragMouseEvent += OnMouseDragOnDragMouseEvent;

            SetupScrollBarColors();
        }

        private void OnMouseDragOnDragMouseEvent(object sender, [NotNull] DragMouseEventRecognizer.DragMouseEventArgs args)
        {
            switch (args.State)
            {
                case DragMouseEventRecognizer.DragMouseEventState.MouseEntered:
                    _scrollBarKnob.BackColor = ScrollBarStyle.ScrollBarKnobHighlightColor();
                    break;

                case DragMouseEventRecognizer.DragMouseEventState.MousePressed:
                    _dragStart = args.MousePosition;
                    _scrollStart = Scroll;

                    _scrollBarKnob.BackColor = ScrollBarStyle.ScrollBarKnobPressedColor();
                    break;

                case DragMouseEventRecognizer.DragMouseEventState.MouseMoved:
                    // Calculate scroll magnitude
                    float magnitude = ContentSize / VisibleSize;

                    var offset = args.MousePosition - _dragStart;
                    float axis = Orientation == ScrollBarOrientation.Horizontal ? offset.X : offset.Y;

                    Scroll = _scrollStart + axis * magnitude;
                    ScrollChanged?.Invoke(this, EventArgs.Empty);
                    break;

                case DragMouseEventRecognizer.DragMouseEventState.MouseReleased:
                    _scrollBarKnob.BackColor = ScrollBarStyle.ScrollBarKnobHighlightColor();
                    break;

                case DragMouseEventRecognizer.DragMouseEventState.MouseLeft:
                    _scrollBarKnob.BackColor = ScrollBarStyle.ScrollBarKnobNormalColor();
                    break;
            }
        }

        private void SetupRepeatFire([NotNull] ControlView button, [NotNull] Action onFire)
        {
            button.Rx
                .MouseDownRepeating(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(50))
                .Subscribe(e =>
                {
                    if(button.Bounds.Contains(e.Location))
                        onFire();
                }).AddToDisposable(DisposeBag);
        }

        protected override void OnResize()
        {
            base.OnResize();

            Layout();
        }

        private void UpdateScrollBarPosition()
        {
            if (_orientation == ScrollBarOrientation.Vertical)
            {
                var barArea = Bounds;
                barArea = barArea.Inset(new InsetBounds(2, 2, 2, 2));
                barArea = barArea.Inset(new InsetBounds(0, _decreaseScroll.Bounds.Height, _increaseScroll.Bounds.Height,
                    0));

                float ratio = VisibleSize / ContentSize;
                if (ratio >= 1)
                {
                    _scrollBarKnob.Location = barArea.Minimum;
                    _scrollBarKnob.Size = barArea.Size;

                    return;
                }

                float barSize = barArea.Height * ratio;

                float start = barArea.Top + barArea.Height * Math.Min(1, Scroll / ContentSize);

                start = Math.Max(barArea.Top, Math.Min(barArea.Bottom, start));

                _scrollBarKnob.Location = new Vector(barArea.Left, start);
                _scrollBarKnob.Size = new Vector(barArea.Width, barSize);
            }
            else
            {
                var barArea = Bounds;
                barArea = barArea.Inset(new InsetBounds(2, 2, 2, 2));
                barArea = barArea.Inset(new InsetBounds(_decreaseScroll.Bounds.Width, 0, 0, _increaseScroll.Bounds.Width));

                float ratio = VisibleSize / ContentSize;
                if (ratio >= 1)
                {
                    _scrollBarKnob.Location = barArea.Minimum;
                    _scrollBarKnob.Size = barArea.Size;

                    return;
                }

                float barSize = barArea.Width * ratio;

                float start = barArea.Left + barArea.Width * Math.Min(1, Scroll / ContentSize);

                start = Math.Max(barArea.Left, Math.Min(barArea.Right, start));

                _scrollBarKnob.Location = new Vector(start, barArea.Top);
                _scrollBarKnob.Size = new Vector(barSize, barArea.Height);
            }
        }

        private void Layout()
        {
            switch (_orientation)
            {
                case ScrollBarOrientation.Vertical:
                    _decreaseScroll.Size = new Vector(Bounds.Width, Bounds.Width);
                    _increaseScroll.Size = new Vector(Bounds.Width, Bounds.Width);

                    _decreaseScroll.Location = Vector.Zero;
                    _increaseScroll.Location = new Vector(0, Bounds.Bottom - _increaseScroll.Size.Y);

                    _scrollBarArea.Location = new Vector(0, Bounds.Width);
                    _scrollBarArea.Size = new Vector(Bounds.Width, Bounds.Height - Bounds.Width * 2);
                    break;

                case ScrollBarOrientation.Horizontal:
                    _decreaseScroll.Size = new Vector(Bounds.Height, Bounds.Height);
                    _increaseScroll.Size = new Vector(Bounds.Height, Bounds.Height);

                    _decreaseScroll.Location = Vector.Zero;
                    _increaseScroll.Location = new Vector(Bounds.Right - _increaseScroll.Size.X, 0);

                    _scrollBarArea.Location = new Vector(Bounds.Height, 0);
                    _scrollBarArea.Size = new Vector(Bounds.Width - Bounds.Height * 2, Bounds.Height);
                    break;
            }
        }

        private void SetupScrollBarColors()
        {
            CornerRadius = Size.X / 3;

            var baseColor = ScrollBarStyle.ScrollBarBackgroundColor();
            var barColor = ScrollBarStyle.ScrollBarKnobNormalColor();

            UpdateButtonColor(_increaseScroll);
            UpdateButtonColor(_decreaseScroll);
            BackColor = baseColor;
            _scrollBarKnob.BackColor = barColor;

            void UpdateButtonColor(ButtonControl button)
            {
                button.NormalColor = ScrollBarStyle.ScrollBarButtonNormalColor();
                button.HighlightColor = ScrollBarStyle.ScrollBarButtonHighlightColor();
                button.SelectedColor = ScrollBarStyle.ScrollBarButtonPressedColor();
            }
        }
        
        /// <summary>
        /// Orientation of a scroll bar
        /// </summary>
        public enum ScrollBarOrientation
        {
            Vertical,
            Horizontal
        }
    }

    /// <summary>
    /// Interface for objects that provide styles for scroll bars
    /// </summary>
    internal interface IScrollBarControlStyle
    {
        /// <summary>
        /// Background color for scroll bar
        /// </summary>
        Color ScrollBarBackgroundColor();

        /// <summary>
        /// Color for scrollbar knob when not pressed down and not
        /// hovered.
        /// </summary>
        Color ScrollBarKnobNormalColor();

        /// <summary>
        /// Color for scrollbar knob when not pressed down and
        /// hovered.
        /// </summary>
        Color ScrollBarKnobHighlightColor();

        /// <summary>
        /// Color for scrollbar knob when pressed down and
        /// hovered.
        /// </summary>
        Color ScrollBarKnobPressedColor();

        /// <summary>
        /// Color for scrollbar increase/decrease button when not 
        /// pressed down and not hovered.
        /// </summary>
        Color ScrollBarButtonNormalColor();

        /// <summary>
        /// Color for scrollbar increase/decrease button when not 
        /// pressed down and hovered.
        /// </summary>
        Color ScrollBarButtonHighlightColor();

        /// <summary>
        /// Color for scrollbar increase/decrease button when
        /// pressed down and hovered.
        /// </summary>
        Color ScrollBarButtonPressedColor();
    }

    internal class DarkScrollBarControlStyle : IScrollBarControlStyle
    {
        public Color ScrollBarBackgroundColor() => Color.DimGray;

        public Color ScrollBarKnobNormalColor() => Color.DarkGray;

        public Color ScrollBarKnobHighlightColor()
        {
            return ScrollBarKnobNormalColor().Faded(Color.White, 0.2f);
        }

        public Color ScrollBarKnobPressedColor()
        {
            return ScrollBarKnobNormalColor().Faded(Color.Black, 0.2f);
        }

        public Color ScrollBarButtonNormalColor() => Color.DimGray;

        public Color ScrollBarButtonHighlightColor()
        {
            return ScrollBarButtonNormalColor().Faded(Color.White, 0.2f);
        }

        public Color ScrollBarButtonPressedColor()
        {
            return ScrollBarButtonNormalColor().Faded(Color.Black, 0.2f);
        }
    }
}
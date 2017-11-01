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
using System.Drawing;

using JetBrains.Annotations;

using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A scroll bar control that allows scrolling 
    /// </summary>
    internal sealed class ScrollBarControl : ControlView
    {
        private readonly ButtonControl _decreaseScroll = new ButtonControl();
        private readonly ButtonControl _increaseScroll = new ButtonControl();

        private readonly ControlView _scrollBarKnob = new ControlView();
        private readonly ControlView _scrollBarArea = new ControlView();

        private ScrollBarStyles _scrollBarStyle = ScrollBarStyles.Dark;
        private ScrollBarOrientation _orientation = ScrollBarOrientation.Vertical;
        private float _contentSize;
        private float _visibleSize;
        private float _scroll;

        public ScrollBarStyles ScrollBarStyle
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
            SetupScrollBarColors();

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

            var dragStart = Vector.Zero;
            float scrollStart = Scroll;

            mouseDrag.DragMouseEvent += (sender, args) =>
            {
                switch (args.State)
                {
                    case DragMouseEventRecognizer.DragMouseEventState.MousePressed:
                        dragStart = args.MousePosition;
                        scrollStart = Scroll;
                        break;
                    case DragMouseEventRecognizer.DragMouseEventState.MouseMoved:
                        // Calculate scroll magnitude
                        float magnitude = ContentSize / VisibleSize;

                        var offset = args.MousePosition - dragStart;
                        float axis = Orientation == ScrollBarOrientation.Horizontal ? offset.X : offset.Y;

                        Scroll = scrollStart + axis * magnitude;
                        ScrollChanged?.Invoke(this, EventArgs.Empty);
                        break;
                }
            };
        }

        private void SetupRepeatFire([NotNull] ControlView button, [NotNull] Action onFire)
        {
            button.Rx
                .MouseDownRepeating(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(50))
                .Subscribe(_ =>
                {
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

            Color baseColor;
            Color barColor;

            if (ScrollBarStyle == ScrollBarStyles.Light)
            {
                baseColor = Color.White;
                barColor = Color.LightGray;
            }
            else
            {
                baseColor = Color.DimGray;
                barColor = Color.DarkGray;
            }

            UpdateButtonColor(_increaseScroll, baseColor);
            UpdateButtonColor(_decreaseScroll, baseColor);
            BackColor = baseColor;
            _scrollBarKnob.BackColor = barColor;

            void UpdateButtonColor(ButtonControl button, Color color)
            {
                button.NormalColor = color;

                if (ScrollBarStyle == ScrollBarStyles.Light)
                {
                    button.HighlightColor = color.Blend(Color.Black, 0.2f);
                }
                else
                {
                    button.HighlightColor = color.Blend(Color.White, 0.2f);
                }
            }
        }

        /// <summary>
        /// Specifies the color style of the scroll bar
        /// </summary>
        public enum ScrollBarStyles
        {
            Light,
            Dark
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
}
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
using System.Windows.Forms;
using PixUI.Utils;

namespace PixUI.Controls
{
    /// <summary>
    /// A container view which scrolls to allow panning contents into a rectangular view
    /// </summary>
    internal class ScrollViewControl : ControlView
    {
        private const float ScrollBarSize = 20;
        
        private Vector _contentSize;
        private Vector _contentOffset;

        private Vector _targetContentOffset;
        private VisibleScrollBars _scrollBarsMode;

        /// <summary>
        /// Gets or sets a value specifying the scroll bar visbiility mode of this scroll view control.
        /// </summary>
        public VisibleScrollBars ScrollBarsMode
        {
            get => _scrollBarsMode;
            set
            {
                _scrollBarsMode = value;

                switch (value)
                {
                    case VisibleScrollBars.Both:
                        HorizontalBar.Visible = true;
                        VerticalBar.Visible = true;
                        break;
                    case VisibleScrollBars.Horizontal:
                        HorizontalBar.Visible = true;
                        VerticalBar.Visible = false;
                        break;
                    case VisibleScrollBars.Vertical:
                        HorizontalBar.Visible = false;
                        VerticalBar.Visible = true;
                        break;
                    case VisibleScrollBars.None:
                        HorizontalBar.Visible = false;
                        VerticalBar.Visible = false;
                        break;
                }

                UpdateScrollBarPositions();
            }
        }

        /// <summary>
        /// Velocity of acceleration for this scroll view's scrolling.
        /// This value decreases to Vector.Zero over time, or clips to zero
        /// across coordinates that hit the scrolling bounds limits.
        /// 
        /// Updated every <see cref="ControlView.OnFixedFrame"/> call.
        /// </summary>
        public Vector Velocity { get; set; }
        
        /// <summary>
        /// Total size of the larget inner contents of this scroll view
        /// </summary>
        public Vector ContentSize
        {
            get => _contentSize;
            set
            {
                _contentSize = value;
                LimitContentOffset();

                HorizontalBar.ContentSize = _contentSize.X;
                VerticalBar.ContentSize = _contentSize.Y;
                ContainerView.Size = ContentSize;
            }
        }
        
        /// <summary>
        /// View to add scrollable content to
        /// </summary>
        public BaseView ContainerView { get; } = new BaseView();

        public override AABB ContentBounds => new AABB(_contentOffset, _contentOffset + ContentSize);

        /// <summary>
        /// Gets the visible content area of the <see cref="ContainerView"/> which is not occluded by
        /// scroll bars.
        /// 
        /// If no scroll bars are visible, <see cref="VisibleContentBounds"/> is the same as <see cref="BaseView.Bounds"/>.
        /// </summary>
        public AABB VisibleContentBounds
        {
            get
            {
                var final = Bounds;

                if (ScrollBarsMode.HasFlag(VisibleScrollBars.Vertical))
                {
                    final = final.Inset(new InsetBounds(0, 0, 0, ScrollBarSize));
                }
                if (ScrollBarsMode.HasFlag(VisibleScrollBars.Horizontal))
                {
                    final = final.Inset(new InsetBounds(0, 0, ScrollBarSize, 0));
                }

                return final;
            }
        }

        /// <summary>
        /// Gets the horizontal scroll bar for this scroll view
        /// </summary>
        public ScrollBarControl HorizontalBar { get; } = new ScrollBarControl();

        /// <summary>
        /// Gets the vertical scroll bar for this scroll view
        /// </summary>
        public ScrollBarControl VerticalBar { get; } = new ScrollBarControl();

        public ScrollViewControl()
        {
            UpdateScrollBarPositions();

            base.AddChild(ContainerView);
            base.AddChild(HorizontalBar);
            base.AddChild(VerticalBar);

            HorizontalBar.ScrollChanged += HorizontalScrollChanged;
            VerticalBar.ScrollChanged += VerticalScrollChanged;

            HorizontalBar.Orientation = ScrollBarControl.ScrollBarOrientation.Horizontal;
            
            HorizontalBar.VisibleSize = Bounds.Width;
            VerticalBar.VisibleSize = Bounds.Height;

            ScrollBarsMode = VisibleScrollBars.Both;
        }

        private void HorizontalScrollChanged(object sender, EventArgs eventArgs)
        {
            _targetContentOffset = new Vector(-HorizontalBar.Scroll, _targetContentOffset.Y);
            _contentOffset = new Vector(-HorizontalBar.Scroll, _contentOffset.Y);
        }

        private void VerticalScrollChanged(object sender, EventArgs eventArgs)
        {
            _targetContentOffset = new Vector(_targetContentOffset.X, -VerticalBar.Scroll);
            _contentOffset = new Vector(_contentOffset.X, -VerticalBar.Scroll);
        }

        public override void AddChild(BaseView view)
        {
            throw new InvalidOperationException("Use ScrollViewControl.ContentView.AddChild() instead.");
        }

        public override void RemoveChild(BaseView child)
        {
            throw new InvalidOperationException("Use ScrollViewControl.ContentView.RemoveChild() instead.");
        }

        public override void OnFixedFrame(FixedFrameEventArgs e)
        {
            base.OnFixedFrame(e);

            if (_contentOffset.Distance(_targetContentOffset) > 0.1f)
            {
                _contentOffset += (_targetContentOffset - _contentOffset) * 0.3f;
                UpdateScrollBarPositions();
            }
            else
            {
                _contentOffset = _targetContentOffset;
            }

            ContainerView.Location = _contentOffset;
            
            HorizontalBar.Scroll = -_contentOffset.X;
            VerticalBar.Scroll = -_contentOffset.Y;
        }

        public override bool CanHandle(IEventRequest eventRequest)
        {
            if (eventRequest is IMouseEventRequest mouseEvent && mouseEvent.EventType == MouseEventType.MouseWheel)
                return true;
            
            return base.CanHandle(eventRequest);
        }

        public override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (e.Delta == 0)
                return;

            // Scroll contents a fixed ammount
            IncrementContentOffset(new Vector(0, Math.Sign(e.Delta) * 50));
        }
        
        protected void IncrementContentOffset(Vector offset)
        {
            if (offset == Vector.Zero)
                return;

            _targetContentOffset += offset;

            LimitContentOffset();
            UpdateScrollBarPositions();
        }
        
        private void LimitContentOffset()
        {
            // Limit content offset within a maximum visible bounds
            var contentOffsetClip = new AABB(-(ContentBounds.Size - Bounds.Size), Vector.Zero);
            _targetContentOffset = _targetContentOffset.LimitedWithin(contentOffsetClip);
        }

        private void UpdateScrollBarPositions()
        {
            if (ScrollBarsMode == VisibleScrollBars.Vertical || ScrollBarsMode == VisibleScrollBars.Both)
            {
                var vertReg = AABB.FromRectangle(Bounds.Width - ScrollBarSize, 0, ScrollBarSize, Bounds.Height);
                vertReg = vertReg.Inset(new InsetBounds(-2, 2, 2, 2));

                // Pad common square at bottom-right corners between both scroll bars
                if (ScrollBarsMode == VisibleScrollBars.Both)
                    vertReg = vertReg.Inset(new InsetBounds(0, 0, ScrollBarSize, 0));

                VerticalBar.Location = vertReg.Minimum;
                VerticalBar.Size = vertReg.Size;
            }
            
            if (ScrollBarsMode == VisibleScrollBars.Horizontal || ScrollBarsMode == VisibleScrollBars.Both)
            {
                var horReg = AABB.FromRectangle(0, Bounds.Height - ScrollBarSize, Bounds.Width, ScrollBarSize);
                horReg = horReg.Inset(new InsetBounds(2, -2, 2, 2));

                // Pad common square at bottom-right corners between both scroll bars
                if (ScrollBarsMode == VisibleScrollBars.Both)
                    horReg = horReg.Inset(new InsetBounds(0, 0, 0, ScrollBarSize));
                
                HorizontalBar.Location = horReg.Minimum;
                HorizontalBar.Size = horReg.Size;
            }
        }

        protected override void OnResize()
        {
            base.OnResize();

            ContainerView.Location = ContentBounds.Minimum;
            ContainerView.Size = ContentBounds.Size;

            UpdateScrollBarPositions();
            
            HorizontalBar.VisibleSize = Bounds.Width;
            VerticalBar.VisibleSize = Bounds.Height;
        }

        /// <summary>
        /// Specifies which scroll bars to display on a <see cref="ScrollViewControl"/>
        /// </summary>
        [Flags]
        public enum VisibleScrollBars
        {
            Vertical = 0b1,
            Horizontal = 0b10,
            Both = 0b11,
            None = 0
        }
    }
}
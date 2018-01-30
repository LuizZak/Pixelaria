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
using PixCore.Geometry;

namespace PixUI.Controls
{
    /// <summary>
    /// A container view which scrolls to allow panning contents into a rectangular view
    /// </summary>
    public class ScrollViewControl : ControlView
    {
        private const float ScrollBarSize = 20;
        
        protected Vector contentSize;
        protected Vector contentOffset;

        protected Vector targetContentOffset;
        private VisibleScrollBars _scrollBarsMode;
        private bool _scrollBarsAlwaysVisible;

        /// <summary>
        /// Gets or sets a value specifying the scroll bar visbiility mode of this scroll view control.
        /// </summary>
        public VisibleScrollBars ScrollBarsMode
        {
            get => _scrollBarsMode;
            set
            {
                _scrollBarsMode = value;

                UpdateScrollBarVisibility();
                UpdateScrollBarPositions();
            }
        }

        /// <summary>
        /// Gets or sets a value specifying whether the scroll bars should always be visible, even when the
        /// content bounds don't exceed the size of the scroll view.
        /// </summary>
        public bool ScrollBarsAlwaysVisible
        {
            get => _scrollBarsAlwaysVisible;
            set
            {
                _scrollBarsAlwaysVisible = value;
                UpdateScrollBarVisibility();
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
            get => contentSize;
            set
            {
                contentSize = value;
                Layout();
            }
        }
        
        /// <summary>
        /// View to add scrollable content to
        /// </summary>
        public BaseView ContainerView { get; } = new BaseView();

        public override AABB ContentBounds => new AABB(contentOffset, contentOffset + EffectiveContentSize());

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

                if (ScrollBarsMode.HasFlag(VisibleScrollBars.Vertical) && VerticalBar.Visible)
                {
                    final = final.Inset(new InsetBounds(0, 0, 0, ScrollBarSize));
                }
                if (ScrollBarsMode.HasFlag(VisibleScrollBars.Horizontal) && HorizontalBar.Visible)
                {
                    final = final.Inset(new InsetBounds(0, 0, ScrollBarSize, 0));
                }

                return final;
            }
        }

        /// <summary>
        /// Gets the horizontal scroll bar for this scroll view
        /// </summary>
        public ScrollBarControl HorizontalBar { get; } = ScrollBarControl.Create();

        /// <summary>
        /// Gets the vertical scroll bar for this scroll view
        /// </summary>
        public ScrollBarControl VerticalBar { get; } = ScrollBarControl.Create();

        /// <summary>
        /// Creates a new instance of <see cref="ScrollViewControl"/>
        /// </summary>
        public static ScrollViewControl Create()
        {
            var control = new ScrollViewControl();

            control.Initialize();

            return control;
        }

        protected ScrollViewControl()
        {
            
        }

        protected virtual void Initialize()
        {
            _scrollBarsAlwaysVisible = false;

            UpdateScrollBarPositions();
            UpdateScrollBarVisibility();

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
            targetContentOffset = new Vector(-HorizontalBar.Scroll, targetContentOffset.Y);
            contentOffset = new Vector(-HorizontalBar.Scroll, contentOffset.Y);
        }

        private void VerticalScrollChanged(object sender, EventArgs eventArgs)
        {
            targetContentOffset = new Vector(targetContentOffset.X, -VerticalBar.Scroll);
            contentOffset = new Vector(contentOffset.X, -VerticalBar.Scroll);
        }

        public override void AddChild(BaseView view)
        {
            ContainerView.AddChild(view);
        }

        public override void RemoveChild(BaseView child)
        {
            ContainerView.RemoveChild(child);
        }

        public override void OnFixedFrame(FixedFrameEventArgs e)
        {
            base.OnFixedFrame(e);

            if (contentOffset.Distance(targetContentOffset) > 0.1f)
            {
                contentOffset += (targetContentOffset - contentOffset) * 0.3f;
                UpdateScrollBarPositions();
            }
            else
            {
                contentOffset = targetContentOffset;
            }

            if (ContainerView.Location != contentOffset)
            {
                ContainerView.Location = contentOffset;
            }
            
            HorizontalBar.Scroll = -contentOffset.X;
            VerticalBar.Scroll = -contentOffset.Y;
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

            targetContentOffset += offset;

            LimitContentOffset();
            UpdateScrollBarPositions();
        }
        
        private void LimitContentOffset()
        {
            // Limit content offset within a maximum visible bounds
            var contentOffsetClip = new AABB(-(ContentBounds.Size - Bounds.Size), Vector.Zero);

            if (ContentBounds.Width <= Bounds.Width)
            {
                targetContentOffset = new Vector(0, targetContentOffset.Y);
            }
            if (ContentBounds.Height <= Bounds.Height)
            {
                targetContentOffset = new Vector(targetContentOffset.X, 0);
            }

            if (ContentBounds.Width > Bounds.Width || ContentBounds.Height > Bounds.Height)
            {
                targetContentOffset = targetContentOffset.LimitedWithin(contentOffsetClip);
            }
        }

        private void UpdateScrollBarVisibility()
        {
            if(_scrollBarsAlwaysVisible)
            {
                switch (_scrollBarsMode)
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
                
                return;
            }

            var horizontalBarVisible = ContentSize.X > Width - VerticalBar.Width;
            var verticalBarVisible = ContentSize.Y > Height - HorizontalBar.Height;

            switch (_scrollBarsMode)
            {
                case VisibleScrollBars.Both:
                    HorizontalBar.Visible = horizontalBarVisible;
                    VerticalBar.Visible = verticalBarVisible;
                    break;
                case VisibleScrollBars.Horizontal:
                    HorizontalBar.Visible = horizontalBarVisible;
                    VerticalBar.Visible = false;
                    break;
                case VisibleScrollBars.Vertical:
                    HorizontalBar.Visible = false;
                    VerticalBar.Visible = verticalBarVisible;
                    break;
                case VisibleScrollBars.None:
                    HorizontalBar.Visible = false;
                    VerticalBar.Visible = false;
                    break;
            }
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

        public override void Layout()
        {
            base.Layout();

            UpdateScrollBarVisibility();
            LimitContentOffset();

            HorizontalBar.ContentSize = contentSize.X;
            VerticalBar.ContentSize = contentSize.Y;
            ContainerView.Size = EffectiveContentSize();
        }

        /// <summary>
        /// Gets the effective content size of this scroll view by replacing 0's in <see cref="ContentSize"/>
        /// with the maximum effective size available based on the control's size and visible scroll bars.
        /// </summary>
        private Vector EffectiveContentSize()
        {
            var bounds = VisibleContentBounds;
            float width = Math.Abs(ContentSize.X) < float.Epsilon ? bounds.Width : ContentSize.X;
            float height = Math.Abs(ContentSize.Y) < float.Epsilon ? bounds.Height : ContentSize.Y;

            return new Vector(width, height);
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
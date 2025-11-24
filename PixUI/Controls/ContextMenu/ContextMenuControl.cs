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

using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixRendering;
using PixUI.LayoutSystem;
using PixUI.Utils.Layout;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PixUI.Controls.ContextMenu
{
    /// <summary>
    /// An inline context menu-like control
    /// </summary>
    public class ContextMenuControl: ControlView, IDialogControl
    {
        public static Font DefaultItemFont = new Font(FontFamily.GenericSansSerif, 14);

        private const float LeftMarginWidth = 24;

        private List<ContextMenuItemViewBase> _itemViews;

        private ContextMenuDropDownItem _rootItem;

        /// <summary>
        /// The innermost visible context menu item currently displayed.
        ///
        /// When set, this menu, along all parent items are made visible on screen.
        /// </summary>
        private ContextMenuItem _visibleItem;

        public event DialogControlClosing Closing;

        public event DialogControlClosed Closed;

        public event EventHandler Opened;

        public override bool CanBecomeFirstResponder => true;

        public DialogControlContextFlags DialogContextFlags => DialogControlContextFlags.ContextMenu;

        public static ContextMenuControl Create(ContextMenuDropDownItem rootItem)
        {
            var control = new ContextMenuControl(rootItem);
            control.Initialize();

            return control;
        }

        protected ContextMenuControl(ContextMenuDropDownItem rootItem)
        {
            _rootItem = rootItem;
            _visibleItem = rootItem;

            SetNeedsLayout();
        }

        protected virtual void Initialize()
        {
            BackColor = Color.Black;
            StrokeColor = Color.Transparent;

            _itemViews = new List<ContextMenuItemViewBase>();
            RecreateItemViews();
        }

        /// <summary>
        /// Raises the <see cref="Closing"/> and later <see cref="Closed"/> event.
        /// </summary>
        protected virtual void OnClose(DialogControlCloseReason reason)
        {
            Closing?.Invoke(this, reason);
            Closed?.Invoke(this, reason);
        }

        public void Close()
        {
            Close(DialogControlCloseReason.CloseCalled);
        }

        public void Close(DialogControlCloseReason reason)
        {
            OnClose(reason);
        }

        public void Show()
        {
            Opened?.Invoke(this, EventArgs.Empty);
        }

        private void RecreateItemViews()
        {
            foreach (var itemView in _itemViews)
            {
                itemView.RemoveFromParent();
            }

            _itemViews.Clear();

            foreach (var item in _rootItem.DropDownItems)
            {
                var itemViewBase = item.CreateContextMenuItemView();

                AddChild(itemViewBase);

                _itemViews.Add(itemViewBase);

                if (item is ContextMenuItem menuItem)
                {
                    menuItem.SelectChange += (sender, e) =>
                    {
                        var itemView = (itemViewBase as ContextMenuItemView);

                        if (e)
                        {
                            // Deselect all other items
                            foreach (var selectableItemView in SelectableItemViews())
                            {
                                if (selectableItemView == itemViewBase)
                                    continue;

                                selectableItemView.Selected = false;
                            }
                            foreach (var selectableItem in SelectableItems())
                            {
                                if (selectableItem == item)
                                    continue;

                                selectableItem.Selected = false;
                            }

                            if (itemView != null)
                                itemView.Selected = true;
                        }
                        else
                        {
                            if (itemView != null)
                                itemView.Selected = false;
                        }
                    };

                    menuItem.Click += (sender, e) =>
                    {
                        Close(DialogControlCloseReason.ItemClicked);
                    };
                }
            }
        }

        private List<ContextMenuItem> VisibleItems()
        {
            var items = new List<ContextMenuItem>();
            var current = _visibleItem;
            while (current != null)
            {
                items.Add(current);

                current = current.DropDownItem;
            }

            return items;
        }

        private IEnumerable<ContextMenuItemView> SelectableItemViews()
        {
            return _itemViews.OfType<ContextMenuItemView>();
        }

        private IEnumerable<ContextMenuItem> SelectableItems()
        {
            var result = new List<ContextMenuItem>();

            foreach (var itemBase in _rootItem.DropDownItems)
            {
                if (itemBase is ContextMenuItem item)
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public override void RenderBackground(ControlRenderingContext context)
        {
            base.RenderBackground(context);

            var area = Bounds.WithSize(LeftMarginWidth, Bounds.Height).Inset(1);

            var bodyFillBrush = context.Renderer.CreateLinearGradientBrush(new[]
            {
                new PixGradientStop(BackColor.Faded(Color.White, 0.2f), 0),
                new PixGradientStop(BackColor, 1)
            }, area.TopLeft, area.BottomLeft);

            context.Renderer.SetFillBrush(bodyFillBrush);
            context.Renderer.FillArea(area);
        }

        public override void Layout()
        {
            if (!needsLayout)
                return;

            base.Layout();

            foreach (var itemView in _itemViews.Where(view => view.Visible))
            {
                itemView.AutoSize();
            }

            float maxWidth = _itemViews.Where(view => view.Visible).Aggregate(LeftMarginWidth, (d, view) => Math.Max(d, view.Bounds.Width));
            float y = 0.0f;

            foreach (var itemView in _itemViews.Where(view => view.Visible))
            {
                itemView.Size = new Vector(maxWidth, itemView.Size.Y);
                itemView.Location = new Vector(0, y);
                y += itemView.Height;
            }

            AutoSize();
        }

        public void AutoSize()
        {
            float maxWidth = _itemViews.Where(view => view.Visible).Aggregate(LeftMarginWidth, (d, view) => Math.Max(d, view.Bounds.Width));
            float totalHeight = _itemViews.Where(view => view.Visible).Aggregate(0.0f, (d, view) => d + view.Bounds.Height);

            Size = new Vector(maxWidth, totalHeight);
        }

        /// <summary>
        /// Requests the bounding size for displaying an item at a given index on this context menu control.
        ///
        /// Returns <see cref="AABB.Invalid"/> in case the item is not visible.
        /// </summary>
        public AABB BoundsForItem([NotNull] ContextMenuItem item)
        {
            if (item.DropDownItem != _rootItem)
                return AABB.Invalid;

            return _itemViews[item.Index].FrameOnParent;
        }

        internal class ContextMenuItemViewBase : ControlView
        {
            private readonly ContextMenuItemBase _item;

            protected ContextMenuItemViewBase(ContextMenuItemBase item)
            {
                _item = item;
                Visible = _item.Visible;

                _item.VisibleChanged += (sender, visible) =>
                {
                    Visible = visible;
                    ParentContextMenu()?.SetNeedsLayout();
                    ParentContextMenu()?.Layout();
                };
            }

            public virtual void AutoSize()
            {

            }

            protected virtual AABB BoundsForSelectionHighlight()
            {
                return Bounds.Inflated(0, 0);
            }

            [CanBeNull]
            protected ContextMenuControl ParentContextMenu()
            {
                BaseView view = this;

                while (view != null)
                {
                    if (view is ContextMenuControl contextMenuControl)
                        return contextMenuControl;

                    view = view.Parent;
                }

                return null;
            }
        }

        internal class ContextMenuControlHostItemView : ContextMenuItemViewBase
        {
            private readonly ContextMenuControlHostItem _item;

            public static ContextMenuControlHostItemView Create([NotNull] ContextMenuControlHostItem item)
            {
                var view = new ContextMenuControlHostItemView(item);
                view.Initialize();

                return view;
            }

            protected ContextMenuControlHostItemView(ContextMenuControlHostItem item) : base(item)
            {
                _item = item;
            }

            protected void Initialize()
            {
                MouseOverHighlight = true;
                BackColor = Color.Transparent;
                StrokeColor = Color.Transparent;

                AddChild(_item.control);

                if (_item.CreateConstraints)
                {
                    LayoutConstraint.Create(_item.control.Anchors.Left, Anchors.Left, LayoutRelationship.Equal, constant: LeftMarginWidth + 4);
                    LayoutConstraint.Create(_item.control.Anchors.Right, Anchors.Right, LayoutRelationship.LessThanOrEqual, constant: -4);
                    LayoutConstraint.Create(_item.control.Anchors.Top, Anchors.Top, LayoutRelationship.Equal, constant: 4);
                    LayoutConstraint.Create(_item.control.Anchors.Bottom, Anchors.Bottom, LayoutRelationship.Equal, constant: -4);
                    LayoutConstraint.Create(_item.control.Anchors.Width, LayoutRelationship.GreaterThanOrEqual, constant: _item.MinimumWidth);
                }

                Layout();
                AutoSize();
            }

            public override void Layout()
            {
                base.Layout();

                if (!_item.CreateConstraints)
                {
                    _item.control.Layout();
                    _item.control.Location = new Vector(LeftMarginWidth + 4, Height / 2 - _item.control.Height / 2);
                }
            }

            public override void AutoSize()
            {
                if (!_item.CreateConstraints)
                {
                    Size = Bounds.Union(_item.control.FrameOnParent).Size;
                }
            }
        }

        internal class ContextMenuSeparatorItemView : ContextMenuItemViewBase
        {
            private readonly ContextMenuSeparatorItem _item;

            public static ContextMenuSeparatorItemView Create([NotNull] ContextMenuSeparatorItem item)
            {
                var view = new ContextMenuSeparatorItemView(item);
                view.Initialize();

                return view;
            }

            protected ContextMenuSeparatorItemView(ContextMenuSeparatorItem item) : base(item)
            {
                _item = item;
            }

            protected void Initialize()
            {
                MouseOverHighlight = false;
                BackColor = Color.Transparent;
                StrokeColor = Color.Transparent;

                AutoSize();
                Layout();
            }

            public override void AutoSize()
            {
                Size = new Vector(0, 8);
            }

            public override void RenderBackground(ControlRenderingContext context)
            {
                base.RenderBackground(context);

                context.Renderer.SetStrokeColor(Color.DimGray);
                context.Renderer.StrokeLine(new Vector(LeftMarginWidth + 4, Bounds.Height / 2), new Vector(Bounds.Width - 8, Bounds.Height / 2));
            }
        }

        internal class ContextMenuItemView : ContextMenuItemViewBase
        {
            private const float SubItemsArrowBounds = 16;
            private static readonly Vector SubItemsArrowSize = new Vector(6, 8);

            private readonly ContextMenuItem _item;

            private readonly LabelViewControl _label;
            private readonly ImageViewControl _imageView;

            private bool HasSubItems => (_item as ContextMenuDropDownItem)?.DropDownItems.Count > 0;

            public static ContextMenuItemView Create([NotNull] ContextMenuItem item)
            {
                var view = new ContextMenuItemView(item);
                view.Initialize();

                return view;
            }

            protected ContextMenuItemView(ContextMenuItem item) : base(item)
            {
                _item = item;
                _label = LabelViewControl.Create(_item.Name);
                if (_item.Image != null)
                {
                    _imageView = ImageViewControl.Create(_item.Image.Value);
                    _imageView.InteractionEnabled = false;
                }
                else
                {
                    _imageView = ImageViewControl.Create(_item.ManagedImage);
                    _imageView.InteractionEnabled = false;
                }
            }
            
            protected override void OnChangedState(ControlViewState newState)
            {
                base.OnChangedState(newState);

                Invalidate();
            }

            protected void Initialize()
            {
                MouseOverHighlight = true;
                BackColor = Color.Transparent;
                StrokeColor = Color.Transparent;

                _label.AutoResize = true;
                _label.TextFont = DefaultItemFont;
                _label.ForeColor = Color.White;
                _label.StrokeColor = Color.Transparent;
                _label.BackColor = Color.Transparent;

                _imageView.StrokeColor = Color.Transparent;
                _imageView.BackColor = Color.Transparent;

                AddChild(_label);
                AddChild(_imageView);

                SetupEvents();

                AutoSize();
                Layout();
            }

            protected void SetupEvents()
            {
                _item.AttributedNameChanged += (sender, name) =>
                {
                    _label.AttributedText = name;

                    AutoSize();
                    Layout();

                    ParentContextMenu()?.SetNeedsLayout();
                    ParentContextMenu()?.Layout();
                };
            }

            public override void OnMouseEnter()
            {
                base.OnMouseEnter();

                _item.OnMouseEnter(this, new EventArgs());
            }

            public override void OnMouseLeave()
            {
                base.OnMouseLeave();

                _item.OnMouseLeave(this, new EventArgs());
            }

            public override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);

                _item.OnClick(this, EventArgs.Empty);
            }

            public override void Layout()
            {
                base.Layout();

                _label.Location = new Vector(LeftMarginWidth + 4, Height / 2 - _label.Height / 2);
                _imageView.Size = new Vector(16, 16);
                _imageView.Center = new Vector(LeftMarginWidth / 2, Height / 2);
            }

            public override void RenderBackground(ControlRenderingContext context)
            {
                base.RenderBackground(context);

                if (Highlighted || Selected)
                {
                    var bounds = BoundsForSelectionHighlight();

                    context.Renderer.SetFillColor(Color.DodgerBlue.WithTransparency(0.5f));
                    context.Renderer.SetStrokeColor(Color.LightBlue);

                    context.Renderer.FillArea(bounds);
                    context.Renderer.StrokeArea(bounds);
                }

                if (HasSubItems)
                {
                    var bounds = BoundsForSubItemsArrow();

                    var path = context.Renderer.CreatePath(p =>
                    {
                        p.MoveTo(bounds.Left, bounds.Top);
                        p.LineTo(bounds.Right, bounds.Center.Y);
                        p.LineTo(bounds.Left, bounds.Bottom);
                        p.EndFigure(true);
                    });

                    context.Renderer.SetFillColor(Color.White);
                    context.Renderer.FillPath(path);
                }
            }

            public override void AutoSize()
            {
                _label.AutoSize();

                Size = new Vector(_label.Bounds.Width + 38 + (HasSubItems ? SubItemsArrowBounds : 0), Math.Max(24, _label.Bounds.Height + 12));
            }

            protected override AABB BoundsForSelectionHighlight()
            {
                return Bounds.Inflated(0, 0);
            }

            private AABB BoundsForSubItemsArrow()
            {
                var totalBounds = new AABB(_label.FrameOnParent.Right, 0, Height, Width);
                var arrowBounds = AABB.FromRectangle(Vector.Zero, SubItemsArrowSize);
                
                return arrowBounds.WithCenterOn(totalBounds.Center);
            }
        }
    }
}

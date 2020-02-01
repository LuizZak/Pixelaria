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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixRendering;
using PixUI.Utils.Layout;

namespace PixUI.Controls.ContextMenu
{
    /// <summary>
    /// An inline context menu-like control
    /// </summary>
    public class ContextMenuControl: ControlView
    {
        private const float LeftMarginWidth = 24;

        private List<ContextMenuItemView> _itemViews;

        private ContextMenuDropDownItem _rootItem;

        /// <summary>
        /// The innermost visible context menu item currently displayed.
        ///
        /// When set, this menu, along all parent items are made visible on screen.
        /// </summary>
        private ContextMenuItem _visibleItem;

        public override bool CanBecomeFirstResponder => true;

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
        }

        protected virtual void Initialize()
        {
            BackColor = Color.Black;
            StrokeColor = Color.Transparent;

            _itemViews = new List<ContextMenuItemView>();
            RecreateItemViews();
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
                var itemView = ContextMenuItemView.Create(item);

                AddChild(itemView);

                _itemViews.Add(itemView);
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

        public override void RenderBackground(ControlRenderingContext context)
        {
            base.RenderBackground(context);

            var bodyFillBrush = context.Renderer.CreateLinearGradientBrush(new[]
            {
                new PixGradientStop(BackColor.Faded(Color.White, 0.2f), 0),
                new PixGradientStop(BackColor, 1)
            }, Vector.Zero, new Vector(0, Bounds.Height));

            context.Renderer.SetFillBrush(bodyFillBrush);
            context.Renderer.FillArea(Bounds.WithSize(LeftMarginWidth, Bounds.Height));
        }

        public override void Layout()
        {
            if (!needsLayout)
                return;

            base.Layout();

            float maxWidth = _itemViews.Aggregate(24.0f, (d, view) => Math.Max(d, view.Bounds.Width));
            float y = 0.0f;

            foreach (var itemView in _itemViews)
            {
                itemView.Size = new Vector(maxWidth, itemView.Size.Y);
                itemView.Location = new Vector(0, y);
                y += itemView.Height;
            }
        }

        public void AutoSize()
        {
            float maxWidth = _itemViews.Aggregate(24.0f, (d, view) => Math.Max(d, view.Bounds.Width));
            float totalHeight = _itemViews.Aggregate(0.0f, (d, view) => d + view.Bounds.Height);

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

        private class ContextMenuItemView : ControlView
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

            protected ContextMenuItemView(ContextMenuItem item)
            {
                _item = item;
                _label = LabelViewControl.Create(_item.Name);
                if (_item.Image != null)
                    _imageView = ImageViewControl.Create(_item.Image.Value);
                else
                    _imageView = ImageViewControl.Create(_item.ManagedImage);
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
                _label.TextFont = new Font(FontFamily.GenericSansSerif, 14);
                _label.ForeColor = Color.White;
                _label.StrokeColor = Color.Transparent;
                _label.BackColor = Color.Transparent;

                _imageView.StrokeColor = Color.Transparent;
                _imageView.BackColor = Color.Transparent;

                if(!IsSeparator())
                {
                    AddChild(_label);
                    AddChild(_imageView);
                }

                AutoSize();
                Layout();
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

                if (IsSeparator())
                {
                    context.Renderer.SetStrokeColor(Color.DimGray);
                    context.Renderer.StrokeLine(new Vector(LeftMarginWidth + 4, Bounds.Height / 2), new Vector(Bounds.Width - 8, Bounds.Height / 2));
                }
                else
                {
                    if (Highlighted)
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
            }

            public void AutoSize()
            {
                _label.AutoSize();

                if (IsSeparator())
                {
                    Size = new Vector(0, 8);
                }
                else
                {
                    Size = new Vector(_label.Bounds.Width + 38 + (HasSubItems ? SubItemsArrowBounds : 0), Math.Max(24, _label.Bounds.Height + 12));
                }
            }

            private AABB BoundsForSelectionHighlight()
            {
                return Bounds.Inflated(0, 0);
            }

            private AABB BoundsForSubItemsArrow()
            {
                var totalBounds = new AABB(_label.FrameOnParent.Right, 0, Height, Width);
                var arrowBounds = AABB.FromRectangle(Vector.Zero, SubItemsArrowSize);
                
                return arrowBounds.WithCenterOn(totalBounds.Center);
            }

            private bool IsSeparator()
            {
                return _label.Text == "-";
            }
        }
    }
}

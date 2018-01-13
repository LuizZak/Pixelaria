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
using PixCore.Geometry;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using SharpDX.Direct2D1;

namespace PixUI.Controls
{
    /// <summary>
    /// A base view that provides its own basic self-rendering logic.
    /// </summary>
    public class SelfRenderingBaseView : BaseView
    {
        private Color _backColor = Color.FromKnownColor(KnownColor.Control);
        private Color _foreColor = Color.Black;
        private bool _clipToBounds = true;
        private float _cornerRadius;
        private bool _visible = true;

        /// <summary>
        /// This view's neutral background color
        /// </summary>
        public Color BackColor
        {
            get => _backColor;
            set
            {
                _backColor = value;
                InvalidateFullBounds();
            }
        }

        /// <summary>
        /// This view's foreground color
        /// </summary>
        public Color ForeColor
        {
            get => _foreColor;
            set
            {
                _foreColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Whether to clip the rendering of subviews to within this view's <see cref="BaseView.Bounds"/>
        /// 
        /// Defaults to true.
        /// </summary>
        public bool ClipToBounds
        {
            get => _clipToBounds;
            set
            {
                _clipToBounds = value;
                InvalidateFullBounds();
            }
        }

        /// <summary>
        /// Corner radius for this control's corners
        /// (does not affect clipping region)
        /// </summary>
        public float CornerRadius
        {
            get => _cornerRadius;
            set
            {
                _cornerRadius = value;
                InvalidateFullBounds();
            }
        }

        /// <summary>
        /// Whether this view is visible (calling <see cref="Render"/> actually renders content).
        /// 
        /// Default is true.
        /// </summary>
        public bool Visible
        {
            get => _visible;
            set
            {
                _visible = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Base logic to render this view.
        /// Deals with setting up the renderer's transform state
        /// </summary>
        public void Render([NotNull] ControlRenderingContext state)
        {
            if (!Visible)
                return;

            if (!state.ClippingRegion.IsVisibleInClippingRegion(Bounds, this))
                return;

            RenderBackground(state);
            RenderForeground(state);
        }

        /// <summary>
        /// Renders this view's background
        /// </summary>
        public virtual void RenderBackground([NotNull] ControlRenderingContext context)
        {
            // Default background renderer
            using (var brush = new SolidColorBrush(context.RenderTarget, BackColor.ToColor4()))
            {
                if (Math.Abs(CornerRadius) < float.Epsilon)
                {
                    context.RenderTarget.FillRectangle(Bounds.ToRawRectangleF(), brush);
                }
                else
                {
                    var roundedRect = new RoundedRectangle
                    {
                        RadiusX = CornerRadius,
                        RadiusY = CornerRadius,
                        Rect = Bounds.ToRawRectangleF()
                    };

                    context.RenderTarget.FillRoundedRectangle(roundedRect, brush);
                }
            }

            // Stroke
            using (var brush = new SolidColorBrush(context.RenderTarget, StrokeColor.ToColor4()))
            {
                if (Math.Abs(CornerRadius) < float.Epsilon)
                {
                    context.RenderTarget.DrawRectangle(Bounds.ToRawRectangleF(), brush, StrokeWidth);
                }
                else
                {
                    var roundedRect = new RoundedRectangle
                    {
                        RadiusX = CornerRadius,
                        RadiusY = CornerRadius,
                        Rect = Bounds.ToRawRectangleF()
                    };

                    context.RenderTarget.DrawRoundedRectangle(roundedRect, brush, StrokeWidth);
                }
            }
        }

        /// <summary>
        /// Renders this view's foreground content (not displayed on top of child views)
        /// </summary>
        public virtual void RenderForeground([NotNull] ControlRenderingContext context)
        {

        }

        /// <summary>
        /// Returns true if this and all parent views have <see cref="Visible"/> set to true.
        /// </summary>
        public bool IsVisibleOnScreen()
        {
            BaseView cur = this;
            while (cur != null)
            {
                if (cur is SelfRenderingBaseView selfView && !selfView.Visible)
                    return false;

                cur = cur.Parent;
            }

            return true;
        }

        protected override void Invalidate(RedrawRegion region, ISpatialReference reference)
        {
            if (!ReferenceEquals(reference, this) && ClipToBounds)
            {
                if (Bounds.IsEmpty || Bounds.Validity == AABB.State.Invalid)
                    region.Clear();
                else
                    region.ApplyClip(Bounds, this);
            }

            base.Invalidate(region, reference);
        }
    }

    /// <summary>
    /// A control rendering state object that has the base state along with
    /// a basic Direct2D renderer associated with it.
    /// </summary>
    public class ControlRenderingContext
    {
        public IDirect2DRenderingState State { get; }
        public IDirect2DRenderer Renderer { get; }

        /// <summary>
        /// Gets the current render target.
        /// 
        /// Shortcut for <see cref="State"/>'s <see cref="IDirect2DRenderingState.D2DRenderTarget"/>.
        /// </summary>
        public RenderTarget RenderTarget => State.D2DRenderTarget;

        /// <summary>
        /// Gets the current drawing clipping region.
        /// 
        /// Shortcut for <see cref="Renderer"/>'s <see cref="IDirect2DRenderer.ClippingRegion"/>.
        /// </summary>
        public IClippingRegion ClippingRegion => Renderer.ClippingRegion;

        /// <summary>
        /// Gets the text metrics provider.
        /// </summary>
        public ITextMetricsProvider TextMetricsProvider { get; }

        public ControlRenderingContext(IDirect2DRenderingState state, IDirect2DRenderer renderer, ITextMetricsProvider textMetricsProvider)
        {
            State = state;
            Renderer = renderer;
            TextMetricsProvider = textMetricsProvider;
        }
    }
}
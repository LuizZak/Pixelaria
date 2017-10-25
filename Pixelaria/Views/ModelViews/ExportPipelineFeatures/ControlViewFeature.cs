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
using Color = System.Drawing.Color;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Utils;
using Pixelaria.Views.ModelViews.PipelineView;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Font = System.Drawing.Font;
using FontFamily = System.Drawing.FontFamily;
using TextFormat = SharpDX.DirectWrite.TextFormat;

namespace Pixelaria.Views.ModelViews.ExportPipelineFeatures
{
    internal class ControlViewFeature : ExportPipelineUiFeature, IBaseViewRenderer
    {
        private readonly ControlView _baseControl = new ControlView();

        /// <summary>
        /// When mouse is down on a control, this is the control that the mouse
        /// was pressed down on
        /// </summary>
        [CanBeNull]
        private IEventHandler _mouseDownControl;

        /// <summary>
        /// Last control the mouse was resting on top of on the last call to OnMouseMove.
        /// 
        /// Used to handle OnMouseEnter/OnMouseLeave on controls
        /// </summary>
        [CanBeNull]
        private IEventHandler _mouseHoverControl;

        public ControlViewFeature([NotNull] ExportPipelineControl control) : base(control)
        {
            _baseControl.Size = control.Size;
            _baseControl.BackColor = Color.Transparent;
        }

        public void AddControl([NotNull] ControlView view)
        {
            _baseControl.AddChild(view);
        }

        public override void OnRender(Direct2DRenderingState state)
        {
            base.OnRender(state);

            // Create a renderer visitor for the root UI element we got
            var renderVisitor = new BaseViewRendererVisitor(state, this);
            renderVisitor.Visit(_baseControl);
        }

        public void OnRendererEnter(Direct2DRenderingState state, BaseView view)
        {
            state.PushMatrix(new Matrix3x2(view.LocalTransform.Elements));

            // Clip rendering area
            var clip = view.Bounds;
            state.D2DRenderTarget.PushAxisAlignedClip(clip, AntialiasMode.Aliased);
            
            // If this is a ScrollViewControl, use its content offset to translate the transform matrix as well
            if (view is ScrollViewControl scrollView)
            {
                state.PushMatrix(Matrix3x2.Translation(scrollView.ContentOffset));
            }
        }

        public void RenderView(Direct2DRenderingState state, BaseView view)
        {
            if (view is ControlView control)
            {
                control.Render(state);
            }
        }

        public void OnRendererExit(Direct2DRenderingState state, BaseView view)
        {
            // Pop extra matrix pushes by scroll views
            if (view is ScrollViewControl)
            {
                state.PopMatrix();
            }

            state.PopMatrix();
            state.D2DRenderTarget.PopAxisAlignedClip();
        }

        public override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _baseControl.Size = Control.Size;
        }

        public override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_mouseHoverControl != null)
            {
                _mouseHoverControl.OnMouseLeave();
                _mouseHoverControl = null;
            }
        }
        
        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            // Find control
            var control = ControlViewUnder(e.Location);
            if (control != null && !ReferenceEquals(control, _baseControl))
            {
                // Make request
                var request = new EventRequest(handler =>
                {
                    if (RequestExclusiveControl())
                    {
                        handler.OnMouseDown(ConvertMouseEvent(e, handler));

                        _mouseDownControl = control;
                    }
                });

                control.HandleOrPass(request);
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!OtherFeatureHasExclusiveControl())
            {
                // Fixed mouse-over on control that was pressed down
                if (_mouseDownControl != null)
                {
                    _mouseDownControl.OnMouseMove(ConvertMouseEvent(e, _mouseDownControl));
                }
                else
                {
                    UpdateMouseOver(e);
                }
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            var control = _mouseDownControl;
            if (control != null)
            {
                // Figure out if it's a click or mouse up event
                // Click events fire when MouseDown + MouseUp occur over the same element
                var upControl = ControlViewUnder(e.Location);
                if (upControl != null && ReferenceEquals(upControl, _mouseDownControl))
                {
                    upControl.OnMouseClick(ConvertMouseEvent(e, control));
                }
                else
                {
                    control.OnMouseUp(ConvertMouseEvent(e, control));
                }

                _mouseDownControl = null;

                UpdateMouseOver(e);

                ReleaseExclusiveControl();
            }
        }

        public override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (OtherFeatureHasExclusiveControl())
                return;

            var control = ControlViewUnder(e.Location);
            if (control != null)
            {
                // Make request
                var request = new EventRequest(handler =>
                {
                    var mouse = _mouseDownControl;

                    if (mouse != null)
                    {
                        mouse.OnMouseWheel(ConvertMouseEvent(e, mouse));
                    }
                    else
                    {
                        handler.OnMouseWheel(ConvertMouseEvent(e, handler));
                    }
                });

                control.HandleOrPass(request);
            }
        }

        private void UpdateMouseOver([NotNull] MouseEventArgs e)
        {
            var control = ControlViewUnder(e.Location);

            if (control != null)
            {
                // Make request
                var request = new EventRequest(handler =>
                {
                    if (!ReferenceEquals(_mouseHoverControl, handler))
                    {
                        _mouseHoverControl?.OnMouseLeave();
                        handler.OnMouseEnter();

                        _mouseHoverControl = handler;
                    }
                });

                control.HandleOrPass(request);

                if (request.NotAccepted)
                {
                    _mouseHoverControl?.OnMouseLeave();
                    _mouseHoverControl = null;
                }
            }
            else
            {
                _mouseHoverControl?.OnMouseLeave();
                _mouseHoverControl = null;
            }
        }

        private static MouseEventArgs ConvertMouseEvent([NotNull] MouseEventArgs e, [NotNull] IEventHandler handler)
        {
            var relative = handler.ConvertFromScreen(e.Location);
            return new MouseEventArgs(e.Button, e.Clicks, (int)relative.X, (int)relative.Y, e.Delta);
        }

        [CanBeNull]
        private ControlView ControlViewUnder(Vector point)
        {
            var loc = _baseControl.ConvertFrom(point, null);
            var control = _baseControl.HitTestControl(loc);

            return !ReferenceEquals(control, _baseControl) ? control : null;
        }

        private class EventRequest : IEventRequest
        {
            private Action<IEventHandler> OnAccept { get; }
            public bool NotAccepted { get; private set; } = true;

            public EventRequest(Action<IEventHandler> onAccept)
            {
                OnAccept = onAccept;
            }

            public void Accept(IEventHandler handler)
            {
                NotAccepted = false;
                OnAccept(handler);
            }
        }
    }

    internal interface IEventHandler
    {
        /// <summary>
        /// Next target to direct an event to, in case this handler has not handled the event.
        /// </summary>
        [CanBeNull]
        IEventHandler Next { get; }

        /// <summary>
        /// Asks this event handler to convert a screen-coordinate space point into its own
        /// local coordinates when synthesizing location events (e.g. mouse events) into this 
        /// event handler.
        /// </summary>
        Vector ConvertFromScreen(Vector vector);

        void HandleOrPass(IEventRequest eventRequest);

        void OnMouseLeave();
        void OnMouseEnter();
        
        void OnMouseClick([NotNull] MouseEventArgs e);
        void OnMouseDown([NotNull] MouseEventArgs e);
        void OnMouseMove([NotNull] MouseEventArgs e);
        void OnMouseUp([NotNull] MouseEventArgs e);
        void OnMouseWheel([NotNull] MouseEventArgs e);
    }

    /// <summary>
    /// A base view with added UI interactivity capabilities
    /// </summary>
    internal class ControlView : BaseView, IEventHandler
    {
        /// <summary>
        /// Default implementation of Next searches the view hierarchy up.
        /// </summary>
        public IEventHandler Next => NextControlViewFrom(this);

        /// <summary>
        /// This control's neutral background color
        /// </summary>
        public Color BackColor { get; set; } = Color.FromKnownColor(KnownColor.Control);

        /// <summary>
        /// This control's foreground color
        /// </summary>
        public Color ForeColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets the content bounds of this control, which are the inner area of this control
        /// where content is effectively visible in.
        /// 
        /// Scroll views and other controls may modify bounds to alter the interactible area
        /// of the control.
        /// </summary>
        public virtual AABB ContentBounds => Bounds;

        /// <summary>
        /// Returns the first control view under a given point on this control view.
        /// 
        /// Returns null, if no control was found.
        /// </summary>
        /// <param name="point">Point to hit-test against, in local coordinates of this ControlView</param>
        [CanBeNull]
        public ControlView HitTestControl(Vector point)
        {
            // Test children first
            return ViewUnder(point, Vector.Zero, view => view is ControlView) as ControlView;
        }

        public void HandleMouseClick([NotNull] MouseEventArgs e)
        {
            OnMouseClick(e);
        }

        public void HandleMouseDown([NotNull] MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        public void HandleMouseMove([NotNull] MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        public void HandleMouseUp([NotNull] MouseEventArgs e)
        {
            OnMouseUp(e);
        }

        public virtual void OnMouseLeave() { }
        public virtual void OnMouseEnter() { }

        public virtual void OnMouseClick(MouseEventArgs e) { }
        public virtual void OnMouseDown(MouseEventArgs e) { }
        public virtual void OnMouseMove(MouseEventArgs e) { }
        public virtual void OnMouseUp(MouseEventArgs e) { }
        public virtual void OnMouseWheel(MouseEventArgs e) { }

        public Vector ConvertFromScreen(Vector vector)
        {
            return ConvertFrom(vector, null);
        }

        public void HandleOrPass(IEventRequest eventRequest)
        {
            if(CanHandle(eventRequest))
                Next?.HandleOrPass(eventRequest);
            else
                eventRequest.Accept(this);
        }

        public virtual bool CanHandle(IEventRequest eventRequest)
        {
            // Consume all mouse event requests by default
            return eventRequest is IMouseEventRequest;
        }

        /// <summary>
        /// Base logic to render this control.
        /// Deals with setting up the renderer's transform state
        /// </summary>
        public void Render([NotNull] Direct2DRenderingState state)
        {
            RenderBackground(state);
            RenderForeground(state);
        }

        /// <summary>
        /// Renders this control's background
        /// </summary>
        public virtual void RenderBackground([NotNull] Direct2DRenderingState state)
        {
            // Default background renderer
            using (var brush = new SolidColorBrush(state.D2DRenderTarget, BackColor.ToColor4()))
            {
                state.D2DRenderTarget.FillRectangle(ContentBounds, brush);
            }
        }

        /// <summary>
        /// Renders this control's foreground content
        /// </summary>
        public virtual void RenderForeground([NotNull] Direct2DRenderingState state)
        {
            
        }

        /// <summary>
        /// Traverses the hierarchy of a given view, returning the first ControlView
        /// that the method finds.
        /// 
        /// The method ignores <see cref="view"/> itself.
        /// </summary>
        [CanBeNull]
        private static ControlView NextControlViewFrom([NotNull] BaseView view)
        {
            var next = view.Parent;
            while (next != null)
            {
                if (next is ControlView control)
                    return control;

                next = next.Parent;
            }

            return null;
        }
    }

    /// <summary>
    /// A container view which scrolls to allow panning contents into a rectangular view
    /// </summary>
    internal class ScrollViewControl : ControlView
    {
        private Vector _contentSize;
        private Vector _contentOffset;

        /// <summary>
        /// Total size of the larget inner contents of this scroll view
        /// </summary>
        public Vector ContentSize
        {
            get => _contentSize;
            set
            {
                _contentSize = value;
                CalculateClipSize();
            }
        }

        /// <summary>
        /// Offset from (0, 0) that the view is scrolled by
        /// </summary>
        public Vector ContentOffset
        {
            get => _contentOffset;
            set
            {
                _contentOffset = value;
                CalculateClipSize();
            }
        }

        public override AABB ContentBounds => new AABB(ContentOffset, ContentOffset + ContentSize);

        private void CalculateClipSize()
        {
            
        }

        public override void RenderBackground(Direct2DRenderingState state)
        {
            // Default background renderer
            using (var brush = new SolidColorBrush(state.D2DRenderTarget, BackColor.ToColor4()))
            {
                state.D2DRenderTarget.FillRectangle(ContentBounds.OffsetTo(0, 0), brush);
            }
        }
    }

    /// <summary>
    /// A basic textual label
    /// </summary>
    internal class LabelViewControl : ControlView
    {
        private string _text = "";
        private Font _textFont = new Font(FontFamily.GenericSansSerif, 11);

        [NotNull]
        public Font TextFont
        {
            get => _textFont;
            set
            {
                _textFont = value;
                CalculateBounds();
            }
        }

        [NotNull]
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;

                CalculateBounds();
            }
        }
        
        private void CalculateBounds()
        {
            Size = LabelView.DefaultLabelViewSizeProvider.CalculateTextSize(Text, TextFont);
        }

        public override void RenderForeground(Direct2DRenderingState state)
        {
            using (var brush = new SolidColorBrush(state.D2DRenderTarget, ForeColor.ToColor4()))
            using (var textFormat = new TextFormat(state.DirectWriteFactory, _textFont.FontFamily.Name, _textFont.Size))
            {
                state.D2DRenderTarget.DrawText(_text, textFormat, Bounds, brush, DrawTextOptions.Clip);
            }
        }
    }

    internal class ButtonControl : ControlView
    {
        private bool _mouseDown;

        public Color HighlightColor { get; set; } = Color.LightGray;

        public Color SelectedColor { get; set; } = Color.Gray;

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

        public override void RenderBackground(Direct2DRenderingState state)
        {
            // Default background renderer
            using (var brush = new SolidColorBrush(state.D2DRenderTarget, BackColor.ToColor4()))
            {
                state.D2DRenderTarget.FillRoundedRectangle(new RoundedRectangle { RadiusX = 3, RadiusY = 3, Rect = Bounds }, brush);
            }
            
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

            BackColor = Color.LightGray;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            BackColor = SelectedColor;

            _mouseDown = true;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDown)
            {
                BackColor = Bounds.Contains(e.Location) ? SelectedColor : HighlightColor;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            BackColor = Color.FromKnownColor(KnownColor.Control);

            _mouseDown = false;
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();

            BackColor = Color.FromKnownColor(KnownColor.Control);
        }
    }

    /// <summary>
    /// Encapsulates an event request object that traverses responder chains looking
    /// for a target for input events.
    /// </summary>
    internal interface IEventRequest
    {
        /// <summary>
        /// Accepts a given event handler for receiving input events
        /// </summary>
        void Accept(IEventHandler handler);
    }

    internal interface IMouseEventRequest : IEventRequest
    {
        
    }

    /// <summary>
    /// Describes an encapsulated event <see cref="T"/> that can be consumed by 
    /// a control view such that other views up the hierarchy chain are signaled 
    /// to not handle the event.
    /// </summary>
    internal class ConsumableEvent<T> : EventArgs where T : EventArgs
    {
        /// <summary>
        /// Event to be handled
        /// </summary>
        public T Event { get; }
        
        /// <summary>
        /// Whether this event was handled by a control
        /// </summary>
        public bool Handled { get; set; }

        public ConsumableEvent(T @event, bool handled)
        {
            Event = @event;
            Handled = handled;
        }
    }
}

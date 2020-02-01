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

using JetBrains.Annotations;
using PixCore.Geometry;
using PixRendering;
using PixUI.Controls;
using PixUI.Rendering;
using PixUI.Visitor;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    /// <summary>
    /// Manages input handling of <see cref="ControlView"/> instances allowing the user to interact with control
    /// using mouse/keyboard.
    /// 
    /// Also handles focus management for keyboard event receiving.
    /// </summary>
    internal class ControlViewFeature : ExportPipelineUiFeature, IFirstResponderDelegate<IEventHandler>, IRenderListener, IInvalidateRegionDelegate, IControlContainer
    {
        /// <summary>
        /// Gets the base view that all control views must be added to to enable user interaction
        /// </summary>
        public RootControlView BaseControl { get; }

        public int RenderOrder => RenderOrdering.UserInterface;

        /// <summary>
        /// When mouse is down on a control, this is the control that the mouse
        /// was pressed down on
        /// </summary>
        [CanBeNull]
        private IMouseEventHandler _mouseDownTarget;

        /// <summary>
        /// Last control the mouse was resting on top of on the last call to <see cref="OnMouseMove"/>.
        /// 
        /// Used to handle <see cref="ExportPipelineUiFeature.OnMouseEnter"/>/<see cref="OnMouseLeave"/> on controls
        /// </summary>
        [CanBeNull]
        private IMouseEventHandler _mouseHoverTarget;

        /// <summary>
        /// First responder for keyboard events
        /// </summary>
        [CanBeNull]
        private IKeyboardEventHandler _firstResponder;

        public ControlViewFeature([NotNull] IExportPipelineControl control, [NotNull] IRenderManager renderManager) : base(control)
        {
            BaseControl = new RootControlView(this)
            {
                Size = control.Size,
                InvalidateRegionDelegate = this
            };

            renderManager.AddRenderListener(this);
        }

        /// <summary>
        /// Adds a new control to the UI
        /// </summary>
        public void AddControl(ControlView view)
        {
            BaseControl.AddChild(view);
        }

        /// <summary>
        /// Removes a control from the UI.
        /// 
        /// If view is not currently a control on this UI, nothing is done.
        /// </summary>
        public void RemoveControl(ControlView view)
        {
            if (!view.IsDescendentOf(BaseControl))
                return;

            view.RemoveFromParent();
        }

        public override void OnFixedFrame(EventArgs e)
        {
            base.OnFixedFrame(e);

            var args = new FixedFrameEventArgs(TimeSpan.FromMilliseconds(8));

            // Call frame tick on all views
            var visitor = new BaseViewVisitor<object>((o, view) =>
            {
                if (view is ControlView controlView)
                {
                    controlView.OnFixedFrame(args);
                }
            });

            var traverser = new BaseViewTraveler<object>(null, visitor);
            traverser.Visit(BaseControl);
        }

        public void Render(IRenderListenerParameters parameters)
        {
            // Apply layout events first
            BaseControl.Layout();

            var context = new ControlRenderingContext(parameters);

            // Create a renderer visitor for the root UI element we got
            var visitor = new ViewRenderingVisitor();
            var traverser = new BaseViewTraveler<ControlRenderingContext>(context, visitor);
            traverser.Visit(BaseControl);
        }

        public override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            BaseControl.Size = e.Size;
        }

        public void RecreateState(IRenderLoopState state)
        {
            
        }

        public override void OtherFeatureConsumedMouseDown()
        {
            base.OtherFeatureConsumedMouseDown();

            _firstResponder?.ResignFirstResponder();
        }

        #region Mouse Events

        public override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_mouseHoverTarget != null)
            {
                _mouseHoverTarget.OnMouseLeave();
                _mouseHoverTarget = null;
            }
        }
        
        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            
            // Find control
            var control = ControlViewUnder(e.Location);
            if (control == null)
            {
                _firstResponder?.ResignFirstResponder();
                return;
            }

            // Make request
            var request = new MouseEventRequest(MouseEventType.MouseDown, handler =>
            {
                if (!RequestExclusiveControl())
                    return;

                handler.OnMouseDown(ConvertMouseEvent(e, handler));

                _mouseDownTarget = handler;

                if(handler != _firstResponder)
                    _firstResponder?.ResignFirstResponder();

                ConsumeEvent();
            });

            control.HandleOrPass(request);
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!OtherFeatureHasExclusiveControl())
            {
                // Fixed mouse-over on control that was pressed down
                if (_mouseDownTarget != null)
                {
                    _mouseDownTarget.OnMouseMove(ConvertMouseEvent(e, _mouseDownTarget));
                }
                else
                {
                    UpdateMouseOver(e, MouseEventType.MouseMove);
                }
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            var control = _mouseDownTarget;
            if (control != null)
            {
                // Figure out if it's a click or mouse up event
                // Click events fire when MouseDown + MouseUp occur over the same element
                var upControl = ControlViewUnder(e.Location);
                if (upControl != null && ReferenceEquals(upControl, _mouseDownTarget))
                {
                    upControl.OnMouseUp(ConvertMouseEvent(e, upControl));
                    upControl.OnMouseClick(ConvertMouseEvent(e, upControl));
                }
                else
                {
                    control.OnMouseUp(ConvertMouseEvent(e, control));
                }

                _mouseDownTarget = null;

                UpdateMouseOver(e, MouseEventType.MouseMove);

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
                var request = new MouseEventRequest(MouseEventType.MouseWheel, handler =>
                {
                    var mouse = _mouseDownTarget;

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

                ConsumeEvent();
            }
        }

        #endregion

        #region Keyboard Events

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (OtherFeatureHasExclusiveControl())
                return;

            var responder = _firstResponder;
            if (responder == null)
                return;

            var request = new KeyboardEventRequest(KeyboardEventType.KeyDown, handler =>
            {
                ConsumeEvent();

                handler.OnKeyDown(e);
            });

            responder.HandleOrPass(request);
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (OtherFeatureHasExclusiveControl())
                return;

            var responder = _firstResponder;
            if (responder == null)
                return;

            var request = new KeyboardEventRequest(KeyboardEventType.KeyUp, handler =>
            {
                ConsumeEvent();

                handler.OnKeyUp(e);
            });

            responder.HandleOrPass(request);
        }

        public override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (OtherFeatureHasExclusiveControl())
                return;

            var responder = _firstResponder;
            if (responder == null)
                return;

            var request = new KeyboardEventRequest(KeyboardEventType.KeyPress, handler =>
            {
                ConsumeEvent();

                handler.OnKeyPress(e);
            });

            responder.HandleOrPass(request);
        }

        public override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (OtherFeatureHasExclusiveControl())
                return;

            var responder = _firstResponder;
            if (responder == null)
                return;

            var request = new KeyboardEventRequest(KeyboardEventType.PreviewKeyDown, handler =>
            {
                ConsumeEvent();

                handler.OnPreviewKeyDown(e);
            });

            responder.HandleOrPass(request);
        }

        #endregion

        private void UpdateMouseOver([NotNull] MouseEventArgs e, MouseEventType eventType)
        {
            var control = ControlViewUnder(e.Location);

            if (control != null)
            {
                // Make request
                var request = new MouseEventRequest(eventType, handler =>
                {
                    if (!ReferenceEquals(_mouseHoverTarget, handler))
                    {
                        _mouseHoverTarget?.OnMouseLeave();
                        handler.OnMouseEnter();

                        _mouseHoverTarget = handler;
                    }
                    else
                    {
                        _mouseHoverTarget?.OnMouseMove(e);
                    }
                });

                control.HandleOrPass(request);

                if (request.NotAccepted)
                {
                    _mouseHoverTarget?.OnMouseLeave();
                    _mouseHoverTarget = null;
                }
            }
            else
            {
                _mouseHoverTarget?.OnMouseLeave();
                _mouseHoverTarget = null;
            }
        }

        private static MouseEventArgs ConvertMouseEvent([NotNull] MouseEventArgs e, [NotNull] IEventHandler handler)
        {
            var relative = handler.ConvertFromScreen(e.Location);
            return new MouseEventArgs(e.Button, e.Clicks, (int)relative.X, (int)relative.Y, e.Delta);
        }

        [CanBeNull]
        private ControlView ControlViewUnder(Vector point, bool enabledOnly = true)
        {
            var loc = BaseControl.ConvertFrom(point, null);
            var control = BaseControl.HitTestControl(loc, enabledOnly);

            return control;
        }

        private class EventRequest<THandler> : IEventRequest where THandler: IEventHandler
        {
            private Action<THandler> OnAccept { get; }
            public bool NotAccepted { get; private set; } = true;

            protected EventRequest(Action<THandler> onAccept)
            {
                OnAccept = onAccept;
            }

            public void Accept(IEventHandler handler)
            {
                if (!(handler is THandler casted))
                    return;

                NotAccepted = false;
                OnAccept(casted);
            }
        }

        private class MouseEventRequest : EventRequest<IMouseEventHandler>, IMouseEventRequest
        {
            public MouseEventType EventType { get; }

            public MouseEventRequest(MouseEventType eventType, Action<IMouseEventHandler> onAccept) : base(onAccept)
            {
                EventType = eventType;
            }
        }

        private class KeyboardEventRequest : EventRequest<IKeyboardEventHandler>, IKeyboardEventRequest
        {
            public KeyboardEventType EventType { get; }

            public KeyboardEventRequest(KeyboardEventType eventType, Action<IKeyboardEventHandler> onAccept) : base(onAccept)
            {
                EventType = eventType;
            }
        }

        #region IInvalidateRegionDelegate

        public void DidInvalidate(RedrawRegion region, ISpatialReference reference)
        {
            Control.InvalidateRegion(region);
        }

        #endregion

        #region IFirstResponderDelegate<IEventHandler>

        public bool SetAsFirstResponder(IEventHandler firstResponder, bool force)
        {
            if (!(firstResponder is IKeyboardEventHandler keyboardResponder))
                return false;

            // Try to resign previous first responder, first
            if (_firstResponder != null)
            {
                if (!_firstResponder.CanResignFirstResponder && !force)
                    return false;

                _firstResponder.ResignFirstResponder();
            }

            _firstResponder = keyboardResponder;

            return true;
        }

        public void RemoveCurrentResponder()
        {
            _firstResponder = null;
        }

        public bool IsFirstResponder(IEventHandler handler)
        {
            return _firstResponder == handler;
        }

        #endregion
    }
}

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
using Color = System.Drawing.Color;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;

using JetBrains.Annotations;

using Pixelaria.Utils;

using SharpDX;
using SharpDX.Direct2D1;

using Pixelaria.Views.ExportPipeline.PipelineView;
using Pixelaria.Views.ExportPipeline.PipelineView.Controls;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    internal class ControlViewFeature : ExportPipelineUiFeature, IBaseViewVisitor<ControlRenderingContext>
    {
        private readonly ControlView _baseControl = new ControlView();

        /// <summary>
        /// When mouse is down on a control, this is the control that the mouse
        /// was pressed down on
        /// </summary>
        [CanBeNull]
        private IMouseEventHandler _mouseDownTarget;

        /// <summary>
        /// Last control the mouse was resting on top of on the last call to OnMouseMove.
        /// 
        /// Used to handle OnMouseEnter/OnMouseLeave on controls
        /// </summary>
        [CanBeNull]
        private IMouseEventHandler _mouseHoverTarget;

        public ControlViewFeature([NotNull] ExportPipelineControl control) : base(control)
        {
            _baseControl.Size = control.Size;
            _baseControl.BackColor = Color.Transparent;
        }

        public void AddControl([NotNull] ControlView view)
        {
            _baseControl.AddChild(view);
        }

        public override void OnFixedFrame(EventArgs e)
        {
            base.OnFixedFrame(e);

            // Call frame tick on all views
            var visitor = new BaseViewVisitor<object>((o, view) =>
            {
                if (view is ControlView controlView)
                {
                    controlView.OnFixedFrame();
                }
            });

            var traverser = new BaseViewTraverser<object>(null, visitor);
            traverser.Visit(_baseControl);
        }

        public override void OnRender(Direct2DRenderingState state)
        {
            base.OnRender(state);

            var context = new ControlRenderingContext(state, Control.D2DRenderer);

            // Create a renderer visitor for the root UI element we got
            var traverser = new BaseViewTraverser<ControlRenderingContext>(context, this);
            traverser.Visit(_baseControl);
        }

        public void OnVisitorEnter([NotNull] ControlRenderingContext context, BaseView view)
        {
            context.State.PushMatrix(new Matrix3x2(view.LocalTransform.Elements));

            // Clip rendering area
            if (view is SelfRenderingBaseView selfRendering && selfRendering.ClipToBounds)
            {
                var clip = view.Bounds;
                context.RenderTarget.PushAxisAlignedClip(clip, AntialiasMode.Aliased);
            }
        }

        public void VisitView([NotNull] ControlRenderingContext context, BaseView view)
        {
            if (view is SelfRenderingBaseView selfRendering && selfRendering.IsVisibleOnScreen())
            {
                selfRendering.Render(context);
            }
        }

        public void OnVisitorExit([NotNull] ControlRenderingContext context, BaseView view)
        {
            if (view is SelfRenderingBaseView selfRendering && selfRendering.ClipToBounds)
            {
                context.RenderTarget.PopAxisAlignedClip();
            }

            context.State.PopMatrix();
        }

        public override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            _baseControl.Size = Control.Size;
        }

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
            if (control != null && !ReferenceEquals(control, _baseControl))
            {
                // Make request
                var request = new MouseEventRequest(MouseEventType.MouseDown, handler =>
                {
                    if (!RequestExclusiveControl())
                        return;

                    handler.OnMouseDown(ConvertMouseEvent(e, handler));

                    _mouseDownTarget = handler;
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
            var loc = _baseControl.ConvertFrom(point, null);
            var control = _baseControl.HitTestControl(loc, enabledOnly);

            return !ReferenceEquals(control, _baseControl) ? control : null;
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
                if (!(handler is THandler))
                    return;

                var casted = (THandler)handler;

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
    }

    internal static class ControlViewRxUtils
    {
        /// <summary>
        /// Returns a signal that alterns between true and false as the user presses/releases the mouse
        /// on the control.
        /// 
        /// This observable only signals when the next <see cref="ControlView.IReactive.MouseDown"/> or
        /// <see cref="ControlView.IReactive.MouseUp"/> observables fire.
        /// </summary>
        public static IObservable<bool> IsMouseDown([NotNull] this ControlView.IReactive reactive)
        {
            var onDown = reactive.MouseDown.Select(_ => true);
            var onUp = reactive.MouseUp.Select(_ => false);

            return onDown.Merge(onUp);
        }

        /// <summary>
        /// Returns an observable that fires repeatedly for as long as the user holds down the mouse button
        /// over the control.
        /// 
        /// The observable always fires a single event for the mouse down right away, and configures a delayed
        /// repeating interval of multiple signals afterwards.
        /// 
        /// The observable is non-terminating, and will remain subscribed until the subscription to itself is
        /// disposed.
        /// </summary>
        /// <param name="reactive">ControlView reactive bindings object.</param>
        /// <param name="delay">Initial delay before start rapid-firing.</param>
        /// <param name="interval">The interval between each subsequence repeat event</param>
        public static IObservable<MouseEventArgs> MouseDownRepeating([NotNull] this ControlView.IReactive reactive, TimeSpan delay, TimeSpan interval)
        {
            return Observable.Create<MouseEventArgs>(obs =>
            {
                IDisposable disposableRepeat = null;

                var onDown = reactive.MouseDown.Select(e => (true, e));
                var onUp = reactive.MouseUp.Select(e => (false, e));

                var isMouseDown = onDown.Merge(onUp);

                return
                    isMouseDown
                        .Select(a =>
                        {
                            var (isDown, e) = a;

                            if (!isDown)
                                return Observable.Never<MouseEventArgs>();

                            return
                                Observable.Interval(interval)
                                    .Delay(delay)
                                    .WithLatestFrom(reactive.MouseMove.StartWith(e), (l, args) => args)
                                    .StartWith(e)
                                    .ObserveOn(reactive.Dispatcher);
                        })
                        .Subscribe(timer =>
                        {
                            disposableRepeat?.Dispose();
                            disposableRepeat = timer.Subscribe(obs);
                        });
            });
        }
    }
}

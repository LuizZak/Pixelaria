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
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows.Forms;

using JetBrains.Annotations;
using Pixelaria.Utils;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A base view with added UI interactivity capabilities
    /// </summary>
    internal class ControlView : SelfRenderingBaseView, IMouseEventHandler, IDisposable
    {
        private readonly Reactive _reactive = new Reactive();

        private readonly List<MouseEventRecognizer> _recognizers = new List<MouseEventRecognizer>();

        /// <summary>
        /// Returns true only if this and all parent control views have <see cref="InteractionEnabled"/>
        /// set to true.
        /// </summary>
        private bool IsRecursivelyInteractiveEnabled
        {
            get
            {
                var view = this;
                while (view != null)
                {
                    if (!view.InteractionEnabled)
                        return false;

                    view = NextControlViewFrom(view);
                }

                return true;
            }
        }

        /// <summary>
        /// A composite disposable that is disposed automatically when this control view is
        /// disposed.
        /// 
        /// This is used for controls that setup custom reactive logic and need a common place
        /// to put subscriptions' <see cref="IDisposable"/> returns into.
        /// </summary>
        protected readonly CompositeDisposable DisposeBag = new CompositeDisposable();

        /// <summary>
        /// Exposed reactive signal for this control
        /// </summary>
        public IReactive Rx => _reactive;

        /// <summary>
        /// Default implementation of Next searches the view hierarchy up.
        /// </summary>
        public IEventHandler Next => NextControlViewFrom(this);

        /// <summary>
        /// Whether mouse/keyboard interaction is enabled on this control.
        /// 
        /// If false, event handlers for keyboard and mouse are ignored for this
        /// and every child control, including custom event recognizers registered.
        /// 
        /// Defaults to true.
        /// </summary>
        public bool InteractionEnabled { get; set; } = true;
        
        /// <summary>
        /// Gets the content bounds of this control, which are the inner area of this control
        /// where content is effectively contained within. May be larger than this control's <see cref="BaseView.Bounds"/>.
        /// 
        /// Scroll views and other controls may modify bounds to alter the interactible area
        /// of the control.
        /// </summary>
        public virtual AABB ContentBounds => Bounds;

        /// <summary>
        /// Gets an array of all mouse event recognizers registered on this control view
        /// </summary>
        public IReadOnlyList<MouseEventRecognizer> MouseEventRecognizers => _recognizers;

        /// <summary>
        /// A user-defined tag that can be defined for this control
        /// </summary>
        public object Tag { get; set; }

        ~ControlView()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of resources allocated by this control view.
        /// 
        /// Child controls are <i>not</i> disposed of automatically.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            
            DisposeBag.Dispose();
        }

        /// <summary>
        /// Returns the first control view under a given point on this control view.
        /// 
        /// Returns null, if no control was found.
        /// </summary>
        /// <param name="point">Point to hit-test against, in local coordinates of this ControlView</param>
        /// <param name="enabledOnly">Whether to only consider views that have interactivity enabled. See <see cref="InteractionEnabled"/></param>
        [CanBeNull]
        public ControlView HitTestControl(Vector point, bool enabledOnly = true)
        {
            // Test children first
            return ViewUnder(point, Vector.Zero,
                view => view is ControlView c && c.IsVisibleOnScreen() &&
                        (!enabledOnly || c.IsRecursivelyInteractiveEnabled)) as ControlView;
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
        
        /// <summary>
        /// Fixed-step frame update. Time is dependent on <see cref="ExportPipelineUiFeature.OnFixedFrame"/> interval.
        /// </summary>
        public virtual void OnFixedFrame() { }

        public virtual void OnMouseLeave()
        {
            _reactive.MouseLeaveSubject.OnNext(Unit.Default);
        }

        public virtual void OnMouseEnter()
        {
            _reactive.MouseEnterSubject.OnNext(Unit.Default);
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
            _reactive.MouseClickSubject.OnNext(e);
        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {
            _reactive.MouseDownSubject.OnNext(e);
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
            _reactive.MouseMoveSubject.OnNext(e);
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
            _reactive.MouseUpSubject.OnNext(e);
        }

        public virtual void OnMouseWheel(MouseEventArgs e)
        {
            _reactive.MouseWheelSubject.OnNext(e);
        }

        public Vector ConvertFromScreen(Vector vector)
        {
            return ConvertFrom(vector, null);
        }

        public void HandleOrPass(IEventRequest eventRequest)
        {
            if (!IsRecursivelyInteractiveEnabled)
            {
                Next?.HandleOrPass(eventRequest);
                return;
            }

            // Check mouse recognizers, first
            var recognizer = _recognizers.FirstOrDefault(r => r.CanHandle(eventRequest));
            if (recognizer != null)
            {
                eventRequest.Accept(recognizer);
                return;
            }

            if (CanHandle(eventRequest))
                eventRequest.Accept(this);
            else
                Next?.HandleOrPass(eventRequest);
        }

        public virtual bool CanHandle(IEventRequest eventRequest)
        {
            // Consume all mouse event requests (except mouse wheel) by default
            if (eventRequest is IMouseEventRequest mouseEvent)
                return mouseEvent.EventType != MouseEventType.MouseWheel;

            return false;
        }

        /// <summary>
        /// Adds a mouse event recognizer that is capable of handling mouse events
        /// that pass through this control.
        /// 
        /// If <see cref="recognizer"/> is already added to another control, an
        /// <see cref="ArgumentException"/> is raised.
        /// </summary>
        public void AddMouseRecognizer([NotNull] MouseEventRecognizer recognizer)
        {
            if(recognizer.Control != null)
                throw new ArgumentException(@"Recognizer is already contained on another control", nameof(recognizer));

            recognizer.Control = this;

            _recognizers.Add(recognizer);
        }

        /// <summary>
        /// Removes 
        /// </summary>
        /// <param name="recognizer"></param>
        public void RemoveMouseRecognizer([NotNull] MouseEventRecognizer recognizer)
        {
            recognizer.Control = null;

            _recognizers.Remove(recognizer);
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

        /// <summary>
        /// Available reactive signals for a <see cref="ControlView"/>
        /// </summary>
        public interface IReactive
        {
            IObservable<MouseEventArgs> MouseClick { get; }
            IObservable<MouseEventArgs> MouseDown { get; }
            IObservable<MouseEventArgs> MouseMove { get; }
            IObservable<MouseEventArgs> MouseUp { get; }
            IObservable<MouseEventArgs> MouseWheel { get; }
            IObservable<Unit> MouseEnter { get; }
            IObservable<Unit> MouseLeave { get; }
        }

        /// <summary>
        /// Just for organizing a ControlView's reactive publisher/signals
        /// </summary>
        private class Reactive : IReactive
        {
            public readonly Subject<MouseEventArgs> MouseClickSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseDownSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseMoveSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseUpSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseWheelSubject = new Subject<MouseEventArgs>();
            public readonly Subject<Unit> MouseEnterSubject = new Subject<Unit>();
            public readonly Subject<Unit> MouseLeaveSubject = new Subject<Unit>();

            public IObservable<MouseEventArgs> MouseClick => MouseClickSubject;
            public IObservable<MouseEventArgs> MouseDown => MouseDownSubject;
            public IObservable<MouseEventArgs> MouseMove => MouseMoveSubject;
            public IObservable<MouseEventArgs> MouseUp => MouseUpSubject;
            public IObservable<MouseEventArgs> MouseWheel => MouseWheelSubject;
            public IObservable<Unit> MouseEnter => MouseEnterSubject;
            public IObservable<Unit> MouseLeave => MouseLeaveSubject;
        }
    }

    /// <summary>
    /// Base class for all mouse event recognizers
    /// </summary>
    internal class MouseEventRecognizer : IMouseEventHandler
    {
        /// <summary>
        /// Control this handler is associated with
        /// </summary>
        public ControlView Control { get; set; }

        public IEventHandler Next
        {
            get
            {
                if (Control == null)
                    return null;

                // Find next mouse event handler from this one
                for (int i = 1; i < Control.MouseEventRecognizers.Count; i++)
                {
                    if (Control.MouseEventRecognizers[i - 1] == this)
                        return Control.MouseEventRecognizers[i];
                }

                // Fallback to the control's own next handler
                return Control.Next;
            }
        }

        public Vector ConvertFromScreen(Vector vector)
        {
            return Control.ConvertFrom(vector, null);
        }

        public void HandleOrPass(IEventRequest eventRequest)
        {
            if (CanHandle(eventRequest))
                eventRequest.Accept(this);
            else
                Next?.HandleOrPass(eventRequest);
        }

        public virtual bool CanHandle(IEventRequest eventRequest)
        {
            return false;
        }

        public virtual void OnMouseLeave()
        {

        }

        public virtual void OnMouseEnter()
        {

        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {

        }

        public virtual void OnMouseWheel(MouseEventArgs e)
        {

        }

        public virtual void OnMouseDown(MouseEventArgs e)
        {

        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {

        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {

        }

        public class MouseEventRecognizerEventArgs : EventArgs
        {
            
        }
    }

    /// <summary>
    /// A mouse event handler that detects mouse presses and drags, reporting mouse
    /// location as it moves over a control while pressed down.
    /// </summary>
    internal class DragMouseEventRecognizer : MouseEventRecognizer
    {
        /// <summary>
        /// Called when this event recognizer's state is changed.
        /// </summary>
        public event DragMouseEventHandler DragMouseEvent;

        /// <summary>
        /// Current state of this mouse event.
        /// 
        /// Only ever <see cref="DragMouseEventState.Idle"/>, <see cref="DragMouseEventState.MousePressed"/>
        /// or <see cref="DragMouseEventState.MouseMoved"/>.
        /// </summary>
        public DragMouseEventState State { get; private set; } = DragMouseEventState.Idle;

        /// <summary>
        /// Location of mouse over control.
        /// 
        /// Only valid when <see cref="State"/> is either <see cref="DragMouseEventState.MousePressed"/> or
        /// <see cref="DragMouseEventState.MouseMoved"/>.
        /// </summary>
        public Vector MousePosition { get; private set; } = Vector.Zero;

        /// <summary>
        /// Delegate for a DragMouseEvent event
        /// </summary>
        public delegate void DragMouseEventHandler(object sender, DragMouseEventArgs e);

        public override bool CanHandle(IEventRequest eventRequest)
        {
            if (!(eventRequest is IMouseEventRequest mouseEvent))
                return false;

            return
                mouseEvent.EventType == MouseEventType.MouseDown ||
                mouseEvent.EventType == MouseEventType.MouseMove ||
                mouseEvent.EventType == MouseEventType.MouseUp;
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            MousePosition = ConvertFromScreen(e.Location);

            State = DragMouseEventState.MousePressed;

            DragMouseEvent?.Invoke(this, new DragMouseEventArgs(MousePosition, State));
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            if (State != DragMouseEventState.MousePressed && State != DragMouseEventState.MouseMoved)
                return;

            MousePosition = ConvertFromScreen(e.Location);

            State = DragMouseEventState.MouseMoved;

            DragMouseEvent?.Invoke(this, new DragMouseEventArgs(MousePosition, State));
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            if (State != DragMouseEventState.MousePressed && State != DragMouseEventState.MouseMoved)
                return;

            MousePosition = ConvertFromScreen(e.Location);

            State = DragMouseEventState.Idle;

            DragMouseEvent?.Invoke(this, new DragMouseEventArgs(MousePosition, DragMouseEventState.MouseReleased));
        }

        public override void OnMouseEnter()
        {
            DragMouseEvent?.Invoke(this, new DragMouseEventArgs(Vector.Zero, DragMouseEventState.MouseEntered));
        }

        public override void OnMouseLeave()
        {
            DragMouseEvent?.Invoke(this, new DragMouseEventArgs(Vector.Zero, DragMouseEventState.MouseLeft));
        }

        /// <summary>
        /// Event args for mouse drags
        /// </summary>
        public sealed class DragMouseEventArgs : MouseEventRecognizerEventArgs
        {
            /// <summary>
            /// Mouse position in relation to <see cref="MouseEventRecognizer.Control"/> where
            /// the mouse is located at during the time of this event args creation.
            /// 
            /// This value is not a valid mouse position when <see cref="State"/> is <see cref="DragMouseEventState.MouseEntered"/>
            /// or <see cref="DragMouseEventState.MouseLeft"/>
            /// </summary>
            public Vector MousePosition { get; }

            /// <summary>
            /// State for the event.
            /// 
            /// Is <see cref="DragMouseEventState.MouseEntered"/> when called from <see cref="OnMouseEnter"/>,
            /// <see cref="DragMouseEventState.MousePressed"/> when called from <see cref="OnMouseDown"/>,
            /// <see cref="DragMouseEventState.MouseMoved"/> when called from <see cref="OnMouseMove"/>,
            /// <see cref="DragMouseEventState.MouseReleased"/> when called from <see cref="OnMouseUp"/>, and
            /// <see cref="DragMouseEventState.MouseLeft"/> when called from <see cref="OnMouseLeave"/>,.
            /// 
            /// <see cref="DragMouseEventState.Idle"/> is never sent with an event, and thus this property
            /// never has this value.
            /// </summary>
            public DragMouseEventState State { get; }

            public DragMouseEventArgs(Vector mousePosition, DragMouseEventState state)
            {
                MousePosition = mousePosition;
                State = state;
            }
        }

        /// <summary>
        /// Possible states of a drag mouse event recognizer
        /// </summary>
        public enum DragMouseEventState
        {
            /// <summary>
            /// Recognizer is not handling any state.
            /// </summary>
            Idle,
            /// <summary>
            /// Mouse has entered the control's bounds
            /// </summary>
            MouseEntered,
            /// <summary>
            /// Mouse has been pressed down, but not moved yet.
            /// </summary>
            MousePressed,
            /// <summary>
            /// Mouse has been moved while pressed down.
            /// </summary>
            MouseMoved,
            /// <summary>
            /// Mouse has been released.
            /// 
            /// Only ever used in event args and drag mouse event recognizers
            /// never have this state explicitly through <see cref="State"/>.
            /// </summary>
            MouseReleased,
            /// <summary>
            /// Mouse has left the control's bounds
            /// </summary>
            MouseLeft
        }
    }
}
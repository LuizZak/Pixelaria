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
using System.Windows.Threading;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixUI.Utils;
using SharpDX.DirectWrite;

namespace PixUI.Controls
{
    /// <summary>
    /// A base view with added UI interactivity capabilities
    /// </summary>
    public class ControlView : SelfRenderingBaseView, IMouseEventHandler, IDisposable
    {
        /// <summary>
        /// A global direct write factory used by controls that deal with text.
        /// </summary>
        public static Factory DirectWriteFactory { get; set; }

        /// <summary>
        /// Gets or sets the dispatcher for UI events.
        /// 
        /// Must be set before initializing a <see cref="ControlView"/> instance.
        /// </summary>
        public static Dispatcher UiDispatcher { get; set; }
        
        private readonly Reactive _reactive;
        private readonly StateManager _stateManager = new StateManager();
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

                    view = ClosestParentViewOfType<ControlView>(view);
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

        private bool _interactionEnabled = true;

        public override bool Visible
        {
            get => base.Visible;
            set
            {
                base.Visible = value;
                _reactive.IsVisibleSubject.OnNext(value);
            }
        }

        /// <summary>
        /// If true, <see cref="Highlighted"/> is automatically toggled on and off whenever the user enters
        /// and exits this control with the mouse.
        /// </summary>
        public bool MouseOverHighlight { get; set; } = true;

        /// <summary>
        /// Event fired when this control view has successfully become the first responder
        /// </summary>
        public event EventHandler BecameFirstResponder;

        /// <summary>
        /// Event fired when this control view was resigned as a first responder
        /// </summary>
        public event EventHandler ResignedFirstResponder;

        /// <summary>
        /// Exposed reactive signal for this control
        /// </summary>
        public IReactive Rx => _reactive;

        /// <summary>
        /// Default implementation of <see cref="Next"/> searches the view hierarchy up.
        /// </summary>
        public IEventHandler Next => ClosestParentViewOfType<ControlView>(this);

        /// <summary>
        /// Default implementathi of this method returns false.
        /// </summary>
        public bool IsFirstResponder => ClosestParentViewOfType<RootControlView>(this)?.IsFirstResponder(this) ?? false;

        /// <summary>
        /// Default implementation of this method returns false.
        /// 
        /// Subclasses may override with true to indicate they can become first
        /// responders of keyboard events.
        /// </summary>
        public virtual bool CanBecomeFirstResponder => false;

        /// <summary>
        /// Default implementation of this method returns true.
        /// 
        /// Subclasses may override with false to indicate under certain circumstances
        /// that they cannot currently resign first responder status.
        /// </summary>
        public virtual bool CanResignFirstResponder => true;

        /// <summary>
        /// Whether mouse/keyboard interaction is enabled on this control.
        /// 
        /// If false, event handlers for keyboard and mouse are ignored for this
        /// and every child control, including custom event recognizers registered.
        /// 
        /// Defaults to true.
        /// </summary>
        public bool InteractionEnabled
        {
            get => _interactionEnabled;
            set
            {
                if (IsFirstResponder)
                    ResignFirstResponder();
                _interactionEnabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value specifying whether this control is selected.
        /// </summary>
        public bool Selected
        {
            get => _stateManager.Selected;
            set => _stateManager.SetSelected(value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control's main interaction is enabled.
        /// 
        /// This is separate from <see cref="InteractionEnabled"/>, as this solely dictates the
        /// control's response to input, instead of ignoring all input altogether.
        /// </summary>
        public bool Enabled
        {
            get => _stateManager.Enabled;
            set => _stateManager.SetEnabled(value);
        }

        /// <summary>
        /// Gets or sets a value specifying whether this control is highlighted.
        /// 
        /// The highlighted state indicates a possible actionable input is available for this control,
        /// ]like pressint Enter on the keyboard or clicking with a mouse button.
        /// </summary>
        public bool Highlighted
        {
            get => _stateManager.Highlighted;
            set => _stateManager.SetHighlighted(value);
        }

        /// <summary>
        /// Current computed state for this control view.
        /// </summary>
        public ControlViewState CurrentState => _stateManager.State;

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
        
        public ControlView()
        {
            if(UiDispatcher == null)
                throw new InvalidOperationException(
                        @"ControlView.UiDispatcher static property must be set prior to creating instances of ControlView. " +
                        @"Consider setting it to the proper UI dispatcher when creating the very first Form control of your program.");
            
            _reactive = new Reactive(UiDispatcher);

            // Hookup our mouse double click event directly on the control itself
            var time = TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime);
            
            var disposable =
                Rx.MouseDoubleClick(time, SystemInformation.DoubleClickSize)
                    .Subscribe(OnMouseDoubleClick);

            DisposeBag.Add(disposable);

            _stateManager.SetEnabled(true);

            _stateManager.OnStateChanged = OnChangedState;
        }

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

        protected override void OnResize()
        {
            base.OnResize();

            Layout();
        }

        protected virtual void OnChangedState(ControlViewState newState)
        {

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
        /// Fixed-step frame update.
        /// </summary>
        public virtual void OnFixedFrame([NotNull] FixedFrameEventArgs e) { }
        
        public virtual void OnMouseEnter()
        {
            _reactive.MouseEnterSubject.OnNext(Unit.Default);

            if (MouseOverHighlight)
                Highlighted = true;
        }

        public virtual void OnMouseLeave()
        {
            _reactive.MouseLeaveSubject.OnNext(Unit.Default);

            if (MouseOverHighlight)
                Highlighted = false;
        }

        public virtual void OnMouseClick(MouseEventArgs e)
        {
            _reactive.MouseClickSubject.OnNext(e);
        }

        public virtual void OnMouseDoubleClick([NotNull] MouseEventArgs e)
        {
            
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
            if (!Visible || !IsRecursivelyInteractiveEnabled)
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
        
        public virtual bool BecomeFirstResponder()
        {
            // Already first responder
            if (IsFirstResponder)
                return true;

            if (!CanBecomeFirstResponder)
                return false;

            var closest = ClosestParentViewOfType<RootControlView>(this);

            if (closest?.SetFirstResponder(this) ?? false)
            {
                BecameFirstResponder?.Invoke(this, EventArgs.Empty);
                _stateManager.SetIsFirstResponder(true);

                return true;
            }

            return false;
        }

        public virtual void ResignFirstResponder()
        {
            var closest = ClosestParentViewOfType<RootControlView>(this);
            if (closest == null)
                return;

            if (closest.RemoveAsFirstResponder(this))
            {
                ResignedFirstResponder?.Invoke(this, EventArgs.Empty);
                _stateManager.SetIsFirstResponder(false);
            }
        }

        #region Layout

        /// <summary>
        /// Called by <see cref="ControlView"/> when it's size has changed to request re-layouting.
        /// Can also be called by clients to force a re-layout of this control.
        /// 
        /// Avoid making any changes to <see cref="BaseView.Size"/> on this method as to not trigger an infinite
        /// recursion.
        /// 
        /// Note: Always call base.Layout() when overriding this method.
        /// </summary>
        public virtual void Layout()
        {

        }

        #endregion

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
        /// Removes a mouse recognizer from this control.
        /// 
        /// Mouse recognizer must be in list of registered recognizers on this control view.
        /// </summary>
        public void RemoveMouseRecognizer([NotNull] MouseEventRecognizer recognizer)
        {
            if (!Equals(recognizer.Control, this))
                return;

            recognizer.Control = null;

            _recognizers.Remove(recognizer);
        }

        /// <summary>
        /// Traverses the hierarchy of a given view, returning the first <see cref="T"/>
        /// that the method finds.
        /// 
        /// The method ignores <see cref="view"/> itself.
        /// </summary>
        [CanBeNull]
        private static T ClosestParentViewOfType<T>([NotNull] BaseView view) where T: BaseView
        {
            var next = view.Parent;
            while (next != null)
            {
                if (next is T type)
                    return type;

                next = next.Parent;
            }

            return null;
        }
        
        /// <summary>
        /// Available reactive signals for a <see cref="ControlView"/>
        /// </summary>
        public interface IReactive
        {
            Dispatcher Dispatcher { get; }

            IObservable<MouseEventArgs> MouseClick { get; }
            IObservable<MouseEventArgs> MouseDown { get; }
            IObservable<MouseEventArgs> MouseMove { get; }
            IObservable<MouseEventArgs> MouseUp { get; }
            IObservable<MouseEventArgs> MouseWheel { get; }
            IObservable<Unit> MouseEnter { get; }
            IObservable<Unit> MouseLeave { get; }
            IObservable<bool> IsVisible { get; }
        }

        /// <summary>
        /// Just for organizing a ControlView's reactive publisher/signals
        /// </summary>
        private sealed class Reactive : IReactive, IDisposable
        {
            public Dispatcher Dispatcher { get; }

            public Reactive(Dispatcher dispatcher)
            {
                Dispatcher = dispatcher;
            }
            
            public void Dispose()
            {
                MouseClickSubject.Dispose();
                MouseDownSubject.Dispose();
                MouseMoveSubject.Dispose();
                MouseUpSubject.Dispose();
                MouseWheelSubject.Dispose();
                MouseEnterSubject.Dispose();
                MouseLeaveSubject.Dispose();
            }

            public readonly Subject<MouseEventArgs> MouseClickSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseDownSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseMoveSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseUpSubject = new Subject<MouseEventArgs>();
            public readonly Subject<MouseEventArgs> MouseWheelSubject = new Subject<MouseEventArgs>();
            public readonly Subject<Unit> MouseEnterSubject = new Subject<Unit>();
            public readonly Subject<Unit> MouseLeaveSubject = new Subject<Unit>();
            public readonly Subject<bool> IsVisibleSubject = new Subject<bool>();

            public IObservable<MouseEventArgs> MouseClick => MouseClickSubject;
            public IObservable<MouseEventArgs> MouseDown => MouseDownSubject;
            public IObservable<MouseEventArgs> MouseMove => MouseMoveSubject;
            public IObservable<MouseEventArgs> MouseUp => MouseUpSubject;
            public IObservable<MouseEventArgs> MouseWheel => MouseWheelSubject;
            public IObservable<Unit> MouseEnter => MouseEnterSubject;
            public IObservable<Unit> MouseLeave => MouseLeaveSubject;
            public IObservable<bool> IsVisible => IsVisibleSubject;
        }

        private class StateManager
        {
            public bool Enabled { get; private set; }
            public bool Selected { get; private set; }
            public bool Highlighted { get; private set; }
            private bool IsFirstResponder { get; set; }

            public ControlViewState State { get; private set; } = ControlViewState.Normal;

            public Action<ControlViewState> OnStateChanged;

            public void SetEnabled(bool enabled)
            {
                Enabled = enabled;
                DeriveNewState();
            }

            public void SetSelected(bool selected)
            {
                Selected = selected;
                DeriveNewState();
            }

            public void SetHighlighted(bool highlighted)
            {
                Highlighted = highlighted;
                DeriveNewState();
            }

            public void SetIsFirstResponder(bool isFirstResponder)
            {
                IsFirstResponder = isFirstResponder;
                DeriveNewState();
            }

            private void DeriveNewState()
            {
                if (!Enabled)
                {
                    SetState(ControlViewState.Disabled);
                    return;
                }
                if (IsFirstResponder)
                {
                    SetState(ControlViewState.Focused);
                    return;
                }
                if (Selected)
                {
                    SetState(ControlViewState.Selected);
                    return;
                }
                if (Highlighted)
                {
                    SetState(ControlViewState.Highlighted);
                    return;
                }
                SetState(ControlViewState.Normal);
            }

            private void SetState(ControlViewState state)
            {
                if (state == State)
                    return;

                State = state;

                OnStateChanged?.Invoke(State);
            }
        }

        /// <summary>
        /// A value store that can store different values depending on the state
        /// of the control.
        /// 
        /// When requesting a value for a state that is not specified, the <see cref="ControlViewState.Normal"/>'s version
        /// of the value is returned. If that state is not present, the default type for T is finally returned instead.
        /// </summary>
        protected class StatedValueStore<T>
        {
            private readonly Dictionary<ControlViewState, T> _statesMap = new Dictionary<ControlViewState, T>();

            public T GetValue(ControlViewState state)
            {
                if (_statesMap.TryGetValue(state, out var value))
                    return value;

                if (_statesMap.TryGetValue(ControlViewState.Normal, out var normalValue))
                    return normalValue;

                return default;
            }

            public void SetValue(T value, ControlViewState state)
            {
                _statesMap[state] = value;
            }

            public void RemoveValueForState(ControlViewState state)
            {
                _statesMap.Remove(state);
            }
        }
    }
    
    /// <summary>
    /// State for a control view.
    /// 
    /// Used mostly by controls in order to style them according to different states.
    /// </summary>
    public enum ControlViewState
    {
        Normal,
        Highlighted,
        Selected,
        Disabled,
        Focused
    }

    /// <summary>
    /// Base class for all mouse event recognizers
    /// </summary>
    public class MouseEventRecognizer : IMouseEventHandler
    {
        /// <summary>
        /// Control this handler is associated with
        /// </summary>
        public ControlView Control { get; set; }
        
        public bool IsFirstResponder => false;
        public bool CanBecomeFirstResponder => false;

        public bool CanResignFirstResponder => true;
        
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

        public bool BecomeFirstResponder()
        {
            return false;
        }

        public void ResignFirstResponder()
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
    public class DragMouseEventRecognizer : MouseEventRecognizer
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

    /// <summary>
    /// Arguments for the <see cref="ControlView.OnFixedFrame"/> event.
    /// </summary>
    public class FixedFrameEventArgs: EventArgs
    {
        /// <summary>
        /// Time span for this fixed frame
        /// </summary>
        public TimeSpan TimeSpan { get; }

        public FixedFrameEventArgs(TimeSpan timeSpan)
        {
            TimeSpan = timeSpan;
        }
    }
}
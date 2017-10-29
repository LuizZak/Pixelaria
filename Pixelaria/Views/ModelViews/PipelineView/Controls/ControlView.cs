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
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Views.ModelViews.ExportPipelineFeatures;
using SharpDX.Direct2D1;

namespace Pixelaria.Views.ModelViews.PipelineView.Controls
{
    /// <summary>
    /// A base view with added UI interactivity capabilities
    /// </summary>
    internal class ControlView : SelfRenderingBaseView, IEventHandler, IDisposable
    {
        private readonly Reactive _reactive = new Reactive();

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
        /// Gets the content bounds of this control, which are the inner area of this control
        /// where content is effectively contained within. May be larger than this control's <see cref="BaseView.Bounds"/>.
        /// 
        /// Scroll views and other controls may modify bounds to alter the interactible area
        /// of the control.
        /// </summary>
        public virtual AABB ContentBounds => Bounds;

        ~ControlView()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
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
        [CanBeNull]
        public ControlView HitTestControl(Vector point)
        {
            // Test children first
            return ViewUnder(point, Vector.Zero, view => view is ControlView c && c.IsVisibleOnScreen()) as ControlView;
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
}
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
using System.Reactive.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;

using PixUI.Controls;
using Pixelaria.ExportPipeline;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    internal static class ControlViewRxUtils
    {
        /// <summary>
        /// Returns a signal that alternates between true and false as the user presses/releases the mouse
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
        /// Returns a signal that alternates between true and false as the user enter/exits the mouse
        /// over the control.
        /// 
        /// This observable only signals when the next <see cref="ControlView.IReactive.MouseEnter"/> or
        /// <see cref="ControlView.IReactive.MouseLeave"/> observables fire.
        /// </summary>
        public static IObservable<bool> IsMouseOver([NotNull] this ControlView.IReactive reactive)
        {
            var onDown = reactive.MouseEnter.Select(_ => true);
            var onUp = reactive.MouseLeave.Select(_ => false);

            return onDown.Merge(onUp);
        }

        /// <summary>
        /// Returns a signal that fires whenever the user double clicks the control within a specified time delay and
        /// distance tolerance.
        /// </summary>
        public static IObservable<MouseEventArgs> MouseDoubleClick([NotNull] this ControlView.IReactive reactive, TimeSpan delay, Size size)
        {
            return
                reactive
                    .MouseClick
                    .Buffer(delay, 2)
                    .Where(l => l.Count == 2)
                    .Where(l => Math.Abs(l[0].Location.X - l[1].Location.X) <= size.Width && Math.Abs(l[0].Location.Y - l[1].Location.Y) <= size.Height)
                    .Select(l => l[0]);
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
                                    .PxlWithLatestFrom(reactive.MouseMove.StartWith(e), (l, args) => args)
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
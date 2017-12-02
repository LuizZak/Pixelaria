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

using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Utils;

namespace Pixelaria.PixUI.Controls
{
    /// <summary>
    /// Base event handler interface
    /// </summary>
    internal interface IEventHandler
    {
        /// <summary>
        /// Returns whether this event handler is the first responder in the responder chain.
        /// </summary>
        bool IsFirstResponder { get; }

        /// <summary>
        /// Returns whether this event handler can become the first responder of targeted events.
        /// </summary>
        bool CanBecomeFirstResponder { get; }

        /// <summary>
        /// If this event handler is the first responder, returns whether it can currently resign
        /// the state.
        /// </summary>
        bool CanResignFirstResponder { get; }

        /// <summary>
        /// Asks this event handler to become the first responder on the event responder chain.
        /// 
        /// Returns a value specifying whether this event handler successfully became the first responder.
        /// If another event handler in the hierarchy is the first responder and it denies resigning it, or 
        /// this event handler returns <see cref="CanBecomeFirstResponder"/> as false, false is returned and
        /// this event handler does not become the first responder.
        /// 
        /// If this handler is already the first responder (see <see cref="IsFirstResponder"/>), the method
        /// returns true immediately.
        /// </summary>
        bool BecomeFirstResponder();

        /// <summary>
        /// Asks this event handler to dismiss its first responder status.
        /// </summary>
        void ResignFirstResponder();

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

        void HandleOrPass([NotNull] IEventRequest eventRequest);

        void OnMouseLeave();
        void OnMouseEnter();

        void OnMouseClick([NotNull] MouseEventArgs e);
        void OnMouseWheel([NotNull] MouseEventArgs e);
    }
}
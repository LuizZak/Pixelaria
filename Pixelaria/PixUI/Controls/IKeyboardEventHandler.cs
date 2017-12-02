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

namespace Pixelaria.PixUI.Controls
{
    /// <summary>
    /// Event handler for keyboard inputs
    /// </summary>
    internal interface IKeyboardEventHandler: IEventHandler
    {
        void OnKeyPress([NotNull] KeyPressEventArgs e);

        void OnKeyDown([NotNull] KeyEventArgs e);
        void OnKeyUp([NotNull] KeyEventArgs e);

        void OnPreviewKeyDown([NotNull] PreviewKeyDownEventArgs e);
    }

    /// <summary>
    /// Event requests for keyboard input
    /// </summary>
    internal interface IKeyboardEventRequest : IEventRequest
    {
        /// <summary>
        /// Gets the event this keyboard event request represents
        /// </summary>
        KeyboardEventType EventType { get; }
    }

    internal enum KeyboardEventType
    {
        KeyDown,
        KeyPress,
        KeyUp,
        PreviewKeyDown
    }
}

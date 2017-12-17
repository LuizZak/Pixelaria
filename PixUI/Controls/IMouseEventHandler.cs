﻿/*
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

namespace PixUI.Controls
{
    /// <summary>
    /// Interface for objects capable of handling mouse events
    /// </summary>
    public interface IMouseEventHandler: IEventHandler
    {
        void OnMouseDown([NotNull] MouseEventArgs e);
        void OnMouseMove([NotNull] MouseEventArgs e);
        void OnMouseUp([NotNull] MouseEventArgs e);
    }

    /// <summary>
    /// Event requests for mouse input
    /// </summary>
    public interface IMouseEventRequest : IEventRequest
    {
        /// <summary>
        /// Gets the event this mouse event request represents
        /// </summary>
        MouseEventType EventType { get; }
    }

    public enum MouseEventType
    {
        MouseDown,
        MouseMove,
        MouseUp,
        MouseClick,
        MouseDoubleClick,
        MouseWheel
    }
}
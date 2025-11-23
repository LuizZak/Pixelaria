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

using PixCore.Geometry;
using System;

namespace PixUI.Controls
{
    /// <summary>
    /// An interface for controls that can be displayed as dialog views.
    /// </summary>
    public interface IDialogControl : ISpatialReference, IRegionInvalidateable, IMouseEventHandler, IDisposable
    {
        /// <summary>
        /// Gets the dialog context flags for this dialog control.
        /// </summary>
        DialogControlContextFlags DialogContextFlags { get; }

        /// <summary>
        /// Event raised when this dialog control is about to be closed.
        /// </summary>
        event DialogControlClosing Closing;

        /// <summary>
        /// Event raised when this dialog control has been closed.
        /// </summary>
        event DialogControlClosed Closed;

        /// <summary>
        /// Event raised when this dialog control has been opened.
        /// </summary>
        event EventHandler Opened;

        /// <summary>
        /// Closes this dialog control.
        /// 
        /// Issues <see cref="Closing"/> and <see cref="Closed"/> events with the reason
        /// <see cref="DialogControlCloseReason.CloseCalled"/>.
        /// </summary>
        void Close();

        /// <summary>
        /// Closes this dialog control, using the provided close reason.
        ///
        /// Issues <see cref="Closing"/> and <see cref="Closed"/> events with the provided
        /// reason.
        /// </summary>
        void Close(DialogControlCloseReason reason);

        /// <summary>
        /// Raises the <see cref="Opened"/> event.
        /// </summary>
        void Show();
    }

    /// <summary>
    /// Delegate for a dialog close event.
    /// </summary>
    public delegate void DialogControlClosed(object sender, DialogControlCloseReason e);

    /// <summary>
    /// Delegate for a dialog closing event.
    /// </summary>
    public delegate void DialogControlClosing(object sender, DialogControlCloseReason e);

    /// <summary>
    /// Reason for the closing of a dialog control.
    /// </summary>
    public enum DialogControlCloseReason
    {
        /// <summary>
        /// The dialog was closed after the application lost focus.
        /// 
        /// Not all dialog types are automatically closed when app focus has changed.
        /// </summary>
        AppFocusChange = 0,

        /// <summary>
        /// The dialog was closed because another application was started.
        /// 
        /// Not all dialog types are automatically closed when an app is started.
        /// </summary>
        AppClicked = 1,

        /// <summary>
        /// The dialog was closed after an item from the dialog was clicked.
        /// 
        /// Relevant to context menu controls only.
        /// </summary>
        ItemClicked = 2,

        /// <summary>
        /// The dialog was closed after the user accepted one or more items.
        /// 
        /// Relevant to dialog boxes only.
        /// </summary>
        Accepted = 2,

        /// <summary>
        /// The dialog was closed after the user cancelled the operation.
        /// 
        /// Relevant to dialog boxes only.
        /// </summary>
        Cancelled = 3,

        /// <summary>
        /// The dialog was closed after the user pressed the 'Escape' key while the dialog was open,
        /// or some other keyboard event.
        /// </summary>
        Keyboard = 4,

        /// <summary>
        /// The dialog was closed by code calling 'Close()'.
        /// </summary>
        CloseCalled = 5
    }

    /// <summary>
    /// Flags for the exhibition of a <see cref="IDialogControl"/>.
    /// </summary>
    [Flags]
    public enum DialogControlContextFlags
    {
        /// <summary>
        /// The dialog behaves like a context menu: It should be automatically closed if focus is requested somewhere else.
        /// </summary>
        ContextMenu = 0b001,
    }
}

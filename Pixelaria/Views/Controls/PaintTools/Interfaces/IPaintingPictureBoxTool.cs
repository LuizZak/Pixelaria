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
using System.Windows.Forms;
using JetBrains.Annotations;

namespace Pixelaria.Views.Controls.PaintTools.Interfaces
{
    /// <summary>
    /// Specifies a Paint Tool to be used on a <see cref="PaintingOperationsPictureBox"/>
    /// </summary>
    internal interface IPaintingPictureBoxTool : IDisposable
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this tool is up
        /// </summary>
        Cursor ToolCursor { get; }

        /// <summary>
        /// Gets whether this Paint Tool has resources loaded
        /// </summary>
        bool Loaded { get; }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        void Initialize([NotNull] PaintingOperationsPictureBox targetPictureBox);
        
        /// <summary>
        /// Called when the bitmap currently being edited changes to a specified instance
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        void ChangeBitmap(Bitmap newBitmap);

        /// <summary>
        /// Called to notify this Paint Tool that the control is being redrawn.
        /// 
        /// Drawings made on this event handler appear under the picture box's image.
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void Paint([NotNull] PaintEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the foreground of the control is being redrawn.
        /// 
        /// Drawings made on this event handler appear above the picture box's image.
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void PaintForeground([NotNull] PaintEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse was clicked
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseClick([NotNull] MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseDown([NotNull] MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseMove([NotNull] MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseUp([NotNull] MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseLeave([NotNull] EventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseEnter([NotNull] EventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyDown([NotNull] KeyEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyUp([NotNull] KeyEventArgs e);
    }
}
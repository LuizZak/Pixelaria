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
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Specifies an empty paint operation
    /// </summary>
    public class NullPaintTool : IPaintTool
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public Cursor ToolCursor { get { return Cursors.Default; } }

        /// <summary>
        /// Gets whether this Paint Tool has resources loaded
        /// </summary>
        public bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox) { }

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        public void Destroy() { }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public void ChangeBitmap(Bitmap newBitmap) { }

        /// <summary>
        /// Called to notify this PaintTool that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void Paint(PaintEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintTool that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void PaintForeground(PaintEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseLeave(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseEnter(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintTool that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void KeyDown(KeyEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintTool that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void KeyUp(KeyEventArgs e) { }
    }
}
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

using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintTools.Abstracts;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Picker paint operation
    /// </summary>
    internal class PickerPaintTool : AbstractPaintTool
    {
        /// <summary>
        /// The last absolute position of the mouse
        /// </summary>
        protected Point lastMousePointAbsolute;

        /// <summary>
        /// Gets whether this Paint Tool has resources loaded
        /// </summary>
        public override bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(PaintingOperationsPictureBox targetPictureBox)
        {
            pictureBox = targetPictureBox;

            lastMousePointAbsolute = new Point(-1, -1);

            // Initialize the operation cursor
            var cursorMemoryStream = new MemoryStream(Properties.Resources.picker_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            Loaded = true;
        }
        
        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var absolute = GetAbsolutePoint(e.Location);
                ColorPickAtPoint(absolute, e.Button == MouseButtons.Left ? ColorIndex.FirstColor : ColorIndex.SecondColor);

                lastMousePointAbsolute = absolute;
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var absolute = GetAbsolutePoint(e.Location);

                if (absolute != lastMousePointAbsolute)
                {
                    ColorPickAtPoint(absolute, e.Button == MouseButtons.Left ? ColorIndex.FirstColor : ColorIndex.SecondColor);
                }

                lastMousePointAbsolute = absolute;
            }
        }
    }
}
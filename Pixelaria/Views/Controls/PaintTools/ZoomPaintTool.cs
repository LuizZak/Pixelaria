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
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintTools.Abstracts;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Zoom paint tool
    /// </summary>
    internal class ZoomPaintTool : BaseDraggingPaintTool
    {
        /// <summary>
        /// The relative point where the mouse was held down, in control coordinates
        /// </summary>
        private Point _mouseDownRelative;

        /// <summary>
        /// The relative point where the mouse is currently at, in control coordinates
        /// </summary>
        private Point _mousePointRelative;

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint tool on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the tool cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.zoom_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }
        
        /// <summary>
        /// Called to notify this PaintTool that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (mouseDown)
            {
                // Draw the zoom area
                Rectangle rec = GetRectangleArea(new [] { _mouseDownRelative, _mousePointRelative }, false);

                e.Graphics.ResetTransform();

                e.Graphics.DrawRectangle(Pens.Gray, rec);
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            _mousePointRelative = _mouseDownRelative = e.Location;
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            if (mouseDown)
            {
                pictureBox?.Invalidate(GetRectangleArea(new [] { _mouseDownRelative, _mousePointRelative }, false));

                _mousePointRelative = e.Location;

                pictureBox?.Invalidate(GetRectangleArea(new [] { _mouseDownRelative, _mousePointRelative }, false));
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            FinishOperation();

            base.MouseUp(e);
        }

        /// <summary>
        /// Finishes this ZoomTool's current operation
        /// </summary>
        public void FinishOperation()
        {
            if (!mouseDown)
                return;

            var internalPictureBox = pictureBox;
            if (internalPictureBox == null)
                return;

            var zoomArea = GetRectangleArea(new [] { _mouseDownRelative, _mousePointRelative }, false);

            internalPictureBox.Invalidate(zoomArea);

            float zoomX = internalPictureBox.Width / (float)zoomArea.Width;
            float zoomY = internalPictureBox.Height / (float)zoomArea.Height;

            if (zoomArea.Width < 2 && zoomArea.Height < 2)
            {
                zoomX = zoomY = 2f;

                zoomArea.X -= internalPictureBox.Width / 2;
                zoomArea.Y -= internalPictureBox.Height / 2;
            }

            zoomY = zoomX = Math.Min(zoomX, zoomY);

            internalPictureBox.Zoom = new PointF(internalPictureBox.Zoom.X * zoomX, internalPictureBox.Zoom.Y * zoomY);

            // Zoom in into the located region
            var relative = internalPictureBox.Offset;

            relative.X += (int)(zoomArea.X * zoomX);
            relative.Y += (int)(zoomArea.Y * zoomX);

            internalPictureBox.Offset = relative;
        }
    }
}
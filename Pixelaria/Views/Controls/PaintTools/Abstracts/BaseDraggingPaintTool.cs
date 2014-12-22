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

namespace Pixelaria.Views.Controls.PaintTools.Abstracts
{
    /// <summary>
    /// Implements a basic functionality for paint operations that require 'dragging to draw' features
    /// </summary>
    public abstract class BaseDraggingPaintTool : BasePaintTool
    {
        /// <summary>
        /// Whether the mouse is currently being held down
        /// </summary>
        protected bool mouseDown;

        /// <summary>
        /// The mouse button currently being held down
        /// </summary>
        protected MouseButtons mouseButton;

        /// <summary>
        /// The point at which the mouse was held down
        /// </summary>
        protected Point mouseDownAbsolutePoint;

        /// <summary>
        /// The current mouse point
        /// </summary>
        protected Point mouseAbsolutePoint;

        /// <summary>
        /// The last absolute mouse position
        /// </summary>
        protected Point lastMouseAbsolutePoint;

        /// <summary>
        /// The last rectangle drawn by the mouse
        /// </summary>
        protected Rectangle lastRect;

        /// <summary>
        /// Whether the SHIFT key is currently held down
        /// </summary>
        protected bool shiftDown;

        /// <summary>
        /// Whether the CONTROL key is currently held down
        /// </summary>
        protected bool ctrlDown;

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            shiftDown = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            Point imagePoint = GetAbsolutePoint(e.Location);

            if (WithinBounds(imagePoint))
            {
                mouseAbsolutePoint = imagePoint;
                mouseDownAbsolutePoint = imagePoint;
                lastMouseAbsolutePoint = imagePoint;

                mouseButton = e.Button;

                mouseDown = true;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            if (mouseDown)
            {
                Rectangle oldArea = lastRect;//GetCurrentRectangle(true);

                lastMouseAbsolutePoint = mouseAbsolutePoint;
                mouseAbsolutePoint = GetAbsolutePoint(e.Location);

                Rectangle newArea = GetCurrentRectangle(true);
                Rectangle newAreaAbs = GetRectangleArea(new [] { mouseDownAbsolutePoint, mouseAbsolutePoint }, false);

                if (shiftDown)
                {
                    newArea.Width = Math.Max(newArea.Width, newArea.Height);
                    newArea.Height = Math.Max(newArea.Width, newArea.Height);
                    newAreaAbs.Width = Math.Max(newAreaAbs.Width, newAreaAbs.Height);
                    newAreaAbs.Height = Math.Max(newAreaAbs.Width, newAreaAbs.Height);
                }

                newAreaAbs.Width++; newAreaAbs.Height++;
                pictureBox.OwningPanel.FireOperationStatusEvent(this, newAreaAbs.Width + " x " + newAreaAbs.Height);

                if (newArea != oldArea)
                {
                    oldArea.Inflate((int)(2 * pictureBox.Zoom.X), (int)(2 * pictureBox.Zoom.X));

                    newArea.Inflate((int)(2 * pictureBox.Zoom.X), (int)(2 * pictureBox.Zoom.X));

                    pictureBox.Invalidate(oldArea);
                    pictureBox.Invalidate(newArea);
                }

                lastRect = newArea;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            mouseDown = false;

            pictureBox.OwningPanel.FireOperationStatusEvent(this, "");
        }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void KeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ShiftKey)
            {
                shiftDown = true;
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                ctrlDown = true;
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void KeyUp(KeyEventArgs e)
        {
            if(e.KeyCode == Keys.ShiftKey)
            {
                shiftDown = false;
            }
            else if (e.KeyCode == Keys.ControlKey)
            {
                ctrlDown = false;
            }
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected virtual Rectangle GetCurrentRectangle(bool relative)
        {
            Rectangle rec = GetRectangleArea(new [] { mouseDownAbsolutePoint, mouseAbsolutePoint }, relative);

            if (shiftDown)
            {
                rec.Width = Math.Max(rec.Width, rec.Height);
                rec.Height = Math.Max(rec.Width, rec.Height);
            }

            return rec;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="pointList">An array of points that describe the corners of the rectangle</param>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected virtual Rectangle GetRectangleArea(Point[] pointList, bool relative)
        {
            Point p1 = pointList[0];

            if (relative)
            {
                p1 = Point.Truncate(GetRelativePoint(p1));
            }

            int minX = p1.X;
            int minY = p1.Y;

            int maxX = p1.X;
            int maxY = p1.Y;

            foreach (Point p in pointList)
            {
                p1 = p;

                if (relative)
                {
                    p1 = Point.Truncate(GetRelativePoint(p));
                }

                minX = Math.Min(p1.X, minX);
                minY = Math.Min(p1.Y, minY);

                maxX = Math.Max(p1.X, maxX);
                maxY = Math.Max(p1.Y, maxY);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Returns the smallest Rectangle object that encloses all points provided
        /// </summary>
        /// <param name="pointList">An array of points to convert</param>
        /// <returns>The smallest Rectangle object that encloses all points provided</returns>
        public static Rectangle GetRectangleAreaAbsolute(Point[] pointList)
        {
            int minX = pointList[0].X;
            int minY = pointList[0].Y;

            int maxX = pointList[0].X;
            int maxY = pointList[0].Y;

            foreach (Point p in pointList)
            {
                Point p1 = p;

                minX = Math.Min(p1.X, minX);
                minY = Math.Min(p1.Y, minY);

                maxX = Math.Max(p1.X, maxX);
                maxY = Math.Max(p1.Y, maxY);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
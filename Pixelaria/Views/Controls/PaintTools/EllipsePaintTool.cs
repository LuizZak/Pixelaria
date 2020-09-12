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
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements an Ellipse paint tool
    /// </summary>
    internal class EllipsePaintTool : AbstractShapeTool, IColoredPaintTool, ICompositingPaintTool, IFillModePaintTool
    {
        /// <summary>
        /// Initializes a new instance of the EllipsePaintTool class, setting the two drawing colors
        /// for the paint tool
        /// </summary>
        /// <param name="firstColor">The first color for the paint tool</param>
        /// <param name="secondColor">The second color for the paint tool</param>
        public EllipsePaintTool(Color firstColor, Color secondColor)
            : base(firstColor, secondColor)
        {

        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint tool on</param>
        public override void Initialize(PaintingOperationsPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the tool cursor
            var cursorMemoryStream = new MemoryStream(Properties.Resources.circle_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            base.Destroy();

            ToolCursor.Dispose();
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            var rec = GetRectangleArea(new [] { mouseDownAbsolutePoint, mouseAbsolutePoint }, relative);

            if (shiftDown)
            {
                rec.Width = Math.Max(rec.Width, rec.Height);
                rec.Height = Math.Max(rec.Width, rec.Height);
            }

            return rec;
        }

        /// <summary>
        /// Performs the shape paint operation with the given parameters
        /// </summary>
        /// <param name="color1">The first color to use when drawing the shape</param>
        /// <param name="color2">The second color to use when drawing the shape</param>
        /// <param name="area">The area of the shape to draw</param>
        /// <param name="bitmap">The Bitmap to draw the shape on</param>
        /// <param name="compMode">The CompositingMode to use when drawing the shape</param>
        /// <param name="opFillMode">The fill mode for this shape operation</param>
        /// <param name="registerUndo">Whether to register an undo task for this shape operation</param>
        [CanBeNull]
        public override ShapeUndoTask PerformShapeOperation(Color color1, Color color2, Rectangle area, [NotNull] Bitmap bitmap, CompositingMode compMode, OperationFillMode opFillMode, bool registerUndo)
        {
            var internalPictureBox = pictureBox;
            if (internalPictureBox == null)
                return null;

            ShapeUndoTask returnTask = null;

            if (registerUndo)
            {
                returnTask = new EllipseUndoTask(internalPictureBox.Bitmap, color1, color2, area, compMode, opFillMode);
                internalPictureBox.OwningPanel.UndoSystem.RegisterUndo(returnTask);
            }

            PerformEllipseOperation(color1, color2, area, bitmap, compMode, opFillMode);

            return returnTask;
        }

        /// <summary>
        /// Performs the shape paint operation with the given parameters
        /// </summary>
        /// <param name="color1">The first color to use when drawing the shape</param>
        /// <param name="color2">The second color to use when drawing the shape</param>
        /// <param name="area">The area of the shape to draw</param>
        /// <param name="gph">The Graphics to draw the shape on</param>
        /// <param name="compMode">The CompositingMode to use when drawing the shape</param>
        /// <param name="opFillMode">The fill mode for this shape operation</param>
        /// <param name="registerUndo">Whether to register an undo task for this shape operation</param>
        public override void PerformShapeOperation(Color color1, Color color2, Rectangle area, [NotNull] Graphics gph, CompositingMode compMode, OperationFillMode opFillMode, bool registerUndo)
        {
            PerformEllipseOperation(color1, color2, area, gph, compMode, opFillMode);
        }

        /// <summary>
        /// Performs the Ellipse paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the ellipse</param>
        /// <param name="secondColor">The second color to use when drawing the ellipse</param>
        /// <param name="area">The area of the ellipse to draw</param>
        /// <param name="bitmap">The Bitmap to draw the ellipse on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the ellipse</param>
        /// <param name="fillMode">The fill mode for this ellipse operation</param>
        public static void PerformEllipseOperation(Color firstColor, Color secondColor, Rectangle area, [NotNull] Bitmap bitmap, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            //FastBitmap fb = new FastBitmap(bitmap);
            //fb.Lock();

            /*int originx = area.X + area.Width / 2;
            int originy = area.Y;
            int width = area.Width;
            int height = area.Height;
            int hh = height * height;
            int ww = width * width;
            int hhww = hh * ww;
            int x0 = width;
            int dx = 0;

            // do the horizontal diameter
            for (int x = -width; x <= width; x++)
            {
                //setpixel(origin.x + x, origin.y);
                if (originx + x > 0 && originx + x < fb.Width && originy > 0 && originy < fb.Height)
                    fb.SetPixel(originx + x, originy, firstColor);
            }

            // now do both halves at the same time, away from the diameter
            for (int y = 1; y <= height; y++)
            {
                int x1 = x0 - (dx - 1);  // try slopes of dx - 1 or more
                for (; x1 > 0; x1--)
                    if (x1 * x1 * hh + y * y * ww <= hhww)
                        break;
                dx = x0 - x1;  // current approximation of the slope
                x0 = x1;

                for (int x = -x0; x <= x0; x++)
                {
                    if (originx + x > 0 && originx + x < fb.Width && originy - y > 0 && originy - y < fb.Height)
                    fb.SetPixel(originx + x, originy - y, firstColor);
                    if (originx + x > 0 && originx + x < fb.Width && originy + y > 0 && originy + y < fb.Height)
                    fb.SetPixel(originx + x, originy + y, firstColor);
                    //setpixel(origin.x + x, origin.y - y);
                    //setpixel(origin.x + x, origin.y + y);
                }
            }*/

            //fb.Unlock();

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CompositingMode = compositingMode;

                PerformEllipseOperation(firstColor, secondColor, area, graphics, compositingMode, fillMode);

                graphics.Flush();
            }
        }

        /// <summary>
        /// Performs the Ellipse paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the ellipse</param>
        /// <param name="secondColor">The second color to use when drawing the ellipse</param>
        /// <param name="area">The area of the ellipse to draw</param>
        /// <param name="graphics">The Graphics to draw the ellipse on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the ellipse</param>
        /// <param name="fillMode">The fill mode for this ellipse operation</param>
        public static void PerformEllipseOperation(Color firstColor, Color secondColor, Rectangle area,
            [NotNull] Graphics graphics, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            var brush = new SolidBrush((fillMode == OperationFillMode.SolidFillFirstColor ? firstColor : secondColor));

            graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

            if (fillMode == OperationFillMode.SolidFillFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor || fillMode == OperationFillMode.SolidFillSecondColor)
            {
                graphics.FillEllipse(brush, area);
            }

            if (fillMode == OperationFillMode.OutlineFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor)
            {
                var pen = new Pen(firstColor);

                graphics.DrawEllipse(pen, area);

                pen.Dispose();
            }

            brush.Dispose();
        }

        /// <summary>
        /// An ellipse undo task
        /// </summary>
        public class EllipseUndoTask : ShapeUndoTask, IDisposable
        {
            /// <summary>
            /// The area of the the image that was affected by the ellipse operation
            /// </summary>
            private Rectangle _area;

            /// <summary>
            /// The first color used to draw the ellipse
            /// </summary>
            private readonly Color _firstColor;

            /// <summary>
            /// The second color used to draw the ellipse
            /// </summary>
            private readonly Color _secondColor;

            /// <summary>
            /// The original slice of bitmap that represents the image region before the ellipse
            /// was drawn
            /// </summary>
            private readonly Bitmap _originalSlice;

            /// <summary>
            /// The bitmap where the ellipse was drawn on
            /// </summary>
            private readonly Bitmap _bitmap;

            /// <summary>
            /// The compositing mode of the paint operation
            /// </summary>
            private readonly CompositingMode _compositingMode;

            /// <summary>
            /// The fill mode for the paint operation
            /// </summary>
            private readonly OperationFillMode _fillMode;

            /// <summary>
            /// Initializes a new instance of the EllipseUndoTask class
            /// </summary>
            /// <param name="targetBitmap">The target bitmap of this EllipseUndoTask</param>
            /// <param name="firstColor">The first color to use when drawing the ellipse</param>
            /// <param name="secondColor">The second color to use when drawing the ellipse</param>
            /// <param name="area">The area of the ellipse to draw</param>
            /// <param name="compositingMode">The CompositingMode to use when drawing the ellipse</param>
            /// <param name="fillMode">The fill mode for this ellipse operation</param>
            public EllipseUndoTask(Bitmap targetBitmap, Color firstColor, Color secondColor, Rectangle area, CompositingMode compositingMode, OperationFillMode fillMode)
            {
                _firstColor = firstColor;
                _secondColor = secondColor;
                _area = area;
                _bitmap = targetBitmap;
                _compositingMode = compositingMode;
                _fillMode = fillMode;

                // Take the image slide now
                _originalSlice = new Bitmap(area.Width + 1, area.Height + 1);
                
                Graphics g = Graphics.FromImage(_originalSlice);
                g.DrawImage(_bitmap, new Point(-area.X, -area.Y));
                g.Flush();
                g.Dispose();
            }

            ~EllipseUndoTask()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposing)
                    return;

                _originalSlice.Dispose();
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public override void Clear()
            {
                _originalSlice.Dispose();
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public override void Undo()
            {
                // Redraw the original slice back to the image
                using (var g = Graphics.FromImage(_bitmap))
                {
                    g.SetClip(new Rectangle(_area.X, _area.Y, _originalSlice.Width, _originalSlice.Height));
                    g.Clear(Color.Transparent);
                    g.CompositingMode = CompositingMode.SourceCopy;

                    g.DrawImage(_originalSlice, new Rectangle(_area.X, _area.Y, _originalSlice.Width, _originalSlice.Height));

                    g.Flush();
                }
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public override void Redo()
            {
                // Draw the ellipse again
                PerformEllipseOperation(_firstColor, _secondColor, _area, _bitmap, _compositingMode, _fillMode);
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public override string GetDescription()
            {
                return "Ellipse";
            }
        }
    }
}
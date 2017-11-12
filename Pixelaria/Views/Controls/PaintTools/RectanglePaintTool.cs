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
    /// Implements a Rectangle paint operation
    /// </summary>
    internal class RectanglePaintTool : AbstractShapeTool, IColoredPaintTool, ICompositingPaintTool, IFillModePaintTool
    {
        /// <summary>
        /// Initialies a new instance of the RectanglePaintOperation class, setting the two drawing colors
        /// for the paint operation
        /// </summary>
        /// <param name="firstColor">The first color for the paint operation</param>
        /// <param name="secondColor">The second color for the paint operation</param>
        public RectanglePaintTool(Color firstColor, Color secondColor)
            : base(firstColor, secondColor)
        {
            
        }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(PaintingOperationsPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            var cursorMemoryStream = new MemoryStream(Properties.Resources.rect_cursor);
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
            var internalPictureBox = pictureBox;
            if (internalPictureBox == null)
                return Rectangle.Empty;

            var rec = GetRectangleArea(new [] { mouseDownAbsolutePoint, mouseAbsolutePoint }, relative);

            if (relative)
            {
                rec.Width += (int)internalPictureBox.Zoom.X;
                rec.Height += (int)internalPictureBox.Zoom.Y;
            }
            else
            {
                rec.Width ++;
                rec.Height ++;
            }

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
                returnTask = new RectangleUndoTask(internalPictureBox.Bitmap, color1, color2, area, compMode, opFillMode);
                internalPictureBox.OwningPanel.UndoSystem.RegisterUndo(returnTask);
            }

            PerformRectangleOperation(color1, color2, area, bitmap, compMode, opFillMode);

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
        public override void PerformShapeOperation(Color color1, Color color2, Rectangle area, Graphics gph, CompositingMode compMode, OperationFillMode opFillMode, bool registerUndo)
        {
            PerformRectangleOperation(color1, color2, area, gph, compMode, opFillMode);
        }

        /// <summary>
        /// Performs the Rectangle paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the rectangle</param>
        /// <param name="secondColor">The second color to use when drawing the rectangle</param>
        /// <param name="area">The area of the rectangle to draw</param>
        /// <param name="bitmap">The Bitmap to draw the rectangle on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
        /// <param name="fillMode">The fill mode for this rectangle operation</param>
        public static void PerformRectangleOperation(Color firstColor, Color secondColor, Rectangle area,
            [NotNull] Bitmap bitmap, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CompositingMode = compositingMode;

                PerformRectangleOperation(firstColor, secondColor, area, graphics, compositingMode, fillMode);

                graphics.Flush();
            }
        }

        /// <summary>
        /// Performs the Rectangle paint operation with the given parameters
        /// </summary>
        /// <param name="firstColor">The first color to use when drawing the rectangle</param>
        /// <param name="secondColor">The second color to use when drawing the rectangle</param>
        /// <param name="area">The area of the rectangle to draw</param>
        /// <param name="graphics">The Graphics to draw the rectangle on</param>
        /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
        /// <param name="fillMode">The fill mode for this rectangle operation</param>
        public static void PerformRectangleOperation(Color firstColor, Color secondColor, Rectangle area, Graphics graphics, CompositingMode compositingMode, OperationFillMode fillMode)
        {
            Brush brush = new SolidBrush((fillMode == OperationFillMode.SolidFillFirstColor ? firstColor : secondColor));

            if (fillMode == OperationFillMode.SolidFillFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor || fillMode == OperationFillMode.SolidFillSecondColor)
            {
                var nArea = area;

                if (fillMode == OperationFillMode.OutlineFirstColorFillSecondColor)
                    nArea.Inflate(-1, -1);

                graphics.FillRectangle(brush, nArea);
            }

            if (fillMode == OperationFillMode.OutlineFirstColor || fillMode == OperationFillMode.OutlineFirstColorFillSecondColor)
            {
                var pen = new Pen(firstColor);

                area.Inflate(-1, -1);

                RectangleF nArea = area;

                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                nArea.X -= 0.5f;
                nArea.Y -= 0.5f;
                nArea.Width += 1f;
                nArea.Height += 1f;

                graphics.DrawRectangle(pen, nArea.X, nArea.Y, nArea.Width, nArea.Height);

                pen.Dispose();
            }

            brush.Dispose();
        }

        /// <summary>
        /// A rectangle undo task
        /// </summary>
        public class RectangleUndoTask : ShapeUndoTask, IDisposable
        {
            /// <summary>
            /// The area of the the image that was affected by the Rectangle operation
            /// </summary>
            private readonly Rectangle _area;

            /// <summary>
            /// The first color used to draw the Rectangle
            /// </summary>
            private readonly Color _firstColor;

            /// <summary>
            /// The second color used to draw the Rectangle
            /// </summary>
            private readonly Color _secondColor;

            /// <summary>
            /// The original slice of bitmap that represents the image region before the rectangle
            /// was drawn
            /// </summary>
            private readonly Bitmap _originalSlice;

            /// <summary>
            /// The bitmap where the Rectangle was drawn on
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
            /// Initializes a new instance of the RectangleUndoTask class
            /// </summary>
            /// <param name="bitmap">The target bitmap of this RectangleUndoTask</param>
            /// <param name="firstColor">The first color to use when drawing the rectangle</param>
            /// <param name="secondColor">The second color to use when drawing the rectangle</param>
            /// <param name="area">The area of the rectangle to draw</param>
            /// <param name="compositingMode">The CompositingMode to use when drawing the rectangle</param>
            /// <param name="fillMode">The fill mode for this rectangle operation</param>
            public RectangleUndoTask(Bitmap bitmap, Color firstColor, Color secondColor, Rectangle area, CompositingMode compositingMode, OperationFillMode fillMode)
            {
                _firstColor = firstColor;
                _secondColor = secondColor;
                _area = area;
                _bitmap = bitmap;
                _compositingMode = compositingMode;
                _fillMode = fillMode;

                // Take the image slide now
                _originalSlice = new Bitmap(area.Width, area.Height);

                using (var g = Graphics.FromImage(_originalSlice))
                {
                    g.DrawImage(_bitmap, new Point(-area.X, -area.Y));
                    g.Flush();
                }
            }

            ~RectangleUndoTask()
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
                    g.SetClip(_area);
                    g.Clear(Color.Transparent);
                    g.CompositingMode = CompositingMode.SourceCopy;
                
                    g.DrawImage(_originalSlice, _area);

                    g.Flush();
                }
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public override void Redo()
            {
                // Draw the rectangle again
                PerformRectangleOperation(_firstColor, _secondColor, _area, _bitmap, _compositingMode, _fillMode);
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public override string GetDescription()
            {
                return "Rectangle";
            }
        }
    }
}
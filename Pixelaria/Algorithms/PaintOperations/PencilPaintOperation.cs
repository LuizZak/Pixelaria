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
using Pixelaria.Algorithms.PaintOperations.Abstracts;
using Pixelaria.Algorithms.PaintOperations.Interfaces;
using Pixelaria.Algorithms.PaintOperations.UndoTasks;
using Pixelaria.Utils;

namespace Pixelaria.Algorithms.PaintOperations
{
    /// <summary>
    /// Defines the behavior for a pencil type paint operation that works by calling 'MoveTo's and 'DrawTo's
    /// </summary>
    public class PencilPaintOperation : BasicContinuousPaintOperation, IPencilOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// Gets or sets the paint color attributed to this pencil operation
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the compositing mode for this pencil operation
        /// </summary>
        public CompositingMode CompositingMode { get; set; }

        /// <summary>
        /// Gets a value specifying whether the opration is currently accumulating the alpha transparency of the pixels it is affecting.
        /// Having this true means the paint operation accumulates the alpha of pixels it has already drawn over, and having it false means it only affects
        /// pixels it has not been rendered on still. Disabled alpha accumulation has a small performance penalty over having it enabled
        /// </summary>
        public bool AccumulateAlpha { get; private set; }

        /// <summary>
        /// The point that specified the current pencil tip
        /// </summary>
        protected Point pencilTip;

        /// <summary>
        /// The fast bitmap used during the operation
        /// </summary>
        protected FastBitmap fastBitmap;

        /// <summary>
        /// Whether the pencil tip is being 'pressed' into the bitmap.
        /// Calls to DrawTo() set the field to true, and calls to MoveTo() set to false.
        /// At the time of creation of the pencil paint operation, it is set to false always
        /// This field is used during sequential DrawTo calls to avoid drawing duplicated pixels where the lines meet
        /// </summary>
        protected bool pencilTipPressed;

        /// <summary>
        /// The pixel history tracker associated with this pencil paint operation, used to track pixels already drawn over when the AccumulateAlpha is set to true
        /// </summary>
        protected PixelHistoryTracker pixelsDrawn;

        /// <summary>
        /// The object to notify to when this operation has plotted a pixel
        /// </summary>
        public IPlottingOperationNotifier Notifier { get; set; }

        /// <summary>
        /// Specifies a delegate for the plot function used by the DrawLine method
        /// </summary>
        /// <param name="point">The point to plot the line at</param>
        public delegate void PlotFunction(Point point);

        /// <summary>
        /// Initializes a new PencilPaintOperation, with the specified target bitmap as target
        /// </summary>
        /// <param name="targetBitmap">The bitmap to perform the operation on</param>
        /// <param name="pencilTip">The tip of the pencil </param>
        public PencilPaintOperation(Bitmap targetBitmap, Point pencilTip = new Point()) : base(targetBitmap)
        {
            this.pencilTip = pencilTip;
        }

        /// <summary>
        /// Moves the pencil tip to point to a specific spot, without drawing in the process
        /// </summary>
        /// <param name="x">The position to move the pencil tip to</param>
        /// <param name="y">The position to move the pencil tip to</param>
        public void MoveTo(int x, int y)
        {
            pencilTip.X = x;
            pencilTip.Y = y;

            pencilTipPressed = false;
        }

        /// <summary>
        /// Moves the pencil tip to point to a specific spot, drawing between the last and new positions.
        /// If the operation has not been started by calling <see cref="BasicContinuousPaintOperation.StartOpertaion"/>, an exception is thrown
        /// </summary>
        /// <param name="x">The position to move the pencil tip to</param>
        /// <param name="y">The position to move the pencil tip to</param>
        /// <exception cref="InvalidOperationException">StartOperation() was not called prior to this method</exception>
        public void DrawTo(int x, int y)
        {
            if (!operationStarted)
            {
                throw new InvalidOperationException("The StartOperation() method must be caled prior to this method");
            }

            Point newPencilTip = new Point(x, y);

            InvokePlotsOnLine(PlotPoint, pencilTip, newPencilTip, pencilTipPressed);

            pencilTipPressed = true;

            pencilTip = newPencilTip;
        }

        /// <summary>
        /// Plots a pencil point at the specified point coordinates
        /// </summary>
        /// <param name="point">The point to plot at</param>
        protected virtual void PlotPoint(Point point)
        {
            if (!AccumulateAlpha && pixelsDrawn.ContainsPixel(point.X, point.Y))
                return;

            Color oldColor = fastBitmap.GetPixel(point.X, point.Y);
            Color newColor = GetBlendedColor(oldColor);
            fastBitmap.SetPixel(point.X, point.Y, newColor);

            if (!AccumulateAlpha)
            {
                pixelsDrawn.RegisterPixel(point.X, point.Y, oldColor, newColor);
                return;
            }

            if (Notifier != null)
                Notifier.PlottedPixel(point, oldColor.ToArgb(), newColor.ToArgb());
        }

        /// <summary>
        /// Returns a color that represents the given color blended with this pencil paint operation's blend information.
        /// The resulting color depends on the Color and CompositingMode of this paint operation
        /// </summary>
        /// <param name="color">The color to blend</param>
        /// <returns>A blended version of the specified color</returns>
        protected Color GetBlendedColor(Color color)
        {
            if (CompositingMode == CompositingMode.SourceCopy)
            {
                return Color;
            }

            return Utilities.FlattenColor(color, Color);
        }

        /// <summary>
        /// Starts this pencil paint operation
        /// </summary>
        public override void StartOpertaion()
        {
            StartOpertaion(true);
        }

        /// <summary>
        /// Starts this pencil paint operation, specifying whethe to accumulate the alpha of the pixels
        /// </summary>
        /// <param name="accumulateAlpha">Whether to accumulate the trasparency of the pixels as the pencil draws over them repeatedly</param>
        public void StartOpertaion(bool accumulateAlpha)
        {
            base.StartOpertaion();

            fastBitmap = new FastBitmap(targetBitmap);
            fastBitmap.Lock();

            AccumulateAlpha = accumulateAlpha;

            if(!accumulateAlpha)
            {
                pixelsDrawn = new PixelHistoryTracker(true, false, targetBitmap.Width);
            }
        }

        /// <summary>
        /// Finishes this pencil paint operation
        /// </summary>
        public override void FinishOperation()
        {
            base.FinishOperation();

            fastBitmap.Unlock();
            fastBitmap.Dispose();
        }

        /// <summary>
        /// Invokes a speicifed plot function for each pixel that passes through on the speicfied line
        /// </summary>
        /// <param name="plotFunction">The fuction to call to plot the lines</param>
        /// <param name="startPoint">The start point for the line</param>
        /// <param name="endPoint">The end point of the line</param>
        /// <param name="ignoreFirstPlot">Whether to ignore the first plot of the sequence and not call the plot function on it</param>
        protected void InvokePlotsOnLine(PlotFunction plotFunction, Point startPoint, Point endPoint, bool ignoreFirstPlot = false)
        {
            int x0 = startPoint.X;
            int y0 = startPoint.Y;
            int x1 = endPoint.X;
            int y1 = endPoint.Y;

            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t = x0;
                x0 = y0;
                y0 = t;

                t = x1;
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t = x0;
                x0 = x1;
                x1 = t;

                t = y0;
                y0 = y1;
                y1 = t;
            }
            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;

            if (y0 < y1)
                ystep = 1;
            else
                ystep = -1;

            // The point that represents each pixel to plot
            Point p = new Point();

            // Iterate through and plot the line
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    p.X = y;
                    p.Y = x;
                }
                else
                {
                    p.X = x;
                    p.Y = y;
                }

                if (!(ignoreFirstPlot && p == startPoint))
                {
                    plotFunction(p);
                }

                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }
        }
    }

    /// <summary>
    /// Plug-in class for generating undo tasks for plotting-type paint operations
    /// </summary>
    public class PlottingPaintUndoGenerator : IPlottingOperationNotifier
    {
        /// <summary>
        /// Gets the undo task that is being generated by this generator
        /// </summary>
        public PerPixelUndoTask UndoTask { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool CheckExisitingPixels { get; set; }

        /// <summary>
        /// Initializes a new instance of the PixelUndoTask, creating the underlying undo task
        /// </summary>
        /// <param name="bitmap">The target bitmap for hte undo operation</param>
        /// <param name="description">A description to use for this UndoTask</param>
        /// <param name="indexPixels">Whether to index the pixels being added so they appear sequentially on the pixel list</param>
        /// <param name="keepReplacedOriginals">Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.</param>
        public PlottingPaintUndoGenerator(Bitmap bitmap, string description, bool indexPixels = true, bool keepReplacedOriginals = true)
        {
            UndoTask = new PerPixelUndoTask(bitmap, description, indexPixels, keepReplacedOriginals);
        }

        /// <summary>
        /// Initializes a new instance of the PixelUndoTask with a specified undo task to use into this generator
        /// </summary>
        /// <param name="undoTask">The undo task to associate to this undo generator</param>
        public PlottingPaintUndoGenerator(PerPixelUndoTask undoTask)
        {
            UndoTask = undoTask;
        }

        /// <summary>
        /// Method called whenever the pencil operation has plotted a pixel on the underlying bitmap
        /// </summary>
        /// <param name="point">The position of the plot</param>
        /// <param name="oldColor">The old color of the pixel, before the plot</param>
        /// <param name="newColor">The new color of the pixel, after the plot</param>
        public void PlottedPixel(Point point, int oldColor, int newColor)
        {
            UndoTask.PixelHistoryTracker.RegisterPixel(point.X, point.Y, oldColor, newColor);
        }
    }
}
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
using FastBitmapLib;
using JetBrains.Annotations;
using Pixelaria.Algorithms.PaintOperations.Abstracts;
using Pixelaria.Algorithms.PaintOperations.Interfaces;
using Pixelaria.Algorithms.PaintOperations.UndoTasks;
using PixelariaLib.Utils;

namespace Pixelaria.Algorithms.PaintOperations
{
    /// <summary>
    /// Defines the behavior for a pencil type paint operation that works by calling 'MoveTo's and 'DrawTo's
    /// </summary>
    public class PencilPaintOperation : BasicContinuousPaintOperation, IDisposable, IPencilOperation, IColoredPaintOperation, ICompositingPaintOperation, ISizedPaintOperation
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
        /// Gets or sets the size of this pencil operation
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the target bitmap for this PencilPaintOperation
        /// </summary>
        public override Bitmap TargetBitmap
        {
            get => base.TargetBitmap;
            set
            {
                base.TargetBitmap = value;

                if(value != null)
                {
                    bitmapWidth = value.Width;
                    bitmapHeight = value.Height;
                }
            }
        }

        /// <summary>
        /// Gets a value specifying whether the operation is currently accumulating the alpha transparency of the pixels it is affecting.
        /// Having this true means the paint operation accumulates the alpha of pixels it has already drawn over, and having it false means it only affects
        /// pixels it has not been rendered on still
        /// </summary>
        public bool AccumulateAlpha { get; private set; }

        /// <summary>
        /// Gets or sets the object to notify to when this operation has plotted a pixel
        /// </summary>
        public IPlottingOperationNotifier Notifier { get; set; }

        /// <summary>
        /// Gets or sets the color blender used by this paint operation in order to blend the colors
        /// </summary>
        public IColorBlender ColorBlender { get; set; }

        /// <summary>
        /// The point that specified the current pencil tip
        /// </summary>
        protected Point pencilTip;

        /// <summary>
        /// The fast bitmap used during the operation
        /// </summary>
        protected FastBitmap fastBitmap;

        /// <summary>
        /// Whether to use a FastBitmap that locks and unlocks during StartOperation() and FinishOperation() calls
        /// </summary>
        protected bool useFastBitmap;

        /// <summary>
        /// Whether the pencil tip is being 'pressed' into the bitmap.
        /// Calls to DrawTo() set the field to true, and calls to MoveTo() set to false.
        /// At the time of creation of the pencil paint operation, it is set to false always
        /// This field is used during sequential DrawTo calls to avoid drawing duplicated pixels where the lines meet
        /// </summary>
        protected bool pencilTipPressed;

        /// <summary>
        /// The width of the current bitmap being manipulated
        /// </summary>
        protected int bitmapWidth;

        /// <summary>
        /// The height of the current bitmap being manipulated
        /// </summary>
        protected int bitmapHeight;

        /// <summary>
        /// The pixel history tracker associated with this pencil paint operation, used to track pixels already drawn over when the AccumulateAlpha is set to true
        /// </summary>
        protected PixelHistoryTracker pixelsDrawn;

        /// <summary>
        /// Specifies a delegate for the plot function used by the DrawLine method
        /// </summary>
        /// <param name="point">The point to plot the line at</param>
        /// <param name="size">The size of the brush to plot at</param>
        /// <param name="fillSize">
        /// The size of the fill of the disk. Leaving 1 will produce an outline circle, with progressive values producing a thicker inside.
        /// Leaving 0 will default to a completely filled circle.
        /// </param>
        public delegate void PlotFunction(Point point, int size, int fillSize = 0);

        /// <summary>
        /// Initializes a new PencilPaintOperation, with the specified target bitmap as target
        /// </summary>
        /// <param name="targetBitmap">The bitmap to perform the operation on</param>
        /// <param name="useFastBitmap">Whether to use a FastBitmap that locks and unlocks during StartOperation() and FinishOperation() calls</param>
        /// <param name="pencilTipPoint">The tip of the pencil </param>
        public PencilPaintOperation([NotNull] Bitmap targetBitmap, bool useFastBitmap = false, Point pencilTipPoint = new Point()) : base(targetBitmap)
        {
            bitmapWidth = targetBitmap.Width;
            bitmapHeight = targetBitmap.Height;

            pencilTip = pencilTipPoint;
            ColorBlender = new DefaultColorBlender();
            Size = 1;
            this.useFastBitmap = useFastBitmap;
        }

        ~PencilPaintOperation()
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
            if (!disposing || fastBitmap == null)
                return;

            fastBitmap.Dispose();
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
                throw new InvalidOperationException("The StartOperation() method must be called prior to this method");
            }

            var newPencilTip = new Point(x, y);

            // Temporarily switch to the fast bitmap for faster plotting
            bool oldUseFastBitmap = useFastBitmap;
            if (!useFastBitmap)
            {
                fastBitmap = targetBitmap.FastLock();
                useFastBitmap = true;
            }

            // Detect the algorithm to use based on the pencil's size
            InvokePlotsOnLine(PlotLinePoint, pencilTip, newPencilTip, Size, pencilTipPressed);

            // Switch back the fast bitmap in case it was previously disabled
            if (useFastBitmap && !oldUseFastBitmap)
            {
                fastBitmap.Dispose();
                useFastBitmap = false;
            }

            pencilTipPressed = true;

            pencilTip = newPencilTip;
        }

        /// <summary>
        /// Plots a pencil point at the specified point coordinates
        /// </summary>
        /// <param name="pointX">The X position to plot at</param>
        /// <param name="pointY">The Y position to plot at</param>
        public virtual void PlotPixel(int pointX, int pointY)
        {
            // Test boundaries
            if (!WithinBounds(pointX, pointY))
                return;

            if (!AccumulateAlpha && pixelsDrawn.ContainsPixel(pointX, pointY))
                return;

            Color oldColor = (useFastBitmap ? fastBitmap.GetPixel(pointX, pointY) : targetBitmap.GetPixel(pointX, pointY));
            Color newColor = GetBlendedColor(oldColor);

            uint oldColorArgb = unchecked((uint)oldColor.ToArgb());
            uint newColorArgb = unchecked((uint)newColor.ToArgb());

            // If the colors are virtually the same, quit early
            if (oldColorArgb == newColorArgb)
                return;

            if (useFastBitmap)
            {
                fastBitmap.SetPixel(pointX, pointY, newColorArgb);
            }
            else
            {
                targetBitmap.SetPixel(pointX, pointY, newColor);
            }

            if (!AccumulateAlpha)
            {
                pixelsDrawn.RegisterPixel(pointX, pointY, oldColorArgb, newColorArgb);
            }

            Notifier?.PlottedPixel(new Point(pointX, pointY), (int)oldColorArgb, (int)newColorArgb);
        }

        /// <summary>
        /// Plots a single line point segment at the specified point coordinates
        /// </summary>
        /// <param name="point">The point to plot at</param>
        /// <param name="size">The size of the pencil brush to plot at</param>
        /// <param name="fillSize">
        /// The size of the fill of the disk. Leaving 1 will produce an outline circle, with progressive values producing a thicker inside.
        /// Leaving 0 will default to a completely filled circle.
        /// </param>
        protected virtual void PlotLinePoint(Point point, int size, int fillSize = 0)
        {
            if (Size == 1)
            {
                PlotPixel(point.X, point.Y);
                return;
            }

            int px = point.X;
            int py = point.Y;
            int sizeSqrd = size * size;

            // 0 fill size: Fill whole circle
            // We also draw a full circle when accumulate alpha mode is on so the strokes end up with the expected blend color
            if (AccumulateAlpha || fillSize == 0)
            {
                for (int y = -size; y <= size; y++)
                {
                    int ySqrd = y * y;
                    for (int x = -size; x <= size; x++)
                    {
                        if (x * x + ySqrd <= sizeSqrd)
                        {
                            PlotPixel(px + x, py + y);
                        }
                    }
                }

                return;
            }

            // Draw the circle with a tolerance to use as borders
            int fillSqrd = (size - fillSize) * (size - fillSize);

            for (int y = -size; y <= size; y++)
            {
                int ySqrd = y * y;
                for (int x = -size; x <= size; x++)
                {
                    int sqrd = x * x + ySqrd;
                    if (sqrd <= sizeSqrd && sqrd >= fillSqrd)
                    {
                        PlotPixel(px + x, py + y);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a color that represents the given color blended with this pencil paint operation's blend information.
        /// The resulting color depends on the Color and CompositingMode of this paint operation
        /// </summary>
        /// <param name="backColor">The color to blend</param>
        /// <returns>A blended version of the specified color</returns>
        protected Color GetBlendedColor(Color backColor)
        {
            return ColorBlender.BlendColors(backColor, Color, CompositingMode);
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

            if (useFastBitmap)
            {
                fastBitmap = new FastBitmap(targetBitmap);
                fastBitmap.Lock();
            }

            // Ignore alpha accumulation if the color has no transparency
            AccumulateAlpha = (accumulateAlpha && Color.A != 255);

            if(!AccumulateAlpha)
            {
                pixelsDrawn = new PixelHistoryTracker(true, targetBitmap.Width);
            }

            Notifier?.OperationStarted(AccumulateAlpha);
        }

        /// <summary>
        /// Finishes this pencil paint operation
        /// </summary>
        public override void FinishOperation()
        {
            base.FinishOperation();

            if (useFastBitmap)
            {
                fastBitmap.Unlock();
                fastBitmap.Dispose();
            }

            // Send notifications
            Notifier?.OperationFinished(pixelsDrawn);

            pixelsDrawn = null;
        }

        /// <summary>
        /// Returns whether the given coordinates is within the image bounds
        /// </summary>
        /// <param name="x">The X coordinate to get whether or not it's within the image</param>
        /// <param name="y">The Y coordinate to get whether or not it's within the image</param>
        /// <returns>Whether the given coordinates is within the image bounds</returns>
        protected virtual bool WithinBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < bitmapWidth && y < bitmapHeight;
        }

        /// <summary>
        /// Invokes a speicifed plot function for each pixel that passes through on the speicfied line
        /// </summary>
        /// <param name="plotFunction">The fuction to call to plot the lines</param>
        /// <param name="startPoint">The start point for the line</param>
        /// <param name="endPoint">The end point of the line</param>
        /// <param name="size">The size of the line to plot</param>
        /// <param name="ignoreFirstPlot">Whether to ignore the first plot of the sequence and not call the plot function on it</param>
        protected void InvokePlotsOnLine([NotNull] PlotFunction plotFunction, Point startPoint, Point endPoint, int size, bool ignoreFirstPlot = false)
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
                    plotFunction(p, size, (p == startPoint || p == endPoint) ? 0 : 2);
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
    /// The default color blender for the drawing operations
    /// </summary>
    public class DefaultColorBlender : IColorBlender
    {
        /// <summary>
        /// Returns a Color that represents the blend of the two provided background and foreground colors
        /// </summary>
        /// <param name="backColor">The background color to blend</param>
        /// <param name="foreColor">The foreground color to blend</param>
        /// <param name="compositingMode"></param>
        /// <returns>The blend result of the two colors</returns>
        public Color BlendColors(Color backColor, Color foreColor, CompositingMode compositingMode)
        {
            if (compositingMode == CompositingMode.SourceCopy)
            {
                return foreColor;
            }

            return Utilities.FlattenColor(backColor, foreColor);
        }
    }

    /// <summary>
    /// Plug-in class for generating undo tasks for plotting-type paint operations
    /// </summary>
    public class PlottingPaintUndoGenerator : IPlottingOperationNotifier
    {
        /// <summary>
        /// The target bitmap for the undo operation
        /// </summary>
        private readonly Bitmap _bitmap;

        /// <summary>
        /// The description for the generated undo task
        /// </summary>
        private readonly string _description;

        /// <summary>
        /// Whether to register the pixels manually, or await the call for OperationFinished to record the pixels modified
        /// </summary>
        private bool _registerPixels;

        /// <summary>
        /// Gets the undo task that is being generated by this generator
        /// </summary>
        public PerPixelUndoTask UndoTask { get; private set; }

        /// <summary>
        /// Whether to ignore duplicated pixels during calls to PlottedPixel
        /// </summary>
        public bool IgnoreDuplicatedPlots { get; set; }

        /// <summary>
        /// Initializes a new instance of the PixelUndoTask, creating the underlying undo task
        /// </summary>
        /// <param name="bitmap">The target bitmap for hte undo operation</param>
        /// <param name="description">A description to use for this UndoTask</param>
        /// <param name="keepReplacedUndos">
        ///     Whether to keep the first color of pixels that are being replaced. When replacing with this flag on, only the redo color is set, the original undo color being unmodified.
        /// </param>
        /// <param name="ignoreDuplicatedPlots">Whether to ignore duplicated pixels during calls to PlottedPixel</param>
        public PlottingPaintUndoGenerator([NotNull] Bitmap bitmap, string description, bool keepReplacedUndos = true, bool ignoreDuplicatedPlots = false)
            : this(new PerPixelUndoTask(bitmap, description, keepReplacedUndos), ignoreDuplicatedPlots)
        {
            _bitmap = bitmap;
            _description = description;
        }

        /// <summary>
        /// Initializes a new instance of the PixelUndoTask with a specified undo task to use into this generator
        /// </summary>
        /// <param name="undoTask">The undo task to associate to this undo generator</param>
        /// <param name="ignoreDuplicatedPlots">Whether to ignore duplicated pixels during calls to PlottedPixel</param>
        public PlottingPaintUndoGenerator([NotNull] PerPixelUndoTask undoTask, bool ignoreDuplicatedPlots = true)
        {
            IgnoreDuplicatedPlots = ignoreDuplicatedPlots;
            UndoTask = undoTask;
        }

        /// <summary>
        /// Method called to notify the plotting operation has started
        /// </summary>
        /// <param name="accumulateAlpha">Whether the plotting operation has alpha accumulation mode on</param>
        public void OperationStarted(bool accumulateAlpha)
        {
            _registerPixels = accumulateAlpha;
        }

        /// <summary>
        /// Method called whenever the pencil operation has plotted a pixel on the underlying bitmap
        /// </summary>
        /// <param name="point">The position of the plot</param>
        /// <param name="oldColor">The old color of the pixel, before the plot</param>
        /// <param name="newColor">The new color of the pixel, after the plot</param>
        public void PlottedPixel(Point point, int oldColor, int newColor)
        {
            if (_registerPixels)
                UndoTask.PixelHistoryTracker.RegisterPixel(point.X, point.Y, oldColor, newColor, IgnoreDuplicatedPlots);
        }

        /// <summary>
        /// Method called to notify the plotting operation was finished
        /// </summary>
        /// <param name="pixelHistory">The pixel history tracker containing the information about the pixels that were modified during the operation</param>
        public void OperationFinished(PixelHistoryTracker pixelHistory)
        {
            if (pixelHistory != null && !_registerPixels)
                UndoTask = new PerPixelUndoTask(_bitmap, _description, pixelHistory);
        }
    }
}
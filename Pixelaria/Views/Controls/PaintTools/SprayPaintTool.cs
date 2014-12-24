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
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Spray paint operation
    /// </summary>
    public class SprayPaintTool : BasePencilPaintTool, IColoredPaintTool, ISizedPaintTool, ICompositingPaintTool
    {
        /// <summary>
        /// Instance of a Random class used to randomize the spray of this SprayPaintTool
        /// </summary>
        readonly Random _random;

        /// <summary>
        /// The spray's timer, used to make the operation paint with the mouse held down at a stationary point
        /// </summary>
        readonly Timer _sprayTimer;

        /// <summary>
        /// The Bitmap to use as pen with the first color
        /// </summary>
        protected Bitmap firstPenBitmap;

        /// <summary>
        /// The Bitmap to use as pen with the second color
        /// </summary>
        protected Bitmap secondPenBitmap;

        /// <summary>
        /// Initializes a new instance of the SprayPaintTool class
        /// </summary>
        public SprayPaintTool()
        {
            _random = new Random();

            _sprayTimer = new Timer { Interval = 10 };
            _sprayTimer.Tick += sprayTimer_Tick;
        }
        
        /// <summary>
        /// Initializes a new instance of the SprayPaintTool class, initializing the object with the two spray colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public SprayPaintTool(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
            size = pencilSize;
        }

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        public override void Destroy()
        {
            _sprayTimer.Stop();
            _sprayTimer.Dispose();

            if (firstPenBitmap != null)
                firstPenBitmap.Dispose();
            if (secondPenBitmap != null)
                secondPenBitmap.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// Initializes this PencilPaintTool
        /// </summary>
        /// <param name="targetPictureBox">The target picture box for this pencil tool</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.spray_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            undoDecription = "Spray";
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            if (mouseDown && e.Button != MouseButtons.Middle)
            {
                _sprayTimer.Start();
            }
        }

        /// <summary>
        /// Called to notify this PaintTool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);

            if (!mouseDown)
            {
                _sprayTimer.Stop();
            }
        }

        /// <summary>
        /// Draws the pencil with the current properties on the given bitmap object
        /// </summary>
        /// <param name="p">The point to draw the pencil to</param>
        /// <param name="bitmap">The bitmap to draw the pencil on</param>
        protected override void DrawPencil(Point p, Bitmap bitmap)
        {
            // Find the properties to draw the pen with
            double angle = _random.NextDouble() * Math.PI * 2;
            float radius = (_random.Next(0, size) / 2.0f);

            p.X = p.X + (int)Math.Round(Math.Cos(angle) * radius);
            p.Y = p.Y + (int)Math.Round(Math.Sin(angle) * radius);

            pencilOperation.PlotPixel(p.X, p.Y);

            PointF pf = GetRelativePoint(p);
            InvalidateRect(pf, 1.2f, 1.2f);
        }

        /// <summary>
        /// Draws the pencil preview on a specified bitmap at the specified point.
        /// If the current pencil operation is currently started, no preview is drawn
        /// </summary>
        /// <param name="bitmap">The bitmap to draw the pencil preview on</param>
        /// <param name="point">The point on the bitmap draw the pencil preview on</param>
        protected override void DrawPencilPreview(Bitmap bitmap, Point point)
        {
            if (mouseDown)
                return;

            Graphics bitmapGraphics = Graphics.FromImage(bitmap);

            Bitmap pen = (penId == 0 ? firstPenBitmap : secondPenBitmap);

            if (size > 1)
            {
                point.Offset(-size / 2, -size / 2);
            }

            bitmapGraphics.PixelOffsetMode = PixelOffsetMode.Half;
            bitmapGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;

            // Create a color matrix object
            ColorMatrix matrix = new ColorMatrix
            {
                Matrix33 = ((float)(penId == 0 ? firstColor : secondColor).A / 255)
            };

            // Create image attributes
            ImageAttributes attributes = new ImageAttributes();
                
            // Set the color(opacity) of the image
            attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            Graphics gfx = Graphics.FromImage(pictureBox.Buffer);

            gfx.DrawImage(pen, new Rectangle(point, new Size(pen.Width, pen.Height)), 0, 0, pen.Width, pen.Height, GraphicsUnit.Pixel, attributes);

            gfx.Flush();
            gfx.Dispose();
        }

        /// <summary>
        /// Updates the pen configuration
        /// </summary>
        protected override void UpdatePen()
        {
            base.UpdatePen();

            if (firstPenBitmap != null)
            {
                firstPenBitmap.Dispose();
            }
            if (secondPenBitmap != null)
            {
                secondPenBitmap.Dispose();
            }

            firstPenBitmap = new Bitmap(size + 1, size + 1, PixelFormat.Format32bppArgb);

            if (size == 1)
            {
                firstPenBitmap.SetPixel(0, 0, Color.FromArgb(255, firstColor.R, firstColor.G, firstColor.B));
            }
            else
            {
                Graphics g = Graphics.FromImage(firstPenBitmap);
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Brush b = new SolidBrush(Color.FromArgb(255, firstColor.R, firstColor.G, firstColor.B));
                g.FillEllipse(b, 0, 0, size, size);
            }

            secondPenBitmap = new Bitmap(size + 1, size + 1, PixelFormat.Format32bppArgb);

            if (size == 1)
            {
                secondPenBitmap.SetPixel(0, 0, Color.FromArgb(255, secondColor.R, secondColor.G, secondColor.B));
            }
            else
            {
                Graphics g = Graphics.FromImage(secondPenBitmap);
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Brush b = new SolidBrush(Color.FromArgb(255, secondColor.R, secondColor.G, secondColor.B));
                g.FillEllipse(b, 0, 0, size, size);
            }
        }

        // 
        // Spray Timer tick
        // 
        private void sprayTimer_Tick(object sender, EventArgs e)
        {
            DrawPencil(GetAbsolutePoint(pencilPoint), (CompositingMode == CompositingMode.SourceOver ? currentTraceBitmap : pictureBox.Bitmap));
        }
    }
}
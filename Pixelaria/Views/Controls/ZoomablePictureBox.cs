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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Pixelaria.Utils;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// PictureBox implementation that supports zooming in and out of the loaded image
    /// </summary>
    [DefaultEvent("ZoomChanged")]
    public class ZoomablePictureBox : CPictureBox
    {
        /// <summary>
        /// This control's horizontal scrollbar
        /// </summary>
        protected HScrollBar hScrollBar;

        /// <summary>
        /// This control's vertical scrollbar
        /// </summary>
        protected VScrollBar vScrollBar;

        /// <summary>
        /// Offset point transform to use when drawing the component
        /// </summary>
        protected Point offsetPoint;

        /// <summary>
        /// Scale transform to use when drawing the component
        /// </summary>
        protected PointF scale;

        /// <summary>
        /// Whether the viewport is currently being dragged
        /// </summary>
        protected bool draggingViewport;

        /// <summary>
        /// Whether the mouse is currently held down on this control
        /// </summary>
        protected bool mouseDown;

        /// <summary>
        /// The offset of the mouse when it started dragging the viewport
        /// </summary>
        protected Point mouseOffset;

        /// <summary>
        /// The control whose bounds are used to clip the mouse wheel
        /// </summary>
        protected Control boundsControl;

        /// <summary>
        /// Whether to clip the background image to the image's size
        /// </summary>
        private bool _clipBackgroundToImage;

        /// <summary>
        /// Delegate for the ZoomChanged event
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="e">The arguments for the event</param>
        public delegate void ZoomChangedEventHandler(object sender, ZoomChangedEventArgs e);

        /// <summary>
        /// Occurs whenever the zoom of the control changes
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the zoom of the control changes")]
        public event ZoomChangedEventHandler ZoomChanged;

        /// <summary>
        /// Occurs whenever the horizontal scroll of the control changes
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the horizontal scroll of the changes")]
        public event ScrollEventHandler HorizontalScroll;

        /// <summary>
        /// Occurs whenever the vertical scroll of the control changes
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the vertical scroll of the changes")]
        public event ScrollEventHandler VerticalScroll;

        /// <summary>
        /// Gets or sets whether this ZoomablePictureBox should show a white outline around the image area
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether this ZoomablePictureBox should show a white outline around the image area")]
        public bool ShowImageArea { get; set; }

        /// <summary>
        /// Gets or sets a value specifing whether this ZoomablePictureBox should change it's size to match the
        /// image's current zoom settings. Setting this value to true disables scrollbars display when the
        /// image bounds is larger than the control's area
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("Whether this ZoomablePictureBox should change it's size to match the image's current zoom settings. Setting this value to true disables scrollbars display when the image bounds is larger than the control's area")]
        public bool AutomaticallyResize { get; set; }

        /// <summary>
        /// Gets or sets whether to clip the background image to the image's size
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether to clip the background image to the image's size")]
        public bool ClipBackgroundToImage
        {
            get => _clipBackgroundToImage;
            set
            {
                _clipBackgroundToImage = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether to allow dragging of the image
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Whether to allow dragging of the image")]
        public bool AllowDrag { get; set; }

        /// <summary>
        /// Gets or sets whether to allow the display of scrollbars when the image bounds is largest than the control's size
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Whether to allow the display of scrollbars when the image bounds is largest than the control's size")]
        public bool AllowScrollbars { get; set; }

        /// <summary>
        /// Gets or sets the minimum zoom scale
        /// </summary>
        [Browsable(true)]
        public PointF MinimumZoom { get; set; }

        /// <summary>
        /// Gets or sets the maximum zoom scale
        /// </summary>
        [Browsable(false)]
        public PointF MaximumZoom { get; set; }

        /// <summary>
        /// Gets or sets the current zoom of this ZoomablePictureBox
        /// </summary>
        [Browsable(false)]
        public PointF Zoom
        {
            get => scale;
            set
            {
                float oldZoom = scale.X;

                scale = value;

                Point currentOffset = offsetPoint;

                ClipTransform();

                // Zoom around the mouse
                var pivot = PointToClient(MousePosition);

                if (!ClientRectangle.Contains(pivot))
                {
                    pivot = Point.Empty;
                }

                currentOffset.X += pivot.X;
                currentOffset.Y += pivot.Y;

                currentOffset.X = (int)(currentOffset.X * (scale.X / oldZoom));
                currentOffset.Y = (int)(currentOffset.Y * (scale.X / oldZoom));

                currentOffset.X -= pivot.X;
                currentOffset.Y -= pivot.Y;

                offsetPoint = currentOffset;

                ClipTransform();
                Invalidate();

                // Fire the zoom changed event
                ZoomChanged?.Invoke(this, new ZoomChangedEventArgs(oldZoom, scale.X));
            }
        }

        /// <summary>
        /// Gets or sets the zoom factor for when the mouse wheel is scrolled on top of this control
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(1)]
        [Description("The zoom factor for when the mouse wheel is scrolled on top of this control")]
        public float ZoomFactor { get; set; }

        /// <summary>
        /// Gets or sets the zoom factor mode for this ZoomablePictureBox
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(ZoomFactorMode.Multiply)]
        [Description("The maximum zoom scale")]
        public ZoomFactorMode ZoomFactorMode { get; set; }

        /// <summary>
        /// Gets or sets the offset of the image
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(typeof(Point), "0, 0")]
        [Description("The offset of the image")]
        public Point Offset { get => offsetPoint; set { offsetPoint = value; ClipTransform(); Invalidate(); } }

        /// <summary>
        /// Gets or sets the horizontal scroll of this control
        /// </summary>
        [Browsable(false)]
        public int HorizontalScrollValue
        {
            get => hScrollBar.Value;
            set
            {
                hScrollBar.Value = value;
                offsetPoint.X = value;
                Invalidate();

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the vertical scroll of this control
        /// </summary>
        [Browsable(false)]
        public int VerticalScrollValue
        {
            get => vScrollBar.Value;
            set
            {
                vScrollBar.Value = value;
                offsetPoint.Y = value;
                Invalidate();

                Invalidate();
            }
        }

        /// <summary>
        /// Default constructor for the ZoomablePictureBox
        /// </summary>
        public ZoomablePictureBox()
        {
            offsetPoint = new Point();
            scale = new PointF(1, 1);
            ZoomFactor = (float)Math.Sqrt(2);
            ZoomFactorMode = ZoomFactorMode.Multiply;

            MinimumZoom = new PointF(0.125f, 0.125f);
            MaximumZoom = new PointF(15, 15);

            AllowDrag = true;

            // Scrollbar creation
            hScrollBar = new HScrollBar
            {
                Dock = DockStyle.Bottom,
                Value = 0,
                Minimum = 0,
                Maximum = 150,
                SmallChange = 1,
                LargeChange = 300,
                Visible = false,
                Cursor = Cursors.Arrow
            };

            vScrollBar = new VScrollBar
            {
                Dock = DockStyle.Right,
                Value = 0,
                Minimum = 0,
                Maximum = 150,
                SmallChange = 1,
                LargeChange = 300,
                Visible = false,
                Cursor = Cursors.Arrow
            };

            hScrollBar.Scroll += hScrollBar_Scroll;
            vScrollBar.Scroll += vScrollBar_Scroll;

            Controls.Add(hScrollBar);
            Controls.Add(vScrollBar);
        }

        /// <summary>
        /// Swaps the currently displayed image with the given one.
        /// Optionally specify to reset zoom and offset transformations
        /// </summary>
        /// <param name="image">The new image to display on this form</param>
        /// <param name="resetTransform">Whether to reset the transformation after setting the new image</param>
        public void SetImage(Image image, bool resetTransform = false)
        {
            Image = image;

            if (resetTransform)
            {
                offsetPoint = new Point();
                Zoom = new PointF(1, 1);
            }

            ClipTransform();
        }

        /// <summary>
        /// Hooks the mouse wheel event listener to the given control
        /// </summary>
        /// <param name="owningControl">The control that owns this SheetPreviewPictureBox. Used to hook the mouse wheel listener</param>
        /// <param name="wheelClipBoundsControl">The control to use the bounds of to clip the mouse wheel event. Setting to null uses this control as bounds</param>
        public void HookToControl(Control owningControl, Control wheelClipBoundsControl = null)
        {
            owningControl.MouseWheel += ZoomablePictureBox_MouseWheel;
            boundsControl = wheelClipBoundsControl ?? this;
        }

        /// <summary>
        /// Gets the absolute pixel point that represents the given control point transformed
        /// to aboslute pixel coordinates
        /// </summary>
        /// <param name="controlPoint">The control point to convert to absolute coordinates</param>
        /// <returns>The absolute point that represents the given control point transformed to aboslute pixel coordinates</returns>
        public Point GetAbsolutePoint(PointF controlPoint)
        {
            var m = new Matrix();
            m.Reset();

            m.Scale(1 / scale.X, 1 / scale.Y);
            m.Translate(offsetPoint.X, offsetPoint.Y);

            PointF[] p = { controlPoint };

            m.TransformPoints(p);

            return Point.Truncate(p[0]);
        }

        /// <summary>
        /// Gets the relative control point that represents the given pixel point transformed
        /// to relative control coordinates
        /// </summary>
        /// <param name="pixelPoint">The absolute pixel point to convert to relative control coordinates</param>
        /// <returns>the relative point that represents the given pixel point transformed to relative control coordinates</returns>
        public PointF GetRelativePoint(Point pixelPoint)
        {
            var m = new Matrix();
            m.Reset();
            
            m.Translate(-offsetPoint.X, -offsetPoint.Y);
            m.Scale(scale.X, scale.Y);

            PointF[] p = { pixelPoint };

            m.TransformPoints(p);

            return p[0];
        }

        /// <summary>
        /// Clips the offset and scale to be within the current acceptable bounds
        /// </summary>
        protected virtual void ClipTransform()
        {
            scale.X = Math.Max(MinimumZoom.X, Math.Min(MaximumZoom.X, scale.X));
            scale.Y = Math.Max(MinimumZoom.Y, Math.Min(MaximumZoom.Y, scale.Y));

            offsetPoint.X = Math.Max(0, Math.Min((int)(Image?.Width * scale.X - (Width - vScrollBar.Width) ?? 0), offsetPoint.X));
            offsetPoint.Y = Math.Max(0, Math.Min((int)(Image?.Height * scale.Y - (Height - hScrollBar.Height) ?? 0), offsetPoint.Y));

            if (Image != null)
            {
                if (AutomaticallyResize)
                {
                    Size = new Size((int)(Image.Width * scale.X), (int)(Image.Height * scale.X));
                }
                else
                {
                    UpdateScrollbars();
                }
            }
        }

        /// <summary>
        /// Updates the transform of the given Graphics object and prepares it to display the image on this 
        /// ZoomablePictureBox instance
        /// </summary>
        /// <param name="graphics">The Graphics object to update</param>
        protected virtual void UpdateGraphicsTransform(Graphics graphics)
        {
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            graphics.InterpolationMode = ImageInterpolationMode;

            if (ImageLayout != ImageLayout.Center)
            {
                graphics.TranslateTransform(-offsetPoint.X, -offsetPoint.Y);
                graphics.ScaleTransform(scale.X, scale.Y);
            }
        }

        /// <summary>
        /// Updates the scrollbars display
        /// </summary>
        protected void UpdateScrollbars()
        {
            // Get the ammount of pixels that don't fit the control's width and height
            int excessW = (int)(Image.Width * scale.X - (Width - vScrollBar.Width));
            int excessH = (int)(Image.Height * scale.Y - (Height - hScrollBar.Height));

            if (excessW > 0)
            {
                int largeChange = Math.Min((Width - vScrollBar.Width), excessW);
                hScrollBar.LargeChange = Math.Min((Width - vScrollBar.Width), excessW);
                hScrollBar.Maximum = excessW + largeChange - 1;
                hScrollBar.Value = offsetPoint.X;
                hScrollBar.Visible = true;
            }
            else
            {
                hScrollBar.Visible = false;
            }

            if (excessH > 0)
            {
                int largeChange = Math.Min((Height - hScrollBar.Height), excessH);
                vScrollBar.LargeChange = Math.Min((Height - hScrollBar.Height), excessH);
                vScrollBar.Maximum = excessH + largeChange - 1;
                vScrollBar.Value = offsetPoint.Y;
                vScrollBar.Visible = true;
            }
            else
            {
                vScrollBar.Visible = false;
            }
        }

        // 
        // Horizontal Scrollbar scroll event
        // 
        private void hScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetPoint.X = e.NewValue;
            Invalidate();

            HorizontalScroll?.Invoke(this, e);
        }

        // 
        // Vertical Scrollbar scroll event
        // 
        private void vScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            offsetPoint.Y = e.NewValue;
            Invalidate();

            VerticalScroll?.Invoke(this, e);
        }

        // 
        // OnPaint event handler. Draws the underlying sheet, and the frame rectangles on the sheet
        // 
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image == null)
                return;

            var rec = CalculateBackgroundImageRectangle(ClientRectangle, Image, ImageLayout);

            if (ImageLayout == ImageLayout.Center)
            {
                rec.X = (int)((ClientRectangle.Width / 2.0f - (rec.Width * scale.X) / 2));
                rec.Y = (int)((ClientRectangle.Height / 2.0f - (rec.Height * scale.Y) / 2));
                rec.Width = (int)(rec.Width * scale.X);
                rec.Height = (int)(rec.Height * scale.Y);
            }

            UpdateGraphicsTransform(pe.Graphics);

            if (ShowImageArea)
            {
                // Draw the image bounds rectangle
                RectangleF imageBoundsRec = rec;
                imageBoundsRec.X += 0.5f;
                imageBoundsRec.Y += 0.5f;
                imageBoundsRec.Width -= 1;
                imageBoundsRec.Height -= 1;
                pe.Graphics.DrawRectangle(Pens.White, imageBoundsRec.X, imageBoundsRec.Y, imageBoundsRec.Width, imageBoundsRec.Height);
            }

            RectangleF imgArea = rec;

            pe.Graphics.DrawImage(Image, imgArea);
        }
        
        // 
        // OnPaintBackground event handler
        // 
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            if (ClipBackgroundToImage && Image != null)
            {
                if (BackgroundImage == null)
                    return;
                
                pevent.Graphics.Clear(BackColor);

                var rec = CalculateBackgroundImageRectangle(ClientRectangle, Image, ImageLayout);

                // Transform the rectangle by the transform matrix
                var transform = pevent.Graphics.Transform;
                UpdateGraphicsTransform(pevent.Graphics);

                if (ImageLayout == ImageLayout.Center)
                {
                    rec.X = (int)(ClientRectangle.Width / 2.0f - rec.Width * scale.X / 2);
                    rec.Y = (int)(ClientRectangle.Height / 2.0f - rec.Height * scale.Y / 2);
                    rec.Width = (int)(rec.Width * scale.X);
                    rec.Height = (int)(rec.Height * scale.Y);
                }

                PointF[] points =
                {
                    new PointF(rec.X, rec.Y), new PointF(rec.X + rec.Width, rec.Y),
                    new PointF(rec.X + rec.Width, rec.Y + rec.Height), new PointF(rec.X, rec.Y + rec.Height)
                };

                pevent.Graphics.Transform.TransformPoints(points);
                pevent.Graphics.Transform = transform;

                if (BackgroundImageLayout == ImageLayout.Tile)
                {
                    using (var tex = new TextureBrush(BackgroundImage) {WrapMode = WrapMode.Tile})
                    {
                        var state = pevent.Graphics.Save();

                        tex.TranslateTransform(points[0].X, points[0].Y);
                        
                        var rect = Utilities.GetRectangleArea(points);

                        pevent.Graphics.CompositingMode = CompositingMode.SourceCopy;
                        pevent.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                        pevent.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                        pevent.Graphics.SmoothingMode = SmoothingMode.None;
                        pevent.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                        pevent.Graphics.FillRectangle(tex, rect);

                        pevent.Graphics.Restore(state);
                    }
                }
                else
                {
                    pevent.Graphics.DrawImage(BackgroundImage, rec);
                }
            }
            else
            {
                base.OnPaintBackground(pevent);
            }
        }

        // 
        // OnResize event handler. Clamps the offset point to within the acceptable bounds
        // 
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            ClipTransform();
        }

        // 
        // Mouse Click event handler. Used to reset the zoom when the user clicks with the middle mouse button
        // 
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Middle)
            {
                offsetPoint = new Point();
                Zoom = new PointF(1, 1);
            }
        }

        // 
        // Mouse Down event handler. Used to start dragging the viewport
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            mouseDown = true;

            if (AllowDrag && e.Button == MouseButtons.Left)
            {
                // Steal the focus from the current control so the mouse wheel event handler funcions correctly
                var findForm = FindForm();
                if (findForm != null)
                    findForm.ActiveControl = this;

                draggingViewport = true;
                mouseOffset = new Point(MousePosition.X + offsetPoint.X, MousePosition.Y + offsetPoint.Y);
            }
        }

        // 
        // Mouse Move event handler. Used to drag the viewport
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (draggingViewport)
            {
                offsetPoint = new Point(-MousePosition.X + mouseOffset.X, -MousePosition.Y + mouseOffset.Y);

                // Fire scroll events
                HorizontalScroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, offsetPoint.X));
                VerticalScroll?.Invoke(this, new ScrollEventArgs(ScrollEventType.ThumbPosition, offsetPoint.Y));

                ClipTransform();
                Invalidate();
            }
        }

        // 
        // Mouse Down event handler. Used to stop dragging the viewport
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            mouseDown = false;

            if (e.Button == MouseButtons.Left)
            {
                draggingViewport = false;
                mouseOffset = MousePosition;
            }
        }

        // 
        // Mouse wheel event handler. Used to zoom in and out of the preview sheet
        // 
        private void ZoomablePictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            var form = FindForm();

            if (form == null)
                return;

            Point p = form.PointToScreen(e.Location);

            p = boundsControl.PointToClient(p);

            // Test if the mouse is not over another control
            Control target = form.GetChildAtPoint(e.Location);
            if (target != null && target != this && !IsControlChildOf(this, target))
            {
                return;
            }

            if (boundsControl.ClientRectangle.Contains(p))
            {
                PointF newZoom = scale;

                if (ZoomFactorMode == ZoomFactorMode.Multiply)
                {
                    if (e.Delta > 0)
                    {
                        newZoom.X *= ZoomFactor;
                        newZoom.Y *= ZoomFactor;
                    }
                    else if (e.Delta < 0)
                    {
                        newZoom.X /= ZoomFactor;
                        newZoom.Y /= ZoomFactor;
                    }
                }
                else if (ZoomFactorMode == ZoomFactorMode.Add)
                {
                    if (e.Delta > 0)
                    {
                        newZoom.X += ZoomFactor;
                        newZoom.Y += ZoomFactor;
                    }
                    else if (e.Delta < 0)
                    {
                        newZoom.X -= ZoomFactor;
                        newZoom.Y -= ZoomFactor;
                    }
                }

                Zoom = newZoom;
            }
        }

        /// <summary>
        /// Returns whether a Control is parented by the given Control in some degree
        /// </summary>
        /// <param name="control">The control to check for inheritance as the child</param>
        /// <param name="parent">The control to check for inheritance as the parent</param>
        /// <returns>Whether a Control is parented by the given Control in some degree</returns>
        private static bool IsControlChildOf(Control control, Control parent)
        {
            while (control != null)
            {
                if (control.Parent == parent)
                    return true;

                control = control.Parent;
            }

            return false;
        }
    }

    /// <summary>
    /// Event arguments for the ZoomChanged event
    /// </summary>
    public class ZoomChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the old zoom before the event
        /// </summary>
        public float OldZoom { get; }

        /// <summary>
        /// Gets the new zoom after the event
        /// </summary>
        public float NewZoom { get; }

        /// <summary>
        /// Initializes a new instance of the ZoomChangeEventArgs
        /// </summary>
        /// <param name="oldZoom">The old zoom before the event</param>
        /// <param name="newZoom">The new zoom after the event</param>
        public ZoomChangedEventArgs(float oldZoom, float newZoom)
        {
            OldZoom = oldZoom;
            NewZoom = newZoom;
        }
    }

    /// <summary>
    /// Specifies the way a ZoomablePictureBox zooms when the user scrolls the mouse wheel on top of it
    /// </summary>
    public enum ZoomFactorMode
    {
        /// <summary>
        /// The zoom is multiplied/divied by the zoom factor
        /// </summary>
        Multiply,
        /// <summary>
        /// The zoom is added/subtracted by the zoom factor
        /// </summary>
        Add
    }
}
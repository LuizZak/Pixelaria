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
using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Selection paint operation
    /// </summary>
    public class SelectionPaintTool : BaseDraggingPaintTool, IClipboardPaintTool, ICompositingPaintTool, IAreaOperation, IUndoIntercepterPaintTool
    {
        /// <summary>
        /// Timer used to animate the selection area
        /// </summary>
        private Timer _animTimer;

        /// <summary>
        /// The dash offset to use when drawing the selection area
        /// </summary>
        private float _dashOffset;
        
        /// <summary>
        /// The currently selected area
        /// </summary>
        protected Rectangle selectedArea;

        /// <summary>
        /// The currently selected area in control coordinates
        /// </summary>
        protected Rectangle relativeSelectedArea;

        /// <summary>
        /// The area the current selection was sliced from
        /// </summary>
        protected Rectangle selectedStartArea;

        /// <summary>
        /// Whether there's an area currently selected
        /// </summary>
        protected bool selected;

        /// <summary>
        /// Whether to display the selection
        /// </summary>
        protected bool displaySelection;

        /// <summary>
        /// Whether the mouse is currently drawing the selection area
        /// </summary>
        protected bool drawingSelection;

        /// <summary>
        /// Whether the mouse is currently moving the selection area
        /// </summary>
        protected bool movingSelection;

        /// <summary>
        /// The offset for the selection moving operation
        /// </summary>
        protected Point movingOffset;

        /// <summary>
        /// The compositing mode for this paint operation
        /// </summary>
        protected CompositingMode compositingMode;

        /// <summary>
        /// Graphics used to draw on the selection bitmap
        /// </summary>
        protected Graphics graphics;

        /// <summary>
        /// The buffer bitmap for drawing the clipboard on
        /// </summary>
        protected Bitmap selectionBitmap;

        /// <summary>
        /// The undo event handler
        /// </summary>
        private UndoSystem.UndoEventHandler _undoHandler;
        /// <summary>
        /// The redo event handler
        /// </summary>
        private UndoSystem.UndoEventHandler _redoHandler;

        /// <summary>
        /// Gets whether there's an area currently selected
        /// </summary>
        public bool Selected => selected;

        /// <summary>
        /// Gets or sets the bitmap that represents the currently selected graphics
        /// </summary>
        public Bitmap SelectionBitmap { get => selectionBitmap;
            set { selectionBitmap = value; pictureBox.Invalidate(); } }

        /// <summary>
        /// Gets the area the selection is currently occupying in the canvas
        /// </summary>
        public Rectangle SelectionArea
        {
            get => selectedArea;
            set
            {
                // Invalidate before and after the modification
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea = value;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
        }

        /// <summary>
        /// Gets the area the selection was snipped from
        /// </summary>
        public Rectangle SelectionStartArea => selectedStartArea;

        /// <summary>
        /// Gets the operation currently being performed by this SelectionPaintOperation
        /// </summary>
        public SelectionOperationType OperationType { get; private set; }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get => compositingMode;
            set { compositingMode = value; if (selected) { pictureBox.Invalidate(GetSelectionArea(true)); } } }

        /// <summary>
        /// Gets or sets whether to force the creation of an undo task after the operation is finished even if no significant changes have been made with this paint operation
        /// </summary>
        public bool ForceApplyChanges { get; set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.sel_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            _animTimer = new Timer { Interval = 150 };
            _animTimer.Tick += animTimer_Tick;

            displaySelection = true;
            selected = false;

            base.Initialize(targetPictureBox);

            _undoHandler = UndoSystem_WillPerformUndo;
            _redoHandler = UndoSystem_WillPerformRedo;

            targetPictureBox.OwningPanel.UndoSystem.WillPerformUndo += _undoHandler;
            targetPictureBox.OwningPanel.UndoSystem.WillPerformRedo += _redoHandler;

            Loaded = true;
        }

        // 
        // Animation Timer tick
        // 
        private void animTimer_Tick(object sender, EventArgs e)
        {
            _dashOffset -= 0.5f;
            pictureBox.Invalidate(GetSelectionArea(true));
        }

        // 
        // Undo System Will Perform Undo event handler
        // 
        private void UndoSystem_WillPerformUndo(object sender, UndoEventArgs e)
        {
            CancelOperation(true);
        }

        // 
        // Undo System Will Perform Redo event handler
        // 
        private void UndoSystem_WillPerformRedo(object sender, UndoEventArgs e)
        {
            CancelOperation(true);
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            FinishOperation(true);

            // Remove the event handler
            pictureBox.OwningPanel.UndoSystem.WillPerformUndo -= _undoHandler;
            pictureBox.OwningPanel.UndoSystem.WillPerformRedo -= _redoHandler;

            pictureBox = null;

            if (graphics != null)
            {
                graphics.Dispose();
                graphics = null;
            }

            if (selectionBitmap != null)
            {
                selectionBitmap.Dispose();
                selectionBitmap = null;
            }

            ToolCursor.Dispose();

            _animTimer.Stop();
            _animTimer.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public override void ChangeBitmap(Bitmap newBitmap)
        {
            CancelOperation(false);
        }

        /// <summary>
        /// Creates a selection that is the size of the current image
        /// </summary>
        public void SelectAll()
        {
            if (selected)
            {
                FinishOperation(true);
            }

            // Find the minimum rectangle for the selection
            Rectangle selectionRectangle = ImageUtilities.FindMinimumImageArea(pictureBox.Bitmap);

            // If the whole area is empty, select the whole bitmap area
            if (selectionRectangle.Size == Size.Empty)
            {
                selectionRectangle = new Rectangle(0, 0, pictureBox.Image.Width, pictureBox.Image.Height);
            }

            StartOperation(selectionRectangle, null, SelectionOperationType.Moved);
        }

        /// <summary>
        /// Performs a Copy operation
        /// </summary>
        public void Copy()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                selectionBitmap.Save(stream, ImageFormat.Png);

                Clipboard.Clear();

                var data = new DataObject();
                data.SetData(DataFormats.Bitmap, true, selectionBitmap);
                data.SetData("PNG", true, stream);

                Clipboard.SetDataObject(data, true);
            }

            UpdateClipboardState();
        }

        /// <summary>
        /// Performs a Cut operation
        /// </summary>
        public void Cut()
        {
            OperationType = SelectionOperationType.Cut;

            Copy();

            FinishOperation(false);

            UpdateClipboardState();
        }

        /// <summary>
        /// Performs a Paste operation
        /// </summary>
        public void Paste()
        {
            Stream str = Clipboard.GetData("PNG") as Stream;
            Bitmap bit;

            if (str != null)
            {
                bit = Image.FromStream(str) as Bitmap;

                if (bit == null)
                {
                    str.Dispose();
                    return;
                }

                Bitmap temp = new Bitmap(bit.Width, bit.Height, PixelFormat.Format32bppArgb);

                FastBitmap.CopyPixels(bit, temp);

                bit.Dispose();
                bit = temp;

                str.Dispose();
            }
            else
            {
                bit = Clipboard.GetImage() as Bitmap;
            }

            if (bit != null)
            {
                FinishOperation(true);

                OperationType = SelectionOperationType.Paste;

                // Get the top-left pixel to place the selection at
                Point loc = GetAbsolutePoint(new PointF(0, 0));
                
                StartOperation(new Rectangle(loc.X, loc.Y, bit.Width, bit.Height), bit);
            }

            pictureBox.MarkModified();
        }

        /// <summary>
        /// Returns whether the paint operation can copy content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can copy content to the clipboard</returns>
        public bool CanCopy()
        {
            return selected && !movingSelection;
        }

        /// <summary>
        /// Returns whether the paint operation can cut content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can cut content to the clipboard</returns>
        public bool CanCut()
        {
            return selected && !movingSelection;
        }

        /// <summary>
        /// Returns whether the paint operation can paste content from the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can paste content from the clipboard</returns>
        public bool CanPaste()
        {
            return Clipboard.ContainsData("PNG") || Clipboard.ContainsImage();
        }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void Paint(PaintEventArgs e)
        {
            if (drawingSelection)
            {
                selectedArea = GetCurrentRectangle(false);
                relativeSelectedArea = GetCurrentRectangle(true);
            }

            if (selected || drawingSelection)
            {
                Graphics gfx = Graphics.FromImage(pictureBox.Buffer);

                // Draw the selection bitmap
                if (selectionBitmap != null)
                {
                    if (compositingMode == CompositingMode.SourceCopy)
                    {
                        Region reg = gfx.Clip;

                        gfx.SetClip(GetSelectionArea(false));

                        gfx.Clear(Color.Transparent);

                        gfx.Clip = reg;
                    }

                    gfx.DrawImage(selectionBitmap, selectedArea);
                }

                gfx.Flush();
                gfx.Dispose();
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the foreground of the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void PaintForeground(PaintEventArgs e)
        {
            if (!displaySelection || !selected && !drawingSelection)
                return;

            SelectionPaintToolPainter.PaintSelectionRectangle(e.Graphics, GetSelectionArea(false), _dashOffset);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);
            
            Point p = GetAbsolutePoint(e.Location);

            if (!selectedArea.Contains(p))
            {
                if (selected && (selectedArea != selectedStartArea || ForceApplyChanges || OperationType == SelectionOperationType.Paste))
                {
                    FinishOperation(true);
                }
                else
                {
                    CancelOperation(true);
                }
            }
            else if(selected && WithinBounds(p))
            {
                // If the control key is currently down, duplicate the image and start dragging the duplicated image instead
                if (ctrlDown)
                {
                    Bitmap copy = selectionBitmap.Clone(new Rectangle(Point.Empty, selectionBitmap.Size), selectionBitmap.PixelFormat);

                    FinishOperation(true);
                    StartOperation(selectedArea, copy, SelectionOperationType.Paste);
                }

                movingSelection = true;
                movingOffset = new Point(p.X - selectedArea.X, p.Y - selectedArea.Y);
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            base.MouseMove(e);

            Point p = GetAbsolutePoint(e.Location);

            if (selected)
            {
                if (movingSelection)
                {
                    if (lastMouseAbsolutePoint != mouseAbsolutePoint)
                    {
                        pictureBox.Invalidate(GetSelectionArea(true));

                        selectedArea.X = p.X - movingOffset.X;
                        selectedArea.Y = p.Y - movingOffset.Y;

                        pictureBox.Invalidate(GetSelectionArea(true));

                        displaySelection = false;

                        UpdateClipboardState();
                    }
                }

                pictureBox.Cursor = selectedArea.Contains(p) ? Cursors.SizeAll : ToolCursor;
            }
            else if (mouseDown)
            {
                if (lastMouseAbsolutePoint != mouseAbsolutePoint)
                {
                    drawingSelection = true;
                }
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            if (mouseDown)
            {
                if (movingSelection)
                {
                    movingSelection = false;
                    displaySelection = true;

                    if(OperationType != SelectionOperationType.Paste && selectedArea != selectedStartArea)
                        pictureBox.MarkModified();

                    UpdateClipboardState();
                }
                else if(drawingSelection)
                {
                    selectedArea = GetCurrentRectangle(false);
                    relativeSelectedArea = GetCurrentRectangle(true);

                    if (selectedArea.Width > 0 && selectedArea.Height > 0)
                    {
                        selected = true;
                        _animTimer.Start();

                        OperationType = SelectionOperationType.Moved;

                        selectedStartArea = selectedArea;

                        StartOperation(selectedArea, null);

                        drawingSelection = false;
                        mouseDown = false;
                    }
                }
            }
            else if (selected == false)
            {
                _animTimer.Stop();
            }

            base.MouseUp(e);
        }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void KeyDown(KeyEventArgs e)
        {
            base.KeyDown(e);

            // Selection delete
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    if (selected)
                    {
                        if (OperationType == SelectionOperationType.Paste)
                        {
                            CancelOperation(false);
                        }
                        else
                        {
                            FinishOperation(false);
                            pictureBox.MarkModified();
                        }

                        OperationType = SelectionOperationType.Cut;
                    }
                    break;

                case Keys.Left:
                    pictureBox.Invalidate(GetSelectionArea(true));
                    selectedArea.X--;
                    pictureBox.Invalidate(GetSelectionArea(true));
                    break;

                case Keys.Right:
                    pictureBox.Invalidate(GetSelectionArea(true));
                    selectedArea.X++;
                    pictureBox.Invalidate(GetSelectionArea(true));
                    break;

                case Keys.Up:
                    pictureBox.Invalidate(GetSelectionArea(true));
                    selectedArea.Y--;
                    pictureBox.Invalidate(GetSelectionArea(true));
                    break;

                case Keys.Down:
                    pictureBox.Invalidate(GetSelectionArea(true));
                    selectedArea.Y++;
                    pictureBox.Invalidate(GetSelectionArea(true));
                    break;

                default:
                    if (e.KeyCode == Keys.Z && e.Modifiers == Keys.Control)
                    {
                        if (OperationType == SelectionOperationType.Moved)
                        {
                            // If the selection area is not the same as the start...
                            if (selectedArea != selectedStartArea)
                            {
                                SelectionArea = selectedStartArea;
                            }
                            // If it's the same area, cancel the operation
                            else
                            {
                                CancelOperation(true);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Forces this paint tool to intercept the undo operation, returning whether this Paint Tool has intercepted the undo operation successfully.
        /// While intercpting an undo, a paint tool might perform actions of its own
        /// </summary>
        /// <returns>Whether the current paint tool intercepted the undo task. When the return is true, no undo operation might be performed from an owning object</returns>
        public bool InterceptUndo()
        {
            if (!Selected)
                return false;

            if (OperationType == SelectionOperationType.Paste)
            {
                CancelOperation(false);
                return true;
            }

            if (OperationType != SelectionOperationType.Moved)
                return false;

            // If the selection area is not the same as the start...
            if (selectedArea != selectedStartArea)
            {
                SelectionArea = selectedStartArea;
                return true;
            }

            // If it's the same area, cancel the operation
            CancelOperation(true);
            return false;
        }

        /// <summary>
        /// Forces this paint tool to intercept the redo operation, returning whether this Paint Tool has intercepted the redooperation successfully.
        /// While intercpting a redo, a paint tool might perform actions of its own
        /// </summary>
        /// <returns>Whether the current paint tool intercepted the redo task. When the return is true, no redo operation might be performed from an owning object</returns>
        public bool InterceptRedo()
        {
            return false;
        }

        /// <summary>
        /// Extracts a portion of the edit image and returns a bitmap of it.
        /// The operation does not modifies the original image
        /// </summary>
        /// <param name="area">The area to remove from the image</param>
        /// <returns>A bitmap of the selected portion of the bitmap</returns>
        public Bitmap ExtractBitmap(Rectangle area)
        {
            Bitmap bitmap = new Bitmap(area.Width, area.Height);

            graphics = Graphics.FromImage(bitmap);

            graphics.DrawImage(pictureBox.Bitmap, new Rectangle(0, 0, area.Width, area.Height), area, GraphicsUnit.Pixel);

            graphics.Dispose();
            graphics = null;

            return bitmap;
        }

        /// <summary>
        /// Starts the selection operation with the given area
        /// </summary>
        /// <param name="area">The area to snip the bitmap from</param>
        public void StartOperation(Rectangle area)
        {
            StartOperation(area, null);
        }

        /// <summary>
        /// Starts the selection operation with the given area and operation mode
        /// </summary>
        /// <param name="area">The area to select</param>
        /// <param name="operation">The operation mode to apply</param>
        public void StartOperation(Rectangle area, SelectionOperationType operation)
        {
            StartOperation(area, null, operation);
        }

        /// <summary>
        /// Starts the selection operation with the given area and bitmap
        /// </summary>
        /// <param name="area">The area to place the bitmap at</param>
        /// <param name="pasteBitmap">The bitmap to paste</param>
        public void StartOperation(Rectangle area, Bitmap pasteBitmap)
        {
            StartOperation(area, pasteBitmap, OperationType);
        }

        /// <summary>
        /// Starts the selection operation witht he given area, bitmap and operation type
        /// </summary>
        /// <param name="area">The area to place the bitmap at</param>
        /// <param name="pasteBitmap">The bitmap to paste</param>
        /// <param name="operation">The operation type to mark this selection as</param>
        public void StartOperation(Rectangle area, Bitmap pasteBitmap, SelectionOperationType operation)
        {
            pictureBox.OwningPanel.UndoSystem.StartGroupUndo("Selection", true);

            OperationType = operation;

            ForceApplyChanges = false;

            selectionBitmap?.Dispose();

            selected = true;
            selectedArea = area;

            selectedStartArea = area;

            // Get the selected area
            if (pasteBitmap == null)
            {
                pasteBitmap = ExtractBitmap(area);

                // Clear the bitmap behind the image
                graphics = Graphics.FromImage(pictureBox.Image);

                graphics.SetClip(selectedArea);

                graphics.Clear(Color.Transparent);

                graphics.Dispose();
                graphics = null;

                UpdateClipboardState();
            }

            selectionBitmap = pasteBitmap;

            pictureBox.Invalidate(GetSelectionArea(true));

            _animTimer.Start();
        }

        /// <summary>
        /// Cancels the current dragging operation
        /// </summary>
        public void CancelOperation(bool drawOnCanvas, bool cancelGroup = true)
        {
            if (cancelGroup)
                pictureBox.OwningPanel.UndoSystem.FinishGroupUndo(true);

            if (!selected)
                return;

            if (OperationType == SelectionOperationType.Moved && drawOnCanvas)
            {
                if (selectionBitmap != null)
                {
                    // Draw the original slice back
                    graphics = Graphics.FromImage(pictureBox.Image);

                    graphics.DrawImage(selectionBitmap, selectedStartArea);

                    graphics.Flush();
                    graphics.Dispose();
                    graphics = null;
                }
            }

            pictureBox.Invalidate(GetSelectionArea(true));

            selectedArea = selectedStartArea;

            pictureBox.Invalidate(GetSelectionArea(true));

            pictureBox.Cursor = ToolCursor;

            selectionBitmap?.Dispose();
            selectionBitmap = null;

            movingSelection = false;
            displaySelection = true;

            selected = false;
            _animTimer.Stop();

            pictureBox.NotifyBitmapModified();

            UpdateClipboardState();

            // Default the operation mode to 'Moved'
            OperationType = SelectionOperationType.Moved;
        }

        /// <summary>
        /// Finishes the selection dragging operation
        /// </summary>
        /// <param name="drawToCanvas">Whether to draw the image to canvas before deleting it</param>
        public void FinishOperation(bool drawToCanvas)
        {
            if (selectionBitmap == null)
                return;

            Bitmap originalSlice = null;

            if (drawToCanvas)
            {
                originalSlice = new Bitmap(selectedArea.Width, selectedArea.Height);

                Graphics g = Graphics.FromImage(originalSlice);

                g.DrawImage(pictureBox.Image, new Rectangle(0, 0, selectedArea.Width, selectedArea.Height), selectedArea, GraphicsUnit.Pixel);

                g.Flush();
                g.Dispose();

                // Render the selected area
                graphics = Graphics.FromImage(pictureBox.Image);

                graphics.CompositingMode = compositingMode;

                graphics.DrawImage(selectionBitmap, selectedArea);

                graphics.Flush();
                graphics.Dispose();
                graphics = null;
            }

            if (selectedArea != selectedStartArea || OperationType != SelectionOperationType.Moved || ForceApplyChanges)
            {
                // Record the undo operation
                pictureBox.OwningPanel.UndoSystem.RegisterUndo(new SelectionUndoTask(pictureBox.Bitmap, selectionBitmap, originalSlice, selectedStartArea, selectedArea, OperationType, compositingMode));

                pictureBox.MarkModified();
            }

            Rectangle rec = GetSelectionArea(true);

            pictureBox.Invalidate(rec);

            pictureBox.Cursor = ToolCursor;

            selectionBitmap.Dispose();
            selectionBitmap = null;

            movingSelection = false;
            displaySelection = true;

            selected = false;
            _animTimer.Stop();

            UpdateClipboardState();

            // Default the operation mode to 'Moved'
            OperationType = SelectionOperationType.Moved;

            pictureBox.OwningPanel.UndoSystem.FinishGroupUndo();
        }

        /// <summary>
        /// Updates the clipboard state of this paint operation
        /// </summary>
        private void UpdateClipboardState()
        {
            pictureBox.OwningPanel.FireClipboardStateEvent(CanCopy(), CanCut(), CanPaste());
        }

        /// <summary>
        /// Returns a Rectangle object that represents the current rectangle area being dragged by the user
        /// </summary>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the current rectangle area being dragged by the user</returns>
        protected override Rectangle GetCurrentRectangle(bool relative)
        {
            Point p1 = mouseDownAbsolutePoint;
            Point p2 = mouseAbsolutePoint;

            // Clip the selected area to be within the image boundaries
            p1.X = Math.Max(0, Math.Min(pictureBox.Image.Width - 1, p1.X));
            p1.Y = Math.Max(0, Math.Min(pictureBox.Image.Height - 1, p1.Y));

            p2.X = Math.Max(0, Math.Min(pictureBox.Image.Width - 1, p2.X));
            p2.Y = Math.Max(0, Math.Min(pictureBox.Image.Height - 1, p2.Y));

            Rectangle rec = GetRectangleArea(new [] { p1, p2 }, relative);

            if (relative)
            {
                rec.Width += (int)(pictureBox.Zoom.X);
                rec.Height += (int)(pictureBox.Zoom.Y);
            }
            else
            {
                rec.Width++;
                rec.Height++;
            }

            if (shiftDown)
            {
                rec.Width = Math.Max(rec.Width, rec.Height);
                rec.Height = Math.Max(rec.Width, rec.Height);
            }

            return rec;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the currently selected area
        /// </summary>
        /// <param name="relative">Whether to return a rectangle in relative coordinates</param>
        /// <returns>A Rectangle object that represents the currently selected area</returns>
        protected Rectangle GetSelectionArea(bool relative)
        {
            if (!relative)
            {
                return selectedArea;
            }

            return relativeSelectedArea = new Rectangle((int)Math.Floor(-pictureBox.Offset.X + selectedArea.X * pictureBox.Zoom.X) - 1, (int)Math.Floor(-pictureBox.Offset.Y + selectedArea.Y * pictureBox.Zoom.Y) - 1, (int)Math.Ceiling(selectedArea.Width * pictureBox.Zoom.X) + 2, (int)Math.Ceiling(selectedArea.Height * pictureBox.Zoom.Y) + 2);
        }

        /// <summary>
        /// A selection undo task
        /// </summary>
        public class SelectionUndoTask : IUndoTask
        {
            /// <summary>
            /// The target bitmap for this undo task
            /// </summary>
            private readonly Bitmap _targetbitmap;

            /// <summary>
            /// The selection bitmap
            /// </summary>
            readonly Bitmap _selectionBitmap;

            /// <summary>
            /// The original image slice before a Move operation was made
            /// </summary>
            readonly Bitmap _originalSlice;

            /// <summary>
            /// The area that the selection started at
            /// </summary>
            readonly Rectangle _selectionStartArea;

            /// <summary>
            /// The area of the affected SelectionUndoTask
            /// </summary>
            readonly Rectangle _area;

            /// <summary>
            /// The operation type of this SelectionUndoTask
            /// </summary>
            readonly SelectionOperationType _operationType;

            /// <summary>
            /// The compositing mode for the operation
            /// </summary>
            readonly CompositingMode _compositingMode;

            /// <summary>
            /// Initializes a new instance of the SelectionUndoTask class
            /// </summary>
            /// <param name="targetBitmap">The target bitmap for the undo task</param>
            /// <param name="selectionBitmap">The selection bitmap</param>
            /// <param name="originalSlice">The original image slice before a Move operation was made</param>
            /// <param name="selectionStartArea">The area that the selection started at</param>
            /// <param name="area">The area of the affected SelectionUndoTask</param>
            /// <param name="operationType">The operation type of this SelectionUndoTask</param>
            /// <param name="compositingMode">The compositing mode for the operation</param>
            public SelectionUndoTask(Bitmap targetBitmap, Bitmap selectionBitmap, Bitmap originalSlice, Rectangle selectionStartArea, Rectangle area, SelectionOperationType operationType, CompositingMode compositingMode)
            {
                _targetbitmap = targetBitmap;
                _selectionBitmap = (Bitmap)selectionBitmap.Clone();
                _originalSlice = (Bitmap)originalSlice?.Clone();
                _selectionStartArea = selectionStartArea;
                _area = area;
                _operationType = operationType;
                _compositingMode = compositingMode;
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                _originalSlice?.Dispose();
                _selectionBitmap?.Dispose();
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                Graphics gfx = Graphics.FromImage(_targetbitmap);

                gfx.CompositingMode = CompositingMode.SourceCopy;

                switch (_operationType)
                {
                    case SelectionOperationType.Moved:
                        // Draw the original slice back
                        gfx.DrawImage(_originalSlice, _area, new Rectangle(0, 0, _originalSlice.Width, _originalSlice.Height), GraphicsUnit.Pixel);
                        // Draw the selection back
                        gfx.DrawImage(_selectionBitmap, _selectionStartArea, new Rectangle(0, 0, _selectionBitmap.Width, _selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                    case SelectionOperationType.Cut:
                        // Draw the original slice back
                        gfx.DrawImage(_selectionBitmap, _selectionStartArea, new Rectangle(0, 0, _selectionBitmap.Width, _selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                    case SelectionOperationType.Paste:
                        // Draw the original slice back
                        gfx.DrawImage(_originalSlice, _area, new Rectangle(0, 0, _originalSlice.Width, _originalSlice.Height), GraphicsUnit.Pixel);
                        break;
                }

                gfx.Flush();
                gfx.Dispose();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                Graphics gfx = Graphics.FromImage(_targetbitmap);
                Region reg;

                gfx.CompositingMode = _compositingMode;

                switch (_operationType)
                {
                    case SelectionOperationType.Moved:
                        // Clear the image background
                        reg = gfx.Clip;
                        gfx.SetClip(_selectionStartArea);
                        gfx.Clear(Color.Transparent);
                        gfx.Clip = reg;
                        gfx.DrawImage(_selectionBitmap, _area, new Rectangle(0, 0, _selectionBitmap.Width, _selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                    case SelectionOperationType.Cut:
                        reg = gfx.Clip;
                        gfx.SetClip(_selectionStartArea);
                        gfx.Clear(Color.Transparent);
                        gfx.Clip = reg;
                        break;
                    case SelectionOperationType.Paste:
                        // Draw the original slice back
                        gfx.DrawImage(_selectionBitmap, _area, new Rectangle(0, 0, _selectionBitmap.Width, _selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                }

                gfx.Flush();
                gfx.Dispose();
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                switch (_operationType)
                {
                    case SelectionOperationType.Cut:
                        return "Cut";
                    case SelectionOperationType.Moved:
                        return "Move";
                    case SelectionOperationType.Paste:
                        return "Paste";
                    default:
                        return "Selection";
                }
            }
        }

        /// <summary>
        /// Describes the type of operation of a SelectionUndoTask
        /// </summary>
        public enum SelectionOperationType
        {
            /// <summary>
            /// The selection was cut from the canvas
            /// </summary>
            Cut,
            /// <summary>
            /// The selection was moved on the canvas
            /// </summary>
            Moved,
            /// <summary>
            /// The selection was pasted on the canvas
            /// </summary>
            Paste
        }
        
        /// <summary>
        /// A class that abstracts away painting of selection regions on a selection paint tool
        /// </summary>
        public class SelectionPaintToolPainter
        {
            /// <summary>
            /// Draws a selection region on a given graphics, with a given dash offset.
            /// The dash offset can be alternated to produce an animation
            /// </summary>
            public static void PaintSelectionRectangle(Graphics graphics, Rectangle region, float dashOffset)
            {
                PaintSelectionRectangle(graphics, (RectangleF) region, dashOffset);
            }

            /// <summary>
            /// Draws a selection region on a given graphics, with a given dash offset.
            /// The dash offset can be alternated to produce an animation
            /// </summary>
            public static void PaintSelectionRectangle(Graphics graphics, RectangleF region, float dashOffset)
            {
                graphics.PixelOffsetMode = PixelOffsetMode.Default;

                var p = new Pen(Color.Black)
                {
                    DashStyle = DashStyle.Dash,
                    DashOffset = dashOffset,
                    DashPattern = new[] { 2f, 2f },
                    Alignment = PenAlignment.Inset,
                    Width = 1
                };

                graphics.DrawRectangles(p, new []{ region });

                p = new Pen(Color.White)
                {
                    DashStyle = DashStyle.Dash,
                    DashOffset = dashOffset + 1,
                    DashPattern = new[] { 2f, 2f },
                    Alignment = PenAlignment.Inset,
                    Width = 1
                };

                graphics.DrawRectangles(p, new[] { region });
            }
        }
    }
}
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Data.Undo;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Selection paint operation
    /// </summary>
    public class SelectionPaintOperation : BaseDraggingPaintOperation, IClipboardPaintOperation, ICompositingPaintOperation
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
        /// The mode of the current selection operation
        /// </summary>
        private SelectionOperationType _operationMode;

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
        public bool Selected { get { return selected; } }

        /// <summary>
        /// Gets or sets the bitmap that represents the currently selected graphics
        /// </summary>
        public Bitmap SelectionBitmap { get { return selectionBitmap; } set { selectionBitmap = value; pictureBox.Invalidate(); } }

        /// <summary>
        /// Gets the area the selection is currently occupying in the canvas
        /// </summary>
        public Rectangle SelectionArea { get { return selectedArea; } set { selectedArea = value; pictureBox.Invalidate(GetSelectionArea(true)); } }

        /// <summary>
        /// Gets the area the selection was snipped from
        /// </summary>
        public Rectangle SelectionStartArea { get { return selectedStartArea; } }

        /// <summary>
        /// Gets the operation currently being performed by this SelectionPaintOperation
        /// </summary>
        public SelectionOperationType OperationType { get { return _operationMode; } }

        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        public CompositingMode CompositingMode { get { return compositingMode; } set { compositingMode = value; if (selected) { pictureBox.Invalidate(GetSelectionArea(true)); } } }

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
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            _animTimer = new Timer();
            _animTimer.Interval = 200;
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
            _dashOffset += 1f;
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

            OperationCursor.Dispose();

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

            StartOperation(new Rectangle(0, 0, pictureBox.Image.Width, pictureBox.Image.Height), null);
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
            _operationMode = SelectionOperationType.Cut;

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

                _operationMode = SelectionOperationType.Paste;

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

                if (displaySelection)
                {
                    Rectangle rec = GetSelectionArea(false);

                    rec.Width--;
                    rec.Height--;

                    Pen p = new Pen(Color.Black)
                    {
                        DashStyle = DashStyle.Dash,
                        DashOffset = _dashOffset,
                        DashPattern = new[] {2f, 2f},
                        Alignment = PenAlignment.Inset,
                        Width = 1
                    };

                    gfx.DrawRectangle(p, rec);

                    p = new Pen(Color.White)
                    {
                        DashStyle = DashStyle.Dash,
                        DashOffset = _dashOffset + 1.99f,
                        DashPattern = new[] {2f, 2f},
                        Alignment = PenAlignment.Inset,
                        Width = 1
                    };

                    gfx.DrawRectangle(p, rec);

                    p.Dispose();
                }

                gfx.Flush();
                gfx.Dispose();
            }
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
                if (selected && (selectedArea != selectedStartArea || ForceApplyChanges || _operationMode == SelectionOperationType.Paste))
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
                    FinishOperation(true);
                    StartOperation(selectedArea, ExtractBitmap(selectedArea), SelectionOperationType.Paste);
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

                if (selectedArea.Contains(p))
                {
                    pictureBox.Cursor = Cursors.SizeAll;
                }
                else
                {
                    pictureBox.Cursor = OperationCursor;
                }
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

                    if(_operationMode != SelectionOperationType.Paste)
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

                        _operationMode = SelectionOperationType.Moved;

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
            if (e.KeyCode == Keys.Delete)
            {
                if (selected)
                {
                    if (_operationMode == SelectionOperationType.Paste)
                    {
                        CancelOperation(false);
                    }
                    else
                    {
                        FinishOperation(false);
                        pictureBox.MarkModified();
                    }

                    _operationMode = SelectionOperationType.Cut;
                }
            }
            // Selection moving
            else if (e.KeyCode == Keys.Left)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.X--;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
            else if (e.KeyCode == Keys.Right)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.X++;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
            else if (e.KeyCode == Keys.Up)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.Y--;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
            else if (e.KeyCode == Keys.Down)
            {
                pictureBox.Invalidate(GetSelectionArea(true));
                selectedArea.Y++;
                pictureBox.Invalidate(GetSelectionArea(true));
            }
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
            StartOperation(area, pasteBitmap, _operationMode);
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

            _operationMode = operation;

            ForceApplyChanges = false;

            if (selectionBitmap != null)
            {
                selectionBitmap.Dispose();
            }

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

            if (_operationMode == SelectionOperationType.Moved && drawOnCanvas)
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

            pictureBox.Cursor = OperationCursor;

            if (selectionBitmap != null)
                selectionBitmap.Dispose();
            selectionBitmap = null;

            movingSelection = false;
            displaySelection = true;

            selected = false;
            _animTimer.Stop();

            UpdateClipboardState();

            // Default the operation mode to 'Moved'
            _operationMode = SelectionOperationType.Moved;
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

            if (selectedArea != selectedStartArea || _operationMode != SelectionOperationType.Moved || ForceApplyChanges)
            {
                // Record the undo operation
                pictureBox.OwningPanel.UndoSystem.RegisterUndo(new SelectionUndoTask(pictureBox, selectionBitmap, originalSlice, selectedStartArea, selectedArea, _operationMode, compositingMode));

                pictureBox.MarkModified();
            }

            Rectangle rec = GetSelectionArea(true);

            pictureBox.Invalidate(rec);

            pictureBox.Cursor = OperationCursor;

            selectionBitmap.Dispose();
            selectionBitmap = null;

            movingSelection = false;
            displaySelection = true;

            selected = false;
            _animTimer.Stop();

            UpdateClipboardState();

            // Default the operation mode to 'Moved'
            _operationMode = SelectionOperationType.Moved;

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
        protected class SelectionUndoTask : IUndoTask
        {
            /// <summary>
            /// The target picture box for the undo task
            /// </summary>
            readonly ImageEditPanel.InternalPictureBox _pictureBox;

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
            /// <param name="pictureBox">The target picture box for the undo task</param>
            /// <param name="selectionBitmap">The selection bitmap</param>
            /// <param name="originalSlice">The original image slice before a Move operation was made</param>
            /// <param name="selectionStartArea">The area that the selection started at</param>
            /// <param name="area">The area of the affected SelectionUndoTask</param>
            /// <param name="operationType">The operation type of this SelectionUndoTask</param>
            /// <param name="compositingMode">The compositing mode for the operation</param>
            public SelectionUndoTask(ImageEditPanel.InternalPictureBox pictureBox, Bitmap selectionBitmap, Bitmap originalSlice, Rectangle selectionStartArea, Rectangle area, SelectionOperationType operationType, CompositingMode compositingMode)
            {
                _pictureBox = pictureBox;
                _selectionBitmap = (Bitmap)selectionBitmap.Clone();
                _originalSlice = (originalSlice == null ? null : (Bitmap)originalSlice.Clone());
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
                if (_originalSlice != null)
                {
                    _originalSlice.Dispose();
                }

                if (_selectionBitmap != null)
                {
                    _selectionBitmap.Dispose();
                }
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                Graphics gfx = Graphics.FromImage(_pictureBox.Bitmap);

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

                _pictureBox.Invalidate();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                Graphics gfx = Graphics.FromImage(_pictureBox.Bitmap);
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
                        //gfx.DrawImage(selectionBitmap, area.X, area.Y);
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
                        //gfx.DrawImage(selectionBitmap, area.X, area.Y);
                        gfx.DrawImage(_selectionBitmap, _area, new Rectangle(0, 0, _selectionBitmap.Width, _selectionBitmap.Height), GraphicsUnit.Pixel);
                        break;
                }

                gfx.Flush();
                gfx.Dispose();

                _pictureBox.Invalidate();
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
    }
}
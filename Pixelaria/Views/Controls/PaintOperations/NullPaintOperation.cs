using System;
using System.Drawing;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Specifies an empty paint operation
    /// </summary>
    public class NullPaintOperation : IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        public Cursor OperationCursor { get { return Cursors.Default; } }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox) { }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public void Destroy() { }

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        public void ChangeBitmap(Bitmap newBitmap) { }

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void Paint(PaintEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseDown(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseMove(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseUp(MouseEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseLeave(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void MouseEnter(EventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void KeyDown(KeyEventArgs e) { }

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public void KeyUp(KeyEventArgs e) { }
    }
}
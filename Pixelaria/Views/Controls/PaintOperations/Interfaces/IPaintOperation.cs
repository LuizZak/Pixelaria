using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls.PaintOperations.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation to be performed on the InternalPictureBox
    /// </summary>
    public interface IPaintOperation
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this operation is up
        /// </summary>
        Cursor OperationCursor { get; }

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        bool Loaded { get; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox);

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        void Destroy();

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        void ChangeBitmap(Bitmap newBitmap);

        /// <summary>
        /// Called to notify this PaintOperation that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void Paint(PaintEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseDown(MouseEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseMove(MouseEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseUp(MouseEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseLeave(EventArgs e);

        /// <summary>
        /// Called to notify this PaintOperaiton that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseEnter(EventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyDown(KeyEventArgs e);

        /// <summary>
        /// Called to notify this PaintOperation that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyUp(KeyEventArgs e);
    }
}
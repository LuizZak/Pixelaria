using System;
using System.Drawing;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls.PaintTools.Interfaces
{
    /// <summary>
    /// Specifies a Paint Tool to be used on a ImageEditPanel.InternalPictureBox
    /// </summary>
    public interface IPaintTool
    {
        /// <summary>
        /// Gets the cursor to use when hovering over the InternalPictureBox while this tool is up
        /// </summary>
        Cursor ToolCursor { get; }

        /// <summary>
        /// Gets whether this Paint Tool has resources loaded
        /// </summary>
        bool Loaded { get; }

        /// <summary>
        /// Initializes this Paint Tool
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox);

        /// <summary>
        /// Finalizes this Paint Tool
        /// </summary>
        void Destroy();

        /// <summary>
        /// Changes the bitmap currently being edited
        /// </summary>
        /// <param name="newBitmap">The new bitmap being edited</param>
        void ChangeBitmap(Bitmap newBitmap);

        /// <summary>
        /// Called to notify this Paint Tool that the control is being redrawn
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void Paint(PaintEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseDown(MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseMove(MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseUp(MouseEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse left the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseLeave(EventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that the mouse entered the image area
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void MouseEnter(EventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that a keyboard key was pressed down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyDown(KeyEventArgs e);

        /// <summary>
        /// Called to notify this Paint Tool that a keyboard key was released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        void KeyUp(KeyEventArgs e);
    }
}
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Pixelaria.Views.Controls.ColorControls;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Picker paint operation
    /// </summary>
    public class PickerPaintOperation : BasePaintOperation
    {
        /// <summary>
        /// The last absolute position of the mouse
        /// </summary>
        protected Point lastMousePointAbsolute;

        /// <summary>
        /// Gets whether this Paint Operation has resources loaded
        /// </summary>
        public override bool Loaded { get; protected set; }

        /// <summary>
        /// Initializes this Paint Operation
        /// </summary>
        /// <param name="targetPictureBox">The picture box to initialize the paint operation on</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            pictureBox = targetPictureBox;

            lastMousePointAbsolute = new Point(-1, -1);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.picker_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            Loaded = true;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            pictureBox = null;

            OperationCursor.Dispose();

            Loaded = false;
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            MouseMove(e);
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being moved
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                Point absolute = GetAbsolutePoint(e.Location);

                if (absolute != lastMousePointAbsolute)
                {
                    if (WithinBounds(absolute))
                    {
                        Color color = pictureBox.Bitmap.GetPixel(absolute.X, absolute.Y);

                        pictureBox.OwningPanel.FireColorChangeEvent(color, e.Button == MouseButtons.Left ? ColorPickerColor.FirstColor : ColorPickerColor.SecondColor);
                    }
                }

                lastMousePointAbsolute = absolute;
            }
        }
    }
}
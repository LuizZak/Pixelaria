using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Pencil paint operation
    /// </summary>
    public class PencilPaintOperation : BasePencilPaintOperation, IColoredPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// Initializes a new instance of the PencilPaintOperation class
        /// </summary>
        public PencilPaintOperation()
            : base()
        {
            this.undoDecription = "Pencil";
        }

        /// <summary>
        /// Initializes a new instance of the PencilPaintOperation class, initializing the object
        /// with the two pencil colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public PencilPaintOperation(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.FirstColor = firstColor;
            this.SecondColor = secondColor;
            this.Size = 1;
        }

        /// <summary>
        /// Initializes this PencilPaintOperation
        /// </summary>
        /// <param name="pictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.pencil_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }
    }
}
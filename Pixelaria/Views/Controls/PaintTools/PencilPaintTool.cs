using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Pencil paint operation
    /// </summary>
    public class PencilPaintTool : BasePencilPaintTool, IColoredPaintTool, ICompositingPaintTool
    {
        /// <summary>
        /// Initializes a new instance of the PencilPaintTool class
        /// </summary>
        public PencilPaintTool()
        {
            undoDecription = "Pencil";
        }

        /// <summary>
        /// Initializes a new instance of the PencilPaintTool class, initializing the object
        /// with the two pencil colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public PencilPaintTool(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
            size = 1;
        }

        /// <summary>
        /// Initializes this PencilPaintTool
        /// </summary>
        /// <param name="targetPictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.pencil_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }
    }
}
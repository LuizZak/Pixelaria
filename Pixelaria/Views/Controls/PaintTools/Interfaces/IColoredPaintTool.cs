using System.Drawing;

namespace Pixelaria.Views.Controls.PaintTools.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation that has a color component
    /// </summary>
    public interface IColoredPaintTool
    {
        /// <summary>
        /// Gets or sets the first color being used to paint on the InternalPictureBox
        /// </summary>
        Color FirstColor { get; set; }

        /// <summary>
        /// Gets or sets the second color being used to paint on the InternalPictureBox
        /// </summary>
        Color SecondColor { get; set; }
    }
}
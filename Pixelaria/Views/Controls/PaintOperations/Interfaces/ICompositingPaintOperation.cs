using System.Drawing.Drawing2D;

namespace Pixelaria.Views.Controls.PaintOperations.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation that has a compositing mode
    /// </summary>
    public interface ICompositingPaintOperation
    {
        /// <summary>
        /// Gets or sets the compositing mode for this paint operation
        /// </summary>
        CompositingMode CompositingMode { get; set; }
    }
}
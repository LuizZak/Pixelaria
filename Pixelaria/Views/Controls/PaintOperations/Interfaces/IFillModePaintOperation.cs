namespace Pixelaria.Views.Controls.PaintOperations.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation that has a fill mode
    /// </summary>
    public interface IFillModePaintOperation
    {
        /// <summary>
        /// Gets or sets the FillMode for this paint operation
        /// </summary>
        OperationFillMode FillMode { get; set; }
    }
}
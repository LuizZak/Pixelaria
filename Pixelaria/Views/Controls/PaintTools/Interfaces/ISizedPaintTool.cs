using System.ComponentModel;

namespace Pixelaria.Views.Controls.PaintTools.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation that has a size component
    /// </summary>
    public interface ISizedPaintTool
    {
        /// <summary>
        /// Gets or sets the size of this SizedPaintOperation
        /// </summary>
        [DefaultValue(1)]
        int Size { get; set; }
    }
}
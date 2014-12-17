namespace Pixelaria.Views.Controls.PaintOperations.Interfaces
{
    /// <summary>
    /// Specifies a Paint Operation that has clipboard access capabilities
    /// </summary>
    public interface IClipboardPaintOperation
    {
        /// <summary>
        /// Performs a Copy operation
        /// </summary>
        void Copy();

        /// <summary>
        /// Performs a Cut operation
        /// </summary>
        void Cut();

        /// <summary>
        /// Performs a Paste operation
        /// </summary>
        void Paste();

        /// <summary>
        /// Returns whether the paint operation can copy content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can copy content to the clipboard</returns>
        bool CanCopy();

        /// <summary>
        /// Returns whether the paint operation can cut content to the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can cut content to the clipboard</returns>
        bool CanCut();

        /// <summary>
        /// Returns whether the paint operation can paste content from the clipboard
        /// </summary>
        /// <returns>Whether the paint operation can paste content from the clipboard</returns>
        bool CanPaste();
    }
}
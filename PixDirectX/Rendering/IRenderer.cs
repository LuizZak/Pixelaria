using System.Drawing;
using PixCore.Geometry;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// An object that encapsulates rendering capabilities.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Gets or sets the color for Fill- operations
        /// </summary>
        Color FillColor { get; set; }
        /// <summary>
        /// Gets or sets the color for Stroke- operations
        /// </summary>
        Color StrokeColor { get; set; }

        #region Stroke

        /// <summary>
        /// Strokes a line with the current <see cref="StrokeColor"/>.
        /// </summary>
        void StrokeLine(Vector start, Vector end);
        /// <summary>
        /// Strokes the outline of a circle with the current <see cref="StrokeColor"/>.
        /// </summary>
        void StrokeCircle(Vector center, float radius);
        /// <summary>
        /// Strokes the outline of an ellipse with the current <see cref="StrokeColor"/>.
        /// </summary>
        void StrokeEllipse(AABB ellipseArea);
        /// <summary>
        /// Strokes the outline of a rectangle with the current <see cref="StrokeColor"/>.
        /// </summary>
        void StrokeRectangle(RectangleF rectangle);
        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with the current <see cref="StrokeColor"/>.
        /// </summary>
        void StrokeArea(AABB area);
        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current <see cref="StrokeColor"/>.
        /// </summary>
        void StrokeRoundedArea(AABB area, float radiusX, float radiusY);

        #endregion

        #region Fill

        /// <summary>
        /// Fills the area of a circle with the current <see cref="StrokeColor"/>.
        /// </summary>
        void FillCircle(Vector center, float radius);
        /// <summary>
        /// Fills the area of an ellipse with the current <see cref="StrokeColor"/>.
        /// </summary>
        void FillEllipse(AABB ellipseArea);
        /// <summary>
        /// Fills the area of a rectangle with the current <see cref="StrokeColor"/>.
        /// </summary>
        void FillRectangle(RectangleF rectangle);
        /// <summary>
        /// Fills an <see cref="AABB"/>-bounded area with the current <see cref="StrokeColor"/>.
        /// </summary>
        void FillArea(AABB area);
        /// <summary>
        /// Fills the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current <see cref="StrokeColor"/>.
        /// </summary>
        void FillRoundedArea(AABB area, float radiusX, float radiusY);

        #endregion

        #region Bitmap

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolation">The interpolation mode to use when rendering the image.</param>
        void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolation interpolation);

        #endregion
    }

    /// <summary>
    /// Specifies the image interpolation mode to use when rendering bitmaps in an <see cref="IRenderer"/>.
    /// </summary>
    public enum ImageInterpolation
    {
        NearestNeighbor,
        Linear
    }
}

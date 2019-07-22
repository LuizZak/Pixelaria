/*
    Pixelaria
    Copyright (C) 2013 Luiz Fernando Silva

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation; either version 2 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along
    with this program; if not, write to the Free Software Foundation, Inc.,
    51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.

    The full license may be found on the License.txt file attached to the
    base directory of this project.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using PixCore.Geometry;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// An object that encapsulates rendering capabilities.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Gets or sets the topmost active transformation matrix.
        /// </summary>
        Matrix2D Transform { get; set; }

        #region Stroke

        /// <summary>
        /// Strokes a line with the current stroke brush.
        /// </summary>
        void StrokeLine(Vector start, Vector end, float strokeWidth = 1);
        /// <summary>
        /// Strokes the outline of a circle with the current stroke brush.
        /// </summary>
        void StrokeCircle(Vector center, float radius, float strokeWidth = 1);
        /// <summary>
        /// Strokes the outline of an ellipse with the current stroke brush.
        /// </summary>
        void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1);
        /// <summary>
        /// Strokes the outline of a rectangle with the current stroke brush.
        /// </summary>
        void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1);
        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with the current stroke brush.
        /// </summary>
        void StrokeArea(AABB area, float strokeWidth = 1);
        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current stroke brush.
        /// </summary>
        void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1);
        /// <summary>
        /// Strokes a geometrical object with the current stroke brush.
        /// </summary>
        void StrokeGeometry([NotNull] PolyGeometry geometry, float strokeWidth = 1);
        /// <summary>
        /// Strokes a path geometry object with the current stroke brush.
        /// </summary>
        void StrokePath([NotNull] IPathGeometry path, float strokeWidth = 1);

        #endregion

        #region Fill

        /// <summary>
        /// Fills the area of a circle with the current fill brush.
        /// </summary>
        void FillCircle(Vector center, float radius);
        /// <summary>
        /// Fills the area of an ellipse with the current fill brush.
        /// </summary>
        void FillEllipse(AABB ellipseArea);
        /// <summary>
        /// Fills the area of a rectangle with the current fill brush.
        /// </summary>
        void FillRectangle(RectangleF rectangle);
        /// <summary>
        /// Fills an <see cref="AABB"/>-bounded area with the current fill brush.
        /// </summary>
        void FillArea(AABB area);
        /// <summary>
        /// Fills the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current fill brush.
        /// </summary>
        void FillRoundedArea(AABB area, float radiusX, float radiusY);
        /// <summary>
        /// Fills a geometrical object with the current fill brush.
        /// </summary>
        void FillGeometry([NotNull] PolyGeometry geometry);
        /// <summary>
        /// Fills a path geometry with the current fill brush.
        /// </summary>
        void FillPath([NotNull] IPathGeometry path);

        #endregion

        #region Path Geometry

        /// <summary>
        /// Creates a path geometry by invoking path-drawing operations on an
        /// <see cref="IPathInputSink"/> provided within a closure.
        ///
        /// The path returned by this method can then be used in further rendering
        /// operations by this <see cref="IRenderer"/>.
        /// </summary>
        IPathGeometry CreatePath([NotNull] Action<IPathInputSink> execute);

        #endregion

        #region Bitmap

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode);

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        void DrawBitmap(ImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode);

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        void DrawBitmap([NotNull] IManagedImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode);

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        void DrawBitmap([NotNull] IManagedImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode);

        #endregion

        #region Clipping

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        void PushClippingArea(AABB area);

        /// <summary>
        /// Pops the most recently pushed clipping area.
        /// </summary>
        void PopClippingArea();

        #endregion

        #region Transformation

        /// <summary>
        /// Pushes an Identity 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        void PushTransform();

        /// <summary>
        /// Pushes a 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        void PushTransform(Matrix2D matrix);

        /// <summary>
        /// Pops the top-most active transformation matrix.
        /// </summary>
        void PopTransform();

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform()"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        void PushingTransform([NotNull] Action execute);

        #endregion

        #region Brush

        /// <summary>
        /// Sets the stroke color for this renderer.
        /// </summary>
        void SetStrokeColor(Color color);

        /// <summary>
        /// Sets the stroke brush for this renderer.
        /// </summary>
        void SetStrokeBrush([NotNull] IBrush brush);

        /// <summary>
        /// Sets the fill color for this renderer.
        /// </summary>
        void SetFillColor(Color color);

        /// <summary>
        /// Sets the fill brush for this renderer.
        /// </summary>
        void SetFillBrush([NotNull] IBrush brush);

        /// <summary>
        /// Creates a linear gradient brush for drawing.
        /// </summary>
        ILinearGradientBrush CreateLinearGradientBrush([NotNull] IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end);

        #endregion
    }

    /// <summary>
    /// Specifies the image interpolation mode to use when rendering bitmaps in an <see cref="IRenderer"/>.
    /// </summary>
    public enum ImageInterpolationMode
    {
        NearestNeighbor,
        Linear
    }
}

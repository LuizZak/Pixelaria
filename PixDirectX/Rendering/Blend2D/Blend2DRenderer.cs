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
using Blend2DCS;
using PixCore.Geometry;
using PixRendering;

namespace PixDirectX.Rendering.Blend2D
{
    public class Blend2DRenderer: IRenderer
    {
        private readonly BLContext _context;

        /// <summary>
        /// Gets or sets the topmost active transformation matrix.
        /// </summary>
        public Matrix2D Transform { get; set; }

        public Blend2DRenderer(BLContext context)
        {
            _context = context;
        }

        #region Stroke

        /// <summary>
        /// Strokes a line with the current stroke brush.
        /// </summary>
        public void StrokeLine(Vector start, Vector end, float strokeWidth = 1)
        {

        }

        /// <summary>
        /// Strokes the outline of a circle with the current stroke brush.
        /// </summary>
        public void StrokeCircle(Vector center, float radius, float strokeWidth = 1)
        {

        }

        /// <summary>
        /// Strokes the outline of an ellipse with the current stroke brush.
        /// </summary>
        public void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1)
        {

        }
        /// <summary>
        /// Strokes the outline of a rectangle with the current stroke brush.
        /// </summary>
        public void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1)
        {

        }

        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with the current stroke brush.
        /// </summary>
        public void StrokeArea(AABB area, float strokeWidth = 1)
        {

        }

        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current stroke brush.
        /// </summary>
        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1)
        {
            
        }

        /// <summary>
        /// Strokes a geometrical object with the current stroke brush.
        /// </summary>
        public void StrokeGeometry(PolyGeometry geometry, float strokeWidth = 1)
        {

        }

        /// <summary>
        /// Strokes a path geometry object with the current stroke brush.
        /// </summary>
        public void StrokePath(IPathGeometry path, float strokeWidth = 1)
        {

        }

        #endregion

        #region Fill

        /// <summary>
        /// Fills the area of a circle with the current fill brush.
        /// </summary>
        public void FillCircle(Vector center, float radius)
        {

        }

        /// <summary>
        /// Fills the area of an ellipse with the current fill brush.
        /// </summary>
        public void FillEllipse(AABB ellipseArea)
        {

        }

        /// <summary>
        /// Fills the area of a rectangle with the current fill brush.
        /// </summary>
        public void FillRectangle(RectangleF rectangle)
        {

        }

        /// <summary>
        /// Fills an <see cref="AABB"/>-bounded area with the current fill brush.
        /// </summary>
        public void FillArea(AABB area)
        {

        }

        /// <summary>
        /// Fills the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current fill brush.
        /// </summary>
        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {

        }

        /// <summary>
        /// Fills a geometrical object with the current fill brush.
        /// </summary>
        public void FillGeometry(PolyGeometry geometry)
        {

        }

        /// <summary>
        /// Fills a path geometry with the current fill brush.
        /// </summary>
        public void FillPath(IPathGeometry path)
        {

        }

        #endregion

        #region Path Geometry

        /// <summary>
        /// Creates a path geometry by invoking path-drawing operations on an
        /// <see cref="IPathInputSink"/> provided within a closure.
        ///
        /// The path returned by this method can then be used in further rendering
        /// operations by this <see cref="IRenderer"/>.
        /// </summary>
        public IPathGeometry CreatePath(Action<IPathInputSink> execute)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Bitmap

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        /// <param name="tintColor">A color to use as a tinting color for the bitmap. If null, no color tinting is performed.</param>
        public void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {

        }

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        /// <param name="tintColor">A color to use as a tinting color for the bitmap. If null, no color tinting is performed.</param>
        public void DrawBitmap(ImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {

        }

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        /// <param name="tintColor">A color to use as a tinting color for the bitmap. If null, no color tinting is performed.</param>
        public void DrawBitmap(IManagedImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {

        }

        /// <summary>
        /// Renders an image resource.
        /// </summary>
        /// <param name="image">An image resource to render.</param>
        /// <param name="region">The region to render the image to. The image is stretched to fill this region's size exactly.</param>
        /// <param name="opacity">Opacity to use when rendering the image.</param>
        /// <param name="interpolationMode">The interpolation mode to use when rendering the image.</param>
        /// <param name="tintColor">A color to use as a tinting color for the bitmap. If null, no color tinting is performed.</param>
        public void DrawBitmap(IManagedImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {

        }

        #endregion

        #region Clipping

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        public void PushClippingArea(AABB area)
        {

        }

        /// <summary>
        /// Pops the most recently pushed clipping area.
        /// </summary>
        public void PopClippingArea()
        {

        }

        #endregion

        #region Transformation

        /// <summary>
        /// Pushes an Identity 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform()
        {

        }

        /// <summary>
        /// Pushes a 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform(Matrix2D matrix)
        {

        }

        /// <summary>
        /// Pops the top-most active transformation matrix.
        /// </summary>
        public void PopTransform()
        {

        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform(Matrix2D)"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Matrix2D matrix, Action execute)
        {

        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform()"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Action execute)
        {

        }

        #endregion

        #region Brush

        /// <summary>
        /// Sets the stroke color for this renderer.
        /// </summary>
        public void SetStrokeColor(Color color)
        {

        }

        /// <summary>
        /// Sets the stroke brush for this renderer.
        /// </summary>
        public void SetStrokeBrush(IBrush brush)
        {

        }

        /// <summary>
        /// Sets the fill color for this renderer.
        /// </summary>
        public void SetFillColor(Color color)
        {

        }

        /// <summary>
        /// Sets the fill brush for this renderer.
        /// </summary>
        public void SetFillBrush(IBrush brush)
        {

        }

        /// <summary>
        /// Creates a linear gradient brush for drawing.
        /// </summary>
        public ILinearGradientBrush CreateLinearGradientBrush(IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        ///
        /// Bitmap brushes by default tile the image contents when drawn within bounds that exceed that of the original image's size.
        /// </summary>
        public IBrush CreateBitmapBrush(ImageResource image)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        ///
        /// Bitmap brushes by default tile the image contents when drawn within bounds that exceed that of the original image's size.
        /// </summary>
        public IBrush CreateBitmapBrush(IManagedImageResource image)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

﻿/*
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
using System.Linq;
using Blend2DCS;
using Blend2DCS.Geometry;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Rendering.DirectX;
using PixDirectX.Utils;
using PixRendering;
using SharpDX.Direct2D1;

namespace PixDirectX.Rendering.Blend2D
{
    public class Blend2DRenderer: IRenderer
    {
        private InternalBrush _strokeBrush;
        private InternalBrush _fillBrush;

        private readonly Blend2DImageResources _imageResources;
        private readonly BLContext _context;

        /// <summary>
        /// Gets or sets the topmost active transformation matrix.
        /// </summary>
        public Matrix2D Transform 
        {
            get => _context.UserMatrix.ToMatrix2D();
            set => _context.SetMatrix(value.ToBLMatrix());
        }

        public Blend2DRenderer(BLContext context, Blend2DImageResources imageResources)
        {
            _context = context;
            _imageResources = imageResources;
        }

        private void SetBrushForStroke(float strokeWidth)
        {
            _context.SetStrokeWidth(strokeWidth);
            _strokeBrush.LoadBrush(_context);
        }

        private void SetBrushForFill()
        {
            _fillBrush.LoadBrush(_context);
        }

        #region Stroke

        /// <summary>
        /// Strokes a line with the current stroke brush.
        /// </summary>
        public void StrokeLine(Vector start, Vector end, float strokeWidth = 1)
        {
            var path = new BLPath();
            path.MoveTo(start.X, start.Y);
            path.LineTo(end.X, end.Y);

            SetBrushForStroke(strokeWidth);
            _context.StrokePath(path);
        }

        /// <summary>
        /// Strokes the outline of a circle with the current stroke brush.
        /// </summary>
        public void StrokeCircle(Vector center, float radius, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);
            _context.StrokeCircle(new BLCircle(center.X, center.Y, radius));
        }

        /// <summary>
        /// Strokes the outline of an ellipse with the current stroke brush.
        /// </summary>
        public void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);
            _context.StrokeEllipse(new BLEllipse(ellipseArea.Left, ellipseArea.Top, ellipseArea.Width / 2, ellipseArea.Height / 2));
        }

        /// <summary>
        /// Strokes the outline of a rectangle with the current stroke brush.
        /// </summary>
        public void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);
            _context.StrokeRectangle(new BLRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
        }

        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with the current stroke brush.
        /// </summary>
        public void StrokeArea(AABB area, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);
            _context.StrokeRectangle(area.ToBLRect());
        }

        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current stroke brush.
        /// </summary>
        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);

            var rect = new BLRoundRect(area.Left, area.Top, area.Width, area.Height, radiusX, radiusY);
            _context.StrokeRoundRectangle(rect);
        }

        /// <summary>
        /// Strokes a geometrical object with the current stroke brush.
        /// </summary>
        public void StrokeGeometry(PolyGeometry geometry, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);

            foreach (var polygon in geometry.Polygons())
            {
                var path = new BLPath();
                path.MoveTo(polygon[0].X, polygon[0].Y);
                foreach (var vector in polygon.Skip(1))
                {
                    path.LineTo(vector.X, vector.Y);
                }
                path.LineTo(polygon[0].X, polygon[0].Y);

                _context.StrokePath(path);
            }
        }

        /// <summary>
        /// Strokes a path geometry object with the current stroke brush.
        /// </summary>
        public void StrokePath(IPathGeometry path, float strokeWidth = 1)
        {
            SetBrushForStroke(strokeWidth);

            var internalPath = CastPathOrFail(path);

            _context.StrokePath(internalPath.Path);
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

        private static InternalBrush CastBrushOrFail([NotNull] IBrush brush)
        {
            if (brush is InternalBrush internalBrush)
                return internalBrush;

            throw new InvalidOperationException($"Expected a brush of type {typeof(InternalBrush)}");
        }

        private static InternalPathGeometry CastPathOrFail([NotNull] IPathGeometry path)
        {
            if (path is InternalPathGeometry internalPath)
                return internalPath;

            throw new InvalidOperationException($"Expected a path geometry of type {typeof(InternalPathGeometry)}");
        }

        private static Blend2DBitmap CastBitmapOrFail([NotNull] IManagedImageResource bitmap)
        {
            if (bitmap is Blend2DBitmap blBitmap)
                return blBitmap;

            throw new InvalidOperationException($"Expected a bitmap of type {typeof(Blend2DBitmap)}");
        }

        private class InternalBrush : IBrush
        {
            internal bool IsLoaded { get; private set; }

            public virtual void LoadBrush(BLContext context)
            {
                IsLoaded = true;
            }

            public virtual void UnloadBrush()
            {
                if (!IsLoaded)
                    return;

                IsLoaded = false;
            }
        }

        private class InternalSolidBrush : InternalBrush, ISolidBrush
        {
            public Color Color { get; }

            public InternalSolidBrush(Color color)
            {
                Color = color;
            }

            public override void LoadBrush(BLContext context)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(context);
            }
        }

        private class InternalLinearBrush : InternalBrush, ILinearGradientBrush
        {
            public IReadOnlyList<PixGradientStop> GradientStops { get; }
            public Vector Start { get; }
            public Vector End { get; }

            public InternalLinearBrush([NotNull] IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
            {
                GradientStops = gradientStops;
                Start = start;
                End = end;
            }

            public override void LoadBrush(BLContext context)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(context);
            }

            public override void UnloadBrush()
            {
                if (!IsLoaded)
                    return;

                base.UnloadBrush();
            }
        }

        private class InternalBitmapBrush : InternalBrush
        {
            public BLImage Bitmap { get; }

            public InternalBitmapBrush(BLImage bitmap)
            {
                Bitmap = bitmap;
            }

            public override void LoadBrush(BLContext context)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(context);
            }
        }

        private class InternalPathSink : IPathInputSink
        {
            private readonly BLPath _path;

            public InternalPathSink(BLPath path)
            {
                _path = path;
            }

            public void BeginFigure(Vector location, bool filled)
            {
                _path.MoveTo(location.X, location.Y);
            }

            public void MoveTo(Vector point)
            {
                _path.MoveTo(point.X, point.Y);
            }

            public void LineTo(Vector point)
            {
                _path.LineTo(point.X, point.Y);
            }

            public void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint)
            {
                _path.CubicTo(anchor1.X, anchor1.Y, anchor2.X, anchor2.Y, endPoint.X, endPoint.Y);
            }

            public void AddRectangle(AABB rectangle)
            {
                _path.AddRectangle(rectangle.ToBLRect());
            }

            public void EndFigure(bool closePath)
            {
                
            }
        }

        private class InternalPathGeometry : IPathGeometry
        {
            public BLPath Path { get; }

            public InternalPathGeometry(BLPath path)
            {
                Path = path;
            }

            public void Dispose()
            {
                
            }
        }
    }
}

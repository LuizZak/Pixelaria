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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Utils;
using PixRendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace PixDirectX.Rendering.Skia
{
    public class SkiaRenderer: IRenderer
    {
        private InternalBrush _strokeBrush;
        private InternalBrush _fillBrush;

        private Stack<SKMatrix> _transformStack = new Stack<SKMatrix>();

        private readonly SKCanvas _canvas;
        private readonly SkiaImageResources _imageResource;

        /// <summary>
        /// Gets or sets the topmost active transformation matrix.
        /// </summary>
        public Matrix2D Transform { get; set; }

        public SkiaRenderer([NotNull] SKCanvas canvas, [NotNull] SkiaImageResources imageResource)
        {
            _canvas = canvas;
            _imageResource = imageResource;
        }

        private SKPaint BrushForStroke(float strokeWidth)
        {
            _strokeBrush.LoadBrush();
            _strokeBrush.Brush.Style = SKPaintStyle.Stroke;
            _strokeBrush.Brush.StrokeWidth = strokeWidth;
            return _strokeBrush.Brush;
        }

        private SKPaint BrushForFill()
        {
            _fillBrush.LoadBrush();
            _fillBrush.Brush.Style = SKPaintStyle.Fill;
            return _fillBrush.Brush;
        }

        #region Stroke

        /// <summary>
        /// Strokes a line with the current stroke brush.
        /// </summary>
        public void StrokeLine(Vector start, Vector end, float strokeWidth = 1)
        {
            _canvas.DrawLine(start.ToSKPoint(), end.ToSKPoint(), BrushForStroke(strokeWidth));
        }

        /// <summary>
        /// Strokes the outline of a circle with the current stroke brush.
        /// </summary>
        public void StrokeCircle(Vector center, float radius, float strokeWidth = 1)
        {
            _canvas.DrawCircle(center.ToSKPoint(), radius, BrushForStroke(strokeWidth));
        }

        /// <summary>
        /// Strokes the outline of an ellipse with the current stroke brush.
        /// </summary>
        public void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1)
        {
            _canvas.DrawOval(ellipseArea.ToSKRect(), BrushForStroke(strokeWidth));
        }

        /// <summary>
        /// Strokes the outline of a rectangle with the current stroke brush.
        /// </summary>
        public void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1)
        {
            _canvas.DrawRect(rectangle.ToSKRect(), BrushForStroke(strokeWidth));
        }

        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with the current stroke brush.
        /// </summary>
        public void StrokeArea(AABB area, float strokeWidth = 1)
        {
            _canvas.DrawRect(area.ToSKRect(), BrushForStroke(strokeWidth));
        }

        /// <summary>
        /// Strokes the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current stroke brush.
        /// </summary>
        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1)
        {
            _canvas.DrawRoundRect(area.ToSKRect(), radiusX, radiusY, BrushForStroke(strokeWidth));
        }

        /// <summary>
        /// Strokes a geometrical object with the current stroke brush.
        /// </summary>
        public void StrokeGeometry(PolyGeometry geometry, float strokeWidth = 1)
        {
            foreach (var polygon in geometry.Polygons())
            {
                _canvas.DrawPoints(SKPointMode.Polygon, polygon.Select(v => v.ToSKPoint()).ToArray(), BrushForStroke(strokeWidth));
            }
        }

        /// <summary>
        /// Strokes a path geometry object with the current stroke brush.
        /// </summary>
        public void StrokePath(IPathGeometry path, float strokeWidth = 1)
        {
            var pathGeom = CastPathOrFail(path);

            _canvas.DrawPath(pathGeom.PathGeometry, BrushForStroke(strokeWidth));
        }

        #endregion

        #region Fill

        /// <summary>
        /// Fills the area of a circle with the current fill brush.
        /// </summary>
        public void FillCircle(Vector center, float radius)
        {
            _canvas.DrawCircle(center.ToSKPoint(), radius, BrushForFill());
        }

        /// <summary>
        /// Fills the area of an ellipse with the current fill brush.
        /// </summary>
        public void FillEllipse(AABB ellipseArea)
        {
            _canvas.DrawOval(ellipseArea.ToSKRect(), BrushForFill());
        }

        /// <summary>
        /// Fills the area of a rectangle with the current fill brush.
        /// </summary>
        public void FillRectangle(RectangleF rectangle)
        {
            _canvas.DrawRect(rectangle.ToSKRect(), BrushForFill());
        }

        /// <summary>
        /// Fills an <see cref="AABB"/>-bounded area with the current fill brush.
        /// </summary>
        public void FillArea(AABB area)
        {
            _canvas.DrawRect(area.ToSKRect(), BrushForFill());
        }

        /// <summary>
        /// Fills the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current fill brush.
        /// </summary>
        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {
            _canvas.DrawRoundRect(area.ToSKRect(), radiusX, radiusY, BrushForFill());
        }

        /// <summary>
        /// Fills a geometrical object with the current fill brush.
        /// </summary>
        public void FillGeometry(PolyGeometry geometry)
        {
            foreach (var polygon in geometry.Polygons())
            {
                _canvas.DrawPoints(SKPointMode.Polygon, polygon.Select(v => v.ToSKPoint()).ToArray(), BrushForFill());
            }
        }

        /// <summary>
        /// Fills a path geometry with the current fill brush.
        /// </summary>
        public void FillPath(IPathGeometry path)
        {
            var pathGeom = CastPathOrFail(path);

            _canvas.DrawPath(pathGeom.PathGeometry, BrushForFill());
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
            var path = new SKPath();
            var sink = new InternalPathSink(path);
            execute(sink);
            return new InternalPathGeometry(path);
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
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            DrawBitmap(bitmap, region, opacity, interpolationMode, tintColor);
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
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            DrawBitmap(bitmap, (RectangleF)region, opacity, interpolationMode, tintColor);
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
            var bitmap = CastBitmapOrFail(image);

            DrawBitmap(bitmap.bitmap, region, opacity, interpolationMode, tintColor);
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
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode, tintColor);
        }

        private void DrawBitmap(SKBitmap bitmap, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            _canvas.DrawBitmap(bitmap, region.ToSKRect());
        }

        #endregion

        #region Clipping

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        public void PushClippingArea(AABB area)
        {
            _canvas.Save();
        }

        /// <summary>
        /// Pops the most recently pushed clipping area.
        /// </summary>
        public void PopClippingArea()
        {
            _canvas.Restore();
        }

        #endregion

        #region Transformation

        /// <summary>
        /// Pushes an Identity 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform()
        {
            _transformStack.Push(_canvas.TotalMatrix);
        }

        /// <summary>
        /// Pushes a 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform(Matrix2D matrix)
        {
            _transformStack.Push(_canvas.TotalMatrix);
            _canvas.SetMatrix(matrix.ToSKMatrix());
        }

        /// <summary>
        /// Pops the top-most active transformation matrix.
        /// </summary>
        public void PopTransform()
        {
            _canvas.SetMatrix(_transformStack.Pop());
        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform(Matrix2D)"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Matrix2D matrix, Action execute)
        {
            var topmost = _canvas.TotalMatrix;
            _canvas.SetMatrix(matrix.ToSKMatrix());
            execute();
            _canvas.SetMatrix(topmost);
        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform()"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Action execute)
        {
            var topmost = _canvas.TotalMatrix;
            execute();
            _canvas.SetMatrix(topmost);
        }

        #endregion

        #region Brush

        /// <summary>
        /// Sets the stroke color for this renderer
        /// </summary>
        public void SetStrokeColor(Color color)
        {
            _strokeBrush?.UnloadBrush();
            _strokeBrush = new InternalSolidBrush(color);
        }

        /// <summary>
        /// Sets the stroke brush for this renderer.
        /// </summary>
        public void SetStrokeBrush(IBrush brush)
        {
            if (brush == _strokeBrush)
                return;

            _strokeBrush?.UnloadBrush();
            _strokeBrush = CastBrushOrFail(brush);
        }

        /// <summary>
        /// Sets the fill color for this renderer
        /// </summary>
        public void SetFillColor(Color color)
        {
            _fillBrush?.UnloadBrush();
            _fillBrush = new InternalSolidBrush(color);
        }

        /// <summary>
        /// Sets the fill brush for this renderer.
        /// </summary>
        public void SetFillBrush(IBrush brush)
        {
            if (brush == _fillBrush)
                return;

            _fillBrush?.UnloadBrush();
            _fillBrush = CastBrushOrFail(brush);
        }

        /// <summary>
        /// Creates a linear gradient brush for drawing.
        /// </summary>
        public ILinearGradientBrush CreateLinearGradientBrush([NotNull] IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
        {
            return new InternalLinearBrush(gradientStops, start, end);
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        /// </summary>
        public IBrush CreateBitmapBrush(ImageResource image)
        {
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            return new InternalBitmapBrush(bitmap);
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        /// </summary>
        public IBrush CreateBitmapBrush([NotNull] IManagedImageResource image)
        {
            var bitmap = CastBitmapOrFail(image);
            return new InternalBitmapBrush(bitmap.bitmap);
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

        private static SkiaBitmap CastBitmapOrFail([NotNull] IManagedImageResource bitmap)
        {
            if (bitmap is SkiaBitmap skiaBitmap)
                return skiaBitmap;

            throw new InvalidOperationException($"Expected a bitmap of type {typeof(SkiaBitmap)}");
        }

        private class InternalBrush : IBrush
        {
            internal bool IsLoaded { get; private set; }
            public SKPaint Brush { get; protected set; }

            public virtual void LoadBrush()
            {
                IsLoaded = true;
            }

            public virtual void UnloadBrush()
            {
                if (!IsLoaded)
                    return;

                IsLoaded = false;
                Brush.Dispose();
            }
        }

        private class InternalSolidBrush : InternalBrush, ISolidBrush
        {
            public Color Color { get; }

            public InternalSolidBrush(Color color)
            {
                Color = color;
            }

            public override void LoadBrush()
            {
                if (IsLoaded)
                    return;

                base.LoadBrush();

                Brush = new SKPaint
                {
                    Color = new SKColor(Color.R, Color.G, Color.B, Color.A)
                };
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

            public override void LoadBrush()
            {
                if (IsLoaded)
                    return;

                base.LoadBrush();

                var colors = GradientStops.OrderBy(stop => stop.Position).Select(stop => stop.Color.ToSKColor()).ToArray();
                var positions = GradientStops.OrderBy(stop => stop.Position).Select(stop => stop.Position).ToArray();
                var shader = SKShader.CreateLinearGradient(Start.ToSKPoint(), End.ToSKPoint(), colors, positions, SKShaderTileMode.Clamp);

                Brush = new SKPaint
                {
                    Shader = shader
                };
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
            public SKBitmap Bitmap { get; }

            public InternalBitmapBrush(SKBitmap bitmap)
            {
                Bitmap = bitmap;
            }

            public override void LoadBrush()
            {
                if (IsLoaded)
                    return;

                base.LoadBrush();

                var shader = SKShader.CreateBitmap(Bitmap, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);
                Brush = new SKPaint
                {
                    Shader = shader
                };
            }
        }

        private class InternalPathSink : IPathInputSink
        {
            private readonly SKPath _path;

            public InternalPathSink(SKPath path)
            {
                _path = path;
            }

            public void BeginFigure(Vector location, bool filled)
            {
                _path.MoveTo(location.ToSKPoint());
            }

            public void MoveTo(Vector point)
            {
                _path.MoveTo(point.ToSKPoint());
            }

            public void LineTo(Vector point)
            {
                _path.LineTo(point.ToSKPoint());
            }

            public void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint)
            {
                _path.CubicTo(anchor1.ToSKPoint(), anchor2.ToSKPoint(), endPoint.ToSKPoint());
            }

            public void AddRectangle(AABB rectangle)
            {
                _path.AddRect(rectangle.ToSKRect());
            }

            public void EndFigure(bool closePath)
            {
                _path.Close();
            }
        }

        private class InternalPathGeometry : IPathGeometry
        {
            public SKPath PathGeometry { get; }

            public InternalPathGeometry(SKPath pathGeometry)
            {
                PathGeometry = pathGeometry;
            }

            public void Dispose()
            {
                PathGeometry.Dispose();
            }
        }
    }
}

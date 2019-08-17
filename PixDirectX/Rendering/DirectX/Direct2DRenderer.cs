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
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Brush = SharpDX.Direct2D1.Brush;

namespace PixDirectX.Rendering.DirectX
{
    public sealed class Direct2DRenderer : IRenderer, IDisposable
    {
        private InternalBrush _strokeBrush;
        private InternalBrush _fillBrush;

        private readonly IDirect2DRenderingState _state;
        private readonly D2DImageResources _imageResource;

        public Matrix2D Transform
        {
            get => _state.Transform.ToMatrix2D();
            set => _state.Transform = value.ToRawMatrix3X2();
        }

        public Direct2DRenderer([NotNull] IDirect2DRenderingState state, [NotNull] D2DImageResources imageResource)
        {
            _state = state;
            _imageResource = imageResource;
            _strokeBrush = new InternalSolidBrush(Color.Black);
            _fillBrush = new InternalSolidBrush(Color.Black);
        }

        public void Dispose()
        {
            _strokeBrush?.UnloadBrush();
            _fillBrush?.UnloadBrush();
        }

        private Brush BrushForStroke()
        {
            _strokeBrush.LoadBrush(_state.D2DRenderTarget);
            return _strokeBrush.Brush;
        }

        private Brush BrushForFill()
        {
            _fillBrush.LoadBrush(_state.D2DRenderTarget);
            return _fillBrush.Brush;
        }

        #region Stroke

        public void StrokeLine(Vector start, Vector end, float strokeWidth = 1)
        {
            _state.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), BrushForStroke(), strokeWidth);
        }

        public void StrokeCircle(Vector center, float radius, float strokeWidth = 1)
        {
            StrokeEllipse(new AABB(center - new Vector(radius) * 2, center + new Vector(radius) * 2), strokeWidth);
        }

        public void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1)
        {
            var ellipse = new Ellipse(ellipseArea.Center.ToRawVector2(), ellipseArea.Width / 2, ellipseArea.Height / 2);

            _state.D2DRenderTarget.DrawEllipse(ellipse, BrushForStroke(), strokeWidth);
        }

        public void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1)
        {
            StrokeArea(new AABB(rectangle), strokeWidth);
        }

        public void StrokeArea(AABB area, float strokeWidth = 1)
        {
            _state.D2DRenderTarget.DrawRectangle(area.ToRawRectangleF(), BrushForStroke(), strokeWidth);
        }

        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1)
        {
            var roundedRect = new RoundedRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Rect = area.ToRawRectangleF()
            };

            _state.D2DRenderTarget.DrawRoundedRectangle(roundedRect, BrushForStroke(), strokeWidth);
        }

        public void StrokeGeometry(PolyGeometry geometry, float strokeWidth = 1)
        {
            using (var geom = new PathGeometry(_state.D2DFactory))
            {
                foreach (var polygon in geometry.Polygons())
                {
                    var sink = geom.Open();
                    sink.BeginFigure(polygon[0].ToRawVector2(), FigureBegin.Filled);
                    foreach (var vector in polygon.Skip(1))
                    {
                        sink.AddLine(vector.ToRawVector2());
                    }
                    sink.Close();
                }

                _state.D2DRenderTarget.DrawGeometry(geom, BrushForStroke(), strokeWidth);
            }
        }

        public void StrokePath(IPathGeometry path, float strokeWidth = 1)
        {
            var pathGeom = CastPathOrFail(path);

            _state.D2DRenderTarget.DrawGeometry(pathGeom.PathGeometry, BrushForStroke(), strokeWidth);
        }

        #endregion

        #region Fill

        public void FillCircle(Vector center, float radius)
        {
            FillEllipse(new AABB(center - new Vector(radius) * 2, center + new Vector(radius) * 2));
        }

        public void FillEllipse(AABB ellipseArea)
        {
            var ellipse = new Ellipse(ellipseArea.Center.ToRawVector2(), ellipseArea.Width / 2, ellipseArea.Height / 2);

            _state.D2DRenderTarget.FillEllipse(ellipse, BrushForFill());
        }

        public void FillRectangle(RectangleF rectangle)
        {
            FillArea(new AABB(rectangle));
        }

        public void FillArea(AABB area)
        {
            _state.D2DRenderTarget.FillRectangle(area.ToRawRectangleF(), BrushForFill());
        }
        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {
            var roundedRect = new RoundedRectangle
            {
                RadiusX = radiusX,
                RadiusY = radiusY,
                Rect = area.ToRawRectangleF()
            };

            _state.D2DRenderTarget.FillRoundedRectangle(roundedRect, BrushForFill());
        }

        public void FillGeometry(PolyGeometry geometry)
        {
            using (var geom = new PathGeometry(_state.D2DFactory))
            {
                var sink = geom.Open();

                foreach (var polygon in geometry.Polygons())
                {
                    sink.BeginFigure(polygon[0].ToRawVector2(), FigureBegin.Filled);
                    foreach (var vector in polygon.Skip(1))
                    {
                        sink.AddLine(vector.ToRawVector2());
                    }
                    sink.EndFigure(FigureEnd.Closed);
                }

                sink.Close();

                _state.D2DRenderTarget.FillGeometry(geom, BrushForFill());
            }
        }

        public void FillPath(IPathGeometry path)
        {
            var pathGeom = CastPathOrFail(path);

            _state.D2DRenderTarget.FillGeometry(pathGeom.PathGeometry, BrushForFill());
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
            var geom = new PathGeometry(_state.D2DFactory);

            var sink = geom.Open();

            var pathSink = new InternalPathSink(sink, FigureBegin.Filled);
            execute(pathSink);

            sink.Close();

            return new InternalPathGeometry(geom);
        }

        #endregion

        #region Bitmap

        public void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            var bitmap = _imageResource.BitmapForResource(image);
            Contract.Assert(bitmap != null, $"No bitmap found for image resource {image}. Make sure the bitmap is pre-loaded before using it.");

            DrawBitmap(bitmap, region, opacity, interpolationMode, tintColor);
        }

        public void DrawBitmap(ImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode, tintColor);
        }

        public void DrawBitmap(IManagedImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            var bitmap = CastBitmapOrFail(image);

            DrawBitmap(bitmap.bitmap, region, opacity, interpolationMode, tintColor);
        }

        public void DrawBitmap(IManagedImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode, tintColor);
        }

        private void DrawBitmap(SharpDX.Direct2D1.Bitmap bitmap, RectangleF region, float opacity, ImageInterpolationMode interpolationMode, Color? tintColor = null)
        {
            if (tintColor == null)
            {
                _state.D2DRenderTarget.DrawBitmap(bitmap, ((AABB)region).ToRawRectangleF(), opacity, ToBitmapInterpolation(interpolationMode));
            }
            else
            {
                using (var context = _state.D2DRenderTarget.QueryInterface<DeviceContext>())
                using (var effect = new Effect(context, Effect.Tint))
                {
                    effect.SetInput(0, bitmap, true);
                    effect.SetValue(0, (RawColor4) tintColor.Value.ToColor4());
                    effect.SetValue(1, true);

                    context.DrawImage(effect, ((AABB) region).Minimum.ToRawVector2(), ToInterpolation(interpolationMode));
                }
            }
        }

        private static BitmapInterpolationMode ToBitmapInterpolation(ImageInterpolationMode imageInterpolationMode, Color? tintColor = null)
        {
            switch (imageInterpolationMode)
            {
                case ImageInterpolationMode.Linear:
                    return BitmapInterpolationMode.Linear;
                case ImageInterpolationMode.NearestNeighbor:
                    return BitmapInterpolationMode.NearestNeighbor;
                default:
                    return BitmapInterpolationMode.Linear;
            }
        }
        private static InterpolationMode ToInterpolation(ImageInterpolationMode imageInterpolationMode, Color? tintColor = null)
        {
            switch (imageInterpolationMode)
            {
                case ImageInterpolationMode.Linear:
                    return InterpolationMode.Linear;
                case ImageInterpolationMode.NearestNeighbor:
                    return InterpolationMode.NearestNeighbor;
                default:
                    return InterpolationMode.Linear;
            }
        }

        #endregion

        #region Clipping

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        public void PushClippingArea(AABB area)
        {
            _state.D2DRenderTarget.PushAxisAlignedClip(area.ToRawRectangleF(), AntialiasMode.Aliased);
        }

        /// <summary>
        /// Pops the most recently pushed clipping area.
        /// </summary>
        public void PopClippingArea()
        {
            _state.D2DRenderTarget.PopAxisAlignedClip();
        }

        #endregion

        #region Transformation

        /// <summary>
        /// Pushes an Identity transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform()
        {
            _state.PushMatrix();
        }

        /// <summary>
        /// Pushes a 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform(Matrix2D matrix)
        {
            _state.PushMatrix(matrix.ToRawMatrix3X2());
        }

        /// <summary>
        /// Pops the top-most active transformation matrix.
        /// </summary>
        public void PopTransform()
        {
            _state.PopMatrix();
        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform(Matrix2D)"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Matrix2D matrix, Action execute)
        {
            PushTransform(matrix);
            execute();
            PopTransform();
        }

        /// <summary>
        /// Runs a closure between a pair of <see cref="PushTransform()"/>/<see cref="PopTransform"/> invocations.
        /// </summary>
        public void PushingTransform(Action execute)
        {
            PushTransform();
            execute();
            PopTransform();
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
        public ILinearGradientBrush CreateLinearGradientBrush(IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
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
        public IBrush CreateBitmapBrush(IManagedImageResource image)
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

        private static DirectXBitmap CastBitmapOrFail([NotNull] IManagedImageResource bitmap)
        {
            if (bitmap is DirectXBitmap dxBitmap)
                return dxBitmap;

            throw new InvalidOperationException($"Expected a bitmap of type {typeof(DirectXBitmap)}");
        }

        private class InternalBrush : IBrush
        {
            internal bool IsLoaded { get; private set; }
            public Brush Brush { get; protected set; }

            public virtual void LoadBrush(RenderTarget renderTarget)
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

            public override void LoadBrush(RenderTarget renderTarget)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(renderTarget);

                Brush = new SolidColorBrush(renderTarget, Color.ToColor4());
            }
        }

        private class InternalLinearBrush : InternalBrush, ILinearGradientBrush
        {
            private GradientStopCollection _stopCollection;
            public IReadOnlyList<PixGradientStop> GradientStops { get; }
            public Vector Start { get; }
            public Vector End { get; }

            public InternalLinearBrush([NotNull] IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
            {
                GradientStops = gradientStops;
                Start = start;
                End = end;
            }

            public override void LoadBrush(RenderTarget renderTarget)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(renderTarget);

                var stops = new GradientStopCollection(renderTarget, GradientStops.Select(ToGradientStop).ToArray());
                var properties = new LinearGradientBrushProperties
                {
                    StartPoint = Start.ToRawVector2(),
                    EndPoint = End.ToRawVector2()
                };

                Brush = new LinearGradientBrush(renderTarget, properties, stops);
                _stopCollection = stops;
            }

            public override void UnloadBrush()
            {
                if (!IsLoaded)
                    return;

                base.UnloadBrush();

                _stopCollection.Dispose();
            }

            private static GradientStop ToGradientStop(PixGradientStop stop)
            {
                return new GradientStop
                {
                    Color = stop.Color.ToColor4(),
                    Position = stop.Position
                };
            }
        }

        private class InternalBitmapBrush : InternalBrush
        {
            public SharpDX.Direct2D1.Bitmap Bitmap { get; }

            public InternalBitmapBrush(SharpDX.Direct2D1.Bitmap bitmap)
            {
                Bitmap = bitmap;
            }

            public override void LoadBrush(RenderTarget renderTarget)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(renderTarget);

                var brush = new BitmapBrush(renderTarget, Bitmap)
                {
                    ExtendModeX = ExtendMode.Wrap,
                    ExtendModeY = ExtendMode.Wrap
                };
                Brush = brush;
            }
        }

        private class InternalPathSink : IPathInputSink
        {
            private readonly GeometrySink _geometrySink;
            private bool _startOfFigure = true;
            private Vector _startLocation;
            private readonly FigureBegin _figureBegin;

            public InternalPathSink(GeometrySink geometrySink, FigureBegin figureBegin)
            {
                _geometrySink = geometrySink;
                _figureBegin = figureBegin;
            }

            public void BeginFigure(Vector location, bool filled)
            {
                _startOfFigure = false;
                _geometrySink.BeginFigure(location.ToRawVector2(), filled ? FigureBegin.Filled : FigureBegin.Hollow);
                _startLocation = location;
            }

            public void MoveTo(Vector point)
            {
                if (!_startOfFigure)
                    _geometrySink.EndFigure(FigureEnd.Open);

                _startLocation = point;
                _startOfFigure = true;
            }

            public void LineTo(Vector point)
            {
                EnsureBeginFigure();

                _geometrySink.AddLine(point.ToRawVector2());
                _startLocation = point;
            }

            public void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint)
            {
                EnsureBeginFigure();

                _geometrySink.AddBezier(new BezierSegment
                {
                    Point1 = anchor1.ToRawVector2(),
                    Point2 = anchor2.ToRawVector2(),
                    Point3 = endPoint.ToRawVector2(),
                });

                _startLocation = endPoint;
            }

            public void AddRectangle(AABB rectangle)
            {
                _geometrySink.AddLine(new Vector(rectangle.Right, rectangle.Top).ToRawVector2());
                _geometrySink.AddLine(new Vector(rectangle.Right, rectangle.Bottom).ToRawVector2());
                _geometrySink.AddLine(new Vector(rectangle.Left, rectangle.Bottom).ToRawVector2());
            }

            public void EndFigure(bool closePath)
            {
                EndFigure(closePath ? FigureEnd.Closed : FigureEnd.Open);
            }

            private void EnsureBeginFigure()
            {
                if (!_startOfFigure)
                    return;

                _geometrySink.BeginFigure(_startLocation.ToRawVector2(), _figureBegin);
                _startOfFigure = false;
            }

            private void EndFigure(FigureEnd end)
            {
                if (_startOfFigure)
                    return;

                _geometrySink.EndFigure(end);
                _startOfFigure = true;
            }
        }

        private class InternalPathGeometry : IPathGeometry
        {
            public PathGeometry PathGeometry { get; }

            public InternalPathGeometry(PathGeometry pathGeometry)
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
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
using System.Linq;
using Blend2DCS;
using Blend2DCS.Geometry;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Utils;
using PixRendering;

namespace PixDirectX.Rendering.Blend2D
{
    public class Blend2DRenderer: IRenderer
    {
        private readonly Stack<AABB> _clipStack = new Stack<AABB>();
        private readonly Stack<Matrix2D> _transformStack = new Stack<Matrix2D>();

        private InternalBrush _strokeBrush;
        private InternalBrush _fillBrush;

        private readonly Blend2DImageResources _imageResources;
        private readonly BLContext _context;
        private readonly Blend2DFontManager _fontManager = new Blend2DFontManager();

        /// <summary>
        /// Gets or sets the topmost active transformation matrix.
        /// </summary>
        public Matrix2D Transform 
        {
            get => _context.UserMatrix.ToMatrix2D();
            set => _context.SetMatrix(value.ToBLMatrix2D());
        }

        public Blend2DRenderer(BLContext context, Blend2DImageResources imageResources)
        {
            _context = context;
            _imageResources = imageResources;
        }

        private void SetBrushForStroke(float strokeWidth)
        {
            _context.SetStrokeWidth(strokeWidth);
            _strokeBrush.LoadBrush(_context, BrushKind.Stroke);
        }

        private void SetBrushForFill()
        {
            _fillBrush.LoadBrush(_context, BrushKind.Fill);
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
            _context.StrokeEllipse(new BLEllipse(ellipseArea.Left + ellipseArea.Width / 2, ellipseArea.Top + ellipseArea.Height / 2, ellipseArea.Width / 2, ellipseArea.Height / 2));
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
            _context.FillCircle(new BLCircle(center.X, center.Y, radius));
        }

        /// <summary>
        /// Fills the area of an ellipse with the current fill brush.
        /// </summary>
        public void FillEllipse(AABB ellipseArea)
        {
            _context.FillEllipse(new BLEllipse(ellipseArea.Left + ellipseArea.Width / 2, ellipseArea.Top + ellipseArea.Height / 2, ellipseArea.Width / 2, ellipseArea.Height / 2));
        }

        /// <summary>
        /// Fills the area of a rectangle with the current fill brush.
        /// </summary>
        public void FillRectangle(RectangleF rectangle)
        {
            _context.FillRectangle(new BLRect(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height));
        }

        /// <summary>
        /// Fills an <see cref="AABB"/>-bounded area with the current fill brush.
        /// </summary>
        public void FillArea(AABB area)
        {
            _context.FillRectangle(area.ToBLRect());
        }

        /// <summary>
        /// Fills the outline of an <see cref="AABB"/>-bounded area with rounded corners with the current fill brush.
        /// </summary>
        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {
            var rect = new BLRoundRect(area.Left, area.Top, area.Width, area.Height, radiusX, radiusY);
            _context.FillRoundRectangle(rect);
        }

        /// <summary>
        /// Fills a geometrical object with the current fill brush.
        /// </summary>
        public void FillGeometry(PolyGeometry geometry)
        {
            foreach (var polygon in geometry.Polygons())
            {
                var path = new BLPath();
                path.MoveTo(polygon[0].X, polygon[0].Y);
                foreach (var vector in polygon.Skip(1))
                {
                    path.LineTo(vector.X, vector.Y);
                }
                path.LineTo(polygon[0].X, polygon[0].Y);

                _context.FillPath(path);
            }
        }

        /// <summary>
        /// Fills a path geometry with the current fill brush.
        /// </summary>
        public void FillPath(IPathGeometry path)
        {
            var internalPath = CastPathOrFail(path);

            _context.FillPath(internalPath.Path);
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
            var path = new BLPath();
            var pathSink = new InternalPathSink(path);
            execute(pathSink);

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
            AdjustInterpolationMode(interpolationMode);

            DrawBitmap(image, (AABB)region, opacity, interpolationMode, tintColor);
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
            AdjustInterpolationMode(interpolationMode);

            var bitmap = _imageResources.BitmapForResource(image) ??
                         throw new InvalidOperationException($"No image found for resource ${image.ResourceName}");

            _context.BlitImage(bitmap, new BLRectI(0, 0, bitmap.Width, bitmap.Height), region.ToBLRect());
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
            AdjustInterpolationMode(interpolationMode);

            DrawBitmap(image, (AABB)region, opacity, interpolationMode, tintColor);
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
            AdjustInterpolationMode(interpolationMode);

            var bitmap = CastBitmapOrFail(image);

            _context.BlitImage(bitmap.Bitmap, new BLRectI(0, 0, bitmap.Width, bitmap.Height), region.ToBLRect());
        }

        private void AdjustInterpolationMode(ImageInterpolationMode interpolationMode)
        {
            switch (interpolationMode)
            {
                case ImageInterpolationMode.NearestNeighbor:
                    _context.SetPatternQuality(BLPatternQuality.Nearest);
                    break;
                case ImageInterpolationMode.Linear:
                    _context.SetPatternQuality(BLPatternQuality.Bilinear);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(interpolationMode), interpolationMode, null);
            }
        }

        #endregion

        #region Clipping

        private AABB? ComputeClipStack()
        {
            if (_clipStack.Count == 0)
                return null;

            return _clipStack.Aggregate(_clipStack.Peek(), (aabb, aabb1) => aabb.Intersect(aabb1));
        }

        private void ApplyClipStack()
        {
            _context.RestoreClipping();

            var clip = ComputeClipStack();

            if (clip != null)
            {
                _context.ClipToRect(clip.Value.ToBLRect());
            }
        }

        /// <summary>
        /// Pushes a clipping area where all further drawing operations will be constrained into.
        /// </summary>
        public void PushClippingArea(AABB area)
        {
            _clipStack.Push(area);
            ApplyClipStack();
        }

        /// <summary>
        /// Pops the most recently pushed clipping area.
        /// </summary>
        public void PopClippingArea()
        {
            _clipStack.Pop();
            ApplyClipStack();
        }

        #endregion

        #region Transformation

        private Matrix2D? ComputeTransformStack()
        {
            if (_transformStack.Count == 0)
                return null;

            return _transformStack.Aggregate(Matrix2D.Identity, (aabb, aabb1) => aabb * aabb1);
        }

        private void ApplyTransformStack()
        {
            _context.SetMatrix(BLMatrix2D.Identity());

            var matrix = ComputeTransformStack();

            if (matrix != null)
            {
                _context.SetMatrix(matrix.Value.ToBLMatrix2D());
            }
        }

        /// <summary>
        /// Pushes an Identity 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform()
        {
            PushTransform(Matrix2D.Identity);
        }

        /// <summary>
        /// Pushes a 2D transformation matrix on top of the currently active transform matrix.
        /// </summary>
        public void PushTransform(Matrix2D matrix)
        {
            _transformStack.Push(matrix);
            ApplyTransformStack();
        }

        /// <summary>
        /// Pops the top-most active transformation matrix.
        /// </summary>
        public void PopTransform()
        {
            _transformStack.Pop();
            ApplyTransformStack();
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
        /// Sets the stroke color for this renderer.
        /// </summary>
        public void SetStrokeColor(Color color)
        {
            SetStrokeBrush(new InternalSolidBrush(color));
        }

        /// <summary>
        /// Sets the stroke brush for this renderer.
        /// </summary>
        public void SetStrokeBrush(IBrush brush)
        {
            _strokeBrush?.UnloadBrush();
            _strokeBrush = CastBrushOrFail(brush);
        }

        /// <summary>
        /// Sets the fill color for this renderer.
        /// </summary>
        public void SetFillColor(Color color)
        {
            SetFillBrush(new InternalSolidBrush(color));
        }

        /// <summary>
        /// Sets the fill brush for this renderer.
        /// </summary>
        public void SetFillBrush(IBrush brush)
        {
            _fillBrush?.UnloadBrush();
            _fillBrush = CastBrushOrFail(brush);
            SetBrushForFill();
        }

        /// <summary>
        /// Creates a linear gradient brush for drawing.
        /// </summary>
        public ILinearGradientBrush CreateLinearGradientBrush(IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
        {
            var brush = new InternalLinearBrush(gradientStops, start, end);
            return brush;
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        ///
        /// Bitmap brushes by default tile the image contents when drawn within bounds that exceed that of the original image's size.
        /// </summary>
        public IBrush CreateBitmapBrush(ImageResource image)
        {
            var bitmap = _imageResources.BitmapForResource(image) ??
                         throw new InvalidOperationException($"No image found for resource ${image.ResourceName}");

            return new InternalBitmapBrush(bitmap);
        }

        /// <summary>
        /// Creates a bitmap brush from a given image.
        ///
        /// Bitmap brushes by default tile the image contents when drawn within bounds that exceed that of the original image's size.
        /// </summary>
        public IBrush CreateBitmapBrush(IManagedImageResource image)
        {
            var bitmap = CastBitmapOrFail(image);
            return new InternalBitmapBrush(bitmap.Bitmap);
        }

        #endregion

        #region Font and Text

        public IFontManager GetFontManager()
        {
            return _fontManager;
        }

        public void DrawText(string text, IFont font, AABB area)
        {
            var castFont = font as Blend2DFont ?? throw new ArgumentException($"Expected font of type {typeof(Blend2DFont)}");

            _context.FillText(area.Minimum.ToBLPoint(), castFont.Font, text);
        }

        public void DrawAttributedText(IAttributedText text, TextFormatAttributes attributes, AABB area)
        {
            var font = (Blend2DFont)GetFontManager().DefaultFont(attributes.FontSize);

            _context.FillText(area.Minimum.ToBLPoint(), font.Font, text.String);
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

            public virtual void LoadBrush(BLContext context, BrushKind kind)
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

            public override void LoadBrush(BLContext context, BrushKind kind)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(context, kind);

                switch (kind)
                {
                    case BrushKind.Stroke:
                        context.SetStrokeStyle(unchecked((uint)Color.ToArgb()));
                        break;
                    case BrushKind.Fill:
                        context.SetFillStyle(unchecked((uint)Color.ToArgb()));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }
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

            public override void LoadBrush(BLContext context, BrushKind kind)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(context, kind);

                var gradient = BLGradient.Linear(Start.ToBLPoint(), End.ToBLPoint());
                
                foreach (var stop in GradientStops)
                {
                    gradient.AddStop(stop.Position, unchecked((uint)stop.Color.ToArgb()));
                }

                switch (kind)
                {
                    case BrushKind.Stroke:
                        context.SetStrokeStyle(gradient);
                        break;
                    case BrushKind.Fill:
                        context.SetFillStyle(gradient);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }
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

            public override void LoadBrush(BLContext context, BrushKind kind)
            {
                if (IsLoaded)
                    return;

                base.LoadBrush(context, kind);

                switch (kind)
                {
                    case BrushKind.Stroke:
                        context.SetStrokeStyle(new BLPattern(Bitmap));
                        break;
                    case BrushKind.Fill:
                        context.SetFillStyle(new BLPattern(Bitmap));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }
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
                Path.Dispose();
            }
        }

        private enum BrushKind
        {
            Stroke,
            Fill
        }
    }
}

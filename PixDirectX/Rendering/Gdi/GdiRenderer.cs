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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Utils;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiRenderer: IRenderer
    {
        private InternalBrush _strokeBrush;
        private InternalBrush _fillBrush;
        private Pen _strokePen;
        private readonly Graphics _graphics;
        private readonly GdiImageResourceManager _imageResourceManager;
        private readonly Stack<Region> _clippings = new Stack<Region>();
        private readonly Stack<Matrix> _transformStack = new Stack<Matrix>();

        public Matrix2D Transform
        {
            get => _graphics.Transform.ToMatrix2D();
            set => _graphics.Transform = value.ToMatrix();
        }

        public GdiRenderer(Graphics graphics, GdiImageResourceManager imageResourceManager)
        {
            _graphics = graphics;
            _imageResourceManager = imageResourceManager;
            _strokeBrush = new InternalSolidBrush(Color.Black);
            _fillBrush = new InternalSolidBrush(Color.White);
        }

        private Brush BrushForFill()
        {
            _fillBrush.LoadBrush();
            return _fillBrush.Brush;
        }

        private Pen PenForStroke(float penWidth)
        {
            _strokeBrush.LoadBrush();
            _strokePen = new Pen(_strokeBrush.Brush) { Width = penWidth };
            return _strokePen;
        }

        #region Stroke

        public void StrokeLine(Vector start, Vector end, float strokeWidth = 1)
        {
            _graphics.DrawLine(PenForStroke(strokeWidth), start, end);
        }

        public void StrokeCircle(Vector center, float radius, float strokeWidth = 1)
        {
            StrokeEllipse(new AABB(center - new Vector(radius) * 2, center + new Vector(radius) * 2), strokeWidth);
        }

        public void StrokeEllipse(AABB ellipseArea, float strokeWidth = 1)
        {
            _graphics.DrawEllipse(PenForStroke(strokeWidth), (RectangleF)ellipseArea);
        }

        public void StrokeRectangle(RectangleF rectangle, float strokeWidth = 1)
        {
            _graphics.DrawRectangle(PenForStroke(strokeWidth), rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public void StrokeArea(AABB area, float strokeWidth = 1)
        {
            StrokeRectangle((RectangleF) area, strokeWidth);
        }

        public void StrokeRoundedArea(AABB area, float radiusX, float radiusY, float strokeWidth = 1)
        {
            var gp = CreateRoundedRectPath(area, radiusX, radiusY);

            _graphics.DrawPath(PenForStroke(strokeWidth), gp);
        }

        public void StrokeGeometry(PolyGeometry geometry, float strokeWidth = 1)
        {
            var polys = geometry.Polygons();
            foreach (var poly in polys)
            {
                _graphics.DrawPolygon(PenForStroke(strokeWidth), poly.Select(v => (PointF)v).ToArray());
            }
        }

        public void StrokePath(IPathGeometry path, float strokeWidth = 1)
        {
            var internalPath = CastPathOrFail(path);
            
            _graphics.DrawPath(PenForStroke(strokeWidth), internalPath.GraphicsPath);
        }

        #endregion

        #region Fill

        public void FillCircle(Vector center, float radius)
        {
            FillEllipse(new AABB(center - new Vector(radius) * 2, center + new Vector(radius) * 2));
        }

        public void FillEllipse(AABB ellipseArea)
        {
            _graphics.FillEllipse(BrushForFill(), (RectangleF)ellipseArea);
        }

        public void FillRectangle(RectangleF rectangle)
        {
            _graphics.FillRectangle(BrushForFill(), rectangle);
        }

        public void FillArea(AABB area)
        {
            FillRectangle((RectangleF)area);
        }

        public void FillRoundedArea(AABB area, float radiusX, float radiusY)
        {
            var gp = CreateRoundedRectPath(area, radiusX, radiusY);

            _graphics.FillPath(BrushForFill(), gp);
        }

        public void FillGeometry(PolyGeometry geometry)
        {
            var polys = geometry.Polygons();
            foreach (var poly in polys)
            {
                _graphics.FillPolygon(BrushForFill(), poly.Select(v => (PointF)v).ToArray());
            }
        }

        public void FillPath(IPathGeometry path)
        {
            var internalPath = CastPathOrFail(path);

            _graphics.FillPath(BrushForFill(), internalPath.GraphicsPath);
        }

        #endregion

        #region Path Geometry

        public IPathGeometry CreatePath(Action<IPathInputSink> execute)
        {
            var graphicsPath = new GraphicsPath();
            var inputSink = new GraphicsPathInputSink(graphicsPath);
            execute(inputSink);

            return new InternalPathGeometry(graphicsPath);
        }

        #endregion

        #region Bitmap

        public void DrawBitmap(ImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode)
        {
            var bitmap = _imageResourceManager.BitmapForResource(image) ??
                         throw new InvalidOperationException($"No image found for resource ${image.ResourceName}");

            if (opacity >= 1)
            {
                _graphics.DrawImage(bitmap, region);
            }
            else
            {
                var opacityMatrix = new ColorMatrix
                {
                    Matrix33 = opacity
                };

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(opacityMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                _graphics.DrawImage(bitmap, ((AABB) region).Corners.Select(c => (PointF) c).ToArray(),
                    new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel, attributes);
            }
        }

        public void DrawBitmap(ImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode)
        {
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode);
        }

        public void DrawBitmap(IManagedImageResource image, RectangleF region, float opacity, ImageInterpolationMode interpolationMode)
        {
            var bitmap = CastBitmapOrFail(image);

            if (opacity >= 1)
            {
                _graphics.DrawImage(bitmap.bitmap, region);
            }
            else
            {
                var opacityMatrix = new ColorMatrix
                {
                    Matrix33 = opacity
                };

                var attributes = new ImageAttributes();
                attributes.SetColorMatrix(opacityMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                _graphics.DrawImage(bitmap.bitmap, ((AABB)region).Corners.Select(c => (PointF)c).ToArray(),
                    new RectangleF(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel, attributes);
            }
        }

        public void DrawBitmap(IManagedImageResource image, AABB region, float opacity, ImageInterpolationMode interpolationMode)
        {
            DrawBitmap(image, (RectangleF)region, opacity, interpolationMode);
        }

        #endregion

        #region Clipping

        public void PushClippingArea(AABB area)
        {
            _clippings.Push(_graphics.Clip.Clone());

            _graphics.IntersectClip((RectangleF)area);
        }

        public void PopClippingArea()
        {
            _graphics.Clip = _clippings.Pop();
        }

        #endregion

        #region Transformation

        public void PushTransform()
        {
            _transformStack.Push(_graphics.Transform);
        }

        public void PushTransform(Matrix2D matrix)
        {
            _transformStack.Push(_graphics.Transform.Clone());
            
            _graphics.MultiplyTransform(matrix.ToMatrix());
        }

        public void PopTransform()
        {
            _graphics.Transform = _transformStack.Pop();
        }

        public void PushingTransform(Matrix2D matrix, Action execute)
        {
            PushTransform(matrix);
            execute();
            PopTransform();
        }

        public void PushingTransform(Action execute)
        {
            PushTransform();
            execute();
            PopTransform();
        }

        #endregion

        #region Brush

        public void SetStrokeColor(Color color)
        {
            _strokeBrush?.UnloadBrush();
            _strokeBrush = new InternalSolidBrush(color);
        }

        public void SetStrokeBrush(IBrush brush)
        {
            var internalBrush = CastBrushOrFail(brush);
            _strokeBrush?.UnloadBrush();
            _strokeBrush = internalBrush;
        }

        public void SetFillColor(Color color)
        {
            _fillBrush?.UnloadBrush();
            _fillBrush = new InternalSolidBrush(color);
        }

        public void SetFillBrush(IBrush brush)
        {
            var internalBrush = CastBrushOrFail(brush);
            _fillBrush?.UnloadBrush();
            _fillBrush = internalBrush;
        }

        public ILinearGradientBrush CreateLinearGradientBrush(IReadOnlyList<PixGradientStop> gradientStops, Vector start, Vector end)
        {
            return new InternalLinearBrush(gradientStops, start, end);
        }

        #endregion

        private static InternalPathGeometry CastPathOrFail([NotNull] IPathGeometry path)
        {
            if (path is InternalPathGeometry internalPath)
                return internalPath;

            throw new InvalidOperationException($"Expected a path geometry of type {typeof(InternalPathGeometry)}");
        }

        private static GdiImageResourceManager.ManagedBitmap CastBitmapOrFail([NotNull] IManagedImageResource image)
        {
            if (image is GdiImageResourceManager.ManagedBitmap bitmap)
                return bitmap;

            throw new InvalidOperationException($"Expected a bitmap of type {typeof(GdiImageResourceManager.ManagedBitmap)}");
        }

        private static InternalBrush CastBrushOrFail([NotNull] IBrush brush)
        {
            if (brush is InternalBrush internalBrush)
                return internalBrush;

            throw new InvalidOperationException($"Expected a brush of type {typeof(InternalBrush)}");
        }

        private static GraphicsPath CreateRoundedRectPath(AABB area, float radiusX, float radiusY)
        {
            var gp = new GraphicsPath();

            gp.AddArc(area.Left, area.Top, radiusX, radiusY, 180, 90);
            gp.AddArc(area.Left + area.Width - radiusX, area.Top, radiusY, radiusY, 270, 90);
            gp.AddArc(area.Left + area.Width - radiusX, area.Top + area.Height - radiusY, radiusY, radiusY, 0, 90);
            gp.AddArc(area.Left, area.Top + area.Height - radiusY, radiusY, radiusY, 90, 90);
            gp.AddLine(area.Left, area.Top + area.Height - radiusY, area.Left, area.Top + radiusY / 2);
            return gp;
        }

        private class InternalBrush : IBrush
        {
            internal bool IsLoaded { get; private set; }
            public Brush Brush { get; protected set; }

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

                Brush = new SolidBrush(Color);
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

                var brush = new LinearGradientBrush(Start, End, Color.Black, Color.White);

                var colorBlend = new ColorBlend(GradientStops.Count)
                {
                    Colors = GradientStops.Select(s => s.Color).ToArray(),
                    Positions = GradientStops.Select(s => s.Position).ToArray()
                };
                FixUpColorBlend(colorBlend);

                brush.InterpolationColors = colorBlend;

                Brush = brush;
            }

            static void FixUpColorBlend([NotNull] ColorBlend colorBlend)
            {
                if (colorBlend.Positions.Length == 0 || colorBlend.Colors.Length == 0)
                    return;

                if (!colorBlend.Positions.Contains(0))
                {
                    var leastColor = colorBlend.Colors.Zip(colorBlend.Positions, (color, f) => (color, f)).OrderBy(c => c.f).Select(c => c.color).First();

                    colorBlend.Positions = new [] { 0.0f }.Concat(colorBlend.Positions).ToArray();
                    colorBlend.Colors = new [] { leastColor }.Concat(colorBlend.Colors).ToArray();
                }

                if (!colorBlend.Positions.Contains(1))
                {
                    var greatestColor = colorBlend.Colors.Zip(colorBlend.Positions, (color, f) => (color, f)).OrderByDescending(c => c.f).Select(c => c.color).First();

                    colorBlend.Positions = colorBlend.Positions.Concat(new[] { 1.0f }).ToArray();
                    colorBlend.Colors = colorBlend.Colors.Concat(new[] { greatestColor }).ToArray();
                }
            }
        }

        private class GraphicsPathInputSink : IPathInputSink
        {
            private readonly GraphicsPath _graphicsPath;
            private Vector _point;

            public GraphicsPathInputSink(GraphicsPath graphicsPath)
            {
                _graphicsPath = graphicsPath;
            }

            public void BeginFigure(Vector location, bool filled)
            {
                _graphicsPath.StartFigure();
            }

            public void MoveTo(Vector point)
            {
                _point = point;
            }

            public void LineTo(Vector point)
            {
                _graphicsPath.AddLine(_point, point);
                _point = point;
            }

            public void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint)
            {
                _graphicsPath.AddBezier(_point, anchor1, anchor2, endPoint);
                _point = endPoint;
            }

            public void AddRectangle(AABB rectangle)
            {
                _graphicsPath.AddRectangle((Rectangle)rectangle);
                _point = rectangle.Minimum;
            }

            public void EndFigure(bool closePath)
            {
                if (closePath)
                    _graphicsPath.CloseFigure();
            }
        }

        private class InternalPathGeometry : IPathGeometry
        {
            public GraphicsPath GraphicsPath { get; }

            public InternalPathGeometry(GraphicsPath graphicsPath)
            {
                GraphicsPath = graphicsPath;
            }

            public void Dispose()
            {
                GraphicsPath?.Dispose();
            }
        }
    }
}

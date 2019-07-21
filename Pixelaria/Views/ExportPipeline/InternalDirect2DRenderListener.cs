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
using FastBitmapLib;
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Bitmap = System.Drawing.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;
using TextRange = SharpDX.DirectWrite.TextRange;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Internal render listener for rendering pipeline steps
    /// </summary>
    internal class InternalDirect2DRenderListener : IRenderListener, IRenderingDecoratorContainer
    {
        public int RenderOrder { get; } = RenderOrdering.PipelineView;

        private readonly IPipelineContainer _container; // For relative position calculations
        private readonly IExportPipelineControl _control;

        /// <summary>
        /// A small 32x32 box used to draw shadow boxes for labels.
        /// </summary>
        private SharpDX.Direct2D1.Bitmap _shadowBox;

        /// <summary>
        /// For rendering title of pipeline nodes
        /// </summary>
        private TextFormat _nodeTitlesTextFormat;

        protected readonly List<IRenderingDecorator> RenderingDecorators = new List<IRenderingDecorator>();

        public InternalDirect2DRenderListener(IPipelineContainer container, IExportPipelineControl control)
        {
            _container = container;
            _control = control;
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _shadowBox.Dispose();

                _nodeTitlesTextFormat.Dispose();
            }
        }

        public void RecreateState(IDirect2DRenderingState state)
        {
            _nodeTitlesTextFormat?.Dispose();

            _nodeTitlesTextFormat = new TextFormat(state.DirectWriteFactory, "Microsoft Sans Serif", 11)
            {
                TextAlignment = TextAlignment.Leading, 
                ParagraphAlignment = ParagraphAlignment.Center
            };

            // Create shadow box image
            using (var bitmap = new Bitmap(64, 64))
            {
                FastBitmap.ClearBitmap(bitmap, Color.Black);
                _shadowBox?.Dispose();
                _shadowBox = BaseDirect2DRenderer.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
            }
        }

        public void Render(IRenderListenerParameters parameters)
        {
            var decorators = RenderingDecorators;

            // Draw background across visible region
            RenderBackground(parameters);

            RenderInView(_container.ContentsView, parameters, decorators.ToArray());
            RenderInView(_container.UiContainerView, parameters, decorators.ToArray());
        }

        #region View Rendering

        public void RenderInView([NotNull] BaseView view, [NotNull] IRenderListenerParameters parameters, [NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var clippingRegion = parameters.ClippingRegion;
            var bezierRenderer = new BezierViewRenderer(new StaticDirect2DRenderingStateProvider(parameters.State));

            // Render all remaining objects
            var labels = view.Children.OfType<LabelView>().ToArray();
            var beziers = view.Children.OfType<BezierPathView>().ToArray();
            var nodeViews = view.Children.OfType<PipelineNodeView>().ToArray();
            var beziersLow = beziers.Where(b => !b.RenderOnTop);
            var beziersOver = beziers.Where(b => b.RenderOnTop);

            // Under beziers
            bezierRenderer.RenderBezierViews(beziersLow, clippingRegion, decorators);

            // Node views

            foreach (var stepView in nodeViews)
            {
                RenderStepView(stepView, parameters, decorators);
            }

            // Over beziers
            bezierRenderer.RenderBezierViews(beziersOver, clippingRegion, decorators);

            // Label views
            foreach (var label in labels.Where(l => l.Visible))
            {
                RenderLabelView(label, parameters, decorators);
            }
        }

        public void RenderStepView([NotNull] PipelineNodeView nodeView, [NotNull] IRenderListenerParameters parameters, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var state = parameters.State;
            var clippingRegion = parameters.ClippingRegion;

            state.PushingTransform(() =>
            {
                state.D2DRenderTarget.Transform = nodeView.GetAbsoluteTransform().ToRawMatrix3X2();

                var visibleArea = nodeView.GetFullBounds().Corners.Transform(nodeView.GetAbsoluteTransform()).Area();

                if (!clippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;

                var renderView = new InternalNodeViewRenderer(nodeView, parameters, _container.ContentsView.LocalTransform.ScaleVector >= Vector.Unit);
                renderView.RenderView(decorators);
            });
        }
        
        public void RenderLabelView([NotNull] LabelView labelView, [NotNull] IRenderListenerParameters parameters, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            InternalNodeViewRenderer.DrawLabelView(parameters, labelView, decorators);
        }

        public void RenderBackground([NotNull] IRenderListenerParameters parameters)
        {
            var renderingState = parameters.State;
            var clippingRegion = parameters.ClippingRegion;

            renderingState.PushingTransform(() =>
            {
                var transform = _container.ContentsView.GetAbsoluteTransform();

                var topLeft = _container.ContentsView.ConvertFrom(Vector.Zero, null);

                var scale = Vector.Unit;
                var gridOffset = topLeft;

                // Raw, non-transformed target grid separation.
                var baseGridSize = new Vector(100, 100);

                // Scale grid to increments of baseGridSize over zoom step.
                var largeGridSize = Vector.Round(baseGridSize * scale);
                var smallGridSize = largeGridSize / 10;
                
                var reg = new RectangleF(topLeft, new SizeF(_control.Size / _container.ContentsView.Scale));

                float startX = gridOffset.X;
                float endX = reg.Right;

                float startY = gridOffset.Y;
                float endY = reg.Bottom;

                var smallGridColor = Color.FromArgb(40, 40, 40).ToColor4();
                var largeGridColor = Color.FromArgb(50, 50, 50).ToColor4();

                // Draw small grid (when zoomed in enough)
                if (_container.ContentsView.Scale > new Vector(1.5f, 1.5f))
                {
                    using (var gridPen = new SolidColorBrush(renderingState.D2DRenderTarget, smallGridColor))
                    {
                        for (float x = startX - reg.Left % smallGridSize.X; x <= endX; x += smallGridSize.X)
                        {
                            var start = new Vector(x, reg.Top) * transform;
                            var end = new Vector(x, reg.Bottom) * transform;

                            if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(1, 0)))
                                continue;

                            renderingState.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), gridPen);
                        }

                        for (float y = startY - reg.Top % smallGridSize.Y; y <= endY; y += smallGridSize.Y)
                        {
                            var start = new Vector(reg.Left, y) * transform;
                            var end = new Vector(reg.Right, y) * transform;

                            if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(0, 1)))
                                continue;

                            renderingState.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), gridPen);
                        }
                    }
                }

                // Draw large grid on top
                using (var gridPen = new SolidColorBrush(renderingState.D2DRenderTarget, largeGridColor))
                {
                    for (float x = startX - reg.Left % largeGridSize.X; x <= endX; x += largeGridSize.X)
                    {
                        var start = new Vector(x, reg.Top) * transform;
                        var end = new Vector(x, reg.Bottom) * transform;

                        if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(1, 0)))
                            continue;

                        renderingState.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), gridPen);
                    }

                    for (float y = startY - reg.Top % largeGridSize.Y; y <= endY; y += largeGridSize.Y)
                    {
                        var start = new Vector(reg.Left, y) * transform;
                        var end = new Vector(reg.Right, y) * transform;

                        if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(0, 1)))
                            continue;

                        renderingState.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), gridPen);
                    }
                }
            });
        }

        #endregion

        #region Decorators

        public void AddDecorator(IRenderingDecorator decorator)
        {
            RenderingDecorators.Add(decorator);
        }

        public void RemoveDecorator(IRenderingDecorator decorator)
        {
            RenderingDecorators.Remove(decorator);
        }

        #endregion

        private class BezierViewRenderer
        {
            private readonly IDirect2DRenderingStateProvider _stateProvider;
            private IClippingRegion _clippingRegion;

            public BezierViewRenderer(IDirect2DRenderingStateProvider stateProvider)
            {
                _stateProvider = stateProvider;
            }

            public void RenderBezierViews([NotNull] IEnumerable<BezierPathView> views, IClippingRegion clippingRegion, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
            {
                var viewArray = views.ToArray();

                _clippingRegion = clippingRegion;

                // Render in steps
                foreach (var view in viewArray)
                {
                    RenderBezierView(view, decorators, Step.Fill);
                }

                foreach (var view in viewArray)
                {
                    RenderBezierView(view, decorators, Step.OuterStroke);
                }

                foreach (var view in viewArray)
                {
                    RenderBezierView(view, decorators, Step.Stroke);
                }
            }

            private void RenderBezierView([NotNull] BezierPathView bezierView, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators, Step step)
            {
                var renderingState = _stateProvider.GetLatestValidRenderingState();

                renderingState?.PushingTransform(() =>
                {
                    renderingState.D2DRenderTarget.Transform = bezierView.GetAbsoluteTransform().ToRawMatrix3X2();

                    var visibleArea = bezierView.GetFullBounds().Corners.Transform(bezierView.GetAbsoluteTransform()).Area();

                    if (!_clippingRegion.IsVisibleInClippingRegion(visibleArea))
                        return;

                    InnerRenderBezierView(bezierView, decorators, renderingState, step);
                });
            }

            private static void InnerRenderBezierView([NotNull] BezierPathView bezierView, [NotNull] IEnumerable<IRenderingDecorator> decorators, [NotNull] IDirect2DRenderingState renderingState, Step step)
            {
                var state = new BezierPathViewState
                {
                    StrokeColor = bezierView.StrokeColor,
                    StrokeWidth = bezierView.StrokeWidth,
                    FillColor = bezierView.FillColor,
                    OuterStrokeColor = bezierView.OuterStrokeColor,
                    OuterStrokeWidth = bezierView.OuterStrokeWidth
                };

                var geom = new PathGeometry(renderingState.D2DRenderTarget.Factory);

                var sink = geom.Open();

                FillBezierFigureSink(bezierView, sink);

                sink.Close();

                // Decorate
                foreach (var decorator in decorators)
                {
                    decorator.DecorateBezierPathView(bezierView, ref state);
                }

                switch (step)
                {
                    case Step.Fill:
                        // Fill
                        if (state.FillColor != Color.Transparent)
                        {
                            using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.FillColor.ToColor4()))
                            {
                                renderingState.D2DRenderTarget.FillGeometry(geom, brush);
                            }
                        }

                        break;

                    case Step.OuterStroke:

                        // Outer stroke
                        if (state.OuterStrokeWidth > 0 && state.OuterStrokeColor != Color.Transparent)
                        {
                            using (var brushOuterStroke =
                                new SolidColorBrush(renderingState.D2DRenderTarget, state.OuterStrokeColor.ToColor4()))
                            {
                                renderingState.D2DRenderTarget.DrawGeometry(geom, brushOuterStroke,
                                    state.StrokeWidth + state.OuterStrokeWidth);
                            }
                        }

                        break;

                    case Step.Stroke:

                        // Inner stroke
                        if (state.StrokeWidth > 0 && state.StrokeColor != Color.Transparent)
                        {
                            using (var brushStroke = new SolidColorBrush(renderingState.D2DRenderTarget, state.StrokeColor.ToColor4()))
                            {
                                renderingState.D2DRenderTarget.DrawGeometry(geom, brushStroke, state.StrokeWidth);
                            }
                        }

                        break;
                }

                sink.Dispose();
                geom.Dispose();
            }

            private static void FillBezierFigureSink([NotNull] BezierPathView bezierView, [NotNull] GeometrySink sink)
            {
                var d2DSink = new D2DPathSink(sink, FigureBegin.Filled);

                foreach (var input in bezierView.GetPathInputs())
                {
                    input.ApplyOnSink(d2DSink);
                }

                d2DSink.EndFigure(false);
            }

            private enum Step
            {
                Fill,
                OuterStroke,
                Stroke
            }
        }

        internal class D2DPathSink : IPathInputSink
        {
            private readonly GeometrySink _geometrySink;
            private bool _startOfFigure = true;
            private Vector _startLocation;
            private readonly FigureBegin _figureBegin;

            public D2DPathSink(GeometrySink geometrySink, FigureBegin figureBegin)
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

        internal sealed class GeometryCombination : IDisposable
        {
            private readonly PathGeometry _geometry;
            private readonly SharpDX.Direct2D1.Factory _factory;

            public GeometryCombination(SharpDX.Direct2D1.Factory factory)
            {
                _factory = factory;
                _geometry = new PathGeometry(factory);
            }

            #region IDisposable

            ~GeometryCombination()
            {
                Dispose(false);
            }

            public void Dispose()
            {
                Dispose(true);

                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                _geometry?.Dispose();
            }

            #endregion

            public Geometry GetGeometry()
            {
                return _geometry;
            }

            public void Combine([NotNull] Geometry geometry, CombineMode combineMode)
            {
                using (var sink = _geometry.Open())
                {
                    geometry.Combine(_geometry, combineMode, sink);

                    sink.Close();
                }
            }

            public void Combine([NotNull] Geometry geometry1, [NotNull] Geometry geometry2, CombineMode combineMode)
            {
                using (var sink = _geometry.Open())
                {
                    geometry1.Combine(geometry2, combineMode, sink);

                    sink.Close();
                }
            }

            public void Combine(RawRectangleF rectangle, [NotNull] Geometry geometry, CombineMode combineMode)
            {
                using (var rectGeometry = new RectangleGeometry(_factory, rectangle))
                {
                    Combine(rectGeometry, geometry, combineMode);
                }
            }

            public void Combine([NotNull] Geometry geometry, RawRectangleF rectangle, CombineMode combineMode)
            {
                using (var rectGeometry = new RectangleGeometry(_factory, rectangle))
                {
                    Combine(geometry, rectGeometry, combineMode);
                }
            }
        }

        internal sealed class DisposeBag : IDisposable
        {
            private bool _isDisposed;
            private readonly IList<IDisposable> _disposables = new List<IDisposable>();

            public void Dispose()
            {
                CheckNotDisposed();

                _isDisposed = true;

                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
            }

            public void AddDisposable(IDisposable disposable)
            {
                CheckNotDisposed();

                _disposables.Add(disposable);
            }

            private void CheckNotDisposed()
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(DisposeBag));
                }
            }
        }
    }

    internal class InternalNodeViewRenderer
    {
        [NotNull] private readonly PipelineNodeView _nodeView;
        private readonly IRenderListenerParameters _parameters;
        private readonly IDirect2DRenderingState _state;
        private readonly bool _useNearestNeighborOnIcon;

        private IRenderer Renderer => _parameters.Renderer;

        private AABB TitleArea => _nodeView.GetTitleArea();
        private AABB BodyTextArea => _nodeView.GetBodyTextArea();
        private AABB Bounds => _nodeView.Bounds;
        private AABB LinkLabelArea => _nodeView.LinkViewLabelArea;
        private IReadOnlyList<PipelineNodeLinkView> InLinks => _nodeView.InputViews;
        private IReadOnlyList<PipelineNodeLinkView> OutLinks => _nodeView.OutputViews;

        public InternalNodeViewRenderer([NotNull] PipelineNodeView nodeView, [NotNull] IRenderListenerParameters parameters, bool useNearestNeighborOnIcon)
        {
            _nodeView = nodeView;
            _parameters = parameters;
            _state = _parameters.State;
            _useNearestNeighborOnIcon = useNearestNeighborOnIcon;
        }

        public void RenderView([NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            Renderer.PushTransform();

            Renderer.Transform = _nodeView.GetAbsoluteTransform();

            var disposeBag = new InternalDirect2DRenderListener.DisposeBag();

            // Create rendering states for decorators
            var stepViewState = new PipelineStepViewState
            {
                FillColor = _nodeView.Color,
                TitleFillColor = _nodeView.Color.Faded(Color.Black, 0.8f),
                StrokeColor = _nodeView.StrokeColor,
                StrokeWidth = _nodeView.StrokeWidth,
                TitleFontColor = Color.White,
                BodyFontColor = Color.Black
            };

            // Decorate
            foreach (var decorator in decorators)
                decorator.DecoratePipelineStep(_nodeView, ref stepViewState);

            // Create disposable objects
            var roundedRect = new RoundedRectangle
            {
                RadiusX = 5,
                RadiusY = 5,
                Rect = new RawRectangleF(0, 0, Bounds.Width, Bounds.Height)
            };

            var roundedRectArea = new RoundedRectangleGeometry(_state.D2DFactory, roundedRect);
            var bodyFillStopCollection = new GradientStopCollection(_state.D2DRenderTarget, new[]
            {
                new GradientStop {Color = stepViewState.FillColor.ToColor4(), Position = 0},
                new GradientStop {Color = stepViewState.FillColor.Faded(Color.Black, 0.1f).ToColor4(), Position = 1}
            });
            var bodyFillGradientBrush = new LinearGradientBrush(
                _state.D2DRenderTarget,
                new LinearGradientBrushProperties
                {
                    StartPoint = new RawVector2(0, 0),
                    EndPoint = new RawVector2(0, Bounds.Height)
                },
                bodyFillStopCollection);

            var textFormat = new TextFormat(_state.DirectWriteFactory, _nodeView.Font.Name, _nodeView.Font.Size)
            {
                TextAlignment = TextAlignment.Leading, 
                ParagraphAlignment = ParagraphAlignment.Center
            };

            disposeBag.AddDisposable(roundedRectArea);
            disposeBag.AddDisposable(bodyFillStopCollection);
            disposeBag.AddDisposable(bodyFillGradientBrush);
            disposeBag.AddDisposable(textFormat);

            DrawConnectionLabelsBackground(roundedRectArea, bodyFillGradientBrush);
            DrawConnectionsBackground(roundedRectArea);
            DrawTitleBackground(roundedRectArea, stepViewState);
            DrawBodyText(stepViewState, roundedRectArea, bodyFillGradientBrush, textFormat);
            DrawBodyOutline(stepViewState, roundedRect);
            DrawIcon(TitleArea);
            DrawTitleText(textFormat, stepViewState);
            DrawLinkViews(decorators);

            disposeBag.Dispose();
        }

        private void DrawConnectionLabelsBackground([NotNull] Geometry bodyGeometry, Brush bodyFillGradientBrush)
        {
            using (var linkAreaGeom = new InternalDirect2DRenderListener.GeometryCombination(_state.D2DFactory))
            {
                linkAreaGeom.Combine(LinkLabelArea.ToRawRectangleF(), bodyGeometry, CombineMode.Intersect);

                // Fill for link label area
                _state.D2DRenderTarget.FillGeometry(linkAreaGeom.GetGeometry(), bodyFillGradientBrush);
            }
        }

        private void DrawLinkViews(IReadOnlyList<IRenderingDecorator> decorators)
        {
            // Draw outputs
            foreach (var link in OutLinks)
            {
                DrawNodeLinkView(link, decorators);
            }

            // Draw separation between input and output links
            if (InLinks.Count > 0 && OutLinks.Count > 0)
            {
                float yLine = (float)Math.Round(OutLinks.Select(o => o.FrameOnParent.Bottom + 6).Max());

                Renderer.StrokeColor = Color.Gray.WithTransparency(0.5f);
                Renderer.StrokeLine(new Vector(LinkLabelArea.Left + 6, yLine), new Vector(LinkLabelArea.Right - 6, yLine));
            }

            // Draw inputs
            foreach (var link in InLinks)
            {
                DrawNodeLinkView(link, decorators);
            }
        }

        private void DrawBodyOutline(PipelineStepViewState stepViewState, RoundedRectangle roundedRect)
        {
            Renderer.StrokeColor = stepViewState.StrokeColor;
            Renderer.StrokeWidth = stepViewState.StrokeWidth;

            Renderer.StrokeRoundedArea(roundedRect.Rect.ToAABB(), roundedRect.RadiusX, roundedRect.RadiusY);
        }

        private void DrawTitleBackground([NotNull] Geometry bodyGeometry, PipelineStepViewState stepViewState)
        {
            using (var titleAreaGeom = new InternalDirect2DRenderListener.GeometryCombination(_state.D2DFactory))
            {
                var titleRect = new RawRectangleF(0, 0, TitleArea.Width, TitleArea.Height);

                titleAreaGeom.Combine(titleRect, bodyGeometry, CombineMode.Intersect);

                // Fill title BG
                using (var solidColorBrush =
                    new SolidColorBrush(_state.D2DRenderTarget, stepViewState.TitleFillColor.ToColor4()))
                {
                    _state.D2DRenderTarget.FillGeometry(titleAreaGeom.GetGeometry(), solidColorBrush);
                }
            }
        }

        private void DrawConnectionsBackground([NotNull] Geometry bodyGeometry)
        {
            using (var stopCollection = new GradientStopCollection(_state.D2DRenderTarget, new[]
            {
                new GradientStop {Color = Color.Black.WithTransparency(0.5f).Faded(Color.White, 0.2f).ToColor4(), Position = 0},
                new GradientStop {Color = Color.Black.WithTransparency(0.5f).ToColor4(), Position = 0.3f}
            }))
            using (var gradientBrush = new LinearGradientBrush(
                _state.D2DRenderTarget,
                new LinearGradientBrushProperties
                {
                    StartPoint = new RawVector2(0, 0),
                    EndPoint = new RawVector2(0, Bounds.Height)
                },
                stopCollection))
            using (var linkAreaGeom = new InternalDirect2DRenderListener.GeometryCombination(_state.D2DFactory))
            {
                linkAreaGeom.Combine(bodyGeometry, LinkLabelArea.ToRawRectangleF(), CombineMode.Exclude);

                _state.D2DRenderTarget.FillGeometry(linkAreaGeom.GetGeometry(), gradientBrush);
            }
        }

        private void DrawBodyText(PipelineStepViewState stepViewState, Geometry bodyGeometry, Brush bodyFillBrush, TextFormat textFormat)
        {
            string bodyText = _nodeView.BodyText;
            if (string.IsNullOrEmpty(bodyText))
                return;

            float yLine = 0;

            // Draw separation between links and body text
            bool hasLinks = InLinks.Count > 0 || OutLinks.Count > 0;
            if (hasLinks)
            {
                yLine = (float)Math.Round(OutLinks.Concat(InLinks).Select(o => o.FrameOnParent.Bottom + 6).Max());
                using (var brush = new SolidColorBrush(_state.D2DRenderTarget,
                    stepViewState.StrokeColor.WithTransparency(0.7f).ToColor4()))
                {
                    _state.D2DRenderTarget.DrawLine(new Vector(0, yLine).ToRawVector2(),
                        new Vector(_nodeView.Width, yLine).ToRawVector2(), brush);
                }
            }

            // Draw fill color
            using (var textAreaGeom = new InternalDirect2DRenderListener.GeometryCombination(_state.D2DFactory))
            {
                var areaOnView = new AABB(0, hasLinks ? yLine : TitleArea.Bottom, _nodeView.Height, _nodeView.Width);

                textAreaGeom.Combine(areaOnView.ToRawRectangleF(), bodyGeometry, CombineMode.Intersect);

                _state.D2DRenderTarget.FillGeometry(textAreaGeom.GetGeometry(), bodyFillBrush);
            }

            var attributes = new TextLayoutAttributes
            {
                Font = textFormat.FontFamilyName,
                FontSize = textFormat.FontSize,
                HorizontalTextAlignment = Direct2DConversionHelpers.HorizontalTextAlignmentFor(textFormat.TextAlignment),
                VerticalTextAlignment = Direct2DConversionHelpers.VerticalTextAlignmentFor(textFormat.ParagraphAlignment),
                WordWrap = Direct2DConversionHelpers.TextWordWrapFor(textFormat.WordWrapping),
                AvailableWidth = BodyTextArea.Width,
                AvailableHeight = BodyTextArea.Height
            };

            ITextLayout layout = null;
            _parameters.TextLayoutRenderer.WithPreparedTextLayout(stepViewState.BodyFontColor, (AttributedText)bodyText, ref layout, attributes,
                (textLayout, renderer) =>
                {
                    renderer.Draw(textLayout, BodyTextArea.Minimum.X, BodyTextArea.Minimum.Y);
                });
        }

        private void DrawTitleText([NotNull] TextFormat textFormat, PipelineStepViewState stepViewState)
        {
            var attributes = new TextLayoutAttributes
            {
                Font = textFormat.FontFamilyName,
                FontSize = textFormat.FontSize,
                HorizontalTextAlignment = Direct2DConversionHelpers.HorizontalTextAlignmentFor(textFormat.TextAlignment),
                VerticalTextAlignment = Direct2DConversionHelpers.VerticalTextAlignmentFor(textFormat.ParagraphAlignment),
                WordWrap = Direct2DConversionHelpers.TextWordWrapFor(textFormat.WordWrapping),
                AvailableWidth = _nodeView.TitleTextArea.Width,
                AvailableHeight = _nodeView.TitleTextArea.Height
            };

            ITextLayout layout = null;
            _parameters.TextLayoutRenderer.WithPreparedTextLayout(stepViewState.TitleFontColor, (AttributedText) _nodeView.Name, ref layout, attributes,
                (textLayout, renderer) =>
                {
                    renderer.Draw(textLayout, _nodeView.TitleTextArea.Minimum.X, _nodeView.TitleTextArea.Minimum.Y);
                });
        }

        private void DrawIcon(AABB titleArea)
        {
            if (_nodeView.Icon == null)
                return;

            var icon = _nodeView.Icon.Value;

            var bitmap = _parameters.ImageResources.BitmapForResource(icon);
            if (bitmap == null)
                return;

            float imgY = titleArea.Height / 2 - (float)icon.Height / 2;
            var imgBounds = (AABB)new RectangleF(imgY, imgY, icon.Width, icon.Height);

            var mode = BitmapInterpolationMode.Linear;

            // Draw with pixel quality when zoomed in so icon doesn't render all blurry
            if (_useNearestNeighborOnIcon)
            {
                mode = BitmapInterpolationMode.NearestNeighbor;
            }

            _state.D2DRenderTarget.DrawBitmap(bitmap, imgBounds.ToRawRectangleF(), 1f, mode);
        }

        private void DrawNodeLinkView([NotNull] PipelineNodeLinkView link, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var state = _parameters.State;
            var clippingRegion = _parameters.ClippingRegion;

            Renderer.PushTransform();

            var visibleArea = link.Bounds.TransformedBounds(link.GetAbsoluteTransform());

            if (clippingRegion.IsVisibleInClippingRegion(visibleArea))
            {
                var linkState = new PipelineStepViewLinkState
                {
                    FillColor = Color.White,
                    StrokeColor = link.StrokeColor,
                    StrokeWidth = link.StrokeWidth
                };

                // Decorate
                foreach (var decorator in decorators)
                {
                    if (link.NodeLink is IPipelineInput)
                        decorator.DecoratePipelineStepInput(link.NodeView, link, ref linkState);
                    else
                        decorator.DecoratePipelineStepOutput(link.NodeView, link, ref linkState);
                }

                Renderer.Transform = link.GetAbsoluteTransform();

                var rectangle = link.Bounds;

                Renderer.StrokeColor = linkState.StrokeColor;
                Renderer.FillColor = linkState.FillColor;

                Renderer.FillEllipse(rectangle);
                Renderer.StrokeEllipse(rectangle);
            }

            // Draw label view
            DrawLabelView(_parameters, link.LinkLabel, decorators);

            Renderer.PopTransform();
        }

        public static void DrawLabelView([NotNull] IRenderListenerParameters parameters, [NotNull] LabelView labelView, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var renderingState = parameters.State;
            var clippingRegion = parameters.ClippingRegion;

            renderingState.PushingTransform(() =>
            {
                renderingState.D2DRenderTarget.Transform = labelView.GetAbsoluteTransform().ToRawMatrix3X2();

                var visibleArea =
                    labelView
                        .GetFullBounds().Corners
                        .Transform(labelView.GetAbsoluteTransform()).Area();

                if (!clippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;

                var state = new LabelViewState
                {
                    StrokeColor = labelView.StrokeColor,
                    StrokeWidth = labelView.StrokeWidth,
                    TextColor = labelView.TextColor,
                    BackgroundColor = labelView.BackgroundColor
                };

                // Decorate
                foreach (var decorator in decorators)
                    decorator.DecorateLabelView(labelView, ref state);

                var roundedRect = new RoundedRectangle
                {
                    RadiusX = 5,
                    RadiusY = 5,
                    Rect = new RawRectangleF(0, 0, labelView.Bounds.Width, labelView.Bounds.Height)
                };

                using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.BackgroundColor.ToColor4()))
                {
                    renderingState.D2DRenderTarget.FillRoundedRectangle(roundedRect, brush);
                }

                if (state.StrokeWidth > 0)
                    using (var pen = new SolidColorBrush(renderingState.D2DRenderTarget, state.StrokeColor.ToColor4()))
                    {
                        renderingState.D2DRenderTarget.DrawRoundedRectangle(roundedRect, pen, state.StrokeWidth);
                    }

                var textBounds = labelView.TextBounds;

                var format = new TextFormat(renderingState.DirectWriteFactory, labelView.TextFont.Name, labelView.TextFont.Size)
                {
                    TextAlignment = TextAlignment.Leading, 
                    ParagraphAlignment = ParagraphAlignment.Center
                };

                using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.TextColor.ToColor4()))
                using (var textFormat = format)
                using (var textLayout = new TextLayout(renderingState.DirectWriteFactory, labelView.Text, textFormat, textBounds.Width, textBounds.Height))
                {
                    // Apply text attributes
                    if (labelView.AttributedText.HasAttributes)
                    {
                        var disposes = new List<IDisposable>();

                        foreach (var textSegment in labelView.AttributedText.GetTextSegments())
                        {
                            if (textSegment.HasAttribute<ForegroundColorAttribute>())
                            {
                                var colorAttr = textSegment.GetAttribute<ForegroundColorAttribute>();

                                var segmentBrush =
                                    new SolidColorBrush(renderingState.D2DRenderTarget,
                                        colorAttr.ForeColor.ToColor4());

                                disposes.Add(segmentBrush);

                                textLayout.SetDrawingEffect(segmentBrush,
                                    new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                            }
                            if (textSegment.HasAttribute<TextFontAttribute>())
                            {
                                var fontAttr = textSegment.GetAttribute<TextFontAttribute>();

                                textLayout.SetFontFamilyName(fontAttr.Font.FontFamily.Name,
                                    new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                                textLayout.SetFontSize(fontAttr.Font.Size,
                                    new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                            }
                        }

                        var textRenderer = parameters.TextColorRenderer;

                        var prev = textRenderer.DefaultBrush;
                        textRenderer.DefaultBrush = brush;

                        textLayout.Draw(textRenderer, textBounds.Minimum.X, textBounds.Minimum.Y);

                        textRenderer.DefaultBrush = prev;

                        foreach (var disposable in disposes)
                        {
                            disposable.Dispose();
                        }
                    }
                    else
                    {
                        renderingState.D2DRenderTarget.DrawTextLayout(textBounds.Minimum.ToRawVector2(), textLayout, brush);
                    }
                }
            });
        }
    }
}
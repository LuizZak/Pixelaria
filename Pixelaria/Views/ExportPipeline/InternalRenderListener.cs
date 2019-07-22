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
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Internal render listener for rendering pipeline steps
    /// </summary>
    internal class InternalRenderListener : IRenderListener, IRenderingDecoratorContainer
    {
        public int RenderOrder { get; } = RenderOrdering.PipelineView;

        private readonly IPipelineContainer _container; // For relative position calculations
        private readonly IExportPipelineControl _control;

        protected readonly List<IRenderingDecorator> RenderingDecorators = new List<IRenderingDecorator>();

        public InternalRenderListener(IPipelineContainer container, IExportPipelineControl control)
        {
            _container = container;
            _control = control;
        }

        public void RecreateState(IRenderLoopState state)
        {
            
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
            var bezierRenderer = new BezierViewRenderer(parameters);

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
            var clippingRegion = parameters.ClippingRegion;

            parameters.Renderer.PushingTransform(() =>
            {
                parameters.Renderer.Transform = nodeView.GetAbsoluteTransform();

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
            var clippingRegion = parameters.ClippingRegion;
            var renderer = parameters.Renderer;

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

            var smallGridColor = Color.FromArgb(40, 40, 40);
            var largeGridColor = Color.FromArgb(50, 50, 50);

            // Draw small grid (when zoomed in enough)
            if (_container.ContentsView.Scale > new Vector(1.5f, 1.5f))
            {
                renderer.SetStrokeColor(smallGridColor);

                for (float x = startX - reg.Left % smallGridSize.X; x <= endX; x += smallGridSize.X)
                {
                    var start = new Vector(x, reg.Top) * transform;
                    var end = new Vector(x, reg.Bottom) * transform;

                    if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(1, 0)))
                        continue;

                    renderer.StrokeLine(start, end);
                }

                for (float y = startY - reg.Top % smallGridSize.Y; y <= endY; y += smallGridSize.Y)
                {
                    var start = new Vector(reg.Left, y) * transform;
                    var end = new Vector(reg.Right, y) * transform;

                    if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(0, 1)))
                        continue;

                    renderer.StrokeLine(start, end);
                }
            }

            // Draw large grid on top
            renderer.SetStrokeColor(largeGridColor);
            for (float x = startX - reg.Left % largeGridSize.X; x <= endX; x += largeGridSize.X)
            {
                var start = new Vector(x, reg.Top) * transform;
                var end = new Vector(x, reg.Bottom) * transform;

                if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(1, 0)))
                    continue;

                renderer.StrokeLine(start, end);
            }

            for (float y = startY - reg.Top % largeGridSize.Y; y <= endY; y += largeGridSize.Y)
            {
                var start = new Vector(reg.Left, y) * transform;
                var end = new Vector(reg.Right, y) * transform;

                if (!clippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(0, 1)))
                    continue;

                renderer.StrokeLine(start, end);
            }
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
            private readonly IRenderListenerParameters _parameters;
            private IClippingRegion _clippingRegion;

            public BezierViewRenderer(IRenderListenerParameters parameters)
            {
                _parameters = parameters;
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
                _parameters.Renderer.PushingTransform(() =>
                {
                    _parameters.Renderer.Transform = bezierView.GetAbsoluteTransform();

                    var visibleArea = bezierView.GetFullBounds().Corners.Transform(bezierView.GetAbsoluteTransform()).Area();

                    if (!_clippingRegion.IsVisibleInClippingRegion(visibleArea))
                        return;

                    InnerRenderBezierView(bezierView, decorators, _parameters, step);
                });
            }

            private static void InnerRenderBezierView([NotNull] BezierPathView bezierView, [NotNull] IEnumerable<IRenderingDecorator> decorators, [NotNull] IRenderListenerParameters parameters, Step step)
            {
                var renderer = parameters.Renderer;

                var state = new BezierPathViewState
                {
                    StrokeColor = bezierView.StrokeColor,
                    StrokeWidth = bezierView.StrokeWidth,
                    FillColor = bezierView.FillColor,
                    OuterStrokeColor = bezierView.OuterStrokeColor,
                    OuterStrokeWidth = bezierView.OuterStrokeWidth
                };

                var path = renderer.CreatePath(sink =>
                {
                    FillBezierFigureSink(bezierView, sink);
                });

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
                            renderer.SetFillColor(state.FillColor);
                            renderer.FillPath(path);
                        }

                        break;

                    case Step.OuterStroke:

                        // Outer stroke
                        if (state.OuterStrokeWidth > 0 && state.OuterStrokeColor != Color.Transparent)
                        {
                            renderer.SetStrokeColor(state.OuterStrokeColor);
                            renderer.StrokePath(path, state.StrokeWidth + state.OuterStrokeWidth);
                        }

                        break;

                    case Step.Stroke:

                        // Inner stroke
                        if (state.StrokeWidth > 0 && state.StrokeColor != Color.Transparent)
                        {
                            renderer.SetStrokeColor(state.StrokeColor);
                            renderer.StrokePath(path, state.StrokeWidth);
                        }

                        break;
                }

                path.Dispose();
            }

            private static void FillBezierFigureSink([NotNull] BezierPathView bezierView, [NotNull] IPathInputSink sink)
            {
                foreach (var input in bezierView.GetPathInputs())
                {
                    input.ApplyOnSink(sink);
                }

                sink.EndFigure(false);
            }

            private enum Step
            {
                Fill,
                OuterStroke,
                Stroke
            }
        }
    }

    internal class InternalNodeViewRenderer
    {
        [NotNull] private readonly PipelineNodeView _nodeView;
        private readonly IRenderListenerParameters _parameters;
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
            _useNearestNeighborOnIcon = useNearestNeighborOnIcon;
        }

        public void RenderView([NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            Renderer.PushTransform();

            Renderer.Transform = _nodeView.GetAbsoluteTransform();

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

            var roundedRectGeom = PolyGeometry.RoundedRectangle(Bounds, 5, 5, 5);
            
            // Create disposable objects
            var textFormat = new TextFormatAttributes
            {
                Font = _nodeView.Font.Name,
                FontSize = _nodeView.Font.Size,
                HorizontalTextAlignment = HorizontalTextAlignment.Leading, 
                VerticalTextAlignment = VerticalTextAlignment.Center
            };

            var bodyFillBrush = Renderer.CreateLinearGradientBrush(new[]
            {
                new PixGradientStop(stepViewState.FillColor, 0),
                new PixGradientStop(stepViewState.FillColor.Faded(Color.Black, 0.1f), 1)
            }, Vector.Zero, new Vector(0, Bounds.Height));

            DrawConnectionLabelsBackground(roundedRectGeom, bodyFillBrush);
            DrawConnectionsBackground(roundedRectGeom);
            DrawTitleBackground(roundedRectGeom, stepViewState);
            DrawBodyText(stepViewState, roundedRectGeom, bodyFillBrush, textFormat);
            DrawBodyOutline(stepViewState, Bounds, 5, 5);
            DrawIcon(TitleArea);
            DrawTitleText(textFormat, stepViewState);
            DrawLinkViews(decorators);

            Renderer.PopTransform();
        }

        private void DrawConnectionLabelsBackground([NotNull] PolyGeometry bodyGeometry, [NotNull] IBrush bodyFillGradientBrush)
        {
            var linkAreaGeom = PolyGeometry.Rectangle(LinkLabelArea);
            linkAreaGeom.Combine(bodyGeometry, GeometryOperation.Intersect);

            // Fill for link label area
            Renderer.SetFillBrush(bodyFillGradientBrush);
            Renderer.FillGeometry(linkAreaGeom);
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

                Renderer.SetStrokeColor(Color.Gray.WithTransparency(0.5f));
                Renderer.StrokeLine(new Vector(LinkLabelArea.Left + 6, yLine), new Vector(LinkLabelArea.Right - 6, yLine));
            }

            // Draw inputs
            foreach (var link in InLinks)
            {
                DrawNodeLinkView(link, decorators);
            }
        }

        private void DrawBodyOutline(PipelineStepViewState stepViewState, AABB rect, float radiusX, float radiusY)
        {
            Renderer.SetStrokeColor(stepViewState.StrokeColor);
            Renderer.StrokeRoundedArea(rect, radiusX, radiusY, stepViewState.StrokeWidth);
        }

        private void DrawTitleBackground([NotNull] PolyGeometry bodyGeometry, PipelineStepViewState stepViewState)
        {
            var titleRect = AABB.FromRectangle(0, 0, TitleArea.Width, TitleArea.Height);

            var titleAreaGeom = PolyGeometry.Rectangle(titleRect);
            titleAreaGeom.Combine(bodyGeometry, GeometryOperation.Intersect);

            // Fill title BG
            Renderer.SetFillColor(stepViewState.TitleFillColor);
            Renderer.FillGeometry(titleAreaGeom);
        }

        private void DrawConnectionsBackground([NotNull] PolyGeometry bodyGeometry)
        {
            var stops = new[]
            {
                new PixGradientStop(Color.Black.WithTransparency(0.5f).Faded(Color.White, 0.2f), 0),
                new PixGradientStop(Color.Black.WithTransparency(0.5f), 0.3f)
            };

            var brush = Renderer.CreateLinearGradientBrush(stops, Vector.Zero, new Vector(0, Bounds.Height));

            var linkAreaGeom = new PolyGeometry(bodyGeometry);
            linkAreaGeom.Combine(LinkLabelArea, GeometryOperation.Exclude);

            Renderer.SetFillBrush(brush);
            Renderer.FillGeometry(linkAreaGeom);
        }

        private void DrawBodyText(PipelineStepViewState stepViewState, PolyGeometry bodyGeometry, IBrush bodyFillBrush, TextFormatAttributes textFormatAttributes)
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

                Renderer.SetStrokeColor(stepViewState.StrokeColor.WithTransparency(0.7f));
                Renderer.StrokeLine(new Vector(0, yLine), new Vector(_nodeView.Width, yLine));
            }

            // Draw fill color
            var areaOnView = new AABB(0, hasLinks ? yLine : TitleArea.Bottom, _nodeView.Height, _nodeView.Width);

            var textAreaGeom = PolyGeometry.Rectangle(areaOnView);
            textAreaGeom.Combine(bodyGeometry, GeometryOperation.Intersect);

            Renderer.SetFillBrush(bodyFillBrush);
            Renderer.FillGeometry(textAreaGeom);

            var attributes = new TextLayoutAttributes(textFormatAttributes)
            {
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

        private void DrawTitleText(TextFormatAttributes textFormatAttributes, PipelineStepViewState stepViewState)
        {
            var attributes = new TextLayoutAttributes(textFormatAttributes)
            {
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

            float imgY = titleArea.Height / 2 - (float)icon.Height / 2;
            var imgBounds = (AABB)new RectangleF(imgY, imgY, icon.Width, icon.Height);

            var mode = ImageInterpolationMode.Linear;

            // Draw with pixel quality when zoomed in so icon doesn't render all blurry
            if (_useNearestNeighborOnIcon)
            {
                mode = ImageInterpolationMode.NearestNeighbor;
            }

            Renderer.DrawBitmap(icon, (RectangleF)imgBounds, 1, mode);
        }

        private void DrawNodeLinkView([NotNull] PipelineNodeLinkView link, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
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

                Renderer.SetStrokeColor(linkState.StrokeColor);
                Renderer.SetFillColor(linkState.FillColor);

                Renderer.FillEllipse(rectangle);
                Renderer.StrokeEllipse(rectangle);
            }

            // Draw label view
            DrawLabelView(_parameters, link.LinkLabel, decorators);

            Renderer.PopTransform();
        }

        public static void DrawLabelView([NotNull] IRenderListenerParameters parameters, [NotNull] LabelView labelView, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var renderer = parameters.Renderer;

            var clippingRegion = parameters.ClippingRegion;

            renderer.PushingTransform(() =>
            {
                renderer.Transform = labelView.GetAbsoluteTransform();

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

                renderer.SetFillColor(state.BackgroundColor);
                renderer.FillRoundedArea(labelView.Bounds, 5, 5);

                if (state.StrokeWidth > 0)
                {
                    renderer.SetStrokeColor(state.StrokeColor);
                    renderer.StrokeRoundedArea(labelView.Bounds, 5, 5);
                }

                var textBounds = labelView.TextBounds;

                var format = new TextFormatAttributes(labelView.TextFont.Name, labelView.TextFont.Size)
                {
                    HorizontalTextAlignment = HorizontalTextAlignment.Leading,
                    VerticalTextAlignment = VerticalTextAlignment.Center
                };
                var attributes = new TextLayoutAttributes(format)
                {
                    AvailableWidth = textBounds.Width,
                    AvailableHeight = textBounds.Height
                };

                ITextLayout existing = null;
                parameters.TextLayoutRenderer.WithPreparedTextLayout(state.TextColor, labelView.AttributedText, ref existing, attributes,
                    (layout, textRenderer) =>
                    {
                        textRenderer.Draw(layout, textBounds.Minimum.X, textBounds.Minimum.Y);
                    });
            });
        }
    }
}
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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixDirectX.Rendering;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixRendering;
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
            parameters.Renderer.PushTransform(view.LocalTransform);

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

            parameters.Renderer.PopTransform();
        }

        public void RenderStepView([NotNull] PipelineNodeView nodeView, [NotNull] IRenderListenerParameters parameters, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var renderView = new InternalNodeViewRenderer(nodeView, parameters, parameters.Renderer.Transform.ScaleVector >= Vector.Unit);
            renderView.RenderView(decorators);
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
                    _parameters.Renderer.Transform *= bezierView.LocalTransform;

                    var visibleArea = bezierView.BoundsForInvalidateFullBounds().TransformedBounds(_parameters.Renderer.Transform);

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
}
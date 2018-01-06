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
using System.Windows.Forms;

using JetBrains.Annotations;

using PixCore.Colors;
using PixCore.Geometry;
using PixCore.Text.Attributes;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using PixUI;

using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI.Rendering;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;

using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using RectangleF = System.Drawing.RectangleF;
using CombineMode = SharpDX.Direct2D1.CombineMode;
using TextRange = SharpDX.DirectWrite.TextRange;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Renders a pipeline export view
    /// </summary>
    internal class Direct2DRenderer : BaseDirect2DRenderer, IExportPipelineDirect2DRenderer
    {
        private readonly ExportPipelineControl.IPipelineContainer _container; // For relative position calculations
        private readonly DefaultLabelViewSizeProvider _labelViewSizeProvider;
        private readonly Control _control;

        /// <summary>
        /// A small 32x32 box used to draw shadow boxes for labels.
        /// </summary>
        private SharpDX.Direct2D1.Bitmap _shadowBox;

        /// <summary>
        /// For rendering title of pipeline nodes
        /// </summary>
        private TextFormat _nodeTitlesTextFormat;

        public ILabelViewSizeProvider LabelViewSizeProvider => _labelViewSizeProvider;

        protected readonly List<IRenderingDecorator> RenderingDecorators = new List<IRenderingDecorator>();
        
        public Direct2DRenderer(ExportPipelineControl.IPipelineContainer container, Control control)
        {
            _labelViewSizeProvider = new DefaultLabelViewSizeProvider(this);
            _container = container;
            _control = control;
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _shadowBox.Dispose();

                _nodeTitlesTextFormat.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void Initialize(IDirect2DRenderingState state)
        {
            base.Initialize(state);
            
            _nodeTitlesTextFormat = new TextFormat(state.DirectWriteFactory, "Microsoft Sans Serif", 11)
            {
                TextAlignment = TextAlignment.Leading,
                ParagraphAlignment = ParagraphAlignment.Center
            };

            // Create shadow box image
            using (var bitmap = new Bitmap(32, 32))
            {
                _shadowBox = CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
            }
        }
        
        #region View Rendering

        public override void Render(IDirect2DRenderingState state, IClippingRegion clipping)
        {
            base.Render(state, clipping);

            var decorators = RenderingDecorators;
            
            // Draw background across visible region
            RenderBackground(state);

            RenderInView(_container.ContentsView, state, decorators.ToArray());
            RenderInView(_container.UiContainerView, state, decorators.ToArray());
        }

        public void RenderInView([NotNull] BaseView view, [NotNull] IDirect2DRenderingState state, IReadOnlyList<IRenderingDecorator> decorators)
        {
            // Render all remaining objects
            var labels = view.Children.OfType<LabelView>().ToArray();
            var beziers = view.Children.OfType<BezierPathView>().ToArray();
            var nodeViews = view.Children.OfType<PipelineNodeView>().ToArray();
            var beziersLow = beziers.Where(b => !b.RenderOnTop);
            var beziersOver = beziers.Where(b => b.RenderOnTop);
            foreach (var bezier in beziersLow)
            {
                RenderBezierView(bezier, state, decorators);
            }
            foreach (var stepView in nodeViews)
            {
                RenderStepView(stepView, state, decorators);
            }
            foreach (var bezier in beziersOver)
            {
                RenderBezierView(bezier, state, decorators);
            }
            foreach (var label in labels.Where(l => l.Visible))
            {
                RenderLabelView(label, state, decorators);
            }
        }

        public void RenderStepView([NotNull] PipelineNodeView nodeView, [NotNull] IDirect2DRenderingState state, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            state.PushingTransform(() =>
            {
                state.D2DRenderTarget.Transform = new Matrix3x2(nodeView.GetAbsoluteTransform().Elements);
                    
                var visibleArea = nodeView.GetFullBounds().Corners.Transform(nodeView.GetAbsoluteTransform()).Area();
                    
                if (!ClippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;
                    
                // Create rendering states for decorators
                var stepViewState = new PipelineStepViewState
                {
                    FillColor = nodeView.Color,
                    TitleFillColor = nodeView.Color.Faded(Color.Black, 0.8f),
                    StrokeColor = nodeView.StrokeColor,
                    StrokeWidth = nodeView.StrokeWidth,
                    TitleFontColor = Color.White,
                    BodyFontColor = Color.Black
                };

                // Decorate
                foreach (var decorator in decorators)
                    decorator.DecoratePipelineStep(nodeView, ref stepViewState);

                var bounds = nodeView.Bounds;

                var roundedRect = new RoundedRectangle
                {
                    RadiusX = 5,
                    RadiusY = 5,
                    Rect = new RawRectangleF(0, 0, bounds.Width, bounds.Height)
                };

                // Draw body fill
                using (var stopCollection = new GradientStopCollection(state.D2DRenderTarget, new[]
                {
                    new GradientStop {Color = stepViewState.FillColor.ToColor4(), Position = 0},
                    new GradientStop {Color = stepViewState.FillColor.Faded(Color.Black, 0.1f).ToColor4(), Position = 1}
                }))
                using (var gradientBrush = new LinearGradientBrush(
                    state.D2DRenderTarget,
                    new LinearGradientBrushProperties
                    {
                        StartPoint = new RawVector2(0, 0),
                        EndPoint = new RawVector2(0, bounds.Height)
                    },
                    stopCollection))

                {
                    state.D2DRenderTarget.FillRoundedRectangle(roundedRect, gradientBrush);
                }

                var titleArea = nodeView.GetTitleArea();

                // Title background (clipped)
                using (var titleAreaGeom = new PathGeometry(state.D2DFactory))
                using (var roundedRectArea = new RoundedRectangleGeometry(state.D2DFactory, roundedRect))
                {
                    var titleRect = new RawRectangleF(0, 0, titleArea.Width, titleArea.Height);
                    using (var titleClip = new RectangleGeometry(state.D2DFactory, titleRect))
                    {
                        var sink = titleAreaGeom.Open();

                        titleClip.Combine(roundedRectArea, CombineMode.Intersect, sink);

                        sink.Close();
                        sink.Dispose();
                    }

                    // Fill title BG
                    using (var solidColorBrush =
                        new SolidColorBrush(state.D2DRenderTarget, stepViewState.TitleFillColor.ToColor4()))
                    {
                        state.D2DRenderTarget.FillGeometry(titleAreaGeom, solidColorBrush);
                    }

                    roundedRectArea.Dispose();
                }

                // Draw title icon/text
                int titleX = 4;

                // Draw icon, if available
                if (nodeView.Icon != null)
                {
                    var icon = nodeView.Icon.Value;

                    titleX += icon.Width + 5;

                    float imgY = titleArea.Height / 2 - (float)icon.Height / 2;

                    var imgBounds = (AABB)new RectangleF(imgY, imgY, icon.Width, icon.Height);

                    var bitmap = ImageResources.BitmapForResource(icon);
                    if (bitmap != null)
                    {
                        var mode = BitmapInterpolationMode.Linear;

                        // Draw with high quality only when zoomed out
                        if (new AABB(Vector.Zero, Vector.Unit).TransformedBounds(_container.ContentsView.LocalTransform).Size >=
                            Vector.Unit)
                        {
                            mode = BitmapInterpolationMode.NearestNeighbor;
                        }

                        state.D2DRenderTarget.DrawBitmap(bitmap, imgBounds.ToRawRectangleF(), 1f, mode);
                    }
                }

                // Draw title text
                using (var textLayout = new TextLayout(state.DirectWriteFactory, nodeView.Name, _nodeTitlesTextFormat, titleArea.Width, titleArea.Height))
                using (var brush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.TitleFontColor.ToColor4()))
                {
                    state.D2DRenderTarget.DrawTextLayout(new RawVector2(titleX, 0), textLayout, brush, DrawTextOptions.EnableColorFont);
                }

                // Draw body text, if available
                string bodyText = nodeView.BodyText;
                if (bodyText != null)
                {
                    var area = nodeView.GetBodyTextArea();

                    using (var textLayout = new TextLayout(state.DirectWriteFactory, bodyText, _nodeTitlesTextFormat, area.Width, area.Height))
                    using (var brush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.BodyFontColor.ToColor4()))
                    {
                        state.D2DRenderTarget.DrawTextLayout(area.Minimum.ToRawVector2(), textLayout, brush, DrawTextOptions.EnableColorFont);
                    }
                }

                // Draw outline now
                using (var penBrush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.StrokeColor.ToColor4()))
                {
                    state.D2DRenderTarget.DrawRoundedRectangle(roundedRect, penBrush, stepViewState.StrokeWidth);
                }
                    
                // Draw in-going and out-going links
                var inLinks = nodeView.InputViews;
                var outLinks = nodeView.OutputViews;

                // Draw inputs
                foreach (var link in inLinks)
                {
                    RenderNodeLinkView(state, link, decorators);
                }

                // Draw outputs
                foreach (var link in outLinks)
                {
                    RenderNodeLinkView(state, link, decorators);
                }
            });
        }

        private void RenderNodeLinkView([NotNull] IDirect2DRenderingState state, [NotNull] PipelineNodeLinkView link, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            state.PushingTransform(() =>
            {
                var visibleArea =
                    link.GetFullBounds().Corners
                        .Transform(link.GetAbsoluteTransform()).Area();

                if (!ClippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;

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

                state.D2DRenderTarget.Transform = new Matrix3x2(link.GetAbsoluteTransform().Elements);

                var rectangle = link.Bounds;

                using (var pen = new SolidColorBrush(state.D2DRenderTarget, linkState.StrokeColor.ToColor4()))
                using (var brush = new SolidColorBrush(state.D2DRenderTarget, linkState.FillColor.ToColor4()))
                {
                    var ellipse = new Ellipse(rectangle.Center.ToRawVector2(), rectangle.Width / 2, rectangle.Width / 2);

                    state.D2DRenderTarget.FillEllipse(ellipse, brush);
                    state.D2DRenderTarget.DrawEllipse(ellipse, pen, linkState.StrokeWidth);
                }

                // Draw label view
                RenderLabelView(link.LinkLabel, state, decorators);
            });
        }

        public void RenderBezierView([NotNull] BezierPathView bezierView, [NotNull] IDirect2DRenderingState renderingState, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            renderingState.PushingTransform(() =>
            {
                renderingState.D2DRenderTarget.Transform = new Matrix3x2(bezierView.GetAbsoluteTransform().Elements);
                    
                var visibleArea = bezierView.GetFullBounds().Corners.Transform(bezierView.GetAbsoluteTransform()).Area();

                if (!ClippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;
                    
                var state = new BezierPathViewState
                {
                    StrokeColor = bezierView.StrokeColor,
                    StrokeWidth = bezierView.StrokeWidth,
                    FillColor = bezierView.FillColor
                };
                        
                var geom = new PathGeometry(renderingState.D2DRenderTarget.Factory);
                        
                var sink = geom.Open();
                    
                foreach (var input in bezierView.GetPathInputs())
                {
                    if (input is BezierPathView.RectanglePathInput recInput)
                    {
                        var rec = recInput.Rectangle;

                        sink.BeginFigure(rec.Minimum.ToRawVector2(), FigureBegin.Filled);
                        sink.AddLine(new Vector(rec.Right, rec.Top).ToRawVector2());
                        sink.AddLine(new Vector(rec.Right, rec.Bottom).ToRawVector2());
                        sink.AddLine(new Vector(rec.Left, rec.Bottom).ToRawVector2());
                        sink.EndFigure(FigureEnd.Closed);
                    }
                    else if (input is BezierPathView.BezierPathInput bezInput)
                    {
                        sink.BeginFigure(bezInput.Start.ToRawVector2(), FigureBegin.Filled);

                        sink.AddBezier(new BezierSegment
                        {
                            Point1 = bezInput.ControlPoint1.ToRawVector2(),
                            Point2 = bezInput.ControlPoint2.ToRawVector2(),
                            Point3 = bezInput.End.ToRawVector2()
                        });

                        sink.EndFigure(FigureEnd.Open);
                    }
                }

                sink.Close();

                // Decorate
                foreach (var decorator in decorators)
                    decorator.DecorateBezierPathView(bezierView, ref state);

                if (state.FillColor != Color.Transparent)
                {
                    using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.FillColor.ToColor4()))
                    {
                        renderingState.D2DRenderTarget.FillGeometry(geom, brush);
                    }
                }

                using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.StrokeColor.ToColor4()))
                {
                    renderingState.D2DRenderTarget.DrawGeometry(geom, brush, state.StrokeWidth);
                }
                    
                sink.Dispose();
                geom.Dispose();
            });
        }

        public void RenderLabelView([NotNull] LabelView labelView, [NotNull] IDirect2DRenderingState renderingState, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            renderingState.PushingTransform(() =>
            {
                renderingState.D2DRenderTarget.Transform = new Matrix3x2(labelView.GetAbsoluteTransform().Elements);
                
                var visibleArea =
                    labelView
                        .GetFullBounds().Corners
                        .Transform(labelView.GetAbsoluteTransform()).Area();

                if (!ClippingRegion.IsVisibleInClippingRegion(visibleArea))
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
                    RadiusX = 5, RadiusY = 5,
                    Rect = new RawRectangleF(0, 0, labelView.Bounds.Width, labelView.Bounds.Height)
                };

                using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.BackgroundColor.ToColor4()))
                {
                    renderingState.D2DRenderTarget.FillRoundedRectangle(roundedRect, brush);
                }
                
                if(state.StrokeWidth > 0)
                    using (var pen = new SolidColorBrush(renderingState.D2DRenderTarget, state.StrokeColor.ToColor4()))
                    {
                        renderingState.D2DRenderTarget.DrawRoundedRectangle(roundedRect, pen, state.StrokeWidth);
                    }

                var textBounds = labelView.TextBounds;

                using (var brush = new SolidColorBrush(renderingState.D2DRenderTarget, state.TextColor.ToColor4()))
                using (var textFormat = new TextFormat(renderingState.DirectWriteFactory, labelView.TextFont.Name, labelView.TextFont.Size) { TextAlignment = TextAlignment.Leading, ParagraphAlignment = ParagraphAlignment.Center })
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

                        var prev = TextColorRenderer.DefaultBrush;
                        TextColorRenderer.DefaultBrush = brush;

                        textLayout.Draw(TextColorRenderer, textBounds.Minimum.X, textBounds.Minimum.Y);

                        TextColorRenderer.DefaultBrush = prev;
                                
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

        public void RenderBackground([NotNull] IDirect2DRenderingState renderingState)
        {
            renderingState.D2DRenderTarget.Clear(BackColor.ToColor4());

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

                            if (!ClippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(1, 0)))
                                continue;

                            renderingState.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), gridPen);
                        }

                        for (float y = startY - reg.Top % smallGridSize.Y; y <= endY; y += smallGridSize.Y)
                        {
                            var start = new Vector(reg.Left, y) * transform;
                            var end = new Vector(reg.Right, y) * transform;

                            if (!ClippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(0, 1)))
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

                        if (!ClippingRegion.IsVisibleInClippingRegion(new AABB(new[] {start, end}).Inflated(1, 0)))
                            continue;

                        renderingState.D2DRenderTarget.DrawLine(start.ToRawVector2(), end.ToRawVector2(), gridPen);
                    }

                    for (float y = startY - reg.Top % largeGridSize.Y; y <= endY; y += largeGridSize.Y)
                    {
                        var start = new Vector(reg.Left, y) * transform;
                        var end = new Vector(reg.Right, y) * transform;

                        if (!ClippingRegion.IsVisibleInClippingRegion(new AABB(new[] { start, end }).Inflated(0, 1)))
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

            decorator.Added(this);
        }

        public void RemoveDecorator(IRenderingDecorator decorator)
        {
            RenderingDecorators.Remove(decorator);

            decorator.Removed(this);
        }

        #endregion
    }
    
    /// <summary>
    /// Decorator that modifies rendering of objects in the export pipeline view.
    /// </summary>
    internal interface IRenderingDecorator
    {
        void Added([NotNull] IExportPipelineDirect2DRenderer renderer);

        void Removed([NotNull] IExportPipelineDirect2DRenderer renderer);

        void DecoratePipelineStep([NotNull] PipelineNodeView nodeView, ref PipelineStepViewState state);

        void DecoratePipelineStepInput([NotNull] PipelineNodeView nodeView, [NotNull] PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state);

        void DecoratePipelineStepOutput([NotNull] PipelineNodeView nodeView, [NotNull] PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state);

        void DecorateBezierPathView([NotNull] BezierPathView pathView, ref BezierPathViewState state);

        void DecorateLabelView([NotNull] LabelView pathView, ref LabelViewState state);
    }

    internal struct PipelineStepViewState
    {
        public int StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color TitleFillColor { get; set; }
        public Color StrokeColor { get; set; }
        public Color TitleFontColor { get; set; }
        public Color BodyFontColor { get; set; }
    }

    internal struct PipelineStepViewLinkState
    {
        public int StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color StrokeColor { get; set; }
    }

    internal struct BezierPathViewState
    {
        public int StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
    }

    internal struct LabelViewState
    {
        public int StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
    }

    internal abstract class AbstractRenderingDecorator : IRenderingDecorator
    {
        public virtual void Added(IExportPipelineDirect2DRenderer renderer)
        {

        }

        public virtual void Removed(IExportPipelineDirect2DRenderer renderer)
        {

        }

        public virtual void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
        {

        }

        public virtual void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {

        }

        public virtual void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {

        }

        public virtual void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
        {

        }

        public virtual void DecorateLabelView(LabelView pathView, ref LabelViewState state)
        {

        }
    }
}

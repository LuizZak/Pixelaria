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
using FastBitmapLib;
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
        private readonly IPipelineContainer _container; // For relative position calculations
        private readonly DefaultLabelViewSizeProvider _labelViewSizeProvider;
        private readonly IExportPipelineControl _control;

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
        
        public Direct2DRenderer(IPipelineContainer container, IExportPipelineControl control)
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
            
            _nodeTitlesTextFormat = new TextFormat(state.DirectWriteFactory, "Microsoft Sans Serif", 11);
            _nodeTitlesTextFormat.SetTextAlignment(TextAlignment.Leading);
            _nodeTitlesTextFormat.SetParagraphAlignment(ParagraphAlignment.Center);

            // Create shadow box image
            using (var bitmap = new Bitmap(64, 64))
            {
                FastBitmap.ClearBitmap(bitmap, Color.Black);
                _shadowBox = CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
            }
        }

        protected override void RecreateState(IDirect2DRenderingState state)
        {
            base.RecreateState(state);

            // Create shadow box image
            using (var bitmap = new Bitmap(64, 64))
            {
                FastBitmap.ClearBitmap(bitmap, Color.Black);
                _shadowBox = CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
            }
        }

        public override void InvalidateState()
        {
            base.InvalidateState();

            _shadowBox.Dispose();
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

        public void RenderInView([NotNull] BaseView view, [NotNull] IDirect2DRenderingState state, [NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var bezierRenderer = new BezierViewRenderer(this);

            // Render all remaining objects
            var labels = view.Children.OfType<LabelView>().ToArray();
            var beziers = view.Children.OfType<BezierPathView>().ToArray();
            var nodeViews = view.Children.OfType<PipelineNodeView>().ToArray();
            var beziersLow = beziers.Where(b => !b.RenderOnTop);
            var beziersOver = beziers.Where(b => b.RenderOnTop);

            // Under beziers
            bezierRenderer.RenderBezierViews(beziersLow, ClippingRegion, decorators);

            // Node views

            foreach (var stepView in nodeViews)
            {
                RenderStepView(stepView, state, decorators);
            }

            // Over beziers
            bezierRenderer.RenderBezierViews(beziersOver, ClippingRegion, decorators);

            // Label views
            foreach (var label in labels.Where(l => l.Visible))
            {
                RenderLabelView(label, state, decorators);
            }
        }

        private void RenderStepView([NotNull] PipelineNodeView nodeView, [NotNull] IDirect2DRenderingState state, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            state.PushingTransform(() =>
            {
                state.D2DRenderTarget.Transform = nodeView.GetAbsoluteTransform().ToRawMatrix3X2();
                    
                var visibleArea = nodeView.GetFullBounds().Corners.Transform(nodeView.GetAbsoluteTransform()).Area();
                    
                if (!ClippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;

                var disposeBag = new DisposeBag();
                
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
                
                var titleArea = nodeView.GetTitleArea();
                var linkLabelArea = nodeView.LinkViewLabelArea;

                // Create disposable objects
                var roundedRect = new RoundedRectangle
                {
                    RadiusX = 5,
                    RadiusY = 5,
                    Rect = new RawRectangleF(0, 0, bounds.Width, bounds.Height)
                };

                var roundedRectArea = new RoundedRectangleGeometry(state.D2DFactory, roundedRect);
                var bodyFillStopCollection = new GradientStopCollection(state.D2DRenderTarget, new[]
                {
                    new GradientStop {Color = stepViewState.FillColor.ToColor4(), Position = 0},
                    new GradientStop {Color = stepViewState.FillColor.Faded(Color.Black, 0.1f).ToColor4(), Position = 1}
                });
                var bodyFillGradientBrush = new LinearGradientBrush(
                    state.D2DRenderTarget,
                    new LinearGradientBrushProperties
                    {
                        StartPoint = new RawVector2(0, 0),
                        EndPoint = new RawVector2(0, bounds.Height)
                    },
                    bodyFillStopCollection);

                var textFormat = new TextFormat(state.DirectWriteFactory, nodeView.Font.Name, nodeView.Font.Size);
                textFormat.SetTextAlignment(TextAlignment.Leading);
                textFormat.SetParagraphAlignment(ParagraphAlignment.Center);

                disposeBag.AddDisposable(roundedRectArea);
                disposeBag.AddDisposable(bodyFillStopCollection);
                disposeBag.AddDisposable(bodyFillGradientBrush);
                disposeBag.AddDisposable(textFormat);

                using (var linkAreaGeom = new GeometryCombination(state.D2DFactory))
                {
                    linkAreaGeom.Combine(linkLabelArea.ToRawRectangleF(), roundedRectArea, CombineMode.Intersect);

                    // Fill for link label area
                    state.D2DRenderTarget.FillGeometry(linkAreaGeom.GetGeometry(), bodyFillGradientBrush);
                }

                // Fill for link circle areas (black borders around colored body)
                using (var stopCollection = new GradientStopCollection(state.D2DRenderTarget, new[]
                {
                    new GradientStop {Color = Color.Black.WithTransparency(0.5f).Faded(Color.White, 0.2f).ToColor4(), Position = 0},
                    new GradientStop {Color = Color.Black.WithTransparency(0.5f).ToColor4(), Position = 0.3f},
                }))
                using (var gradientBrush = new LinearGradientBrush(
                    state.D2DRenderTarget,
                    new LinearGradientBrushProperties
                    {
                        StartPoint = new RawVector2(0, 0),
                        EndPoint = new RawVector2(0, bounds.Height)
                    },
                    stopCollection))
                using (var linkAreaGeom = new GeometryCombination(state.D2DFactory))
                {
                    linkAreaGeom.Combine(roundedRectArea, linkLabelArea.ToRawRectangleF(), CombineMode.Exclude);

                    state.D2DRenderTarget.FillGeometry(linkAreaGeom.GetGeometry(), gradientBrush);
                }

                // Title background (clipped)
                using (var titleAreaGeom = new GeometryCombination(state.D2DFactory))
                {
                    var titleRect = new RawRectangleF(0, 0, titleArea.Width, titleArea.Height);
                    
                    titleAreaGeom.Combine(titleRect, roundedRectArea, CombineMode.Intersect);

                    // Fill title BG
                    using (var solidColorBrush =
                        new SolidColorBrush(state.D2DRenderTarget, stepViewState.TitleFillColor.ToColor4()))
                    {
                        state.D2DRenderTarget.FillGeometry(titleAreaGeom.GetGeometry(), solidColorBrush);
                    }
                }
                
                // Draw body outline
                using (var penBrush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.StrokeColor.ToColor4()))
                {
                    state.D2DRenderTarget.DrawRoundedRectangle(roundedRect, penBrush, stepViewState.StrokeWidth);
                }

                // Draw icon, if available
                if (nodeView.Icon != null)
                {
                    var icon = nodeView.Icon.Value;
                    
                    float imgY = titleArea.Height / 2 - (float)icon.Height / 2;

                    var imgBounds = (AABB)new RectangleF(imgY, imgY, icon.Width, icon.Height);

                    var bitmap = ImageResources.BitmapForResource(icon);
                    if (bitmap != null)
                    {
                        var mode = BitmapInterpolationMode.Linear;

                        // Draw with pixel quality when zoomed in so icon doesn't render all blurry
                        if (_container.ContentsView.LocalTransform.ScaleVector >= Vector.Unit)
                        {
                            mode = BitmapInterpolationMode.NearestNeighbor;
                        }

                        state.D2DRenderTarget.DrawBitmap(bitmap, imgBounds.ToRawRectangleF(), 1f, mode);
                    }
                }

                // Draw title text
                using (var textLayout = new TextLayout(state.DirectWriteFactory, nodeView.Name, textFormat, nodeView.TitleTextArea.Width, nodeView.TitleTextArea.Height))
                using (var brush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.TitleFontColor.ToColor4()))
                {
                    state.D2DRenderTarget.DrawTextLayout(nodeView.TitleTextArea.Minimum.ToRawVector2(), textLayout, brush, DrawTextOptions.EnableColorFont);
                }

                // Draw in-going and out-going links
                var inLinks = nodeView.InputViews;
                var outLinks = nodeView.OutputViews;
                
                // Draw outputs
                foreach (var link in outLinks)
                {
                    RenderNodeLinkView(state, link, decorators);
                }
                
                // Draw separation between input and output links
                if (inLinks.Count > 0 && outLinks.Count > 0)
                {
                    float yLine = (float)Math.Round(outLinks.Select(o => o.FrameOnParent.Bottom + 6).Max());

                    using (var brush = new SolidColorBrush(state.D2DRenderTarget, Color.Gray.WithTransparency(0.5f).ToColor4()))
                    {
                        state.D2DRenderTarget.DrawLine(new Vector(linkLabelArea.Left + 6, yLine).ToRawVector2(), new Vector(linkLabelArea.Right - 6, yLine).ToRawVector2(), brush);
                    }
                }

                // Draw inputs
                foreach (var link in inLinks)
                {
                    RenderNodeLinkView(state, link, decorators);
                }

                // Draw body text, if available
                string bodyText = nodeView.BodyText;
                if (!string.IsNullOrEmpty(bodyText))
                {
                    var area = nodeView.GetBodyTextArea();

                    float yLine = 0;

                    // Draw separation between links and body text
                    bool hasLinks = inLinks.Count > 0 || outLinks.Count > 0;
                    if (hasLinks)
                    {
                        yLine = (float)Math.Round(outLinks.Concat(inLinks).Select(o => o.FrameOnParent.Bottom + 6).Max());
                        using (var brush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.StrokeColor.WithTransparency(0.7f).ToColor4()))
                        {
                            state.D2DRenderTarget.DrawLine(new Vector(0, yLine).ToRawVector2(), new Vector(nodeView.Width, yLine).ToRawVector2(), brush);
                        }
                    }

                    // Draw fill color
                    using (var textAreaGeom = new GeometryCombination(state.D2DFactory))
                    {
                        var areaOnView = new AABB(0, hasLinks ? yLine : titleArea.Bottom, nodeView.Height, nodeView.Width);

                        textAreaGeom.Combine(areaOnView.ToRawRectangleF(), roundedRectArea, CombineMode.Intersect);

                        state.D2DRenderTarget.FillGeometry(textAreaGeom.GetGeometry(), bodyFillGradientBrush);
                    }

                    using (var textLayout = new TextLayout(state.DirectWriteFactory, bodyText, textFormat, area.Width, area.Height))
                    using (var brush = new SolidColorBrush(state.D2DRenderTarget, stepViewState.BodyFontColor.ToColor4()))
                    {
                        state.D2DRenderTarget.DrawTextLayout(area.Minimum.ToRawVector2(), textLayout, brush, DrawTextOptions.EnableColorFont);
                    }
                }

                disposeBag.Dispose();
            });
        }

        private void RenderNodeLinkView([NotNull] IDirect2DRenderingState state, [NotNull] PipelineNodeLinkView link, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            state.PushingTransform(() =>
            {
                var visibleArea = link.Bounds.TransformedBounds(link.GetAbsoluteTransform());

                if (ClippingRegion.IsVisibleInClippingRegion(visibleArea))
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

                    state.D2DRenderTarget.Transform = link.GetAbsoluteTransform().ToRawMatrix3X2();

                    var rectangle = link.Bounds;

                    using (var pen = new SolidColorBrush(state.D2DRenderTarget, linkState.StrokeColor.ToColor4()))
                    using (var brush = new SolidColorBrush(state.D2DRenderTarget, linkState.FillColor.ToColor4()))
                    {
                        var ellipse = new Ellipse(rectangle.Center.ToRawVector2(), rectangle.Width / 2,
                            rectangle.Width / 2);

                        state.D2DRenderTarget.FillEllipse(ellipse, brush);
                        state.D2DRenderTarget.DrawEllipse(ellipse, pen, linkState.StrokeWidth);
                    }
                }

                // Draw label view
                RenderLabelView(link.LinkLabel, state, decorators);
            });
        }

        public void RenderBezierView([NotNull] BezierPathView bezierView, [NotNull] IDirect2DRenderingState renderingState, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            renderingState.PushingTransform(() =>
            {
                renderingState.D2DRenderTarget.Transform = bezierView.GetAbsoluteTransform().ToRawMatrix3X2();
                    
                var visibleArea = bezierView.Bounds.TransformedBounds(bezierView.GetAbsoluteTransform());

                if (!ClippingRegion.IsVisibleInClippingRegion(visibleArea))
                    return;
                    
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

                var d2DSinkWrapper = new D2DPathSink(sink, FigureBegin.Filled);

                var path = bezierView.GetCompletePathInput();
                path.ApplyOnSink(d2DSinkWrapper);
                
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
                
                if(state.OuterStrokeWidth > 0 && state.OuterStrokeColor != Color.Transparent)
                {
                    using (var brushOuterStroke = new SolidColorBrush(renderingState.D2DRenderTarget, state.OuterStrokeColor.ToColor4()))
                    {
                        renderingState.D2DRenderTarget.DrawGeometry(geom, brushOuterStroke, state.StrokeWidth + state.OuterStrokeWidth);
                    }
                }
                
                if(state.StrokeWidth > 0 && state.StrokeColor != Color.Transparent)
                {
                    using (var brushStroke = new SolidColorBrush(renderingState.D2DRenderTarget, state.StrokeColor.ToColor4()))
                    {
                        renderingState.D2DRenderTarget.DrawGeometry(geom, brushStroke, state.StrokeWidth);
                    }
                }

                sink.Dispose();
                geom.Dispose();
            });
        }

        public void RenderLabelView([NotNull] LabelView labelView, [NotNull] IDirect2DRenderingState renderingState, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            renderingState.PushingTransform(() =>
            {
                renderingState.D2DRenderTarget.Transform = labelView.GetAbsoluteTransform().ToRawMatrix3X2();
                
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
                
                var format = new TextFormat(renderingState.DirectWriteFactory, labelView.TextFont.Name, labelView.TextFont.Size);
                format.SetTextAlignment(TextAlignment.Leading);
                format.SetParagraphAlignment(ParagraphAlignment.Center);

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

        private class D2DPathSink : IPathInputSink
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

        private sealed class GeometryCombination: IDisposable
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

        private sealed class DisposeBag : IDisposable
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
        public float StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color TitleFillColor { get; set; }
        public Color StrokeColor { get; set; }
        public Color TitleFontColor { get; set; }
        public Color BodyFontColor { get; set; }
    }

    internal struct PipelineStepViewLinkState
    {
        public float StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color StrokeColor { get; set; }
    }

    internal struct BezierPathViewState
    {
        public float StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
        public float OuterStrokeWidth { get; set; }
        public Color OuterStrokeColor { get; set; }
    }

    internal struct LabelViewState
    {
        public float StrokeWidth { get; set; }
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

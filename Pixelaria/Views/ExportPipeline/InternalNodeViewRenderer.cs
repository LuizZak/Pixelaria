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
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixRendering;
using PixUI;

namespace Pixelaria.Views.ExportPipeline
{
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
            Renderer.PushTransform(_nodeView.LocalTransform);

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

            Renderer.SetFillColor(stepViewState.BodyFontColor);
            Renderer.DrawAttributedText((AttributedText)bodyText, textFormatAttributes, BodyTextArea);
        }

        private void DrawTitleText(TextFormatAttributes textFormatAttributes, PipelineStepViewState stepViewState)
        {
            Renderer.SetFillColor(stepViewState.TitleFontColor);
            Renderer.DrawAttributedText((AttributedText) _nodeView.Name, textFormatAttributes, _nodeView.TitleTextArea);
        }

        private void DrawIcon(AABB titleArea)
        {
            if (_nodeView.Icon == null && _nodeView.ManagedIcon == null)
                return;

            var iconSize = _nodeView.ManagedIcon?.Size ?? _nodeView.Icon.Value.Size;

            float imgY = titleArea.Height / 2 - (float)iconSize.Height / 2;
            var imgBounds = (AABB)new RectangleF(imgY, imgY, iconSize.Width, iconSize.Height);

            var mode = ImageInterpolationMode.Linear;

            // Draw with pixel quality when zoomed in so icon doesn't render all blurry
            if (_useNearestNeighborOnIcon)
            {
                mode = ImageInterpolationMode.NearestNeighbor;
            }

            if (_nodeView.ManagedIcon != null)
            {
                Renderer.DrawBitmap(_nodeView.ManagedIcon, (RectangleF)imgBounds, 1, mode);
            }
            else if (_nodeView.Icon != null)
            {
                Renderer.DrawBitmap(_nodeView.Icon.Value, (RectangleF)imgBounds, 1, mode);
            }
        }

        private void DrawNodeLinkView([NotNull] PipelineNodeLinkView link, [ItemNotNull, NotNull] IReadOnlyList<IRenderingDecorator> decorators)
        {
            var clippingRegion = _parameters.ClippingRegion;

            Renderer.PushTransform(link.LocalTransform);

            if (clippingRegion.IsVisibleInClippingRegion(link.BoundsForInvalidateFullBounds(), link))
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
                    if (link is PipelineNodeInputLinkView)
                        decorator.DecoratePipelineStepInput(link.NodeView, link, ref linkState);
                    else
                        decorator.DecoratePipelineStepOutput(link.NodeView, link, ref linkState);
                }

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

            renderer.PushingTransform(labelView.LocalTransform, () =>
            {
                if (!clippingRegion.IsVisibleInClippingRegion(labelView.BoundsForInvalidateFullBounds(), labelView))
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

                renderer.SetFillColor(state.TextColor);
                renderer.DrawAttributedText(labelView.AttributedText, format, textBounds);
            });
        }
    }
}
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
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Utils;
using PixRendering;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Brush = SharpDX.Direct2D1.Brush;
using Factory = SharpDX.DirectWrite.Factory;
using TextRange = SharpDX.DirectWrite.TextRange;

namespace PixDirectX.Rendering.DirectX
{
    internal class Direct2DTextRenderer : ITextRenderer
    {
        public TextColorRenderer TextColorRenderer { get; }
        private readonly Factory _directWriteFactory;
        private readonly RenderTarget _renderTarget;

        /// <summary>
        /// TODO: ITextRenderers should take in IBrush instances; for now, we set custom brushes this way here.
        /// </summary>
        public Brush Brush { get; set; }

        public Direct2DTextRenderer(TextColorRenderer textColorRenderer, Factory directWriteFactory, RenderTarget renderTarget)
        {
            TextColorRenderer = textColorRenderer;
            _directWriteFactory = directWriteFactory;
            _renderTarget = renderTarget;
        }

        public void Draw(ITextLayout textLayout, float x, float y)
        {
            if (!(textLayout is Direct2DRenderManager.InnerTextLayout layout))
                return;

            // Render background segments
            var backSegments =
                textLayout.Text.GetTextSegments()
                    .Where(seg => seg.HasAttribute<BackgroundColorAttribute>());

            foreach (var segment in backSegments)
            {
                var attr = segment.GetAttribute<BackgroundColorAttribute>();

                var metrics = layout.TextLayout.HitTestTextRange(segment.TextRange.Start, segment.TextRange.Length, 0, 0);

                var bounds = metrics.Select(range => AABB.FromRectangle(range.Left, range.Top, range.Width, range.Height));

                using (var backBrush = new SolidColorBrush(_renderTarget, attr.BackColor.ToColor4()))
                {
                    foreach (var aabb in bounds)
                    {
                        _renderTarget.FillRectangle(aabb.Inflated(attr.Inflation).ToRawRectangleF(), backBrush);
                    }
                }
            }

            layout.TextLayout.Draw(TextColorRenderer, x, y);
        }

        public void Draw(IAttributedText text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            EllipsisTrimming trimming = null;

            using (var textFormat = TextFormatForAttributes(textFormatAttributes, ref trimming))
            using (var textLayout = new TextLayout(_directWriteFactory, text.String, textFormat, area.Width, area.Height))
            {
                var disposes = new List<IDisposable>();

                // Apply text attributes
                foreach (var textSegment in text.GetTextSegments())
                {
                    var consumer = new CollectingTextAttributeConsumer();
                    textSegment.ConsumeAttributes(consumer);

                    if (consumer.ForeColor.HasValue)
                    {
                        var segmentBrush =
                            new SolidColorBrush(_renderTarget, consumer.ForeColor.Value.ToColor4());

                        disposes.Add(segmentBrush);

                        textLayout.SetDrawingEffect(segmentBrush,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }
                    if (consumer.Font != null)
                    {
                        textLayout.SetFontFamilyName(consumer.Font.FontFamily.Name,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                        textLayout.SetFontSize(consumer.Font.Size,
                            new TextRange(textSegment.TextRange.Start, textSegment.TextRange.Length));
                    }
                }

                // Render background segments
                var backSegments =
                    text.GetTextSegments()
                        .Where(seg => seg.HasAttribute<BackgroundColorAttribute>());

                foreach (var segment in backSegments)
                {
                    var attr = segment.GetAttribute<BackgroundColorAttribute>();

                    var metrics = textLayout.HitTestTextRange(segment.TextRange.Start, segment.TextRange.Length, 0, 0);

                    var bounds = metrics.Select(range => AABB.FromRectangle(range.Left, range.Top, range.Width, range.Height));

                    using (var backBrush = new SolidColorBrush(_renderTarget, attr.BackColor.ToColor4()))
                    {
                        foreach (var aabb in bounds)
                        {
                            _renderTarget.FillRectangle(aabb.Inflated(attr.Inflation).ToRawRectangleF(), backBrush);
                        }
                    }
                }

                var brush = Brush ?? new SolidColorBrush(_renderTarget, color.ToColor4());

                textLayout.Draw(brush, TextColorRenderer, area.Left, area.Top);

                foreach (var disposable in disposes)
                {
                    disposable.Dispose();
                }

                if (Brush == null)
                {
                    brush.Dispose();
                }
            }
        }

        public void Draw(string text, TextFormatAttributes textFormatAttributes, AABB area, Color color)
        {
            EllipsisTrimming trimming = null;

            using (var foreground = new SolidColorBrush(_renderTarget, color.ToColor4()))
            using (var textFormat = TextFormatForAttributes(textFormatAttributes, ref trimming))
            {
                _renderTarget.DrawText(text, textFormat, area.ToRawRectangleF(), foreground);
            }

            trimming?.Dispose();
        }

        private TextFormat TextFormatForAttributes(TextFormatAttributes textFormatAttributes, ref EllipsisTrimming trimming)
        {
            var textFormat = new TextFormat(_directWriteFactory, textFormatAttributes.Font, textFormatAttributes.FontSize)
            {
                WordWrapping = Direct2DConversionHelpers.DirectWriteWordWrapFor(textFormatAttributes.WordWrap),
                TextAlignment = Direct2DConversionHelpers.DirectWriteAlignmentFor(textFormatAttributes.HorizontalTextAlignment),
                ParagraphAlignment = Direct2DConversionHelpers.DirectWriteAlignmentFor(textFormatAttributes.VerticalTextAlignment)
            };

            if (textFormatAttributes.TextEllipsisTrimming.HasValue)
            {
                trimming = new EllipsisTrimming(_directWriteFactory, textFormat);
                textFormat.SetTrimming(Direct2DRenderManager.CreateTrimming(textFormatAttributes.TextEllipsisTrimming.Value), trimming);
            }

            return textFormat;
        }
    }
}
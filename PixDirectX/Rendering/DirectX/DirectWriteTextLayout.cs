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

using JetBrains.Annotations;
using PixCore.Text;
using PixRendering;
using SharpDX.DirectWrite;
using HitTestMetrics = PixRendering.HitTestMetrics;

namespace PixDirectX.Rendering.DirectX
{
    public class DirectWriteTextLayout : ITextLayout
    {
        [CanBeNull]
        private readonly TextFormat _textFormat;
        public TextLayout TextLayout { get; }
        [CanBeNull]
        public EllipsisTrimming EllipsisTrimming { get; }
        public TextLayoutAttributes Attributes { get; }
        public IAttributedText Text { get; }

        public DirectWriteTextLayout(Factory directWriteFactory, [NotNull] IAttributedText text, TextLayoutAttributes attributes)
        {
            _textFormat = new TextFormat(directWriteFactory, attributes.TextFormatAttributes.Font, attributes.TextFormatAttributes.FontSize);
            TextLayout = new TextLayout(directWriteFactory, text.String, _textFormat, attributes.AvailableWidth, attributes.AvailableHeight);
            Attributes = attributes;
            Text = text;
        }

        public DirectWriteTextLayout(TextLayout textLayout, EllipsisTrimming ellipsisTrimming, TextLayoutAttributes attributes)
        {
            TextLayout = textLayout;
            EllipsisTrimming = ellipsisTrimming;
            Attributes = attributes;
        }

        public void Dispose()
        {
            _textFormat?.Dispose();
            TextLayout?.Dispose();
            EllipsisTrimming?.Dispose();
        }

        public HitTestMetrics HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside)
        {
            var metrics = TextLayout.HitTestPoint(x, y, out var trailing, out var inside);
            isTrailingHit = trailing;
            isInside = inside;

            return MetricsFromDirectWrite(metrics);
        }

        public HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y)
        {
            var metrics = TextLayout.HitTestTextPosition(textPosition, isTrailingHit, out x, out y);
            return MetricsFromDirectWrite(metrics);
        }

        private static HitTestMetrics MetricsFromDirectWrite(SharpDX.DirectWrite.HitTestMetrics metrics)
        {
            return new HitTestMetrics(metrics.TextPosition);
        }
    }
}
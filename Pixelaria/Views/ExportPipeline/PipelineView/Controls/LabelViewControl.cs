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
using Pixelaria.Views.ModelViews;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using Font = System.Drawing.Font;
using FontFamily = System.Drawing.FontFamily;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A basic textual label
    /// </summary>
    internal class LabelViewControl : ControlView
    {
        private string _text = "";
        private Font _textFont = new Font(FontFamily.GenericSansSerif, 11);

        [NotNull]
        public Font TextFont
        {
            get => _textFont;
            set
            {
                _textFont = value;
                CalculateBounds();
            }
        }

        [NotNull]
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;

                _text = value;

                CalculateBounds();
            }
        }
        
        private void CalculateBounds()
        {
            Size = LabelView.DefaultLabelViewSizeProvider.CalculateTextSize(Text, TextFont);
        }

        public override void RenderForeground(Direct2DRenderingState state)
        {
            using (var brush = new SolidColorBrush(state.D2DRenderTarget, ForeColor.ToColor4()))
            using (var textFormat = new TextFormat(state.DirectWriteFactory, _textFont.FontFamily.Name, _textFont.Size))
            {
                state.D2DRenderTarget.DrawText(_text, textFormat, Bounds, brush, DrawTextOptions.Clip);
            }
        }
    }
}
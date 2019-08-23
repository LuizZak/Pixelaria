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

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace PixDirectX.Rendering.DirectX
{
    /// <summary>
    /// For rendering colored texts on a D2DRenderer
    /// </summary>
    public class TextColorRenderer : TextRendererBase
    {
        private RenderTarget _renderTarget;
        public Brush DefaultBrush { get; set; }

        public TextColorRenderer()
        {
            // BUG fix for issue described at: https://github.com/sharpdx/SharpDX/issues/1019
            CppObject.ToCallbackPtr<TextColorRenderer>(this);
        }

        public void AssignResources(RenderTarget renderTarget, Brush defaultBrush)
        {
            _renderTarget = renderTarget;
            DefaultBrush = defaultBrush;
        }

        public override Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
        {
            var sb = DefaultBrush;
            if (clientDrawingEffect is SolidColorBrush brush)
                sb = brush;
            
            try
            {
                _renderTarget.DrawGlyphRun(new Vector2(baselineOriginX, baselineOriginY), glyphRun, sb, measuringMode);
                return Result.Ok;
            }
            catch
            {
                return Result.Fail;
            }
        }
    }
}
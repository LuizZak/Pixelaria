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

using SharpGen.Runtime;
using System;
using System.Numerics;
using Vortice.DirectWrite;

using RenderTarget = Vortice.Direct2D1.ID2D1RenderTarget;
using Brush = Vortice.Direct2D1.ID2D1Brush;
using SolidColorBrush = Vortice.Direct2D1.ID2D1SolidColorBrush;
using Vortice.DCommon;

namespace PixDirectX.Rendering.DirectX
{
    /// <summary>
    /// For rendering colored texts on a D2DRenderer
    /// </summary>
    public class TextColorRenderer : CallbackBase, IDWriteTextRenderer
    {
        private RenderTarget _renderTarget;
        public Brush DefaultBrush { get; set; }

        public TextColorRenderer()
        {

        }

        public void AssignResources(RenderTarget renderTarget, Brush defaultBrush)
        {
            _renderTarget = renderTarget;
            DefaultBrush = defaultBrush;
        }

        public RawBool IsPixelSnappingDisabled(IntPtr clientDrawingContext)
        {
            return false;
        }

        public Matrix3x2 GetCurrentTransform(IntPtr clientDrawingContext)
        {
            return Matrix3x2.Identity;
        }

        public float GetPixelsPerDip(IntPtr clientDrawingContext)
        {
            return 1.0f;
        }

        public void DrawGlyphRun(IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, IUnknown clientDrawingEffect)
        {
            var sb = DefaultBrush;
            if (clientDrawingContext != IntPtr.Zero)
                sb = new SolidColorBrush(clientDrawingContext);

            try
            {
                _renderTarget.DrawGlyphRun(new Vector2(baselineOriginX, baselineOriginY), glyphRun, sb, measuringMode);
            }
            catch
            {

            }
        }

        public void DrawUnderline(IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, IUnknown clientDrawingEffect)
        {
            
        }

        public void DrawStrikethrough(IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, IUnknown clientDrawingEffect)
        {
            
        }

        public void DrawInlineObject(IntPtr clientDrawingContext, float originX, float originY, IDWriteInlineObject inlineObject, RawBool isSideways, RawBool isRightToLeft, IUnknown clientDrawingEffect)
        {
            
        }
    }
}
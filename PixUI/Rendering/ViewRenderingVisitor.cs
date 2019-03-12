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
using PixDirectX.Utils;
using PixUI.Controls;
using PixUI.Visitor;
using SharpDX.Direct2D1;

namespace PixUI.Rendering
{
    /// <summary>
    /// A visitor that handles rendering of nested self-rendering views on a base view hierarchy.
    /// </summary>
    public class ViewRenderingVisitor : IBaseViewVisitor<ControlRenderingContext>
    {
        public void OnVisitorEnter([NotNull] ControlRenderingContext context, BaseView view)
        {
            context.State.PushMatrix(view.LocalTransform.ToRawMatrix3X2());

            // Clip rendering area
            if (view is SelfRenderingBaseView selfRendering && selfRendering.ClipToBounds)
            {
                var clip = view.Bounds;
                context.RenderTarget.PushAxisAlignedClip(clip.ToRawRectangleF(), AntialiasMode.Aliased);
            }
        }

        public void VisitView([NotNull] ControlRenderingContext context, BaseView view)
        {
            if (view is SelfRenderingBaseView selfRendering && selfRendering.IsVisibleOnScreen())
            {
                selfRendering.Render(context);
            }
        }

        public bool ShouldVisitView(ControlRenderingContext state, BaseView view)
        {
            if (!(view is SelfRenderingBaseView selfRendering)) 
                return true;

            if (selfRendering.ClipToBounds && !state.ClippingRegion.IsVisibleInClippingRegion(selfRendering.Bounds, selfRendering))
                return false;

            return selfRendering.Visible;

        }

        public void OnVisitorExit([NotNull] ControlRenderingContext context, BaseView view)
        {
            if (view is SelfRenderingBaseView selfRendering && selfRendering.ClipToBounds)
            {
                context.RenderTarget.PopAxisAlignedClip();
            }

            context.State.PopMatrix();
        }
    }
}

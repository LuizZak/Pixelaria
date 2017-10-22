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

using System.Drawing;
using JetBrains.Annotations;
using Pixelaria.Views.ModelViews.PipelineView;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Public interface for the Export Pipeline's Direct2D renderer
    /// </summary>
    public interface IDirect2DRenderer : ILabelViewSizeProvider
    {
        void AddImageResource([NotNull] Direct2DRenderingState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);
        void RemoveImageResource([NotNull] string resourceName);
        void RemoveImageResources();

        /// <summary>
        /// Shortcut for creating and assigning a bitmap to use in pipeline node view's icons and other image resources
        /// </summary>
        PipelineNodeView.ImageResource AddPipelineNodeImageResource([NotNull] Direct2DRenderingState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);

        /// <summary>
        /// Shortcut for fetching an image resource formatted as a pipeline node view's image resource struct.
        /// 
        /// Returns null, if no resource with the given name is found.
        /// </summary>
        PipelineNodeView.ImageResource? PipelineNodeImageResource([NotNull] string resourceName);
    }
}
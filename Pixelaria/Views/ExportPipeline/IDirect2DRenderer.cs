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

using System.Drawing;
using JetBrains.Annotations;
using Pixelaria.Views.ExportPipeline.PipelineView;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Public interface for the Export Pipeline's Direct2D renderer
    /// </summary>
    internal interface IDirect2DRenderer : ILabelViewSizeProvider
    {
        /// <summary>
        /// Gets or sets the background color that this Direct2DRenderer uses to clear the display area
        /// </summary>
        Color BackColor { get; set; }

        /// <summary>
        /// Gets the imaage resources manager for this D2DRenderer
        /// </summary>
        ID2DImageResourceManager ImageResources { get; }

        void AddDecorator(IRenderingDecorator decorator);

        void RemoveDecorator(IRenderingDecorator decorator);
        
        void PushTemporaryDecorator(IRenderingDecorator decorator);
    }

    /// <summary>
    /// Interface for objects capable of creating, updating, providing and destroying Direct2D bitmap resources.
    /// </summary>
    internal interface ID2DImageResourceManager: ID2DImageResourceProvider
    {
        void AddImageResource([NotNull] Direct2DRenderingState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);
        void RemoveImageResource([NotNull] string resourceName);
        void RemoveImageResources();

        /// <summary>
        /// Shortcut for creating and assigning a bitmap to use in pipeline node view's icons and other image resources
        /// </summary>
        ImageResource AddPipelineNodeImageResource([NotNull] Direct2DRenderingState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);
    }

    /// <summary>
    /// Interface for objects that can provide encapsulated access to image resources.
    /// </summary>
    internal interface ID2DImageResourceProvider
    {
        /// <summary>
        /// Fetches an image resource formatted as an image resource struct.
        /// 
        /// Returns null, if no resource with the given name is found.
        /// </summary>
        ImageResource? PipelineNodeImageResource([NotNull] string resourceName);

        /// <summary>
        /// Gets a Direct2D bitmap object for a matching image resource from this provider.
        /// 
        /// Returns null, if resource could not be found.
        /// </summary>
        [CanBeNull]
        SharpDX.Direct2D1.Bitmap BitmapForResource(ImageResource resource);

        /// <summary>
        /// Gets a Direct2D bitmap object for an image resource matching a given name from this provider.
        /// 
        /// Returns null, if resource could not be found.
        /// </summary>
        [CanBeNull]
        SharpDX.Direct2D1.Bitmap BitmapForResource([NotNull] string named);
    }
}
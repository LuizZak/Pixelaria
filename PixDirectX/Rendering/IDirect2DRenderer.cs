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
using System.Drawing;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixCore.Text;
using SharpDX;
using SharpDX.DirectWrite;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Public interface for the Export Pipeline's Direct2D renderer
    /// </summary>
    public interface IDirect2DRenderer
    {
        /// <summary>
        /// Gets or sets the background color that this Direct2D renderer uses to clear the display area
        /// </summary>
        Color BackColor { get; set; }

        /// <summary>
        /// Gets the imaage resources manager for this Direct2D renderer
        /// </summary>
        ID2DImageResourceManager ImageResources { get; }
        
        /// <summary>
        /// The clipping region for this Direct2D renderer
        /// </summary>
        IClippingRegion ClippingRegion { get; }

        /// <summary>
        /// Using a given attributed string, prepares the given <see cref="TextLayout"/> and calls
        /// the closure to allow the caller to perform rendering operations with the prepared text layout.
        /// </summary>
        void WithPreparedTextLayout(Color4 textColor, [NotNull] IAttributedText text, [NotNull] TextLayout layout, [NotNull, InstantHandle] Action<TextLayout, TextRendererBase> perform);
    }

    /// <summary>
    /// Represents a common interface for an object that provides clipping region querying for a <see cref="IDirect2DRenderer"/>.
    /// </summary>
    public interface IClippingRegion
    {
        /// <summary>
        /// Returns true if a section of <see cref="rectangle"/> is visible on the clipping region.
        /// </summary>
        bool IsVisibleInClippingRegion(Rectangle rectangle);

        /// <summary>
        /// Returns true if <see cref="point"/> is contained within the clipping region.
        /// </summary>
        bool IsVisibleInClippingRegion(Point point);

        /// <summary>
        /// Returns true if a section of <see cref="aabb"/> is visible on the clipping region.
        /// </summary>
        bool IsVisibleInClippingRegion(AABB aabb);

        /// <summary>
        /// Returns true if <see cref="point"/> is contained within the clipping region.
        /// </summary>
        bool IsVisibleInClippingRegion(Vector point);

        /// <summary>
        /// Returns true if a section of <see cref="aabb"/> is visible on the clipping region when transformed
        /// on a given reference point to screen-space.
        /// </summary>
        bool IsVisibleInClippingRegion(AABB aabb, [NotNull] ISpatialReference reference);

        /// <summary>
        /// Returns true if <see cref="point"/> is contained within the clipping region when transformed on a
        /// given reference point to screen-space.
        /// </summary>
        bool IsVisibleInClippingRegion(Vector point, [NotNull] ISpatialReference reference);
    }

    /// <summary>
    /// Interface for objects capable of creating, updating, providing and destroying Direct2D bitmap resources.
    /// </summary>
    public interface ID2DImageResourceManager: ID2DImageResourceProvider
    {
        void AddImageResource([NotNull] IDirect2DRenderingState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);
        void RemoveImageResource([NotNull] string resourceName);
        void RemoveImageResources();

        /// <summary>
        /// Shortcut for creating and assigning a bitmap to use in pipeline node view's icons and other image resources
        /// </summary>
        ImageResource AddPipelineNodeImageResource([NotNull] IDirect2DRenderingState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);
    }

    /// <summary>
    /// Interface for objects that can provide encapsulated access to image resources.
    /// </summary>
    public interface ID2DImageResourceProvider
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
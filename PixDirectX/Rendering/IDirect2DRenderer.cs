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
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Public interface for the Export Pipeline's Direct2D renderer
    /// </summary>
    public interface IDirect2DRenderer : ITextLayoutRenderer
    {
        /// <summary>
        /// Gets or sets the background color that this Direct2D renderer uses to clear the display area
        /// </summary>
        Color BackColor { get; set; }

        /// <summary>
        /// Gets the image resources manager for this Direct2D renderer
        /// </summary>
        IImageResourceManager ImageResources { get; }

        /// <summary>
        /// An object specialized in calculating text metrics using the latest available rendering state of this <see cref="IDirect2DRenderer"/>
        /// </summary>
        ITextMetricsProvider TextMetricsProvider { get; }
        
        /// <summary>
        /// The clipping region for this Direct2D renderer
        /// </summary>
        IClippingRegion ClippingRegion { get; }

        /// <summary>
        /// Updates the rendering state and clipping region of this Direct2D renderer instance to the ones specified.
        /// 
        /// Must be called whenever devices/surfaces/etc. have been invalidated or the clipping region has been changed.
        /// </summary>
        void UpdateRenderingState([NotNull] IRenderLoopState state, [NotNull] IClippingRegion clipping);

        /// <summary>
        /// Adds a new render listener which will be invoked when rendering is being performed.
        /// </summary>
        void AddRenderListener(IRenderListener renderListener);

        /// <summary>
        /// Removes a render listener registered on this renderer.
        /// </summary>
        void RemoveRenderListener(IRenderListener renderListener);
    }

    public interface ITextLayoutRenderer
    {
        /// <summary>
        /// Using a given attributed string, prepares the given <see cref="ITextLayout"/> and calls
        /// the closure to allow the caller to perform rendering operations with the prepared text layout.
        /// </summary>
        void WithPreparedTextLayout(Color textColor, [NotNull] IAttributedText text, [CanBeNull] ref ITextLayout layout, TextLayoutAttributes attributes, [NotNull, InstantHandle] Action<ITextLayout, ITextRenderer> perform);

        /// <summary>
        /// Creates a new text layout using a given set of attributes.
        /// </summary>
        ITextLayout CreateTextLayout([NotNull] IAttributedText text, TextLayoutAttributes attributes);
    }

    /// <summary>
    /// A reference to a text layout that was created by an invocation to <see cref="ITextLayoutRenderer"/>.
    /// </summary>
    public interface ITextLayout: IDisposable
    {
        /// <summary>
        /// Gets the text attributes for this text layout instance.
        /// </summary>
        TextLayoutAttributes Attributes { get; }

        /// <summary>
        /// Performs a hit test operation at a given location on this text layout, relative to the top-left location of the layout box.
        /// </summary>
        /// <param name="x">The pixel location X to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="y">The pixel location Y to hit-test, relative to the top-left location of the layout box.</param>
        /// <param name="isTrailingHit">An output flag that indicates whether the hit-test location is at the leading or the trailing side of the character. When the output <em>*isInside</em> value is set to <strong>false</strong>, this value is set according to the output <em>HitTestMetrics.TextPosition</em> value to represent the edge closest to the hit-test location.</param>
        /// <param name="isInside">The output geometry fully enclosing the hit-test location. When the output <em>*isInside</em> value is set to <strong>false</strong>, this structure represents the geometry enclosing the edge closest to the hit-test location.</param>
        /// <returns>A struct representing the result of the hit test.</returns>
        HitTestMetrics HitTestPoint(float x, float y, out bool isTrailingHit, out bool isInside);

        /// <summary>
        /// The application calls this function to get the pixel location relative to the top-left of the layout box given the text position and the logical side of the position. This function is normally used as part of caret positioning of text where the caret is drawn at the location corresponding to the current text editing position. It may also be used as a way to programmatically obtain the geometry of a particular text position in UI automation.
        /// </summary>
        /// <param name="textPosition">The text position used to get the pixel location.</param>
        /// <param name="isTrailingHit">A Boolean flag that indicates whether the pixel location is of the leading or the trailing side of the specified text position.</param>
        /// <param name="x">When this method returns, contains the output pixel location X, relative to the top-left location of the layout box.</param>
        /// <param name="y">When this method returns, contains the output pixel location Y, relative to the top-left location of the layout box.</param>
        /// <returns>When this method returns, contains the output geometry fully enclosing the specified text position.</returns>
        HitTestMetrics HitTestTextPosition(int textPosition, bool isTrailingHit, out float x, out float y);
    }

    /// <summary>
    /// Metrics returned when hit testing an <see cref="ITextLayout"/> instance.
    /// </summary>
    public struct HitTestMetrics
    {
        public int TextPosition { get; }

        public HitTestMetrics(int textPosition)
        {
            TextPosition = textPosition;
        }
    }

    /// <summary>
    /// An interface for a text renderer object.
    /// </summary>
    public interface ITextRenderer
    {
        /// <summary>
        /// Draws a text layout at a given location on the current render target.
        /// </summary>
        void Draw(ITextLayout textLayout, float x, float y);

        /// <summary>
        /// Draws a string of text with a given set of attributes, on a given area with a given color.
        /// </summary>
        void Draw(string text, TextFormatAttributes textFormatAttributes, AABB area, Color color);
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
    public interface IImageResourceManager: IImageResourceProvider
    {
        /// <summary>
        /// Creates a new image from a given <see cref="Bitmap"/> instance, assigns it with a given resource name, and
        /// then returns it to be used.
        /// </summary>
        ImageResource AddImageResource([NotNull] IRenderLoopState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);

        void RemoveImageResource([NotNull] string resourceName);

        void RemoveAllImageResources();
    }

    /// <summary>
    /// Interface for objects that can provide encapsulated access to image resources.
    /// </summary>
    public interface IImageResourceProvider
    {
        /// <summary>
        /// Fetches an image resource formatted as an image resource struct.
        /// 
        /// Returns null, if no resource with the given name is found.
        /// </summary>
        ImageResource? GetImageResource([NotNull] string resourceName);
    }
}
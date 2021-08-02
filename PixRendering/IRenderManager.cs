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

namespace PixRendering
{
    /// <summary>
    /// Public interface for the Export Pipeline's renderer manager
    /// </summary>
    public interface IRenderManager : ITextLayoutRenderer
    {
        /// <summary>
        /// Gets or sets the background color that this renderer manager uses to clear the display area
        /// </summary>
        Color BackColor { get; set; }

        /// <summary>
        /// Gets the image resources manager for this renderer manager
        /// </summary>
        IImageResourceManager ImageResources { get; }

        /// <summary>
        /// An object specialized in calculating text metrics using the latest available rendering state of this <see cref="IRenderManager"/>
        /// </summary>
        ITextMetricsProvider TextMetricsProvider { get; }

        /// <summary>
        /// Gets the text size provider for this render manager.
        /// </summary>
        ITextSizeProvider TextSizeProvider { get; }

        /// <summary>
        /// The clipping region for this renderer manager
        /// </summary>
        IClippingRegion ClippingRegion { get; }

        /// <summary>
        /// Initializes this render manager with a given rendering state.
        /// </summary>
        void Initialize([NotNull] IRenderLoopState state);

        /// <summary>
        /// Updates the rendering state and clipping region of this renderer manager instance to the ones specified.
        /// 
        /// Must be called whenever devices/surfaces/etc. have been invalidated or the clipping region has been changed.
        /// </summary>
        void UpdateRenderingState([NotNull] IRenderLoopState state, [NotNull] IClippingRegion clipping);

        /// <summary>
        /// Renders all render listeners on this <see cref="IRenderManager"/> instance.
        ///
        /// If overriden, must be called to properly update the render state of the renderer.
        /// </summary>
        void Render([NotNull] IRenderLoopState renderLoopState, [NotNull] IClippingRegion clipping);

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
        /// Gets the string associated with this text layout.
        /// </summary>
        IAttributedText Text { get; }

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
    public readonly struct HitTestMetrics
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
        void Draw([NotNull] ITextLayout textLayout, float x, float y);

        /// <summary>
        /// Draws an attributed text with a given set of attributes, on a given area with a given color.
        /// </summary>
        void Draw([NotNull] IAttributedText text, TextFormatAttributes textFormatAttributes, AABB area, Color color);

        /// <summary>
        /// Draws a string of text with a given set of attributes, on a given area with a given color.
        /// </summary>
        void Draw([NotNull] string text, TextFormatAttributes textFormatAttributes, AABB area, Color color);
    }

    /// <summary>
    /// Represents a common interface for an object that provides clipping region querying for a <see cref="IRenderManager"/>.
    /// </summary>
    public interface IClippingRegion
    {
        /// <summary>
        /// Returns a series of <see cref="RectangleF"/> instances that approximate the redraw region
        /// of this <see cref="IClippingRegion"/>, truncated to be within the given <see cref="Size"/>-d rectangle.
        /// </summary>
        RectangleF[] RedrawRegionRectangles(Size size);

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
}
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

namespace PixRendering
{
    /// <summary>
    /// Describes an object that can participate in the main render sequence of an <see cref="IRenderManager"/>.
    /// </summary>
    public interface IRenderListener
    {
        /// <summary>
        /// Gets an integer that specifies the ordering for invoking this render listener in relation to other render listeners.
        /// <br/>
        /// <br/>
        /// Render listeners with a higher render order are invoked before listeners with a lower render order.
        /// <br/>
        /// <br/>
        /// If two listeners share the same render order, the order in which they where registered in the <see cref="IRenderManager"/>
        /// is used to break the tie, instead.
        /// <br/>
        /// <br/>
        /// This value is read only when adding the render listener to a <see cref="IRenderManager"/>; updating this value at runtime is
        /// not supported and may lead to undesired render ordering of elements.
        /// </summary>
        int RenderOrder { get; }

        /// <summary>
        /// (Re-)initializes this render listener with a given render state.
        ///
        /// This method is invoked by a renderer after a render listener is added, as soon as a valid render state is available, as well as
        /// when previous render states have been invalidated and need to be recreated, using a given render state as a new state basis.
        /// </summary>
        void RecreateState([NotNull] IRenderLoopState state);

        /// <summary>
        /// Called to notify a new rendering process will take place on an <see cref="IRenderManager"/>, with a given valid render state
        /// associated.
        /// </summary>
        void Render([NotNull] IRenderListenerParameters parameters);
    }

    /// <summary>
    /// Specifies all required parameters for invoking render listeners
    /// </summary>
    public interface IRenderListenerParameters
    {
        /// <summary>
        /// Gets the image resources manager for managing image resources for rendering
        /// </summary>
        [NotNull]
        IImageResourceManager ImageResources { get; }

        /// <summary>
        /// The clipping region for drawing on
        /// </summary>
        [NotNull]
        IClippingRegion ClippingRegion { get; }

        /// <summary>
        /// The currently active renderer
        /// </summary>
        [NotNull]
        IRenderer Renderer { get; }

        /// <summary>
        /// The latest valid rendering state to use
        /// </summary>
        [NotNull]
        IRenderLoopState State { get; }

        /// <summary>
        /// Gets a text layout renderer
        /// </summary>
        [NotNull]
        ITextLayoutRenderer TextLayoutRenderer { get; }

        /// <summary>
        /// Gets the text renderer
        /// </summary>
        [NotNull]
        ITextRenderer TextRenderer { get; }

        /// <summary>
        /// Gets the text metrics provider
        /// </summary>
        [NotNull]
        ITextMetricsProvider TextMetricsProvider { get; }
    }
}

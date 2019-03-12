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

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Describes an object that can participate in the main render sequence of an <see cref="IDirect2DRenderer"/>.
    /// </summary>
    public interface IRenderListener
    {
        /// <summary>
        /// Gets an integer that specifies the ordering for invoking this render listener in relation to other render listeners.
        ///
        /// Render listeners with a higher render order are invoked before listeners with a lower render order.
        ///
        /// If two listeners share the same render order, the order in which they where registered in the <see cref="IDirect2DRenderer"/>
        /// is used to break the tie, instead.
        /// </summary>
        int RenderOrder { get; }

        /// <summary>
        /// Called to notify a new rendering process will take place on an <see cref="IDirect2DRenderer"/>, with a given valid render state
        /// associated.
        /// </summary>
        void Render(IDirect2DRenderingState state, [NotNull] IClippingRegion clipping);
    }
}

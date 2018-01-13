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

namespace PixUI
{
    /// <summary>
    /// Defines an object that is capable of having its contents invalidated.
    /// </summary>
    public interface IRegionInvalidateable
    {
        /// <summary>
        /// Invalidates the entirety of this objects's drawing region on its parent.
        /// 
        /// The invalidation is propagated through the parent view chain until the root
        /// view, which may handle it in ways such as invalidating a window screen region.
        /// </summary>
        void Invalidate();

        /// <summary>
        /// Invalidates the entirety of this view's drawing region on its parent, including
        /// bounds of all of its hierarchy. See <see cref="BaseView.GetFullBounds"/>.
        /// 
        /// The invalidation is propagated through the parent view chain until the root
        /// view, which may handle it in ways such as invalidating a window screen region.
        /// </summary>
        void InvalidateFullBounds();
    }
}
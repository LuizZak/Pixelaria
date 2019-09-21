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

using Pixelaria.Data;

namespace Pixelaria.Controllers
{
    /// <summary>
    /// Interface for objects that can provide information about 
    /// </summary>
    public interface IInterfaceStateProvider
    {
        /// <summary>
        /// Returns whether, to the knowledge of this interface state provider, the given animation
        /// is currently opened in a view with pending changes to save
        /// </summary>
        /// <returns>A value specifying whether the animation has unsaved changes in any view this interface state provider is able to reach</returns>
        bool HasUnsavedChangesForAnimation(Animation animation);

        /// <summary>
        /// Returns whether, to the knowledge of this interface state provider, the given animation sheet
        /// is currently opened in a view with pending changes to save
        /// </summary>
        /// <returns>A value specifying whether the animation sheet has unsaved changes in any view this interface state provider is able to reach</returns>
        bool HasUnsavedChangesForAnimationSheet(AnimationSheet sheet);
    }
}
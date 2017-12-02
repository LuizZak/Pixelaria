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

namespace PixUI.Visitor
{
    /// <summary>
    /// Interface for objects that deal with visiting of base view instances with their own logic.
    /// </summary>
    public interface IBaseViewVisitor<in T>
    {
        /// <summary>
        /// Called when the visitor first arrives at a view
        /// </summary>
        void OnVisitorEnter(T state, [NotNull] BaseView view);

        /// <summary>
        /// Called to apply a visit logic to a view
        /// </summary>
        void VisitView(T state, [NotNull] BaseView view);

        /// <summary>
        /// Called when the last child of a view has been visited and traversal will 
        /// continue up the siblings/parent chain
        /// </summary>
        void OnVisitorExit(T state, [NotNull] BaseView view);
    }
}
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

namespace Pixelaria.PixUI.Visitor
{
    /// <summary>
    /// A visitor that walks through a hierarchy of <see cref="BaseView"/> instances, visiting each child view
    /// recursively and passing them to a base view visitor, along with a custom shared state.
    /// </summary>
    internal sealed class BaseViewTraverser<T>
    {
        private readonly T _state;
        private readonly IBaseViewVisitor<T> _viewVisitor;

        public BaseViewTraverser(T state, [NotNull] IBaseViewVisitor<T> viewVisitor)
        {
            _state = state;
            _viewVisitor = viewVisitor;
        }

        public void Visit([NotNull] BaseView view)
        {
            _viewVisitor.OnVisitorEnter(_state, view);

            _viewVisitor.VisitView(_state, view);

            // Render children
            foreach (var child in view.Children)
            {
                Visit(child);
            }

            _viewVisitor.OnVisitorExit(_state, view);
        }
    }
}
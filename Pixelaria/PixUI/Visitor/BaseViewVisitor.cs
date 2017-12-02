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

namespace Pixelaria.PixUI.Visitor
{
    /// <summary>
    /// A generic implementation of <see cref="IBaseViewVisitor{T}"/> that calls a closure on each call to
    /// <see cref="IBaseViewVisitor{T}.VisitView"/>.
    /// </summary>
    internal class BaseViewVisitor<T> : IBaseViewVisitor<T>
    {
        private readonly Action<T, BaseView> _onVisit;

        internal BaseViewVisitor(Action<T, BaseView> onVisit)
        {
            _onVisit = onVisit;
        }

        public void OnVisitorEnter(T state, BaseView view)
        {

        }

        public void VisitView(T state, BaseView view)
        {
            _onVisit(state, view);
        }

        public void OnVisitorExit(T state, BaseView view)
        {

        }
    }
}
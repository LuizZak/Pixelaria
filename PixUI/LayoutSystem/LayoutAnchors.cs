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

namespace PixUI.LayoutSystem
{
    /// <summary>
    /// Returns layout anchors for a <see cref="BaseView"/> or <see cref="LayoutGuide"/>
    /// </summary>
    public class LayoutAnchors
    {
        private readonly ILayoutVariablesContainer _target;
        
        public LayoutAnchor Top => new LayoutAnchor(_target, LayoutAnchorKind.Top);
        public LayoutAnchor Left => new LayoutAnchor(_target, LayoutAnchorKind.Left);
        public LayoutAnchor Right => new LayoutAnchor(_target, LayoutAnchorKind.Right);
        public LayoutAnchor Bottom => new LayoutAnchor(_target, LayoutAnchorKind.Bottom);
        public LayoutAnchor Width => new LayoutAnchor(_target, LayoutAnchorKind.Width);
        public LayoutAnchor Height => new LayoutAnchor(_target, LayoutAnchorKind.Height);

        internal LayoutAnchors(ILayoutVariablesContainer target)
        {
            _target = target;
        }
    }
}
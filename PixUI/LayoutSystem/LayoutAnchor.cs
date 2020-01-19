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
using System.Diagnostics;
using Cassowary;

namespace PixUI.LayoutSystem
{
    /// <summary>
    /// A layout anchor in a view
    /// </summary>
    [DebuggerDisplay("[{Target}:{AnchorKind}]")]
    public class LayoutAnchor
    {
        public BaseView Target { get; }

        public LayoutAnchorKind AnchorKind { get; }

        internal LayoutAnchorOrientationFlags Orientation
        {
            get
            {
                switch (AnchorKind)
                {
                    case LayoutAnchorKind.Left:
                    case LayoutAnchorKind.Right:
                    case LayoutAnchorKind.Width:
                        return LayoutAnchorOrientationFlags.Horizontal;
                    case LayoutAnchorKind.Top:
                    case LayoutAnchorKind.Bottom:
                    case LayoutAnchorKind.Height:
                        return LayoutAnchorOrientationFlags.Vertical;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal LayoutAnchor(BaseView target, LayoutAnchorKind anchorKind)
        {
            Target = target;
            AnchorKind = anchorKind;
        }

        internal ClAbstractVariable Variable()
        {
            switch (AnchorKind)
            {
                case LayoutAnchorKind.Left:
                    return Target.LayoutVariables.Left;
                case LayoutAnchorKind.Top:
                    return Target.LayoutVariables.Top;
                case LayoutAnchorKind.Right:
                    return Target.LayoutVariables.Right;
                case LayoutAnchorKind.Bottom:
                    return Target.LayoutVariables.Bottom;
                case LayoutAnchorKind.Width:
                    return Target.LayoutVariables.Width;
                case LayoutAnchorKind.Height:
                    return Target.LayoutVariables.Height;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// Returns layout anchors for a view
    /// </summary>
    public class LayoutAnchors
    {
        private readonly BaseView _target;
        
        public LayoutAnchor Top => new LayoutAnchor(_target, LayoutAnchorKind.Top);
        public LayoutAnchor Left => new LayoutAnchor(_target, LayoutAnchorKind.Left);
        public LayoutAnchor Right => new LayoutAnchor(_target, LayoutAnchorKind.Right);
        public LayoutAnchor Bottom => new LayoutAnchor(_target, LayoutAnchorKind.Bottom);
        public LayoutAnchor Width => new LayoutAnchor(_target, LayoutAnchorKind.Width);
        public LayoutAnchor Height => new LayoutAnchor(_target, LayoutAnchorKind.Height);

        internal LayoutAnchors(BaseView target)
        {
            _target = target;
        }
    }
}
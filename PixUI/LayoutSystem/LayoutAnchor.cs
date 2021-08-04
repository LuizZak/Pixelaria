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
    [DebuggerDisplay("[{Owner}:{AnchorKind}]")]
    public readonly struct LayoutAnchor
    {
        internal readonly ILayoutVariablesContainer container;
        
        public LayoutAnchorKind AnchorKind { get; }

        /// <summary>
        /// Gets the generic owner of this layout anchor.
        /// </summary>
        public object Owner { get; }

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

        internal LayoutAnchor(ILayoutVariablesContainer target, LayoutAnchorKind anchorKind)
        {
            container = target;
            Owner = target;
            AnchorKind = anchorKind;
        }

        internal ClAbstractVariable Variable()
        {
            switch (AnchorKind)
            {
                case LayoutAnchorKind.Left:
                    return container.LayoutVariables.Left;
                case LayoutAnchorKind.Top:
                    return container.LayoutVariables.Top;
                case LayoutAnchorKind.Right:
                    return container.LayoutVariables.Right;
                case LayoutAnchorKind.Bottom:
                    return container.LayoutVariables.Bottom;
                case LayoutAnchorKind.Width:
                    return container.LayoutVariables.Width;
                case LayoutAnchorKind.Height:
                    return container.LayoutVariables.Height;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal ClLinearExpression RelativeExpression(ILayoutVariablesContainer relativeContainer)
        {
            switch (AnchorKind)
            {
                case LayoutAnchorKind.Left:
                    return new ClLinearExpression(relativeContainer.LayoutVariables.Left);
                case LayoutAnchorKind.Top:
                    return new ClLinearExpression(relativeContainer.LayoutVariables.Top);
                case LayoutAnchorKind.Right:
                    return new ClLinearExpression(relativeContainer.LayoutVariables.Left);
                case LayoutAnchorKind.Bottom:
                    return new ClLinearExpression(relativeContainer.LayoutVariables.Bottom);
                case LayoutAnchorKind.Width:
                    return new ClLinearExpression(0);
                case LayoutAnchorKind.Height:
                    return new ClLinearExpression(0);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool Equals(LayoutAnchor other)
        {
            return ReferenceEquals(container, other.container) && AnchorKind == other.AnchorKind;
        }

        public override bool Equals(object obj)
        {
            return obj is LayoutAnchor other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((container != null ? container.GetHashCode() : 0) * 397) ^ (int)AnchorKind;
            }
        }

        public static bool operator ==(LayoutAnchor left, LayoutAnchor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LayoutAnchor left, LayoutAnchor right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"[{container}:{AnchorKind}]";
        }
    }
}
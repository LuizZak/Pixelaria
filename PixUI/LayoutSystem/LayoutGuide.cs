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

using System.Collections.Generic;
using JetBrains.Annotations;
using PixCore.Geometry;

namespace PixUI.LayoutSystem
{
    /// <summary>
    /// A rectangular area that can interact with the layout constraint system.
    /// </summary>
    public class LayoutGuide : ILayoutVariablesContainer
    {
        /// <summary>
        /// The view that this layout guide is currently contained within, or <c>null</c>, in case it is not
        /// in a view hierarchy currently.
        /// </summary>
        [CanBeNull]
        internal BaseView ownerView;

        private readonly LayoutVariables _layoutVariables;

        LayoutVariables ILayoutVariablesContainer.LayoutVariables => _layoutVariables;

        public ISpatialReference ParentSpatialReference => ownerView;

        public AABB FrameOnParent { get; set; }

        public BaseView ViewInHierarchy => ownerView;

        List<LayoutConstraint> ILayoutVariablesContainer.AffectingConstraints { get; } = new List<LayoutConstraint>();

        public LayoutGuide()
        {
            _layoutVariables = new LayoutVariables(this);
        }

        public BaseView ViewForFirstBaseline()
        {
            return null;
        }

        public void SetNeedsLayout()
        {
            ownerView?.SetNeedsLayout();
        }

        AABB ILayoutVariablesContainer.BoundsForRedrawOnScreen()
        {
            return FrameOnParent;
        }
    }
}

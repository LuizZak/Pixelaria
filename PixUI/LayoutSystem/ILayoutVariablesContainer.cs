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
    /// Common interface for objects that have an area on a hierarchy view and can be constrained by the constraint system.
    /// </summary>
    internal interface ILayoutVariablesContainer
    {
        /// <summary>
        /// The layout variables for this layout variables container.
        /// </summary>
        LayoutVariables LayoutVariables { get; }

        /// <summary>
        /// A parent spatial reference for this layout variable container.
        ///
        /// Usually the parent view, for <see cref="BaseView"/> instances, and the container view for <see cref="LayoutGuide"/> instances.
        /// </summary>
        [CanBeNull]
        ISpatialReference ParentSpatialReference { get; }

        /// <summary>
        /// The frame of this layout container within its parent object.
        ///
        /// Returns a 0-based rectangle, in case this object is not contained within a hierarchy.
        /// </summary>
        AABB FrameOnParent { get; }

        /// <summary>
        /// Gets the view that contains this variables container.
        ///
        /// Usually <c>this</c> for <see cref="BaseView"/> instances, and the container view for <see cref="LayoutGuide"/> instances.
        /// </summary>
        [CanBeNull]
        BaseView ViewInHierarchy { get; }

        /// <summary>
        /// List of constraints that are directly affecting this layout variables container.
        /// </summary>
        List<LayoutConstraint> AffectingConstraints { get; }

        /// <summary>
        /// The view to use for Y baseline calculations.
        ///
        /// Not every view has a baseline height, although <see cref="LabelView"/> instances always have a baseline which is tied to their font settings.
        /// </summary>
        [CanBeNull]
        BaseView ViewForFirstBaseline();

        /// <summary>
        /// Marks this layout variables container as requiring a re-layout.
        /// </summary>
        void SetNeedsLayout();

        /// <summary>
        /// Returns the bounds for the redraw rectangle of this container on its parent's coordinate system.
        /// 
        /// Returns a 0-based rectangle, in case this object is not contained within a hierarchy.
        /// </summary>
        AABB BoundsForRedrawOnScreen();
    }
}

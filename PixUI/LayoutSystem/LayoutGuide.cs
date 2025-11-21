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
    public class LayoutGuide : ISpatialReference, ILayoutAnchorsContainer, ILayoutVariablesContainer
    {
        /// <summary>
        /// The view that this layout guide is currently contained within, or <c>null</c>, in case it is not
        /// in a view hierarchy currently.
        /// </summary>
        [CanBeNull]
        internal BaseView ownerView;

        internal readonly LayoutVariables layoutVariables;

        LayoutVariables ILayoutVariablesContainer.LayoutVariables => layoutVariables;

        internal Matrix2D Transform => Matrix2D.Translation(FrameOnParent.Left, FrameOnParent.Top);

        public ISpatialReference ParentSpatialReference => ownerView;

        public AABB FrameOnParent { get; set; }

        public BaseView ViewInHierarchy => ownerView;

        public LayoutAnchors Anchors { get; }

        List<LayoutConstraint> ILayoutVariablesContainer.AffectingConstraints { get; } = new List<LayoutConstraint>();

        public IReadOnlyList<LayoutConstraint> AffectingConstraints => ((ILayoutVariablesContainer)this).AffectingConstraints;

        public LayoutGuide()
        {
            layoutVariables = new LayoutVariables(this);
            Anchors = new LayoutAnchors(this);
        }

        public BaseView ViewForFirstBaseline()
        {
            return null;
        }

        public void SetNeedsLayout()
        {
            ownerView?.SetNeedsLayout();
        }
        
        /// <summary>
        /// Converts a point from a given <see cref="ISpatialReference"/>'s local coordinates to this
        /// layout guide's coordinates.
        /// 
        /// If <see cref="from"/> is null, converts from screen coordinates.
        /// </summary>
        public Vector ConvertFrom(Vector point, ISpatialReference from)
        {
            // Convert point to global, if it's currently local to 'from'
            var global = point;
            if (from != null)
                global = from.GetAbsoluteTransform() * point;

            var matrix = GetAbsoluteTransform().Inverted();
            return global * matrix;
        }

        /// <summary>
        /// Converts a point from this <see cref="ISpatialReference"/>'s local coordinates to a given
        /// layout guide's coordinates.
        /// 
        /// If <see cref="to"/> is null, converts from this node to screen coordinates.
        /// </summary>
        public Vector ConvertTo(Vector point, ISpatialReference to)
        {
            if (to == null)
                return point * GetAbsoluteTransform();

            return to.ConvertFrom(point, this);
        }

        /// <summary>
        /// Converts an AABB from a given <see cref="ISpatialReference"/>'s local coordinates to this
        /// layout guide's coordinates.
        /// 
        /// If <see cref="from"/> is null, converts from screen coordinates.
        /// </summary>
        public AABB ConvertFrom(AABB aabb, ISpatialReference from)
        {
            // Convert point to global, if it's currently local to from
            var global = aabb;
            if (from != null)
                global = aabb.TransformedBounds(from.GetAbsoluteTransform());

            var matrix = GetAbsoluteTransform().Inverted();
            return global.TransformedBounds(matrix);
        }

        /// <summary>
        /// Converts an AABB from this <see cref="ISpatialReference"/>'s local coordinates to a given
        /// layout guide's coordinates.
        /// 
        /// If <see cref="to"/> is null, converts from this node to screen coordinates.
        /// </summary>
        public AABB ConvertTo(AABB aabb, ISpatialReference to)
        {
            if (to == null)
                return aabb.TransformedBounds(GetAbsoluteTransform());

            return to.ConvertFrom(aabb, this);
        }

        /// <summary>
        /// Gets the absolute matrix for this layout guide's coordinates system.
        /// 
        /// Multiplying a point (0, 0) by this matrix results in a point that lands on
        /// the top-left corner of this layout guide's coordinate system.
        /// </summary>
        public Matrix2D GetAbsoluteTransform()
        {
            if (ownerView == null)
            {
                return Transform;
            }

            var total = ownerView.GetAbsoluteTransform();
            var local = Transform;

            return Matrix2D.Multiply(in local, in total);
        }
        
        AABB ILayoutVariablesContainer.BoundsForRedrawOnScreen()
        {
            return FrameOnParent;
        }
    }
}
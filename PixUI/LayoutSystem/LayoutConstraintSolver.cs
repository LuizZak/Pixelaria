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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cassowary;
using JetBrains.Annotations;
using PixUI.Visitor;

namespace PixUI.LayoutSystem
{
    /// <summary>
    /// View layout constraint solver
    /// </summary>
    internal sealed class LayoutConstraintSolver
    {
        public void Solve([NotNull] BaseView view)
        {
            var visitor = new BaseViewVisitor<LayoutConstraintTraversalResult>((constraintList, baseView) =>
            {
                constraintList.AffectedViews.Add(baseView.LayoutVariables);

                constraintList.HorizontalConstraints.AddRange(
                    baseView.LayoutConstraints.Where(lc => lc.FirstAnchor.Orientation == LayoutAnchorOrientationFlags.Horizontal)
                );

                constraintList.VerticalConstraints.AddRange(
                    baseView.LayoutConstraints.Where(lc => lc.FirstAnchor.Orientation == LayoutAnchorOrientationFlags.Vertical)
                );
            });

            var result = new LayoutConstraintTraversalResult();
            var traverser = new BaseViewTraverser<LayoutConstraintTraversalResult>(result, visitor);

            traverser.Visit(view);

            Solve(result.HorizontalConstraints, result.AffectedViews, LayoutAnchorOrientationFlags.Horizontal);
            Solve(result.VerticalConstraints, result.AffectedViews, LayoutAnchorOrientationFlags.Vertical);

            foreach (var affectedViewVariables in result.AffectedViews)
            {
                affectedViewVariables.ApplyVariables();
            }
        }

        private static void Solve([NotNull] IEnumerable<LayoutConstraint> constraints, [NotNull] IEnumerable<ViewLayoutConstraintVariables> affectedViews, LayoutAnchorOrientationFlags orientation)
        {
            var solver = new ClSimplexSolver();

            foreach (var affectedView in affectedViews)
            {
                affectedView.AddVariables(solver, orientation);
                affectedView.BuildConstraints(solver, orientation);
            }

            foreach (var constraint in constraints)
            {
                constraint.BuildConstraints(solver);
            }

            try
            {
                solver.Solve();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error while solving layout constraints: {e}");
                throw;
            }
        }

        private class LayoutConstraintTraversalResult
        {
            public readonly List<ViewLayoutConstraintVariables> AffectedViews = new List<ViewLayoutConstraintVariables>();

            public readonly List<LayoutConstraint> HorizontalConstraints = new List<LayoutConstraint>();
            public readonly List<LayoutConstraint> VerticalConstraints = new List<LayoutConstraint>();
        }
    }
}
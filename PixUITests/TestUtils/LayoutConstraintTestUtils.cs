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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixUI;
using PixUI.LayoutSystem;

namespace PixUITests.TestUtils
{
    /// <summary>
    /// Test utilities for testing layout constraints on views
    /// </summary>
    public class LayoutConstraintTestUtils
    {
        private readonly BaseView _view;

        public LayoutConstraintTestUtils(BaseView view)
        {
            _view = view;
        }

        /// <summary>
        /// Asserts that the view associated with this <see cref="LayoutConstraintTestUtils"/> constraints the two given anchors.
        ///
        /// This assertion only checks the constraints available within <see cref="BaseView.LayoutConstraints"/>.
        /// </summary>
        public void Constrains(LayoutAnchor anchor1, LayoutAnchor anchor2, LayoutRelationship relationship = LayoutRelationship.Equal, float constant = 0)
        {
            foreach (var constraint in _view.LayoutConstraints)
            {
                if (!ConstraintConstrains(constraint, anchor1, anchor2))
                    continue;

                if (constraint.Relationship == relationship)
                {
                    Assert.AreEqual(constant, constraint.Constant, $"Expected constraint on anchors {anchor1} <-> {anchor2} to have constant of {constant}, received {constraint.Constant}");
                    return;
                }
            }

            Assert.Fail($"Could not find constraint constraining {anchor1} to {anchor2} in {_view} with relationship {relationship}");
        }

        /// <summary>
        /// Asserts that the view associated with this <see cref="LayoutConstraintTestUtils"/> constraints a given anchor.
        ///
        /// This assertion only checks the constraints available within <see cref="BaseView.LayoutConstraints"/>.
        /// </summary>
        public void Constrains(LayoutAnchor anchor, LayoutRelationship relationship = LayoutRelationship.Equal, float constant = 0)
        {
            foreach (var constraint in _view.LayoutConstraints)
            {
                if (!ConstraintConstrains(constraint, anchor))
                    continue;

                if (constraint.Relationship == relationship)
                {
                    Assert.AreEqual(constant, constraint.Constant, $"Expected constraint on anchor {anchor} to have constant of {constant}, received {constraint.Constant}");
                    return;
                }
            }

            Assert.Fail($"Could not find constraint constraining {anchor} in {_view} with relationship {relationship}");
        }

        /// <summary>
        /// Asserts that the view associated with this <see cref="LayoutConstraintTestUtils"/> has a constraint between an anchor point within and an anchor point of another view.
        ///
        /// This assertion only checks the constraints available within <see cref="BaseView.AffectingConstraints"/>.
        ///
        /// If neither <see cref="anchor1"/> nor <see cref="anchor2"/> are anchors of the view associated with this <see cref="LayoutConstraintTestUtils"/>, an assertion is raised.
        /// </summary>
        public void IsConstrained(LayoutAnchor anchor1, LayoutAnchor anchor2, LayoutRelationship relationship = LayoutRelationship.Equal, float constant = 0)
        {
            if (Equals(_view, anchor1.Target) && Equals(_view, anchor2.Target))
            {
                Assert.Fail($"Expected one of the anchors to belong to view {_view}");
                return;
            }

            foreach (var constraint in _view.AffectingConstraints)
            {
                if (!ConstraintConstrains(constraint, anchor1, anchor2))
                    continue;

                if (constraint.Relationship == relationship)
                {
                    Assert.AreEqual(constant, constraint.Constant, $"Expected constraint to have constant of {constant}, received {constraint.Constant}");
                }
            }

            Assert.Fail($"Could not find constraint constraining {anchor1} to {anchor2} in {_view} with relationship {relationship}");
        }

        /// <summary>
        /// Asserts that the view associated with this <see cref="LayoutConstraintTestUtils"/> has a constant constraint on a given anchor.
        ///
        /// This assertion only checks the constraints available within <see cref="BaseView.AffectingConstraints"/>.
        ///
        /// If <see cref="anchor"/> is not an anchor of the view associated with this <see cref="LayoutConstraintTestUtils"/>, an assertion is raised.
        /// </summary>
        public void IsConstrained(LayoutAnchor anchor, LayoutRelationship relationship = LayoutRelationship.Equal, float constant = 0)
        {
            if (Equals(_view, anchor.Target))
            {
                Assert.Fail($"Expected anchor {anchor} to belong to view {_view}");
                return;
            }

            foreach (var constraint in _view.AffectingConstraints)
            {
                if (!ConstraintConstrains(constraint, anchor))
                    continue;

                if (constraint.Relationship == relationship)
                {
                    Assert.AreEqual(constant, constraint.Constant, $"Expected constraint to have constant of {constant}, received {constraint.Constant}");
                }
            }

            Assert.Fail($"Could not find constraint constraining {anchor} in {_view} with relationship {relationship}");
        }

        private static bool ConstraintConstrains([NotNull] LayoutConstraint constraint, LayoutAnchor anchor1, LayoutAnchor anchor2)
        {
            return (constraint.FirstAnchor == anchor1 && constraint.SecondAnchor == anchor2) || (constraint.FirstAnchor == anchor2 && constraint.SecondAnchor == anchor1);
        }

        private static bool ConstraintConstrains([NotNull] LayoutConstraint constraint, LayoutAnchor anchor)
        {
            return constraint.FirstAnchor == anchor && constraint.SecondAnchor == null;
        }
    }

    public static class LayoutConstraintTestUtilsAssert
    {
        [Pure]
        public static LayoutConstraintTestUtils Layout(this Assert assert, BaseView view)
        {
            return new LayoutConstraintTestUtils(view);
        }
    }
}

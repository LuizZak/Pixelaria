﻿/*
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI;
using PixUI.LayoutSystem;

namespace PixUITests
{
    /// <summary>
    /// Tests for <see cref="BaseView"/> class
    /// </summary>
    [TestClass]
    public class BaseViewTests
    {
        [TestMethod]
        public void TestAbsoluteMatrix()
        {
            var root = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();

            root.AddChild(child);
            child.AddChild(grandchild);

            root.Location = new Vector(1, 1);
            root.Scale = new Vector(0.5f, 0.5f);
            child.Scale = new Vector(0.5f, 0.5f);
            child.Rotation = (float)Math.PI;

            var matrix = grandchild.GetAbsoluteTransform();

            var actual =
                Matrix2D.Rotation((float)Math.PI) * Matrix2D.Scaling(0.5f, 0.5f) * Matrix2D.Translation(0, 0) *
                Matrix2D.Rotation(0) * Matrix2D.Scaling(0.5f, 0.5f) * Matrix2D.Translation(1, 1);

            Assert.AreEqual(matrix, actual);
        }

        [TestMethod]
        public void TestPointConversionToScreen()
        {
            var root = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();

            root.AddChild(child);
            child.AddChild(grandchild);

            root.Scale = new Vector(0.5f, 0.5f);
            child.Location = new Vector(1, 1);
            //grandchild.LocalTransform.Rotate(-90);

            var pt1 = root.ConvertTo(new Vector(5f,5f), null);
            var pt2 = child.ConvertTo(new Vector(5f, 5f), null);
            //var pt3 = grandchild.ConvertTo(new Vector(5f, 5f), null);

            Assert.AreEqual(new Vector(2.5f, 2.5f), pt1);
            Assert.AreEqual(new Vector(3f, 3f), pt2);
            //Assert.AreEqual(new Vector(3f, -2f), Point.Round(pt3));
        }

        [TestMethod]
        public void TestPointConversionFromScreen()
        {
            var root = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();

            root.AddChild(child);
            child.AddChild(grandchild);

            root.Scale = new Vector(0.5f, 0.5f);
            child.Location = new Vector(1, 1);
            //grandchild.LocalTransform.Rotate(-90);

            var pt1 = root.ConvertFrom(new Vector(2.5f, 2.5f), null);
            var pt2 = child.ConvertFrom(new Vector(3f, 3f), null);
            //var pt3 = grandchild.ConvertFrom(new Vector(3f, -2f), null);

            Assert.AreEqual(new Vector(5f, 5f), pt1);
            Assert.AreEqual(new Vector(5, 5), pt2);
            //Assert.AreEqual(new Vector(5, 5), Point.Round(pt3));
        }

        [TestMethod]
        public void TestIsDescendentOf()
        {
            var sut = new BaseView();
            var parent = new BaseView();
            parent.AddChild(sut);

            Assert.IsTrue(sut.IsDescendentOf(parent));
        }

        [TestMethod]
        public void TestIsDescendentOfFalse()
        {
            var sut = new BaseView();
            var nonParent = new BaseView();

            Assert.IsFalse(sut.IsDescendentOf(nonParent));
        }

        [TestMethod]
        public void TestIsDescendentOfIndirectParent()
        {
            var sut = new BaseView();
            var parent = new BaseView();
            var grandparent = new BaseView();
            parent.AddChild(sut);
            grandparent.AddChild(parent);

            Assert.IsTrue(sut.IsDescendentOf(grandparent));
        }

        [TestMethod]
        public void TestCommonAncestor()
        {
            var parent = new BaseView();
            var child1 = new BaseView();
            var child2 = new BaseView();
            var subChild1 = new BaseView();
            var unrelated = new BaseView();
            parent.AddChild(child1);
            parent.AddChild(child2);
            child1.AddChild(subChild1);

            Assert.AreEqual(child1.CommonAncestor(child2), parent);
            Assert.AreEqual(subChild1.CommonAncestor(child2), parent);
            Assert.AreEqual(child2.CommonAncestor(subChild1), parent);
            Assert.AreEqual(parent.CommonAncestor(child1), parent);
            Assert.AreEqual(parent.CommonAncestor(parent), parent);
            Assert.IsNull(child1.CommonAncestor(unrelated));
        }

        [TestMethod]
        public void TestAddChildRecursiveHierarchyVerification()
        {
            var root = new BaseView();
            var newParent = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();
            child.AddChild(grandchild);
            root.AddChild(child);

            newParent.AddChild(child);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddChildRecursiveHierarchyVerificationFailure()
        {
            var root = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();
            child.AddChild(grandchild);
            root.AddChild(child);

            grandchild.AddChild(root);
        }

        [TestMethod]
        public void TestInsertChildRecursiveHierarchyVerification()
        {
            var root = new BaseView();
            var newParent = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();
            child.AddChild(grandchild);
            root.AddChild(child);

            newParent.InsertChild(0, child);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestInsertChildRecursiveHierarchyVerificationFailure()
        {
            var root = new BaseView();
            var child = new BaseView();
            var grandchild = new BaseView();
            child.AddChild(grandchild);
            root.AddChild(child);

            grandchild.InsertChild(0, root);
        }

        [TestMethod]
        public void TestRemoveChild_RemovesAffectedConstraints()
        {
            var sut = new BaseView();
            var child = new BaseView();
            sut.AddChild(child);
            LayoutConstraint.Create(sut.Anchors.Left, child.Anchors.Left);

            sut.RemoveChild(child);

            Assert.AreEqual(0, sut.LayoutConstraints.Count);
            Assert.AreEqual(0, sut.AffectingConstraints.Count);
            Assert.AreEqual(0, child.LayoutConstraints.Count);
            Assert.AreEqual(0, child.AffectingConstraints.Count);
        }

        [TestMethod]
        public void TestRemoveChild_DoesNotRemoveChildOnlyConstraints()
        {
            var sut = new BaseView();
            var child = new BaseView();
            var subChild = new BaseView();
            child.AddChild(subChild);
            sut.AddChild(child);
            LayoutConstraint.Create(subChild.Anchors.Left, child.Anchors.Left);

            sut.RemoveChild(child);
            
            Assert.AreEqual(1, child.LayoutConstraints.Count);
            Assert.AreEqual(1, child.AffectingConstraints.Count);
            Assert.AreEqual(0, subChild.LayoutConstraints.Count);
            Assert.AreEqual(1, subChild.AffectingConstraints.Count);
        }

        [TestMethod]
        public void TestInvalidate()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var child = new BaseView { Size = new Vector(100, 100) };
            root.AddChild(child);
            root._ResetInvalidation();

            child.Invalidate();

            root.AssertViewBoundsWhereInvalidated(child);
        }

        #region Invalidation Triggers

        [TestMethod]
        public void TestResizeInvalidatesView()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var child = new BaseView { Size = new Vector(100, 100) };
            var grandchild = new BaseView { Location = new Vector(5, 5), Size = new Vector(200, 200) };
            child.AddChild(grandchild);
            root.AddChild(child);
            root._ResetInvalidation();

            child.Size = new Vector(150, 150);

            root.AssertViewBoundsWhereInvalidated(child);
        }

        [TestMethod]
        public void TestRelocationInvalidatesView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);
            root.AddChild(child);
            root._ResetInvalidation();

            child.Location = new Vector(10, 10);
            
            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        [TestMethod]
        public void TestAddChildInvalidatesView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);

            root.AddChild(child);

            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        [TestMethod]
        public void TestAddChildInvalidatesPreviousParentView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var newParent = new BaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);
            root.AddChild(child);
            root._ResetInvalidation();

            newParent.AddChild(child);

            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        [TestMethod]
        public void TestInsertChildInvalidatesView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);

            root.InsertChild(0, child);

            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        [TestMethod]
        public void TestInsertChildInvalidatesPreviousParentView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var newParent = new BaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);
            root.AddChild(child);
            root._ResetInvalidation();

            newParent.InsertChild(0, child);

            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        [TestMethod]
        public void TestRemoveChildInvalidatesView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);
            root.AddChild(child);
            root._ResetInvalidation();

            child.RemoveFromParent();

            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        #endregion

        [TestMethod]
        public void TestRemoveConstraints()
        {
            var sut = new BaseView();
            var child = new BaseView();
            sut.AddChild(child);
            LayoutConstraint.Create(sut.Anchors.Top, child.Anchors.Top);

            sut.RemoveConstraints();

            Assert.AreEqual(0, sut.LayoutConstraints.Count);
            Assert.AreEqual(0, sut.AffectingConstraints.Count);
            Assert.AreEqual(0, child.LayoutConstraints.Count);
            Assert.AreEqual(0, child.AffectingConstraints.Count);
        }

        [TestMethod]
        public void TestRemoveConstraints_DoesNotAffectChildViews()
        {
            var sut = new BaseView();
            var child1 = new BaseView();
            var child2 = new BaseView();
            sut.AddChild(child1);
            sut.AddChild(child2);
            LayoutConstraint.Create(child1.Anchors.Top, child2.Anchors.Top);

            sut.RemoveConstraints();
            
            Assert.AreEqual(1, sut.LayoutConstraints.Count);
            Assert.AreEqual(1, child1.AffectingConstraints.Count);
            Assert.AreEqual(1, child2.AffectingConstraints.Count);
        }
    }
}

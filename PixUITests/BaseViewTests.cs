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
using System.Drawing.Drawing2D;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI;

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

            root.Scale = new Vector(0.5f, 0.5f);
            child.Scale = new Vector(0.5f, 0.5f);

            var matrix = grandchild.GetAbsoluteTransform();

            var actual = new Matrix();
            actual.Scale(0.5f, 0.5f);
            actual.Scale(0.5f, 0.5f);

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
        public void TestInvalidate()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var child = new BaseView { Size = new Vector(100, 100) };
            root.AddChild(child);
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

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
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

            child.Size = new Vector(150, 150);

            root.AssertViewBoundsWhereInvalidated(root);
        }

        [TestMethod]
        public void TestRelocationInvalidatesView()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var child = new BaseView {Size = new Vector(100, 100)};
            var grandchild = new BaseView {Location = new Vector(5, 5), Size = new Vector(200, 200)};
            child.AddChild(grandchild);
            root.AddChild(child);
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

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
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

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
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

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
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

            child.RemoveFromParent();

            root.AssertViewFullBoundsWhereInvalidated(child);
        }

        #endregion
    }
}

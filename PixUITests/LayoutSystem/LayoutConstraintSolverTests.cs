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

using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI.LayoutSystem;
using PixUI;
using PixUI.Controls;
using PixUITests.TestUtils;

namespace PixUITests.LayoutSystem
{
    [TestClass]
    public class LayoutConstraintSolverTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.TextLayoutRenderer = new TestDirect2DRenderManager();
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
        }

        [TestMethod]
        public void TestSolve()
        {
            // Create a simple root view 300x200, with a 120x80 button and a autosized, 40-height label side by side.
            // The width of the label should be dictated by its left constraint to the button and right constraint to
            // the container.

            // Arrange
            var sut = new LayoutConstraintSolver();
            var root = new BaseView
            {
                TranslateBoundsIntoConstraints = true, 
                Size = new Vector(300, 200)
            };
            var button = ButtonControl.Create();
            var label = LabelViewControl.Create("This is a label");
            button.TranslateBoundsIntoConstraints = false;
            label.TranslateBoundsIntoConstraints = false;
            root.AddChild(button);
            root.AddChild(label);
            LayoutConstraint.Create(button.Anchors.Top, root.Anchors.Top, constant: 25);
            LayoutConstraint.Create(button.Anchors.Left, root.Anchors.Left, constant: 25);
            LayoutConstraint.Create(button.Anchors.Width, constant: 120);
            LayoutConstraint.Create(button.Anchors.Height, constant: 80);
            LayoutConstraint.Create(label.Anchors.Left, button.Anchors.Right, constant: 15);
            LayoutConstraint.Create(label.Anchors.Top, root.Anchors.Top, constant: 25);
            LayoutConstraint.Create(root.Anchors.Right, label.Anchors.Right, constant: 10);
            LayoutConstraint.Create(label.Anchors.Height, constant: 40);

            // Act
            sut.Solve(root);

            // Assert
            Assert.AreEqual(25, button.X);
            Assert.AreEqual(25, button.Y);
            Assert.AreEqual(120, button.Width);
            Assert.AreEqual(80, button.Height);
            Assert.AreEqual(160, label.X);
            Assert.AreEqual(25, label.Y);
            Assert.AreEqual(130, label.Width);
            Assert.AreEqual(40, label.Height);
        }

        [TestMethod]
        public void TestSolveIntrinsicSize()
        {
            // Add a label view to an empty root view and check if its intrinsic size is properly constrained

            // Arrange
            var sut = new LayoutConstraintSolver();
            var root = new BaseView
            {
                TranslateBoundsIntoConstraints = true,
                Size = new Vector(300, 200)
            };
            var label = LabelViewControl.Create("This is a label");
            label.TranslateBoundsIntoConstraints = false;
            root.AddChild(label);
            LayoutConstraint.Create(label.Anchors.Top, root.Anchors.Top, constant: 25);
            LayoutConstraint.Create(label.Anchors.Left, root.Anchors.Left, constant: 25);
            
            // Act
            sut.Solve(root);

            // Assert
            Assert.AreEqual(25, label.X);
            Assert.AreEqual(25, label.Y);
            Assert.AreEqual(label.IntrinsicSize.X, label.Width);
            Assert.AreEqual(label.IntrinsicSize.Y, label.Height);
        }
    }
}

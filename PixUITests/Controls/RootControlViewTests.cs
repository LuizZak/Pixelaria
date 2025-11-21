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
using PixUI;
using PixUI.Controls;
using PixUI.LayoutSystem;
using Rhino.Mocks;

namespace PixUITests.Controls
{
    [TestClass]
    public class RootControlViewTests
    {
        private IFirstResponderDelegate<IEventHandler> _stubFirstResponderDelegate;
        private IInvalidateRegionDelegate _stubInvalidateRegionDelegate;

        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
            _stubFirstResponderDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>();
            _stubInvalidateRegionDelegate = MockRepository.GenerateStub<IInvalidateRegionDelegate>();
        }

        [TestMethod]
        public void TestHitTestControl()
        {
            var sut = CreateSut();
            var child = new ControlView {Location = new Vector(300, 300), Size = new Vector(50, 50)};
            sut.AddChild(child);

            var result = sut.HitTestControl(new Vector(310, 310));

            Assert.AreEqual(child, result);
        }
        
        [TestMethod]
        public void TestInvalidateDelegateIsCalled()
        {
            var sut = CreateSut();
            var child = new BaseView {Location = new Vector(10, 10), Size = new Vector(100, 100)};
            sut.AddChild(child);

            child.Invalidate();

            _stubInvalidateRegionDelegate.AssertWasCalled(stub => stub.DidInvalidate(null, child), options => options.IgnoreArguments());
        }

        #region Layout
        
        [TestMethod]
        public void TestLayout_SolvesConstraints()
        {
            var sut = CreateSut();
            // Hierarchy
            var child1 = new BaseView();
            var child2 = new BaseView();
            sut.AddChild(child1);
            sut.AddChild(child2);
            // Constraints
            sut.Size = new Vector(200, 200);
            child1.TranslateBoundsIntoConstraints = false;
            child2.TranslateBoundsIntoConstraints = false;
            LayoutConstraint.Create(child1.Anchors.Left, sut.Anchors.Left, constant: 10);
            LayoutConstraint.Create(child1.Anchors.Top, sut.Anchors.Top, constant: 10);
            LayoutConstraint.Create(child1.Anchors.Width, constant: 100);
            LayoutConstraint.Create(child1.Anchors.Height, constant: 50);
            LayoutConstraint.Create(child2.Anchors.Left, child1.Anchors.Right, constant: 10);
            LayoutConstraint.Create(child2.Anchors.Top, sut.Anchors.Top, constant: 10);
            LayoutConstraint.Create(child2.Anchors.Width, constant: 50);
            LayoutConstraint.Create(child2.Anchors.Height, constant: 25);

            sut.Layout();

            Assert.AreEqual(AABB.FromRectangle(10, 10, 100, 50), child1.FrameOnParent);
            Assert.AreEqual(AABB.FromRectangle(120, 10, 50, 25), child2.FrameOnParent);
        }

        #endregion

        private RootControlView CreateSut()
        {
            return new RootControlView(_stubFirstResponderDelegate, _stubInvalidateRegionDelegate);
        }
    }
}

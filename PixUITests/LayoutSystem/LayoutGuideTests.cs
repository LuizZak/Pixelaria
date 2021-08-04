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
using PixUI.Controls;
using PixUI.LayoutSystem;
using Rhino.Mocks;

namespace PixUITests.LayoutSystem
{
    [TestClass]
    public class LayoutGuideTests
    {
        private IFirstResponderDelegate<IEventHandler> _stubFirstResponderDelegate;

        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
            _stubFirstResponderDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>();
        }

        [TestMethod]
        public void TestLayout()
        {
            var sut = new LayoutGuide();
            var root = new RootControlView(_stubFirstResponderDelegate);
            root.AddLayoutGuide(sut);
            root.Size = new Vector(100, 100);
            LayoutConstraint.Create(sut.Anchors.Left, root.Anchors.Left, constant: 10);
            LayoutConstraint.Create(sut.Anchors.Top, root.Anchors.Top, constant: 20);
            LayoutConstraint.Create(sut.Anchors.Width, constant: 50);
            LayoutConstraint.Create(sut.Anchors.Height, constant: 60);

            root.Layout();

            Assert.AreEqual(AABB.FromRectangle(10, 20, 50, 60), sut.FrameOnParent);
        }
    }
}

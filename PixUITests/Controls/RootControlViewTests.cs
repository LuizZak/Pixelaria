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
            var sut = new RootControlView(null);
            var child = new ControlView {Location = new Vector(300, 300), Size = new Vector(50, 50)};
            sut.AddChild(child);

            var result = sut.HitTestControl(new Vector(310, 310));

            Assert.AreEqual(child, result);
        }
        
        [TestMethod]
        public void TestInvalidateDelegateIsCalled()
        {
            var child = new BaseView {Location = new Vector(10, 10), Size = new Vector(100, 100)};
            var sut = new RootControlView(_stubFirstResponderDelegate, _stubInvalidateRegionDelegate);
            sut.AddChild(child);

            child.Invalidate();

            _stubInvalidateRegionDelegate.AssertWasCalled(stub => stub.DidInvalidate(null, child), options => options.IgnoreArguments());
        }
    }
}

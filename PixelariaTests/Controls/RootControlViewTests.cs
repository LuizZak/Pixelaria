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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI;
using PixUI.Controls;
using Rhino.Mocks;

namespace PixelariaTests.Controls
{
    [TestClass]
    public class RootControlViewTests
    {
        private IFirstResponderDelegate<IEventHandler> stubFirstResponderDelegate;
        private IInvalidateRegionDelegate stubInvalidateRegionDelegate;

        [TestInitialize]
        public void TestInitialize()
        {
            stubFirstResponderDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>();
            stubInvalidateRegionDelegate = MockRepository.GenerateStub<IInvalidateRegionDelegate>();
        }

        [TestMethod]
        public void TestInvalidateDelegateIsCalled()
        {
            var child = new BaseView {Location = new Vector(10, 10), Size = new Vector(100, 100)};
            var sut = CreateRootControlView();
            sut.AddChild(child);

            child.Invalidate();

            stubInvalidateRegionDelegate.AssertWasCalled(stub => stub.DidInvalidate(null, child), options => options.IgnoreArguments());
        }

        private RootControlView CreateRootControlView()
        {
            return new RootControlView(stubFirstResponderDelegate, stubInvalidateRegionDelegate);
        }
    }
}

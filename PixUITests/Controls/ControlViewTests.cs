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
using PixUI.Controls;

namespace PixUITests.Controls
{
    [TestClass]
    public class ControlViewTests
    {
        [TestInitialize]
        public void Setup()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
        }

        [TestMethod]
        public void TestCanHandleMouseEvents()
        {
            // Base control view handles mouse events by default.
            var sut = new ControlView();

            Assert.IsTrue(sut.CanHandle(new MouseEventRequest(MouseEventType.MouseDown)));
            Assert.IsTrue(sut.CanHandle(new MouseEventRequest(MouseEventType.MouseUp)));
            Assert.IsTrue(sut.CanHandle(new MouseEventRequest(MouseEventType.MouseMove)));
            Assert.IsTrue(sut.CanHandle(new MouseEventRequest(MouseEventType.MouseClick)));
            Assert.IsTrue(sut.CanHandle(new MouseEventRequest(MouseEventType.MouseDoubleClick)));

            // Mouse wheel events are not consumed by default
            Assert.IsFalse(sut.CanHandle(new MouseEventRequest(MouseEventType.MouseWheel)));
        }

        [TestMethod]
        public void TestCanHandleKeyboardEvents()
        {
            // Base control view doesn't handle keyboard events by default.
            var sut = new ControlView();

            Assert.IsFalse(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.KeyDown)));
            Assert.IsFalse(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.KeyPress)));
            Assert.IsFalse(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.KeyUp)));
            Assert.IsFalse(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.PreviewKeyDown)));
        }

        private class KeyboardEventRequest : IKeyboardEventRequest
        {
            public KeyboardEventRequest(KeyboardEventType eventType)
            {
                EventType = eventType;
            }

            public void Accept(IEventHandler handler)
            {

            }

            public KeyboardEventType EventType { get; }
        }

        private class MouseEventRequest : IMouseEventRequest
        {
            public MouseEventRequest(MouseEventType eventType)
            {
                EventType = eventType;
            }

            public void Accept(IEventHandler handler)
            {

            }

            public MouseEventType EventType { get; }
        }
    }
}

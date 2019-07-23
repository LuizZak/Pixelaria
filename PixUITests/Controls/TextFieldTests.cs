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

using System.Drawing;
using System.Windows.Forms;
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI.Controls;
using PixUI.Text;
using PixUITests.TestUtils;
using TextRange = PixCore.Text.TextRange;

namespace PixUITests.Controls
{
    [TestClass]
    public class TextFieldTests
    {
        [TestInitialize]
        public void Setup()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
            ControlView.TextLayoutRenderer = new TestDirect2DRender();

            //BaseViewSnapshot.RecordMode = true;
        }

        [TestMethod]
        public void TestCanHandle()
        {
            var sut = TextField.Create();
            
            Assert.IsTrue(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.KeyDown)));
            Assert.IsTrue(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.KeyPress)));
            Assert.IsTrue(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.KeyUp)));
            Assert.IsTrue(sut.CanHandle(new KeyboardEventRequest(KeyboardEventType.PreviewKeyDown)));
        }

        [TestMethod]
        public void TestKeyPressNotEditable()
        {
            var sut = TextField.Create();
            sut.Editable = false;
            var keyEv = new KeyPressEventArgs('a');

            sut.OnKeyPress(keyEv);

            Assert.AreEqual("", sut.Text);
        }

        [TestMethod]
        public void TestKeyDownNotEditable()
        {
            var sut = TextField.Create();
            sut.Editable = false;
            sut.Text = "Abc";
            var keyEv = new KeyEventArgs(Keys.Delete);

            sut.OnKeyDown(keyEv);

            Assert.AreEqual("Abc", sut.Text);
        }

        [TestMethod]
        public void TestNavigateWithNotEditable()
        {
            var sut = TextField.Create();
            sut.Editable = false;
            sut.Text = "Abc";
            var keyEv = new PreviewKeyDownEventArgs(Keys.Right);

            sut.OnPreviewKeyDown(keyEv);

            Assert.AreEqual(new Caret(1), sut.Caret);
        }

        [TestMethod]
        public void TestEnterKey()
        {
            bool raisedEnterKey = false;
            var sut = TextField.Create();
            sut.AcceptsEnterKey = true;
            sut.EnterKey += (o, e) => { raisedEnterKey = true; };

            sut.OnKeyDown(new KeyEventArgs(Keys.Enter));

            Assert.IsTrue(raisedEnterKey);
        }

        [TestMethod]
        public void TestEnterKeyNotRaisedWhenAcceptsEnterKeyIsFalse()
        {
            bool raisedEnterKey = false;
            var sut = TextField.Create();
            sut.AcceptsEnterKey = false;
            sut.EnterKey += (o, e) => { raisedEnterKey = true; };

            sut.OnKeyDown(new KeyEventArgs(Keys.Enter));

            Assert.IsFalse(raisedEnterKey);
        }
        
        #region Snapshot rendering tests

        [TestMethod]
        public void TestRenderingForegroundColor()
        {
            var sut = TextField.Create(false);
            sut.Size = new Vector(80, 28);
            sut.Text = "Lorem ipsum";

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingNormalState()
        {
            var sut = TextFieldWithStyles();
            
            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingFocusedState()
        {
            var sut = TextFieldWithStyles();
            sut.BecomeFirstResponder();
            
            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingSelectionArea()
        {
            var sut = TextField.Create(false);
            sut.Size = new Vector(80, 28);
            sut.Text = "Lorem ipsum";
            sut.Caret = new Caret(new TextRange(1, 3), CaretPosition.End);

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        private static TextField TextFieldWithStyles()
        {
            var root = new RootControlView(new MockFirstResponderDelegate());
            var sut = TextField.Create(false);
            sut.Size = new Vector(80, 28);
            sut.Text = "Lorem ipsum";
            root.AddChild(sut);

            var normalStyle = new TextFieldVisualStyleParameters(
                Color.DimGray, Color.White, Color.LightGray, Color.Transparent, 0, Color.Black, Color.Blue
            );
            var focusedStyle = new TextFieldVisualStyleParameters(
                Color.Black, Color.White, Color.White, Color.LightBlue, 1, Color.Black, Color.Blue
            );

            sut.SetStyleForState(normalStyle, ControlViewState.Normal);
            sut.SetStyleForState(focusedStyle, ControlViewState.Focused);

            return sut;
        }

        #endregion

        public TestContext TestContext { get; set; }

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

        private class MockFirstResponderDelegate : IFirstResponderDelegate<IEventHandler>
        {
            private IEventHandler _firstResponder;

            public bool SetAsFirstResponder(IEventHandler firstResponder, bool force)
            {
                _firstResponder = firstResponder;
                return true;
            }

            public void RemoveCurrentResponder()
            {
                
            }

            public bool IsFirstResponder(IEventHandler handler)
            {
                return ReferenceEquals(_firstResponder, handler);
            }
        }
    }
}

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
using System.Windows.Forms;
using System.Windows.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.PixUI.Controls;
using Pixelaria.PixUI.Controls.Text;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Rhino.Mocks;

namespace PixelariaTests.Tests.Views.ExportPipeline.ExportPipelineFeatures
{
    [TestClass]
    public class ControlViewFeatureTests
    {
        [TestInitialize]
        public void Setup()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
        }

        #region Mouse Events

        [TestMethod]
        public void TestOnMouseDown()
        {
            var sut = new ControlViewFeature(new ExportPipelineControl());

            var ev = new MouseEventArgs(MouseButtons.Left, 0, 10, 10, 0);

            var control = MockRepository.GeneratePartialMock<ControlView>();
            
            control.Location = Vector.Zero;
            control.Size = new Vector(200, 200);

            control.Expect(c => c.OnMouseDown(ev)).IgnoreArguments();

            sut.AddControl(control);

            sut.OnMouseDown(ev);

            control.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestOnMouseUp()
        {
            var sut = new ControlViewFeature(new ExportPipelineControl());

            var ev = new MouseEventArgs(MouseButtons.Left, 0, 10, 10, 0);

            var control = MockRepository.GeneratePartialMock<ControlView>();

            control.Location = Vector.Zero;
            control.Size = new Vector(200, 200);

            control.Expect(c => c.OnMouseUp(ev)).IgnoreArguments();

            sut.AddControl(control);

            sut.OnMouseDown(ev);
            sut.OnMouseUp(ev);

            control.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestOnMouseUpOnlyAfterOnMouseDown()
        {
            // OnMouseUp should only fire on controls which have been pressed down
            // with OnMouseDown beforehands
            var sut = new ControlViewFeature(new ExportPipelineControl());

            var ev = new MouseEventArgs(MouseButtons.Left, 0, 10, 10, 0);

            var control = MockRepository.GeneratePartialMock<ControlView>();

            control.Location = Vector.Zero;
            control.Size = new Vector(200, 200);

            sut.AddControl(control);

            sut.OnMouseUp(ev);

            control.AssertWasNotCalled(c => c.OnMouseUp(ev));
        }

        [TestMethod]
        public void TestOnMouseClick()
        {
            // OnMouseClick is called when both OnMouseDown and OnMouseUp occur on top
            // of the same control

            var sut = new ControlViewFeature(new ExportPipelineControl());

            var evOver = new MouseEventArgs(MouseButtons.Left, 0, 10, 10, 0);
            var evNotOver = new MouseEventArgs(MouseButtons.Left, 0, -100, -100, 0);

            var control = MockRepository.GeneratePartialMock<ControlView>();

            control.Location = Vector.Zero;
            control.Size = new Vector(200, 200);

            control.BackToRecord();

            using (control.GetMockRepository().Ordered())
            {
                // First event test when mouse down & mouse up are over control
                control.Expect(c => c.OnMouseDown(evOver)).IgnoreArguments();
                control.Expect(c => c.OnMouseUp(evOver)).IgnoreArguments();
                control.Expect(c => c.OnMouseClick(evOver)).IgnoreArguments();

                // Second event test where mouse down is over, but mouse up is out of control's bounds
                control.Expect(c => c.OnMouseDown(evNotOver)).IgnoreArguments();
                control.Expect(c => c.OnMouseUp(evNotOver)).IgnoreArguments();
            }

            control.Replay();

            sut.AddControl(control);

            sut.OnMouseDown(evOver);
            sut.OnMouseUp(evOver);

            sut.OnMouseDown(evOver);
            sut.OnMouseUp(evNotOver);

            control.VerifyAllExpectations();
        }

        [TestMethod]
        public void TestOnMouseMove()
        {
            // Tests a full sequence of mouse move events where the proper
            // sequence of mouse enter/move/leave events are fired.

            var sut = new ControlViewFeature(new ExportPipelineControl());

            var evOver = new MouseEventArgs(MouseButtons.Left, 0, 10, 10, 0);
            var evNotOver = new MouseEventArgs(MouseButtons.Left, 0, -100, 100, 0);

            var control = MockRepository.GeneratePartialMock<ControlView>();
            
            control.Location = Vector.Zero;
            control.Size = new Vector(200, 200);

            using (control.GetMockRepository().Ordered())
            {
                control.Expect(c => c.OnMouseEnter()).IgnoreArguments();
                control.Expect(c => c.OnMouseMove(evNotOver)).IgnoreArguments();
                control.Expect(c => c.OnMouseLeave()).IgnoreArguments();
            }

            control.Replay();

            sut.AddControl(control);

            sut.OnMouseMove(evOver); // Calls OnMouseEnter
            sut.OnMouseMove(evOver); // Calls OnMouseMove
            sut.OnMouseMove(evNotOver); // Calls OnMouseLeave

            control.VerifyAllExpectations();
        }
        
        #endregion

        [TestMethod]
        public void TestTextFieldControlSample()
        {
            var sut = new ControlViewFeature(new ExportPipelineControl());

            var textField = new TextField
            {
                Location = new Vector(),
                Size = new Vector(200, 200)
            };
            
            sut.AddControl(textField);

            Assert.IsFalse(textField.IsFirstResponder);

            // Synthesize a mouse click on top of the text field
            var ev = new MouseEventArgs(MouseButtons.Left, 0, 10, 10, 0);

            sut.OnMouseDown(ev);
            sut.OnMouseUp(ev); // At this point, a mouse click event should be raised on the text field

            // Text field should become the first responder now
            Assert.IsTrue(textField.IsFirstResponder);

            // Send some keyboard input to test handling
            var keyEv = new KeyPressEventArgs('a');

            sut.OnKeyPress(keyEv);

            Assert.AreEqual("a", textField.Text);
        }

        #region First Responder

        [TestMethod]
        public void TestIsFirstResponderQueriesRootControlView()
        {
            var sut = new TestControl();

            var stubDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>(null);

            stubDelegate.Stub(del => del.IsFirstResponder(sut)).Return(true).Repeat.Once();
            stubDelegate.Stub(del => del.IsFirstResponder(sut)).Return(false);

            var root = new RootControlView(stubDelegate);
            
            root.AddChild(sut);

            Assert.IsTrue(sut.IsFirstResponder);
            Assert.IsFalse(sut.IsFirstResponder);
        }

        [TestMethod]
        public void TestSetAsFirstResponderTrue()
        {
            var sut = new TestControl();

            var stubDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>(null);

            stubDelegate.Stub(del => del.SetAsFirstResponder(sut, false)).Return(true);

            var root = new RootControlView(stubDelegate);

            root.AddChild(sut);

            Assert.IsTrue(sut.BecomeFirstResponder());
        }

        [TestMethod]
        public void TestSetAsFirstResponderFalse()
        {
            var sut = new TestControl();

            var stubDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>(null);

            stubDelegate.Stub(del => del.SetAsFirstResponder(sut, false)).Return(false);

            var root = new RootControlView(stubDelegate);

            root.AddChild(sut);

            Assert.IsFalse(sut.BecomeFirstResponder());
        }

        [TestMethod]
        public void TestSetAsFirstResponderWithCanBecomeFirstResponderFalse()
        {
            var sut = new TestControl
            {
                StubCanBecomeFirstResponder = false
            };

            var stubDelegate = MockRepository.GenerateStub<IFirstResponderDelegate<IEventHandler>>(null);
            
            var root = new RootControlView(stubDelegate);

            root.AddChild(sut);

            Assert.IsFalse(sut.BecomeFirstResponder());
            
            stubDelegate.AssertWasNotCalled(del => del.SetAsFirstResponder(sut, false));
        }

        #endregion

        private class TestControl : ControlView
        {
            public bool StubCanBecomeFirstResponder { private get; set; } = true;

            public override bool CanBecomeFirstResponder => StubCanBecomeFirstResponder;
        }
    }
}

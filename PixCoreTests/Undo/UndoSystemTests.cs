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
using PixCore.Undo;
using Rhino.Mocks;

namespace PixCoreTests.Undo
{
    [TestClass]
    public class UndoSystemTests
    {
        [TestMethod]
        public void TestUndo()
        {
            var testUndo = new TestUndoTask();
            var sut = new UndoSystem();
            sut.RegisterUndo(testUndo);

            sut.Undo();

            Assert.IsTrue(testUndo.CalledUndo);
        }

        [TestMethod]
        public void TestRedo()
        {
            var testUndo = new TestUndoTask();
            var sut = new UndoSystem();
            sut.RegisterUndo(testUndo);
            sut.Undo();

            sut.Redo();

            Assert.IsTrue(testUndo.CalledRedo);
        }

        [TestMethod]
        public void TestGroupUndo()
        {
            var testUndo = new TestUndoTask();
            var sut = new UndoSystem();

            sut.StartGroupUndo("");
            sut.RegisterUndo(testUndo);
            sut.FinishGroupUndo();

            Assert.AreEqual(1, sut.Count);
        }

        [TestMethod]
        public void TestCallingUndoWhileInGroupUndoDoesNotCrash()
        {
            var testUndo1 = new TestUndoTask();
            var testUndo2 = new TestUndoTask();
            var testUndo3 = new TestUndoTask();
            var sut = new UndoSystem();
            sut.StartGroupUndo("");
            sut.RegisterUndo(testUndo1);
            sut.RegisterUndo(testUndo2);
            sut.RegisterUndo(testUndo3);
            
            sut.Undo();
        }

        [TestMethod]
        public void TestClearFinishesGroupUndoTasks()
        {
            var testUndo = new TestUndoTask();
            var sut = new UndoSystem();
            sut.StartGroupUndo("");
            sut.RegisterUndo(testUndo);

            sut.Clear();
            sut.FinishGroupUndo();

            Assert.AreEqual(0, sut.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(UndoSystemRecursivelyModifiedException))]
        public void TestRegisterUndoWhileUndoingWorkThrowsException()
        {
            var sut = new UndoSystem();
            var mock = MockRepository.GenerateMock<IUndoTask>();
            mock.Stub(m => m.Undo()).WhenCalled(a => sut.RegisterUndo(new TestUndoTask()));
            sut.RegisterUndo(mock);

            // Bang!
            sut.Undo();
        }

        [TestMethod]
        [ExpectedException(typeof(UndoSystemRecursivelyModifiedException))]
        public void TestUndoWhileUndoingWorkThrowsException()
        {
            var sut = new UndoSystem();
            var mock = MockRepository.GenerateMock<IUndoTask>();
            mock.Stub(m => m.Undo()).WhenCalled(a => sut.Undo());
            sut.RegisterUndo(mock);

            // Bang!
            sut.Undo();
        }

        internal class TestUndoTask : IUndoTask
        {
            public bool CalledClear { get; set; }
            public bool CalledUndo { get; set; }
            public bool CalledRedo { get; set; }
            public string Description { get; set; } = "Test Undo";

            public void Clear()
            {
                CalledClear = true;
            }

            public void Undo()
            {
                CalledUndo = true;
            }

            public void Redo()
            {
                CalledRedo = true;
            }

            public string GetDescription()
            {
                return Description;
            }
        }
    }
}

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Undo;

namespace PixCoreTests.Undo
{
    [TestClass]
    public class GroupUndoTaskTests
    {
        private TestUndoTask task1;
        private TestUndoTask task2;
        private TestUndoTask task3;
        private GroupUndoTask sut;

        [TestInitialize]
        public void TestInitialize()
        {
            task1 = new TestUndoTask();
            task2 = new TestUndoTask();
            task3 = new TestUndoTask();
            sut = new GroupUndoTask(new []{task1, task2, task3}, "Undo");
        }

        /// <summary>
        /// Tests Undo is performed front-to-back, with the last task on the undo tasks array
        /// being undone first
        /// </summary>
        [TestMethod]
        public void TestUndoOrderWhenReversed()
        {
            var order = new OrderTester();
            task1.OnUndo = () => { order.ExpectToBeCall(3); };
            task2.OnUndo = () => { order.ExpectToBeCall(2); };
            task3.OnUndo = () => { order.ExpectToBeCall(1); };

            sut.Undo();

            order.AssertLastCallIndex(3);
        }

        /// <summary>
        /// 'Reverseness' of undo ordering can be toggled off via a property of <see cref="GroupUndoTask"/>
        /// </summary>
        [TestMethod]
        public void TestUndoOrderWhenNotReversed()
        {
            var order = new OrderTester();
            task1.OnUndo = () => { order.ExpectToBeCall(1); };
            task2.OnUndo = () => { order.ExpectToBeCall(2); };
            task3.OnUndo = () => { order.ExpectToBeCall(3); };
            sut.ReverseOnUndo = false;

            sut.Undo();

            order.AssertLastCallIndex(3);
        }

        /// <summary>
        /// Tests Undo is performed back-to-front, with the first task on the undo tasks array
        /// being redone first
        /// </summary>
        [TestMethod]
        public void TestRedoOrderWhenReversed()
        {
            var order = new OrderTester();
            task1.OnRedo = () => { order.ExpectToBeCall(1); };
            task2.OnRedo = () => { order.ExpectToBeCall(2); };
            task3.OnRedo = () => { order.ExpectToBeCall(3); };

            sut.Redo();

            order.AssertLastCallIndex(3);
        }

        /// <summary>
        /// 'Reverseness' of redo ordering is not affected by <see cref="GroupUndoTask.ReverseOnUndo"/>
        /// </summary>
        [TestMethod]
        public void TestRedoOrderWhenNotReversed()
        {
            // TODO: This should reverse too, but doesn't currently. Try to analyze impact on main app of fixing this later.

            var order = new OrderTester();
            task1.OnRedo = () => { order.ExpectToBeCall(1); };
            task2.OnRedo = () => { order.ExpectToBeCall(2); };
            task3.OnRedo = () => { order.ExpectToBeCall(3); };
            sut.ReverseOnUndo = false;

            sut.Redo();

            order.AssertLastCallIndex(3);
        }

        internal class TestUndoTask : IUndoTask
        {
            public bool CalledClear { get; set; }
            public bool CalledUndo { get; set; }
            public bool CalledRedo { get; set; }
            public string Description { get; set; } = "Test Undo";

            public Action OnUndo;
            public Action OnRedo;
            public Action OnClear;

            public void Clear()
            {
                CalledClear = true;
                OnClear?.Invoke();
            }

            public void Undo()
            {
                CalledUndo = true;
                OnUndo?.Invoke();
            }

            public void Redo()
            {
                CalledRedo = true;
                OnRedo?.Invoke();
            }

            public string GetDescription()
            {
                return Description;
            }
        }

        /// <summary>
        /// Provides a way to test the ordering of method calls in a more sane way
        /// </summary>
        private class OrderTester
        {
            private int _next = 1;

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            public void ExpectToBeCall(int index)
            {
                if (_next != index)
                    Assert.Fail($"Expected next call index to be {_next}, but received {index}");

                _next += 1;
            }

            // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
            public void AssertLastCallIndex(int index)
            {
                if (_next != index + 1)
                    Assert.Fail($"Expected last call index to be {index}, but is currently at {_next - 1}");
            }
        }
    }
}

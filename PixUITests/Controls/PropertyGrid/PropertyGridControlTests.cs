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
using PixUI.Controls.PropertyGrid;

namespace PixUITests.Controls.PropertyGrid
{
    [TestClass]
    public class PropertyGridControlTests
    {
        [TestMethod]
        public void TestPropertyInspectorGetProperties()
        {
            var target = new TestObject();
            var sut = new PropertyGridControl.PropertyInspector(target);

            var props = sut.GetProperties();

            Assert.AreEqual(2, props.Length);
            Assert.AreEqual("Id", props[0].Name);
            Assert.AreEqual(typeof(int), props[0].PropertyType);
            Assert.IsTrue(props[0].CanSet);
            Assert.AreEqual("IsAlive", props[1].Name);
            Assert.AreEqual(typeof(bool), props[1].PropertyType);
            Assert.IsFalse(props[1].CanSet);
        }

        [TestMethod]
        public void TestInspectablePropertySetValue()
        {
            var target = new TestObject();
            var sut = new PropertyGridControl.PropertyInspector(target);
            var props = sut.GetProperties();

            props[0].SetValue(1);
            Assert.ThrowsException<InvalidOperationException>(() => props[1].SetValue(true));

            Assert.AreEqual(1, target.Id);
            Assert.IsFalse(target.IsAlive);
        }

        [TestMethod]
        public void TestInspectablePropertyGetValue()
        {
            var target = new TestObject
            {
                Id = 1,
                IsAlive = true
            };
            var sut = new PropertyGridControl.PropertyInspector(target);
            var props = sut.GetProperties();

            Assert.AreEqual(1, props[0].GetValue());
            Assert.AreEqual(true, props[1].GetValue());
        }

        internal class TestObject
        {
            public float Field;

            public int Id { get; set; }
            public bool IsAlive { get; internal set; }
            public object SetterOnly { internal get; set; }
        }
    }
}
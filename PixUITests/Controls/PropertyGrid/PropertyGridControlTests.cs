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
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixUI.Controls;
using PixUI.Controls.PropertyGrid;

namespace PixUITests.Controls.PropertyGrid
{
    [TestClass]
    public class PropertyGridControlTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;
        }

        [TestMethod]
        public void TestPropertyInspectorGetProperties()
        {
            var target = new TestObject();
            var sut = new PropertyGridControl.PropertyInspector(target);

            var props = sut.GetProperties();

            Assert.AreEqual(2, props.Length);
            Assert.AreEqual("Id", props[0].Name);
            Assert.AreEqual(typeof(int), props[0].PropertyType);
            Assert.AreEqual(typeof(TestObject), props[0].TargetType);
            Assert.IsTrue(props[0].CanSet);
            Assert.AreEqual("IsAlive", props[1].Name);
            Assert.AreEqual(typeof(bool), props[1].PropertyType);
            Assert.AreEqual(typeof(TestObject), props[1].TargetType);
            Assert.IsFalse(props[1].CanSet);
        }

        [TestMethod]
        public void TestPropertyInspectorGetProperties_TargetType()
        {
            var target = new TestObjectSub();
            var sut = new PropertyGridControl.PropertyInspector(target);

            var props = sut.GetProperties();

            Assert.AreEqual(3, props.Length);
            Assert.AreEqual("Id_Sub", props[0].Name);
            Assert.AreEqual(typeof(TestObjectSub), props[0].TargetType);
            Assert.AreEqual("Id", props[1].Name);
            Assert.AreEqual(typeof(TestObject), props[1].TargetType);
            Assert.AreEqual("IsAlive", props[2].Name);
            Assert.AreEqual(typeof(TestObject), props[2].TargetType);
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

            Assert.AreEqual(1, props[0].GetValues()[0]);
            Assert.AreEqual(true, props[1].GetValues()[0]);
        }

        [TestMethod]
        public void TestInspectablePropertyMultipleTargetsGetValue()
        {
            var target1 = new TestObject
            {
                Id = 1,
                IsAlive = false
            };
            var target2 = new TestObject2
            {
                Id = 1.0f,
                IsAlive = true
            };
            var sut = new PropertyGridControl.PropertyInspector(new object[] {target1, target2});
            var props = sut.GetProperties();

            Assert.AreEqual(1, props.Length);

            Assert.AreEqual(false, props[0].GetValues()[0]);
            Assert.AreEqual(true, props[0].GetValues()[1]);
        }

        [TestMethod]
        public void TestPropertyFieldWithInspectablePropertyOfArrayOfTypes()
        {
            var target = new TestObject3
            {
                Property = new[] {typeof(int), typeof(string)}
            };
            var gridControl = PropertyGridControl.Create();
            var property = new PropertyGridControl.PropertyInspector(target).GetProperties()[0];
            var sut = PropertyGridControl.PropertyField.Create(gridControl, property);

            Assert.AreEqual("[2 values]", sut.Value);
        }

        [TestMethod]
        public void TestInspectablePropertySingleTargetGetTargets()
        {
            var target1 = new TestObject();
            var sut = new PropertyGridControl.PropertyInspector(new object[] {target1});
            var props = sut.GetProperties();

            Assert.AreEqual(props[0].GetTargets().Length, 1);
            Assert.AreSame(props[0].GetTargets()[0], target1);
        }

        [TestMethod]
        public void TestInspectablePropertyMultipleTargetsGetTargets()
        {
            var target1 = new TestObject();
            var target2 = new TestObject2();
            var sut = new PropertyGridControl.PropertyInspector(new object[] {target1, target2});
            var props = sut.GetProperties();

            Assert.AreEqual(props[0].GetTargets().Length, 2);
            Assert.AreSame(props[0].GetTargets()[0], target1);
            Assert.AreSame(props[0].GetTargets()[1], target2);
        }

        internal class TestObject
        {
            public float Field;

            public int Id { get; set; }
            public bool IsAlive { get; internal set; }
            public object SetterOnly { internal get; set; }
        }
        
        internal class TestObject2
        {
            public float Field;

            public float Id { get; set; }
            public bool IsAlive { get; set; }
            public object SetterOnly { internal get; set; }
        }

        internal class TestObject3
        {
            public Type[] Property { get; set; }
        }

        internal class TestObjectSub : TestObject
        {
            public int Id_Sub { get; set; }
        }
    }
}
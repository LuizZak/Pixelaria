﻿/*
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
using PixUITests.TestUtils;

namespace PixUITests.Controls
{
    [TestClass]
    public class LabelViewControlTests
    {
        #region Invalidation Triggers

        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            //BaseViewSnapshot.RecordMode = true;
        }

        [TestMethod]
        public void TestInvalidateOnHorizontalTextAlignment()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var sut = new LabelViewControl {Size = new Vector(100, 100)};
            root.AddChild(sut);
            root.InvalidateReference = null;
            root.InvalidateRegion = null;

            sut.HorizontalTextAlignment = HorizontalTextAlignment.Leading;

            root.AssertViewBoundsWhereInvalidated(sut);
        }

        [TestMethod]
        public void TestInvalidateOnVerticalTextAlignment()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var sut = new LabelViewControl { Size = new Vector(100, 100) };
            root.AddChild(sut);
            root._ResetInvalidation();

            sut.VerticalTextAlignment = VerticalTextAlignment.Near;

            root.AssertViewBoundsWhereInvalidated(sut);
        }

        [TestMethod]
        public void TestRendering()
        {
            var sut = new LabelViewControl("Abc");

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingLeadingHorizontalText()
        {
            var sut = new LabelViewControl("Abc")
            {
                AutoResize = false,
                Size = new Vector(50, 50),
                HorizontalTextAlignment = HorizontalTextAlignment.Leading
            };
            
            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingCenterHorizontalText()
        {
            var sut = new LabelViewControl("Abc")
            {
                AutoResize = false,
                Size = new Vector(50, 50),
                HorizontalTextAlignment = HorizontalTextAlignment.Center
            };
            
            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingTrailingHorizontalText()
        {
            var sut = new LabelViewControl("Abc")
            {
                AutoResize = false,
                Size = new Vector(50, 50),
                HorizontalTextAlignment = HorizontalTextAlignment.Trailing
            };

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingNearVerticalText()
        {
            var sut = new LabelViewControl("Abc")
            {
                AutoResize = false,
                Size = new Vector(50, 50),
                VerticalTextAlignment = VerticalTextAlignment.Near
            };

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingCenterVerticalText()
        {
            var sut = new LabelViewControl("Abc")
            {
                AutoResize = false,
                Size = new Vector(50, 50),
                VerticalTextAlignment = VerticalTextAlignment.Center
            };

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingFarVerticalText()
        {
            var sut = new LabelViewControl("Abc")
            {
                AutoResize = false,
                Size = new Vector(50, 50),
                VerticalTextAlignment = VerticalTextAlignment.Far
            };

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        public TestContext TestContext { get; set; }

        #endregion
    }
}

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
using System.Windows.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixRendering;
using PixUI.Controls;
using PixUITests.TestUtils;

namespace PixUITests.Controls
{
    [TestClass]
    public class LabelViewControlTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            ControlView.UiDispatcher = Dispatcher.CurrentDispatcher;

            // BaseViewSnapshot.RecordMode = true;
        }

        #region Rendering
        
        [TestMethod]
        public void TestRendering()
        {
            var sut = LabelViewControl.Create("Abc");

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingLeadingHorizontalText()
        {
            var sut = LabelViewControl.Create("Abc");
            sut.AutoResize = false;
            sut.Size = new Vector(50, 50);
            sut.HorizontalTextAlignment = HorizontalTextAlignment.Leading;

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingCenterHorizontalText()
        {
            var sut = LabelViewControl.Create("Abc");
            sut.AutoResize = false;
            sut.Size = new Vector(50, 50);
            sut.HorizontalTextAlignment = HorizontalTextAlignment.Center;

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingTrailingHorizontalText()
        {
            var sut = LabelViewControl.Create("Abc");
            sut.AutoResize = false;
            sut.Size = new Vector(50, 50);
            sut.HorizontalTextAlignment = HorizontalTextAlignment.Trailing;

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingNearVerticalText()
        {
            var sut = LabelViewControl.Create("Abc");
            sut.AutoResize = false;
            sut.Size = new Vector(50, 50);
            sut.VerticalTextAlignment = VerticalTextAlignment.Near;

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingCenterVerticalText()
        {
            var sut = LabelViewControl.Create("Abc");
            sut.AutoResize = false;
            sut.Size = new Vector(50, 50);
            sut.VerticalTextAlignment = VerticalTextAlignment.Center;

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingFarVerticalText()
        {
            var sut = LabelViewControl.Create("Abc");
            sut.AutoResize = false;
            sut.Size = new Vector(50, 50);
            sut.VerticalTextAlignment = VerticalTextAlignment.Far;

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingBackgroundColor()
        {
            var sut = LabelViewControl.Create("Abcd");
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(0, 1), new BackgroundColorAttribute(Color.Red));
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(1, 2), new BackgroundColorAttribute(Color.Green));
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(2, 3), new BackgroundColorAttribute(Color.Blue));

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        [TestMethod]
        public void TestRenderingForegroundColor()
        {
            var sut = LabelViewControl.Create("Abcd");
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(0, 1), new ForegroundColorAttribute(Color.Red));
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(1, 2), new ForegroundColorAttribute(Color.Green));
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(2, 3), new ForegroundColorAttribute(Color.Blue));

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }
        
        [TestMethod]
        public void TestRenderingFontAttribute()
        {
            var sut = LabelViewControl.Create("Abcd");
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(0, 1), new TextFontAttribute(new Font(FontFamily.GenericMonospace, 10)));
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(1, 2), new TextFontAttribute(new Font(FontFamily.GenericSerif, 11)));
            sut.AttributedText.SetAttributes(TextRange.FromOffsets(2, 3), new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 12)));

            BaseViewSnapshot.Snapshot(sut, TestContext);
        }

        public TestContext TestContext { get; set; }

        #endregion

        #region Invalidation Triggers

        [TestMethod]
        public void TestInvalidateOnHorizontalTextAlignment()
        {
            var root = new TestInvalidateBaseView {Location = new Vector(5, 5)};
            var sut = LabelViewControl.Create();
            sut.Size = new Vector(100, 100);
            root.AddChild(sut);
            root._ResetInvalidation();

            sut.HorizontalTextAlignment = HorizontalTextAlignment.Leading;

            root.AssertViewBoundsWhereInvalidated(sut);
        }

        [TestMethod]
        public void TestInvalidateOnVerticalTextAlignment()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var sut = LabelViewControl.Create();
            sut.Size = new Vector(100, 100);
            root.AddChild(sut);
            root._ResetInvalidation();

            sut.VerticalTextAlignment = VerticalTextAlignment.Near;

            root.AssertViewBoundsWhereInvalidated(sut);
        }
        
        [TestMethod]
        public void TestInvalidateOnSetText()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var sut = LabelViewControl.Create();
            sut.AutoResize = false;
            sut.Text = "Abc";
            sut.Size = new Vector(100, 100);
            root.AddChild(sut);
            root._ResetInvalidation();

            sut.Text = "Def";

            root.AssertViewBoundsWhereInvalidated(sut);
        }

        [TestMethod]
        public void TestInvalidateOnSetAttributedText()
        {
            var root = new TestInvalidateBaseView { Location = new Vector(5, 5) };
            var sut = LabelViewControl.Create();
            sut.AutoResize = false;
            sut.Text = "Abc";
            sut.Size = new Vector(100, 100);
            root.AddChild(sut);
            root._ResetInvalidation();

            sut.AttributedText.SetText("Def");

            root.AssertViewBoundsWhereInvalidated(sut);
        }

        #endregion
    }
}

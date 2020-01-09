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
using PixDirectX.Rendering.DirectX;
using PixRendering;
using SharpDX.Direct2D1;
using Bitmap = System.Drawing.Bitmap;
using ImagingFactory = SharpDX.WIC.ImagingFactory;
using BitmapCreateCacheOption = SharpDX.WIC.BitmapCreateCacheOption;
using PixelFormat = SharpDX.WIC.PixelFormat;
using WicBitmap = SharpDX.WIC.Bitmap;

namespace PixDirectXTests.Rendering
{
    [TestClass]
    public class D2DImageResourcesTests
    {
        private WicBitmap _bitmap;
        private Direct2DWicBitmapRenderManager _renderer;
        private Factory _factory;

        [TestMethod]
        public void TestAddImageResource()
        {
            var bitmap = new Bitmap(4, 4);
            var sut = new ImageResources();

            sut.AddImageResource(_renderer.RenderingState, bitmap, "test");

            Assert.IsNotNull(sut.BitmapForResource("test"));
        }

        [TestMethod]
        public void TestAddImageResourceReturn()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();

            var resource = sut.AddImageResource(_renderer.RenderingState, bitmap, "test");

            Assert.AreEqual("test", resource.ResourceName);
            Assert.AreEqual(4, resource.Size.Width);
            Assert.AreEqual(8, resource.Size.Height);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestAddImageResourceDuplicatedResourceNameException()
        {
            var bitmap = new Bitmap(4, 4);
            var sut = new ImageResources();
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test");

            // Bang!
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test");
        }

        [TestMethod]
        public void TestGetImageResource()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test");

            var resource = sut.GetImageResource("test");

            Assert.IsNotNull(resource);
            Assert.AreEqual("test", resource.Value.ResourceName);
            Assert.AreEqual(4, resource.Value.Size.Width);
            Assert.AreEqual(8, resource.Value.Size.Height);
        }

        [TestMethod]
        public void TestGetImageResourceWithNonExistantImage()
        {
            var sut = new ImageResources();

            var resource = sut.GetImageResource("test");

            Assert.IsNull(resource);
        }

        [TestMethod]
        public void TestRemoveAllImageResources()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test 1");
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test 2");

            sut.RemoveAllImageResources();

            Assert.IsNull(sut.GetImageResource("test 1"));
            Assert.IsNull(sut.GetImageResource("test 2"));
        }

        [TestMethod]
        public void TestRemoveImageResource()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test 1");
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test 2");

            sut.RemoveImageResource("test 1");

            Assert.IsNull(sut.GetImageResource("test 1"));
            Assert.IsNotNull(sut.GetImageResource("test 2"));
        }

        [TestMethod]
        public void TestBitmapForResourceImageResource()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();
            var resource = sut.AddImageResource(_renderer.RenderingState, bitmap, "test 1");
            
            Assert.IsNotNull(sut.BitmapForResource(resource));
            Assert.IsNull(sut.BitmapForResource(new ImageResource("non-existant", 0, 0)));
        }

        [TestMethod]
        public void TestBitmapForResourceString()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test 1");
            
            Assert.IsNotNull(sut.BitmapForResource("test 1"));
            Assert.IsNull(sut.BitmapForResource("non-existant"));
        }

        [TestMethod]
        public void TestDispose()
        {
            var bitmap = new Bitmap(4, 8);
            var sut = new ImageResources();
            sut.AddImageResource(_renderer.RenderingState, bitmap, "test 1");
            var d2DBitmap = sut.BitmapForResource("test 1");

            sut.Dispose();
            
            Assert.IsNotNull(d2DBitmap);
            Assert.IsTrue(d2DBitmap.IsDisposed);
            Assert.IsNull(sut.BitmapForResource("test 1"));
        }

        [TestInitialize]
        public void TestInitialize()
        {
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = PixelFormat.Format32bppPBGRA;

            _factory = new Factory();

            using (var factory = new ImagingFactory())
            {
                _bitmap = new WicBitmap(factory, 2, 2, pixelFormat, bitmapCreateCacheOption);
                _renderer = new Direct2DWicBitmapRenderManager(_bitmap, _factory);
                _renderer.Initialize();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _renderer.Dispose();
            _bitmap.Dispose();
            _factory.Dispose();
        }
    }
}
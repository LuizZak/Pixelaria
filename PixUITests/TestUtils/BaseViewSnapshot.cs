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
using System.Collections.Generic;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixDirectX.Rendering.DirectX;
using PixRendering;
using PixSnapshot;
using PixUI;
using PixUI.Controls;
using PixUI.Rendering;
using PixUI.Visitor;
using SharpDX.WIC;
using Bitmap = System.Drawing.Bitmap;

namespace PixUITests.TestUtils
{
    /// <inheritdoc />
    /// <summary>
    /// Helper static class to perform bitmap-based rendering comparisons of <see cref="T:PixUI.Controls.SelfRenderingBaseView" /> instances
    /// (mostly <see cref="T:PixUI.Controls.ControlView" /> subclasses) to assert visual and style consistency.
    /// </summary>
    public class BaseViewSnapshot : ISnapshotProvider<BaseViewSnapshotTest>
    {
        /// <summary>
        /// Whether tests are currently under record mode- under record mode, results are recorded on disk to be later
        /// compared when not in record mode.
        /// 
        /// Calls to <see cref="Snapshot"/> always fail with an assertion during record mode.
        /// 
        /// Defaults to false.
        /// </summary>
        public static bool RecordMode = false;

        /// <summary>
        /// The default tolerance to use when comparing resulting images.
        /// </summary>
        public static float Tolerance = 0.01f;
        
        public static void Snapshot([NotNull] BaseView view, [NotNull] TestContext context, string suffix = "", float? tolerance = null, bool? recordMode = null, [CanBeNull] BaseViewSnapshotResources resources = null)
        {
            var test = new BaseViewSnapshotTest(view, resources);

            BitmapSnapshotTesting.Snapshot<BaseViewSnapshot, BaseViewSnapshotTest>(
                test,
                new MsTestAdapter(typeof(BaseViewSnapshot)),
                new MsTestContextAdapter(context),
                recordMode ?? RecordMode,
                suffix,
                tolerance ?? Tolerance);
        }

        // TODO: Collapse this implementation with GenerateBitmap bellow
        public static void SnapshotTest([NotNull] BaseView baseView, [NotNull] TestContext context, [NotNull] Action<IImageResourceManager> testSetup, string suffix = "", float? tolerance = null, bool? recordMode = null)
        {
            // Create a temporary Direct3D rendering context and render the view on it
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = PixelFormat.Format32bppPBGRA;

            int width = (int)Math.Round(baseView.Width);
            int height = (int)Math.Round(baseView.Height);

            using (var imgFactory = new ImagingFactory())
            using (var wicBitmap = new SharpDX.WIC.Bitmap(imgFactory, width, height, pixelFormat, bitmapCreateCacheOption))
            using (var factory = new SharpDX.Direct2D1.Factory())
            using (var renderLoop = new Direct2DWicBitmapRenderManager(wicBitmap, factory))
            using (var renderer = new TestDirect2DRenderManager())
            {
                ControlView.TextLayoutRenderer = renderer;

                var last = LabelView.defaultTextSizeProvider;
                LabelView.defaultTextSizeProvider = renderer.TextSizeProvider;

                renderLoop.Initialize();
                renderer.Initialize(renderLoop.D2DRenderState, new FullClippingRegion());

                renderLoop.RenderSingleFrame(state =>
                {
                    testSetup(renderer.ImageResources);

                    var visitor = new ViewRenderingVisitor();

                    var direct2DRenderer = new WrappedDirect2DRenderer((IDirect2DRenderingState)state, (ImageResources)renderer.ImageResources);
                    var renderContext = new ControlRenderingContext(
                        direct2DRenderer, renderer.ClippingRegion,
                        renderer.TextMetricsProvider, renderer.ImageResources, renderer);
                    var traverser = new BaseViewTraveler<ControlRenderingContext>(renderContext, visitor);

                    traverser.Visit(baseView);
                });

                LabelView.defaultTextSizeProvider = last;

                var bitmap = new Bitmap(wicBitmap.Size.Width, wicBitmap.Size.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (var wicBitmapLock = wicBitmap.Lock(BitmapLockFlags.Read))
                using (var bitmapLock = bitmap.FastLock())
                {
                    unchecked
                    {
                        const int bytesPerPixel = 4; // ARGB
                        ulong length = (ulong)(wicBitmap.Size.Width * wicBitmap.Size.Height * bytesPerPixel);
                        FastBitmap.memcpy(bitmapLock.Scan0, wicBitmapLock.Data.DataPointer, length);
                    }
                }

                BitmapSnapshotTesting.Snapshot<BitmapSnapshot, Bitmap>(
                    bitmap,
                    new MsTestAdapter(typeof(BaseViewSnapshot)),
                    new MsTestContextAdapter(context),
                    recordMode ?? RecordMode,
                    suffix,
                    tolerance ?? Tolerance);
            }
        }
        
        public Bitmap GenerateBitmap(BaseViewSnapshotTest test)
        {
            var view = test.BaseView;

            // Create a temporary Direct3D rendering context and render the view on it
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = PixelFormat.Format32bppPBGRA;

            int width = (int) Math.Round(view.Width);
            int height = (int) Math.Round(view.Height);

            using (var imgFactory = new ImagingFactory())
            using (var wicBitmap = new SharpDX.WIC.Bitmap(imgFactory, width, height, pixelFormat, bitmapCreateCacheOption))
            using (var factory = new SharpDX.Direct2D1.Factory())
            using (var renderLoop = new Direct2DWicBitmapRenderManager(wicBitmap, factory))
            using (var renderer = new TestDirect2DRenderManager())
            {
                ControlView.TextLayoutRenderer = renderer;

                var last = LabelView.defaultTextSizeProvider;
                LabelView.defaultTextSizeProvider = renderer.TextSizeProvider;

                renderLoop.Initialize();

                test.Resources?.Register(renderLoop.D2DRenderState, renderer.ImageResources);

                renderer.Initialize(renderLoop.D2DRenderState, new FullClippingRegion());

                renderLoop.RenderSingleFrame(state =>
                {
                    var visitor = new ViewRenderingVisitor();

                    var context = new ControlRenderingContext(
                        new WrappedDirect2DRenderer((IDirect2DRenderingState)state, (ImageResources)renderer.ImageResources), renderer.ClippingRegion,
                        renderer.TextMetricsProvider, renderer.ImageResources, renderer);
                    var traverser = new BaseViewTraveler<ControlRenderingContext>(context, visitor);

                    traverser.Visit(view);
                });

                LabelView.defaultTextSizeProvider = last;

                var bitmap = new Bitmap(wicBitmap.Size.Width, wicBitmap.Size.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                using (var wicBitmapLock = wicBitmap.Lock(BitmapLockFlags.Read))
                using (var bitmapLock = bitmap.FastLock())
                {
                    unchecked
                    {
                        const int bytesPerPixel = 4; // ARGB
                        ulong length = (ulong) (wicBitmap.Size.Width * wicBitmap.Size.Height * bytesPerPixel);
                        FastBitmap.memcpy(bitmapLock.Scan0, wicBitmapLock.Data.DataPointer, length);
                    }
                }

                return bitmap;
            }
        }
    }

    public class BaseViewSnapshotTest
    {
        public BaseView BaseView { get; }
        [CanBeNull]
        public BaseViewSnapshotResources Resources { get; }

        public BaseViewSnapshotTest(BaseView baseView, [CanBeNull] BaseViewSnapshotResources resources)
        {
            BaseView = baseView;
            Resources = resources;
        }
    }

    public class BaseViewSnapshotResources
    {
        private readonly Dictionary<string, Bitmap> _resources = new Dictionary<string, Bitmap>();

        public ImageResource CreateImageResource([NotNull] string name, [NotNull] Bitmap bitmap)
        {
            var resource = new ImageResource(name, bitmap.Width, bitmap.Height);
            _resources[name] = bitmap;

            return resource;
        }

        public void Register(IRenderLoopState state, IImageResourceManager manager)
        {
            foreach (var keyValuePair in _resources)
            {
                manager.AddImageResource(state, keyValuePair.Value, keyValuePair.Key);
            }
        }
    }
}
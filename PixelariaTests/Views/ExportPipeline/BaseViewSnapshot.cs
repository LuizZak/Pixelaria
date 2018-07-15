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

using System;
using System.Drawing;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixDirectX.Rendering;
using PixSnapshot;
using PixUI;
using PixUI.Controls;
using PixUI.Rendering;
using PixUI.Visitor;
using SharpDX.WIC;
using Bitmap = System.Drawing.Bitmap;

namespace PixelariaTests.Views.ExportPipeline
{
    // TODO: Try to collapse the implementation of this class with similar PixUITest's implementation.

    /// <inheritdoc />
    /// <summary>
    /// Helper static class to perform bitmap-based rendering comparisons of <see cref="T:PixUI.Controls.SelfRenderingBaseView" /> instances
    /// (mostly <see cref="T:PixUI.Controls.ControlView" /> subclasses) to assert visual and style consistency.
    /// </summary>
    public class BaseViewSnapshot : ISnapshotProvider<BaseView>
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

        [CanBeNull]
        public static Action<ID2DImageResourceManager, IDirect2DRenderingState> ImagesConfig;
        
        public static void Snapshot([NotNull] BaseView view, [NotNull] TestContext context, string suffix = "", float? tolerance = null)
        {
            BitmapSnapshotTesting.Snapshot<BaseViewSnapshot, BaseView>(
                view,
                new MsTestAdapter(typeof(BaseViewSnapshot)), 
                new MsTestContextAdapter(context), 
                RecordMode, 
                suffix,
                tolerance ?? Tolerance);
        }
        
        public Bitmap GenerateBitmap(BaseView view)
        {
            // Create a temporary Direct3D rendering context and render the view on it
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = PixelFormat.Format32bppPBGRA;

            int width = (int) Math.Round(view.Width);
            int height = (int)Math.Round(view.Height);

            using (var imgFactory = new ImagingFactory())
            using (var directWrite = new SharpDX.DirectWrite.Factory())
            using (var wicBitmap = new SharpDX.WIC.Bitmap(imgFactory, width, height, pixelFormat, bitmapCreateCacheOption))
            using (var renderLoop = new Direct2DWicBitmapRenderManager(wicBitmap))
            using (var renderer = new TestDirect2DRenderer())
            {
                ControlView.DirectWriteFactory = directWrite;

                var last = LabelView.DefaultLabelViewSizeProvider;
                LabelView.DefaultLabelViewSizeProvider = renderer.SizeProvider;

                renderLoop.InitializeDirect2D();

                ImagesConfig?.Invoke(renderer.ImageResources, renderLoop.RenderingState);
                ImagesConfig = null; // Always erase after each snapshot to make sure we don't accidentally carry over resources across snapshot tests

                renderer.Initialize(renderLoop.RenderingState, new FullClipping());

                renderLoop.RenderSingleFrame(state =>
                {
                    var visitor = new ViewRenderingVisitor();

                    var context = new ControlRenderingContext(state, renderer, renderer.TextMetricsProvider);
                    var traverser = new BaseViewTraverser<ControlRenderingContext>(context, visitor);

                    traverser.Visit(view);
                });

                LabelView.DefaultLabelViewSizeProvider = last;

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
        
        private class FullClipping : IClippingRegion
        {
            public bool IsVisibleInClippingRegion(Rectangle rectangle)
            {
                return true;
            }

            public bool IsVisibleInClippingRegion(Point point)
            {
                return true;
            }

            public bool IsVisibleInClippingRegion(AABB aabb)
            {
                return true;
            }

            public bool IsVisibleInClippingRegion(Vector point)
            {
                return true;
            }

            public bool IsVisibleInClippingRegion(AABB aabb, ISpatialReference reference)
            {
                return true;
            }

            public bool IsVisibleInClippingRegion(Vector point, ISpatialReference reference)
            {
                return true;
            }
        }
    }
}
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixDirectX.Rendering;
using PixUI;
using PixUI.Controls;
using PixUI.Rendering;
using PixUI.Visitor;
using SharpDX.WIC;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace PixUITests.TestUtils
{
    /// <summary>
    /// Helper static class to perform bitmap-based rendering comparisons of <see cref="SelfRenderingBaseView"/> instances
    /// (mostly <see cref="ControlView"/> subclasses) to assert visual and style consistency.
    /// </summary>
    public static class BaseViewSnapshot
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
        /// If true, when generating output folders for test results, paths are created for each segment of the namespace
        /// of the target test class, e.g. 'PixUI.Controls.LabelControlViewTests' becomes '...\PixUI\Controls\LabelControlViewtests\',
        /// otherwise a single folder with the fully-qualified class name is used instead.
        /// 
        /// If this property is changed across test recordings, the tests must be re-recorded to account for the new directory paths
        /// expected by the snapshot class.
        /// 
        /// Defaults to false.
        /// </summary>
        public static bool SeparateDirectoriesPerNamespace = false;

        public static void Snapshot([NotNull] BaseView view, [NotNull] TestContext context)
        {
            if(view.Bounds.IsEmpty)
                throw new ArgumentException(@"View parameter cannot have empty bounds", nameof(view));
            
            string targetPath = CombinedTestResultPath(TestResultsPath(), context);

            // Verify path exists
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            string testFileName = context.TestName + ".png";
            string testFilePath = Path.Combine(targetPath, testFileName);

            // Verify comparison file's existence (if not in record mode)
            if (!RecordMode)
            {
                if(!File.Exists(testFilePath))
                    Assert.Fail($"Could not find reference image file {testFilePath} to compare. Please re-run the test with {nameof(RecordMode)} set to true to record a test result to compare later.");
            }
            
            var image = SnapshotView(view);

            if (RecordMode)
            {
                image.Save(testFilePath, ImageFormat.Png);

                Assert.Fail(
                    $"Saved image to path {testFilePath}. Re-run test mode with {nameof(RecordMode)} set to false to start comparing with record test result.");
            }
            else
            {
                // Load recorded image and compare
                using (var expected = (Bitmap)Image.FromFile(testFilePath))
                using (var expLock = expected.FastLock())
                using (var actLock = image.FastLock())
                {
                    bool areEqual = expLock.DataArray.SequenceEqual(actLock.DataArray);

                    if (areEqual)
                        return;

                    // Save to test results directory for further inspection
                    string directoryName = CombinedTestResultPath(context.TestRunResultsDirectory, context);
                    string baseFileName = Path.ChangeExtension(testFileName, null);

                    string savePathExpected = Path.Combine(directoryName, Path.ChangeExtension(baseFileName + "-expected", ".png"));
                    string savePathActual = Path.Combine(directoryName, Path.ChangeExtension(baseFileName + "-actual", ".png"));

                    // Ensure path exists
                    if (!Directory.Exists(directoryName))
                    {
                        Assert.IsNotNull(directoryName, "directoryName != null");
                        Directory.CreateDirectory(directoryName);
                    }

                    image.Save(savePathActual, ImageFormat.Png);
                    expected.Save(savePathExpected, ImageFormat.Png);

                    context.AddResultFile(savePathActual);

                    Assert.Fail($"Resulted view did not match expected image. Inspect results under directory {directoryName} for info about results");
                }
            }
        }

        private static Bitmap SnapshotView([NotNull] BaseView view)
        {
            // Create a temporary Direct3D rendering context and render the view on it
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = SharpDX.WIC.PixelFormat.Format32bppPBGRA;

            int width = (int) Math.Round(view.Width);
            int height = (int)Math.Round(view.Height);

            using (var imgFactory = new ImagingFactory())
            using (var wicBitmap = new SharpDX.WIC.Bitmap(imgFactory, width, height, pixelFormat, bitmapCreateCacheOption))
            using (var renderLoop = new Direct2DWicBitmapRenderManager(wicBitmap))
            using (var renderer = new TestDirect2DRenderer())
            {
                var last = LabelView.DefaultLabelViewSizeProvider;
                LabelView.DefaultLabelViewSizeProvider = renderer.SizeProvider;

                renderLoop.InitializeDirect2D();

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

        private static string CombinedTestResultPath([NotNull] string basePath, [NotNull] TestContext context)
        {
            if(!SeparateDirectoriesPerNamespace)
                return Path.Combine(basePath, context.FullyQualifiedTestClassName);

            var segments = context.FullyQualifiedTestClassName.Split('.');

            return Path.Combine(new[] {basePath}.Concat(segments).ToArray());
        }

        private static string TestResultsPath()
        {
            string path = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            
            if(!path.EndsWith("bin\\Debug") && !path.EndsWith("bin\\Release"))
                Assert.Fail($"Invalid/unrecognized test assembly path {path}: Path must end in either bin\\Debug or bin\\Release");

            path = Path.GetFullPath(Path.Combine(path, "..\\..\\Snapshot\\Files"));

            return path;
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
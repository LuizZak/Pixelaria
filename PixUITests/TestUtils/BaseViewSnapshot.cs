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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixUI;
using PixUI.Controls;
using PixUI.Rendering;
using PixUI.Utils;
using PixUI.Visitor;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = System.Drawing.Bitmap;
using Device = SharpDX.Direct3D11.Device;
using Factory2 = SharpDX.DXGI.Factory2;
using FeatureLevel = SharpDX.Direct3D.FeatureLevel;
using Image = System.Drawing.Image;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Resource = SharpDX.Direct3D11.Resource;

namespace PixUITests.TestUtils
{
    /// <summary>
    /// Helper static class to perform bitmap-based rendering comparisons of <see cref="SelfRenderingBaseView"/> instances
    /// (mostly <see cref="ControlView"/> subclasses) to assert visual and style consistency.
    /// </summary>
    public static class BaseViewSnapshot
    {
        private static Control _renderTarget = new Panel {Size = new Size(100, 100)};

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

        public static void Snapshot([NotNull] SelfRenderingBaseView view, [NotNull] TestContext context)
        {
            if(view.Bounds.IsEmpty)
                throw new ArgumentException(@"View parameter cannot have empty bounds", nameof(view));

            _renderTarget = new Panel
            {
                // Always round up to account for possible half-pixels
                Size = new Size((int)Math.Ceiling(view.Width), (int)Math.Ceiling(view.Height)) 
            };

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

        private static Bitmap SnapshotView(SelfRenderingBaseView view)
        {
            // Create a temporary Direct3D rendering context and render the view on it
            using (var renderLoop = new Direct2DControlLoopManager(_renderTarget))
            using (var renderer = new Direct2DRenderer())
            {
                renderLoop.InitializeDirect2D();

                renderer.Initialize(renderLoop.RenderingState, new FullClipping());

                renderLoop.RenderSingleFrame(state =>
                {
                    var visitor = new ViewRenderingVisitor();
                    
                    var context = new ControlRenderingContext(state, renderer);
                    var traverser = new BaseViewTraverser<ControlRenderingContext>(context, visitor);

                    traverser.Visit(view);
                });

                var wicBitmap = renderLoop.RenderingState.Bitmap;
                var bitmap = new Bitmap(wicBitmap.Size.Width, wicBitmap.Size.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                
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

        // TODO: Deal with duplication between this class and Pixelaria's Direct2DControlLoopManager.

        /// <summary>
        /// Simple helper class to initialize and run a Direct2D loop on top of a specific Windows Forms control
        /// </summary>
        internal sealed class Direct2DControlLoopManager : IDisposable
        {
            private readonly Control _target;
            private readonly Stopwatch _frameDeltaTimer = new Stopwatch();
            
            /// <summary>
            /// Gets the public interface for the rendering state of this Direct2D manager
            /// </summary>
            public Direct2DRenderingState RenderingState = new Direct2DRenderingState();

            public Direct2DControlLoopManager(Control target)
            {
                _target = target;
            }

            public void Dispose()
            {
                _frameDeltaTimer.Stop();
                RenderingState.Dispose();
            }

            /// <summary>
            /// Initializes the Direct2D rendering state, but do not start the render loop yet.
            /// </summary>
            public void InitializeDirect2D()
            {
                var featureLevels = new[]
                {
                    FeatureLevel.Level_11_1,
                    FeatureLevel.Level_11_0
                };
                const DeviceCreationFlags creationFlags = DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug;

                var d3Device = new Device(DriverType.Hardware, creationFlags, featureLevels);
                var d3Device1 = d3Device.QueryInterface<SharpDX.Direct3D11.Device1>();

                var dxgiDevice = d3Device1.QueryInterface<SharpDX.DXGI.Device1>();
                var dxgiFactory = dxgiDevice.Adapter.GetParent<Factory2>();

                // This gives DXGI_ERROR_INVALID_CALL
                var swapChainDescription = new SwapChainDescription1
                {
                    Width = _target.Width,
                    Height = _target.Height,
                    Format = Format.B8G8R8A8_UNorm,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.BackBuffer | Usage.RenderTargetOutput,
                    BufferCount = 1,
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.Sequential,
                    Flags = SwapChainFlags.AllowModeSwitch | SwapChainFlags.GdiCompatible
                };
                
                var swapChain = new SwapChain1(dxgiFactory, d3Device1, _target.Handle, ref swapChainDescription);

                // Ignore all windows events
                var factory = swapChain.GetParent<Factory2>();
                factory.MakeWindowAssociation(_target.Handle, WindowAssociationFlags.IgnoreAll);

                var d2DFactory = new SharpDX.Direct2D1.Factory();

                // New RenderTargetView from the backbuffer
                var backBuffer = Resource.FromSwapChain<Texture2D>(swapChain, 0);

                var dxgiSurface = backBuffer.QueryInterface<Surface>();

                var settings = new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied));

                /*
                var renderTarget =
                    new RenderTarget(d2DFactory, dxgiSurface, settings)
                    {
                        TextAntialiasMode = TextAntialiasMode.Cleartype
                    };
                */

                //var bitmapTarget = new BitmapRenderTarget(renderTarget, CompatibleRenderTargetOptions.GdiCompatible);

                using (var imgFactory = new ImagingFactory())
                {
                    var bitmap = new SharpDX.WIC.Bitmap(imgFactory, _target.Width, _target.Height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnDemand);

                    var bitmapTarget = new WicRenderTarget(d2DFactory, bitmap, settings);

                    var directWriteFactory = new SharpDX.DirectWrite.Factory();

                    RenderingState.Bitmap = bitmap;
                    RenderingState.D2DFactory = d2DFactory;
                    RenderingState.WicRenderTarget = bitmapTarget;
                    RenderingState.SwapChain = swapChain;
                    RenderingState.Factory = factory;
                    RenderingState.DxgiSurface = dxgiSurface;
                    RenderingState.BackBuffer = backBuffer;
                    RenderingState.DirectWriteFactory = directWriteFactory;
                }
            }

            /// <summary>
            /// Starts the render loop using a given closure as the actual content rendering delegate.
            /// 
            /// This method does not return after being called, and will continue processing Windows Form events
            /// internally until the application is closed.
            /// </summary>
            public void RenderSingleFrame([NotNull, InstantHandle] Action<IDirect2DRenderingState> render)
            {
                _frameDeltaTimer.Restart();

                RenderingState.D2DRenderTarget.BeginDraw();

                render(RenderingState);

                RenderingState.D2DRenderTarget.EndDraw();
            }
            
            internal class Direct2DRenderingState : IDirect2DRenderingState
            {
                private readonly Stack<Matrix3x2> _matrixStack = new Stack<Matrix3x2>();

                public SwapChain1 SwapChain;
                public SharpDX.DXGI.Factory Factory;

                public Surface DxgiSurface { set; get; }
                public SharpDX.Direct2D1.Factory D2DFactory { set; get; }
                public Texture2D BackBuffer { set; get; }

                public SharpDX.WIC.Bitmap Bitmap { get; set; }

                public WicRenderTarget WicRenderTarget { set; get; }
                public RenderTarget D2DRenderTarget => WicRenderTarget;
                public SharpDX.DirectWrite.Factory DirectWriteFactory { get; set; }

                /// <summary>
                /// Gets the time span since the last frame rendered
                /// </summary>
                public TimeSpan FrameRenderDeltaTime { get; set; }

                public void Dispose()
                {
                    // Release all resources
                    BackBuffer?.Dispose();
                    SwapChain?.Dispose();
                    Factory?.Dispose();
                    Bitmap?.Dispose();
                    WicRenderTarget?.Dispose();
                }
                
                public void WithTemporaryClipping(AABB clipping, [InstantHandle] Action execute)
                {
                    D2DRenderTarget.PushAxisAlignedClip(clipping.ToRawRectangleF(), AntialiasMode.Aliased);

                    execute();

                    D2DRenderTarget.PopAxisAlignedClip();
                }

                public void PushingTransform([InstantHandle] Action execute)
                {
                    var transform = D2DRenderTarget.Transform;
                    execute();
                    D2DRenderTarget.Transform = transform;
                }

                public void PushMatrix(Matrix3x2 matrix)
                {
                    _matrixStack.Push(D2DRenderTarget.Transform);

                    D2DRenderTarget.Transform = D2DRenderTarget.Transform * matrix;
                }

                public void PopMatrix()
                {
                    D2DRenderTarget.Transform = _matrixStack.Pop();
                }
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
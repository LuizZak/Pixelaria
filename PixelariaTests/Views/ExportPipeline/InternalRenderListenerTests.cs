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
using System.Drawing;
using System.Reactive.Subjects;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixCore.Geometry;
using PixDirectX.Rendering;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Inputs.Abstract;
using Pixelaria.ExportPipeline.Outputs.Abstract;
using Pixelaria.Views.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixSnapshot;
using PixUI;
using PixUI.Visitor;
using SharpDX.WIC;
using Bitmap = System.Drawing.Bitmap;

namespace PixelariaTests.Views.ExportPipeline
{
    [TestClass]
    public class InternalRenderListenerTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            // PipelineViewSnapshot.RecordMode = true;
        }

        [TestMethod]
        public void TestRenderEmptyPipelineNodeView()
        {
            var node = new TestPipelineStep();
            var view = PipelineNodeView.Create(node);

            PipelineViewSnapshot.Snapshot(view, TestContext);
        }

        [TestMethod]
        public void TestRenderPipelineNodeViewWithInput()
        {
            var node = new TestPipelineStep();
            node.InputList = new List<IPipelineInput>
            {
                new GenericPipelineInput<int>(node, "Input 1")
            };
            var view = PipelineNodeView.Create(node);

            PipelineViewSnapshot.Snapshot(view, TestContext);
        }
        
        [TestMethod]
        public void TestRenderPipelineNodeViewWithInputPushingWidth()
        {
            var node = new TestPipelineStep {Name = "Stroke"};
            node.InputList = new List<IPipelineInput>
            {
                new GenericPipelineInput<int>(node, "Knockout Image")
            };
            var view = PipelineNodeView.Create(node);

            PipelineViewSnapshot.Snapshot(view, TestContext);
        }

        [TestMethod]
        public void TestRenderPipelineNodeViewWithOutput()
        {
            var node = new TestPipelineStep();
            node.OutputList = new List<IPipelineOutput>
            {
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 1"),
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 2")
            };
            var view = PipelineNodeView.Create(node);

            PipelineViewSnapshot.Snapshot(view, TestContext);
        }

        [TestMethod]
        public void TestRenderPipelineNodeViewWithInputAndOutput()
        {
            var node = new TestPipelineStep();
            node.InputList = new List<IPipelineInput>
            {
                new GenericPipelineInput<int>(node, "Input 1")
            };
            node.OutputList = new List<IPipelineOutput>
            {
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 1"),
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 2")
            };
            var view = PipelineNodeView.Create(node);

            PipelineViewSnapshot.Snapshot(view, TestContext);
        }

        [TestMethod]
        public void TestRenderPipelineNodeViewWithInputAndOutputAndTextBody()
        {
            var node = new TestPipelineStep();
            node.InputList = new List<IPipelineInput>
            {
                new GenericPipelineInput<int>(node, "Input 1")
            };
            node.OutputList = new List<IPipelineOutput>
            {
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 1"),
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 2")
            };
            node.Metadata.Metadata[PipelineMetadataKeys.PipelineStepBodyText] = "This node takes an\ninput integer and\noutputs two strings.";
            var view = PipelineNodeView.Create(node);

            PipelineViewSnapshot.Snapshot(view, TestContext);
        }
        
        [TestMethod]
        public void TestRenderPipelineNodeViewWithInputAndOutputAndTextBodyAndIcon()
        {
            // Arrange
            var icon = new Bitmap(16, 16);
            using (var fastBitmap = icon.FastLock())
            {
                fastBitmap.Clear(Color.White);
            }

            var node = new TestPipelineStep();
            node.InputList = new List<IPipelineInput>
            {
                new GenericPipelineInput<int>(node, "Input 1")
            };
            node.OutputList = new List<IPipelineOutput>
            {
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 1"),
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 2")
            };
            node.Metadata.Metadata[PipelineMetadataKeys.PipelineStepBodyText] = "This node takes an\ninput integer and\noutputs two strings.";

            var view = PipelineNodeView.Create(node);
            view.Icon = new ImageResource("icon", icon.Width, icon.Height);

            var resources = new Dictionary<string, Bitmap> {{"icon", icon}};

            // Act/Assert
            PipelineViewSnapshot.Snapshot(new PipelineViewRenderContext(view, resources), TestContext);
        }

        [TestMethod]
        public void TestRenderPipelineNodeViewConnection()
        {
            // Arrange
            var node = new TestPipelineStep();
            node.InputList = new List<IPipelineInput>
            {
                new GenericPipelineInput<int>(node, "Input 1")
            };
            node.OutputList = new List<IPipelineOutput>
            {
                new GenericPipelineOutput<string>(node, new BehaviorSubject<string>("abc"), "Output 1")
            };

            var view1 = PipelineNodeView.Create(node);
            var view2 = PipelineNodeView.Create(node);
            var conn = PipelineNodeConnectionLineView.Create(view1.OutputViews[0], view2.InputViews[0], new PipelineLinkConnection(node.Input[0], node.Output[0], connection => { }));

            var parent = new BaseView();
            parent.AddChild(view1);
            parent.AddChild(view2);
            parent.AddChild(conn);

            view1.Location = new Vector(15, 15);
            view2.Location = new Vector(165, 15);

            parent.Size = new Size(270, 90);
            
            // Act/Assert
            PipelineViewSnapshot.Snapshot(parent, TestContext);
        }

        private class TestPipelineStep : IPipelineStep
        {
            public Guid Id { get; } = Guid.NewGuid();
            public string Name { get; set; } = "Test Pipeline Step";

            public IReadOnlyList<IPipelineInput> Input => InputList;
            public IReadOnlyList<IPipelineOutput> Output => OutputList;

            public List<IPipelineInput> InputList = new List<IPipelineInput>();
            public List<IPipelineOutput> OutputList = new List<IPipelineOutput>();

            public readonly PipelineMetadata Metadata = new PipelineMetadata();

            public IPipelineMetadata GetMetadata()
            {
                return Metadata;
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Helper static class to perform bitmap-based rendering comparisons of <see cref="T:Pixelaria.Views.ExportPipeline.PipelineView.PipelineNodeView" /> and related
    /// instances to assert visual and style consistency.
    /// </summary>
    public class PipelineViewSnapshot : ISnapshotProvider<PipelineViewRenderContext>
    {
        private readonly ExportPipelineControl _control;

        /// <summary>
        /// Whether tests are currently under record mode- under record mode, results are recorded on disk to be later
        /// compared when not in record mode.
        /// 
        /// Calls to Snapshot() always fail with an assertion during record mode.
        /// 
        /// Defaults to false.
        /// </summary>
        public static bool RecordMode;
        
        /// <summary>
        /// The default tolerance to use when comparing resulting images.
        /// </summary>
        public static float Tolerance = 0.01f;

        public static void Snapshot([NotNull] BaseView view, [NotNull] TestContext context, bool? recordMode = null, float? tolerance = null)
        {
            BitmapSnapshotTesting.Snapshot<PipelineViewSnapshot, PipelineViewRenderContext>(
                new PipelineViewRenderContext(view, null),
                new MsTestAdapter(typeof(PipelineViewSnapshot)),
                new MsTestContextAdapter(context),
                recordMode ?? RecordMode,
                tolerance: tolerance ?? Tolerance);
        }
        
        public static void Snapshot(PipelineViewRenderContext ctx, [NotNull] TestContext context, bool? recordMode = null, float? tolerance = null)
        {
            BitmapSnapshotTesting.Snapshot<PipelineViewSnapshot, PipelineViewRenderContext>(
                ctx,
                new MsTestAdapter(typeof(PipelineViewSnapshot)),
                new MsTestContextAdapter(context),
                recordMode ?? RecordMode,
                tolerance: tolerance ?? Tolerance);
        }

        public PipelineViewSnapshot()
        {
            _control = new ExportPipelineControl();
        }

        public Bitmap GenerateBitmap(PipelineViewRenderContext context)
        {
            // Create a temporary Direct3D rendering context and render the view on it
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = PixelFormat.Format32bppPBGRA;

            // Auto-size views
            var view = context.BaseView;
            
            var visitor = new BaseViewVisitor<IDirect2DRenderingState>((state, baseView) =>
            {
                switch (baseView)
                {
                    case PipelineNodeView nodeView:
                        var labelViewSizer =
                            new D2DTextSizeProvider(new StaticDirect2DRenderingStateProvider(state));

                        var sizer = new DefaultPipelineNodeViewSizer();
                        sizer.AutoSize(nodeView, labelViewSizer);
                        break;

                    case PipelineNodeConnectionLineView connectionView:
                        connectionView.UpdateBezier();
                        break;
                }
            });

            using (var factory = new SharpDX.Direct2D1.Factory())
            using (var renderManager = new Direct2DRenderLoopManager(_control, factory))
            {
                renderManager.Initialize();
                var renderer = new TestDirect2DRender();
                renderer.Initialize(renderManager.RenderingState);
                renderManager.RenderSingleFrame(state =>
                {
                    _control.InitializeRenderer(renderer);
                    if (context.ImageResources != null)
                    {
                        foreach (var pair in context.ImageResources)
                        {
                            _control.ImageResources.AddImageResource(state, pair.Value, pair.Key);
                        }
                    }

                    var traverser = new BaseViewTraverser<IDirect2DRenderingState>((IDirect2DRenderingState)state, visitor);

                    traverser.Visit(view);
                });
            }
            
            int width = (int) Math.Ceiling(view.Width);
            int height = (int) Math.Ceiling(view.Height);

            if(width <= 0 || height <= 0)
                Assert.Fail($@"Width and height of view must be > 0, received {{width: {width}, height: {height}}}");

            using (var imgFactory = new ImagingFactory())
            using (var wicBitmap = new SharpDX.WIC.Bitmap(imgFactory, width, height, pixelFormat, bitmapCreateCacheOption))
            using (var factory = new SharpDX.Direct2D1.Factory())
            using (var renderManager = new Direct2DWicBitmapRenderManager(wicBitmap, factory))
            using (var renderer = new BaseDirect2DRender())
            {
                renderManager.Initialize();

                var listener = new InternalRenderListener(_control.PipelineContainer, _control);
                renderer.AddRenderListener(listener);
                
                renderManager.RenderSingleFrame(state =>
                {
                    renderer.Initialize(renderManager.D2DRenderState);
                    renderer.UpdateRenderingState(state, new FullClipping());

                    var parameters = renderer.CreateRenderListenerParameters((IDirect2DRenderingState)state);

                    PipelineControlConfigurator.RegisterIcons(renderer.ImageResources, state);
                    
                    if (context.ImageResources != null)
                    {
                        foreach (var pair in context.ImageResources)
                        {
                            renderer.ImageResources.AddImageResource(state, pair.Value, pair.Key);
                        }
                    }

                    if (view is PipelineNodeView)
                    {
                        var parentView = new BaseView();
                        parentView.AddChild(view);

                        listener.RenderInView(parentView, parameters, new IRenderingDecorator[0]);
                    }
                    else
                    {
                        listener.RenderInView(view, parameters, new IRenderingDecorator[0]);
                    }
                });
                
                return BitmapFromWicBitmap(wicBitmap);
            }
        }

        private static Bitmap BitmapFromWicBitmap([NotNull] SharpDX.WIC.Bitmap wicBitmap)
        {
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

    public struct PipelineViewRenderContext
    {
        [NotNull]
        public BaseView BaseView { get; }
        [CanBeNull]
        public IReadOnlyDictionary<string, Bitmap> ImageResources { get; }

        public PipelineViewRenderContext([NotNull] BaseView baseView, IReadOnlyDictionary<string, Bitmap> imageResources)
        {
            ImageResources = imageResources;
            BaseView = baseView;
        }
    }
}

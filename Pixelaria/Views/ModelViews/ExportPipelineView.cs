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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Point = System.Drawing.Point;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;
using System.Linq;
using System.Windows.Forms;

using JetBrains.Annotations;

using SharpDX.Direct2D1;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

using Factory = SharpDX.DXGI.Factory;

using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

using Resource = SharpDX.Direct3D11.Resource;
using Device = SharpDX.Direct3D11.Device;

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Data.Persistence;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps;
using Pixelaria.Properties;
using Pixelaria.Utils;
using Pixelaria.Views.ModelViews.PipelineView;

namespace Pixelaria.Views.ModelViews
{
    public partial class ExportPipelineView : RenderForm
    {
        public Direct2DRenderingState RenderingState { get; } = new Direct2DRenderingState();

        public ExportPipelineView()
        {
            InitializeComponent();

            exportPipelineControl.BackColor = exportPipelineControl.D2DRenderer.BackColor;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            
            Direct2DAttempt();
        }

        public void Direct2DAttempt()
        {
            // SwapChain description
            var desc = new SwapChainDescription
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(exportPipelineControl.Width, exportPipelineControl.Height,
                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = exportPipelineControl.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            // Create Device and SwapChain
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.BgraSupport, new [] { SharpDX.Direct3D.FeatureLevel.Level_10_0 }, desc, out RenderingState.Device, out RenderingState.SwapChain);

            RenderingState.D2DFactory = new SharpDX.Direct2D1.Factory();
            RenderingState.DirectWriteFactory = new SharpDX.DirectWrite.Factory();

            // Ignore all windows events
            RenderingState.Factory = RenderingState.SwapChain.GetParent<Factory>();
            RenderingState.Factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);

            // New RenderTargetView from the backbuffer
            RenderingState.BackBuffer = Resource.FromSwapChain<Texture2D>(RenderingState.SwapChain, 0);
            RenderingState.RenderTargetView = new RenderTargetView(RenderingState.Device, RenderingState.BackBuffer);

            RenderingState.DxgiSurface = RenderingState.BackBuffer.QueryInterface<Surface>();

            RenderingState.D2DRenderTarget = new RenderTarget(RenderingState.D2DFactory, RenderingState.DxgiSurface,
                new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)))
            {
                TextAntialiasMode = TextAntialiasMode.Cleartype
            };
            
            exportPipelineControl.InitializeDirect2DRenderer(RenderingState);

            ConfigurePipelineControl();
            InitTest();

            using (var renderLoop = new RenderLoop(this) { UseApplicationDoEvents = true })
            {
                while (renderLoop.NextFrame())
                {
                    RenderingState.D2DRenderTarget.BeginDraw();

                    exportPipelineControl.RenderDirect2D(RenderingState);

                    RenderingState.D2DRenderTarget.EndDraw();

                    RenderingState.SwapChain.Present(0, PresentFlags.None);
                }
            }
        }

        public void ConfigurePipelineControl()
        {
            LabelView.DefaultLabelViewSizeProvider = exportPipelineControl.D2DRenderer;

            exportPipelineControl.D2DRenderer.AddImageResource(RenderingState, Resources.anim_icon,
                "anim_icon");
            exportPipelineControl.D2DRenderer.AddImageResource(RenderingState, Resources.sheet_new,
                "sheet_new");
            exportPipelineControl.D2DRenderer.AddImageResource(RenderingState, Resources.sheet_save_icon,
                "sheet_save_icon");
            exportPipelineControl.D2DRenderer.AddImageResource(RenderingState, Resources.filter_transparency_icon,
                "filter_transparency_icon");
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        [SuppressMessage("ReSharper", "UseNullPropagation")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release all resources
                RenderingState.RenderTargetView.Dispose();
                RenderingState.BackBuffer.Dispose();
                RenderingState.Device.ImmediateContext.ClearState();
                RenderingState.Device.ImmediateContext.Flush();
                RenderingState.Device.Dispose();
                RenderingState.SwapChain.Dispose();
                RenderingState.Factory.Dispose();

                if (components != null)
                    components.Dispose();
            }
            
            base.Dispose(disposing);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (RenderingState.SwapChain != null)
            {
                RenderingState.Device.ImmediateContext.ClearState();

                RenderingState.DxgiSurface.Dispose();
                RenderingState.D2DRenderTarget.Dispose();
                RenderingState.RenderTargetView.Dispose();
                RenderingState.BackBuffer.Dispose();

                RenderingState.SwapChain.ResizeBuffers(RenderingState.SwapChain.Description.BufferCount,
                    exportPipelineControl.Width, exportPipelineControl.Height,
                    RenderingState.SwapChain.Description.ModeDescription.Format,
                    RenderingState.SwapChain.Description.Flags);

                RenderingState.BackBuffer = Resource.FromSwapChain<Texture2D>(RenderingState.SwapChain, 0);
                RenderingState.RenderTargetView = new RenderTargetView(RenderingState.Device, RenderingState.BackBuffer);
                RenderingState.DxgiSurface = RenderingState.BackBuffer.QueryInterface<Surface>();
                RenderingState.D2DRenderTarget = new RenderTarget(RenderingState.D2DFactory, RenderingState.DxgiSurface,
                    new RenderTargetProperties(new PixelFormat(Format.Unknown, AlphaMode.Premultiplied)));
            }
        }

        public void InitTest()
        {
            var anim = new Animation("Anim 1", 48, 48);

            var animNodeView = new PipelineNodeView(new SingleAnimationPipelineStep(anim))
            {
                Location = new Vector(0, 0),
                Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("anim_icon")
            };
            var animJoinerNodeView = new PipelineNodeView(new AnimationJoinerStep())
            {
                Location = new Vector(350, 30)
            };
            var sheetNodeView = new PipelineNodeView(new SpriteSheetGenerationPipelineStep())
            {
                Location = new Vector(450, 30),
                Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("sheet_new")
            };
            var fileExportView = new PipelineNodeView(new FileExportPipelineStep())
            {
                Location = new Vector(550, 30),
                Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("sheet_save_icon")
            };
            var traspFilter = new PipelineNodeView(new TransparencyFilterPipelineStep())
            {
                Location = new Vector(550, 30),
                Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("filter_transparency_icon")
            };

            exportPipelineControl.PipelineContainer.AddNodeView(animNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(animJoinerNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(sheetNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(fileExportView);
            exportPipelineControl.PipelineContainer.AddNodeView(traspFilter);

            exportPipelineControl.PipelineContainer.AutosizeNodes();

            //TestThingsAndStuff();
        }

        void TestThingsAndStuff()
        {
            var bundle = new Bundle("abc");
            var anim1 = new Animation("Anim 1", 48, 48);
            var controller = new AnimationController(bundle, anim1);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var anim2 = new Animation("Anim 2", 64, 64);
            controller = new AnimationController(bundle, anim2);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var anim3 = new Animation("Anim 3", 80, 80);
            controller = new AnimationController(bundle, anim3);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var animSteps = new[]
            {
                new SingleAnimationPipelineStep(anim1),
                new SingleAnimationPipelineStep(anim2),
                new SingleAnimationPipelineStep(anim3)
            };

            var animJoinerStep = new AnimationJoinerStep();

            var exportSettings = new AnimationSheetExportSettings
            {
                FavorRatioOverArea = true, AllowUnorderedFrames = true, ExportJson = false, ForceMinimumDimensions = false, ForcePowerOfTwoDimensions = false,
                HighPrecisionAreaMatching = false, ReuseIdenticalFramesArea = false
            };

            var sheetSettingsOutput = new StaticPipelineOutput<AnimationSheetExportSettings>(exportSettings, "Sheet Export Settings");

            var sheetStep = new SpriteSheetGenerationPipelineStep();

            var finalStep = new FileExportPipelineStep();

            // Link stuff
            foreach (var step in animSteps)
            {
                step.ConnectTo(animJoinerStep);
            }

            animJoinerStep.ConnectTo(sheetStep);
            sheetStep.SheetSettingsInput.Connect(sheetSettingsOutput);

            sheetStep.ConnectTo(finalStep);

            finalStep.Begin();
        }

        private void tsb_sortSelected_Click(object sender, EventArgs e)
        {
            exportPipelineControl.PipelineContainer.PerformAction(new SortSelectedViewsAction());
        }

        private void tab_open_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog {Filter = @"Pixelaria files (*.pxl)|*.pxl"};

            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            
            exportPipelineControl.PipelineContainer.RemoveAllViews();

            exportPipelineControl.PipelineContainer.ContentsView.Scale = Vector.Unit;
            exportPipelineControl.PipelineContainer.ContentsView.Location = Vector.Zero;

            var bundle = PixelariaSaverLoader.LoadBundleFromDisk(ofd.FileName);

            Debug.Assert(bundle != null, "bundle != null");

            exportPipelineControl.SuspendLayout();

            // Add export node from which all sheet steps will derive to
            var exportStep = new FileExportPipelineStep();
            exportPipelineControl.PipelineContainer.AddNodeView(new PipelineNodeView(exportStep)
            {
                Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("sheet_save_icon")
            });

            // Anim steps for animations w/ no owners
            foreach (var animation in bundle.Animations.Where(anim => bundle.GetOwningAnimationSheet(anim) == null))
            {
                var node = new SingleAnimationPipelineStep(animation);
                var step = new PipelineNodeView(node)
                {
                    Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("anim_icon")
                };

                exportPipelineControl.PipelineContainer.AddNodeView(step);
            }
                
            foreach (var sheet in bundle.AnimationSheets)
            {
                var sheetStep = new SpriteSheetGenerationPipelineStep();

                var animsStep = new AnimationsPipelineStep(sheet.Animations);
                    
                exportPipelineControl.PipelineContainer.AddNodeView(new PipelineNodeView(sheetStep)
                {
                    Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("sheet_new")
                });
                exportPipelineControl.PipelineContainer.AddNodeView(new PipelineNodeView(animsStep)
                {
                    Icon = exportPipelineControl.D2DRenderer.PipelineNodeImageResource("anim_icon")
                });

                exportPipelineControl.PipelineContainer.AddConnection(animsStep, sheetStep);
                exportPipelineControl.PipelineContainer.AddConnection(sheetStep, exportStep);
            }

            exportPipelineControl.PipelineContainer.AutosizeNodes();

            exportPipelineControl.PipelineContainer.PerformAction(new SortSelectedViewsAction());
        }
    }

    public partial class ExportPipelineControl: Control
    {
        private readonly Direct2DRenderer _d2DRenderer;

        private readonly InternalPipelineContainer _container;
        private readonly List<ExportPipelineUiFeature> _features = new List<ExportPipelineUiFeature>();
        private readonly Region _invalidatedRegion;

        [CanBeNull]
        private ExportPipelineUiFeature _exclusiveControl;

        public Point MousePoint { get; private set; }

        public IDirect2DRenderer D2DRenderer => _d2DRenderer;

        public IPipelineContainer PipelineContainer => _container;
        
        public ExportPipelineControl()
        {
            _container = new InternalPipelineContainer(this);

            _d2DRenderer = new Direct2DRenderer(_container, this);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            _container.Modified += ContainerOnModified;
            
            _features.Add(new DragAndDropUiFeature(this));
            _features.Add(new ViewPanAndZoomUiFeature(this));
            _features.Add(new SelectionUiFeature(this));
            _features.Add(new NodeLinkHoverLabel(this));

            _d2DRenderer.AddDecorator(new ConnectedLinksDecorator(_container));

            _invalidatedRegion = new Region(new Rectangle(Point.Empty, Size));
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _d2DRenderer.Dispose();
                _container.Modified -= ContainerOnModified;
                _invalidatedRegion.Dispose();
            }

            base.Dispose(disposing);
        }

        public void InitializeDirect2DRenderer([NotNull] Direct2DRenderingState state)
        {
            _d2DRenderer.Initialize(state);
        }

        public void RenderDirect2D([NotNull] Direct2DRenderingState state)
        {
            if(_d2DRenderer == null)
                throw new InvalidOperationException("Direct2D renderer was not initialized");

            _d2DRenderer.Render(state);
        }
        
        private void ContainerOnModified(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }
        
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            
            _invalidatedRegion.Union(new Rectangle(Point.Empty, Size));
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (var feature in _features)
                feature.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            MousePoint = e.Location;

            foreach (var feature in _features)
                feature.OnMouseClick(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            MousePoint = e.Location;

            foreach (var feature in _features)
                feature.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            MousePoint = e.Location;

            foreach (var feature in _features)
                feature.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            MousePoint = e.Location;

            foreach (var feature in _features)
                feature.OnMouseUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            MousePoint = e.Location;

            foreach (var feature in _features)
                feature.OnMouseMove(e);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            MousePoint = e.Location;

            foreach (var feature in _features)
                feature.OnMouseWheel(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            foreach (var feature in _features)
                feature.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            foreach (var feature in _features)
                feature.OnKeyUp(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            foreach (var feature in _features)
                feature.OnKeyPress(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            foreach (var feature in _features)
                feature.OnPreviewKeyDown(e);
        }
        
        /// <summary>
        /// Called to grant a UI feature exclusive access to modifying UI views.
        /// 
        /// This is used only as a managing effort for concurrent features, and can be
        /// entirely bypassed by a feature.
        /// </summary>
        private bool FeatureRequestedExclusiveControl(ExportPipelineUiFeature feature)
        {
            // Check if any control has exclusive access first (and isn't the requesting one)
            var current = CurrentExclusiveControlFeature();
            if (current != null)
            {
                return current == feature;
            }

            _exclusiveControl = feature;

            return true;
        }

        /// <summary>
        /// Returns the current feature under exclusive access, if any.
        /// </summary>
        [CanBeNull]
        private ExportPipelineUiFeature CurrentExclusiveControlFeature()
        {
            return _exclusiveControl;
        }

        /// <summary>
        /// If <see cref="feature"/> is under exclusive control, removes it back so no feature
        /// is marked as exclusive control anymore.
        /// 
        /// Does nothing if <see cref="feature"/> is not the current control under exclusive control.
        /// </summary>
        private void WaiveExclusiveControl(ExportPipelineUiFeature feature)
        {
            if (CurrentExclusiveControlFeature() == feature)
                _exclusiveControl = null;
        }

        /// <summary>
        /// Exposed interface for the pipeline step container of <see cref="ExportPipelineControl"/>
        /// </summary>
        public interface IPipelineContainer
        {
            /// <summary>
            /// Gets an array of all currently selected objects.
            /// </summary>
            [NotNull, ItemNotNull]
            object[] Selection { get; }

            /// <summary>
            /// Selection model for this pipeline container.
            /// 
            /// Encapsulate common selection querying operations.
            /// </summary>
            [NotNull]
            ISelection SelectionModel { get; }
            
            /// <summary>
            /// View contents that should be zoomable should be put on this container
            /// </summary>
            BaseView ContentsView { get; }

            /// <summary>
            /// View contents that should not scale with zooming of <see cref="ContentsView"/>.
            /// 
            /// Subviews on this view are shown over <see cref="ContentsView"/>.
            /// </summary>
            BaseView UiContainerView { get; }

            PipelineNodeView[] NodeViews { get; }

            /// <summary>
            /// Removes all views on this pipeline container
            /// </summary>
            void RemoveAllViews();

            void AddNodeView([NotNull] PipelineNodeView nodeView);

            /// <summary>
            /// Selects a given pipeline node.
            /// </summary>
            void SelectNode([NotNull] IPipelineNode node);

            /// <summary>
            /// Selects a given pipeline link.
            /// </summary>
            void SelectLink([NotNull] IPipelineNodeLink link);

            /// <summary>
            /// De-selects all pipeline nodes.
            /// </summary>
            void ClearSelection();

            /// <summary>
            /// Returns if two link views are connected
            /// </summary>
            bool AreConnected([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end);

            /// <summary>
            /// Adds a connection to the first input/output that match on two pipeline nodes
            /// </summary>
            void AddConnection([NotNull] IPipelineStep start, [NotNull] IPipelineNode end);

            /// <summary>
            /// Adds a connection between the two given pipeline links.
            /// The connection is not made if input.CanConnect(output) returns false.
            /// </summary>
            void AddConnection([NotNull] IPipelineInput input, [NotNull] IPipelineOutput output);

            /// <summary>
            /// Removes a connection from the container's model
            /// </summary>
            void RemoveConnection([NotNull] IPipelineLinkConnection connection);

            /// <summary>
            /// Invalidates all connection line views connected to a given pipeline step
            /// </summary>
            void UpdateConnectionViewsFor([NotNull] PipelineNodeView nodeView);

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's output.
            /// </summary>
            IEnumerable<PipelineNodeView> GetConnectionsFrom([NotNull] PipelineNodeView source);
            
            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's input.
            /// </summary>
            IEnumerable<PipelineNodeView> GetConnectionsTo([NotNull] PipelineNodeView source);

            /// <summary>
            /// Returns a list of node views that are directly connected to a given source node view.
            /// </summary>
            IEnumerable<PipelineNodeView> DirectlyConnectedNodeViews([NotNull] PipelineNodeView source);

            /// <summary>
            /// Returns all pipeline node link views that are connected to the given link views.
            /// </summary>
            IReadOnlyList<PipelineNodeLinkView> GetConnections([NotNull] PipelineNodeLinkView source);

            /// <summary>
            /// Retrieves the view that represents the given pipeline node within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeView ViewForPipelineNode([NotNull] IPipelineNode node);

            /// <summary>
            /// Retrieves the view that represents the given pipeline node within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeLinkView ViewForPipelineNodeLink([NotNull] IPipelineNodeLink node);

            /// <summary>
            /// Retrieves the view that represents the given pipeline connection
            /// </summary>
            [CanBeNull]
            PipelineNodeConnectionLineView ViewForPipelineConnection([NotNull] IPipelineLinkConnection node);

            /// <summary>
            /// Retrieves all combinations of link views between the two node views that are connected to one another.
            /// </summary>
            [NotNull]
            (PipelineNodeLinkView from, PipelineNodeLinkView to)[] ConnectedLinkViewsBetween(
                [NotNull] PipelineNodeView from, [NotNull] PipelineNodeView to);

            /// <summary>
            /// Performs automatic resizing of nodes
            /// </summary>
            void AutosizeNodes();

            void PerformAction([NotNull] IExportPipelineAction action);
        }

        /// <summary>
        /// For exposing selections
        /// </summary>
        public interface ISelection
        {
            /// <summary>
            /// Gets selected node views
            /// </summary>
            PipelineNodeView[] NodeViews();

            /// <summary>
            /// Gets selected node link views
            /// </summary>
            PipelineNodeLinkView[] NodeLinkViews();

            /// <summary>
            /// Gets the connection links between nodes
            /// </summary>
            PipelineNodeConnectionLineView[] NodeLinkConnections();

            /// <summary>
            /// Gets all selected objects that are currently represented as views.
            /// </summary>
            BaseView[] Views();

            /// <summary>
            /// Returns whether a given view is selected.
            /// 
            /// Only accepts selectable view types, other types return false.
            /// </summary>
            bool Contains([CanBeNull] BaseView view);
        }

        /// <summary>
        /// Container for pipeline views.
        /// 
        /// Also aids in position calculations for rendering
        /// </summary>
        private class InternalPipelineContainer : IPipelineContainer
        {
            private readonly BaseView _root = new BaseView();
            private readonly List<object> _selection = new List<object>();
            private readonly List<PipelineNodeView> _stepViews = new List<PipelineNodeView>();
            private readonly List<PipelineNodeConnectionLineView> _connectionViews =
                new List<PipelineNodeConnectionLineView>();
            private readonly _Selection _sel;
            private readonly ExportPipelineControl _control;

            public ISelection SelectionModel => _sel;

            public object[] Selection => _selection.ToArray();

            public BaseView ContentsView { get; } = new BaseView();
            public BaseView UiContainerView { get; } = new BaseView();

            public PipelineNodeView[] NodeViews => _stepViews.ToArray();

            public event EventHandler Modified;
            
            public InternalPipelineContainer(ExportPipelineControl control)
            {
                _sel = new _Selection(this);

                _root.AddChild(ContentsView);
                _root.AddChild(UiContainerView);

                _control = control;
            }
            
            public void RemoveAllViews()
            {
                ClearSelection();

                foreach (var view in ContentsView.Children.ToArray())
                {
                    view.RemoveFromParent();
                }
                foreach (var view in UiContainerView.Children.ToArray())
                {
                    view.RemoveFromParent();
                }

                foreach (var view in _root.Children.ToArray())
                {
                    view.RemoveFromParent();
                }

                _root.AddChild(ContentsView);
                _root.AddChild(UiContainerView);

                _stepViews.Clear();
                _connectionViews.Clear();
            }

            public void AddNodeView(PipelineNodeView nodeView)
            {
                ContentsView.AddChild(nodeView);
                _stepViews.Add(nodeView);

                Modified?.Invoke(this, EventArgs.Empty);
            }

            /// <summary>
            /// Selects the given view (if it's a valid selectable view).
            /// 
            /// Method may run through hierarchy to find a fit selectable view.
            /// </summary>
            public void AttemptSelect(BaseView view)
            {
                if (view is PipelineNodeView nodeView)
                {
                    SelectNode(nodeView.PipelineNode);
                }
                else if (view is PipelineNodeLinkView linkView)
                {
                    SelectLink(linkView.NodeLink);
                }
                else if (view is PipelineNodeConnectionLineView connectionView)
                {
                    SelectConnection(connectionView.Connection);
                }
            }

            public void SelectNode(IPipelineNode node)
            {
                if (_selection.Contains(node))
                    return;

                _selection.Add(node);

                var view = ViewForPipelineNode(node);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }
            }

            public void SelectLink(IPipelineNodeLink link)
            {
                if (_selection.Contains(link))
                    return;

                _selection.Add(link);
                
                var view = ViewForPipelineNodeLink(link);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;
                }
            }

            public void SelectConnection(IPipelineLinkConnection connection)
            {
                if (_selection.Contains(connection))
                    return;

                _selection.Add(connection);

                var view = ViewForPipelineConnection(connection);
                if (view != null)
                {
                    view.StrokeWidth = 5;
                    view.StrokeColor = Color.OrangeRed;
                }
            }

            public void ClearSelection()
            {
                foreach (object o in _selection.ToArray())
                {
                    if (o is IPipelineNode node)
                    {
                        var view = ViewForPipelineNode(node);
                        Deselect(view);
                    }
                    else if (o is IPipelineNodeLink link)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineNodeLink(link);
                        Deselect(view);
                    }
                    else if (o is IPipelineLinkConnection conn)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineConnection(conn);
                        Deselect(view);
                    }
                }

                _selection.Clear();
            }
            
            public void Deselect(BaseView view)
            {
                if (view is PipelineNodeView nodeView)
                {
                    if (!_selection.Contains(nodeView.PipelineNode))
                        return;

                    _selection.Remove(nodeView.PipelineNode);
                    view.StrokeWidth = 1;
                    view.StrokeColor = Color.Black;
                }
                else if (view is PipelineNodeLinkView linkView)
                {
                    if (!_selection.Contains(linkView.NodeLink))
                        return;

                    _selection.Remove(linkView.NodeLink);
                    view.StrokeWidth = 1;
                    view.StrokeColor = Color.Black;
                }
                else if (view is PipelineNodeConnectionLineView connView)
                {
                    if (!_selection.Contains(connView.Connection))
                        return;

                    _selection.Remove(connView.Connection);
                    view.StrokeWidth = 2;
                    view.StrokeColor = Color.Orange;
                }
            }

            private void AddConnectionView([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end, [NotNull] IPipelineLinkConnection connection)
            {
                // Flip start/end to always match output/input
                if (start.NodeLink is IPipelineInput && end.NodeLink is IPipelineOutput)
                {
                    (start, end) = (end, start);
                }

                var view = new PipelineNodeConnectionLineView(start, end, connection);
                _connectionViews.Add(view);

                ContentsView.AddChild(view);

                view.UpdateBezier();
            }

            public void AddConnection(IPipelineStep start, IPipelineNode end)
            {
                if (end is IPipelineStep step)
                {
                    var con = start.ConnectTo(step);
                    if (con != null)
                    {
                        var input = step.Input.First(i => i.Connections.Any(start.Output.Contains));
                        var output = input.Connections.First(start.Output.Contains);

                        var inpView = ViewForPipelineNodeLink(input);
                        var outView = ViewForPipelineNodeLink(output);

                        Debug.Assert(inpView != null, "inpView != null");
                        Debug.Assert(outView != null, "outView != null");

                        AddConnectionView(inpView, outView, con);
                    }
                }
                else if (end is IPipelineEnd endStep)
                {
                    var con = start.ConnectTo(endStep);
                    if (con != null)
                    {
                        var input = endStep.Input.First(i => i.Connections.Any(start.Output.Contains));
                        var output = input.Connections.First(start.Output.Contains);

                        var inpView = ViewForPipelineNodeLink(input);
                        var outView = ViewForPipelineNodeLink(output);

                        Debug.Assert(inpView != null, "inpView != null");
                        Debug.Assert(outView != null, "outView != null");
                        AddConnectionView(inpView, outView, con);
                    }
                }
            }

            public bool AreConnected(PipelineNodeLinkView start, PipelineNodeLinkView end)
            {
                return _connectionViews.Any(view => Equals(view.Start, start) && Equals(view.End, end) ||
                                                    Equals(view.Start, end) && Equals(view.End, start));
            }

            public void AddConnection(IPipelineInput input, IPipelineOutput output)
            {
                if (!input.CanConnect(output))
                    return;
                
                var inpView = ViewForPipelineNodeLink(input);
                var outView = ViewForPipelineNodeLink(output);

                Debug.Assert(inpView != null, "inpView != null");
                Debug.Assert(outView != null, "outView != null");

                var con = input.Connect(output);

                AddConnectionView(inpView, outView, con);
            }

            public void RemoveConnection(IPipelineLinkConnection connection)
            {
                if (!connection.Input.Connections.Contains(connection.Output))
                    return;

                // Find view and remove it
                var view = ViewForPipelineConnection(connection);
                if (view == null)
                    return;

                Deselect(view);
                connection.Disconnect();
                view.RemoveFromParent();

                _connectionViews.Remove(view);
            }

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's output.
            /// </summary>
            public IEnumerable<PipelineNodeView> GetConnectionsFrom(PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (Equals(connectionView.Start, linkView))
                            output.Add(connectionView.End.NodeView);
                    }
                }

                return output.ToArray();
            }

            /// <summary>
            /// Returns all pipeline node views that are connected to one of the given node view's input.
            /// </summary>
            public IEnumerable<PipelineNodeView> GetConnectionsTo(PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (Equals(connectionView.End, linkView))
                            output.Add(connectionView.Start.NodeView);
                    }
                }

                return output.ToArray();
            }
            
            /// <summary>
            /// Returns all pipeline node link views that are connected to the given link views.
            /// </summary>
            public IReadOnlyList<PipelineNodeLinkView> GetConnections(PipelineNodeLinkView source)
            {
                var connections = new List<PipelineNodeLinkView>();

                foreach (var connectionView in _connectionViews)
                {
                    if (Equals(connectionView.Start, source))
                    {
                        connections.Add(connectionView.End);
                    }
                    else if (Equals(connectionView.End, source))
                    {
                        connections.Add(connectionView.Start);
                    }
                }

                return connections;
            }

            /// <summary>
            /// Returns a list of node views that are directly connected to a given source node view.
            /// </summary>
            public IEnumerable<PipelineNodeView> DirectlyConnectedNodeViews(PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (Equals(connectionView.Start, linkView))
                            output.Add(connectionView.End.NodeView);
                        else if (Equals(connectionView.End, linkView))
                            output.Add(connectionView.Start.NodeView);
                    }
                }

                return output.ToArray();
            }

            /// <summary>
            /// Returns a list of all node views that are connected to a given node view, including the view itself.
            /// 
            /// Returns nodes listed from nearest to farthest from the source node.
            /// </summary>
            public IEnumerable<PipelineNodeView> NetworkForNodeView(PipelineNodeView source, IReadOnlyCollection<PipelineNodeView> except = null)
            {
                var output = new List<PipelineNodeView>();
                var queue = new Queue<PipelineNodeView>();

                queue.Enqueue(source);

                // Do a breadth-first search
                while (queue.Count > 0)
                {
                    var cur = queue.Dequeue();

                    output.Add(cur);

                    foreach (var connected in DirectlyConnectedNodeViews(cur))
                    {
                        if(!output.Contains(connected) && (except == null || !except.Contains(connected)))
                            queue.Enqueue(connected);
                    }
                }

                return output;
            }

            /// <summary>
            /// Retrieves all combinations of link views between the two node views that are connected to one another.
            /// </summary>
            public (PipelineNodeLinkView from, PipelineNodeLinkView to)[] ConnectedLinkViewsBetween(PipelineNodeView from, PipelineNodeView to)
            {
                var con = new List<(PipelineNodeLinkView from, PipelineNodeLinkView to)>();

                foreach (var linkFrom in from.GetOutputViews())
                {
                    foreach (var linkTo in to.GetInputViews())
                    {
                        if(AreConnected(linkFrom, linkTo))
                            con.Add((linkFrom, linkTo));
                    }
                }

                return con.ToArray();
            }

            /// <summary>
            /// Retrieves the view that represents the given pipeline step within this container
            /// </summary>
            public PipelineNodeView ViewForPipelineNode(IPipelineNode node)
            {
                return _stepViews.FirstOrDefault(stepView => stepView.PipelineNode == node);
            }

            /// <summary>
            /// Retrieves the view that represents the given pipeline step within this container
            /// </summary>
            public PipelineNodeLinkView ViewForPipelineNodeLink(IPipelineNodeLink node)
            {
                if (node.Node == null)
                    return null;

                return ViewForPipelineNode(node.Node)?.GetLinkViews()
                    .FirstOrDefault(linkView => linkView.NodeLink == node);
            }

            /// <summary>
            /// Retrieves the view that represents the given pipeline connection
            /// </summary>
            public PipelineNodeConnectionLineView ViewForPipelineConnection(IPipelineLinkConnection connection)
            {
                return _connectionViews.FirstOrDefault(view => view.Connection == connection);
            }

            /// <summary>
            /// Invalidates all connection line views connected to a given pipeline step
            /// </summary>
            public void UpdateConnectionViewsFor(PipelineNodeView nodeView)
            {
                foreach (var view in _connectionViews)
                {
                    if (Equals(view.Start.NodeView, nodeView) || Equals(view.End.NodeView, nodeView))
                        view.UpdateBezier();
                }
            }

            /// <summary>
            /// With a given set of link views, combines them with links/nodes under the given absolute
            /// position (in ContentsView coordinates).
            /// 
            /// Given the array of link views, makes the proper guesses of the correct input/outputs
            /// to return a set of link views to connect them to.
            /// 
            /// Returns an array that maps 1-to-1 with each input link view, with either a link view to
            /// attach to, or null, if no fit was found.
            /// 
            /// Does not return links that are part of the input set.
            /// </summary>
            [NotNull]
            [ItemCanBeNull]
            public PipelineNodeLinkView[] FindTargetsForLinkViews([NotNull, ItemNotNull] IReadOnlyCollection<PipelineNodeLinkView> linkViews, Vector position)
            {
                return
                    linkViews.Select(view =>
                        PotentialLinkViewForLinking(view, position)
                    ).Select(found => 
                        // Remove links that where part of the input set.
                        linkViews.Contains(found) ? null : found
                    ).ToArray();
            }

            [CanBeNull]
            private PipelineNodeLinkView PotentialLinkViewForLinking([NotNull] PipelineNodeLinkView linkView, Vector position)
            {
                // Start searching through link views, then we look at node views under the mouse
                // and look through their own outputs instead
                var links =
                    ContentsView
                        .ViewsUnder(position, Vector.Zero)
                        .OfType<PipelineNodeLinkView>()
                        .Concat(
                            ContentsView.ViewsUnder(position, Vector.Zero)
                                .OfType<PipelineNodeView>()
                                .SelectMany(nodeView => nodeView.GetLinkViews())
                        )
                        .Where(view => !Equals(view, linkView));

                foreach (var nodeLinkView in links)
                {
                    var linkSource = linkView.NodeLink;
                    var linkTarget = nodeLinkView.NodeLink;

                    // Avoid links that belong to the same pipeline step
                    if (nodeLinkView.NodeView.GetLinkViews().Contains(linkView))
                        continue;

                    // Avoid links that are already connected
                    if (AreConnected(linkView, nodeLinkView))
                        continue;

                    // Avoid linking inputs-to-inputs and outputs-to-outputs
                    if (!(linkSource is IPipelineInput && linkTarget is IPipelineOutput) &&
                        !(linkSource is IPipelineOutput && linkTarget is IPipelineInput))
                        continue;
                    
                    // Check type validity
                    var source = linkSource as IPipelineInput;
                    if (source != null && linkTarget is IPipelineOutput)
                    {
                        var input = source;
                        var output = (IPipelineOutput)linkTarget;

                        if (input.CanConnect(output))
                            return nodeLinkView;
                    }
                    else
                    {
                        var input = (IPipelineInput)linkTarget;
                        var output = (IPipelineOutput)linkSource;
                        
                        if (input.CanConnect(output))
                            return nodeLinkView;
                    }
                }

                return null;
            }
            
            public void AutosizeNodes()
            {
                foreach (var view in _stepViews)
                {
                    view.AutoSize(_control.D2DRenderer);
                }
            }

            public void PerformAction(IExportPipelineAction action)
            {
                action.Perform(this);
            }

            // ReSharper disable once InconsistentNaming
            private class _Selection : ISelection
            {
                private readonly InternalPipelineContainer _container;

                public _Selection(InternalPipelineContainer container)
                {
                    _container = container;
                }

                public PipelineNodeView[] NodeViews()
                {
                    return _container
                        .Selection.OfType<IPipelineNode>()
                        .Select(node => _container.ViewForPipelineNode(node))
                        .ToArray();
                }

                public PipelineNodeLinkView[] NodeLinkViews()
                {
                    return _container
                        .Selection.OfType<IPipelineNodeLink>()
                        .Select(link => _container.ViewForPipelineNodeLink(link))
                        .ToArray();
                }

                public PipelineNodeConnectionLineView[] NodeLinkConnections()
                {
                    return _container
                        .Selection.OfType<IPipelineLinkConnection>()
                        .Select(con => _container.ViewForPipelineConnection(con))
                        .ToArray();
                }

                public BaseView[] Views()
                {
                    return NodeViews().OfType<BaseView>().Concat(NodeLinkViews()).ToArray();
                }

                public bool Contains(BaseView view)
                {
                    if (view is PipelineNodeView node)
                        return _container.Selection.Contains(node.PipelineNode);
                    if (view is PipelineNodeLinkView link)
                        return _container.Selection.Contains(link.NodeLink);
                    if (view is PipelineNodeConnectionLineView con)
                        return _container.Selection.Contains(con.Connection);

                    return false;
                }
            }
        }

        /// <summary>
        /// Base class for managing UI states/functionalities based on current states of UI and keyboard/mouse.
        /// 
        /// Multiple states can be on simultaneously to satisfy multiple rendering needs.
        /// 
        /// Each state modifies the final rendering state using decorators.
        /// </summary>
        private abstract class ExportPipelineUiFeature
        {
            /// <summary>
            /// Control to manage
            /// </summary>
            [NotNull]
            protected readonly ExportPipelineControl Control;

            /// <summary>
            /// States whether this pipeline feature has exclusive UI control.
            /// 
            /// Exclusive UI control should be requested whenever a UI feature wants to do
            /// work that adds/removes views, modifies the position/scaling of views on 
            /// screen etc., which could otherwise interfere with other features' functionalities
            /// if they happen concurrently.
            /// </summary>
            protected bool hasExclusiveControl => Control.CurrentExclusiveControlFeature() == this;

            protected InternalPipelineContainer container => Control._container;
            protected BaseView contentsView => Control._container.ContentsView;
            protected BaseView uiContainerView => Control._container.UiContainerView;

            protected ExportPipelineUiFeature([NotNull] ExportPipelineControl control)
            {
                Control = control;
            }

            public virtual void OnPaint([NotNull] PaintEventArgs e) { }
            public virtual void OnMouseLeave([NotNull] EventArgs e) { }
            public virtual void OnMouseClick([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseDoubleClick([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseDown([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseUp([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseMove([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseEnter([NotNull] EventArgs e) { }
            public virtual void OnMouseWheel([NotNull] MouseEventArgs e) { }

            public virtual void OnKeyDown([NotNull] KeyEventArgs e) { }
            public virtual void OnKeyUp([NotNull] KeyEventArgs e) { }
            public virtual void OnKeyPress([NotNull] KeyPressEventArgs e) { }
            public virtual void OnPreviewKeyDown(PreviewKeyDownEventArgs e) { }

            /// <summary>
            /// Shortcut for <see cref="FeatureRequestedExclusiveControl"/>, returning whether
            /// exclusive control was granted.
            /// </summary>
            protected bool RequestExclusiveControl()
            {
                return Control.FeatureRequestedExclusiveControl(this);
            }

            /// <summary>
            /// Shortcut for <see cref="WaiveExclusiveControl"/>.
            /// 
            /// Returns exclusive control back for new requesters to pick on.
            /// </summary>
            protected void ReturnExclusiveControl()
            {
                Control.WaiveExclusiveControl(this);
            }

            /// <summary>
            /// Returns true if any feature other than this one currently has exclusive control set.
            /// 
            /// Returns false, if no control currently has exclusive control.
            /// </summary>
            protected bool OtherFeatureHasExclusiveControl()
            {
                var feature = Control.CurrentExclusiveControlFeature();
                return feature != null && feature != this;
            }

            /// <summary>
            /// Executes an action under a temporary exclusive control lock.
            /// 
            /// If no lock could be obtained, the method returns without calling <see cref="action"/>.
            /// </summary>
            protected void ExecuteWithTemporaryExclusiveControl([InstantHandle] Action action)
            {
                if (!RequestExclusiveControl())
                    return;

                action();

                ReturnExclusiveControl();
            }
        }

        private class SelectionUiFeature : ExportPipelineUiFeature, IRenderingDecorator
        {
            [CanBeNull]
            private MouseHoverState? _hovering;

            private bool _isDrawingSelection;
            private Vector _mouseDown;
            private bool _skipMouseUp;
            private HashSet<PipelineNodeView> _underSelectionArea = new HashSet<PipelineNodeView>();

            // For drawing the selection outline with
            private readonly BezierPathView _pathView = new BezierPathView
            {
                RenderOnTop = true,
                FillColor = Color.Orange.ToAhsl().WithTransparency(0.1f).ToColor()
            };

            public SelectionUiFeature([NotNull] ExportPipelineControl control) : base(control)
            {
                control._d2DRenderer.AddDecorator(this);
            }

            public override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                _hovering = null;

                SetHovering(null);
            }

            public override void OnMouseDoubleClick(MouseEventArgs e)
            {
                base.OnMouseDoubleClick(e);

                if (e.Button == MouseButtons.Left)
                {
                    // Select a network of connected views
                    var view =
                        contentsView
                            .ViewsUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5, 5))
                            .FirstOrDefault();
                    
                    if (view is PipelineNodeLinkView linkView)
                    {
                        // Select all nodes connected to the given link view
                        var connected = container.GetConnections(linkView);

                        foreach (var v in connected)
                        {
                            var network = container.NetworkForNodeView(v.NodeView, new[] {linkView.NodeView});

                            foreach (var node in network)
                            {
                                container.AttemptSelect(node);
                            }
                            
                            container.AttemptSelect(v.NodeView);
                        }
                        
                        // Drop selection of link view
                        container.Deselect(linkView);

                        _skipMouseUp = true;
                    }
                    else
                    {
                        // Select all nodes from network
                        var nodeView = (view as PipelineNodeConnectionLineView)?.Start.NodeView ?? view as PipelineNodeView;

                        if (nodeView != null)
                        {
                            var network = container.NetworkForNodeView(nodeView);

                            foreach (var node in network)
                            {
                                container.AttemptSelect(node);
                            }

                            _skipMouseUp = true;
                        }
                    }
                }
            }

            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                _skipMouseUp = false;

                if (OtherFeatureHasExclusiveControl() || e.Button != MouseButtons.Left)
                    return;
                
                _mouseDown = e.Location;

                var closestView = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5));
                var isInSelection = container.SelectionModel.Contains(closestView);

                if (!ModifierKeys.HasFlag(Keys.Shift) && !isInSelection)
                    Control._container.ClearSelection();

                // Selection
                if (ModifierKeys.HasFlag(Keys.Shift) || closestView == null)
                {
                    if (RequestExclusiveControl())
                    {
                        _isDrawingSelection = true;

                        _pathView.ClearPath();
                        contentsView.AddChild(_pathView);
                    }
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (e.Button == MouseButtons.Left && _isDrawingSelection)
                {
                    // Draw selection square
                    var area = new AABB(new[]
                    {
                        contentsView.ConvertFrom(_mouseDown, null),
                        contentsView.ConvertFrom(e.Location, null)
                    });
                    
                    _pathView.SetAsRectangle(area);

                    // Highlight all views under the selected area
                    var viewsInArea =
                        contentsView.ViewsUnder(area, Vector.Zero)
                            .OfType<PipelineNodeView>()
                            .ToArray();

                    var removed =
                        _underSelectionArea
                            .Except(viewsInArea);

                    var newViews =
                        viewsInArea
                            .Except(_underSelectionArea);

                    _underSelectionArea = new HashSet<PipelineNodeView>(viewsInArea);

                    // Create temporary strokes for displaying new selection
                    foreach (var view in newViews)
                    {
                        view.StrokeWidth = 3;
                        view.StrokeColor = Color.Orange;
                    }
                    // Invalidate views that where removed from selection area as well
                    foreach (var view in removed)
                    {
                        view.StrokeWidth = 1;
                        view.StrokeColor = Color.Black;
                    }
                }
                else if (e.Button == MouseButtons.None)
                {
                    // Check hovering a link view
                    var closest = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5));

                    if (closest != null)
                    {
                        if (closest is PipelineNodeView stepView)
                            SetHovering(stepView);
                        else if (closest is PipelineNodeLinkView linkView)
                            SetHovering(linkView);
                        else if (closest is PipelineNodeConnectionLineView pathView)
                            SetHovering(pathView);
                    }
                    else
                    {
                        SetHovering(null);
                    }
                }
            }

            public override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (_skipMouseUp)
                    return;

                if (hasExclusiveControl)
                {
                    // Handle selection of new objects if user let the mouse go over a view
                    // without moving the mouse much (click event)
                    if (_isDrawingSelection)
                    {
                        if (_mouseDown.Distance(e.Location) < 3)
                        {
                            var view = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5, 5));
                            if (view != null)
                                container.AttemptSelect(view);
                        }

                        _isDrawingSelection = false;
                        _pathView.RemoveFromParent();

                        // Append selection
                        foreach (var nodeView in _underSelectionArea)
                        {
                            container.AttemptSelect(nodeView);
                        }

                        _underSelectionArea.Clear();

                        ReturnExclusiveControl();
                    }
                }
                else if (!OtherFeatureHasExclusiveControl())
                {
                    if (_mouseDown.Distance(e.Location) < 3)
                    {
                        var view = contentsView.ViewUnder(contentsView.ConvertFrom(e.Location, null), new Vector(5, 5));
                        if (view != null)
                            container.AttemptSelect(view);
                    }
                }

                _isDrawingSelection = false;
            }

            public override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (!OtherFeatureHasExclusiveControl())
                {
                    ExecuteWithTemporaryExclusiveControl(() =>
                    {
                        if (e.KeyCode != Keys.Delete)
                            return;

                        var cons = container.SelectionModel.NodeLinkConnections();
                        if (cons.Length > 0)
                        {
                            foreach (var con in cons)
                            {
                                container.RemoveConnection(con.Connection);
                            }

                            container.ClearSelection();
                        }
                    });
                }
            }

            private void SetHovering([CanBeNull] BaseView view)
            {
                if (Equals(_hovering?.View, view))
                    return;
                
                if (view == null)
                    _hovering = null;
                else
                    _hovering = new MouseHoverState { View = view };
            }

            #region IRenderingDecorator

            public void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
            {
                if (Equals(_hovering?.View, nodeView) || Equals((_hovering?.View as PipelineNodeLinkView)?.NodeView, nodeView))
                {
                    state.StrokeWidth = 3;
                }
            }

            public void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state)
            {
                if (Equals(_hovering?.View, link))
                {
                    state.StrokeWidth = 3;
                }
            }

            public void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state)
            {
                if (Equals(_hovering?.View, link))
                {
                    state.StrokeWidth = 3;
                }
            }

            public void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
            {
                if (Equals(_hovering?.View, pathView))
                    state.StrokeWidth += 2;
            }

            public void DecorateLabelView(LabelView pathView, ref LabelViewState state)
            {
                
            }

            #endregion

            private struct MouseHoverState
            {
                [NotNull]
                public BaseView View { get; set; }
            }
        }

        private class DragAndDropUiFeature : ExportPipelineUiFeature
        {
            /// <summary>
            /// List of on-going drag operations.
            /// </summary>
            private readonly List<IDragOperation> _operations = new List<IDragOperation>();

            private bool _isMouseDown;
            private bool _isDragging;

            private Vector _mouseDownPoint;
            
            public DragAndDropUiFeature([NotNull] ExportPipelineControl control) : base(control)
            {

            }
            
            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                
                if (e.Button == MouseButtons.Left)
                {
                    _mouseDownPoint = e.Location;
                    _isDragging = false;
                    _isMouseDown = true;
                }
            }

            public override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                _isMouseDown = false;

                if (e.Button == MouseButtons.Left)
                {
                    ConcludeDragging(e.Location);
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (_isMouseDown && e.Button == MouseButtons.Left)
                {
                    // Dragging happens when a minimum distance has been travelled
                    if (!_isDragging && _mouseDownPoint.Distance(e.Location) > 3)
                    {
                        if (RequestExclusiveControl())
                            StartDragging(_mouseDownPoint);
                    }

                    // Dragging
                    if (_isDragging)
                    {
                        foreach (var operation in _operations)
                        {
                            operation.Update(e.Location);
                        }
                    }
                }
            }
            
            /// <summary>
            /// Starts dragging operations for any object selected
            /// </summary>
            private void StartDragging(Vector mousePosition)
            {
                CancelDragging();

                if (container.Selection.Length != 0)
                {
                    var nodes = container.SelectionModel.NodeViews();
                    var links = container.SelectionModel.NodeLinkViews();

                    // Dragging nodes takes precedense over dragging links
                    if (nodes.Length > 0)
                    {
                        var operation = new NodeDragOperation(container, nodes, mousePosition);
                        _operations.Add(operation);
                    }
                    else
                    {
                        var operation = new LinkConnectionDragOperation(container, links);
                        _operations.Add(operation);
                    }
                }
                else
                {
                    // No selection: find view under mouse and use that instead.
                    var position = contentsView.ConvertFrom(mousePosition, null);

                    var viewUnder = contentsView.ViewUnder(position, new Vector(5));
                    if (viewUnder is PipelineNodeView nodeView)
                    {
                        var operation = new NodeDragOperation(container, new[] {nodeView}, mousePosition);
                        _operations.Add(operation);
                    }
                    else if(viewUnder is PipelineNodeLinkView linkView)
                    {
                        var operation = new LinkConnectionDragOperation(container, new[] {linkView});
                        _operations.Add(operation);
                    }
                }

                // Nothing to drag!
                if (_operations.Count == 0)
                {
                    CancelDragging();
                }

                _isDragging = true;
            }

            /// <summary>
            /// Concludes all current dragging operations
            /// </summary>
            private void ConcludeDragging(Vector mousePosition)
            {
                if (!_isDragging)
                    return;

                foreach (var operation in _operations)
                {
                    operation.Finish(mousePosition);
                }
                
                _operations.Clear();

                _isDragging = false;
                
                ReturnExclusiveControl();
            }

            /// <summary>
            /// Cancels on-going drag operations
            /// </summary>
            private void CancelDragging()
            {
                if (!_isDragging)
                    return;

                foreach (var operation in _operations)
                {
                    operation.Cancel();
                }

                _operations.Clear();

                _isDragging = false;

                ReturnExclusiveControl();
            }

            private interface IDragOperation
            {
                IReadOnlyList<object> TargetObjects { get; }

                /// <summary>
                /// Updates the on-going drag operation
                /// </summary>
                void Update(Vector mousePosition);

                /// <summary>
                /// Notifies the mouse was released on a given position
                /// </summary>
                void Finish(Vector mousePosition);

                /// <summary>
                /// Cancels the operation and undoes any changes
                /// </summary>
                void Cancel();
            }

            /// <summary>
            /// Encapsulates a drag-and-drop operation so multiple can be made at the same time.
            /// </summary>
            private sealed class NodeDragOperation : IDragOperation
            {
                /// <summary>
                /// Container used to detect drop of link connections
                /// </summary>
                private readonly InternalPipelineContainer _container;

                private readonly Vector[] _startPositions;
                
                /// <summary>
                /// Target objects for dragging
                /// </summary>
                private PipelineNodeView[] Targets { get; }

                public IReadOnlyList<object> TargetObjects => Targets;
                
                /// <summary>
                /// The node view that when dragging started was under the mouse position.
                /// 
                /// Used to guide the grid-locking of other views when they are not aligned to the grid.
                /// 
                /// In case no view was under the mouse position, the top-left-most view is used instead.
                /// </summary>
                [NotNull]
                private PipelineNodeView DragMaster { get; }

                private readonly Vector _dragMasterOffset;

                /// <summary>
                /// The relative mouse offset off of <see cref="Targets"/> when dragging started
                /// </summary>
                private readonly Vector[] _targetOffsets;
                
                public NodeDragOperation(InternalPipelineContainer container, [NotNull, ItemNotNull] PipelineNodeView[] targets, Vector dragStartMousePosition)
                {
                    Targets = targets;
                    _container = container;

                    // Find master for drag operation
                    var master = Targets.FirstOrDefault(view => view.Contains(view.ConvertFrom(dragStartMousePosition, null)));
                    DragMaster = master ?? Targets.OrderBy(view => view.Location).First();

                    _dragMasterOffset = DragMaster.ConvertFrom(dragStartMousePosition, null);
                    
                    _startPositions = Targets.Select(view => view.Location).ToArray();
                    _targetOffsets = Targets.Select(view => view.Location - DragMaster.Location).ToArray();
                }

                /// <summary>
                /// Updates the on-going drag operation
                /// </summary>
                public void Update(Vector mousePosition)
                {
                    // Drag master target around
                    var masterAbs = DragMaster.Parent?.ConvertFrom(mousePosition, null) ?? mousePosition;
                    var masterPos = masterAbs - _dragMasterOffset;

                    if (ModifierKeys.HasFlag(Keys.Control))
                        masterPos = Vector.Round(masterPos / 10) * 10;

                    DragMaster.Location = masterPos;

                    _container.UpdateConnectionViewsFor(DragMaster);
                    
                    foreach (var (view, targetOffset) in Targets.Zip(_targetOffsets, (v, p) => (v, p)))
                    {
                        if (Equals(view, DragMaster))
                            continue;
                        
                        var position = DragMaster.Location + targetOffset;
                        view.Location = position;
                        
                        _container.UpdateConnectionViewsFor(view);
                    }
                }

                /// <summary>
                /// Notifies the mouse was released on a given position
                /// </summary>
                public void Finish(Vector mousePosition)
                {
                    
                }

                /// <summary>
                /// Cancels the operation and undoes any changes
                /// </summary>
                public void Cancel()
                {
                    foreach (var (view, position) in Targets.Zip(_startPositions, (v1, v2) => (v1, v2)))
                    {
                        view.Location = position;
                    }
                }
            }

            /// <summary>
            /// Encapsulates a drag-and-drop operation so multiple can be made at the same time.
            /// </summary>
            private sealed class LinkConnectionDragOperation : IDragOperation
            {
                /// <summary>
                /// Container used to detect drop of link connections
                /// </summary>
                private readonly InternalPipelineContainer _container;
                
                [NotNull, ItemNotNull]
                private readonly BezierPathView[] _linkDrawingPaths;
                [NotNull, ItemNotNull]
                private readonly BezierPathView[] _linkConnectingPaths;
                [NotNull, ItemNotNull]
                private readonly LabelView[] _linkConnectionLabels;

                /// <summary>
                /// Target objects for dragging
                /// </summary>
                [NotNull, ItemNotNull]
                private PipelineNodeLinkView[] LinkViews { get; }

                public IReadOnlyList<object> TargetObjects => LinkViews;

                public LinkConnectionDragOperation([NotNull] InternalPipelineContainer container, [NotNull] PipelineNodeLinkView[] linkViews)
                {
                    LinkViews = linkViews;
                    _container = container;

                    _linkDrawingPaths = new BezierPathView[linkViews.Length];
                    _linkConnectingPaths = new BezierPathView[linkViews.Length];
                    _linkConnectionLabels = new LabelView[linkViews.Length];
                    for (int i = 0; i < linkViews.Length; i++)
                    {
                        var pathView = new BezierPathView();
                        container.ContentsView.AddChild(pathView);
                        _linkDrawingPaths[i] = pathView;

                        var connectionView = new BezierPathView {RenderOnTop = true};
                        container.ContentsView.AddChild(connectionView);
                        _linkConnectingPaths[i] = connectionView;

                        var label = new LabelView
                        {
                            TextColor = Color.White,
                            BackgroundColor = Color.Black.Fade(Color.Transparent, 0.1f, true),
                            Text = "",
                            Visible = false,
                            TextInsetBounds = new InsetBounds(5, 5, 5, 5)
                        };

                        container.ContentsView.AddChild(label);
                        _linkConnectionLabels[i] = label;
                    }
                }

                private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, Vector mousePosition,
                    [NotNull] BezierPathView pathView)
                {
                    pathView.ClearPath();

                    bool toRight = linkView.NodeLink is IPipelineOutput;

                    var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.ContentsView);
                    var pt4 = _container.ContentsView.ConvertFrom(mousePosition, null);
                    var pt2 = new Vector(toRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                    var pt3 = new Vector(pt1.X, pt4.Y);

                    pathView.AddBezierPoints(pt1, pt2, pt3, pt4);
                }

                private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, [NotNull] PipelineNodeLinkView targetLinkView,
                    [NotNull] BezierPathView pathView, [NotNull] BezierPathView connectView, [NotNull] LabelView labelView)
                {
                    pathView.ClearPath();
                    connectView.ClearPath();

                    bool isStartToRight = linkView.NodeLink is IPipelineOutput;
                    bool isEndToRight = targetLinkView.NodeLink is IPipelineOutput;

                    var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.ContentsView);
                    var pt4 = targetLinkView.ConvertTo(targetLinkView.Bounds.Center, _container.ContentsView);
                    var pt2 = new Vector(isStartToRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                    var pt3 = new Vector(isEndToRight ? pt4.X + 75 : pt4.X - 75, pt4.Y);

                    pathView.AddBezierPoints(pt1, pt2, pt3, pt4);

                    connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.Bounds, targetLinkView).Inflated(3, 3));
                    connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.NodeView.GetTitleArea(), targetLinkView.NodeView).Inflated(3, 3));

                    if (targetLinkView.NodeLink.Node != null)
                    {
                        labelView.Text = targetLinkView.NodeLink.Name;

                        float xOffset = isEndToRight
                            ? targetLinkView.Bounds.Width / 2 + 5
                            : -targetLinkView.Bounds.Width / 2 - labelView.Bounds.Width - 5;

                        labelView.Location =
                            _container.ContentsView.ConvertFrom(targetLinkView.Bounds.Center, targetLinkView) +
                            new Vector(xOffset, -labelView.Bounds.Height / 2);
                    }
                }

                /// <summary>
                /// Updates the on-going drag operation
                /// </summary>
                public void Update(Vector mousePosition)
                {
                    var rootPosition = _container.ContentsView.ConvertFrom(mousePosition, null);

                    // Search for possible drop positions to drop the links onto
                    var targetLinks = _container.FindTargetsForLinkViews(LinkViews, rootPosition);

                    for (int i = 0; i < LinkViews.Length; i++)
                    {
                        var linkView = LinkViews[i];
                        var path = _linkDrawingPaths[i];
                        var connectView = _linkConnectingPaths[i];
                        var labelView = _linkConnectionLabels[i];
                        var target = targetLinks[i];

                        if (target != null)
                        {
                            labelView.Visible = true;

                            UpdateLinkPreview(linkView, target, path, connectView, labelView);
                        }
                        else
                        {
                            labelView.Visible = false;
                            labelView.Location = Vector.Zero;

                            connectView.ClearPath();
                            UpdateLinkPreview(linkView, mousePosition, path);
                        }
                    }
                }

                /// <summary>
                /// Notifies the mouse was released on a given position
                /// </summary>
                public void Finish(Vector mousePosition)
                {
                    RemoveAuxiliaryViews();

                    var rootPosition = _container.ContentsView.ConvertFrom(mousePosition, null);

                    // We pick any link that isn't one of the ones that we're dragging
                    var targets = _container.FindTargetsForLinkViews(LinkViews, rootPosition);
                    
                    // Create links
                    foreach (var (linkView, target) in LinkViews.Zip(targets, (lv, t) => (lv, t)))
                    {
                        if (target == null)
                            continue;

                        IPipelineInput start;
                        IPipelineOutput end;

                        // Figure out direction of connection
                        if (linkView.NodeLink is IPipelineInput && target.NodeLink is IPipelineOutput)
                        {
                            start = (IPipelineInput) linkView.NodeLink;
                            end = (IPipelineOutput) target.NodeLink;
                        }
                        else if (linkView.NodeLink is IPipelineOutput && target.NodeLink is IPipelineInput)
                        {
                            start = (IPipelineInput) target.NodeLink;
                            end = (IPipelineOutput) linkView.NodeLink;
                        }
                        else
                        {
                            continue;
                        }

                        _container.AddConnection(start, end);
                    }
                }

                /// <summary>
                /// Cancels the operation and undoes any changes
                /// </summary>
                public void Cancel()
                {
                    RemoveAuxiliaryViews();
                }

                private void RemoveAuxiliaryViews()
                {
                    for (int i = 0; i < _linkDrawingPaths.Length; i++)
                    {
                        _linkDrawingPaths[i].RemoveFromParent();
                        _linkConnectingPaths[i].RemoveFromParent();
                        _linkConnectionLabels[i].RemoveFromParent();
                    }
                }
            }
        }

        private class NodeLinkHoverLabel : ExportPipelineUiFeature
        {
            private readonly LabelView _labelView;

            public NodeLinkHoverLabel([NotNull] ExportPipelineControl control) : base(control)
            {
                _labelView = new LabelView
                {
                    TextColor = Color.White,
                    TextFont = new Font(FontFamily.GenericSansSerif, 17),
                    BackgroundColor = Color.Black.Fade(Color.Transparent, 0.1f, true),
                    Text = "",
                    TextInsetBounds = new InsetBounds(5, 5, 5, 5)
                };
            }

            public override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                
                _labelView.RemoveFromParent();
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                var mouseLoc = e.Location;

                UpdateWithMousePosition(mouseLoc);
            }

            public override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);

                UpdateWithMousePosition(e.Location);
            }

            private void UpdateWithMousePosition(Point mouseLoc)
            {
                if (OtherFeatureHasExclusiveControl())
                {
                    _labelView.RemoveFromParent();
                }
                else
                {
                    var relativeLoc = contentsView.ConvertFrom(mouseLoc, null);

                    var view = contentsView.ViewUnder(relativeLoc, new Vector(5));
                    if (view is PipelineNodeLinkView linkView)
                    {
                        DisplayLabelForLink(linkView);
                    }
                    else
                    {
                        _labelView.RemoveFromParent();
                    }
                }
            }

            private void DisplayLabelForLink([NotNull] PipelineNodeLinkView linkView)
            {
                Type[] types;
                if (linkView.NodeLink is IPipelineOutput output)
                {
                    types = new[] { output.DataType };
                }
                else if (linkView.NodeLink is IPipelineInput input)
                {
                    types = input.DataTypes;
                }
                else
                {
                    types = new Type[0];
                }
                
                var labelText = new AttributedText();
                
                // Construct accepted types string
                var typeListText = new AttributedText();

                foreach (var type in types)
                {
                    var typeText = new AttributedText(NameForType(type), new ForegroundColorAttribute(ColorForType(type)));

                    if (typeListText.Length > 0)
                    {
                        typeListText += ", ";
                    }

                    typeListText += typeText;
                }

                // Assign label text
                bool isOutput = linkView.NodeLink is IPipelineOutput;

                labelText.Append(isOutput ? "output:" : "input:",
                    new ITextAttribute[]
                    {
                        new ForegroundColorAttribute(isOutput ? Color.MediumVioletRed : Color.ForestGreen),
                        new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 13))
                    });

                labelText.Append("\n");
                labelText.Append(linkView.NodeLink.Name);

                if (types.Length > 0)
                {
                    labelText.Append(": ");
                    labelText.Append(typeListText);
                }

                // Append connection count, if available
                int count = container.GetConnections(linkView).Count;
                if (count > 0)
                {
                    labelText.Append("\n");
                    labelText.Append($"{count} connection{(count == 1 ? "" : "s")}", new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 13)));

                    if (count == 1)
                    {
                        labelText.Append(": " + container.GetConnections(linkView).First().NodeView.Name, new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 13)));
                    }
                }

                _labelView.AttributedText.SetText(labelText);

                var absoluteLinkBounds = uiContainerView.ConvertFrom(linkView.Bounds, linkView);

                float xOffset = isOutput
                    ? absoluteLinkBounds.Width / 2 + 5
                    : -absoluteLinkBounds.Width / 2 - _labelView.Bounds.Width - 5;
                
                _labelView.Location =
                    uiContainerView.ConvertFrom(linkView.Bounds.Center, linkView) +
                    new Vector(xOffset, -_labelView.Bounds.Height / 2);

                uiContainerView.AddChild(_labelView);
            }

            private static string NameForType([NotNull] Type type)
            {
                if (type == typeof(float) || type == typeof(double))
                    return "Number";
                if (type == typeof(int))
                    return "Integer";

                return type.Name;
            }

            private static Color ColorForType(Type type)
            {
                return Color.CornflowerBlue;
            }
        }

        private class ViewPanAndZoomUiFeature : ExportPipelineUiFeature
        {
            private readonly Vector _minZoom = new Vector(0.25f);
            private Vector _dragStart;
            private Point _mouseDownPoint;
            private bool _dragging;

            public ViewPanAndZoomUiFeature([NotNull] ExportPipelineControl control)
                : base(control)
            {

            }

            public override void OnMouseClick(MouseEventArgs e)
            {
                base.OnMouseClick(e);

                if (e.Button == MouseButtons.Middle)
                {
                    // Detect we are not dragging
                    if (_dragging && e.Location.Distance(_mouseDownPoint) > 5)
                        return;
                    
                    SetZoom(Vector.Unit, ((AABB)Control.Bounds).Center);
                }
            }
            
            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                _mouseDownPoint = e.Location;

                if (e.Button == MouseButtons.Middle && RequestExclusiveControl())
                {
                    _dragStart = contentsView.Location - e.Location / contentsView.Scale;
                    _dragging = true;
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (_dragging)
                {
                    var point = (Vector)e.Location;
                    
                    contentsView.Location = _dragStart + point / contentsView.Scale;
                }
            }

            public override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                ReturnExclusiveControl();

                _dragging = false;
            }

            public override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);

                if (!OtherFeatureHasExclusiveControl())
                {
                    var scale = contentsView.Scale;
                    scale *= new Vector(1.0f + Math.Sign(e.Delta) * 0.1f);
                    if (scale < _minZoom)
                        scale = _minZoom;
                    if (scale > new Vector(25f))
                        scale = new Vector(25f);

                    SetZoom(scale, e.Location);
                }
            }

            private void SetZoom(Vector newZoom, Vector focusPosition, bool repositioning = true)
            {
                var priorPivot = contentsView.ConvertFrom(focusPosition, null);

                contentsView.Scale = newZoom;

                if (repositioning)
                {
                    var afterPivot = contentsView.ConvertFrom(focusPosition, null);

                    contentsView.Location += afterPivot - priorPivot;
                }
            }
        }

        private abstract class AbstractRenderingDecorator : IRenderingDecorator
        {
            public virtual void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
            {

            }

            public virtual void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state)
            {

            }

            public virtual void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state)
            {

            }

            public virtual void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
            {

            }

            public virtual void DecorateLabelView(LabelView pathView, ref LabelViewState state)
            {

            }
        }

        private class ConnectedLinksDecorator : AbstractRenderingDecorator
        {
            private readonly IPipelineContainer _container;

            public ConnectedLinksDecorator(IPipelineContainer container)
            {
                _container = container;
            }

            public override void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state)
            {
                if (!_container.GetConnections(link).Any())
                {
                    state.FillColor = Color.Transparent;
                    state.StrokeColor = Color.Gray;
                }
            }

            public override void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state)
            {
                if (!_container.GetConnections(link).Any())
                {
                    state.FillColor = Color.Transparent;
                    state.StrokeColor = Color.Gray;
                }
            }
        }
    }

    /// <summary>
    /// Rendering sub-part of export pipeline control
    /// </summary>
    public partial class ExportPipelineControl
    {
        /// <summary>
        /// Decorator that modifies rendering of objects in the export pipeline view.
        /// </summary>
        public interface IRenderingDecorator
        {
            void DecoratePipelineStep([NotNull] PipelineNodeView nodeView, ref PipelineStepViewState state);

            void DecoratePipelineStepInput([NotNull] PipelineNodeView nodeView, [NotNull] PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state);

            void DecoratePipelineStepOutput([NotNull] PipelineNodeView nodeView, [NotNull] PipelineNodeLinkView link,
                ref PipelineStepViewLinkState state);

            void DecorateBezierPathView([NotNull] BezierPathView pathView, ref BezierPathViewState state);

            void DecorateLabelView([NotNull] LabelView pathView, ref LabelViewState state);
        }

        public struct PipelineStepViewState
        {
            public int StrokeWidth { get; set; }
            public Color FillColor { get; set; }
            public Color TitleFillColor { get; set; }
            public Color StrokeColor { get; set; }
            public Color TitleFontColor { get; set; }
            public Color BodyFontColor { get; set; }
        }

        public struct PipelineStepViewLinkState
        {
            public int StrokeWidth { get; set; }
            public Color FillColor { get; set; }
            public Color StrokeColor { get; set; }
        }

        public struct BezierPathViewState
        {
            public int StrokeWidth { get; set; }
            public Color StrokeColor { get; set; }
            public Color FillColor { get; set; }
        }

        public struct LabelViewState
        {
            public int StrokeWidth { get; set; }
            public Color StrokeColor { get; set; }
            public Color TextColor { get; set; }
            public Color BackgroundColor { get; set; }
        }
    }

    /// <summary>
    /// Actions to be performed on a pipeline container's contents
    /// </summary>
    public interface IExportPipelineAction
    {
        void Perform([NotNull] ExportPipelineControl.IPipelineContainer container);
    }

    internal class SortSelectedViewsAction : IExportPipelineAction
    {
        public void Perform(ExportPipelineControl.IPipelineContainer container)
        {
            var selectedNodes = container.SelectionModel.NodeViews();
            var nodeViews = selectedNodes.Length > 0 ? selectedNodes : container.NodeViews;

            // No work to do
            if (nodeViews.Length == 0)
                return;

            // Create a quick graph for the views
            var root = new DirectedAcyclicNode<PipelineNodeView>(null);
            var nodes = nodeViews.Select(nodeView => new DirectedAcyclicNode<PipelineNodeView>(nodeView)).ToList();

            // Record geometric center so we can re-center later
            var originalCenter =
                nodeViews.Select(nv => nv.Center)
                    .Aggregate(Vector.Zero, (v1, v2) => v1 + v2) / nodeViews.Length;

            // Deal with connections, now
            foreach (var node in nodes)
            {
                var view = node.Value;
                if (view == null)
                    continue;

                var previous = container.GetConnectionsTo(view);
                var next = container.GetConnectionsFrom(view);

                foreach (var prevView in previous)
                {
                    var prevNode = nodes.FirstOrDefault(n => Equals(n.Value, prevView));
                    prevNode?.AddChild(node);
                }

                foreach (var nextView in next)
                {
                    var nextNode = nodes.FirstOrDefault(n => Equals(n.Value, nextView));
                    if (nextNode != null)
                        node.AddChild(nextNode);
                }

                // For nodes w/ no parents, add root as their parents
                if (node.Previous.Count == 0)
                {
                    root.AddChild(node);
                }
            }

            var sorted = root.TopologicalSorted().ToArray();
            
            // Organize the sortings, now
            float x = 0;
            foreach (var node in sorted)
            {
                var view = node.Value;
                if (view == null)
                    continue;

                view.Location = new Vector(x, 0);
                x += view.GetFullBounds().Width + 40;

                container.UpdateConnectionViewsFor(view);
            }

            // Organize Y coordinates
            for (int i = sorted.Length - 1; i >= 0; i--)
            {
                var node = sorted[i];
                var view = node.Value;
                if (view == null)
                    continue;

                var conTo = container.GetConnectionsFrom(view).FirstOrDefault();
                if (conTo == null)
                    continue;

                var connections = container.ConnectedLinkViewsBetween(view, conTo);
                if (connections.Length == 0)
                    continue;

                var (linkFrom, linkTo) = connections[0];

                float globY = linkTo.ConvertTo(linkTo.Bounds.Center, container.ContentsView).Y;

                float targetY = globY - linkFrom.Center.Y;

                view.Location = new Vector(view.Location.X, targetY);
            }
            
            // Recenter around common origin
            if (selectedNodes.Length > 0)
            {
                var newCenter =
                    nodeViews.Select(nv => nv.Center)
                        .Aggregate(Vector.Zero, (v1, v2) => v1 + v2) / nodeViews.Length;

                foreach (var view in nodeViews)
                {
                    view.Center += originalCenter - newCenter;
                }
            }

            // Update link connections
            foreach (var view in nodeViews)
            {
                container.UpdateConnectionViewsFor(view);
            }
        }
    }
}

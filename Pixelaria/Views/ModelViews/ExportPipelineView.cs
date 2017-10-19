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
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.ExportPipeline;
using Pixelaria.Properties;
using Pixelaria.Utils;
using Pixelaria.Views.ModelViews.PipelineView;

namespace Pixelaria.Views.ModelViews
{
    public partial class ExportPipelineView : Form
    {
        public ExportPipelineView()
        {
            InitializeComponent();

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            InitTest();
        }

        public void InitTest()
        {
            DoubleBuffered = true;

            var anim = new Animation("Anim 1", 48, 48);

            var animNodeView = new PipelineNodeView(new AnimationPipelineStep(anim))
            {
                Location = new Vector(0, 0),
                Size = new Vector(100, 80),
                Icon = Resources.anim_icon
            };
            var animJoinerNodeView = new PipelineNodeView(new AnimationJoinerStep())
            {
                Location = new Vector(350, 30),
                Size = new Vector(100, 80)
            };
            var sheetNodeView = new PipelineNodeView(new SpriteSheetGenerationPipelineStep())
            {
                Location = new Vector(450, 30),
                Size = new Vector(100, 80),
                Icon = Resources.sheet_new
            };
            var fileExportView = new PipelineNodeView(new FileExportPipelineStep())
            {
                Location = new Vector(550, 30),
                Size = new Vector(100, 80),
                Icon = Resources.sheet_save_icon
            };

            exportPipelineControl.PipelineContainer.AddNodeView(animNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(animJoinerNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(sheetNodeView);
            exportPipelineControl.PipelineContainer.AddNodeView(fileExportView);

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
                new AnimationPipelineStep(anim1),
                new AnimationPipelineStep(anim2),
                new AnimationPipelineStep(anim3)
            };

            var animJoinerStep = new AnimationJoinerStep();

            var exportSettings = new AnimationSheetExportSettings
            {
                FavorRatioOverArea = true, AllowUnorderedFrames = true, ExportJson = false, ForceMinimumDimensions = false, ForcePowerOfTwoDimensions = false,
                HighPrecisionAreaMatching = false, ReuseIdenticalFramesArea = false
            };

            var sheetSettingsOutput = new StaticPipelineOutput<AnimationSheetExportSettings>(exportSettings);

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
    }
    
    public class ExportPipelineControl: Control
    {
        private readonly InternalPipelineContainer _container;
        private readonly Renderer _renderer;
        private readonly List<ExportPipelineUiFeature> _features = new List<ExportPipelineUiFeature>();
        private readonly Region _invalidatedRegion;

        [CanBeNull]
        private ExportPipelineUiFeature _exclusiveControl;

        public Point MousePoint { get; private set; }
        
        public IPipelineContainer PipelineContainer => _container;
        
        public ExportPipelineControl()
        {
            _container = new InternalPipelineContainer(this);
            _renderer = new Renderer(_container, this);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            _container.Modified += ContainerOnModified;
            
            _features.Add(new DragAndDropUiFeature(this));
            _features.Add(new ViewPanAndZoomUiFeature(this));
            _features.Add(new SelectionUiFeature(this));

            _invalidatedRegion = new Region(new Rectangle(Point.Empty, Size));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container.Modified -= ContainerOnModified;
                _invalidatedRegion.Dispose();
            }

            base.Dispose(disposing);
        }
        
        /// <summary>
        /// Used as a replacement to calls to Invalidate().
        /// 
        /// This makes sure invalidated regions for next redraw operations are properly made.
        /// </summary>
        private void InvalidateEntireRegion()
        {
            InvalidateRectangle(new Rectangle(Point.Empty, Size));
        }

        /// <summary>
        /// Used as a replacement to calls to <see cref="Control.Invalidate(Rectangle)"/>.
        /// 
        /// This makes sure invalidated regions for next redraw operations are properly made.
        /// </summary>
        private void InvalidateRectangle(Rectangle rectangle)
        {
            _invalidatedRegion.Union(rectangle);

            Invalidate(rectangle);
        }

        private void ContainerOnModified(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            _renderer.Render(e.Graphics, e.ClipRectangle, _invalidatedRegion);
            foreach (var feature in _features)
                feature.OnPaint(e);

            _invalidatedRegion.MakeEmpty();
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
            /// Retrieves the view that represent the given pipeline node within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeView ViewForPipelineNode([NotNull] IPipelineNode node);

            /// <summary>
            /// Retrieves the view that represent the given pipeline node within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeLinkView ViewForPipelineNodeLink([NotNull] IPipelineNodeLink node);

            /// <summary>
            /// Performs automatic resizing of nodes
            /// </summary>
            void AutosizeNodes();
        }

        /// <summary>
        /// Container for pipeline views.
        /// 
        /// Also aids in position calculations for rendering
        /// </summary>
        private class InternalPipelineContainer : IPipelineContainer
        {
            private readonly List<object> _selection = new List<object>();

            private readonly List<PipelineNodeConnectionLineView> _connectionViews =
                new List<PipelineNodeConnectionLineView>();

            private readonly _Selection _sel;

            public ISelection SelectionModel => _sel;

            /// <summary>
            /// Control where this pipeline container is contained in
            /// </summary>
            private readonly ExportPipelineControl _control;

            public object[] Selection => _selection.ToArray();

            public BaseView Root { get; } = new BaseView();

            public readonly List<PipelineNodeView> StepViews = new List<PipelineNodeView>();

            public event EventHandler Modified;
            
            public InternalPipelineContainer(ExportPipelineControl control)
            {
                _sel = new _Selection(this);

                _control = control;
                Root.DirtyRegionUpdated += Root_DirtyRegionUpdated;
            }

            private void Root_DirtyRegionUpdated(object sender, EventArgs e)
            {
                using (var reg = Root.DirtyRegion.Clone())
                {
                    reg.Transform(Root.LocalTransform);

                    _control.Invalidate(reg);
                    _control._invalidatedRegion.Union(reg);
                }
            }

            public void AddNodeView(PipelineNodeView nodeView)
            {
                Root.AddChild(nodeView);
                StepViews.Add(nodeView);

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
            }

            public void SelectNode(IPipelineNode node)
            {
                if (_selection.Contains(node))
                    return;

                _selection.Add(node);

                // Invalidate view region
                var view = ViewForPipelineNode(node);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;

                    InvalidateViewRegion(view);
                }
            }

            public void SelectLink(IPipelineNodeLink link)
            {
                if (_selection.Contains(link))
                    return;

                _selection.Add(link);

                // Invalidate view region
                var view = ViewForPipelineNodeLink(link);
                if (view != null)
                {
                    view.StrokeWidth = 3;
                    view.StrokeColor = Color.Orange;

                    InvalidateViewRegion(view);
                }
            }

            public void ClearSelection()
            {
                foreach (object o in _selection)
                {
                    if (o is IPipelineNode node)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineNode(node);
                        if (view != null)
                        {
                            view.StrokeWidth = 1;
                            view.StrokeColor = Color.Black;
                            InvalidateViewRegion(view);
                        }
                    }
                    else if (o is IPipelineNodeLink link)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineNodeLink(link);
                        if (view != null)
                        {
                            view.StrokeWidth = 1;
                            view.StrokeColor = Color.Black;

                            InvalidateViewRegion(view);
                            InvalidateViewRegion(view.NodeView);
                        }
                    }
                }

                _selection.Clear();
            }

            public void AddConnectionView([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end)
            {
                var connection = new PipelineNodeConnectionLineView(start, end);
                _connectionViews.Add(connection);
                
                Root.InsertChild(0, connection);

                connection.UpdateBezier();
            }

            public bool AreConnected([NotNull] PipelineNodeLinkView start, [NotNull] PipelineNodeLinkView end)
            {
                return _connectionViews.Any(view => view.Start == start && view.End == end ||
                                                    view.Start == end && view.End == start);
            }

            /// <summary>
            /// Returns a list of node views that are directly connected to a given source node view.
            /// </summary>
            public IEnumerable<PipelineNodeView> DirectlyConnectedNodeViews([NotNull] PipelineNodeView source)
            {
                var output = new HashSet<PipelineNodeView>();

                foreach (var linkView in source.GetLinkViews())
                {
                    foreach (var connectionView in _connectionViews)
                    {
                        if (connectionView.Start == linkView)
                            output.Add(connectionView.End.NodeView);
                        else if (connectionView.End == linkView)
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
            public IEnumerable<PipelineNodeView> NetworkForNodeView(PipelineNodeView source)
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
                        if(!output.Contains(connected))
                            queue.Enqueue(connected);
                    }
                }

                return output;
            }

            /// <summary>
            /// Retrieves the view that represent the given pipeline step within this container
            /// </summary>
            public PipelineNodeView ViewForPipelineNode(IPipelineNode node)
            {
                return StepViews.FirstOrDefault(stepView => stepView.PipelineNode == node);
            }

            /// <summary>
            /// Retrieves the view that represent the given pipeline step within this container
            /// </summary>
            public PipelineNodeLinkView ViewForPipelineNodeLink(IPipelineNodeLink node)
            {
                if (node.Node == null)
                    return null;

                return ViewForPipelineNode(node.Node)?.GetLinkViews()
                    .FirstOrDefault(linkView => linkView.NodeLink == node);
            }
            
            /// <summary>
            /// Invalidates all connection line views connected to a given pipeline step
            /// </summary>
            public void InvalidateConnectionViewsFor(PipelineNodeView nodeView)
            {
                foreach (var view in _connectionViews)
                {
                    if (view.Start.NodeView == nodeView || view.End.NodeView == nodeView)
                        view.UpdateBezier();
                }
            }

            /// <summary>
            /// With a given set of link views, combines them with links/nodes under the given absolute
            /// position (in Root coordinates).
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
                    Root
                        .ViewsUnder(position, Vector.Zero)
                        .OfType<PipelineNodeLinkView>()
                        .Concat(
                            Root.ViewsUnder(position, Vector.Zero)
                                .OfType<PipelineNodeView>()
                                .SelectMany(nodeView => nodeView.GetLinkViews())
                        )
                        .Where(view => view != linkView);

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
                    if (linkSource is IPipelineInput && linkTarget is IPipelineOutput)
                    {
                        var input = (IPipelineInput)linkSource;
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

            /// <summary>
            /// Invalidates the region a view takes on the control
            /// </summary>
            public void InvalidateViewRegion([NotNull] BaseView view)
            {
                var rect = view.GetFullBounds().Inflated(5, 5);
                var screenBounds = rect.TransformedBounds(view.GetAbsoluteTransform());

                _control.InvalidateRectangle(Rectangle.Round((RectangleF) screenBounds));
            }

            public void AutosizeNodes()
            {
                using (var graphics = _control.CreateGraphics())
                {
                    foreach (var view in StepViews)
                    {
                        view.AutoSize(graphics);
                    }
                }
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
                /// Returns whether a given view is selected.
                /// 
                /// Only accepts selectable view types, other types return false.
                /// </summary>
                bool Contains([CanBeNull] BaseView view);
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

                public bool Contains(BaseView view)
                {
                    if (view is PipelineNodeView)
                        return NodeViews().Contains(view);
                    if (view is PipelineNodeLinkView)
                        return NodeLinkViews().Contains(view);

                    return false;
                }
            }
        }

        /// <summary>
        /// Renders a pipeline export view
        /// </summary>
        private class Renderer
        {
            /// <summary>
            /// For relative position calculations
            /// </summary>
            private readonly InternalPipelineContainer _container;

            private readonly Control _control;

            private readonly List<IRenderingDecorator> _decorators = new List<IRenderingDecorator>();

            /// <summary>
            /// List of decorators that is removed after paint operations complete
            /// </summary>
            private readonly List<IRenderingDecorator> _temporaryDecorators = new List<IRenderingDecorator>();

            /// <summary>
            /// Control-space clip rectangle for current draw operation.
            /// </summary>
            private Rectangle ClipRectangle { get; set; }
            
            public Renderer(InternalPipelineContainer container, Control control)
            {
                _container = container;
                _control = control;
            }

            public void Render([NotNull] Graphics g, Rectangle clipRectangle, [NotNull] Region clipRegion)
            {
                var decorators = _decorators.Concat(_temporaryDecorators).ToList();
                
                // Start rendering
                ClipRectangle = clipRectangle;

                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;

                // When intersecting with the clip region white line/bands start showing up
                // when redrawing.
                // Setting offset mode to .None when adding the clip region ensures this doesn't happen.
                g.PixelOffsetMode = PixelOffsetMode.None;
                g.IntersectClip(clipRegion);
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                g.WithTemporaryState(() =>
                {
                    g.WithTemporaryState(() =>
                    {
                        // Draw background across visible region
                        RenderBackground(g);
                    });

                    // Transform by base view's transform
                    g.MultiplyTransform(_container.Root.LocalTransform);

                    // Render bezier paths
                    var beziers = _container.Root.Children.OfType<BezierPathView>().ToArray();
                    var beziersLow = beziers.Where(b => !b.RenderOnTop);
                    var beziersOver = beziers.Where(b => b.RenderOnTop);
                    foreach (var bezier in beziersLow)
                    {
                        RenderBezierView(bezier, g, decorators.ToArray());
                    }

                    foreach (var stepView in _container.StepViews)
                    {
                        RenderStepView(stepView, g, decorators.ToArray());
                    }

                    foreach (var bezier in beziersOver)
                    {
                        RenderBezierView(bezier, g, decorators.ToArray());
                    }
                });

                _container.Root.ClearDirtyRegion();
                _temporaryDecorators.Clear();
            }

            public void AddDecorator(IRenderingDecorator decorator)
            {
                _decorators.Add(decorator);
            }

            public void RemoveDecorator(IRenderingDecorator decorator)
            {
                _decorators.Remove(decorator);
            }

            public void PushTemporaryDecorator(IRenderingDecorator decorator)
            {
                _temporaryDecorators.Add(decorator);
            }

            private void RenderBackground([NotNull] Graphics g)
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.PixelOffsetMode = PixelOffsetMode.None;

                var backColor = Color.FromArgb(255, 25, 25, 25);

                g.Clear(backColor);

                var scale = _container.Root.Scale;
                var gridOffset = _container.Root.Location * _container.Root.Scale;

                // Raw, non-transformed target grid separation.
                var baseGridSize = new Vector(100, 100);

                // Scale grid to increments of baseGridSize over zoom step.
                var largeGridSize = Vector.Round(baseGridSize * scale);
                var smallGridSize = largeGridSize / 10;

                var reg = new RectangleF(PointF.Empty, _control.Size);
                
                float startX = gridOffset.X % largeGridSize.X - largeGridSize.X;
                float endX = reg.Right;

                float startY = gridOffset.Y % largeGridSize.Y - largeGridSize.Y;
                float endY = reg.Bottom;
                
                // Draw small grid (when zoomed in enough)
                if (scale > new Vector(1.5f, 1.5f))
                {
                    using (var gridPen = new Pen(Color.FromArgb(40, 40, 40), 0))
                    {
                        for (var x = startX - reg.Left % smallGridSize.X; x <= endX; x += smallGridSize.X)
                        {
                            g.DrawLine(gridPen, (int)x, (int)reg.Top, (int)x, (int)reg.Bottom);
                        }

                        for (var y = startY - reg.Top % smallGridSize.Y; y <= endY; y += smallGridSize.Y)
                        {
                            g.DrawLine(gridPen, (int)reg.Left, (int)y, (int)reg.Right, (int)y);
                        }
                    }
                }

                // Draw large grid on top
                using (var gridPen = new Pen(Color.FromArgb(50, 50, 50), 0))
                {
                    for (float x = startX - reg.Left % largeGridSize.X; x <= endX; x += largeGridSize.X)
                    {
                        g.DrawLine(gridPen, (int)x, (int)reg.Top, (int)x, (int)reg.Bottom);
                    }

                    for (float y = startY - reg.Top % largeGridSize.Y; y <= endY; y += largeGridSize.Y)
                    {
                        g.DrawLine(gridPen, (int)reg.Left, (int)y, (int)reg.Right, (int)y);
                    }
                }
            }

            private void RenderStepView([NotNull] PipelineNodeView nodeView, [NotNull] Graphics g, [ItemNotNull, NotNull] IRenderingDecorator[] decorators)
            {
                g.WithTemporaryState(() =>
                {
                    g.MultiplyTransform(nodeView.LocalTransform);

                    using (var path = new GraphicsPath())
                    {
                        var visibleArea = nodeView.GetFullBounds().Corners.Transform(nodeView.GetAbsoluteTransform()).Area();
                        
                        if (!ClipRectangle.IntersectsWith((Rectangle)visibleArea))
                            return;
                        
                        // Create rendering states for decorators
                        var state = new PipelineStepViewState
                        {
                            FillColor = nodeView.Color,
                            TitleFillColor = nodeView.Color.Fade(Color.Black, 0.8f),
                            StrokeColor = nodeView.StrokeColor,
                            StrokeWidth = nodeView.StrokeWidth,
                            FontColor = Color.White
                        };

                        // Decorate
                        foreach (var decorator in decorators)
                            decorator.DecoratePipelineStep(nodeView, g, ref state);

                        var bounds = nodeView.Bounds;

                        path.AddRoundedRectangle((RectangleF)bounds, 5);

                        // Draw body outline
                        using (var brush = new LinearGradientBrush(path.GetBounds(), state.FillColor, state.FillColor.Fade(Color.Black, 0.1f), LinearGradientMode.Vertical))
                        {
                            g.FillPath(brush, path);
                        }

                        // Draw title area
                        g.WithTemporaryState(() =>
                        {
                            g.Clip = new Region(path);

                            using (var brush = new SolidBrush(state.TitleFillColor))
                                g.FillRectangle(brush, (RectangleF) nodeView.GetTitleArea());

                            float height = nodeView.GetTitleArea().Height;

                            int titleX = 4;

                            // Draw icon, if available
                            if (nodeView.Icon != null)
                            {
                                titleX += nodeView.Icon.Width + 5;

                                float imgY = height / 2 - (float) nodeView.Icon.Height / 2;

                                g.WithTemporaryState(() =>
                                {
                                    // Draw with high quality only when zoomed out
                                    if (new AABB(Vector.Zero, Vector.Unit).TransformedBounds(g.Transform).Size >=
                                        Vector.Unit)
                                    {
                                        g.InterpolationMode = InterpolationMode.NearestNeighbor;
                                    }

                                    g.DrawImage(nodeView.Icon, new PointF(imgY, imgY));
                                });
                            }

                            // Correctly position string into available area by centralizing vertically
                            var size = g.MeasureString(nodeView.Name, nodeView.Font);
                            float titleY = height / 2 - size.Height / 2;

                            using (var brush = new SolidBrush(state.FontColor))
                                g.DrawString(nodeView.Name, nodeView.Font, brush, new PointF(titleX, titleY));

                            g.Flush(FlushIntention.Sync);
                        });

                        // Draw outline now
                        using (var pen = new Pen(state.StrokeColor, state.StrokeWidth))
                            g.DrawPath(pen, path);

                        // Draw in-going and out-going links
                        var inLinks = nodeView.GetInputViews();
                        var outLinks = nodeView.GetOutputViews();

                        // Draw inputs
                        foreach (var link in inLinks)
                        {
                            g.WithTemporaryState(() =>
                            {
                                g.MultiplyTransform(link.LocalTransform);

                                var rectangle = link.Bounds;

                                var linkState = new PipelineStepViewLinkState
                                {
                                    FillColor = Color.White,
                                    StrokeColor = Color.Black,
                                    StrokeWidth = 1
                                };

                                // Decorate
                                foreach (var decorator in decorators)
                                    decorator.DecoratePipelineStepInput(nodeView, link, g, ref linkState);

                                using (var pen = new Pen(linkState.StrokeColor, linkState.StrokeWidth))
                                using (var brush = new SolidBrush(linkState.FillColor))
                                {
                                    g.FillRectangle(brush, (RectangleF)rectangle);
                                    g.DrawRectangle(pen, Rectangle.Round((RectangleF)rectangle));
                                }
                            });
                        }

                        // Draw outputs
                        foreach (var link in outLinks)
                        {
                            g.WithTemporaryState(() =>
                            {
                                g.MultiplyTransform(link.LocalTransform);

                                var rectangle = link.Bounds;

                                var linkState = new PipelineStepViewLinkState
                                {
                                    FillColor = Color.White,
                                    StrokeColor = Color.Black,
                                    StrokeWidth = 1
                                };

                                // Decorate
                                foreach (var decorator in decorators)
                                    decorator.DecoratePipelineStepOutput(nodeView, link, g, ref linkState);

                                using (var pen = new Pen(linkState.StrokeColor, linkState.StrokeWidth))
                                using (var brush = new SolidBrush(linkState.FillColor))
                                {
                                    g.FillRectangle(brush, (RectangleF)rectangle);
                                    g.DrawRectangle(pen, Rectangle.Round((RectangleF)rectangle));
                                }
                            });
                        }
                    }
                });
            }

            private void RenderBezierView([NotNull] BezierPathView bezierView, [NotNull] Graphics g, [ItemNotNull, NotNull] IRenderingDecorator[] decorators)
            {
                g.WithTemporaryState(() =>
                {
                    g.MultiplyTransform(bezierView.LocalTransform);

                    using (var path = bezierView.GetPath())
                    {
                        var visibleArea = bezierView.GetFullBounds().Corners.Transform(bezierView.GetAbsoluteTransform()).Area();

                        if (!ClipRectangle.IntersectsWith((Rectangle)visibleArea))
                            return;

                        var state = new BezierPathViewState
                        {
                            StrokeColor = bezierView.StrokeColor,
                            StrokeWidth = bezierView.StrokeWidth,
                            FillColor = bezierView.FillColor
                        };

                        // Decorate
                        foreach (var decorator in decorators)
                            decorator.DecorateBezierPathView(bezierView, g, ref state);

                        if (state.FillColor != Color.Transparent)
                        {
                            using (var brush = new SolidBrush(state.FillColor))
                            {
                                g.FillPath(brush, path);
                            }
                        }

                        using (var pen = new Pen(state.StrokeColor, state.StrokeWidth))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                });
            }

            public struct PipelineStepViewState
            {
                public int StrokeWidth { get; set; }
                public Color FillColor { get; set; }
                public Color TitleFillColor { get; set; }
                public Color StrokeColor { get; set; }
                public Color FontColor { get; set; }
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
            /// work that modifies the position/scaling of views on screen, which could otherwise
            /// interfere with other exclusive feature's functionalities.
            /// </summary>
            public bool HasExclusiveControl => Control.CurrentExclusiveControlFeature() == this;

            protected InternalPipelineContainer container => Control._container;
            protected BaseView root => Control._container.Root;

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
            /// Shortcut for <see cref="FeatureRequestedExclusiveControl"/>, with a closure that
            /// is called if access was granted.
            /// 
            /// Performs temporary exclusive control, resetting it back before returning.
            /// </summary>
            protected bool RequestTemporaryExclusiveControl(Action ifGranted)
            {
                bool hasAccess = RequestExclusiveControl();
                if (hasAccess)
                {
                    ifGranted();
                    Control.WaiveExclusiveControl(this);
                }

                return hasAccess;
            }
        }

        private class SelectionUiFeature : ExportPipelineUiFeature, IRenderingDecorator
        {
            [CanBeNull]
            private MouseHoverState? _hovering;

            private bool _isDrawingSelection;
            private Vector _mouseDown;
            private HashSet<PipelineNodeView> _underSelectionArea = new HashSet<PipelineNodeView>();

            // For drawing the selection outline with
            private readonly BezierPathView _pathView = new BezierPathView
            {
                RenderOnTop = true,
                FillColor = Color.Orange.ToAhsl().WithTransparency(0.03f).ToColor()
            };

            public SelectionUiFeature([NotNull] ExportPipelineControl control) : base(control)
            {
                control._renderer.AddDecorator(this);
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
                        root
                            .ViewsUnder(root.ConvertFrom(e.Location, null), new Vector(5, 5))
                            .FirstOrDefault();

                    var nodeView = (view as PipelineNodeConnectionLineView)?.Start.NodeView ?? view as PipelineNodeView;

                    if (nodeView != null)
                    {
                        var network = container.NetworkForNodeView(nodeView);

                        foreach (var node in network)
                        {
                            container.AttemptSelect(node);
                        }
                    }
                }
            }

            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (OtherFeatureHasExclusiveControl() || e.Button != MouseButtons.Left)
                    return;

                var closestView = root.ViewUnder(root.ConvertFrom(e.Location, null), new Vector(5));

                if (!ModifierKeys.HasFlag(Keys.Shift) && !container.SelectionModel.Contains(closestView))
                    Control._container.ClearSelection();

                // Selection
                if (!ModifierKeys.HasFlag(Keys.Shift) && closestView == null)
                {
                    if (RequestExclusiveControl())
                    {
                        _isDrawingSelection = true;
                        _mouseDown = e.Location;

                        _pathView.ClearPath();
                        root.AddChild(_pathView);
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
                        root.ConvertFrom(_mouseDown, null),
                        root.ConvertFrom(e.Location, null)
                    });
                    
                    _pathView.SetAsRectangle(area);

                    // Highlight all views under the selected area
                    var viewsInArea =
                        root.ViewsUnder(area, Vector.Zero)
                            .OfType<PipelineNodeView>()
                            .ToArray();

                    var removed =
                        _underSelectionArea
                            .Except(viewsInArea);

                    _underSelectionArea = new HashSet<PipelineNodeView>(viewsInArea);

                    // Create temporary strokes for displaying new selection
                    foreach (var view in _underSelectionArea)
                    {
                        view.StrokeWidth = 3;
                        view.StrokeColor = Color.Orange;

                        container.InvalidateViewRegion(view);
                    }
                    // Invalidate views that where removed from selection area as well
                    foreach (var view in removed)
                    {
                        view.StrokeWidth = 1;
                        view.StrokeColor = Color.Black;

                        container.InvalidateViewRegion(view);
                    }
                }
                else if (e.Button == MouseButtons.None)
                {
                    // Check hovering a link view
                    var closest = root.ViewUnder(root.ConvertFrom(e.Location, null), new Vector(5));

                    if (closest != null)
                    {
                        if (closest is PipelineNodeView stepView)
                            SetHovering(stepView);
                        else if (closest is PipelineNodeLinkView linkView)
                            SetHovering(linkView);
                        else if (closest is BezierPathView pathView)
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

                if (HasExclusiveControl)
                {
                    // Handle selection of new objects if user let the mouse go over a view
                    // without moving the mouse much (click event)
                    if (_isDrawingSelection)
                    {
                        if (_mouseDown.Distance(e.Location) < 3)
                        {
                            var view = root.ViewUnder(root.ConvertFrom(e.Location, null), new Vector(5, 5));
                            if (view != null)
                                container.AttemptSelect(view);
                        }

                        _isDrawingSelection = false;
                        _pathView.RemoveFromParent();

                        // Append selection
                        foreach (var nodeView in _underSelectionArea)
                        {
                            nodeView.StrokeWidth = 1;
                            container.AttemptSelect(nodeView);
                        }

                        _underSelectionArea.Clear();

                        ReturnExclusiveControl();
                    }
                }
            }

            private void SetHovering([CanBeNull] BaseView view)
            {
                if (_hovering?.View == view)
                    return;

                // Full invalidation contexts
                if (_hovering == null && view != null)
                {
                    Control._container.InvalidateViewRegion(view);

                    if (view is PipelineNodeLinkView linkView)
                        Control._container.InvalidateViewRegion(linkView.NodeView);
                }
                else if (_hovering != null && view == null)
                {
                    Control._container.InvalidateViewRegion(_hovering.Value.View);

                    if (_hovering.Value.View is PipelineNodeLinkView linkView)
                        Control._container.InvalidateViewRegion(linkView.NodeView);
                }
                else if (_hovering != null && view != null && _hovering.Value.View != view)
                {
                    Control._container.InvalidateViewRegion(_hovering.Value.View);
                    Control._container.InvalidateViewRegion(view);

                    if (_hovering.Value.View is PipelineNodeLinkView valueView)
                    {
                        Control._container.InvalidateViewRegion(valueView.NodeView);
                    }

                    if (view is PipelineNodeLinkView nodeLinkView)
                    {
                        Control._container.InvalidateViewRegion(nodeLinkView.NodeView);
                    }
                }

                if (view == null)
                    _hovering = null;
                else
                    _hovering = new MouseHoverState { View = view };
            }

            #region IRenderingDecorator

            public void DecoratePipelineStep(PipelineNodeView nodeView, Graphics g, ref Renderer.PipelineStepViewState state)
            {
                if (_hovering?.View == nodeView || (_hovering?.View as PipelineNodeLinkView)?.NodeView == nodeView)
                {
                    state.StrokeWidth = 3;
                }
            }

            public void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state)
            {
                if (_hovering?.View == link)
                {
                    state.StrokeWidth = 3;
                }
            }

            public void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state)
            {
                if (_hovering?.View == link)
                {
                    state.StrokeWidth = 3;
                }
            }

            public void DecorateBezierPathView(BezierPathView pathView, Graphics g,
                ref Renderer.BezierPathViewState state)
            {
                if (_hovering?.View == pathView)
                    state.StrokeWidth += 2;
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
                }
            }

            public override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (e.Button == MouseButtons.Left)
                {
                    ConcludeDragging(e.Location);
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (e.Button == MouseButtons.Left)
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
                    var position = root.ConvertFrom(mousePosition, null);

                    var viewUnder = root.ViewUnder(position, new Vector(5));
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

                    _container.InvalidateConnectionViewsFor(DragMaster);
                    
                    foreach (var (view, targetOffset) in Targets.Zip(_targetOffsets, (v, p) => (v, p)))
                    {
                        if (view == DragMaster)
                            continue;
                        
                        var position = DragMaster.Location + targetOffset;
                        view.Location = position;
                        
                        _container.InvalidateConnectionViewsFor(view);
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
                    for (int i = 0; i < linkViews.Length; i++)
                    {
                        var pathView = new BezierPathView();
                        container.Root.AddChild(pathView);
                        _linkDrawingPaths[i] = pathView;

                        var connectionView = new BezierPathView {RenderOnTop = true};
                        container.Root.AddChild(connectionView);
                        _linkConnectingPaths[i] = connectionView;
                    }
                }

                private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, Vector mousePosition,
                    [NotNull] BezierPathView pathView)
                {
                    pathView.ClearPath();

                    bool toRight = linkView.NodeLink is IPipelineOutput;

                    var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.Root);
                    var pt4 = _container.Root.ConvertFrom(mousePosition, null);
                    var pt2 = new Vector(toRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                    var pt3 = new Vector(pt1.X, pt4.Y);

                    pathView.AddBezierPoints(pt1, pt2, pt3, pt4);
                }

                private void UpdateLinkPreview([NotNull] PipelineNodeLinkView linkView, [NotNull] PipelineNodeLinkView targetLinkView,
                    [NotNull] BezierPathView pathView, [NotNull] BezierPathView connectView)
                {
                    pathView.ClearPath();
                    connectView.ClearPath();

                    bool isStartToRight = linkView.NodeLink is IPipelineOutput;
                    bool isEndToRight = targetLinkView.NodeLink is IPipelineOutput;

                    var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.Root);
                    var pt4 = targetLinkView.ConvertTo(targetLinkView.Bounds.Center, _container.Root);
                    var pt2 = new Vector(isStartToRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                    var pt3 = new Vector(isEndToRight ? pt4.X + 75 : pt4.X - 75, pt4.Y);

                    pathView.AddBezierPoints(pt1, pt2, pt3, pt4);

                    connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.Bounds, targetLinkView).Inflated(3, 3));
                    connectView.AddRectangle(connectView.ConvertFrom(targetLinkView.NodeView.GetTitleArea(), targetLinkView.NodeView).Inflated(3, 3));
                }

                /// <summary>
                /// Updates the on-going drag operation
                /// </summary>
                public void Update(Vector mousePosition)
                {
                    var rootPosition = _container.Root.ConvertFrom(mousePosition, null);

                    // Search for possible drop positions to drop the links onto
                    var targetLinks = _container.FindTargetsForLinkViews(LinkViews, rootPosition);

                    for (int i = 0; i < LinkViews.Length; i++)
                    {
                        var linkView = LinkViews[i];
                        var path = _linkDrawingPaths[i];
                        var connectView = _linkConnectingPaths[i];
                        var target = targetLinks[i];

                        if (target != null)
                            UpdateLinkPreview(linkView, target, path, connectView);
                        else
                        {
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
                    for (var i = 0; i < _linkDrawingPaths.Length; i++)
                    {
                        _linkDrawingPaths[i].RemoveFromParent();
                        _linkConnectingPaths[i].RemoveFromParent();
                    }

                    var rootPosition = _container.Root.ConvertFrom(mousePosition, null);

                    // We pick any link that isn't one of the ones that we're dragging
                    var targets = _container.FindTargetsForLinkViews(LinkViews, rootPosition);
                    
                    // Create links
                    foreach (var (linkView, target) in LinkViews.Zip(targets, (lv, t) => (lv, t)))
                    {
                        if (target != null)
                            _container.AddConnectionView(linkView, target);
                    }
                }

                /// <summary>
                /// Cancels the operation and undoes any changes
                /// </summary>
                public void Cancel()
                {
                    foreach (var path in _linkDrawingPaths)
                    {
                        path.RemoveFromParent();
                    }
                }
            }
        }

        private class ViewPanAndZoomUiFeature : ExportPipelineUiFeature
        {
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
                    _dragStart = root.Location - e.Location / root.Scale;
                    _dragging = true;
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (_dragging)
                {
                    var point = (Vector)e.Location;
                    
                    root.Location = _dragStart + point / root.Scale;

                    Control.InvalidateEntireRegion();
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
                    var scale = root.Scale;
                    scale *= new Vector(1.0f + Math.Sign(e.Delta) * 0.1f);
                    if (scale < new Vector(0.5f))
                        scale = new Vector(0.5f);
                    if (scale > new Vector(25f))
                        scale = new Vector(25f);

                    SetZoom(scale, e.Location);
                }
            }

            private void SetZoom(Vector newZoom, Vector focusPosition, bool repositioning = true)
            {
                var priorPivot = root.ConvertFrom(focusPosition, null);

                root.Scale = newZoom;

                if (repositioning)
                {
                    var afterPivot = root.ConvertFrom(focusPosition, null);

                    root.Location += afterPivot - priorPivot;
                }

                Control.InvalidateEntireRegion();
            }
        }

        /// <summary>
        /// Decorator that modifies rendering of objects in the export pipeline view.
        /// </summary>
        private interface IRenderingDecorator
        {
            void DecoratePipelineStep([NotNull] PipelineNodeView nodeView, Graphics g,
                ref Renderer.PipelineStepViewState state);

            void DecoratePipelineStepInput([NotNull] PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state);

            void DecoratePipelineStepOutput([NotNull] PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state);

            void DecorateBezierPathView([NotNull] BezierPathView pathView, Graphics g,
                ref Renderer.BezierPathViewState state);
        }
    }
}

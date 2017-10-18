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
using System.Diagnostics.CodeAnalysis;
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
        private readonly List<ExportPipelineFeature> _features = new List<ExportPipelineFeature>();
        private readonly Region _invalidatedRegion;

        public Point MousePoint { get; private set; }
        
        public IPipelineContainer PipelineContainer => _container;
        
        public ExportPipelineControl()
        {
            _container = new InternalPipelineContainer(this);
            _renderer = new Renderer(_container);

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            _container.Modified += ContainerOnModified;
            
            _features.Add(new DragAndDropFeature(this));
            _features.Add(new ViewPanAndZoomFeature(this));
            _features.Add(new SelectionFeature(this));

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

            public void SelectNode(IPipelineNode node)
            {
                _selection.Add(node);

                // Invalidate view region
                var view = ViewForPipelineNode(node);
                if (view != null)
                    InvalidateViewRegion(view);
            }

            public void SelectLink(IPipelineNodeLink link)
            {
                _selection.Add(link);

                // Invalidate view region
                var view = ViewForPipelineNodeLink(link);
                if (view != null)
                    InvalidateViewRegion(view);
            }

            public void ClearSelection()
            {
                foreach (var o in _selection)
                {
                    if (o is IPipelineNode node)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineNode(node);
                        if (view != null)
                            InvalidateViewRegion(view);
                    }
                    else if (o is IPipelineNodeLink link)
                    {
                        // Invalidate view region
                        var view = ViewForPipelineNodeLink(link);
                        if (view != null)
                        {
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

            private readonly List<IRenderingDecorator> _decorators = new List<IRenderingDecorator>();

            /// <summary>
            /// Control-space clip rectangle for current draw operation.
            /// </summary>
            private Rectangle ClipRectangle { get; set; }
            
            public Renderer(InternalPipelineContainer container)
            {
                _container = container;
            }

            public void Render([NotNull] Graphics g, Rectangle clipRectangle, [NotNull] Region clipRegion)
            {
                // Add some temporary decorators (for selection etc.)
                var decorators = _decorators.ToList();
                
                foreach (var o in _container.Selection)
                {
                    if (o is IPipelineNode node)
                    {
                        var view = _container.ViewForPipelineNode(node);
                        if (view == null)
                            continue;

                        decorators.Add(new StrokeDecorator(view, 3));
                    }
                    else if (o is IPipelineNodeLink link)
                    {
                        var view = _container.ViewForPipelineNodeLink(link);
                        if (view == null)
                            continue;

                        decorators.Add(new StrokeDecorator(view, 3));
                        // Add a decorator for the parent node view, as well
                        decorators.Add(new StrokeDecorator(view.NodeView, 3));
                    }
                }

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
                    // Transform by base view's transform
                    g.MultiplyTransform(_container.Root.LocalTransform);

                    // Draw background across visible region
                    RenderBackground(g);
                    
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
            }

            public void AddDecorator(IRenderingDecorator decorator)
            {
                _decorators.Add(decorator);
            }

            public void RemoveDecorator(IRenderingDecorator decorator)
            {
                _decorators.Remove(decorator);
            }

            private void RenderBackground([NotNull] Graphics g)
            {
                g.Clear(Color.Gray.Fade(Color.Black, 0.8f));

                var gridSize = new Vector(100, 100);

                var reg = _container.Root.ConvertFrom(new AABB(ClipRectangle), null);

                var startX = reg.Left;
                var endX = reg.Right;

                var startY = reg.Top;
                var endY = reg.Bottom;

                using (var gridPen = new Pen(Color.FromArgb(40, 40, 40)))
                {
                    for (var x = startX - reg.Left % gridSize.X; x <= endX; x += gridSize.X)
                    {
                        g.DrawLine(gridPen, x, reg.Top, x, reg.Bottom);
                    }

                    for (var y = startY - reg.Top % gridSize.Y; y <= endY; y += gridSize.Y)
                    {
                        g.DrawLine(gridPen, reg.Left, y, reg.Right, y);
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
                            StrokeColor = Color.Black,
                            StrokeWidth = 1,
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

                            var height = nodeView.GetTitleArea().Height;

                            var titleX = 4;

                            // Draw icon, if available
                            if (nodeView.Icon != null)
                            {
                                titleX += nodeView.Icon.Width + 5;

                                var imgY = height / 2 - (float) nodeView.Icon.Height / 2;

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
                            var titleY = height / 2 - size.Height / 2;

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

            /// <summary>
            /// A basic decorator for changing stroke of views
            /// </summary>
            private struct StrokeDecorator : IRenderingDecorator
            {
                private readonly object _target;
                private readonly int _strokeWidth;

                public StrokeDecorator(object target, int strokeWidth)
                {
                    _target = target;
                    _strokeWidth = strokeWidth;
                }

                public void DecoratePipelineStep(PipelineNodeView nodeView, Graphics g, ref PipelineStepViewState state)
                {
                    if (nodeView == _target)
                        state.StrokeWidth = _strokeWidth;
                }

                public void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                    ref PipelineStepViewLinkState state)
                {
                    if (link == _target)
                        state.StrokeWidth = _strokeWidth;
                }

                public void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                    ref PipelineStepViewLinkState state)
                {
                    if (link == _target)
                        state.StrokeWidth = _strokeWidth;
                }

                public void DecorateBezierPathView(BezierPathView pathView, Graphics g, ref BezierPathViewState state)
                {
                    if (pathView == _target)
                        state.StrokeWidth = _strokeWidth;
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
        private abstract class ExportPipelineFeature
        {
            /// <summary>
            /// Control to manage
            /// </summary>
            [NotNull]
            protected readonly ExportPipelineControl Control;

            protected InternalPipelineContainer container => Control._container;
            protected BaseView root => Control._container.Root;

            protected ExportPipelineFeature([NotNull] ExportPipelineControl control)
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
        }

        private class SelectionFeature : ExportPipelineFeature, IRenderingDecorator
        {
            [CanBeNull]
            private MouseHoverState? _hovering;

            private bool _isDrawingSelection;
            private Vector _mouseDown;

            // For drawing the selection outline with
            private readonly BezierPathView _pathView = new BezierPathView
            {
                RenderOnTop = true,
                FillColor = Color.Orange.ToAhsl().WithTransparency(0.03f).ToColor()
            };

            public SelectionFeature([NotNull] ExportPipelineControl control) : base(control)
            {
                control._renderer.AddDecorator(this);
            }

            public override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);

                _hovering = null;

                SetHovering(null);
            }

            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                if (e.Button != MouseButtons.Left)
                    return;

                if ((ModifierKeys & Keys.Shift) == 0)
                    Control._container.ClearSelection();

                var closestView = root.ViewUnder(e.Location, new Vector(5));

                // Selection
                if ((ModifierKeys & Keys.Shift) == Keys.Shift || closestView == null)
                {
                    _isDrawingSelection = true;
                    _mouseDown = e.Location;

                    _pathView.ClearPath();
                    root.AddChild(_pathView);
                }
                else
                {
                    // Selection
                    if (closestView is PipelineNodeView nodeView)
                    {
                        Control._container.SelectNode(nodeView.PipelineNode);
                    }
                    else if (closestView is PipelineNodeLinkView linkView)
                    {
                        Control._container.SelectLink(linkView.NodeLink);
                    }
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (e.Button == MouseButtons.Left && _isDrawingSelection)
                {
                    // Draw selection square
                    var area = new AABB();
                    area.ExpandToInclude(root.ConvertFrom(_mouseDown, null));
                    area.ExpandToInclude(root.ConvertFrom(e.Location, null));
                    
                    _pathView.SetAsRectangle(area);
                }
                else if (e.Button == MouseButtons.None)
                {
                    // Check hovering a link view
                    var closest = Control._container.Root.ViewUnder(new Vector(e.Location), new Vector(5));

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

                _isDrawingSelection = false;
                _pathView.RemoveFromParent();
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
                    state.StrokeWidth = 3;
            }

            public void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state)
            {
                if (_hovering?.View == link)
                    state.StrokeWidth = 3;
            }

            public void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state)
            {
                if (_hovering?.View == link)
                    state.StrokeWidth = 3;
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

        private class DragAndDropFeature : ExportPipelineFeature
        {
            /// <summary>
            /// List of on-going drag operations.
            /// </summary>
            private readonly List<DragOperation> _operations = new List<DragOperation>();
            
            private bool _isDragging;

            private Vector _mouseDownPoint;
            
            public DragAndDropFeature([NotNull] ExportPipelineControl control) : base(control)
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
                    if (_operations.Count > 0)
                    {
                        foreach (var operation in _operations)
                        {
                            operation.Finish(e.Location);
                            operation.Dispose();
                        }

                        _operations.Clear();
                    }

                    _isDragging = false;
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
                        StartDragging(e.Location);
                        _isDragging = true;
                    }

                    // Dragging
                    if(_isDragging)
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

                var nodes = container.SelectionModel.NodeViews();
                var links = container.SelectionModel.NodeLinkViews();

                foreach (var node in nodes.Cast<BaseView>().Concat(links))
                {
                    var localPoint = node.ConvertFrom(mousePosition, null);

                    var operation = new DragOperation(container, node, localPoint);

                    _operations.Add(operation);
                }
            }

            /// <summary>
            /// Cancels on-going drag operations
            /// </summary>
            private void CancelDragging()
            {
                foreach (var operation in _operations)
                {
                    operation.Cancel();
                }

                _operations.Clear();
            }

            /// <summary>
            /// Encapsulates a drag-and-drop operation so multiple can be made at the same time.
            /// </summary>
            private sealed class DragOperation : IDisposable
            {
                /// <summary>
                /// Container used to detect drop of link connections
                /// </summary>
                private readonly InternalPipelineContainer _container;

                private readonly Vector _startPosition;

                [CanBeNull]
                private readonly BezierPathView _linkDrawingPath;
                
                /// <summary>
                /// Target object for the drag
                /// </summary>
                private BaseView Target { get; }

                /// <summary>
                /// The relative mouse offset off of <see cref="Target"/> when dragging started
                /// </summary>
                private Vector Offset { get; }

                public DragOperation(InternalPipelineContainer container, BaseView target, Vector offset)
                {
                    Target = target;
                    Offset = offset;
                    _container = container;

                    _startPosition = Target.Location;

                    if (target is PipelineNodeLinkView)
                    {
                        _linkDrawingPath = new BezierPathView();
                        container.Root.AddChild(_linkDrawingPath);
                    }
                }
                
                [SuppressMessage("ReSharper", "UseNullPropagation")]
                public void Dispose()
                {
                    if (_linkDrawingPath != null)
                    {
                        _linkDrawingPath.RemoveFromParent();
                        _linkDrawingPath.Dispose();
                    }
                }

                private void UpdateLinkPreview(Vector mousePosition, [NotNull] PipelineNodeLinkView linkView)
                {
                    if (_linkDrawingPath != null)
                    {
                        _linkDrawingPath.ClearPath();

                        var toRight = linkView.NodeLink is IPipelineOutput;

                        var pt1 = linkView.ConvertTo(linkView.Bounds.Center, _container.Root);
                        var pt4 = _container.Root.ConvertFrom(mousePosition, null);
                        var pt2 = new Vector(toRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                        var pt3 = new Vector(pt1.X, pt4.Y);

                        _linkDrawingPath.AddBezierPoints(pt1, pt2, pt3, pt4);
                    }
                }

                /// <summary>
                /// Updates the on-going drag operation
                /// </summary>
                public void Update(Vector mousePosition)
                {
                    if (_linkDrawingPath != null && Target is PipelineNodeLinkView linkView)
                    {
                        UpdateLinkPreview(mousePosition, linkView);
                    }
                    else if (Target is PipelineNodeView view)
                    {
                        var absolutePoint = view.Parent?.ConvertFrom(mousePosition, null) ?? mousePosition;

                        var position = absolutePoint - Offset;
                        
                        if ((ModifierKeys & Keys.Control) != 0)
                            position = Vector.Round(position / 10) * 10;

                        view.Location = position;
                        
                        _container.InvalidateConnectionViewsFor(view);
                    }
                }

                /// <summary>
                /// Notifies the mouse was released on a given position
                /// </summary>
                public void Finish(Vector mousePosition)
                {
                    // Verify if we travelled enough to consider a dragging
                    _linkDrawingPath?.ClearPath();

                    // Create link, if we're dragging one
                    if (Target is PipelineNodeLinkView linkView)
                    {
                        var end = _container.Root.ViewUnder(mousePosition, new Vector(5, 5), view => view != linkView);

                        if (end is PipelineNodeLinkView endLinkView)
                            _container.AddConnectionView(linkView, endLinkView);
                    }
                }

                /// <summary>
                /// Cancels the operation and undoes any changes
                /// </summary>
                public void Cancel()
                {
                    if (Target is PipelineNodeView)
                        Target.Location = _startPosition;

                    _linkDrawingPath?.RemoveFromParent();
                }
            }
        }

        private class ViewPanAndZoomFeature : ExportPipelineFeature
        {
            private Vector _dragStart;
            private Point _mouseDownPoint;
            private bool _dragging;

            public ViewPanAndZoomFeature([NotNull] ExportPipelineControl control)
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

            public override void OnMouseDoubleClick(MouseEventArgs e)
            {
                base.OnMouseDoubleClick(e);

                if (e.Button == MouseButtons.Left)
                {
                    // Zoom into a specific view
                    var view = root.ViewUnder(e.Location, new Vector(5, 5), v => v is PipelineNodeView);

                    if (view != null)
                    {
                        root.Location = -view.Center + (Vector)Control.Size / 2 / root.Scale;
                        Control.Invalidate();
                    }
                }
            }

            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                _mouseDownPoint = e.Location;
                //var point = (Vector) e.Location;

                if (e.Button == MouseButtons.Middle)
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

                _dragging = false;
            }

            public override void OnMouseWheel(MouseEventArgs e)
            {
                base.OnMouseWheel(e);

                var scale = root.Scale;
                scale *= new Vector(1.0f + Math.Sign(e.Delta) * 0.1f);
                if (scale < new Vector(0.5f))
                    scale = new Vector(0.5f);
                if (scale > new Vector(25f))
                    scale = new Vector(25f);

                SetZoom(scale, e.Location);
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

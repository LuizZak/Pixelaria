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

            exportPipelineControl.PipelineContainer.AddStepView(animNodeView);
            exportPipelineControl.PipelineContainer.AddStepView(animJoinerNodeView);
            exportPipelineControl.PipelineContainer.AddStepView(sheetNodeView);
            exportPipelineControl.PipelineContainer.AddStepView(fileExportView);

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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _container.Modified -= ContainerOnModified;
            }

            base.Dispose(disposing);
        }

        private void ContainerOnModified(object sender, EventArgs eventArgs)
        {
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            _renderer.Render(e.Graphics, e.ClipRectangle);
            foreach (var feature in _features)
                feature.OnPaint(e);
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
            void AddStepView(PipelineNodeView nodeView);

            /// <summary>
            /// Retrieves the view that represent the given pipeline step within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeView ViewForPipelineNode([NotNull] IPipelineNode step);

            /// <summary>
            /// Retrieves the view that represent the given pipeline step within this container
            /// </summary>
            [CanBeNull]
            PipelineNodeLinkView ViewForPipelineNodeLink([NotNull] IPipelineNodeLink link);

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
            /// <summary>
            /// Control where this pipeline container is contained in
            /// </summary>
            public readonly Control Control;

            public BaseView Root { get; } = new BaseView();

            public readonly List<PipelineNodeView> StepViews = new List<PipelineNodeView>();
            private readonly List<PipelineNodeConnectionLineView> _connectionViews = new List<PipelineNodeConnectionLineView>();

            public event EventHandler Modified;

            public InternalPipelineContainer(Control control)
            {
                Control = control;
                Root.DirtyRegionUpdated += Root_DirtyRegionUpdated;
            }

            private void Root_DirtyRegionUpdated(object sender, EventArgs e)
            {
                var reg = Root.DirtyRegion.Clone();
                // Convert to screen space first
                reg.Transform(Root.LocalTransform);

                Control.Invalidate(reg);
            }

            public void AddStepView([NotNull] PipelineNodeView nodeView)
            {
                Root.AddChild(nodeView);
                StepViews.Add(nodeView);

                Modified?.Invoke(this, EventArgs.Empty);
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
            public PipelineNodeView ViewForPipelineNode(IPipelineNode step)
            {
                return StepViews.FirstOrDefault(stepView => stepView.PipelineStep == step);
            }

            /// <summary>
            /// Retrieves the view that represent the given pipeline step within this container
            /// </summary>
            public PipelineNodeLinkView ViewForPipelineNodeLink(IPipelineNodeLink link)
            {
                if (link.Node == null)
                    return null;

                return ViewForPipelineNode(link.Node)?.GetLinkViews()
                    .FirstOrDefault(linkView => linkView.NodeLink == link);
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

                Control.Invalidate(Rectangle.Round((RectangleF)screenBounds));
            }

            public void AutosizeNodes()
            {
                using (var graphics = Control.CreateGraphics())
                {
                    foreach (var view in StepViews)
                    {
                        view.AutoSize(graphics);
                    }
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

            public void Render([NotNull] Graphics g, Rectangle clipRectangle)
            {
                ClipRectangle = clipRectangle;
                
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                
                g.WithTemporaryState(() =>
                {
                    // Transform by base view's transform
                    g.MultiplyTransform(_container.Root.LocalTransform);

                    // Draw background across visible region
                    RenderBackground(g);
                    
                    // Render bezier paths
                    var beziers = _container.Root.Children.OfType<BezierPathView>();
                    foreach (var bezier in beziers)
                    {
                        RenderBezierView(bezier, g);
                    }

                    foreach (var stepView in _container.StepViews)
                    {
                        RenderStepView(stepView, g);
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

                var reg = _container.Root.ConvertFrom(_container.Control.Bounds, null);

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

            private void RenderStepView([NotNull] PipelineNodeView nodeView, [NotNull] Graphics g)
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
                        foreach (var decorator in _decorators)
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
                                foreach (var decorator in _decorators)
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
                                foreach (var decorator in _decorators)
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

            private void RenderBezierView([NotNull] BezierPathView bezierView, [NotNull] Graphics g)
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
                            StrokeWidth = bezierView.StrokeWidth
                        };

                        // Decorate
                        foreach (var decorator in _decorators)
                            decorator.DecorateBezierPathView(bezierView, g, ref state);

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
            
            protected BaseView root => Control._container.Root;

            protected ExportPipelineFeature([NotNull] ExportPipelineControl control)
            {
                Control = control;
            }

            public virtual void OnPaint([NotNull] PaintEventArgs e) { }
            public virtual void OnMouseLeave([NotNull] EventArgs e) { }
            public virtual void OnMouseClick([NotNull] MouseEventArgs e) { }
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

        private class DragAndDropFeature : ExportPipelineFeature, IRenderingDecorator
        {
            [CanBeNull]
            private MouseHoverState? _hovering;
            [CanBeNull]
            private MouseHoverState? _dragging;

            private bool _alignToGrid;

            /// <summary>
            /// For when drawing links
            /// </summary>
            private readonly BezierPathView _bezierPath = new BezierPathView();
            
            private Vector _dragOffset;

            public DragAndDropFeature([NotNull] ExportPipelineControl control) : base(control)
            {
                control._renderer.AddDecorator(this);

                root.AddChild(_bezierPath);
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
                
                if (e.Button == MouseButtons.Left)
                {
                    var closestView = root.ViewUnder((Vector)e.Location, new Vector(5));
                    
                    if (closestView is PipelineNodeLinkView link)
                    {
                        SetDragging(link, (Vector) e.Location);
                    }
                    else if (closestView is PipelineNodeView step)
                    {
                        // Start drag operation
                        SetDragging(step, (Vector) e.Location);
                    }
                }
            }

            public override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (e.Button == MouseButtons.Left && _dragging != null)
                {
                    _bezierPath.ClearPath();

                    // Create link
                    if (_dragging.Value.View is PipelineNodeLinkView linkView)
                    {
                        var end = root.ViewUnder((Vector) e.Location, new Vector(5, 5), view => view != linkView);

                        if(end is PipelineNodeLinkView endLinkView)
                            Control._container.AddConnectionView(linkView, endLinkView);
                    }

                    SetDragging(null, Vector.Zero);
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (_dragging != null)
                {
                    // Dragging
                    if (_dragging.Value.View is PipelineNodeLinkView linkView)
                    {
                        _bezierPath.ClearPath();

                        var invalidateRect = linkView.Bounds;
                        invalidateRect = AABB.Union(invalidateRect,
                            new AABB(new Vector(e.Location), new Vector(e.Location)).Inflated(2, 2));

                        Control.Invalidate(Rectangle.Round((RectangleF)invalidateRect.Inflated(50, 50)));
                        
                        var toRight = linkView.NodeLink is IPipelineOutput;
                        
                        var pt1 = linkView.ConvertTo(linkView.Bounds.Center, Control._container.Root);
                        var pt4 = new Vector(Control.MousePoint) * Control._container.Root.LocalTransform.Inverted();
                        var pt2 = new Vector(toRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
                        var pt3 = new Vector(pt1.X, pt4.Y);
                        
                        _bezierPath.AddBezierPoints(pt1, pt2, pt3, pt4);
                    }
                    else
                    {
                        var view = _dragging?.View;
                        
                        var absolutePoint = view.Parent?.ConvertFrom((Vector)e.Location, null) ?? (Vector)e.Location;

                        var position = absolutePoint - _dragOffset;

                        if (_alignToGrid)
                            position = Vector.Round(position / 10) * 10;

                        view.Location = position;

                        if (view is PipelineNodeView nodeView)
                            Control._container.InvalidateConnectionViewsFor(nodeView);
                    }
                }
                else if(e.Button == MouseButtons.None)
                {
                    // Check hovering a link view
                    var closest = Control._container.Root.ViewUnder(new Vector(e.Location), new Vector(5));

                    if (closest != null)
                    {
                        if(closest is PipelineNodeView stepView)
                            SetHovering(stepView);
                        else if(closest is PipelineNodeLinkView linkView)
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

            public override void OnKeyDown(KeyEventArgs e)
            {
                base.OnKeyDown(e);

                if (e.KeyCode == Keys.ControlKey)
                    _alignToGrid = true;
            }

            public override void OnKeyUp(KeyEventArgs e)
            {
                base.OnKeyUp(e);

                if (e.KeyCode == Keys.ControlKey)
                    _alignToGrid = false;
            }

            private void SetDragging([CanBeNull] BaseView view, Vector mouseLoc)
            {
                if (view == null)
                {
                    _dragging = null;
                }
                else
                {
                    var localPoint = view.ConvertFrom(mouseLoc, null);

                    _dragging = new MouseHoverState {View = view };
                    _dragOffset = localPoint;
                }
            }

            private void SetHovering([CanBeNull] BaseView view)
            {
                if (_hovering?.View == view)
                    return;
                
                // Full invalidation contexts
                if (_hovering == null && view != null)
                    Control._container.InvalidateViewRegion(view);
                else if (_hovering != null && view == null)
                    Control._container.InvalidateViewRegion(_hovering.Value.View);
                else if(_hovering != null && view != null && _hovering.Value.View != view)
                {
                    Control._container.InvalidateViewRegion(_hovering.Value.View);
                    Control._container.InvalidateViewRegion(view);
                }
                else if(_hovering?.View == view && view != null) // Partial invalidation contexts: In/out links
                {
                    Control._container.InvalidateViewRegion(view);
                }

                if (view == null)
                    _hovering = null;
                else
                    _hovering = new MouseHoverState {View = view};
            }

            #region IRenderingDecorator

            public void DecoratePipelineStep(PipelineNodeView nodeView, Graphics g, ref Renderer.PipelineStepViewState state)
            {
                if (_hovering?.View == nodeView)
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
                    
                    // Zoom out around the center of the canvas
                    var mid = root.Location + ((AABB)Control.Bounds).Size / 2;
                    
                    SetZoom(Vector.Unit, mid, true);
                }
            }

            public override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);

                _mouseDownPoint = e.Location;
                var point = (Vector) e.Location;

                if (e.Button == MouseButtons.Middle || root.ViewUnder(point, new Vector(10, 10)) == null)
                {
                    _dragStart = root.Location - (Vector)e.Location;
                    _dragging = true;
                }
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (_dragging)
                {
                    var point = (Vector)e.Location;
                    
                    root.Location = point + _dragStart;
                    
                    Control.Invalidate();
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
                scale *= new Vector(1.0f + Math.Sign(e.Delta) * 0.05f);
                if (scale < new Vector(0.5f))
                    scale = new Vector(0.5f);
                if (scale > new Vector(5f))
                    scale = new Vector(5f);

                SetZoom(scale, (Vector)e.Location);
            }

            private void SetZoom(Vector newZoom, Vector focusPosition, bool repositioning = true)
            {
                // Get mouse loc in view
                var oldScale = root.Scale;
                
                root.Scale = newZoom;

                if (repositioning)
                {
                    var pivot = focusPosition;

                    // Correct positioning
                    var currentOffset = root.Location;

                    currentOffset -= pivot;
                    currentOffset = currentOffset * (root.Scale / oldScale);
                    currentOffset += pivot;

                    root.Location = currentOffset;
                }

                Control.Invalidate();
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

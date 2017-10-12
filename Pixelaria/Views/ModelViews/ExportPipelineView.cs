using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.ExportPipeline;
using Pixelaria.Utils;

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
            var sheet = new AnimationSheet("Sheet 1");

            var animStepView = new PipelineStepView(new AnimationPipelineStep(anim))
            {
                Location = new Point(250, 30),
                Size = new Size(100, 80)
            };
            var sheetStepView = new PipelineStepView(new SpriteSheetPipelineStep(sheet))
            {
                Location = new Point(300, 30),
                Size = new Size(100, 80)
            };

            exportPipelineRenderer1.PipelineContainer.AddStepView(animStepView);
            exportPipelineRenderer1.PipelineContainer.AddStepView(sheetStepView);
        }
    }
    
    public class ExportPipelineControl: Control
    {
        private readonly InternalPipelineContainer _container = new InternalPipelineContainer();
        private readonly Renderer _renderer;
        private readonly List<ExportPipelineFeature> _features = new List<ExportPipelineFeature>();

        public Point MousePoint { get; private set; }
        
        public IPipelineContainer PipelineContainer => _container;
        
        public ExportPipelineControl()
        {
            _renderer = new Renderer(_container);
            _container.Offset = Point.Empty;

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            _container.Modified += ContainerOnModified;

            _features.Add(new DragAndDropFeature(this));
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
            
            _renderer.Render(e.Graphics);

            foreach (var feature in _features)
                feature.OnPaint(e);
        }
        
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            foreach (var feature in _features)
                feature.OnMouseLeave(e);
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
        /// Renders a pipeline export view
        /// </summary>
        private class Renderer
        {
            /// <summary>
            /// For relative position calculations
            /// </summary>
            private readonly InternalPipelineContainer _container;

            private readonly List<IRenderingDecorator> _decorators = new List<IRenderingDecorator>();

            public Renderer(InternalPipelineContainer container)
            {
                _container = container;
            }

            public void Render([NotNull] Graphics g)
            {
                g.CompositingMode = CompositingMode.SourceOver;
                g.InterpolationMode = InterpolationMode.High;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.SystemDefault;

                g.Clear(Color.FromKnownColor(KnownColor.Control));

                // Offset by container's inner offset
                g.TranslateTransform(_container.Offset.X, _container.Offset.Y);

                foreach (var stepView in _container.StepViews)
                {
                    RenderStepView(stepView, g);
                }
            }

            public void AddDecorator(IRenderingDecorator decorator)
            {
                _decorators.Add(decorator);
            }

            public void RemoveDecorator(IRenderingDecorator decorator)
            {
                _decorators.Remove(decorator);
            }

            private void RenderStepView([NotNull] PipelineStepView stepView, [NotNull] Graphics g)
            {
                using (var path = new GraphicsPath())
                {
                    var bounds = stepView.Area;

                    // Create rendering states for decorators
                    var state = new PipelineStepViewState
                    {
                        FillColor = stepView.Color,
                        TitleFillColor = stepView.Color.Fade(Color.Black),
                        StrokeColor = Color.Black,
                        StrokeWidth = 1,
                        FontColor = Color.White
                    };

                    // Decorate
                    foreach (var decorator in _decorators)
                        decorator.DecoratePipelineStep(stepView, g, ref state);

                    path.AddRoundedRectangle(bounds, 5);

                    // Draw body outline
                    using (var brush = new SolidBrush(state.FillColor))
                    {
                        g.FillPath(brush, path);
                    }

                    // Draw title area
                    g.WithTemporaryState(() =>
                    {
                        g.Clip = new Region(path);

                        g.TranslateTransform(stepView.Location.X, stepView.Location.Y);

                        using (var brush = new SolidBrush(state.TitleFillColor))
                        {
                            g.FillRectangle(brush, stepView.GetTitleArea());
                        }

                        using (var brush = new SolidBrush(state.FontColor))
                            g.DrawString(stepView.Name, DefaultFont, brush, new PointF(2, 4));

                        g.Flush(FlushIntention.Sync);
                    });

                    using (var pen = new Pen(state.StrokeColor, state.StrokeWidth))
                    {
                        // Decorate
                        g.DrawPath(pen, path);
                    }

                    // Draw in-going and out-going links
                    var inLinks = stepView.GetInputViews();
                    var outLinks = stepView.GetOutputViews();

                    // Draw inputs
                    foreach (var link in inLinks)
                    {
                        var rectangle = link.Area;

                        var linkState = new PipelineStepViewLinkState
                        {
                            FillColor = Color.White,
                            StrokeColor = Color.Black,
                            StrokeWidth = 1
                        };

                        // Decorate
                        foreach (var decorator in _decorators)
                            decorator.DecoratePipelineStepInput(stepView, link, g, ref linkState);

                        using (var pen = new Pen(linkState.StrokeColor, linkState.StrokeWidth))
                        using (var brush = new SolidBrush(linkState.FillColor))
                        {
                            g.FillRectangle(brush, rectangle);
                            g.DrawRectangle(pen, rectangle);
                        }
                    }

                    // Draw outputs
                    foreach (var link in outLinks)
                    {
                        var rectangle = link.Area;

                        var linkState = new PipelineStepViewLinkState
                        {
                            FillColor = Color.White,
                            StrokeColor = Color.Black,
                            StrokeWidth = 1
                        };

                        // Decorate
                        foreach (var decorator in _decorators)
                            decorator.DecoratePipelineStepOutput(stepView, link, g, ref linkState);

                        using (var pen = new Pen(linkState.StrokeColor, linkState.StrokeWidth))
                        using (var brush = new SolidBrush(linkState.FillColor))
                        {
                            g.FillRectangle(brush, rectangle);
                            g.DrawRectangle(pen, rectangle);
                        }
                    }
                }
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
        }

        public interface IPipelineContainer
        {
            void AddStepView(PipelineStepView stepView);
        }

        /// <summary>
        /// Container for pipeline views.
        /// 
        /// Also aids in position calculations for rendering
        /// </summary>
        private class InternalPipelineContainer : IPipelineContainer
        {
            /// <summary>
            /// Viewport offset
            /// </summary>
            public Point Offset { get; set; }

            private readonly BaseView _root = new BaseView();

            public readonly List<PipelineStepView> StepViews = new List<PipelineStepView>();

            public event EventHandler Modified;

            public void AddStepView([NotNull] PipelineStepView stepView)
            {
                _root.AddChild(stepView);
                StepViews.Add(stepView);

                Modified?.Invoke(this, EventArgs.Empty);
            }

            public Rectangle GetScreenBounds([NotNull] PipelineStepView stepView)
            {
                var rect = stepView.GetTotalBounds();

                rect.Offset(stepView.Location);

                return rect;
            }
            
            [CanBeNull]
            public PipelineStepView StepViewUnder(Point point)
            {
                return StepViews.AsQueryable().Reverse().FirstOrDefault(stepView => stepView.Area.Contains(point));
            }
            
            [CanBeNull]
            public PipelineStepLinkView ClosestLinkView(Point point, Size inflating)
            {
                PipelineStepLinkView closestV = null;
                float closestD = float.PositiveInfinity;

                foreach (var stepView in StepViews.AsQueryable().Reverse())
                {
                    foreach (var linkView in stepView.GetInputViews())
                    {
                        var dist = point.Distance(linkView.Area.Center());
                        if (dist < closestD)
                        {
                            closestV = linkView;
                            closestD = dist;
                        }
                    }

                    foreach (var linkView in stepView.GetOutputViews())
                    {
                        var dist = point.Distance(linkView.Area.Center());
                        if (dist < closestD)
                        {
                            closestV = linkView;
                            closestD = dist;
                        }
                    }
                }

                if (closestV != null && closestV.Area.Inflated(inflating).Contains(point))
                    return closestV;

                return null;
            }

            [CanBeNull]
            public BaseView ClosestBaseView(Point point, Size inflating)
            {
                return _root.HitTestClosest(point, inflating);
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
            protected readonly ExportPipelineControl Control;

            protected ExportPipelineFeature([NotNull] ExportPipelineControl control)
            {
                Control = control;
            }

            public virtual void OnPaint([NotNull] PaintEventArgs e) { }
            public virtual void OnMouseMove([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseDown([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseUp([NotNull] MouseEventArgs e) { }
            public virtual void OnMouseLeave([NotNull] EventArgs e) { }
            public virtual void OnMouseEnter([NotNull] EventArgs e) { }

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
            
            private Point _dragOffset;

            public DragAndDropFeature([NotNull] ExportPipelineControl control) : base(control)
            {
                control._renderer.AddDecorator(this);
            }

            public override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);

                if (_dragging != null && _dragging.Value.Link != null)
                {
                    using (var pen = new Pen(Color.Black, 3))
                    {
                        var path = new GraphicsPath();
                        var p1 = _dragging.Value.Link.Area.Center();
                        var p4 = Control.MousePoint;
                        var p2 = new Point(p4.X, p1.Y);
                        var p3 = new Point(p1.X, p4.Y);

                        path.AddBezier(p1, p2, p3, p4);

                        e.Graphics.DrawPath(pen, path);
                    }
                }
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
                    var link = Control._container.ClosestLinkView(e.Location, new Size(5, 5));
                    if (link != null)
                    {
                        SetDragging(link.StepView, link, e.Location);
                    }
                    else
                    {
                        var step = Control._container.StepViewUnder(e.Location);
                        if (step != null)
                        {
                            // Start drag operation
                            SetDragging(step, null, e.Location);
                        }
                    }
                }
            }

            public override void OnMouseUp(MouseEventArgs e)
            {
                base.OnMouseUp(e);

                if (e.Button == MouseButtons.Left && _dragging != null)
                    SetDragging(null, null, Point.Empty);
            }

            public override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (_dragging != null)
                {
                    // Dragging
                    if (_dragging.Value.Link != null)
                    {
                        var invalidateRect = _dragging.Value.Link.Area;
                        invalidateRect = Rectangle.Union(invalidateRect,
                            new Rectangle(e.Location, Size.Empty).Inflated(2, 2));

                        Control.Invalidate(invalidateRect.Inflated(50, 50));
                    }
                    else
                    {
                        var view = _dragging?.View;

                        var beforeArea = Control._container.GetScreenBounds(view).Inflated(5, 5);

                        view.Location = Point.Subtract(e.Location, new Size(_dragOffset));

                        var afterArea = Control._container.GetScreenBounds(view).Inflated(5, 5);

                        var reg = new Region(beforeArea);
                        reg.Union(afterArea);
                        Control.Invalidate(reg);
                    }
                }
                else if(e.Button == MouseButtons.None)
                {
                    // Check hovering a link view
                    var closest = Control._container.ClosestBaseView(e.Location, new Size(5, 5));

                    if (closest != null)
                    {
                        if(closest is PipelineStepView stepView)
                            SetHovering(stepView);
                        else if(closest is PipelineStepLinkView linkView)
                            SetHovering(linkView.StepView, linkView);
                    }
                    else
                    {
                        SetHovering(null);
                    }
                }
            }

            private void SetDragging([CanBeNull] PipelineStepView stepView, [CanBeNull] PipelineStepLinkView link, Point mouseLoc)
            {
                if (stepView == null)
                {
                    _dragging = null;
                }
                else
                {
                    _dragging = new MouseHoverState {View = stepView, Link = link};
                    _dragOffset = Point.Subtract(mouseLoc, new Size(stepView.Location));
                }
            }

            private void SetHovering([CanBeNull] PipelineStepView stepView, PipelineStepLinkView link = null)
            {
                if (_hovering?.View == stepView && _hovering?.Link == link)
                    return;
                
                // Full invalidation contexts
                if (_hovering == null && stepView != null)
                    Control.Invalidate(Control._container.GetScreenBounds(stepView).Inflated(2, 2));
                else if (_hovering != null && stepView == null)
                    Control.Invalidate(Control._container.GetScreenBounds(_hovering.Value.View).Inflated(2, 2));
                else if(_hovering != null && stepView != null && _hovering.Value.View != stepView)
                {
                    Control.Invalidate(Control._container.GetScreenBounds(_hovering.Value.View).Inflated(2, 2));
                    Control.Invalidate(Control._container.GetScreenBounds(stepView).Inflated(2, 2));
                }
                else if(_hovering?.View == stepView && stepView != null) // Partial invalidation contexts: In/out links
                {
                    Control.Invalidate(Control._container.GetScreenBounds(stepView).Inflated(2, 2));
                }

                if (stepView == null)
                    _hovering = null;
                else
                    _hovering = new MouseHoverState {View = stepView, Link = link};
            }

            #region IRenderingDecorator

            public void DecoratePipelineStep(PipelineStepView stepView, Graphics g, ref Renderer.PipelineStepViewState state)
            {
                if (_hovering?.View == stepView)
                    state.StrokeWidth = 3;
            }

            public void DecoratePipelineStepInput(PipelineStepView stepView, PipelineStepLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state)
            {
                if (_hovering?.View == stepView && _hovering?.Link == link)
                    state.StrokeWidth = 3;
            }

            public void DecoratePipelineStepOutput(PipelineStepView stepView, PipelineStepLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state)
            {
                if (_hovering?.View == stepView && _hovering?.Link == link)
                    state.StrokeWidth = 3;
            }

            #endregion

            private struct MouseHoverState
            {
                [NotNull]
                public PipelineStepView View { get; set; }

                [CanBeNull]
                public PipelineStepLinkView Link { get; set; }
            }
        }

        /// <summary>
        /// Decorator that modifies rendering of objects in the export pipeline view.
        /// </summary>
        private interface IRenderingDecorator
        {
            void DecoratePipelineStep([NotNull] PipelineStepView stepView, Graphics g,
                ref Renderer.PipelineStepViewState state);

            void DecoratePipelineStepInput([NotNull] PipelineStepView stepView, PipelineStepLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state);

            void DecoratePipelineStepOutput([NotNull] PipelineStepView stepView, PipelineStepLinkView link, Graphics g,
                ref Renderer.PipelineStepViewLinkState state);
        }
    }

    public class BaseView
    {
        private Size _size;

        /// <summary>
        /// Gets the parent view, if any, of this base view.
        /// 
        /// Area is relative to this base view's Location, if present.
        /// </summary>
        [CanBeNull]
        public BaseView Parent { get; private set; }

        /// <summary>
        /// Children of this base view
        /// </summary>
        protected List<BaseView> children { get; } = new List<BaseView>();

        /// <summary>
        /// Gets the absolute location/size of this view
        /// </summary>
        public virtual Rectangle Area
        {
            get
            {
                if (Parent != null)
                    return new Rectangle(Point.Add(Parent.Area.Location, new Size(Location)), Size);

                return new Rectangle(Location, Size);
            }
        }

        /// <summary>
        /// Top-left location of view, in pixels
        /// </summary>
        public virtual Point Location { get; set; }

        /// <summary>
        /// Size of view, in pixels
        /// </summary>
        public virtual Size Size
        {
            get => _size;
            set
            {
                if (_size == value)
                    return;

                _size = value;
                OnResize();
            }
        }

        /// <summary>
        /// Relative scale of this base view
        /// </summary>
        public Size Scale { get; set; } = new Size(1, 1);
        
        /// <summary>
        /// Returns the relative area of this view, with 0 x 0 mapping
        /// to its top-left pixel
        /// </summary>
        public virtual Rectangle RelativeArea => new Rectangle(Point.Empty, Size);
        
        /// <summary>
        /// Adds a base view as the child of this base view.
        /// </summary>
        public void AddChild([NotNull] BaseView child)
        {
            // Check recursiveness
            var cur = child;
            while (cur != null)
            {
                if(cur == this)
                    throw new ArgumentException(@"Cannot add BaseView as child of itself", nameof(child));

                cur = child.Parent;
            }

            child.Parent = this;
            children.Add(child);
        }

        /// <summary>
        /// Removes a given child from this base view
        /// </summary>
        public void RemoveChild([NotNull] BaseView child)
        {
            if(child.Parent != this)
                throw new ArgumentException(@"Child BaseView passed in is not a direct child of this base view", nameof(child));

            child.Parent = null;
            children.Remove(child);
        }

        /// <summary>
        /// Performs a hit test operation on the area of this, and all child
        /// base views, for the given absolute coordinates point.
        /// 
        /// Returns the base view that has its origin closest to the given point.
        /// 
        /// Only returns an instance if its area is contained within the given point.
        /// 
        /// The <see cref="inflatingArea"/> argument can be used to inflate the
        /// area of the views to perform less precise hit tests.
        /// </summary>
        [CanBeNull]
        public BaseView HitTestClosest(Point point, Size inflatingArea)
        {
            BaseView closestV = null;
            float closestD = float.PositiveInfinity;

            // Search children first
            foreach (var baseView in children.AsQueryable().Reverse())
            {
                var ht = baseView.HitTestClosest(point, inflatingArea);
                if (ht != null)
                {
                    if (ht.Area.Center().Distance(point) < closestD)
                    {
                        closestV = ht;
                    }

                    return ht;
                }
            }

            // Test this instance now
            if (Area.Inflated(inflatingArea).Contains(point) &&
                Area.Center().Distance(point) < closestD)
                closestV = this;

            return closestV;
        }

        protected virtual void OnResize()
        {
            
        }
    }

    public class PipelineStepView : BaseView
    {
        private readonly IPipelineStep _pipelineStep;
        private readonly List<PipelineStepLinkView> _inputs = new List<PipelineStepLinkView>();
        private readonly List<PipelineStepLinkView> _outputs = new List<PipelineStepLinkView>();

        public string Name => _pipelineStep.Name;
        
        /// <summary>
        /// Gets or sets the display color for this step view.
        /// 
        /// Initialized to a default color depending on which IPipelineStep class
        /// was provided during instantiation, via <see cref="DefaultColorForPipelineStep"/>
        /// </summary>
        public Color Color { get; set; }
        
        public PipelineStepView(IPipelineStep pipelineStep)
        {
            _pipelineStep = pipelineStep;
            Color = DefaultColorForPipelineStep(pipelineStep);

            ReloadLinkViews();
        }

        private void ReloadLinkViews()
        {
            _inputs.Clear();
            _outputs.Clear();

            foreach (var view in children.ToArray()) // Copy to avoid iterating over modifying collection
            {
                RemoveChild(view);
            }

            // Create inputs
            
            for (var i = 0; i < _pipelineStep.Input.Count; i++)
            {
                var input = new PipelineStepLinkView(_pipelineStep.Input.ElementAt(i));

                _inputs.Add(input);
                AddChild(input);
            }

            for (var i = 0; i < _pipelineStep.Output.Count; i++)
            {
                var output = new PipelineStepLinkView(_pipelineStep.Output.ElementAt(i));

                _outputs.Add(output);
                AddChild(output);
            }

            PositionLinkViews();
        }

        private void PositionLinkViews()
        {
            const int linkSize = 10;

            var contentArea = GetContentArea();

            var topLeft = new Point(contentArea.Left, contentArea.Top);
            var botLeft = new Point(contentArea.Left, contentArea.Bottom);
            var topRight = new Point(contentArea.Right, contentArea.Top);
            var botRight = new Point(contentArea.Right, contentArea.Bottom);

            var ins = AlignedBoxesAcrossEdge(_pipelineStep.Input.Count, new Size(linkSize, linkSize), topLeft, botLeft, (int)(linkSize * 1.5));
            var outs = AlignedBoxesAcrossEdge(_pipelineStep.Output.Count, new Size(linkSize, linkSize), topRight, botRight, (int)(linkSize * 1.5));

            for (var i = 0; i < _pipelineStep.Input.Count; i++)
            {
                var rect = ins[i];
                _inputs[i].Location = rect.Location;
                _inputs[i].Size = rect.Size;
            }

            for (var i = 0; i < _pipelineStep.Output.Count; i++)
            {
                var rect = outs[i];
                _outputs[i].Location = rect.Location;
                _outputs[i].Size = rect.Size;
            }
        }

        protected override void OnResize()
        {
            base.OnResize();

            PositionLinkViews();
        }

        public PipelineStepLinkView[] GetInputViews()
        {
            return _inputs.ToArray();
        }

        public PipelineStepLinkView[] GetOutputViews()
        {
            return _outputs.ToArray();
        }

        /// <summary>
        /// Gets the total bounds that this step view would take (relatively) on the
        /// screen, including outlink positions on the left and on the right.
        /// 
        /// Can be used for screen invalidation purposes.
        /// </summary>
        public Rectangle GetTotalBounds()
        {
            var min = GetContentArea();
            min = Rectangle.Union(min, GetTitleArea());
            min = children.Select(c => c.RelativeArea.Inflated(2, 2).OffsetBy(c.Location)).Aggregate(min, Rectangle.Union);

            return min;
        }

        /// <summary>
        /// Returns the rectangle that represents the title area for this step view.
        /// 
        /// Expressed relative to <see cref="BaseView.RelativeArea"/>
        /// </summary>
        public Rectangle GetTitleArea()
        {
            var rect = RelativeArea;
            rect.Height = 20;

            return rect;
        }

        /// <summary>
        /// Returns the relative content area for rendering of a step view, starting
        /// from the bottom of its title area.
        /// 
        /// Expressed relative to <see cref="BaseView.RelativeArea"/>
        /// </summary>
        public Rectangle GetContentArea()
        {
            var titleArea = GetTitleArea();

            var rect = RelativeArea;
            rect.Y += titleArea.Height;
            rect.Height -= titleArea.Height;

            return rect;
        }

        /// <summary>
        /// For each inputlink in this step view, returns relative rectangles that match 
        /// each ingoing link on the left side of the step view's borders, along the
        /// content area region.
        /// </summary>
        public Rectangle[] GetInlinkRectangles()
        {
            var contentArea = GetContentArea();

            var topEdge = new Point(contentArea.Left, contentArea.Top);
            var botEdge = new Point(contentArea.Left, contentArea.Bottom);

            const int linkSize = 10;
            return AlignedBoxesAcrossEdge(_pipelineStep.Input.Count, new Size(linkSize, linkSize), topEdge, botEdge, (int)(linkSize * 1.5));
        }

        /// <summary>
        /// For each outlink in this step view, returns relative rectangles that match 
        /// each outgoing link on the right side of the step view's borders, along the
        /// content area region.
        /// </summary>
        public Rectangle[] GetOutlinkRectangles()
        {
            // _____   <- control.minY
            //      \
            //      |
            // -----|  <- control.minY + control.title.height
            //      |  A
            //      |  |  
            //      |  | <- control.contentArea
            //      |  |
            //      |  V
            // _____/  <- control.maxY
            //

            var contentArea = GetContentArea();

            var topEdge = new Point(contentArea.Right, contentArea.Top);
            var botEdge = new Point(contentArea.Right, contentArea.Bottom);

            const int linkSize = 10;
            return AlignedBoxesAcrossEdge(_pipelineStep.Output.Count, new Size(linkSize, linkSize), topEdge, botEdge, (int) (linkSize * 1.5));
        }

        private static Rectangle[] AlignedBoxesAcrossEdge(int count, Size size, Point edgeStart, Point edgeEnd, int separation)
        {
            if (count <= 0)
                return new Rectangle[0];

            var output = new Rectangle[count];

            var mid = Point.Add(edgeStart, new Size(edgeEnd));
            var norm = PointF.Subtract(edgeEnd, new Size(edgeStart)).Normalized();

            mid = new Point(mid.X / 2, mid.Y / 2);
            
            var total = separation * (count - 1);
            var offset = Point.Subtract(mid, new Size(norm.Multiplied(total / 2.0f).Rounded()));

            for (int i = 0; i < count; i++)
            {
                var point = Point.Add(offset, new Size(norm.Multiplied(separation * i).Rounded()));

                // Re-center rect
                var rect = new Rectangle(point, size);
                rect.Offset(-rect.Width / 2, -rect.Height / 2);

                output[i] = rect;
            }

            return output;
        }

        /// <summary>
        /// Gets the default color for the given implementation instance of IPipelineStep.
        /// </summary>
        public static Color DefaultColorForPipelineStep(IPipelineStep step)
        {
            if (step is SpriteSheetPipelineStep)
                return Color.Beige;

            return Color.White;
        }
    }

    /// <summary>
    /// A link for a pipeline step view
    /// </summary>
    public class PipelineStepLinkView : BaseView
    {
        /// <summary>
        /// The connection this link references on its parent step view
        /// </summary>
        public IPipelineConnection Connection { get; }

        /// <summary>
        /// Gets the parent step view for this link view
        /// </summary>
        // ReSharper disable once AnnotateCanBeNullTypeMember
        public PipelineStepView StepView => (PipelineStepView)Parent;
        
        public PipelineStepLinkView(IPipelineConnection connection)
        {
            Connection = connection;
        }
    }

    public class AnimationPipelineStep : IPipelineStep
    {
        public Animation Animation { get; }

        public string Name => Animation.Name;
        public IReadOnlyCollection<IPipelineInput> Input { get; }
        public IReadOnlyCollection<IPipelineOutput> Output { get; }

        public AnimationPipelineStep(Animation animation)
        {
            Animation = animation;
            Input = new IPipelineInput[0];

            Output = new[] { new PipelineOutput() };
        }

        public object[] GetMetadata()
        {
            return new object[0];
        }

        public class PipelineOutput : IPipelineOutput
        {
            public PipelineStepDataType DataType => PipelineStepDataType.PixelAndMetadata;

            public object[] GetMetadata()
            {
                return new object[0];
            }
        }
    }

    public class SpriteSheetPipelineStep : IPipelineStep
    {
        public AnimationSheet AnimationSheet { get; }

        public string Name => AnimationSheet.Name;
        public IReadOnlyCollection<IPipelineInput> Input { get; }
        public IReadOnlyCollection<IPipelineOutput> Output { get; }

        public SpriteSheetPipelineStep(AnimationSheet animationSheet)
        {
            AnimationSheet = animationSheet;
            Input = new[] {new PipelineInput()};
            Output = new[] {new PipelineOutput(), new PipelineOutput() };
        }

        public object[] GetMetadata()
        {
            return new object[0];
        }

        public class PipelineInput : IPipelineInput
        {
            public PipelineStepDataType DataType => PipelineStepDataType.PixelAndMetadata;

            public object[] GetMetadata()
            {
                return new object[0];
            }
        }

        public class PipelineOutput : IPipelineOutput
        {
            public PipelineStepDataType DataType => PipelineStepDataType.PixelAndMetadata;

            public object[] GetMetadata()
            {
                return new object[0];
            }
        }
    }
}

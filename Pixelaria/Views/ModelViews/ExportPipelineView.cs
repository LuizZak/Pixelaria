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

            var sheet = new AnimationSheet("Sheet 1");

            var stepView = new PipelineStepView(new SpriteSheetPipelineStep(sheet))
            {
                Location = new Point(300, 30),
                Size = new Size(80, 60)
            };

            exportPipelineRenderer1.PipelineContainer.AddStepView(stepView);
        }
    }
    
    public class ExportPipelineRenderer: Control
    {
        private readonly InternalPipelineContainer _container = new InternalPipelineContainer();

        [CanBeNull]
        private PipelineStepView _hovering;
        [CanBeNull]
        private PipelineStepView _dragging;

        private Point _dragOffset;

        public IPipelineContainer PipelineContainer => _container;
        
        public ExportPipelineRenderer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);

            _container.Modified += ContainerOnModified;
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

            Render(e.Graphics);
        }

        protected void Render([NotNull] Graphics g)
        {
            g.CompositingMode = CompositingMode.SourceOver;
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = TextRenderingHint.SystemDefault;

            Renderer.Render(_container, g);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _hovering = null;

            SetHovering(null);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                var step = _container.StepViewUnder(e.Location);
                if (step != null)
                {
                    // Start drag operation
                    SetDragging(step, e.Location);
                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButtons.Left)
                SetDragging(null, Point.Empty);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_dragging != null)
            {
                // Dragging
                var beforeArea = _container.ContainingRegion(_dragging).Inflated(5, 5);

                _dragging.Location = Point.Subtract(e.Location, new Size(_dragOffset));

                var afterArea = _container.ContainingRegion(_dragging).Inflated(5, 5);

                var reg = new Region(beforeArea);
                reg.Union(afterArea);
                Invalidate(reg);

                Update();
            }
            else
            {
                // Check hovering on top of a step view
                var newStep = _container.StepViewUnder(e.Location);
                SetHovering(newStep);
            }
        }

        void SetDragging([CanBeNull] PipelineStepView stepView, Point mouseLoc)
        {
            _dragging = stepView;

            if (stepView != null)
            {
                _dragOffset = Point.Subtract(mouseLoc, new Size(stepView.Location));
            }
        }

        void SetHovering([CanBeNull] PipelineStepView stepView)
        {
            if (_hovering == stepView)
                return;

            if (_hovering != null)
            {
                _hovering.RenderMode.Hover = false;
                Invalidate(_container.ContainingRegion(_hovering).Inflated(2, 2));
            }
            if (stepView != null)
            {
                stepView.RenderMode.Hover = true;
                Invalidate(_container.ContainingRegion(stepView).Inflated(2, 2));
            }

            _hovering = stepView;
        }

        /// <summary>
        /// Renders a pipeline export view
        /// </summary>
        private static class Renderer
        {
            public static void Render([NotNull] InternalPipelineContainer container, [NotNull] Graphics g)
            {
                g.Clear(Color.FromKnownColor(KnownColor.Control));

                foreach (var stepView in container.StepViews)
                {
                    RenderStepView(stepView, g, stepView.RenderMode);
                }
            }

            private static void RenderStepView([NotNull] PipelineStepView stepView, [NotNull] Graphics g, PipelineStepView.RenderingMode renderingMode)
            {
                using (var path = new GraphicsPath())
                {
                    path.AddRoundedRectangle(stepView.Area, 5);
                    g.FillPath(Brushes.Beige, path);

                    // Draw title area
                    g.WithTemporaryState(() =>
                    {
                        g.Clip = new Region(path);

                        g.TranslateTransform(stepView.Location.X, stepView.Location.Y);
                        
                        var color = Color.Beige.Fade(Color.Black);
                        using (var brush = new SolidBrush(color))
                        {
                            g.FillRectangle(brush, new Rectangle(0, 0, stepView.Area.Width, 20));
                        }

                        g.DrawString(stepView.Name, DefaultFont, Brushes.White, new PointF(2, 4));

                        g.Flush(FlushIntention.Sync);
                    });
                    
                    if (renderingMode.Hover)
                    {
                        using (var pen = new Pen(Color.Black, 3))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                    else
                    {
                        g.DrawPath(Pens.Black, path);
                    }

                    // Draw out-going links
                    var outLinks = RelativeOutlinkPositions(stepView);

                    g.WithTemporaryState(() =>
                    {
                        g.TranslateTransform(stepView.Location.X, stepView.Location.Y);

                        foreach (var rectangle in RelativeOutlinkPositions(stepView))
                        {
                            g.FillRectangle(Brushes.White, rectangle);

                            using (var pen = new Pen(Color.Black, 1))
                            {
                                g.DrawRectangle(pen, rectangle);
                            }
                        }
                    });
                }
            }

            private static IEnumerable<Rectangle> RelativeOutlinkPositions([NotNull] PipelineStepView stepView)
            {
                if(stepView.Outputs.Count == 0)
                    return new Rectangle[0];

                var linkSize = 5;
                var off = (stepView.Size.Height - 20) / 2 - (stepView.Outputs.Count - 1) * linkSize;

                var links = new List<Rectangle>();

                foreach (var _ in stepView.Outputs)
                {
                    var x = stepView.Size.Width - linkSize / 2;
                    var y = off + linkSize * 2 * links.Count + 20;

                    links.Add(new Rectangle(x, y, linkSize, linkSize));
                }

                return links;
            }
        }

        public interface IPipelineContainer
        {
            void AddStepView(PipelineStepView stepView);
        }

        private class InternalPipelineContainer : IPipelineContainer
        {
            public readonly List<PipelineStepView> StepViews = new List<PipelineStepView>();

            public event EventHandler Modified;

            public void AddStepView([NotNull] PipelineStepView stepView)
            {
                StepViews.Add(stepView);

                Modified?.Invoke(this, EventArgs.Empty);
            }

            [CanBeNull]
            public PipelineStepView StepViewUnder(Point point)
            {
                return StepViews.FirstOrDefault(stepView => stepView.Area.Contains(point));
            }

            public Rectangle ContainingRegion([NotNull] PipelineStepView stepView)
            {
                return stepView.Area;
            }
        }
    }

    public class PipelineStepView
    {
        private readonly IPipelineStep _pipelineStep;

        /// <summary>
        /// Top-left location of step view, in pixels
        /// </summary>
        public Point Location { get; set; }

        public string Name { get; set; } = "Sprite Sheet";

        /// <summary>
        /// Size of step view, in pixels
        /// </summary>
        public Size Size { get; set; }

        /// <summary>
        /// Gets the location/size of this step view
        /// </summary>
        public Rectangle Area => new Rectangle(Location, Size);

        /// <summary>
        /// Current rendering mode for the step view
        /// </summary>
        public RenderingMode RenderMode = new RenderingMode(false);

        /// <summary>
        /// Gets the outputs for this piepline step view
        /// </summary>
        public IReadOnlyCollection<IPipelineOutput> Outputs => _pipelineStep.Output;

        public PipelineStepView(IPipelineStep pipelineStep)
        {
            _pipelineStep = pipelineStep;
        }
        
        public struct RenderingMode
        {
            public bool Hover { get; set; }

            public RenderingMode(bool hover)
            {
                Hover = hover;
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
            Input = new IPipelineInput[0];

            Output = new[] {new PipelineOutput(), new PipelineOutput() };
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
}

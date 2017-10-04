using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews
{
    public partial class ExportPipelineView : Form
    {
        public ExportPipelineView()
        {
            InitializeComponent();

            InitTest();
        }

        public void InitTest()
        {
            var stepView = new PipelineStepView
            {
                Location = new Point(300, 30),
                Size = new Size(40, 60)
            };

            exportPipelineRenderer1.Controls.Add(stepView);
        }
    }

    public class PipelineStepView: Control
    {
        private bool _isDragging;
        private Point _dragPoint;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.Transparent);

            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

            var fillColor = Color.Bisque;
            var strokeColor = fillColor.Fade(Color.Black);

            var path = new GraphicsPath();
            path.AddRoundedRectangle(new RectangleF(Point.Empty, Size).Inflated(-2, -2), 10);
            
            e.Graphics.FillPath(new SolidBrush(fillColor), path);
            e.Graphics.DrawPath(new Pen(strokeColor, 2), path);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _isDragging = true;
            _dragPoint = Point.Subtract(e.Location, new Size(Location));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isDragging)
            {
                Location = Point.Add(e.Location, new Size(_dragPoint));
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _isDragging = false;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // base.OnPaintBackground(pevent);
        }
    }

    public class ExportPipelineRenderer: ContainerControl
    {
        public ExportPipelineRenderer()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}

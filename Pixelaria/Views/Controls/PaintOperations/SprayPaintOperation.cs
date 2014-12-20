using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintOperations.Abstracts;
using Pixelaria.Views.Controls.PaintOperations.Interfaces;

namespace Pixelaria.Views.Controls.PaintOperations
{
    /// <summary>
    /// Implements a Spray paint operation
    /// </summary>
    public class SprayPaintOperation : BasePencilPaintOperation, IColoredPaintOperation, ISizedPaintOperation, ICompositingPaintOperation
    {
        /// <summary>
        /// Instance of a Random class used to randomize the spray of this SprayPaintOperation
        /// </summary>
        readonly Random _random;

        /// <summary>
        /// The spray's timer, used to make the operation paint with the mouse held down at a stationary point
        /// </summary>
        readonly Timer _sprayTimer;

        /// <summary>
        /// Initializes a new instance of the SprayPaintOperation class
        /// </summary>
        public SprayPaintOperation()
        {
            _random = new Random();

            _sprayTimer = new Timer();
            _sprayTimer.Interval = 10;
            _sprayTimer.Tick += sprayTimer_Tick;
        }
        
        /// <summary>
        /// Initializes a new instance of the SprayPaintOperation class, initializing the object with the two spray colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public SprayPaintOperation(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
            size = pencilSize;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            _sprayTimer.Stop();
            _sprayTimer.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// Initializes this PencilPaintOperation
        /// </summary>
        /// <param name="targetPictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.spray_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            undoDecription = "Spray";
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being held down
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseDown(MouseEventArgs e)
        {
            base.MouseDown(e);

            if (mouseDown && e.Button != MouseButtons.Middle)
            {
                _sprayTimer.Start();
            }
        }

        /// <summary>
        /// Called to notify this PaintOperation that the mouse is being released
        /// </summary>
        /// <param name="e">The event args for this event</param>
        public override void MouseUp(MouseEventArgs e)
        {
            base.MouseUp(e);

            if (!mouseDown)
            {
                _sprayTimer.Stop();
            }
        }

        /// <summary>
        /// Draws the pencil with the current properties on the given bitmap object
        /// </summary>
        /// <param name="p">The point to draw the pencil to</param>
        /// <param name="bitmap">The bitmap to draw the pencil on</param>
        protected override void DrawPencil(Point p, Bitmap bitmap)
        {
            // Randomize the point around a circle based on the current radius
            double angle = _random.NextDouble() * Math.PI * 2;
            float radius = (_random.Next(0, size) / 2.0f);

            p.X = p.X + (int)Math.Round(Math.Cos(angle) * radius);
            p.Y = p.Y + (int)Math.Round(Math.Sin(angle) * radius);

            if (WithinBounds(p))
            {
                base.DrawPencil(p, bitmap);
            }
        }

        // 
        // Spray Timer tick
        // 
        private void sprayTimer_Tick(object sender, EventArgs e)
        {
            DrawPencil(GetAbsolutePoint(pencilPoint), (compositingMode == CompositingMode.SourceOver ? currentTraceBitmap : pictureBox.Bitmap));
        }
    }
}
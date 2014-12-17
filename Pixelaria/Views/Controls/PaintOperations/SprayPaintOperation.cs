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
        Random random;

        /// <summary>
        /// The spray's timer, used to make the operation paint with the mouse held down at a stationary point
        /// </summary>
        Timer sprayTimer;

        /// <summary>
        /// Initializes a new instance of the SprayPaintOperation class
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public SprayPaintOperation()
            : base()
        {
            random = new Random();

            sprayTimer = new Timer();
            sprayTimer.Interval = 10;
            sprayTimer.Tick += new EventHandler(sprayTimer_Tick);
        }
        
        /// <summary>
        /// Initializes a new instance of the SprayPaintOperation class, initializing the object
        /// with the two spray colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public SprayPaintOperation(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.FirstColor = firstColor;
            this.SecondColor = secondColor;
            this.Size = pencilSize;
        }

        /// <summary>
        /// Finalizes this Paint Operation
        /// </summary>
        public override void Destroy()
        {
            sprayTimer.Stop();
            sprayTimer.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// Initializes this PencilPaintOperation
        /// </summary>
        /// <param name="pictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox pictureBox)
        {
            base.Initialize(pictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.spray_cursor);
            OperationCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();

            this.undoDecription = "Spray";
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
                sprayTimer.Start();
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
                sprayTimer.Stop();
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
            double angle = random.NextDouble() * Math.PI * 2;
            float radius = (float)((float)(random.Next(0, size) / 2));

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
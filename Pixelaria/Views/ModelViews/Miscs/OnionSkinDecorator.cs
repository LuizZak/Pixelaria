using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using Pixelaria.Views.Controls;

using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews.Miscs
{
    /// <summary>
    /// Decorates the picture box with an onion skin display
    /// </summary>
    public class OnionSkinDecorator : PictureBoxDecorator
    {
        /// <summary>
        /// The frame view binded to this OnionSkinDecorator
        /// </summary>
        protected FrameView frameView;

        /// <summary>
        /// The onion skin bitmap
        /// </summary>
        protected Bitmap onionSkin;

        /// <summary>
        /// Whether the onion skin is currently enabled
        /// </summary>
        public bool OnionSkinEnabled;

        /// <summary>
        /// The depth of the onion skin
        /// </summary>
        public int OnionSkinDepth;

        /// <summary>
        /// The transparency of the onion skin
        /// </summary>
        public float OnionSkinTransparency;

        /// <summary>
        /// The current onion skin mode
        /// </summary>
        public OnionSkinMode OnionSkinMode;

        /// <summary>
        /// Whether to show the current frame on the onion skin
        /// </summary>
        public bool OnionSkinShowCurrentFrame;

        /// <summary>
        /// The event handler for the frame changed event
        /// </summary>
        readonly FrameView.EditFrameChangedEventHandler _frameChangedEventHandler;

        /// <summary>
        /// Initializes a new instance of the OnionSkinDecorator class
        /// </summary>
        /// <param name="frameView">The frame editor view to show the onion skin to</param>
        /// <param name="pictureBox">The picture box to decorate</param>
        public OnionSkinDecorator(FrameView frameView, ImageEditPanel.InternalPictureBox pictureBox)
            : base(pictureBox)
        {
            this.frameView = frameView;

            _frameChangedEventHandler = frameView_EditFrameChanged;

            this.frameView.EditFrameChanged += _frameChangedEventHandler;
        }

        // 
        // Frame View's frame changed event handler
        // 
        private void frameView_EditFrameChanged(object sender, EditFrameChangedEventArgs args)
        {
            if (OnionSkinEnabled)
            {
                DestroyOnionSkin();
                ShowOnionSkin();
            }
        }

        /// <summary>
        /// Initializes this PictureBoxDecorator's instance
        /// </summary>
        public override void Initialize()
        {

        }

        /// <summary>
        /// Destroys this PictureBoxDecorator's instance
        /// </summary>
        public override void Destroy()
        {
            if (onionSkin != null)
            {
                onionSkin.Dispose();
            }

            frameView.EditFrameChanged -= _frameChangedEventHandler;
        }

        /// <summary>
        /// Shows the onion skin
        /// </summary>
        public void ShowOnionSkin()
        {
            OnionSkinEnabled = true;

            if (frameView.FrameLoaded == null)
                return;

            if (onionSkin != null && (onionSkin.Width != frameView.FrameLoaded.Width || onionSkin.Height != frameView.FrameLoaded.Height))
            {
                onionSkin.Dispose();
                onionSkin = null;
            }
            else if (onionSkin != null)
            {
                FastBitmap.ClearBitmap(onionSkin, 0);
            }

            if (onionSkin == null)
            {
                // Create the new onion skin
                onionSkin = new Bitmap(frameView.FrameLoaded.Width, frameView.FrameLoaded.Height, PixelFormat.Format32bppArgb);
            }

            Graphics og = Graphics.FromImage(onionSkin);

            og.CompositingMode = CompositingMode.SourceOver;

            Rectangle bounds = new Rectangle(0, 0, frameView.FrameLoaded.Width, frameView.FrameLoaded.Height);

            // Create image attributes
            ImageAttributes attributes = new ImageAttributes();

            // Create a color matrix object
            ColorMatrix matrix = new ColorMatrix();

            //float multDecay = 0.3f + (0.7f / OnionSkinDepth);
            float multDecay = 0.5f + (OnionSkinDepth / 50.0f);

            // Draw the previous frames
            if (OnionSkinMode == OnionSkinMode.PreviousFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames)
            {
                int fi = frameView.FrameLoaded.Index;
                float mult = 1;
                for (int i = fi - 1; i > fi - OnionSkinDepth - 1 && i >= 0; i--)
                {
                    matrix.Matrix33 = OnionSkinTransparency * mult;
                    mult *= multDecay;
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    og.DrawImage(frameView.FrameLoaded.Animation[i].GetComposedBitmap(), bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            // Draw the next frames
            if (OnionSkinMode == OnionSkinMode.NextFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames)
            {
                int fi = frameView.FrameLoaded.Index;
                float mult = 1;
                for (int i = fi + 1; i < fi + OnionSkinDepth + 1 && i < frameView.FrameLoaded.Animation.FrameCount; i++)
                {
                    matrix.Matrix33 = OnionSkinTransparency * mult;
                    mult *= multDecay;
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    og.DrawImage(frameView.FrameLoaded.Animation[i].GetComposedBitmap(), bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            og.Flush();
            og.Dispose();

            pictureBox.DisplayImage = OnionSkinShowCurrentFrame;
            pictureBox.Invalidate();
        }

        /// <summary>
        /// Hides the onion skin for the current frame
        /// </summary>
        public void HideOnionSkin()
        {
            pictureBox.DisplayImage = true;

            DestroyOnionSkin();

            OnionSkinEnabled = false;
        }

        /// <summary>
        /// Destroys the current onion skin
        /// </summary>
        private void DestroyOnionSkin()
        {
            // Dispose of the onion skin
            if (onionSkin != null)
            {
                onionSkin.Dispose();
                onionSkin = null;
            }

            pictureBox.Invalidate();
        }

        /// <summary>
        /// Decorates the under image, using the given event arguments
        /// </summary>
        /// <param name="image">The under image to decorate</param>
        public override void DecorateUnderImage(Image image)
        {
            if (onionSkin != null)
            {
                Graphics g = Graphics.FromImage(image);

                g.DrawImage(onionSkin, 0, 0);
            }
        }
    }
}
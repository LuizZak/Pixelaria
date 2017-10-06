using System.Drawing;
using System.Drawing.Imaging;
using JetBrains.Annotations;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Utils;
using Pixelaria.Views.Controls.LayerControls;

namespace Pixelaria.Data
{
    /// <summary>
    /// Provides the capability of rendering (or composing) a Frame into a flat final image
    /// </summary>
    public class FrameRenderer
    {
        /// <summary>
        /// Composes the specified frame into a flat bitmap
        /// </summary>
        /// <param name="frame">The frame to compose</param>
        /// <param name="statuses">The layer status information to use when composing the frame</param>
        /// <param name="ignoreStatusTransparency">Whether to ignore the Transparency of a layer when composing</param>
        /// <returns>A new Bitmap object that represents the composed frame</returns>
        public static Bitmap ComposeFrame([NotNull] FrameController frame, LayerStatus[] statuses, bool ignoreStatusTransparency = false)
        {
            var bitmap = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppArgb);

            for (int i = 0; i < frame.LayerCount; i++)
            {
                if (!statuses[i].Visible || (!ignoreStatusTransparency && !(statuses[i].Transparency > 0)))
                    continue;

                var layerBitmap = frame.GetLayerAt(i).LayerBitmap;

                if (ignoreStatusTransparency || statuses[i].Transparency >= 1)
                {
                    ImageUtilities.FlattenBitmaps(bitmap, layerBitmap, false);
                }
                else
                {
                    using (var g = Graphics.FromImage(bitmap))
                    {
                        var cm = new ColorMatrix
                        {
                            Matrix33 = statuses[i].Transparency
                        };

                        var attributes = new ImageAttributes();
                        attributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                        g.DrawImage(layerBitmap, new Rectangle(Point.Empty, layerBitmap.Size), 0, 0, layerBitmap.Width, layerBitmap.Height, GraphicsUnit.Pixel, attributes);

                        g.Flush();
                    }
                }
            }

            return bitmap;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

using Pixelaria.Data;
using Pixelaria.Properties;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Contains static image-related utility methods
    /// </summary>
    public static class ImageUtilities
    {
        /// <summary>
        /// Resizes this Frame so it matches the given dimensions, scaling with the given scaling method, and interpolating
        /// with the given interpolation mode.
        /// Note that trying to resize a frame while it's inside an animation, and that animation's dimensions don't match
        /// the new size, an exception is thrown.
        /// This method disposes of the current frame texture
        /// </summary>
        /// <param name="image">The image to resize</param>
        /// <param name="newWidth">The new width of this animation</param>
        /// <param name="newHeight">The new height of this animation</param>
        /// <param name="scalingMethod">The scaling method to use to match this frame to the new size</param>
        /// <param name="interpolationMode">The interpolation mode to use when drawing the new frame</param>
        public static Image Resize(Image image, int newWidth, int newHeight, PerFrameScalingMethod scalingMethod, InterpolationMode interpolationMode)
        {
            if (image.Width == newWidth && image.Height == newHeight)
                return image;

            Rectangle currentBounds = new Rectangle(0, 0, image.Width, image.Height);
            Rectangle newBounds = new Rectangle(0, 0, newWidth, newHeight);

            // New bounds calculation
            if (scalingMethod == PerFrameScalingMethod.PlaceAtTopLeft)
            {
                newBounds = currentBounds;
            }
            else if (scalingMethod == PerFrameScalingMethod.PlaceAtCenter)
            {
                // Center the sprite
                currentBounds.X = newBounds.Width / 2 - currentBounds.Width / 2;
                currentBounds.Y = newBounds.Height / 2 - currentBounds.Height / 2;

                newBounds = currentBounds;
            }
            else if (scalingMethod == PerFrameScalingMethod.Zoom)
            {
                // If the target size is smaller than the image
                if (newBounds.Width < currentBounds.Width || newBounds.Height < currentBounds.Height)
                {
                    Size size = image.Size;

                    Rectangle rec = newBounds;

                    float num = Math.Min((float)newBounds.Width / currentBounds.Width, (float)newBounds.Height / currentBounds.Height);
                    newBounds.Width = (int)(currentBounds.Width * num);
                    newBounds.Height = (int)(currentBounds.Height * num);
                    newBounds.X = (rec.Width - newBounds.Width) / 2;
                    newBounds.Y = (rec.Height - newBounds.Height) / 2;
                }
                // If the image is smaller than the target size
                else
                {
                    // Center the sprite
                    currentBounds.X = newBounds.Width / 2 - currentBounds.Width / 2;
                    currentBounds.Y = newBounds.Height / 2 - currentBounds.Height / 2;

                    newBounds = currentBounds;
                }
            }

            // New texture creation
            Bitmap newTexture = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);

            Graphics graphics = Graphics.FromImage(newTexture);

            graphics.InterpolationMode = interpolationMode;
            graphics.DrawImage(image, newBounds);

            graphics.Flush();
            graphics.Dispose();

            return newTexture;
        }

        /// <summary>
        /// Returns an Image object that represents the default tile image to be used in the background of controls
        /// that display any type of transparency
        /// </summary>
        /// <returns>
        /// An Image object that represents the default tile image to be used in the background of controlsthat display
        /// any type of transparency
        /// </returns>
        public static Image GetDefaultTile()
        {
            return Resources.checkers_pattern;
        }

        /// <summary>
        /// Returns a Rectangle that specifies the minimum image area, clipping out all the alpha pixels
        /// </summary>
        /// <param name="image">The image to find the mimimum image area</param>
        /// <returns>A Rectangle that specifies the minimum image area, clipping out all the alpha pixels</returns>
        public static Rectangle FindMinimumImageArea(Bitmap image)
        {
            int x = 0;
            int y = 0;
            int widthRange = image.Width - 1;
            int heightRange = image.Height - 1;

            int width = image.Width;
            int height = image.Height;

            FastBitmap fastBitmap = new FastBitmap(image);

            fastBitmap.Lock();

            // Scan horizontally until the first non-0 alpha pixel is found
            for (x = 0; x < width; x++)
            {
                for (int _y = 0; _y < height; _y++)
                {
                    if (fastBitmap.GetPixelInt(x, _y) >> 24 != 0)
                    {
                        goto skipx;
                    }
                }
            }

        skipx:

            widthRange -= x;

            // Scan vertically until the first non-0 alpha pixel is found
            for (y = 0; y < height; y++)
            {
                for (int _x = x; _x < width; _x++)
                {
                    if (fastBitmap.GetPixelInt(_x, y) >> 24 != 0)
                    {
                        goto skipy;
                    }
                }
            }

        skipy:

            heightRange -= y;

            // Scan the width now and skip the empty pixels
            for (; widthRange > x; widthRange--)
            {
                for (int _y = y; _y < height; _y++)
                {
                    if (fastBitmap.GetPixelInt(x + widthRange, _y) >> 24 != 0)
                    {
                        goto skipwidth;
                    }
                }
            }

        skipwidth:

            // Scan the height now and skip the empty pixels
            for (; heightRange > y; heightRange--)
            {
                for (int _x = x; _x < x + widthRange + 1; _x++)
                {
                    if (fastBitmap.GetPixelInt(_x, heightRange + y) >> 24 != 0)
                    {
                        goto skipheight;
                    }
                }
            }

        skipheight:

            fastBitmap.Unlock();

            return new Rectangle(x, y, widthRange + 1, heightRange + 1);
        }
    }
}
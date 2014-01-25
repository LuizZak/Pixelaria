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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Custom picture box used to animate images
    /// </summary>
    public class CPictureBox : PictureBox
    {
        /// <summary>
        /// Gets or sets the image layout to use when drawing the image
        /// </summary>
        [Category("Appearance")]
        [Browsable(true)]
        [DefaultValue(ImageLayout.None)]
        [Description("The image layout used for the component.")]
        public ImageLayout ImageLayout { get; set; }

        /// <summary>
        /// Gets ot sets the interpolation mode to use when drawing the images
        /// </summary>
        [Category("Appearance")]
        [Browsable(true)]
        [DefaultValue(InterpolationMode.NearestNeighbor)]
        [Description("The interpolation mode to use when drawing the images.")]
        public InterpolationMode ImageInterpolationMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the CPictureBox class
        /// </summary>
        public CPictureBox()
        {
            ImageInterpolationMode = InterpolationMode.NearestNeighbor;
        }

        // 
        // OnPaint event handler. Draws the image zoomed to the panel
        // 
        protected override void OnPaint(PaintEventArgs pe)
        {
            if (Image != null)
            {
                pe.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                Rectangle rec = CalculateBackgroundImageRectangle(this.ClientRectangle, this.Image, ImageLayout);

                pe.Graphics.PixelOffsetMode = PixelOffsetMode.Half;

                pe.Graphics.DrawImage(this.Image, rec);
            }
        }

        // 
        // OnPaintBackground event handler. Draws the background image tiled behind the image
        // 
        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            pevent.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            base.OnPaintBackground(pevent);
        }

        // 
        // C# assembly implementation
        // 
        internal static Rectangle CalculateBackgroundImageRectangle(Rectangle bounds, Image backgroundImage, ImageLayout imageLayout)
        {
            Rectangle rectangle = bounds;
            if (backgroundImage != null)
            {
                switch (imageLayout)
                {
                    case ImageLayout.None:
                        rectangle.Size = backgroundImage.Size;
                        return rectangle;

                    case ImageLayout.Tile:
                        return rectangle;

                    case ImageLayout.Center:
                        {
                            rectangle.Size = backgroundImage.Size;
                            Size size = bounds.Size;
                            if (size.Width > rectangle.Width)
                            {
                                rectangle.X = (size.Width - rectangle.Width) / 2;
                            }
                            if (size.Height > rectangle.Height)
                            {
                                rectangle.Y = (size.Height - rectangle.Height) / 2;
                            }
                            return rectangle;
                        }
                    case ImageLayout.Stretch:
                        rectangle.Size = bounds.Size;
                        return rectangle;

                    case ImageLayout.Zoom:
                        {
                            Size size2 = backgroundImage.Size;
                            float num = ((float)bounds.Width) / ((float)size2.Width);
                            float num2 = ((float)bounds.Height) / ((float)size2.Height);
                            if (num >= num2)
                            {
                                rectangle.Height = bounds.Height;
                                rectangle.Width = (int)((size2.Width * num2) + 0.5);
                                if (bounds.X >= 0)
                                {
                                    rectangle.X = (bounds.Width - rectangle.Width) / 2;
                                }
                                return rectangle;
                            }
                            rectangle.Width = bounds.Width;
                            rectangle.Height = (int)((size2.Height * num) + 0.5);
                            if (bounds.Y >= 0)
                            {
                                rectangle.Y = (bounds.Height - rectangle.Height) / 2;
                            }
                            return rectangle;
                        }
                }
            }
            return rectangle;
        }
    }
}
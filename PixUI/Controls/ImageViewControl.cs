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

using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixRendering;

namespace PixUI.Controls
{
    /// <summary>
    /// A view which displays a bitmap as its contents.
    /// </summary>
    public class ImageViewControl: ControlView
    {
        private IManagedImageResource _image;
        private ImageFitMode _imageFitMode;
        private ImageInterpolationMode _interpolationMode;
        private ImageResource? _imageResource;

        private int ImageWidth => Image?.Width ?? ImageResource?.Width ?? 0;
        private int ImageHeight => Image?.Height ?? ImageResource?.Height ?? 0;

        /// <summary>
        /// Gets or sets the image resource to render on this image view.
        ///
        /// The <see cref="Image"/> property, if not null, takes precedence over this property
        /// when rendering.
        /// </summary>
        public ImageResource? ImageResource
        {
            get => _imageResource;
            set
            {
                _imageResource = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the managed image resource for this image view.
        ///
        /// This property, if not null, takes precedence over <see cref="ImageResource"/>
        /// when rendering.
        /// </summary>
        [CanBeNull]
        public IManagedImageResource Image
        {
            get => _image;
            set
            {
                _image = value;
                if (AutoSize)
                    Layout();
            }
        }

        /// <summary>
        /// Gets or sets the layout mode for the image within this control.
        /// </summary>
        public ImageFitMode ImageFitMode
        {
            get => _imageFitMode;
            set
            {
                _imageFitMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the interpolation mode for this image view's image.
        /// </summary>
        public ImageInterpolationMode InterpolationMode
        {
            get => _interpolationMode;
            set
            {
                _interpolationMode = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets whether this view should layout its content size based
        /// on the size of its image.
        /// </summary>
        public bool AutoSize { get; set; }

        /// <summary>
        /// Creates a new <see cref="ImageViewControl"/> with a specified <see cref="ImageResource"/>
        /// to render.
        /// </summary>
        public static ImageViewControl Create(ImageResource imageResource)
        {
            var imageControl = new ImageViewControl
            {
                ImageResource = imageResource
            };
            imageControl.Initialize();

            return imageControl;
        }

        /// <summary>
        /// Creates a new <see cref="ImageViewControl"/> with a specified <see cref="IManagedImageResource"/>
        /// to render.
        /// </summary>
        public static ImageViewControl Create(IManagedImageResource image)
        {
            var imageControl = new ImageViewControl
            {
                Image = image
            };
            imageControl.Initialize();

            return imageControl;
        }

        protected ImageViewControl()
        {

        }

        protected virtual void Initialize()
        {

        }

        public override void RenderBackground(ControlRenderingContext context)
        {
            base.RenderBackground(context);

            var rect = DrawingUtilities.RectangleFit(new RectangleF(0, 0, Width, Height), new SizeF(ImageWidth, ImageHeight), ToImageLayout(ImageFitMode));

            if (Image != null)
            {
                if (ImageFitMode == ImageFitMode.Tile)
                {
                    var brush = context.Renderer.CreateBitmapBrush(Image);
                    context.Renderer.SetFillBrush(brush);
                    context.Renderer.FillRectangle((RectangleF)Bounds);
                }
                else
                {
                    context.Renderer.DrawBitmap(Image, (AABB)rect, 1, InterpolationMode);
                }
            }
            else if (ImageResource != null)
            {
                if (ImageFitMode == ImageFitMode.Tile)
                {
                    var brush = context.Renderer.CreateBitmapBrush(ImageResource.Value);
                    context.Renderer.SetFillBrush(brush);
                    context.Renderer.FillRectangle((RectangleF)Bounds);
                }
                else
                {
                    context.Renderer.DrawBitmap(ImageResource.Value, (AABB)rect, 1, InterpolationMode);
                }
            }
        }

        public override void Layout()
        {
            base.Layout();

            if (AutoSize)
            {
                Width = ImageWidth;
                Height = ImageHeight;
            }
        }

        private static ImageLayout ToImageLayout(ImageFitMode imageFitMode)
        {
            return (ImageLayout) imageFitMode;
        }
    }

    /// <summary>
    /// Specifies the position of the image on the control.
    /// </summary>
    public enum ImageFitMode
    {
        /// <summary>
        /// The image is left-aligned at the top across the control's client rectangle.
        /// </summary>
        None,
        /// <summary>
        /// The image is tiled across the control's client rectangle.
        /// </summary>
        Tile,
        /// <summary>
        /// The image is centered within the control's client rectangle.
        /// </summary>
        Center,
        /// <summary>
        /// The image is stretched across the control's client rectangle.
        /// </summary>
        Stretch,
        /// <summary>
        /// The image is enlarged within the control's client rectangle.
        /// </summary>
        Zoom
    }
}

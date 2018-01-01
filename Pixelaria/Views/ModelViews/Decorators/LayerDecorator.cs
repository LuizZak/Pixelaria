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
using System.Drawing.Imaging;
using FastBitmapLib;
using PixCore.Imaging;
using Pixelaria.Controllers.LayerControlling;
using Pixelaria.Data;
using Pixelaria.Filters;
using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.LayerControls;

namespace Pixelaria.Views.ModelViews.Decorators
{
    /// <summary>
    /// Decorator class used to display all the layers of a frame at once
    /// </summary>
    public class LayerDecorator : PictureBoxDecorator
    {
        /// <summary>
        /// The statuses for the layers being rendered on this layer decorator
        /// </summary>
        private LayerStatus[] _layerStatuses;

        /// <summary>
        /// The layer controller used to fetch information about the layers
        /// </summary>
        private readonly LayerController _layerController;

        /// <summary>
        /// Gets or sets the layer statuses for the layers to be rendered on this layer decorator
        /// </summary>
        public LayerStatus[] LayerStatuses
        {
            get => _layerStatuses;
            set => _layerStatuses = value;
        }

        /// <summary>
        /// Initializes a new instance of the LayerDecorator class
        /// </summary>
        /// <param name="pictureBox">The picture box to decorate</param>
        /// <param name="controller">The layer controller for this layer decorator</param>
        public LayerDecorator(PaintingOperationsPictureBox pictureBox, LayerController controller) : base(pictureBox)
        {
            _layerController = controller;
        }

        /// <summary>
        /// Initializes this layer picture box decorator
        /// </summary>
        public override void Initialize()
        {
            
        }

        // 
        // Decorate Under Image method
        // 
        public override void DecorateUnderBitmap(Bitmap bitmap)
        {
            base.DecorateUnderBitmap(bitmap);

            ApplyOnBitmap(bitmap, LayerSide.BottomLayers);
        }

        // 
        // Decorate Main Image method
        // 
        public override void DecorateMainBitmap(Bitmap bitmap)
        {
            base.DecorateMainBitmap(bitmap);

            if (!_layerStatuses[_layerController.ActiveLayerIndex].Visible)
            {
                FastBitmap.ClearBitmap(bitmap, Color.Transparent);
            }
            // Transparent layer
            else if (_layerStatuses[_layerController.ActiveLayerIndex].Transparency < 1)
            {
                TransparencyFilter filter = new TransparencyFilter
                {
                    Transparency = _layerStatuses[_layerController.ActiveLayerIndex].Transparency
                };

                filter.ApplyToBitmap(bitmap);
            }
        }

        // 
        // Decorate Over Image method
        // 
        public override void DecorateOverBitmap(Bitmap bitmap)
        {
            base.DecorateOverBitmap(bitmap);

            ApplyOnBitmap(bitmap, LayerSide.TopLayers);
        }

        /// <summary>
        /// Applies the layer rendering on a given set of bitmaps
        /// </summary>
        /// <param name="bitmap">The bitmap to apply the layer rendering on</param>
        /// <param name="side">Specifies which side of the layers to draw</param>
        private void ApplyOnBitmap(Bitmap bitmap, LayerSide side)
        {
            // Iterate through and render each layer up to the current layer
            IFrameLayer[] layers = _layerController.FrameLayers;

            int min = (side == LayerSide.BottomLayers ? 0 : _layerController.ActiveLayerIndex + 1);
            int max = (side == LayerSide.BottomLayers ? _layerController.ActiveLayerIndex : layers.Length);

            for (int i = min; i < max; i++)
            {
                if (_layerStatuses[i].Visible && _layerStatuses[i].Transparency > 0)
                {
                    var layerBitmap = layers[i].LayerBitmap;

                    if (_layerStatuses[i].Transparency >= 1)
                    {
                        ImageUtilities.FlattenBitmaps(bitmap, layerBitmap, false);
                    }
                    else
                    {
                        using(var g = Graphics.FromImage(bitmap))
                        {
                            var cm = new ColorMatrix
                            {
                                Matrix33 = _layerStatuses[i].Transparency
                            };

                            var attributes = new ImageAttributes();
                            attributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                            g.DrawImage(layerBitmap, new Rectangle(Point.Empty, layerBitmap.Size), 0, 0, layerBitmap.Width, layerBitmap.Height, GraphicsUnit.Pixel, attributes);

                            g.Flush();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Specifies which layers to render during a call to ApplyOnBitmap
        /// </summary>
        private enum LayerSide
        {
            /// <summary>
            /// Specifies to render the bottom layers
            /// </summary>
            BottomLayers,
            /// <summary>
            /// Specifies to render the top layers
            /// </summary>
            TopLayers
        }
    }
}
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
using Pixelaria.Controllers.LayerControlling;
using Pixelaria.Data;
using Pixelaria.Utils;
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
            get { return _layerStatuses; }
            set { _layerStatuses = value; }
        }

        /// <summary>
        /// Initializes a new instance of the LayerDecorator class
        /// </summary>
        /// <param name="pictureBox">The picture box to decorate</param>
        /// <param name="controller">The layer controller for this layer decorator</param>
        public LayerDecorator(ImageEditPanel.InternalPictureBox pictureBox, LayerController controller) : base(pictureBox)
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

            // Iterate through and render each layer up to the current layer
            IFrameLayer[] layers = _layerController.FrameLayers;

            for (int i = 0; i < _layerController.ActiveLayerIndex; i++)
            {
                if (_layerStatuses[i].Visible)
                {
                    Utilities.FlattenBitmaps(bitmap, layers[i].LayerBitmap);
                }
            }
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
        }

        // 
        // Decorate Over Image method
        // 
        public override void DecorateOverBitmap(Bitmap bitmap)
        {
            base.DecorateOverBitmap(bitmap);

            // Iterate through and render each layer up to the current layer
            IFrameLayer[] layers = _layerController.FrameLayers;

            for (int i = _layerController.ActiveLayerIndex + 1; i < layers.Length; i++)
            {
                if (_layerStatuses[i].Visible)
                {
                    Utilities.FlattenBitmaps(bitmap, layers[i].LayerBitmap);
                }
            }
        }
    }
}
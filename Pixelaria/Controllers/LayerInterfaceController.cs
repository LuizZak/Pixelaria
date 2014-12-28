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
using System.Linq;
using System.Text;
using Pixelaria.Views.Controls;

namespace Pixelaria.Controllers
{
    /// <summary>
    /// Class that is used as an interface for layer management
    /// </summary>
    public class LayerInterfaceController
    {
        
    }

    /// <summary>
    /// Settings to use when displaying a layer on a LayerDecorator
    /// </summary>
    public class LayerDisplaySettings
    {
        /// <summary>
        /// List of values specifying whether to display each layer registered
        /// </summary>
        private List<bool> _visibilidyList;
    }

    /// <summary>
    /// Decorator class used to display all the layers of a frame at once
    /// </summary>
    public class LayerDecorator : PictureBoxDecorator
    {
        /// <summary>
        /// The display settings for this layer decorator
        /// </summary>
        private LayerDisplaySettings _displaySettings;

        /// <summary>
        /// Initializes a new instance of the LayerDecorator class
        /// </summary>
        /// <param name="pictureBox">The picture box to decorate</param>
        public LayerDecorator(ImageEditPanel.InternalPictureBox pictureBox) : base(pictureBox)
        {

        }

        /// <summary>
        /// Initializes this layer picture box decorator
        /// </summary>
        public override void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
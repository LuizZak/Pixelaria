using System;
using Pixelaria.Controllers.LayerControlling;
using Pixelaria.Views.Controls;

namespace Pixelaria.Views.ModelViews.Decorators
{
    /// <summary>
    /// Decorator class used to display all the layers of a frame at once
    /// </summary>
    public class LayerDecorator : PictureBoxDecorator
    {
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
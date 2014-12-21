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

using Pixelaria.Controllers;

namespace Pixelaria.Data.Factories
{
    /// <summary>
    /// Represents a factory that is able to correctly manipulate the creation and manipulation of frames
    /// </summary>
    public class DefaultFrameFactory : IFrameFactory
    {
        /// <summary>
        /// A reference to the main controller
        /// </summary>
        private readonly Controller _controller;

        /// <summary>
        /// Initializes a new instance of the DefaultFrameFactory class
        /// </summary>
        /// <param name="controller">The controller to attach this factory to</param>
        public DefaultFrameFactory(Controller controller)
        {
            _controller = controller;
        }

        /// <summary>
        /// Creates a new frame with the given resolution and an optional starting animation and hash
        /// </summary>
        /// <param name="width">The width of the frame</param>
        /// <param name="height">The height of the frame</param>
        /// <param name="anim">An animation to parent the frame</param>
        /// <param name="initHash">Whether to init the frame hash now</param>
        /// <returns>The newly created frame</returns>
        public Frame CreateFrame(int width, int height, Animation anim = null, bool initHash = true)
        {
            Frame frame = new Frame(anim, width, height, initHash)
            {
                ID = _controller.CurrentBundle.GetNextValidFrameID()
            };

            return frame;
        }

        /// <summary>
        /// Returns a clone copy of the given frame
        /// </summary>
        /// <returns>A clone copy of the given frame</returns>
        public Frame CloneFrame(IFrame frame)
        {
            Frame newFrame = frame.Clone();

            newFrame.ID = _controller.CurrentBundle.GetNextValidFrameID();

            return newFrame;
        }
    }

    /// <summary>
    /// Interface for FrameFactory objects
    /// </summary>
    public interface IFrameFactory
    {
        /// <summary>
        /// Creates a new frame with the given resolution and an optional starting animation and hash
        /// </summary>
        /// <param name="width">The width of the frame</param>
        /// <param name="height">The height of the frame</param>
        /// <param name="anim">An animation to parent the frame</param>
        /// <param name="initHash">Whether to init the frame hash now</param>
        /// <returns>The newly created frame</returns>
        Frame CreateFrame(int width, int height, Animation anim = null, bool initHash = true);

        /// <summary>
        /// Returns a clone copy of the given frame
        /// </summary>
        /// <returns>A clone copy of the given frame</returns>
        Frame CloneFrame(IFrame frame);
    }
}
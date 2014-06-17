using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pixelaria.Controllers;
using Pixelaria.Data;

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
        private Controller controller;

        /// <summary>
        /// Initializes a new instance of the DefaultFrameFactory class
        /// </summary>
        /// <param name="controller">The controller to attach this factory to</param>
        public DefaultFrameFactory(Controller controller)
        {
            this.controller = controller;
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
            Frame frame = new Frame(anim, width, height, initHash);

            frame.ID = controller.CurrentBundle.GetNextValidFrameID();

            return frame;
        }

        /// <summary>
        /// Returns a clone copy of the given frame
        /// </summary>
        /// <returns>A clone copy of the given frame</returns>
        public Frame CloneFrame(Frame frame)
        {
            Frame newFrame = frame.Clone();

            newFrame.ID = controller.CurrentBundle.GetNextValidFrameID();

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
        Frame CloneFrame(Frame frame);
    }
}
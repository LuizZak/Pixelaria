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
using JetBrains.Annotations;

namespace Pixelaria.Data.Factories
{
    /// <summary>
    /// Represents a factory that is able to correctly manipulate the creation and manipulation of frames
    /// </summary>
    public class FrameFactory
    {
        /// <summary>
        /// A frame ID generator for generating unique frame IDs with
        /// </summary>
        public IFrameIdGenerator FrameIdGenerator { get; set; }

        /// <summary>
        /// Initializes a new instance of the DefaultFrameFactory class
        /// </summary>
        /// <param name="frameIdGenerator">A source for unique frame IDs</param>
        public FrameFactory(IFrameIdGenerator frameIdGenerator)
        {
            FrameIdGenerator = frameIdGenerator;
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
            var frame = new Frame(anim, width, height, initHash)
            {
                ID = GetNextUniqueFrameId()
            };

            return frame;
        }

        /// <summary>
        /// Returns a clone copy of the given frame
        /// </summary>
        /// <returns>A clone copy of the given frame</returns>
        public Frame CloneFrame([NotNull] IFrame frame)
        {
            var newFrame = frame.Clone();

            newFrame.ID = GetNextUniqueFrameId();

            return newFrame;
        }

        /// <summary>
        /// Returns a unique ID that is fit to be used as a unique identifier for a frame.
        /// This method always returns the same value until a frame with the given ID is inserted on the bundle of the controller associated
        /// with this factory.
        /// If no frame ID generator is currently set, an <see cref="InvalidOperationException"/> is raised.
        /// </summary>
        /// <exception cref="InvalidOperationException">No bundle is setup on the controller</exception>
        public int GetNextUniqueFrameId()
        {
            var generator = FrameIdGenerator;
            if (generator == null)
                throw new InvalidOperationException(@"No frame ID generator is set - cannot generate unique IDs");

            return generator.GetNextUniqueFrameId();
        }
    }

    /// <summary>
    /// Interface for objects fit for providing unique IDs to objects that require so
    /// </summary>
    public interface IFrameIdGenerator
    {
        /// <summary>
        /// Returns a unique ID that is fit to be used as a unique identifier for a frame
        /// </summary>
        int GetNextUniqueFrameId();
    }
}
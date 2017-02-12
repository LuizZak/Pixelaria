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
using System.Linq;

using Pixelaria.Data;
using Pixelaria.Data.Factories;

namespace Pixelaria.Controllers.DataControllers
{
    public class AnimationController
    {
        private readonly Animation _animation;

        public IFrameIdGenerator FrameIdGenerator;

        /// <summary>
        /// Returns the count of frames in this frame controller
        /// </summary>
        public int FrameCount => _animation.FrameCount;

        /// <summary>
        /// Gets the width of the animation
        /// </summary>
        public int Width => _animation.Width;

        /// <summary>
        /// Gets the height of the animation
        /// </summary>
        public int Height => _animation.Height;

        public AnimationController(Animation animation)
        {
            _animation = animation;
        }
        
        /// <summary>
        /// Returns all frame ids in this frame controller in order
        /// </summary>
        public IFrameId[] GetFrames()
        {
            return _animation.Frames.Select(GetIdForFrame).ToArray();
        }

        /// <summary>
        /// Returns the identifier for a frame at a given index in this frame controller
        /// </summary>
        public IFrameId GetFrameAtIndex(int index)
        {
            return GetIdForFrame(_animation.Frames[index]);
        }
        
        /// <summary>
        /// Swaps the two given frame indices 
        /// </summary>
        /// <param name="index1">The first index to swap</param>
        /// <param name="index2">The second index to swap</param>
        public void SwapFrameIndices(int index1, int index2)
        {
            var temp = _animation.Frames[index1];
            _animation.Frames[index1] = _animation.Frames[index2];
            _animation.Frames[index2] = temp;
        }
        
        /// <summary>
        /// Duplicates a frame from the given index and inserts it at the new index
        /// </summary>
        /// <param name="frameIndex">The index of the frame to duplicate</param>
        /// <param name="newIndex">The index to put the frame on. Leave -1 to put it at the front of the duplicated frame</param>
        /// <returns>The newly duplicated frame</returns>
        public IFrame DuplicateFrame(int frameIndex, int newIndex = -1)
        {
            var dup = _animation.Frames[frameIndex].Clone();

            if (newIndex == -1)
            {
                _animation.Frames.Insert(frameIndex + 1, dup);
            }
            else
            {
                _animation.Frames.Insert(newIndex, dup);
            }

            return dup;
        }

        /// <summary>
        /// Creates a new Frame, adds it to the given position and returns it
        /// </summary>
        /// <param name="position">The position to add the frame. Leave -1 to add to the end of the frames interval</param>
        /// <returns>The newly created frame</returns>
        public IFrameId CreateFrame(int position = -1)
        {
            return CreateFrame<Frame>(position);
        }

        /// <summary>
        /// Creates a new Frame, adds it to the given position and returns it
        /// </summary>
        /// <param name="position">The position to add the frame. Leave -1 to add to the end of the frames interval</param>
        /// <returns>The newly created frame</returns>
        public IFrameId CreateFrame<T>(int position = -1) where T : IFrame, new()
        {
            var frame = new T();

            frame.Initialize(_animation, Width, Height);

            if (FrameIdGenerator != null)
            {
                frame.ID = FrameIdGenerator.GetNextUniqueFrameId();
            }

            if (position == -1)
            {
                _animation.Frames.Add(frame);
            }
            else
            {
                _animation.Frames.Insert(position, frame);
            }

            return GetIdForFrame(frame);
        }

        /// <summary>
        /// Removes the given Frame from this Animation object
        /// </summary>
        /// <param name="frame">The frame to remove</param>
        public void RemoveFrame(IFrameId frame)
        {
            var index = GetFrameIndex(frame);
            if(index == -1)
                throw new ArgumentException($"No frame with id ${frame} found", nameof(frame));

            RemoveFrameIndex(index);
        }

        /// <summary>
        /// Removes a Frame at the given index from this Animation object
        /// </summary>
        /// <param name="frameIndex">The index of the frame to remove</param>
        public void RemoveFrameIndex(int frameIndex)
        {
            _animation.Frames[frameIndex].Animation = null;
            _animation.Frames.RemoveAt(frameIndex);
        }

        /// <summary>
        /// Returns whether this Animation object contains a given Frame
        /// </summary>
        /// <param name="frame">A Frame object</param>
        /// <returns>Whether the provided frame is listed in this animation</returns>
        public bool ContainsFrame(IFrameId frame)
        {
            return GetFrameIndex(frame) > -1;
        }

        /// <summary>
        /// Gets the index of the given frame on this animation
        /// </summary>
        /// <param name="frame">The frame on this animation to get the index of</param>
        /// <returns>An integer representing the index at which the frame resides, or -1 if the frame is not located inside this animation</returns>
        public int GetFrameIndex(IFrameId frame)
        {
            return _animation.Frames.FindIndex(f => f.ID == frame.Id);
        }
        
        /// <summary>
        /// Gets a frame controller for controlling a specified frame id in this animation
        /// </summary>
        public FrameController GetFrameController(IFrameId frame)
        {
            var f = _animation.GetFrameByID(((FrameId)frame).Id);

            return new FrameController((Frame)f);
        }

        private static IFrameId GetIdForFrame(IFrame frame)
        {
            return new FrameId(frame.ID);
        }

        private struct FrameId : IFrameId
        {
            public int Id { get; }

            public FrameId(int id)
            {
                Id = id;
            }

            public override string ToString()
            {
                return $"{Id}";
            }
        }
    }
}
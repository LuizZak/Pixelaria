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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixLib.Data;
using PixLib.Data.Factories;
using PixLib.Utils;

namespace PixLib.Controllers.DataControllers
{
    public class AnimationController: IDisposable
    {
        private bool _disposed;
        [CanBeNull]
        private readonly Bundle _bundle;

        private readonly Animation _animation;

        /// <summary>
        /// If this animation controller is handling a .Clone() of another animation
        /// controller, this field points to that original controller instance, otherwise
        /// this is null.
        /// </summary>
        private AnimationController _original;

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

        /// <summary>
        /// Gets the size of the animation
        /// </summary>
        public Size Size => _animation.Size;

        /// <summary>
        /// Gets or sets the playback settings for the animation
        /// </summary>
        public AnimationPlaybackSettings PlaybackSettings
        {
            get => _animation.PlaybackSettings;
            set => _animation.PlaybackSettings = value;
        }

        /// <summary>
        /// Gets or sets the export settings for the animation
        /// </summary>
        public AnimationSheetExportSettings SheetExportSettings
        {
            get => _animation.SheetExportSettings;
            set => _animation.SheetExportSettings = value;
        }

        public AnimationController([CanBeNull] Bundle bundle, Animation animation)
        {
            _bundle = bundle;
            FrameIdGenerator = _bundle;
            _animation = animation;
        }

        ~AnimationController()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            // Dispose only if we're a copy of a previously stable original version
            Debug.Assert(_original != null, "_original != null", "Trying to discard original animation controller that points to on-disk/storage animation.");

            _animation.Dispose();
            _disposed = true;
        }

        /// <summary>
        /// Returns whether this and a second animation controller control the same underlying
        /// animation.
        /// </summary>
        public bool MatchesController([NotNull] AnimationController other)
        {
            return ReferenceEquals(_animation, other._animation) || _animation.ID == other._animation.ID;
        }

        /// <summary>
        /// Returns a view-only interface for the underlying animation
        /// </summary>
        public IAnimation GetAnimationView()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            return _animation;
        }

        /// <summary>
        /// Deep-clones the animation this animation controller manages, returning an exact
        /// copy that has the same matching IDs.
        /// 
        /// This is intended to be used during editing of animations on forms to track changes
        /// locally.
        /// </summary>
        public AnimationController MakeCopyForEditing(bool copyThis)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            var newAnim = CloneAnimation();

            for (int i = 0; i < _animation.FrameCount; i++)
            {
                newAnim[i].ID = _animation[i].ID;
            }

            newAnim.ID = _animation.ID;

            return new AnimationController(_bundle, newAnim) { _original = copyThis ? this : _original ?? this };
        }

        /// <summary>
        /// Pushes changes to original animation controller, and consequently to the base bundle.
        /// </summary>
        public void ApplyChanges()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            _original?.InternalApplyChanges(this);
        }

        private void InternalApplyChanges([NotNull] AnimationController animation)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            CopyFrom(animation, false);

            for (int i = 0; i < _animation.FrameCount; i++)
            {
                _animation[i].ID = animation._animation[i].ID;
                // Update invalid (negative) frame IDs
                if (_animation[i].ID == -1)
                    _animation[i].ID = FrameIdGenerator.GetNextUniqueFrameId();
            }
        }

        /// <summary>
        /// Returns the memory usage of this animation, in bytes
        /// </summary>
        /// <param name="composed">Whether to calculate the memory usage of the frames after they have been composed into single images</param>
        /// <returns>Total memory usage, in bytes</returns>
        public long CalculateMemoryUsageInBytes(bool composed)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            return _animation.Frames.Sum(frame => frame.CalculateMemoryUsageInBytes(composed));
        }

        /// <summary>
        /// Resizes this Animation object to fit the given dimensions
        /// </summary>
        /// <param name="sizeMatchingSettings">The settings to apply to the frames when resizing</param>
        public void Resize(AnimationResizeSettings sizeMatchingSettings)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            _animation.Resize(sizeMatchingSettings);
        }
        
        /// <summary>
        /// Returns an Animation object that is the exact copy of the Animation this controller handles.
        /// The method copies also the underlying frames, allocating more memory to store the copied frames.
        /// The ID of the animation is not copied for consistency and identity purposes
        /// </summary>
        /// <returns>an Animation object that is the exact copy of this Animation</returns>
        public Animation CloneAnimation()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            var copy = new AnimationController(_bundle, new Animation(_animation.Name, Width, Height));
            
            copy.CopyFrom(this, false);

            return copy._animation;
        }

        /// <summary>
        /// Copies the data from the given Animation object into this animation.
        /// This method copies the width, height, name, and frames to this Animation.
        /// Enabling deep copies allow the cloning of frames as well instead of only
        /// references
        /// </summary>
        /// <param name="anim">The Animation object to copy the properties from</param>
        /// <param name="moveFrames">Whether to remove the frames form the given animation and add them to this Animation. When set to false, instead of moving the frames, the methods clones the frames</param>
        public void CopyFrom([NotNull] AnimationController anim, bool moveFrames)
        {
            _animation.Name = anim._animation.Name;
            _animation.Width = anim.Width;
            _animation.Height = anim.Height;
            PlaybackSettings = anim.PlaybackSettings;
            SheetExportSettings = anim.SheetExportSettings;

            _animation.Clear();

            // Copy frames now
            foreach (var frame in anim._animation.Frames)
            {
                if (moveFrames)
                {
                    frame.Animation = null;
                    
                    AddFrame(frame);
                }
                else
                {
                    var cloneFrame = frame.Clone();

                    if (FrameIdGenerator != null)
                        cloneFrame.ID = FrameIdGenerator.GetNextUniqueFrameId();

                    AddFrame(cloneFrame);
                }
            }

            if (moveFrames)
            {
                anim._animation.Frames.Clear();
            }
        }

        /// <summary>
        /// Changes the name of the underlying animation
        /// </summary>
        /// <param name="newName">New name for animation</param>
        public void SetName(string newName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            _animation.Name = newName;
        }

        /// <summary>
        /// Returns all frame ids in this frame controller in order
        /// </summary>
        [NotNull]
        public IFrameId[] GetFrames()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            return _animation.Frames.Select(GetIdForFrame).ToArray();
        }

        /// <summary>
        /// Returns the identifier for a frame at a given index in this frame controller
        /// </summary>
        [NotNull]
        public IFrameId GetFrameAtIndex(int index)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            return GetIdForFrame(_animation.Frames[index]);
        }
        
        /// <summary>
        /// Swaps the two given frame indices 
        /// </summary>
        /// <param name="index1">The first index to swap</param>
        /// <param name="index2">The second index to swap</param>
        public void SwapFrameIndices(int index1, int index2)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

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
        [NotNull]
        public IFrameId DuplicateFrame(int frameIndex, int newIndex = -1)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            var frame = _animation.Frames[frameIndex].Clone();
            frame.Animation = _animation;

            if (FrameIdGenerator != null)
                frame.ID = FrameIdGenerator.GetNextUniqueFrameId();

            if (newIndex == -1)
            {
                _animation.Frames.Insert(frameIndex + 1, frame);
            }
            else
            {
                _animation.Frames.Insert(newIndex, frame);
            }

            return GetIdForFrame(frame);
        }

        /// <summary>
        /// Creates a new Frame, adds it to the given position and returns it
        /// </summary>
        /// <param name="position">The position to add the frame. Leave -1 to add to the end of the frames interval</param>
        /// <returns>The newly created frame</returns>
        [NotNull]
        public IFrameId CreateFrame(int position = -1)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            return CreateFrame<Frame>(position);
        }

        /// <summary>
        /// Creates a new Frame, adds it to the given position and returns it
        /// </summary>
        /// <param name="position">The position to add the frame. Leave -1 to add to the end of the frames interval</param>
        /// <returns>The newly created frame</returns>
        [NotNull]
        public IFrameId CreateFrame<T>(int position = -1) where T : IFrame, new()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            var frame = new T();

            frame.Initialize(_animation, Width, Height);

            if (FrameIdGenerator != null)
                frame.ID = FrameIdGenerator.GetNextUniqueFrameId();

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
        /// Adds the given frame to the Animation. If the Frame's dimensions don't match with the animation's,
        /// an ArgumentException is thrown.
        /// The method does nothing if the frame is already in this animation
        /// </summary>
        /// <param name="frame">The frame to add to this animation</param>
        /// <param name="index">The index to add the frame at. -1 adds the frame to the end of the frame list</param>
        [NotNull]
        public IFrameId AddFrame([NotNull] IFrame frame, int index = -1)
        {
            return InternalAddFrame(frame, index);
        }

        /// <summary>
        /// Adds the given array of frames to this Animation.
        /// If one of the frames' dimensions don't match with this animation's, an ArgumentException is thrown
        /// </summary>
        /// <param name="frames">The list of frames to add to this animation</param>
        /// <param name="index">The index to add the frames at</param>
        [NotNull]
        public IFrameId[] AddFrames([NotNull] IEnumerable<IFrame> frames, int index = -1)
        {
            // TODO: Fix encapsulation in this part
            return frames.Select(frame => AddFrame(frame, index == -1 ? index : index++)).ToArray();
        }

        /// <summary>
        /// Adds the given array of frames to this Animation.
        /// The frame dimensions are matched based on the provided settings.
        /// </summary>
        /// <param name="frames">The list of frames to add to this animation</param>
        /// <param name="sizeMatchingSettings">The settings to apply to the frames when resizing</param>
        /// <param name="index">The index to add the frames at</param>
        [NotNull]
        public IFrameId[] AddFrames([NotNull] IEnumerable<IFrame> frames, FrameSizeMatchingSettings sizeMatchingSettings, int index = -1)
        {
            // TODO: Fix encapsulation in this part
            // Fetch the largest size for the animation now
            int newAnimWidth = Width;
            int newAnimHeight = Height;

            var framesCasted = frames as IFrame[] ?? frames.ToArray();

            if (sizeMatchingSettings.AnimationDimensionMatchMethod == AnimationDimensionMatchMethod.UseLargestSize ||
                sizeMatchingSettings.AnimationDimensionMatchMethod == AnimationDimensionMatchMethod.UseNewSize)
            {
                if (sizeMatchingSettings.AnimationDimensionMatchMethod == AnimationDimensionMatchMethod.UseNewSize)
                {
                    newAnimWidth = 1;
                    newAnimHeight = 1;
                }

                foreach (var frame in framesCasted)
                {
                    newAnimWidth = Math.Max(newAnimWidth, frame.Width);
                    newAnimHeight = Math.Max(newAnimHeight, frame.Height);
                }
            }

            _animation.Width = newAnimWidth;
            _animation.Height = newAnimHeight;

            var frameIndexes = new IFrameId[framesCasted.Length];

            // Redimension each frame now
            for (var i = 0; i < framesCasted.Length; i++)
            {
                var frame = framesCasted[i];
                frameIndexes[i] = InternalAddFrame(frame, index == -1 ? index : index++, true);
            }

            // Resize the frames now
            foreach (var frame in _animation.Frames)
            {
                frame.Resize(newAnimWidth, newAnimHeight, sizeMatchingSettings.PerFrameScalingMethod, sizeMatchingSettings.InterpolationMode);
            }

            return frameIndexes;
        }

        /// <summary>
        /// Internal method that adds the given frame to this Animation. The method has an optional flag to specify
        /// that the dimensions should not be tested, and if set to true no exceptions will be thrown if the frame's
        /// dimensions don't match this animation's.
        /// The method does nothing if the frame is already in this animation
        /// </summary>
        /// <param name="frame">The frame to add to this animation</param>
        /// <param name="index">The index to add the frame at. -1 adds the frame to the end of the frame list</param>
        /// <param name="ignoreSize">Whether to ignore the size of the frame and add it even if it is in different dimensions than the rest of the animation</param>
        // ReSharper disable once UnusedParameter.Local
        private IFrameId InternalAddFrame([NotNull] IFrame frame, int index, bool ignoreSize = false)
        {
            if (!frame.Initialized)
            {
                throw new ArgumentException("The frame provided has not been initialized");
            }

            if (_animation.Frames.ContainsReference(frame))
            {
                return GetIdForFrame(frame);
            }

            if (!ignoreSize && (frame.Width != Width || frame.Height != Height))
            {
                throw new ArgumentException("Frame and animation sizes do not match", nameof(frame));
            }

            frame.Added(_animation);

            if (frame.ID == -1 && FrameIdGenerator != null)
                frame.ID = FrameIdGenerator.GetNextUniqueFrameId();

            if (index == -1)
                _animation.Frames.Add(frame);
            else
                _animation.Frames.Insert(index, frame);

            return GetIdForFrame(frame);
        }

        /// <summary>
        /// Replaces the frame image of a given frame id
        /// </summary>
        public void ReplaceFrameImage([NotNull] IFrameId frameId, [NotNull] Bitmap frameImage, FrameSizeMatchingSettings sizeMatching)
        {
            var index = GetFrameIndex(frameId);

            var newFrame = new Frame(null, frameImage.Width, frameImage.Height);
            newFrame.SetFrameBitmap(frameImage);
            newFrame.ID = _animation.Frames[index].ID;

            AddFrames(new[] { newFrame }, sizeMatching, index);
            RemoveFrameIndex(index + 1);
        }

        /// <summary>
        /// Removes the given Frame from this Animation object
        /// </summary>
        /// <param name="frame">The frame to remove</param>
        public void RemoveFrame([NotNull] IFrameId frame)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            var index = GetFrameIndex(frame);
            if(index == -1)
                throw new ArgumentException($@"No frame with id ${frame} found", nameof(frame));

            RemoveFrameIndex(index);
        }

        /// <summary>
        /// Removes a Frame at the given index from this Animation object
        /// </summary>
        /// <param name="frameIndex">The index of the frame to remove</param>
        public void RemoveFrameIndex(int frameIndex)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            _animation.Frames[frameIndex].Animation = null;
            _animation.Frames.RemoveAt(frameIndex);
        }

        /// <summary>
        /// Returns whether this Animation object contains a given Frame
        /// </summary>
        /// <param name="frame">A Frame object</param>
        /// <returns>Whether the provided frame is listed in this animation</returns>
        public bool ContainsFrame([NotNull] IFrameId frame)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            return GetFrameIndex(frame) > -1;
        }

        /// <summary>
        /// Gets the index of the given frame on this animation
        /// </summary>
        /// <param name="frame">The frame on this animation to get the index of</param>
        /// <returns>An integer representing the index at which the frame resides, or -1 if the frame is not located inside this animation</returns>
        public int GetFrameIndex([NotNull] IFrameId frame)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            // Try by-reference first
            var iframe = GetFrameByFrameId(frame);
            if (iframe != null)
                return _animation.Frames.IndexOfReference(iframe);

            return _animation.Frames.FindIndex(f => f.ID == frame.Id);
        }
        
        /// <summary>
        /// Gets a frame controller for controlling a specified frame id in this animation
        /// </summary>
        [NotNull]
        public FrameController GetFrameController([NotNull] IFrameId frame)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AnimationController));

            var f = GetFrameByFrameId(frame);
            if(f == null)
                throw new ArgumentException($@"Cannot find frame with ID {frame}", nameof(frame));

            return new FrameController((Frame)f);
        }

        [CanBeNull]
        private IFrame GetFrameByFrameId([NotNull] IFrameId frame)
        {
            var frameId = (FrameId)frame;

            // If frameid instance has a reference, check by-reference first
            if (frameId.OriginalFrame != null && _animation.ContainsFrame(frameId.OriginalFrame))
                return frameId.OriginalFrame;

            return _animation.GetFrameByID(frameId.Id);
        }

        [NotNull]
        private static IFrameId GetIdForFrame([NotNull] IFrame frame)
        {
            return new FrameId(frame.ID, frame);
        }

        private struct FrameId : IFrameId
        {
            public IFrame OriginalFrame { get; }
            public int Id { get; }

            public FrameId(int id, IFrame originalFrame)
            {
                Id = id;
                OriginalFrame = originalFrame;
            }

            public override string ToString()
            {
                return $"{Id}";
            }
        }
    }
}
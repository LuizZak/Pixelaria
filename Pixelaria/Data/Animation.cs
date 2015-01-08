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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using Pixelaria.Localization;
using Pixelaria.Utils;

namespace Pixelaria.Data
{
    /// <summary>
    /// Describes an Animation in the program
    /// </summary>
    public class Animation : IDisposable, IDObject
    {
        /// <summary>
        /// The frames of this animation
        /// </summary>
        private List<IFrame> _frames;

        /// <summary>
        /// Gets or sets the name of this animation
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Gets the width of this animation's frames
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Gets the height of this animation's frames
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Gets the size of this animation's frames
        /// </summary>
        public Size Size { get { return new Size(Width, Height); } }

        /// <summary>
        /// Gets or sets the ID for this Animation
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets the number of frames of this Animaion
        /// </summary>
        public int FrameCount { get { return _frames.Count; } }

        /// <summary>
        /// The playbar settings for this Animation
        /// </summary>
        public AnimationPlaybackSettings PlaybackSettings;

        /// <summary>
        /// The export settings of this animation
        /// </summary>
        public AnimationExportSettings ExportSettings;

        /// <summary>
        /// Gets the list of frames for this Animation
        /// </summary>
        public IFrame[] Frames { get { return _frames.ToArray(); } }

        /// <summary>
        /// Gets a frame at the given index in this animation
        /// </summary>
        /// <param name="index">The index of a frame to get. It must be between [0 - FrameCount[</param>
        /// <returns>The frame at the given index in this animation</returns>
        public IFrame this[int index] { get { return GetFrameAtIndex(index); } }

        /// <summary>
        /// Gets or sets the bundle this animation is contained within
        /// </summary>
        public Bundle OwnerBundle { get; set; }

        /// <summary>
        /// Creates a new Animation with 0 frames
        /// </summary>
        /// <param name="name">The name of this animation</param>
        /// <param name="width">The starting width of this Animation</param>
        /// <param name="height">The starting height of this Animation</param>
        public Animation(String name, int width, int height)
        {
            ID = -1;
            _frames = new List<IFrame>();

            Name = name;
            Width = width;
            Height = height;
            
            // Create the default settings
            PlaybackSettings = new AnimationPlaybackSettings() { FPS = 30, FrameSkip = false };
            ExportSettings = new AnimationExportSettings() { FavorRatioOverArea = false, ForceMinimumDimensions = true, ForcePowerOfTwoDimensions = false, ReuseIdenticalFramesArea = true, UsePaddingOnXml = true, AllowUnorderedFrames = true, XPadding = 0, YPadding = 0 };
        }

        /// <summary>
        /// Disposes of this Animation and all owning frames
        /// </summary>
        public void Dispose()
        {
            if (_frames != null)
            {
                // Frames clearing
                foreach (IFrame frame in _frames)
                {
                    frame.Dispose();
                }
                _frames.Clear();
            }

            _frames = null;
        }

        /// <summary>
        /// Returns an Animation object that is the exact copy of this Animation.
        /// The method copies also the underlying frames, allocating more memory to store the copied frames.
        /// The ID of the animation is not copied for consistency and identity purposes
        /// </summary>
        /// <returns>an Animation object that is the exact copy of this Animation</returns>
        public Animation Clone()
        {
            Animation anim = new Animation(Name, Width, Height);

            anim.CopyFrom(this, false);

            return anim;
        }

        /// <summary>
        /// Clears the frames of this Animation.
        /// The method disposes of the cleared frames
        /// </summary>
        public void Clear()
        {
            foreach (IFrame frame in _frames)
            {
                frame.Removed();
                frame.Dispose();
            }

            _frames.Clear();
        }

        /// <summary>
        /// Copies the data from the given Animation object into this animation.
        /// This method copies the width, height, name, and frames to this Animation.
        /// Enabling deep copies allow the cloning of frames as well instead of only
        /// references
        /// </summary>
        /// <param name="anim">The Animation object to copy the properties from</param>
        /// <param name="moveFrames">Whether to remove the frames form the given animation and add them to this Animation. When set to false, instead of moving the frames, the methods clones the frames</param>
        public void CopyFrom(Animation anim, bool moveFrames)
        {
            Name = anim.Name;
            Width = anim.Width;
            Height = anim.Height;
            PlaybackSettings = anim.PlaybackSettings;
            ExportSettings = anim.ExportSettings;

            Clear();

            // Copy frames now
            foreach (IFrame frame in anim.Frames)
            {
                if (moveFrames)
                {
                    anim.RemoveFrame(frame);
                    AddFrame(frame);
                }
                else
                {
                    IFrame cloneFrame = frame.Clone();

                    if (OwnerBundle != null)
                        cloneFrame.ID = OwnerBundle.GetNextValidFrameID();

                    AddFrame(cloneFrame);
                }
            }
        }

        /// <summary>
        /// Returns the memory usage of this animation, in bytes
        /// </summary>
        /// <param name="composed">Whether to calculate the memory usage of the frames after they have been composed into single images</param>
        /// <returns>Total memory usage, in bytes</returns>
        public long CalculateMemoryUsageInBytes(bool composed)
        {
            return _frames.Sum(frame => frame.CalculateMemoryUsageInBytes(composed));
        }

        /// <summary>
        /// Resizes this Animation object to fit the given dimensions
        /// </summary>
        /// <param name="sizeMatchingSettings">The settings to apply to the frames when resizing</param>
        public void Resize(AnimationResizeSettings sizeMatchingSettings)
        {
            Width = sizeMatchingSettings.NewWidth;
            Height = sizeMatchingSettings.NewHeight;

            // Resize the frames now
            foreach (IFrame frame in _frames)
            {
                frame.Resize(sizeMatchingSettings.NewWidth, sizeMatchingSettings.NewHeight, sizeMatchingSettings.PerFrameScalingMethod, sizeMatchingSettings.InterpolationMode);
            }
        }

        /// <summary>
        /// Adds the given array of frames to this Animation.
        /// If one of the frames' dimensions don't match with this animation's, an ArgumentException is thrown
        /// </summary>
        /// <param name="frames">The list of frames to add to this animation</param>
        /// <param name="index">The index to add the frames at</param>
        public void AddFrames(IEnumerable<IFrame> frames, int index = -1)
        {
            foreach (IFrame frame in frames)
            {
                AddFrame(frame, (index == -1 ? index : index++));
            }
        }

        /// <summary>
        /// Adds the given array of frames to this Animation.
        /// The frame dimensions are matched based on the provided settings.
        /// </summary>
        /// <param name="frames">The list of frames to add to this animation</param>
        /// <param name="sizeMatchingSettings">The settings to apply to the frames when resizing</param>
        /// <param name="index">The index to add the frames at</param>
        public void AddFrames(IEnumerable<IFrame> frames, FrameSizeMatchingSettings sizeMatchingSettings, int index = -1)
        {
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

                foreach (IFrame frame in framesCasted)
                {
                    newAnimWidth = Math.Max(newAnimWidth, frame.Width);
                    newAnimHeight = Math.Max(newAnimHeight, frame.Height);
                }
            }

            Width = newAnimWidth;
            Height = newAnimHeight;

            // Redimension each frame now
            foreach (IFrame frame in framesCasted)
            {
                InternalAddFrame(frame, (index == -1 ? index : index++), true);
            }

            // Resize the frames now
            foreach (IFrame frame in _frames)
            {
                frame.Resize(newAnimWidth, newAnimHeight, sizeMatchingSettings.PerFrameScalingMethod, sizeMatchingSettings.InterpolationMode);
            }
        }

        /// <summary>
        /// Adds the given frame to this Animation. If the Frame's dimensions don't match with the animation's,
        /// an ArgumentException is thrown.
        /// The method does nothing if the frame is already in this animation
        /// </summary>
        /// <param name="frame">The frame to add to this animation</param>
        /// <param name="index">The index to add the frame at. -1 adds the frame to the end of the frame list</param>
        public void AddFrame(IFrame frame, int index = -1)
        {
            InternalAddFrame(frame, index);
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
        private void InternalAddFrame(IFrame frame, int index, bool ignoreSize = false)
        {
            if (!frame.Initialized)
            {
                throw new ArgumentException("The frame provided has not been intialized");
            }

            if (_frames.ContainsReference(frame))
            {
                return;
            }

            if (!ignoreSize && (frame.Width != Width || frame.Height != Height))
            {
                throw new ArgumentException(AnimationMessages.Exception_UnmatchedFramedimensions, "frame");
            }

            frame.Added(this);

            if (frame.ID == -1 && OwnerBundle != null)
                frame.ID = OwnerBundle.GetNextValidFrameID();

            if (index == -1)
            {
                _frames.Add(frame);
            }
            else
            {
                _frames.Insert(index, frame);
            }
        }
        
        /// <summary>
        /// Switches the frame at an index with the given one. This method returns the old frame that was
        /// at that index. If the Frame's dimensions don't match with the animation's,
        /// an ArgumentException is thrown
        /// </summary>
        /// <param name="frame">The frame to set</param>
        /// <param name="index">The index of the frame to set</param>
        /// <returns>The frame that was at that index</returns>
        public IFrame SetFrame(Frame frame, int index)
        {
            if (!frame.Initialized)
            {
                throw new ArgumentException("The frame provided has not been intialized");
            }

            if ((frame.Width != Width || frame.Height != Height))
            {
                throw new ArgumentException(AnimationMessages.Exception_UnmatchedFramedimensions, "frame");
            }

            if (ReferenceEquals(_frames[index], frame))
                return frame;

            IFrame oldFrame = _frames[index];

            _frames[index] = frame;

            frame.Added(this);

            oldFrame.Removed();

            return oldFrame;
        }

        /// <summary>
        /// Swaps the two given frame indices 
        /// </summary>
        /// <param name="index1">The first index to swap</param>
        /// <param name="index2">The second index to swap</param>
        public void SwapFrameIndices(int index1, int index2)
        {
            IFrame temp = _frames[index1];
            _frames[index1] = _frames[index2];
            _frames[index2] = temp;
        }

        /// <summary>
        /// Duplicates a frame from the given index and inserts it at the new index
        /// </summary>
        /// <param name="frameIndex">The index of the frame to duplicate</param>
        /// <param name="newIndex">The index to put the frame on. Leave -1 to put it at the front of the duplicated frame</param>
        /// <returns>The newly duplicated frame</returns>
        public IFrame DuplicateFrame(int frameIndex, int newIndex = -1)
        {
            IFrame dup = _frames[frameIndex].Clone();

            if (newIndex == -1)
            {
                _frames.Insert(frameIndex + 1, dup);
            }
            else
            {
                _frames.Insert(newIndex, dup);
            }

            return dup;
        }

        /// <summary>
        /// Creates a new Frame, adds it to the given position and returns it
        /// </summary>
        /// <param name="position">The position to add the frame. Leave -1 to add to the end of the frames interval</param>
        /// <returns>The newly created frame</returns>
        public Frame CreateFrame(int position = -1)
        {
            return CreateFrame<Frame>(position);
        }

        /// <summary>
        /// Creates a new Frame, adds it to the given position and returns it
        /// </summary>
        /// <param name="position">The position to add the frame. Leave -1 to add to the end of the frames interval</param>
        /// <returns>The newly created frame</returns>
        public T CreateFrame<T>(int position = -1) where T : IFrame, new()
        {
            T frame = new T();

            frame.Initialize(this, Width, Height);

            if (OwnerBundle != null)
            {
                frame.ID = OwnerBundle.GetNextValidFrameID();
            }

            if (position == -1)
            {
                _frames.Add(frame);
            }
            else
            {
                _frames.Insert(position, frame);
            }

            return frame;
        }

        /// <summary>
        /// Removes the given Frame from this Animation object
        /// </summary>
        /// <param name="frame">The frame to remove</param>
        public void RemoveFrame(IFrame frame)
        {
            if (!ReferenceEquals(frame.Animation, this))
            {
                throw new ArgumentException(AnimationMessages.Exception_UnlistedFrameRemoval, "frame");
            }

            frame.Removed();
            _frames.RemoveReference(frame);
        }

        /// <summary>
        /// Removes a Frame at the given index from this Animation object
        /// </summary>
        /// <param name="frameIndex">The index of the frame to remove</param>
        public void RemoveFrameIndex(int frameIndex)
        {
            RemoveFrame(GetFrameAtIndex(frameIndex));
        }

        /// <summary>
        /// Returns whether this Animation object contains a given Frame
        /// </summary>
        /// <param name="frame">A Frame object</param>
        /// <returns>Whether the provided frame is listed in this animation</returns>
        public bool ContainsFrame(IFrame frame)
        {
            return GetFrameIndex(frame) > -1;
        }

        /// <summary>
        /// Gets the index of the given frame on this animation
        /// </summary>
        /// <param name="frame">The frame on this animation to get the index of</param>
        /// <returns>An integer representing the index at which the frame resides, or -1 if the frame is not located inside this animation</returns>
        public int GetFrameIndex(IFrame frame)
        {
            return _frames.IndexOfReference(frame);
        }

        /// <summary>
        /// Gets the frame at the given index
        /// </summary>
        /// <param name="index">The index of the frame to get. It must be between [0 - FrameCount[</param>
        /// <returns>The Frame at the given index</returns>
        public IFrame GetFrameAtIndex(int index)
        {
            return _frames[index];
        }

        /// <summary>
        /// Gets the frame with the given ID
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>A frame with the given ID, or null if none was found</returns>
        // ReSharper disable once InconsistentNaming
        public IFrame GetFrameByID(int id)
        {
            return _frames.FirstOrDefault(frame => frame.ID == id);
        }

        // Override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
                return true;

            Animation other = (Animation) obj;

            if (Name != other.Name || _frames == null || other._frames == null || _frames.Count != other._frames.Count || Width != other.Width || Height != other.Height ||
                !ExportSettings.Equals(other.ExportSettings) || !PlaybackSettings.Equals(other.PlaybackSettings))
                return false;

            // Check frame-by-frame for an innequality
            // Disable LINQ suggestion because it'd actually be considerably slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < _frames.Count; i++)
            {
                if (!_frames[i].Equals(other._frames[i]))
                    return false;
            }

            return true;
        }

        // Override object.GetHashCode
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Width ^ Height;
        }
    }

    /// <summary>
    /// Settings for this animation
    /// </summary>
    public struct AnimationPlaybackSettings
    {
        /// <summary>
        /// The playback FPS for the Animation. A value of -1 specifies playing as fast as possible. A value
        /// of 0 specifies no playback.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int FPS;

        /// <summary>
        /// Whether to allow frame skip
        /// </summary>
        public bool FrameSkip;
    }

    /// <summary>
    /// Encapsulates export settings of an animation
    /// </summary>
    public struct AnimationExportSettings
    {
        /// <summary>
        /// Whether to favor image ratio over image area when composing the final exported atlas.
        /// Favoring ratio will output more square-ish images that may not be optimally spaced, while
        /// favoring area will result in the smallest possible images the algorithm can output, but
        /// may result in alongated images along one of the axis.
        /// </summary>
        public bool FavorRatioOverArea;

        /// <summary>
        /// Whether to force the final dimensions of the exported sheet
        /// to be a power of 2. If the final image is not a power of 2
        /// in either dimension, it is filled to be so.
        /// </summary>
        public bool ForcePowerOfTwoDimensions;

        /// <summary>
        /// Force the frames to be fit in the minimum possible area.
        /// Doing so will fit frames in a possibly smaller rectangle than they were
        /// originally fit on, but minimizes the final sheet size
        /// </summary>
        public bool ForceMinimumDimensions;

        /// <summary>
        /// Whether to reuse identical frame images in the sheet.
        /// Setting to true will pack pixel-level identical frames to use
        /// the same position in the sprite sheet.
        /// </summary>
        public bool ReuseIdenticalFramesArea;

        /// <summary>
        /// Whether to use high precision when calculating the minimum possible area
        /// of the exported sheet. Using high precision may yield slow results, specially
        /// with favor ratio over area disabled
        /// </summary>
        public bool HighPrecisionAreaMatching;

        /// <summary>
        /// Whether to allow unordering of frames in the sheet in exchange of a better
        /// bin-packing algorithm output
        /// </summary>
        public bool AllowUnorderedFrames;

        /// <summary>
        /// Whether to place the frames in a uniform grid that is sized according to the smallest
        /// dimensions capable of fitting all the frames. Setting this option overrides the ForceMinimumDimensions flag
        /// </summary>
        public bool UseUniformGrid;

        /// <summary>
        /// Whether to pad the frame's sheet coordinates using the X and Y padding of this sprite.
        /// Use this to pad the frame's sheet coordinates and size and avoid the clamped edges effect
        /// when rendering frames using non-point clamp sampler modes
        /// </summary>
        public bool UsePaddingOnXml;

        /// <summary>
        /// Whether to generate accompaning .xml files for the animations on the sheet
        /// </summary>
        public bool ExportXml;

        /// <summary>
        /// Ammount of empty pixels to pad horizontally between frames
        /// </summary>
        public int XPadding;

        /// <summary>
        /// Ammount of empty pixels to pad vertically between frames
        /// </summary>
        public int YPadding;
    }

    /// <summary>
    /// Encapsulates frame size matching settings to apply when adding frames of different dimensions
    /// in an animation
    /// </summary>
    public struct FrameSizeMatchingSettings
    {
        /// <summary>
        /// The method of animation redimensioning to apply when adding frames of different
        /// dimensions to an Animation
        /// </summary>
        public AnimationDimensionMatchMethod AnimationDimensionMatchMethod;

        /// <summary>
        /// The method of frame scaling to apply to individual frames when adding frames of different dimensions
        /// to an Animation
        /// </summary>
        public PerFrameScalingMethod PerFrameScalingMethod;

        /// <summary>
        /// The interpolation mode to use when rendering the frames
        /// </summary>
        public InterpolationMode InterpolationMode;
    }

    /// <summary>
    /// Encapsulates frame sizing matching settings to apply when resizing an animation
    /// </summary>
    public struct AnimationResizeSettings
    {
        /// <summary>
        /// The new Height of the animation
        /// </summary>
        public int NewHeight;

        /// <summary>
        /// The new Width of the animation
        /// </summary>
        public int NewWidth;

        /// <summary>
        /// The method of frame scaling to apply to individual frames when adding frames of different dimensions
        /// to an Animation
        /// </summary>
        public PerFrameScalingMethod PerFrameScalingMethod;

        /// <summary>
        /// The interpolation mode to use when rendering the frames
        /// </summary>
        public InterpolationMode InterpolationMode;
    }

    /// <summary>
    /// The method of animation redimensioning to apply when adding frames of different
    /// dimensions to an Animation
    /// </summary>
    public enum AnimationDimensionMatchMethod
    {
        /// <summary>
        /// Specifies that the dimensions of the animation should be left unchanged
        /// </summary>
        KeepOriginal,
        /// <summary>
        /// Specifies that the animation's new size should be the largest size between the frames
        /// </summary>
        UseLargestSize,
        /// <summary>
        /// Specifies that the animation's new size should be the the new frame's size. If multiple frames are fed at once,
        /// the largest frame dimensions are used
        /// </summary>
        UseNewSize
    }

    /// <summary>
    /// The method of frame scaling to apply to individual frames when adding frames of different dimensions
    /// to an Animation
    /// </summary>
    public enum PerFrameScalingMethod
    {
        /// <summary>
        /// Specifies that the frames should be placed at the top-left position of the new frame bounds
        /// </summary>
        PlaceAtTopLeft,
        /// <summary>
        /// Specifies that the frames should be centered around the middle of the new frame size
        /// </summary>
        PlaceAtCenter,
        /// <summary>
        /// Specifies that the frames should be stretched to fit the new bounds
        /// </summary>
        Stretch,
        /// <summary>
        /// Specifies that the frames should be zoomed to fit the new bounds
        /// </summary>
        Zoom
    }
}
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
using JetBrains.Annotations;
using Pixelaria.Data.Factories;
using Pixelaria.Utils;

namespace Pixelaria.Data
{
    /// <summary>
    /// Describes an Animation in the program
    /// </summary>
    public class Animation : IDObject, IAnimation
    {
        /// <summary>
        /// Gets or sets the name of this animation
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the width of this animation's frames
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Gets the height of this animation's frames
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets the size of this animation's frames
        /// </summary>
        public Size Size => new Size(Width, Height);

        /// <summary>
        /// Gets or sets the ID for this Animation
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Id of animation sheet this animation is associated with.
        /// 
        /// Is null, if not associated with any animation sheet currently.
        /// </summary>
        public int? AnimationSheetId { get; set; }

        /// <summary>
        /// Gets the number of frames of this Animaion
        /// </summary>
        public int FrameCount => Frames.Count;

        /// <summary>
        /// The playbar settings for this Animation
        /// </summary>
        public AnimationPlaybackSettings PlaybackSettings { get; set; }

        /// <summary>
        /// The export settings of this animation
        /// </summary>
        public AnimationExportSettings ExportSettings { get; set; }

        /// <summary>
        /// Gets the list of frames for this Animation
        /// </summary>
        public List<IFrame> Frames { get; set; }

        /// <summary>
        /// Gets a frame at the given index in this animation
        /// </summary>
        /// <param name="index">The index of a frame to get. It must be between [0 - FrameCount[</param>
        /// <returns>The frame at the given index in this animation</returns>
        [NotNull]
        public IFrame this[int index] => GetFrameAtIndex(index);
        
        /// <summary>
        /// Gets or sets a frame ID generator for generating unique frame IDs when needed.
        /// In case no frame ID generator is provided, all frames created by this animation's methods will have an id of -1
        /// </summary>
        public IFrameIdGenerator FrameIdGenerator { get; set; }

        /// <summary>
        /// Creates a new Animation with 0 frames
        /// </summary>
        /// <param name="name">The name of this animation</param>
        /// <param name="width">The starting width of this Animation</param>
        /// <param name="height">The starting height of this Animation</param>
        public Animation(string name, int width, int height)
        {
            ID = -1;
            Frames = new List<IFrame>();

            Name = name;
            Width = width;
            Height = height;
            
            // Create the default settings
            PlaybackSettings = new AnimationPlaybackSettings() { FPS = 30, FrameSkip = false };
            ExportSettings = new AnimationExportSettings() { FavorRatioOverArea = false, ForceMinimumDimensions = true, ForcePowerOfTwoDimensions = false, ReuseIdenticalFramesArea = true, UsePaddingOnJson = true, AllowUnorderedFrames = true, XPadding = 0, YPadding = 0 };
        }

        ~Animation()
        {
            Dispose();
        }
        
        /// <summary>
        /// Disposes of this Animation and all owning frames
        /// </summary>
        public void Dispose()
        {
            if (Frames != null)
            {
                // Frames clearing
                foreach (var frame in Frames)
                {
                    frame.Dispose();
                }
                Frames.Clear();
            }

            Frames = null;

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clears the frames of this Animation.
        /// The method disposes of the cleared frames
        /// </summary>
        public void Clear()
        {
            foreach (var frame in Frames)
            {
                frame.Removed();
                frame.Dispose();
            }

            Frames.Clear();
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
            foreach (var frame in Frames)
            {
                frame.Resize(sizeMatchingSettings.NewWidth, sizeMatchingSettings.NewHeight, sizeMatchingSettings.PerFrameScalingMethod, sizeMatchingSettings.InterpolationMode);
            }
        }
        
        /// <summary>
        /// Returns whether this Animation object contains a given Frame
        /// </summary>
        /// <param name="frame">A Frame object</param>
        /// <returns>Whether the provided frame is listed in this animation</returns>
        public bool ContainsFrame([NotNull] IFrame frame)
        {
            return GetFrameIndex(frame) > -1;
        }

        /// <summary>
        /// Gets the index of the given frame on this animation
        /// </summary>
        /// <param name="frame">The frame on this animation to get the index of</param>
        /// <returns>An integer representing the index at which the frame resides, or -1 if the frame is not located inside this animation</returns>
        public int GetFrameIndex([NotNull] IFrame frame)
        {
            return Frames.IndexOfReference(frame);
        }

        /// <summary>
        /// Gets the frame at the given index
        /// </summary>
        /// <param name="index">The index of the frame to get. It must be between [0 - FrameCount[</param>
        /// <returns>The Frame at the given index</returns>
        [NotNull]
        public IFrame GetFrameAtIndex(int index)
        {
            return Frames[index];
        }

        /// <summary>
        /// Gets the frame with the given ID
        /// </summary>
        /// <param name="id">The ID to search for</param>
        /// <returns>A frame with the given ID, or null if none was found</returns>
        [CanBeNull]
        // ReSharper disable once InconsistentNaming
        public IFrame GetFrameByID(int id)
        {
            return Frames.FirstOrDefault(frame => frame.ID == id);
        }

        /// <summary>
        /// Gets the composed bitmap for a specified frame index
        /// </summary>
        /// <param name="frameIndex">The index of the frame to get the composed bitmap of</param>
        /// <returns>The composed bitmap for the frame at the specified index on this animation</returns>
        public Bitmap GetComposedBitmapForFrame(int frameIndex)
        {
            return GetFrameAtIndex(frameIndex).GetComposedBitmap();
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

            var other = (Animation) obj;

            if (Name != other.Name || Frames == null || other.Frames == null || Frames.Count != other.Frames.Count || Width != other.Width || Height != other.Height ||
                !ExportSettings.Equals(other.ExportSettings) || !PlaybackSettings.Equals(other.PlaybackSettings) || ID != other.ID)
                return false;

            // Check frame-by-frame for an innequality
            // Disable LINQ suggestion because it'd actually be considerably slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < Frames.Count; i++)
            {
                if (!Frames[i].Equals(other.Frames[i]))
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

        private sealed class AnimationPlaybackSettingsEqualityComparer : IEqualityComparer<AnimationPlaybackSettings>
        {
            public bool Equals(AnimationPlaybackSettings x, AnimationPlaybackSettings y)
            {
                return x.FPS == y.FPS && x.FrameSkip == y.FrameSkip;
            }

            public int GetHashCode(AnimationPlaybackSettings obj)
            {
                unchecked
                {
                    return (obj.FPS * 397) ^ obj.FrameSkip.GetHashCode();
                }
            }
        }

        public static IEqualityComparer<AnimationPlaybackSettings> AnimationPlaybackSettingsComparer { get; } = new AnimationPlaybackSettingsEqualityComparer();
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
        /// The X offset to apply to the image
        /// </summary>
        public int OffsetX;

        /// <summary>
        /// The Y offset to apply to the image
        /// </summary>
        public int OffsetY;

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
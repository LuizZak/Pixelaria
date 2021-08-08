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

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using PixLib.Controllers.DataControllers;
using PixLib.Utils;

namespace PixLib.Data
{
    /// <summary>
    /// Describes a sheet that list animations that should be exported on the same sprite sheet file.
    /// Bundle sheets each have custom export settings
    /// </summary>
    public class AnimationSheet : IDObject, IAnimationProvider
    {
        /// <summary>
        /// Animations inside this animation sheet
        /// </summary>
        private readonly List<Animation> _animations;

        /// <summary>
        /// Gets or sets the unique identifier for this animation sheet
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name of this animation sheet. The name of the bundle will be used as the name of the exported sprite-sheet file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets an array of the animations currently in this animation sheet
        /// </summary>
        public Animation[] Animations => _animations.ToArray();

        /// <summary>
        /// Gets the number of animations in this AnimationSheet
        /// </summary>
        public int AnimationCount => _animations.Count;

        /// <summary>
        /// Gets or sets the export settings for this animation sheet
        /// </summary>
        public AnimationSheetExportSettings SheetExportSettings { get; set; }

        /// <summary>
        /// Initializes a new instance of the AnimationSheet class
        /// </summary>
        /// <param name="name">The name for this animation sheet</param>
        public AnimationSheet(string name)
        {
            ID = -1;
            Name = name;
            _animations = new List<Animation>();
        }

        /// <summary>
        /// Creates an exact copy of this AnimationSheet object, with all animations and their respective frames cloned as well
        /// </summary>
        public AnimationSheet Clone()
        {
            // TODO: Maybe create an AnimationSheetController and lift this Clone() code there?

            var sheetClone = new AnimationSheet(Name) { SheetExportSettings = SheetExportSettings };

            foreach (var animation in _animations)
            {
                var controller = new AnimationController(null, animation);
                sheetClone.AddAnimation(controller.CloneAnimation());
            }

            return sheetClone;
        }

        public IAnimation[] GetAnimations()
        {
            return _animations.OfType<IAnimation>().ToArray();
        }

        /// <summary>
        /// Adds the given animation to this animation sheet
        /// </summary>
        /// <param name="animation">The animation to add to this animation sheet</param>
        public void AddAnimation([NotNull] Animation animation)
        {
            _animations.Add(animation);
        }

        /// <summary>
        /// Inserts the given Animation object into this AnimationSheet at the given index
        /// </summary>
        /// <param name="animation">The animation to add to this AnimationSheet</param>
        /// <param name="index">The index at which to place the animation</param>
        public void InsertAnimation([NotNull]Animation animation, int index)
        {
            _animations.Insert(index, animation);
        }

        /// <summary>
        /// Removes the given animation from this animation sheet
        /// </summary>
        /// <param name="animation">The animation to remove from this animation sheet</param>
        /// <returns>Whether the animation was successfuly removed from this animation sheet</returns>
        public bool RemoveAnimation(Animation animation)
        {
            return _animations.RemoveReference(animation);
        }

        /// <summary>
        /// Removes all animations from this animation sheet
        /// </summary>
        public void ClearAnimationList()
        {
            foreach (var anim in Animations)
            {
                RemoveAnimation(anim);
            }
        }

        /// <summary>
        /// Returns whether this AnimationSheet object contains an Animation object
        /// </summary>
        /// <param name="anim">An animation object</param>
        /// <returns>Whether this AnimationSheet object contains the given Animation object</returns>
        public bool ContainsAnimation(Animation anim)
        {
            return _animations.ContainsReference(anim);
        }

        /// <summary>
        /// Returns the index of the given Animation object inside this AnimationSheet.
        /// Returns -1 if the animation is not inside this sheet
        /// </summary>
        /// <param name="anim">The animation to return the index of</param>
        /// <returns>The index of the animation in this sheet, or -1 if it's not owned by this sheet</returns>
        public int IndexOfAnimation(Animation anim)
        {
            return _animations.IndexOfReference(anim);
        }

        /// <summary>
        /// Returns the sum of the frames of all the animations in this AnimationSheet
        /// </summary>
        /// <returns>The sum of the frames of all the animations in this AnimationSheet</returns>
        public int GetFrameCount()
        {
            return _animations.Sum(anim => anim.FrameCount);
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

            AnimationSheet other = (AnimationSheet) obj;

            if (_animations == null || other._animations == null || _animations.Count != other._animations.Count || Name != other.Name || !SheetExportSettings.Equals(other.SheetExportSettings))
                return false;
            
            // Iterate through each fo the animations and check for an inequality
            // Disable LINQ suggestion because it'd actually be considerably slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < _animations.Count; i++)
            {
                if (!_animations[i].Equals(other._animations[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // Override object.GetHashCode
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ AnimationCount ^ ID;
        }
    }

    /// <summary>
    /// Encapsulates export settings of an animation
    /// </summary>
    public struct AnimationSheetExportSettings
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
        public bool UsePaddingOnJson;

        /// <summary>
        /// Whether to generate accompaning .json files for the animations on the sheet
        /// </summary>
        public bool ExportJson;

        /// <summary>
        /// Ammount of empty pixels to pad horizontally between frames
        /// </summary>
        public int XPadding;

        /// <summary>
        /// Ammount of empty pixels to pad vertically between frames
        /// </summary>
        public int YPadding;

        private sealed class AnimationExportSettingsEqualityComparer : IEqualityComparer<AnimationSheetExportSettings>
        {
            public bool Equals(AnimationSheetExportSettings x, AnimationSheetExportSettings y)
            {
                return x.FavorRatioOverArea == y.FavorRatioOverArea && x.ForcePowerOfTwoDimensions == y.ForcePowerOfTwoDimensions && x.ForceMinimumDimensions == y.ForceMinimumDimensions && x.ReuseIdenticalFramesArea == y.ReuseIdenticalFramesArea && x.HighPrecisionAreaMatching == y.HighPrecisionAreaMatching && x.AllowUnorderedFrames == y.AllowUnorderedFrames && x.UseUniformGrid == y.UseUniformGrid && x.UsePaddingOnJson == y.UsePaddingOnJson && x.ExportJson == y.ExportJson && x.XPadding == y.XPadding && x.YPadding == y.YPadding;
            }

            public int GetHashCode(AnimationSheetExportSettings obj)
            {
                unchecked
                {
                    var hashCode = obj.FavorRatioOverArea.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ForcePowerOfTwoDimensions.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ForceMinimumDimensions.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ReuseIdenticalFramesArea.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.HighPrecisionAreaMatching.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.AllowUnorderedFrames.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.UseUniformGrid.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.UsePaddingOnJson.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.ExportJson.GetHashCode();
                    hashCode = (hashCode * 397) ^ obj.XPadding;
                    hashCode = (hashCode * 397) ^ obj.YPadding;
                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<AnimationSheetExportSettings> AnimationExportSettingsComparer { get; } = new AnimationExportSettingsEqualityComparer();
    }
}
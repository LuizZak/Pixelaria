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
using Pixelaria.Utils;

namespace Pixelaria.Data
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
        public AnimationExportSettings ExportSettings { get; set; }

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
            AnimationSheet sheetClone = new AnimationSheet(Name) { ExportSettings = ExportSettings };

            foreach (var animation in _animations)
            {
                sheetClone.AddAnimation(animation.Clone());
            }

            return sheetClone;
        }

        /// <summary>
        /// Adds the given animation to this animation sheet
        /// </summary>
        /// <param name="animation">The animation to add to this animation sheet</param>
        public void AddAnimation(Animation animation)
        {
            _animations.Add(animation);
        }

        /// <summary>
        /// Inserts the given Animation object into this AnimationSheet at the given index
        /// </summary>
        /// <param name="animation">The animation to add to this AnimationSheet</param>
        /// <param name="index">The index at which to place the animation</param>
        public void InsertAnimation(Animation animation, int index)
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
            foreach (Animation anim in Animations)
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

            if (_animations == null || other._animations == null || _animations.Count != other._animations.Count || Name != other.Name || !ExportSettings.Equals(other.ExportSettings))
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
}
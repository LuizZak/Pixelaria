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

namespace Pixelaria.Data
{
    /// <summary>
    /// Describes a sheet that list animations that should be exported on the same sprite sheet file.
    /// Bundle sheets each have custom export settings
    /// </summary>
    public class AnimationSheet : IDObject
    {
        /// <summary>
        /// Animations inside this bundle
        /// </summary>
        private List<Animation> animations;

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
        public Animation[] Animations { get { return animations.ToArray(); } }

        /// <summary>
        /// Gets the number of animations in this AnimationSheet
        /// </summary>
        public int AnimationCount { get { return animations.Count; } }

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
            this.Name = name;
            animations = new List<Animation>();
        }

        /// <summary>
        /// Adds the given animation to this animation sheet
        /// </summary>
        /// <param name="animation">The animation to add to this animation sheet</param>
        public void AddAnimation(Animation animation)
        {
            animations.Add(animation);
        }

        /// <summary>
        /// Inserts the given Animation object into this AnimationSheet at the given index
        /// </summary>
        /// <param name="animation">The animation to add to this AnimationSheet</param>
        /// <param name="index">The index at which to place the animation</param>
        public void InsertAnimation(Animation animation, int index)
        {
            animations.Insert(index, animation);
        }

        /// <summary>
        /// Removes the given animation from this animation sheet
        /// </summary>
        /// <param name="animation">The animation to remove from this animation sheet</param>
        /// <returns>Whether the animation was successfuly removed from this animation sheet</returns>
        public bool RemoveAnimation(Animation animation)
        {
            return animations.Remove(animation);
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
            return animations.Contains(anim);
        }

        /// <summary>
        /// Returns the index of the given Animation object inside this AnimationSheet.
        /// Returns -1 if the animation is not inside this sheet
        /// </summary>
        /// <param name="anim">The animation to return the index of</param>
        /// <returns>The index of the animation in this sheet, or -1 if it's not owned by this sheet</returns>
        public int IndexOfAnimation(Animation anim)
        {
            return animations.IndexOf(anim);
        }

        /// <summary>
        /// Returns the sum of the frames of all the animations in this AnimationSheet
        /// </summary>
        /// <returns>The sum of the frames of all the animations in this AnimationSheet</returns>
        public int GetFrameCount()
        {
            return _animations.Sum(anim => anim.FrameCount);
        }
    }
}
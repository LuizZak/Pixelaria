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
using System.Linq;
using JetBrains.Annotations;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data.Factories;

namespace Pixelaria.Data
{
    /// <summary>
    /// A bundle is an object that holds animations and animation sheets.
    /// 
    /// Represents a complete, basic project structure that the user manipulates.
    /// </summary>
    public class Bundle : IDisposable, IFrameIdGenerator
    {
        /// <summary>
        /// List of animations on this Bundle
        /// </summary>
        List<Animation> _animations;

        /// <summary>
        /// List of animation sheets on this bundle
        /// </summary>
        List<AnimationSheet> _animationSheets;

        /// <summary>
        /// Gets the array of animations on this bundle
        /// </summary>
        public IReadOnlyList<Animation> Animations => _animations;

        /// <summary>
        /// Gets the array of animation sheets on this bundle
        /// </summary>
        public IReadOnlyList<AnimationSheet> AnimationSheets => _animationSheets;

        /// <summary>
        /// Gets or sets the name of this bundle
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the export path for the Bundle
        /// </summary>
        public string ExportPath { get; set; }

        /// <summary>
        /// Gets or sets the current file location on disk this bundle is saved on.
        /// Empty strings means the bundle has not been saved to disk yet
        /// </summary>
        public string SaveFile { get; set; }

        /// <summary>
        /// Gets the a project tree that represents the tree visualization for the project contents
        /// </summary>
        public ProjectTree BundleProjectTree { get; }

        /// <summary>
        /// Initializes a new instance of the Bundle class
        /// </summary>
        /// <param name="name">The name for this bundle</param>
        public Bundle(string name)
        {
            Name = name;
            SaveFile = "";
            ExportPath = "";

            _animations = new List<Animation>();
            _animationSheets = new List<AnimationSheet>();

            // Initialize the bundle tree
            BundleProjectTree = ProjectTree.ProjectTreeFromBundle(this);
        }

        ~Bundle()
        {
            Dispose(false);
        }

        /// <summary>
        /// Disposes of this Bundle and all animations owned by it
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            Clear();

            _animations = null;
            _animationSheets = null;
        }

        /// <summary>
        /// Clears this bundle back to an empty state
        /// </summary>
        public void Clear()
        {
            Name = "";
            ExportPath = "";
            SaveFile = "";

            // Animation clearing
            foreach (var anim in _animations)
            {
                anim.Dispose();
            }
            _animations.Clear();

            // Animation sheet clearing
            foreach (var sheet in _animationSheets)
            {
                sheet.ClearAnimationList();
            }
            _animationSheets.Clear();
        }

        /// <summary>
        /// Clones this bundle into a new bundle object that has all of this bundle's properties deep cloned
        /// </summary>
        public Bundle Clone()
        {
            // TODO: Maybe create a BundleController and lift this Clone() code there?

            var newBundle = new Bundle(Name) { ExportPath = ExportPath, SaveFile = SaveFile };

            // Copy animations over
            foreach (var animation in _animations)
            {
                var controller = new AnimationController(this, animation);

                var anim = controller.CloneAnimation();
                anim.ID = animation.ID;

                newBundle.AddAnimation(anim);

                // Maintain frame IDs
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    anim[i].ID = animation[i].ID;
                }
            }

            // Copy Animation Sheets over
            foreach (var animationSheet in _animationSheets)
            {
                var newSheet = new AnimationSheet(animationSheet.Name);
                newBundle.AddAnimationSheet(newSheet);

                foreach (var anim in animationSheet.Animations)
                {
                    newBundle.AddAnimationToAnimationSheet(newBundle.GetAnimationByID(anim.ID), newSheet);
                }
            }

            // Copy frame ID indexing for consistency
            newBundle._nextFrameId = _nextFrameId;

            return newBundle;
        }

        /// <summary>
        /// Creates a new instance of an Animation class and stores it inside this bundle
        /// </summary>
        /// <param name="name">The name of the new animation</param>
        /// <param name="width">The width of the new animation</param>
        /// <param name="height">The height of the new animation</param>
        /// <returns>The animation created</returns>
        public Animation CreateNewAnimation([NotNull] string name, int width, int height)
        {
            var anim = new Animation(name, width, height);

            _animations.Add(anim);

            return anim;
        }

        /// <summary>
        /// Adds an existing animation into this bundle
        /// </summary>
        /// <param name="anim">The animation to add to this bundle</param>
        public void AddAnimation([NotNull] Animation anim)
        {
            // Seek a new ID for this animation if one is not set yet
            if (anim.ID == -1)
                anim.ID = GetNextValidAnimationID();

            if (GetAnimationByID(anim.ID) != null)
            {
                throw new ArgumentException("Trying to add an animation that has a conflict of IDs with an animation already on this bundle.");
            }

            _animations.Add(anim);
            anim.FrameIdGenerator = this;

            // Iterate through the frames and check their ids
            foreach (var frame in anim.Frames)
            {
                if (frame.ID == -1)
                    frame.ID = GetNextUniqueFrameId();
                else
                    _nextFrameId = Math.Max(_nextFrameId, frame.ID + 1);
            }
        }

        /// <summary>
        /// Adds an existing animation into this bundle, and couples it inside
        /// the given animation sheet.
        /// The animation sheet must be a part of this bundle before this method call, or else
        /// the animation will not be added to the sheet. The method still adds the animation
        /// to this bundle even if it's not added to the animation sheet.
        /// </summary>
        /// <param name="anim">The animation to add to this bundle</param>
        /// <param name="parentAnimationSheet">The animation sheet to add the animation to</param>
        public void AddAnimation([NotNull] Animation anim, AnimationSheet parentAnimationSheet)
        {
            AddAnimation(anim);

            if (_animationSheets.Contains(parentAnimationSheet))
            {
                parentAnimationSheet.AddAnimation(anim);
            }
        }

        /// <summary>
        /// Removes the given animation from this bundle
        /// </summary>
        /// <param name="anim">The animation to remove from this bundle</param>
        public void RemoveAnimation([NotNull] Animation anim)
        {
            // Remove 
            foreach (var bundleSheet in _animationSheets)
            {
                if (bundleSheet.RemoveAnimation(anim))
                {
                    break;
                }
            }

            anim.FrameIdGenerator = null;

            _animations.Remove(anim);
        }

        /// <summary>
        /// Duplicates the given Animation on this bundle. The duplicated animation has its name sulfix
        /// changed to avoid name conflicts
        /// </summary>
        /// <param name="anim">The animation to duplicate</param>
        /// <param name="sheet">The AnimationSheet to add the duplicated animation to</param>
        /// <param name="rearrange">Whether to re-arrange the index of the animation on the container</param>
        /// <returns>The new animation that was duplicated</returns>
        public Animation DuplicateAnimation([NotNull] Animation anim, AnimationSheet sheet, bool rearrange = true)
        {
            var controller = new AnimationController(this, anim);

            var dup = controller.CloneAnimation();

            dup.ID = GetNextValidAnimationID();

            sheet = sheet ?? GetOwningAnimationSheet(anim);

            AddAnimation(dup, sheet);

            if (rearrange)
            {
                int index = GetAnimationIndex(anim);

                RearrangeAnimationsPosition(dup, index + 1);
            }

            // Find a new name for the animation
            int n = 2;

            while (true)
            {
                if (GetAnimationByName(anim.Name + "_" + n) == null)
                {
                    break;
                }

                n++;
            }

            dup.Name = anim.Name + "_" + n;

            return dup;
        }

        /// <summary>
        /// Gets the index of the given Animation object in its current parent container
        /// </summary>
        /// <param name="anim">The animation to get the index of</param>
        /// <returns>The index of the animation in its current parent container</returns>
        public int GetAnimationIndex(Animation anim)
        {
            var sheet = GetOwningAnimationSheet(anim);

            return sheet?.IndexOfAnimation(anim) ?? _animations.IndexOf(anim);
        }

        /// <summary>
        /// Rearranges the index of an animation in the animation's current storing container
        /// </summary>
        /// <param name="anim">The animation to rearrange</param>
        /// <param name="newIndex">The new index to place the animation at</param>
        public void RearrangeAnimationsPosition(Animation anim, int newIndex)
        {
            var sheet = GetOwningAnimationSheet(anim);

            if (sheet == null)
            {
                _animations.Remove(anim);
                _animations.Insert(newIndex, anim);
            }
            else
            {
                sheet.RemoveAnimation(anim);
                sheet.InsertAnimation(anim, newIndex);
            }
        }

        /// <summary>
        /// Gets an Animation object inside this that matches the given ID.
        /// If no animation matches the passed ID, null is returned
        /// </summary>
        /// <param name="id">The ID of the animation to get</param>
        /// <returns>An animation object inside this that matches the given ID. If no animation matches the passed ID, null is returned</returns>
        [CanBeNull]
        // ReSharper disable once InconsistentNaming
        public Animation GetAnimationByID(int id)
        {
            return _animations.FirstOrDefault(anim => anim.ID == id);
        }

        /// <summary>
        /// Gets an Animation object inside this that matches the given name.
        /// If no animation matches the passed name, null is returned
        /// </summary>
        /// <param name="name">The name of the animation to get</param>
        /// <returns>An animation object inside this that matches the given name. If no animation matches the passed name, null is returned</returns>
        [CanBeNull]
        public Animation GetAnimationByName(string name)
        {
            return _animations.FirstOrDefault(anim => anim.Name == name);
        }

        /// <summary>
        /// Adds the given animation sheet to this bundle
        /// </summary>
        /// <param name="sheet">The animation sheet to add to this bundle</param>
        public void AddAnimationSheet([NotNull] AnimationSheet sheet)
        {
            if(sheet.ID == -1)
                sheet.ID = GetNextValidAnimationSheetID();

            if (GetAnimationSheetByID(sheet.ID) != null)
            {
                throw new ArgumentException("Trying to add an animation sheet that has a conflict of IDs with an animation sheet already on this bundle.");
            }

            _animationSheets.Add(sheet);

            foreach (var animation in sheet.Animations)
            {
                if (!_animations.Contains(animation))
                {
                    _animations.Add(animation);
                }
            }

            foreach (var animation in sheet.Animations)
            {
                if(animation.ID == -1)
                    animation.ID = GetNextValidAnimationID();
            }
        }

        /// <summary>
        /// Removes the given animation sheet from this bundle.
        /// Removing an animation sheet decouples all animations from it automatically
        /// </summary>
        /// <param name="sheet">The animation sheet to remove</param>
        /// <param name="deleteAnimations">Whether to delete the nested animations as well. If set to false, the animations will be moved to the bundle's root</param>
        public void RemoveAnimationSheet([NotNull] AnimationSheet sheet, bool deleteAnimations)
        {
            // Remove/relocate the animations
            foreach (var anim in sheet.Animations)
            {
                sheet.RemoveAnimation(anim);

                if (deleteAnimations)
                {
                    RemoveAnimation(anim);
                }
            }

            // Decouple all animatins from the sheet
            sheet.ClearAnimationList();

            _animationSheets.Remove(sheet);
        }

        /// <summary>
        /// Duplicates the given animation sheet on this bundle. The duplicated sheet has its name sulfix
        /// changed to avoid name conflicts. The animations on the sheet are also copied over
        /// </summary>
        /// <param name="sheet">The animation sheet to duplicate</param>
        /// <returns>The new animation sheet that was duplicated</returns>
        public AnimationSheet DuplicateAnimationSheet([NotNull] AnimationSheet sheet)
        {
            // Find a new name for the animation
            int n = 2;

            while (true)
            {
                if (GetAnimationSheetByName(sheet.Name + "_" + n) == null)
                {
                    break;
                }

                n++;
            }

            string dupName = sheet.Name + "_" + n;

            // Create the duplicated animation sheet
            var dup = new AnimationSheet(dupName)
            {
                ID = GetNextValidAnimationSheetID(),
                ExportSettings = sheet.ExportSettings
            };

            AddAnimationSheet(dup);

            // Duplicate the animations
            foreach (Animation anim in sheet.Animations)
            {
                DuplicateAnimation(anim, dup, false);
            }

            return dup;
        }

        /// <summary>
        /// Gets the index of the given AnimationSheet object inside its current parent container
        /// </summary>
        /// <param name="sheet">The sheet to get the index of</param>
        /// <returns>The index of the sheet in its current parent container</returns>
        public int GetAnimationSheetIndex(AnimationSheet sheet)
        {
            return _animationSheets.IndexOf(sheet);
        }

        /// <summary>
        /// Rearranges the index of an AnimationSheets in the sheets's current storing container
        /// </summary>
        /// <param name="sheet">The sheet to rearrange</param>
        /// <param name="newIndex">The new index to place the sheet at</param>
        public void RearrangeAnimationSheetsPosition(AnimationSheet sheet, int newIndex)
        {
            _animationSheets.Remove(sheet);
            _animationSheets.Insert(newIndex, sheet);
        }

        /// <summary>
        /// Gets the AnimationSheet that currently owns the given Animation object.
        /// If the Animation is not inside any AnimationSheet, null is returned
        /// </summary>
        /// <param name="anim">The animation object to get the animation sheet of</param>
        /// <returns>The AnimationSheet that currently owns the given Animation object. If the Animation is not inside any AnimationSheet, null is returned</returns>
        [CanBeNull]
        public AnimationSheet GetOwningAnimationSheet(Animation anim)
        {
            return _animationSheets.FirstOrDefault(sheet => sheet.ContainsAnimation(anim));
        }

        /// <summary>
        /// Adds the given Animation object into the given AnimationSheet object.
        /// If null is provided as animation sheet, the animation is removed from it's current animation sheet, if it's inside one
        /// </summary>
        /// <param name="anim">The animation to add to the animation sheet</param>
        /// <param name="sheet">The AnimationSheet to add the animation to</param>
        public void AddAnimationToAnimationSheet(Animation anim, AnimationSheet sheet)
        {
            // Get the current AnimationSheet owning the given anim
            var curSheet = GetOwningAnimationSheet(anim);

            if (curSheet != null)
            {
                RemoveAnimationFromAnimationSheet(anim, curSheet);
            }

            sheet?.AddAnimation(anim);
        }

        /// <summary>
        /// Removes the given Animation object from the given AnimationSheet object
        /// </summary>
        /// <param name="anim">The animation to remove from the animation sheet</param>
        /// <param name="sheet">The AnimationSheet to remove the animation from</param>
        public void RemoveAnimationFromAnimationSheet(Animation anim, [NotNull] AnimationSheet sheet)
        {
            sheet.RemoveAnimation(anim);
        }

        /// <summary>
        /// Gets an AnimationSheet object inside this that matches the given ID.
        /// If no animation sheet matches the passed ID, null is returned
        /// </summary>
        /// <param name="id">The ID of the animation sheet to get</param>
        /// <returns>An animation sheet object inside this that matches the given ID. If no animation sheet matches the passed ID, null is returned</returns>
        [CanBeNull]
        // ReSharper disable once InconsistentNaming
        public AnimationSheet GetAnimationSheetByID(int id)
        {
            return _animationSheets.FirstOrDefault(sheet => sheet.ID == id);
        }

        /// <summary>
        /// Gets an AnimationSheet object inside this that matches the given name.
        /// If no animation sheet matches the passed name, null is returned
        /// </summary>
        /// <param name="name">The name of the animation sheet to get</param>
        /// <returns>An animation sheet object inside this that matches the given name. If no animation sheet matches the passed name, null is returned</returns>
        [CanBeNull]
        public AnimationSheet GetAnimationSheetByName(string name)
        {
            return _animationSheets.FirstOrDefault(sheet => sheet.Name == name);
        }

        /// <summary>
        /// Gets the next valid Frame ID based on the IDs of the current frames
        /// </summary>
        /// <returns>An integer that is safe to be used as an ID for a frame</returns>
        public int GetNextUniqueFrameId()
        {
            int id = _nextFrameId++;

            // If the ID equals to -1, it hasn't been cached yet
            if (id == -1)
            {
                id = (from anim in _animations
                      from frame in anim.Frames
                      select frame.ID + 1).Concat(new[] {0}).Max();

                _nextFrameId = id + 1;
            }

            return id;
        }

        /// <summary>
        /// Gets the next valid Animation ID based on the IDs of the current animations
        /// </summary>
        /// <returns>An integer that is safe to be used as an ID for an animation</returns>
        // ReSharper disable once InconsistentNaming
        public int GetNextValidAnimationID()
        {
            // Generate a list of all the animation IDs + 1, sum with a zero list, then select the largest value on the list.
            // The 0 concat is used to ensure there will be at least one value for Max() to work with
            return _animations.Select(anim => anim.ID + 1).Concat(new[] {0}).Max();
        }

        /// <summary>
        /// Gets the next valid AnimationSheet ID based on the IDs of the current animation sheets
        /// </summary>
        /// <returns>An integer that is safe to be used as an ID for an animation sheets</returns>
        // ReSharper disable once InconsistentNaming
        public int GetNextValidAnimationSheetID()
        {
            // Generate a list of all the animation sheet IDs + 1, sum with a zero list, then select the largest value on the list.
            // The 0 concat is used to ensure there will be at least one value for Max() to work with
            return _animationSheets.Select(sheet => sheet.ID + 1).Concat(new[] {0}).Max();
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

            var other = (Bundle)obj;

            if (Name != other.Name || ExportPath != other.ExportPath || SaveFile != other.SaveFile ||
                _animations == null || other._animations == null || _animationSheets == null ||
                other._animationSheets == null || _animations.Count != other._animations.Count ||
                _animationSheets.Count != other._animationSheets.Count)
                return false;

            // Test equality of animation sheets
            // Disable LINQ suggestion because it'd actually be considerably slower
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < _animationSheets.Count; i++)
            {
                if (!_animationSheets[i].Equals(other._animationSheets[i]))
                {
                    return false;
                }
            }

            // Test equality of animation
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (int i = 0; i < _animations.Count;i++)
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
            return Name.GetHashCode() ^ ExportPath.GetHashCode() ^ SaveFile.GetHashCode();
        }

        /// <summary>
        /// The next valid frame ID
        /// </summary>
        private int _nextFrameId = -1;
    }
}
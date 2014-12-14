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

namespace Pixelaria.Data
{
    /// <summary>
    /// A bundle is an object that holds animations
    /// </summary>
    public class Bundle : IDisposable
    {
        /// <summary>
        /// List of animations on this Bundle
        /// </summary>
        List<Animation> animations;

        /// <summary>
        /// List of animation sheets on this bundle
        /// </summary>
        List<AnimationSheet> animationSheets;

        /// <summary>
        /// A project tree that represents the tree visualization for the project contents
        /// </summary>
        ProjectTree bundleProjectTree;

        /// <summary>
        /// Gets the array of animations on this bundle
        /// </summary>
        public Animation[] Animations { get { return animations.ToArray(); } }

        /// <summary>
        /// Gets the array of animation sheets on this bundle
        /// </summary>
        public AnimationSheet[] AnimationSheets { get { return animationSheets.ToArray(); } }

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
        public ProjectTree BundleProjectTree { get { return this.bundleProjectTree; } }

        /// <summary>
        /// Initializes a new instance of the Bundle class
        /// </summary>
        /// <param name="name">The name for this bundle</param>
        public Bundle(string name)
        {
            this.Name = name;
            this.SaveFile = "";
            this.ExportPath = "";

            this.animations = new List<Animation>();
            this.animationSheets = new List<AnimationSheet>();

            // Initialize the bundle tree
            this.bundleProjectTree = ProjectTree.ProjectTreeFromBundle(this);
        }

        /// <summary>
        /// Disposes of this Bundle and all animations owned by it
        /// </summary>
        public void Dispose()
        {
            // Animation clearing
            foreach(Animation anim in animations)
            {
                anim.Dispose();
            }
            animations.Clear();
            animations = null;

            // Animation sheet clearing
            foreach (AnimationSheet sheet in animationSheets)
            {
                sheet.ClearAnimationList();
            }
            animationSheets.Clear();
            animationSheets = null;
        }

        /// <summary>
        /// Creates a new instance of an Animation class and stores it inside this bundle
        /// </summary>
        /// <param name="name">The name of the new animation</param>
        /// <param name="width">The width of the new animation</param>
        /// <param name="height">The height of the new animation</param>
        /// <returns>The animation created</returns>
        public Animation CreateNewAnimation(string name, int width, int height)
        {
            Animation anim = new Animation(name, width, height);

            animations.Add(anim);

            return anim;
        }

        /// <summary>
        /// Adds an existing animation into this bundle
        /// </summary>
        /// <param name="anim">The animation to add to this bundle</param>
        public void AddAnimation(Animation anim)
        {
            // Seek a new ID for this animation if one is not set yet
            if (anim.ID == -1)
                anim.ID = GetNextValidAnimationID();

            animations.Add(anim);
            anim.OwnerBundle = this;

            // Iterate through the frames and check their ids
            foreach (Frame frame in anim.Frames)
            {
                if (frame.ID == -1)
                    frame.ID = GetNextValidFrameID();
                else
                    nextFrameID = Math.Max(nextFrameID, frame.ID + 1);
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
        public void AddAnimation(Animation anim, AnimationSheet parentAnimationSheet)
        {
            AddAnimation(anim);

            if (animationSheets.Contains(parentAnimationSheet))
            {
                parentAnimationSheet.AddAnimation(anim);
            }
        }

        /// <summary>
        /// Removes the given animation from this bundle
        /// </summary>
        /// <param name="anim">The animation to remove from this bundle</param>
        public void RemoveAnimation(Animation anim)
        {
            // Remove 
            foreach (AnimationSheet bundleSheet in animationSheets)
            {
                if (bundleSheet.RemoveAnimation(anim))
                {
                    break;
                }
            }

            anim.OwnerBundle = null;

            animations.Remove(anim);
        }

        /// <summary>
        /// Duplicates the given Animation on this bundle. The duplicated animation has its name sulfix
        /// changed to avoid name conflicts
        /// </summary>
        /// <param name="anim">The animation to duplicate</param>
        /// <param name="sheet">The AnimationSheet to add the duplicated animation to</param>
        /// <param name="rearrange">Whether to re-arrange the index of the animation on the container</param>
        /// <returns>The new animation that was duplicated</returns>
        public Animation DuplicateAnimation(Animation anim, AnimationSheet sheet, bool rearrange = true)
        {
            Animation dup = anim.Clone();

            sheet = (sheet ?? GetOwningAnimationSheet(anim));

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
            AnimationSheet sheet = GetOwningAnimationSheet(anim);

            if (sheet == null)
            {
                return animations.IndexOf(anim);
            }
            else
            {
                return sheet.IndexOfAnimation(anim);
            }
        }

        /// <summary>
        /// Rearranges the index of an animation in the animation's current storing container
        /// </summary>
        /// <param name="anim">The animation to rearrange</param>
        /// <param name="newIndex">The new index to place the animation at</param>
        public void RearrangeAnimationsPosition(Animation anim, int newIndex)
        {
            AnimationSheet sheet = GetOwningAnimationSheet(anim);

            if (sheet == null)
            {
                animations.Remove(anim);
                animations.Insert(newIndex, anim);
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
        public Animation GetAnimationByID(int id)
        {
            foreach (Animation anim in animations)
            {
                if (anim.ID == id)
                    return anim;
            }

            return null;
        }

        /// <summary>
        /// Gets an Animation object inside this that matches the given name.
        /// If no animation matches the passed name, null is returned
        /// </summary>
        /// <param name="name">The name of the animation to get</param>
        /// <returns>An animation object inside this that matches the given name. If no animation matches the passed name, null is returned</returns>
        public Animation GetAnimationByName(string name)
        {
            foreach (Animation anim in animations)
            {
                if (anim.Name == name)
                    return anim;
            }

            return null;
        }

        /// <summary>
        /// Adds the given animation sheet to this bundle
        /// </summary>
        /// <param name="sheet">The animation sheet to add to this bundle</param>
        public void AddAnimationSheet(AnimationSheet sheet)
        {
            sheet.ID = GetNextValidAnimationSheetID();

            animationSheets.Add(sheet);
        }

        /// <summary>
        /// Removes the given animation sheet from this bundle.
        /// Removing an animation sheet decouples all animations from it automatically
        /// </summary>
        /// <param name="sheet">The animation sheet to remove</param>
        /// <param name="deleteAnimations">Whether to delete the nested animations as well. If set to false, the animations will be moved to the bundle's root</param>
        public void RemoveAnimationSheet(AnimationSheet sheet, bool deleteAnimations)
        {
            // Remove/relocate the animations
            foreach (Animation anim in sheet.Animations)
            {
                sheet.RemoveAnimation(anim);

                if (deleteAnimations)
                {
                    RemoveAnimation(anim);
                }
            }

            // Decouple all animatins from the sheet
            sheet.ClearAnimationList();

            animationSheets.Remove(sheet);
        }

        /// <summary>
        /// Duplicates the given animation sheet on this bundle. The duplicated sheet has its name sulfix
        /// changed to avoid name conflicts. The animations on the sheet are also copied over
        /// </summary>
        /// <param name="sheet">The animation sheet to duplicate</param>
        /// <returns>The new animation sheet that was duplicated</returns>
        public AnimationSheet DuplicateAnimationSheet(AnimationSheet sheet)
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
            AnimationSheet dup = new AnimationSheet(dupName);

            AddAnimationSheet(dup);

            dup.ExportSettings = sheet.ExportSettings;

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
            return animationSheets.IndexOf(sheet);
        }

        /// <summary>
        /// Rearranges the index of an AnimationSheets in the sheets's current storing container
        /// </summary>
        /// <param name="sheet">The sheet to rearrange</param>
        /// <param name="newIndex">The new index to place the sheet at</param>
        public void RearrangeAnimationSheetsPosition(AnimationSheet sheet, int newIndex)
        {
            animationSheets.Remove(sheet);
            animationSheets.Insert(newIndex, sheet);
        }

        /// <summary>
        /// Gets the AnimationSheet that currently owns the given Animation object.
        /// If the Animation is not inside any AnimationSheet, null is returned
        /// </summary>
        /// <param name="anim">The animation object to get the animation sheet of</param>
        /// <returns>The AnimationSheet that currently owns the given Animation object. If the Animation is not inside any AnimationSheet, null is returned</returns>
        public AnimationSheet GetOwningAnimationSheet(Animation anim)
        {
            foreach (AnimationSheet sheet in animationSheets)
            {
                if (sheet.ContainsAnimation(anim))
                    return sheet;
            }

            return null;
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
            AnimationSheet curSheet = GetOwningAnimationSheet(anim);

            if (curSheet != null)
            {
                RemoveAnimationFromAnimationSheet(anim, curSheet);
            }

            if (sheet != null)
            {
                sheet.AddAnimation(anim);
            }
        }

        /// <summary>
        /// Removes the given Animation object from the given AnimationSheet object
        /// </summary>
        /// <param name="anim">The animation to remove from the animation sheet</param>
        /// <param name="sheet">The AnimationSheet to remove the animation from</param>
        public void RemoveAnimationFromAnimationSheet(Animation anim, AnimationSheet sheet)
        {
            sheet.RemoveAnimation(anim);
        }

        /// <summary>
        /// Gets an AnimationSheet object inside this that matches the given ID.
        /// If no animation sheet matches the passed ID, null is returned
        /// </summary>
        /// <param name="id">The ID of the animation sheet to get</param>
        /// <returns>An animation sheet object inside this that matches the given ID. If no animation sheet matches the passed ID, null is returned</returns>
        public AnimationSheet GetAnimationSheetByID(int id)
        {
            foreach (AnimationSheet sheet in animationSheets)
            {
                if (sheet.ID == id)
                    return sheet;
            }

            return null;
        }

        /// <summary>
        /// Gets an AnimationSheet object inside this that matches the given name.
        /// If no animation sheet matches the passed name, null is returned
        /// </summary>
        /// <param name="name">The name of the animation sheet to get</param>
        /// <returns>An animation sheet object inside this that matches the given name. If no animation sheet matches the passed name, null is returned</returns>
        public AnimationSheet GetAnimationSheetByName(string name)
        {
            foreach (AnimationSheet sheet in animationSheets)
            {
                if (sheet.Name == name)
                    return sheet;
            }

            return null;
        }

        /// <summary>
        /// Gets the next valid Frame ID based on the IDs of the current frames
        /// </summary>
        /// <returns>An integer that is safe to be used as an ID for a frame</returns>
        public int GetNextValidFrameID()
        {
            int id = nextFrameID++;

            // If the ID equals to -1, it hasn't been cached yet
            if (id == -1)
            {
                id = 0;

                foreach (Animation anim in animations)
                {
                    foreach (Frame frame in anim.Frames)
                    {
                        id = Math.Max(id, frame.ID + 1);
                    }
                }

                nextFrameID = id + 1;
            }

            return id;
        }

        /// <summary>
        /// Gets the next valid Animation ID based on the IDs of the current animations
        /// </summary>
        /// <returns>An integer that is safe to be used as an ID for an animation</returns>
        public int GetNextValidAnimationID()
        {
            int id = 0;

            foreach (Animation anim in animations)
            {
                id = Math.Max(id, anim.ID + 1);
            }

            return id;
        }

        /// <summary>
        /// Gets the next valid AnimationSheet ID based on the IDs of the current animation sheets
        /// </summary>
        /// <returns>An integer that is safe to be used as an ID for an animation sheets</returns>
        public int GetNextValidAnimationSheetID()
        {
            int id = 0;

            foreach (AnimationSheet sheet in animationSheets)
            {
                id = Math.Max(id, sheet.ID + 1);
            }

            return id;
        }

        /// <summary>
        /// The next valid frame ID
        /// </summary>
        private int nextFrameID = -1;
    }
}
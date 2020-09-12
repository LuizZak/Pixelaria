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
using JetBrains.Annotations;
using PixelariaLib.Data;

namespace Pixelaria.Data
{
    /// <summary>
    /// Represents the structure of a Pixelaria project
    /// </summary>
    public class ProjectTree
    {
        /// <summary>
        /// The root node for this project tree
        /// </summary>
        NestedProjectTreeNode _rootNode;

        /// <summary>
        /// Gets the root node for this project tree
        /// </summary>
        public NestedProjectTreeNode RootNode => _rootNode;

        /// <summary>
        /// Initializes a new instance of the ProjectTree class
        /// </summary>
        public ProjectTree()
        {
            _rootNode = new NestedProjectTreeNode();
        }

        /// <summary>
        /// Helper method that creates a project tree from the given bundle
        /// </summary>
        /// <param name="bundle">The bundle to create the project tree from</param>
        /// <returns>A project tree that was created from the given bundle</returns>
        public static ProjectTree ProjectTreeFromBundle([NotNull] Bundle bundle)
        {
            var generatedTree = new ProjectTree();

            // Start with the Bundle tree node
            var root = new BundleProjectTreeNode(bundle);

            generatedTree._rootNode = root;

            // Fill in the animation sheets
            foreach (var sheet in bundle.AnimationSheets)
            {
                var sheetNode = new AnimationSheetProjectTreeNode(sheet);
                root.AddChild(sheetNode);

                // Now the animations contained within the sheet
                foreach (var anim in sheet.Animations)
                {
                    var animNode = new AnimationProjectTreeNode(anim);
                    sheetNode.AddChild(animNode);
                }
            }

            // Fill in the remaining animations not owned by an animation sheet now
            foreach (var anim in bundle.Animations)
            {
                if (bundle.GetOwningAnimationSheet(anim) == null)
                {
                    var animNode = new AnimationProjectTreeNode(anim);
                    root.AddChild(animNode);
                }
            }
            
            return generatedTree;
        }
    }

    /// <summary>
    /// Represents a Project Tree node
    /// </summary>
    public class ProjectTreeNode
    {
        /// <summary>
        /// The parent node for this project tree node
        /// </summary>
        protected NestedProjectTreeNode parentNode;

        /// <summary>
        /// Gets the parent node for this project tree node
        /// </summary>
        public NestedProjectTreeNode ParentNode => parentNode;

        /// <summary>
        /// Adds a project tree node as a child of another project tree node
        /// </summary>
        /// <param name="node">The project node to add as a child of another node</param>
        /// <param name="parent">The parent node to child the first node</param>
        /// <param name="operation">The operation to perform</param>
        protected void InternalSetParent([NotNull] ProjectTreeNode node, NestedProjectTreeNode parent, SetParentOperation operation)
        {
            if (operation == SetParentOperation.Add)
            {
                if (node.parentNode != parent)
                {
                    throw new ArgumentException(@"The provided node is not a child of this node", nameof(node));
                }
            }
            else
            {
                if (node.parentNode != null)
                {
                    throw new ArgumentException(@"Cannot add as a child a node that is already parented by another node", nameof(node));
                }
            }

            node.parentNode = parent;
        }

        /// <summary>
        /// Stipulates the operation to perform on a InternalSetParent call
        /// </summary>
        protected enum SetParentOperation
        {
            /// <summary>
            /// Adds a node as a child of a nested node
            /// </summary>
            Add,
            /// <summary>
            /// Removes a node as a child of a nested node
            /// </summary>
            Remove
        }
    }
    
    /// <summary>
    /// Represents a project tree node that has children nodes attached to it
    /// </summary>
    public class NestedProjectTreeNode : ProjectTreeNode
    {
        /// <summary>
        /// The children nodes for this project tree node
        /// </summary>
        protected List<ProjectTreeNode> childrenNodes;

        /// <summary>
        /// Gets the children nodes for this project tree node
        /// </summary>
        public ProjectTreeNode[] ChildrenNodes => childrenNodes.ToArray();

        /// <summary>
        /// Initializes a new instance of the ProjectTreeNode class
        /// </summary>
        public NestedProjectTreeNode()
        {
            childrenNodes = new List<ProjectTreeNode>();
        }

        /// <summary>
        /// Adds a project tree node as a child of this project tree node
        /// </summary>
        /// <param name="node">The project node to add as a child of this node</param>
        public void AddChild([NotNull] ProjectTreeNode node)
        {
            InternalSetParent(node, this, SetParentOperation.Add);

            childrenNodes.Add(node);
        }

        /// <summary>
        /// Removes a project tree node as a child of this project tree node
        /// </summary>
        /// <param name="node">The project node to remove as a child of this node</param>
        public void RemoveChild([NotNull] ProjectTreeNode node)
        {
            InternalSetParent(node, this, SetParentOperation.Remove);

            childrenNodes.Remove(node);
        }
    }

    /// <summary>
    /// Represents a Bundle project tree node
    /// </summary>
    public class BundleProjectTreeNode : NestedProjectTreeNode
    {
        /// <summary>
        /// Gets the bundle for this Bundle project tree node
        /// </summary>
        public Bundle Bundle { get; }

        /// <summary>
        /// Initializes a new instance of the BundleProjectTreeNode class
        /// </summary>
        /// <param name="bundle">The bundle to bind to this object</param>
        public BundleProjectTreeNode(Bundle bundle)
        {
            Bundle = bundle;
        }
    }

    /// <summary>
    /// Represents a nested project tree node that contains an animation sheet
    /// </summary>
    public class AnimationSheetProjectTreeNode : NestedProjectTreeNode
    {
        /// <summary>
        /// Gets the AnimationSheet contained within this animation sheet project tree node
        /// </summary>
        public AnimationSheet AnimationSheet { get; }

        /// <summary>
        /// Initializes a new instance of the AnimationSheetProjectTreeNode class
        /// </summary>
        /// <param name="animationSheet">An animation sheet to bind to this AnimationSheetProjectTreeNode</param>
        public AnimationSheetProjectTreeNode(AnimationSheet animationSheet)
        {
            AnimationSheet = animationSheet;
        }
    }

    /// <summary>
    /// Represents a project tree node that contains an animation
    /// </summary>
    public class AnimationProjectTreeNode : ProjectTreeNode
    {
        /// <summary>
        /// Gets the Animation contained within this animation project tree node
        /// </summary>
        public Animation Animation { get; }

        /// <summary>
        /// Initializes a new instance of the AnimationProjectTreeNode class
        /// </summary>
        /// <param name="animation">An animation to bind to this AnimationProjectTreeNode</param>
        public AnimationProjectTreeNode(Animation animation)
        {
            Animation = animation;
        }
    }

    /// <summary>
    /// Represents a project tree node that groups other project tree nodes inside
    /// </summary>
    public class GroupProjectTreeNode : NestedProjectTreeNode
    {

    }
}
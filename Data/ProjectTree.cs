using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        NestedProjectTreeNode rootNode;

        /// <summary>
        /// Gets the root node for this project tree
        /// </summary>
        public NestedProjectTreeNode RootNode { get { return rootNode; } }

        /// <summary>
        /// Initializes a new instance of the ProjectTree class
        /// </summary>
        public ProjectTree()
        {
            rootNode = new NestedProjectTreeNode();
        }

        /// <summary>
        /// Helper method that creates a project tree from the given bundle
        /// </summary>
        /// <param name="bundle">The bundle to create the project tree from</param>
        /// <returns>A project tree that was created from the given bundle</returns>
        public static ProjectTree ProjectTreeFromBundle(Bundle bundle)
        {
            ProjectTree generatedTree = new ProjectTree();

            // Start with the Bundle tree node
            BundleProjectTreeNode root = new BundleProjectTreeNode(bundle);

            generatedTree.rootNode = root;

            // Fill in the animation sheets
            foreach (var sheet in bundle.AnimationSheets)
            {
                AnimationSheetProjectTreeNode sheetNode = new AnimationSheetProjectTreeNode(sheet);
                root.AddChild(sheetNode);

                // Now the animations contained within the sheet
                foreach (var anim in sheet.Animations)
                {
                    AnimationProjectTreeNode animNode = new AnimationProjectTreeNode(anim);
                    sheetNode.AddChild(animNode);
                }
            }

            // Fill in the remaining animations not owned by an animation sheet now
            foreach (var anim in bundle.Animations)
            {
                if (bundle.GetOwningAnimationSheet(anim) == null)
                {
                    AnimationProjectTreeNode animNode = new AnimationProjectTreeNode(anim);
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
        public NestedProjectTreeNode ParentNode
        {
            get { return parentNode; }
        }

        /// <summary>
        /// Adds a project tree node as a child of another project tree node
        /// </summary>
        /// <param name="node">The project node to add as a child of another node</param>
        /// <param name="parent">The parent node to child the first node</param>
        /// <param name="operation">The operation to perform</param>
        protected void InternalSetParent(ProjectTreeNode node, NestedProjectTreeNode parent, SetParentOperation operation)
        {
            if (operation == SetParentOperation.Add)
            {
                if (node.parentNode != parent)
                {
                    throw new ArgumentException("The provided node is not a child of this node", "node");
                }
            }
            else
            {
                if (node.parentNode != null)
                {
                    throw new ArgumentException("Cannot add as a child a node that is already parented by another node", "node");
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
        public ProjectTreeNode[] ChildrenNodes
        {
            get { return childrenNodes.ToArray(); }
        }

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
        public void AddChild(ProjectTreeNode node)
        {
            InternalSetParent(node, this, SetParentOperation.Add);

            childrenNodes.Add(node);
        }

        /// <summary>
        /// Removes a project tree node as a child of this project tree node
        /// </summary>
        /// <param name="node">The project node to remove as a child of this node</param>
        public void RemoveChild(ProjectTreeNode node)
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
        public Bundle Bundle { get; private set; }

        /// <summary>
        /// Initializes a new instance of the BundleProjectTreeNode class
        /// </summary>
        /// <param name="bundle">The bundle to bind to this object</param>
        public BundleProjectTreeNode(Bundle bundle)
        {
            this.Bundle = bundle;
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
        public AnimationSheet AnimationSheet { get; private set; }

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
        public Animation Animation { get; private set; }

        /// <summary>
        /// Initializes a new instance of the AnimationProjectTreeNode class
        /// </summary>
        /// <param name="animation">An animation to bind to this AnimationProjectTreeNode</param>
        public AnimationProjectTreeNode(Animation animation)
        {
            this.Animation = animation;
        }
    }

    /// <summary>
    /// Represents a project tree node that groups other project tree nodes inside
    /// </summary>
    public class GroupProjectTreeNode : NestedProjectTreeNode
    {

    }
}
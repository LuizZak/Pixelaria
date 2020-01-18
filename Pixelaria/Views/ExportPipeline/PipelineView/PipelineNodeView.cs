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
using PixCore.Colors;
using PixCore.Geometry;
using PixUI;

using PixRendering;
using PixPipelineGraph;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// A basic view for a single pipeline step
    /// </summary>
    [DebuggerDisplay("Name: {" + nameof(Name) + "}")]
    internal class PipelineNodeView : BaseView, IEquatable<PipelineNodeView>
    {
        private readonly List<PipelineNodeInputLinkView> _inputs = new List<PipelineNodeInputLinkView>();
        private readonly List<PipelineNodeOutputLinkView> _outputs = new List<PipelineNodeOutputLinkView>();

        /// <summary>
        /// Gets the list of input views on this node view
        /// </summary>
        public IReadOnlyList<PipelineNodeInputLinkView> InputViews => _inputs;

        /// <summary>
        /// Gets the list of output views on this node view
        /// </summary>
        public IReadOnlyList<PipelineNodeOutputLinkView> OutputViews => _outputs;

        /// <summary>
        /// Gets the text associated with this node view's pipeline node's metadata object,
        /// identifier by key <see cref="PipelineMetadataKeys.PipelineStepBodyText"/>.
        /// </summary>
        public string BodyText { get; set; }

        /// <summary>
        /// Area where <see cref="Name"/> should be drawn onto on this node view.
        /// </summary>
        public AABB TitleTextArea { get; set; }

        /// <summary>
        /// Area where <see cref="BodyText"/> should be laid onto on this node view.
        /// </summary>
        public AABB BodyTextArea { get; set; }

        /// <summary>
        /// Area where the input and output node views will be placed in.
        /// </summary>
        public AABB LinkViewLabelArea { get; set; }

        /// <summary>
        /// Gets this node view's underlying node name.
        /// </summary>
        public string Name => NodeView.Title;

        /// <summary>
        /// Gets or sets the display color for this step view.
        /// 
        /// Initialized to a default color depending on which IPipelineStep class
        /// was provided during instantiation, via <see cref="DefaultColorForPipelineStep"/>
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The font to use when drawing the title of the node view
        /// </summary>
        public Font Font { get; set; }
        
        /// <summary>
        /// Gets or sets the resource for the icon to display besides this pipeline node's title.
        /// 
        /// The actual resource contents for the icon's image is dependent on the implementation of the
        /// renderer that is passed in this node view.
        /// 
        /// Set as null to specify no icon should be drawn.
        ///
        /// This property is overriden by <see cref="ManagedIcon"/>
        /// </summary>
        public ImageResource? Icon { get; set; }

        /// <summary>
        /// Gets or sets the resource for the icon to display besides this pipeline node's title.
        /// 
        /// The actual resource contents for the icon's image is dependent on the implementation of the
        /// renderer that is passed in this node view.
        /// 
        /// Set as null to specify no icon should be drawn.
        ///
        /// This property overrides <see cref="Icon"/>.
        /// </summary>
        [CanBeNull]
        public IManagedImageResource ManagedIcon { get; set; }

        /// <summary>
        /// Gets the node ID associated with this pipeline view.
        ///
        /// If <c>null</c>, indicates this view is not associated with a created node.
        /// </summary>
        public PipelineNodeId? NodeId { get; protected set; }

        /// <summary>
        /// Gets the node descriptor that fully describes this pipeline node view
        /// </summary>
        public IPipelineNodeView NodeView { get; private set; }

        /// <summary>
        /// Creates a new pipeline node view for a given pipeline node instance
        /// </summary>
        public static PipelineNodeView Create([NotNull] IPipelineNodeView nodeView)
        {
            var node = new PipelineNodeView();
            node.Initialize(nodeView);

            return node;
        }
        
        /// <summary>
        /// Creates a new pipeline node view for a given pipeline node instance, passing it to
        /// a given initializer closure before returning.
        /// </summary>
        public static PipelineNodeView Create([NotNull] IPipelineNodeView nodeView, [NotNull, InstantHandle] Action<PipelineNodeView> initializer)
        {
            var node = new PipelineNodeView();
            node.Initialize(nodeView);

            initializer(node);

            return node;
        }

        protected PipelineNodeView()
        {
            
        }

        private void Initialize([NotNull] IPipelineNodeView nodeView)
        {
            Font = new Font(FontFamily.GenericSansSerif, 11);

            NodeView = nodeView;
            NodeId = nodeView.NodeId;
            BodyText = nodeView.PipelineMetadata.GetValue(PipelineMetadataKeys.PipelineStepBodyText)?.ToString();
            Color = DefaultColorForPipelineStep(nodeView.NodeKind);
            StrokeColor = DefaultStrokeColorForPipelineStep(nodeView.NodeKind);
            
            ReloadLinkViews();
        }
        
        private void ReloadLinkViews()
        {
            _inputs.Clear();
            _outputs.Clear();

            foreach (var view in children.ToArray()) // Copy to avoid iterating over modifying collection
            {
                RemoveChild(view);
            }

            // Create inputs
            var inputs = NodeView.Inputs;
            var outputs = NodeView.Outputs;

            foreach (var i in inputs)
            {
                var input = PipelineNodeInputLinkView.Create(i);

                _inputs.Add(input);
                AddChild(input);
            }

            foreach (var o in outputs)
            {
                var output = PipelineNodeOutputLinkView.Create(o);

                _outputs.Add(output);
                AddChild(output);
            }
        }
        
        public PipelineNodeLinkView[] GetLinkViews()
        {
            return InputViews.Cast<PipelineNodeLinkView>().Concat(OutputViews).ToArray();
        }

        /// <summary>
        /// Returns the rectangle that represents the title area for this step view.
        /// 
        /// Expressed relative to <see cref="BaseView.Bounds"/>
        /// </summary>
        public AABB GetTitleArea()
        {
            var rect = Bounds;
            rect = rect.WithSize(rect.Width, 25);

            if (Icon != null)
            {
                rect = rect.WithSize(rect.Width, Math.Max(rect.Height, Icon.Value.Height + 4));
            }

            return rect;
        }

        /// <summary>
        /// Returns the relative content area for rendering of a step view, starting
        /// from the bottom of its title area.
        /// 
        /// Expressed relative to <see cref="BaseView.Bounds"/>
        /// </summary>
        public AABB GetContentArea()
        {
            var titleArea = GetTitleArea();

            var rect = Bounds;
            rect = new AABB(new Vector(rect.Left, titleArea.Height), rect.Maximum);

            return rect;
        }

        /// <summary>
        /// If this node view features text content, the AABB returned by this method is
        /// the area where this content can be drawn onto.
        /// 
        /// Result is invalid, if no body text is set (<see cref="BodyText"/> == null).
        /// </summary>
        public AABB GetBodyTextArea()
        {
            return BodyTextArea;
        }

        public bool Equals(PipelineNodeView other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PipelineNodeView) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Gets the default color for the given implementation instance of <see cref="PipelineNodeKind"/>.
        /// </summary>
        public static Color DefaultColorForPipelineStep(PipelineNodeKind nodeKind)
        {
            return Color.White;
        }

        /// <summary>
        /// Gets the default stroke color for the given implementation instance of <see cref="PipelineNodeKind"/>
        /// </summary>
        public static Color DefaultStrokeColorForPipelineStep(PipelineNodeKind nodeKind)
        {
            return Color.White.Faded(Color.Black, 0.3f);
        }
    }
}
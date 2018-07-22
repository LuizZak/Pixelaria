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
using PixDirectX.Rendering;
using PixUI;

using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Steps;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// A basic view for a single pipeline step
    /// </summary>
    [DebuggerDisplay("Name: {" + nameof(Name) + "}")]
    internal class PipelineNodeView : BaseView, IEquatable<PipelineNodeView>
    {
        private readonly List<PipelineNodeLinkView> _inputs = new List<PipelineNodeLinkView>();
        private readonly List<PipelineNodeLinkView> _outputs = new List<PipelineNodeLinkView>();

        /// <summary>
        /// Gets the list of input views on this node view
        /// </summary>
        public IReadOnlyList<PipelineNodeLinkView> InputViews => _inputs;

        /// <summary>
        /// Gets the list of output views on this node view
        /// </summary>
        public IReadOnlyList<PipelineNodeLinkView> OutputViews => _outputs;

        /// <summary>
        /// Gets the text associated with this node view's pipeline node's metadata object,
        /// identifier by key <see cref="PipelineMetadataKeys.PipelineStepBodyText"/>.
        /// </summary>
        public string BodyText => PipelineNode.GetMetadata()?.GetValue(PipelineMetadataKeys.PipelineStepBodyText) as string ?? "";

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
        public string Name => PipelineNode.Name;

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
        /// </summary>
        public ImageResource? Icon { get; set; }

        public IPipelineNode PipelineNode { get; private set; }

        /// <summary>
        /// Creates a new pipeline node view for a given pipeline node instance
        /// </summary>
        public static PipelineNodeView Create(IPipelineNode pipelineNode)
        {
            var node = new PipelineNodeView();
            node.Initialize(pipelineNode);

            return node;
        }
        
        /// <summary>
        /// Creates a new pipeline node view for a given pipeline node instance, passing it to
        /// a given initializer closure before returning.
        /// </summary>
        public static PipelineNodeView Create(IPipelineNode pipelineNode, [NotNull, InstantHandle] Action<PipelineNodeView> initializer)
        {
            var node = new PipelineNodeView();
            node.Initialize(pipelineNode);

            initializer(node);

            return node;
        }

        protected PipelineNodeView()
        {
            
        }

        private void Initialize(IPipelineNode pipelineNode)
        {
            Font = new Font(FontFamily.GenericSansSerif, 11);

            PipelineNode = pipelineNode;
            Color = DefaultColorForPipelineStep(pipelineNode);
            StrokeColor = DefaultStrokeColorForPipelineStep(pipelineNode);
            
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
            var inputs = (PipelineNode as IPipelineNodeWithInputs)?.Input ?? new IPipelineInput[0];
            var outputs = (PipelineNode as IPipelineNodeWithOutputs)?.Output ?? new IPipelineOutput[0];

            for (int i = 0; i < inputs.Count; i++)
            {
                var input = PipelineNodeLinkView.Create(inputs.ElementAt(i));

                _inputs.Add(input);
                AddChild(input);
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                var output = PipelineNodeLinkView.Create(outputs.ElementAt(i));

                _outputs.Add(output);
                AddChild(output);
            }
        }
        
        public PipelineNodeLinkView[] GetLinkViews()
        {
            return InputViews.Concat(OutputViews).ToArray();
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
        /// Gets the default color for the given implementation instance of <see cref="IPipelineNode"/>.
        /// </summary>
        public static Color DefaultColorForPipelineStep(IPipelineNode step)
        {
            if (step is SpriteSheetGenerationPipelineStep)
                return Color.Beige;

            return Color.White;
        }

        /// <summary>
        /// Gets the default stroke color for the given implementation instance of <see cref="IPipelineNode"/>
        /// </summary>
        public static Color DefaultStrokeColorForPipelineStep(IPipelineNode step)
        {
            if (step is SpriteSheetGenerationPipelineStep)
                return Color.Beige.Faded(Color.Black, 0.3f);

            return Color.White.Faded(Color.Black, 0.3f);
        }
    }
}
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
using System.Linq;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Steps;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// A basic view for a single pipeline step
    /// </summary>
    public class PipelineNodeView : BaseView, IEquatable<PipelineNodeView>
    {
        private InsetBounds _bodyTextInset = new InsetBounds(7, 7, 7, 7);

        private const float LinkSize = 10;
        private const float LinkSeparation = 5;

        /// <summary>
        /// Padding between links and top/bottom of content view
        /// </summary>
        private const float LinkPadding = 20;

        private readonly List<PipelineNodeLinkView> _inputs = new List<PipelineNodeLinkView>();
        private readonly List<PipelineNodeLinkView> _outputs = new List<PipelineNodeLinkView>();

        public string BodyText => PipelineNode.GetMetadata()?.GetValue(PipelineMetadataKeys.PipelineStepBodyText) as string ?? "";

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

        public IPipelineNode PipelineNode { get; }

        public PipelineNodeView(IPipelineNode pipelineNode)
        {
            Font = new Font(FontFamily.GenericSansSerif, 11);

            PipelineNode = pipelineNode;
            Color = DefaultColorForPipelineStep(pipelineNode);
            
            ReloadLinkViews();
        }

        public void AutoSize([NotNull] ILabelViewSizeProvider sizeProvider)
        {
            var nameSize = sizeProvider.CalculateTextSize(Name, Font);

            // Calculate link size
            int maxLinkCount = Math.Max(GetInputViews().Length, GetOutputViews().Length);
            float vertLinkSize = maxLinkCount * (LinkSize + LinkSeparation) + LinkPadding;

            if (Icon != null)
            {
                nameSize.Width += Icon.Value.Width + 5;
                nameSize.Height = Math.Max(Icon.Value.Height + 5, nameSize.Height);
            }

            // Measure text from node, if available
            var bodySize = new Vector();
            string bodyText = BodyText;
            if (!string.IsNullOrEmpty(bodyText))
            {
                bodySize = sizeProvider.CalculateTextSize(bodyText, Font) +
                           new Vector(_bodyTextInset.Left + _bodyTextInset.Right,
                               _bodyTextInset.Top + _bodyTextInset.Bottom);

                if (_inputs.Count > 0)
                    bodySize += new Vector(LinkSize, 0);

                if (_outputs.Count > 0)
                    bodySize += new Vector(LinkSize, 0);
            }

            const float minBodySize = 10;
            
            Size = new Vector(Math.Max(80, Math.Max(bodySize.X, nameSize.Width + 8)),
                Math.Max(minBodySize, nameSize.Height + Math.Max(bodySize.Y, vertLinkSize)));

            PositionLinkViews();
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
            var inputs = (PipelineNode as IPipelineStep)?.Input ?? (PipelineNode as IPipelineEnd)?.Input ?? new IPipelineInput[0];
            var outputs = (PipelineNode as IPipelineStep)?.Output ?? new IPipelineOutput[0];

            for (var i = 0; i < inputs.Count; i++)
            {
                var input = new PipelineNodeLinkView(inputs.ElementAt(i));

                _inputs.Add(input);
                AddChild(input);
            }

            for (var i = 0; i < outputs.Count; i++)
            {
                var output = new PipelineNodeLinkView(outputs.ElementAt(i));

                _outputs.Add(output);
                AddChild(output);
            }

            PositionLinkViews();
        }

        private void PositionLinkViews()
        {
            var inputs = (PipelineNode as IPipelineStep)?.Input ?? (PipelineNode as IPipelineEnd)?.Input ?? new IPipelineInput[0];
            var outputs = (PipelineNode as IPipelineStep)?.Output ?? new IPipelineOutput[0];

            var linkSize = new Vector(LinkSize);

            var contentArea = GetContentArea();

            var topLeft = new Vector(contentArea.Left + linkSize.X / 2 + 3, contentArea.Top);
            var botLeft = new Vector(contentArea.Left + linkSize.X / 2 + 3, contentArea.Bottom);
            var topRight = new Vector(contentArea.Right - linkSize.X / 2 - 3, contentArea.Top);
            var botRight = new Vector(contentArea.Right - linkSize.X / 2 - 3, contentArea.Bottom);

            var ins = AlignedBoxesAcrossEdge(inputs.Count, linkSize, topLeft, botLeft, LinkSize + LinkSeparation);
            var outs = AlignedBoxesAcrossEdge(outputs.Count, linkSize, topRight, botRight, LinkSize + LinkSeparation);

            for (int i = 0; i < inputs.Count; i++)
            {
                var rect = ins[i];
                _inputs[i].Location = rect.Minimum;
                _inputs[i].Size = rect.Size;
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                var rect = outs[i];
                _outputs[i].Location = rect.Minimum;
                _outputs[i].Size = rect.Size;
            }
        }

        protected override void OnResize()
        {
            base.OnResize();

            PositionLinkViews();
        }

        public PipelineNodeLinkView[] GetLinkViews()
        {
            return GetInputViews().Concat(GetOutputViews()).ToArray();
        }

        public PipelineNodeLinkView[] GetInputViews()
        {
            return _inputs.ToArray();
        }

        public PipelineNodeLinkView[] GetOutputViews()
        {
            return _outputs.ToArray();
        }
        
        /// <summary>
        /// Returns the rectangle that represents the title area for this step view.
        /// 
        /// Expressed relative to <see cref="BaseView.Bounds"/>
        /// </summary>
        public AABB GetTitleArea()
        {
            var rect = Bounds;
            return rect.WithSize(rect.Width, 25);
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
            rect.Set(new Vector(rect.Left, titleArea.Height), rect.Maximum);

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
            if(BodyText == null)
                return AABB.Empty;
            
            var content = GetContentArea().Inset(_bodyTextInset);
            if (_inputs.Count > 0)
            {
                content = content.Inset(new InsetBounds(LinkSize, 0, 0, 0));
            }
            if (_outputs.Count > 0)
            {
                content = content.Inset(new InsetBounds(0, 0, 0, LinkSize));
            }

            return content;
        }

        private static AABB[] AlignedBoxesAcrossEdge(int count, Vector size, Vector edgeStart, Vector edgeEnd, float separation)
        {
            if (count <= 0)
                return new AABB[0];

            var output = new AABB[count];
            
            var mid = (edgeStart + edgeEnd) / 2;
            var norm = (edgeEnd - edgeStart).Normalized();

            float total = separation * (count - 1);
            var offset = mid - norm * (total / 2.0f);

            for (int i = 0; i < count; i++)
            {
                var point = offset + norm * (separation * i);

                // Re-center rect
                var rect = new AABB(point, point + size);
                rect = rect.OffsetBy(-rect.Width / 2, -rect.Height / 2);

                output[i] = rect;
            }

            return output;
        }

        /// <summary>
        /// Gets the default color for the given implementation instance of IPipelineStep.
        /// </summary>
        public static Color DefaultColorForPipelineStep(IPipelineNode step)
        {
            if (step is SpriteSheetGenerationPipelineStep)
                return Color.Beige;

            return Color.White;
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
        /// Represents an image resource
        /// </summary>
        public struct ImageResource
        {
            public string ResourceName { get; }
            public int Width { get; }
            public int Height { get; }

            public ImageResource(string resourceName, int width, int height)
            {
                ResourceName = resourceName;
                Width = width;
                Height = height;
            }
        }
    }
}
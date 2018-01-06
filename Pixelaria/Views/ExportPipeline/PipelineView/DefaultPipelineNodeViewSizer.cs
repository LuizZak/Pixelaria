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
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixUI;

using Pixelaria.ExportPipeline;
using Pixelaria.Utils.Layouting;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    internal class DefaultPipelineNodeViewSizer : IPipelineNodeViewSizer
    {
        private InsetBounds _bodyTextInset = new InsetBounds(7, 7, 7, 7);

        private const float LinkSize = 10;
        private const float LinkSeparation = 5;

        /// <summary>
        /// Vertical separation between the outputs and inputs link lists on a node
        /// </summary>
        private const float LinkOutputInputsSeparation = 5;

        /// <summary>
        /// Padding between links and top/bottom of content view
        /// </summary>
        private const float LinkPadding = 20;
        
        public void AutoSize(PipelineNodeView nodeView, ILabelViewSizeProvider sizeProvider)
        {
            // Calculate proper label size for the links
            ConfigureLinkViewLabels(nodeView, sizeProvider);

            var nameSize = TitleSize(nodeView, sizeProvider);
            var bodyTextSize = BodyTextSize(nodeView, sizeProvider);

            const float minBodySize = 10;

            // Calculate link size
            float vertLinkSize = MinHeightForLinks(nodeView);
            float horLinkSize = MinWidthForLinks(nodeView);

            nodeView.Size = new Vector(Math.Max(80, Math.Max(bodyTextSize.X + horLinkSize, nameSize.X + 8)),
                Math.Max(minBodySize, nameSize.Y + Math.Max(bodyTextSize.Y, vertLinkSize)));

            nodeView.BodyTextArea = GetBodyTextArea(nodeView);

            PositionLinkViews(nodeView);
        }

        public AABB GetBodyTextArea([NotNull] PipelineNodeView nodeView)
        {
            if (nodeView.BodyText == null)
                return AABB.Empty;

            var content = nodeView.GetContentArea().Inset(_bodyTextInset);
            if (nodeView.InputViews.Count > 0)
            {
                content = content.Inset(new InsetBounds(LinkSize, 0, 0, 0));
            }
            if (nodeView.OutputViews.Count > 0)
            {
                content = content.Inset(new InsetBounds(0, 0, 0, LinkSize));
            }

            return content;
        }

        public static void ConfigureLinkViewLabels([NotNull] PipelineNodeView nodeView, [NotNull] ILabelViewSizeProvider sizeProvider)
        {
            foreach (var link in nodeView.InputViews)
            {
                link.LinkLabel.Size = sizeProvider.CalculateTextSize(link.LinkLabel);
            }

            foreach (var link in nodeView.OutputViews)
            {
                link.LinkLabel.Size = sizeProvider.CalculateTextSize(link.LinkLabel);
            }
        }

        public static void PositionLinkViews([NotNull] PipelineNodeView nodeView)
        {
            var inputs = (nodeView.PipelineNode as IPipelineNodeWithInputs)?.Input ?? new IPipelineInput[0];
            var outputs = (nodeView.PipelineNode as IPipelineNodeWithOutputs)?.Output ?? new IPipelineOutput[0];
            float vertSep = inputs.Count > 0 && outputs.Count > 0 ? LinkOutputInputsSeparation : 0;

            int totalCount = inputs.Count + outputs.Count;

            var linkSize = new Vector(LinkSize);

            var contentArea = nodeView.GetContentArea();

            // Inputs
            var topLeft = new Vector(contentArea.Left + linkSize.X / 2 + 3, contentArea.Top + vertSep);
            var botLeft = new Vector(contentArea.Left + linkSize.X / 2 + 3, contentArea.Bottom);
            var ins = LayoutingHelper.AlignedRectanglesAcrossEdge(totalCount, linkSize, topLeft, botLeft, LinkSize + LinkSeparation);
            for (int i = 0; i < inputs.Count; i++)
            {
                var rect = ins[i + outputs.Count];
                var link = nodeView.InputViews[i];

                link.Location = rect.Minimum;
                link.Size = rect.Size;

                link.LinkLabel.Location = new Vector(link.Size.X + 5, link.Size.Y / 2 - link.LinkLabel.Size.Y / 2);
            }
            
            // Outputs
            var topRight = new Vector(contentArea.Right - linkSize.X / 2 - 3, contentArea.Top);
            var botRight = new Vector(contentArea.Right - linkSize.X / 2 - 3, contentArea.Bottom - vertSep);
            var outs = LayoutingHelper.AlignedRectanglesAcrossEdge(totalCount, linkSize, topRight, botRight, LinkSize + LinkSeparation);
            for (int i = 0; i < outputs.Count; i++)
            {
                var rect = outs[i];
                var link = nodeView.OutputViews[i];

                link.Location = rect.Minimum;
                link.Size = rect.Size;

                link.LinkLabel.Location = new Vector(-link.LinkLabel.Size.X - 5, link.Size.Y / 2 - link.LinkLabel.Size.Y / 2);
            }
        }

        private static Vector TitleSize([NotNull] PipelineNodeView nodeView, [NotNull] ILabelViewSizeProvider sizeProvider)
        {
            // Calculate title size
            var nameSize = sizeProvider.CalculateTextSize(nodeView.Name, nodeView.Font);

            if (nodeView.Icon == null)
                return nameSize;

            nameSize.Width += nodeView.Icon.Value.Width + 5;
            nameSize.Height = Math.Max(nodeView.Icon.Value.Height + 5, nameSize.Height);

            return nameSize;
        }

        private Vector BodyTextSize([NotNull] PipelineNodeView nodeView, [NotNull] ILabelViewSizeProvider sizeProvider)
        {
            var bodySize = new Vector();
            string bodyText = nodeView.BodyText;
            if (!string.IsNullOrEmpty(bodyText))
            {
                bodySize = sizeProvider.CalculateTextSize(bodyText, nodeView.Font) +
                           new Vector(_bodyTextInset.Left + _bodyTextInset.Right,
                               _bodyTextInset.Top + _bodyTextInset.Bottom);

                if (nodeView.InputViews.Count > 0)
                    bodySize += new Vector(LinkSize, 0);

                if (nodeView.OutputViews.Count > 0)
                    bodySize += new Vector(LinkSize, 0);
            }

            return bodySize;
        }
        
        private static float MinHeightForLinks(PipelineNodeView nodeView)
        {
            int linkCount = nodeView.InputViews.Count + nodeView.OutputViews.Count;

            float vertLinkSize = linkCount * (LinkSize + LinkSeparation) + LinkPadding;
            
            // Separator line
            if (nodeView.InputViews.Count > 0 && nodeView.OutputViews.Count > 0)
            {
                vertLinkSize += LinkOutputInputsSeparation;
            }
            
            return vertLinkSize;
        }

        private static float MinWidthForLinks([NotNull] PipelineNodeView nodeView)
        {
            float inputWidth =
                nodeView.InputViews.Select(linkView => LinkSize + 13 + linkView.LinkLabel.Width).Concat(new float[] {0}).Max();

            float outputWidth =
                nodeView.OutputViews.Select(linkView => LinkSize + 13 + linkView.LinkLabel.Width).Concat(new float[] {0}).Max();

            return Math.Max(inputWidth, outputWidth);
        }
    }
}
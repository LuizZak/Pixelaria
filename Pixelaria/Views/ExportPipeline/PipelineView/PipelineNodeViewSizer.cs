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
using JetBrains.Annotations;
using Pixelaria.ExportPipeline;
using Pixelaria.Utils;
using Pixelaria.Utils.Layouting;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    internal class PipelineNodeViewSizer : IPipelineNodeViewSizer
    {
        private InsetBounds _bodyTextInset = new InsetBounds(7, 7, 7, 7);

        private const float LinkSize = 10;
        private const float LinkSeparation = 5;

        /// <summary>
        /// Padding between links and top/bottom of content view
        /// </summary>
        private const float LinkPadding = 20;
        
        public void AutoSize(PipelineNodeView nodeView, ILabelViewSizeProvider sizeProvider)
        {
            var nameSize = sizeProvider.CalculateTextSize(nodeView.Name, nodeView.Font);
            
            // Calculate link size
            int maxLinkCount = Math.Max(nodeView.InputViews.Count, nodeView.OutputViews.Count);
            float vertLinkSize = maxLinkCount * (LinkSize + LinkSeparation) + LinkPadding;

            if (nodeView.Icon != null)
            {
                nameSize.Width += nodeView.Icon.Value.Width + 5;
                nameSize.Height = Math.Max(nodeView.Icon.Value.Height + 5, nameSize.Height);
            }

            // Measure text from node, if available
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

            const float minBodySize = 10;

            nodeView.Size = new Vector(Math.Max(80, Math.Max(bodySize.X, nameSize.Width + 8)),
                Math.Max(minBodySize, nameSize.Height + Math.Max(bodySize.Y, vertLinkSize)));

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

        public static void PositionLinkViews([NotNull] PipelineNodeView nodeView)
        {
            var inputs = (nodeView.PipelineNode as IPipelineStep)?.Input ?? (nodeView.PipelineNode as IPipelineEnd)?.Input ?? new IPipelineInput[0];
            var outputs = (nodeView.PipelineNode as IPipelineStep)?.Output ?? new IPipelineOutput[0];

            var linkSize = new Vector(LinkSize);

            var contentArea = nodeView.GetContentArea();

            var topLeft = new Vector(contentArea.Left + linkSize.X / 2 + 3, contentArea.Top);
            var botLeft = new Vector(contentArea.Left + linkSize.X / 2 + 3, contentArea.Bottom);
            var topRight = new Vector(contentArea.Right - linkSize.X / 2 - 3, contentArea.Top);
            var botRight = new Vector(contentArea.Right - linkSize.X / 2 - 3, contentArea.Bottom);

            var ins = LayoutingHelper.AlignedRectanglesAcrossEdge(inputs.Count, linkSize, topLeft, botLeft, LinkSize + LinkSeparation);
            var outs = LayoutingHelper.AlignedRectanglesAcrossEdge(outputs.Count, linkSize, topRight, botRight, LinkSize + LinkSeparation);

            for (int i = 0; i < inputs.Count; i++)
            {
                var rect = ins[i];
                
                nodeView.InputViews[i].Location = rect.Minimum;
                nodeView.InputViews[i].Size = rect.Size;
            }

            for (int i = 0; i < outputs.Count; i++)
            {
                var rect = outs[i];

                nodeView.OutputViews[i].Location = rect.Minimum;
                nodeView.OutputViews[i].Size = rect.Size;
            }
        }
    }
}
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
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixRendering;
using PixUI.Utils.Layout;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    internal class DefaultPipelineNodeViewLayout : IPipelineNodeViewLayout
    {
        private readonly InsetBounds _nodeTitleInset = new InsetBounds(4, 0, 0, 4);
        private readonly InsetBounds _bodyTextInset = new InsetBounds(7, 0, 0, 7);

        /// <summary>
        /// Padding between title view / link views / text body components
        /// (This is not applied between links themselves, see <see cref="LinkSeparation"/>).
        /// </summary>
        private const float ContentPadding = 7;

        private const float LinkSize = 10;
        private const float LinkSeparation = 5;

        /// <summary>
        /// The padding between the outer margins and the inner effective content area of the
        /// pipeline view.
        /// When input/outputs are placed, the label sits inside this margin, while the link
        /// circle sits on top of it.
        /// </summary>
        private const float OuterMarginsPadding = 20;

        /// <summary>
        /// Vertical separation between the outputs and inputs link lists on a node
        /// </summary>
        private const float LinkOutputInputsSeparation = 7;

        /// <summary>
        /// Vertical padding size to add under editable link inputs
        /// </summary>
        private const float LinkInputEditFieldSize = 14;
        
        public void Layout(PipelineNodeView nodeView, ITextSizeProvider sizeProvider)
        {
            // Get minimum width first
            const float minimumWidth = 80;

            ResizeLinkViewsIn(nodeView, sizeProvider);

            var titleArea = AreaForTitle(nodeView, sizeProvider);
            float widthForLinks = WidthForLinkViewsIn(nodeView, sizeProvider);
            var bodySize = SizeForTextBody(nodeView, sizeProvider);

            float nodeViewWidth = Math.Max(minimumWidth, Math.Max(Math.Max(titleArea.Right, widthForLinks), bodySize.X));

            // Lay down items vertically

            // Title
            float nodeViewHeight = titleArea.Bottom;
            nodeViewHeight += ContentPadding;
            
            // Links
            PositionLinkViews(nodeView, nodeViewHeight, ref nodeViewWidth, out nodeViewHeight);
            nodeViewHeight += ContentPadding;
            
            // Body text
            if(bodySize.Y > 0)
            {
                nodeViewHeight += ContentPadding;
                nodeView.BodyTextArea = AABB.FromRectangle(0, nodeViewHeight, nodeViewWidth, bodySize.Y).Inset(_bodyTextInset);
                nodeViewHeight += nodeView.BodyTextArea.Height;
                nodeViewHeight += ContentPadding;
            }
            
            var size = new Vector(nodeViewWidth, nodeViewHeight);
            nodeView.TitleTextArea = titleArea.Inset(_nodeTitleInset);
            nodeView.Size = size;

            nodeView.LinkViewLabelArea = new AABB(0, titleArea.Bottom, bodySize.Y > 0 ? nodeView.BodyTextArea.Top : nodeView.Height, nodeView.Width);

            if (nodeView.InputViews.Count > 0)
            {
                nodeView.LinkViewLabelArea =
                    nodeView.LinkViewLabelArea.Inset(new InsetBounds(OuterMarginsPadding, 0, 0, 0));
            }
            if (nodeView.OutputViews.Count > 0)
            {
                nodeView.LinkViewLabelArea =
                    nodeView.LinkViewLabelArea.Inset(new InsetBounds(0, 0, 0, OuterMarginsPadding));
            }
        }

        private static void ResizeLinkViewsIn([NotNull] PipelineNodeView nodeView, [NotNull] ITextSizeProvider sizeProvider)
        {
            // Pre-size links
            var size = new Vector(LinkSize);
            
            foreach (var link in nodeView.OutputViews)
            {
                link.Size = size;
                ConfigureNodeLinkLabel(link, LabelLocation.Left, sizeProvider);
            }
            
            foreach (var link in nodeView.InputViews)
            {
                link.Size = size;
                ConfigureNodeLinkLabel(link, LabelLocation.Right, sizeProvider);
            }
        }
        
        private static void PositionLinkViews([NotNull] PipelineNodeView nodeView, float yPos, ref float width, out float height)
        {
            // Position vertically
            float y = yPos;
            
            var inputs = nodeView.InputViews;
            var outputs = nodeView.OutputViews;
            
            var size = new Vector(LinkSize);
            
            const float yStep = LinkSize + LinkSeparation;

            foreach (var link in outputs)
            {
                float x = width - size.X - (OuterMarginsPadding - LinkSize) / 2;

                link.Location = new Vector(x, y);
                y += yStep;
            }

            if (outputs.Count > 0 && inputs.Count > 0)
            {
                y += LinkOutputInputsSeparation;
            }

            foreach (var link in inputs)
            {
                const float x = (OuterMarginsPadding - LinkSize) / 2;

                link.Location = new Vector(x, y);
                y += yStep;

                if (link.IsInputEditable)
                {
                    y += LinkInputEditFieldSize;
                }
            }

            // Subtract extra separation that hangs around
            if (outputs.Count > 0 || inputs.Count > 0)
            {
                y -= LinkSeparation;
            }
            
            height = y;
        }

        private Vector SizeForTextBody([NotNull] PipelineNodeView nodeView, [NotNull] ITextSizeProvider sizeProvider)
        {
            var bodySize = new Vector();
            string bodyText = nodeView.BodyText;
            if (!string.IsNullOrEmpty(bodyText))
            {
                bodySize = sizeProvider.CalculateTextSize(bodyText, nodeView.Font) +
                           new Vector(_bodyTextInset.Left + _bodyTextInset.Right, _bodyTextInset.Top + _bodyTextInset.Bottom);
            }

            return bodySize;
        }
        
        private AABB AreaForTitle([NotNull] PipelineNodeView nodeView, [NotNull] ITextSizeProvider sizeProvider)
        {
            // Calculate title size
            var titleArea =
                new AABB(Vector.Zero, sizeProvider.CalculateTextSize(nodeView.Name, nodeView.Font))
                    .GrowingSizeBy(_nodeTitleInset.Left + _nodeTitleInset.Right, 0);

            titleArea = titleArea.WithSize(titleArea.Width, Math.Max(25, titleArea.Height));
            
            if (nodeView.Icon == null && nodeView.ManagedIcon == null)
                return titleArea;

            var imageArea = Size.Empty;
            if (nodeView.ManagedIcon != null)
            {
                imageArea = nodeView.ManagedIcon.Size;
            }
            else if (nodeView.Icon != null)
            {
                imageArea = nodeView.Icon.Value.Size;
            }

            // Deal with icon
            float horizontalDisplace = imageArea.Width + 4;
            float totalTitleHeight = Math.Max(imageArea.Height + 4, titleArea.Height);

            titleArea = titleArea.OffsetBy(horizontalDisplace, 0);
            titleArea = LayoutHelper.CenterWithinContainer(titleArea, AABB.FromRectangle(0, 0, titleArea.Width, totalTitleHeight), LayoutDirection.Vertical);

            return titleArea;
        }

        private static float WidthForLinkViewsIn([NotNull] PipelineNodeView nodeView, [NotNull] ITextSizeProvider sizeProvider)
        {
            float width = 0;
            foreach (var linkView in nodeView.InputViews.Cast<PipelineNodeLinkView>().Concat(nodeView.OutputViews))
            {
                linkView.LinkLabel.Size = sizeProvider.CalculateTextSize(linkView.LinkLabel.AttributedText, linkView.LinkLabel.TextFont);

                float linkWidth = linkView.GetFullBounds().Width + (OuterMarginsPadding - LinkSize) / 2 + LinkSeparation * 2;

                if (width < linkWidth)
                    width = linkWidth;
            }

            if (nodeView.InputViews.Count > 0 && nodeView.OutputViews.Count > 0)
            {
                width += OuterMarginsPadding;
            }

            return width;
        }
        
        private static void ConfigureNodeLinkLabel([NotNull] PipelineNodeLinkView linkView, LabelLocation location, [NotNull] ITextSizeProvider sizeProvider)
        {
            linkView.LinkLabel.Size = sizeProvider.CalculateTextSize(linkView.LinkLabel.AttributedText, linkView.LinkLabel.TextFont);

            float labelY = linkView.Size.Y / 2 - linkView.LinkLabel.Size.Y / 2;

            if (location == LabelLocation.Right)
            {
                linkView.LinkLabel.Location = new Vector(linkView.Size.X + LinkSeparation * 2, labelY);
            }
            else
            {
                linkView.LinkLabel.Location = new Vector(-linkView.LinkLabel.Size.X - LinkSeparation * 2, labelY);
            }
        }
        
        private enum LabelLocation
        {
            Left,
            Right
        }
    }
}
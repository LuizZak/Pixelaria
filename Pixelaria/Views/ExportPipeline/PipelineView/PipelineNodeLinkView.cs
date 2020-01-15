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

using System.Drawing;
using JetBrains.Annotations;

using PixUI;
using PixPipelineGraph;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// A view for a link of a pipeline step view
    /// </summary>
    internal abstract class PipelineNodeLinkView : BaseView
    {
        /// <summary>
        /// Gets the parent step view for this link view
        /// </summary>
        // ReSharper disable once AnnotateCanBeNullTypeMember
        public PipelineNodeView NodeView => (PipelineNodeView)Parent;

        /// <summary>
        /// Gets the title for this node
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets a label displayed alongside this node link view on the parent
        /// node view's body
        /// </summary>
        public LabelView LinkLabel { get; } = new LabelView();

        protected PipelineNodeLinkView(string title)
        {
            Title = title;
        }

        protected void Initialize()
        {
            LinkLabel.StrokeWidth = 0;
            LinkLabel.TextColor = Color.Black;

            UpdateDisplay();

            AddChild(LinkLabel);
        }

        public override string DebugHierarchyBodyDescription(string tabs)
        {
            return $"{tabs}Label = \"{LinkLabel.Text}\"";
        }

        /// <summary>
        /// Updates the display for this link view based on the latest node link data.
        /// </summary>
        public void UpdateDisplay()
        {
            LinkLabel.Text = Title;
        }
    }

    /// <summary>
    /// A view for an input link of a pipeline step view
    /// </summary>
    internal sealed class PipelineNodeInputLinkView : PipelineNodeLinkView
    {
        /// <summary>
        /// Gets or sets the ID of this node input.
        /// 
        /// If <c>null</c>, indicates this view is not associated with a node.
        /// </summary>
        public PipelineInput? InputId { get; }

        public static PipelineNodeInputLinkView Create([NotNull] IPipelineInput input)
        {
            var view = new PipelineNodeInputLinkView(input);
            view.Initialize();
            return view;
        }

        private PipelineNodeInputLinkView([NotNull] IPipelineInput input) : base(input.Name)
        {
            InputId = input.Id;
        }
    }

    /// <summary>
    /// A view for an output link of a pipeline step view
    /// </summary>
    internal sealed class PipelineNodeOutputLinkView : PipelineNodeLinkView
    {
        /// <summary>
        /// Gets or sets the ID of this node output.
        /// 
        /// If <c>null</c>, indicates this view is not associated with a node.
        /// </summary>
        public PipelineOutput? OutputId { get; }

        public static PipelineNodeOutputLinkView Create([NotNull] IPipelineOutput output)
        {
            var view = new PipelineNodeOutputLinkView(output);
            view.Initialize();
            return view;
        }

        private PipelineNodeOutputLinkView([NotNull] IPipelineOutput output) : base(output.Name)
        {
            OutputId = output.Id;
        }
    }
}
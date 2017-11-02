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

using JetBrains.Annotations;
using Pixelaria.ExportPipeline;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// A view for a link of a pipeline step view
    /// </summary>
    internal class PipelineNodeLinkView : BaseView
    {
        /// <summary>
        /// A static pipeline output connected to this node link, if available.
        /// 
        /// Is set to null and replaced by any other output that is connected to
        /// this input.
        /// </summary>
        [CanBeNull]
        public IStaticPipelineOutput FixedOutput { get; set; }

        /// <summary>
        /// The connection this link references on its parent step view
        /// </summary>
        public IPipelineNodeLink NodeLink { get; }

        /// <summary>
        /// Gets the parent step view for this link view
        /// </summary>
        // ReSharper disable once AnnotateCanBeNullTypeMember
        public PipelineNodeView NodeView => (PipelineNodeView)Parent;

        public PipelineNodeLinkView(IPipelineNodeLink nodeLink)
        {
            NodeLink = nodeLink;
        }
    }
}
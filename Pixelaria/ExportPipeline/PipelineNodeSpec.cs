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

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Small object that encapsulates display and creation info for a pipeline node
    /// </summary>
    public class PipelineNodeSpec
    {
        /// <summary>
        /// The display name for the pipeline node
        /// </summary>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Gets the node's reflection type
        /// </summary>
        [NotNull]
        public Type NodeType { get; }

        /// <summary>
        /// Function that must instantiate pipeline nodes when called.
        /// </summary>
        [NotNull]
        public Func<IPipelineNode> CreateNode;

        public PipelineNodeSpec([NotNull] string name, [NotNull] Type nodeType, [NotNull] Func<IPipelineNode> createNode)
        {
            CreateNode = createNode;
            NodeType = nodeType;
            Name = name;
        }

        /// <summary>
        /// Helper for creating a pipeline node spec w/ a node creation function that uses reflection
        /// to fetch a parameter-less constructor for the node type.
        /// </summary>
        [NotNull]
        public static PipelineNodeSpec Of<T>([NotNull] string name) where T : IPipelineNode, new()
        {
            return new PipelineNodeSpec(name, typeof(T), () => new T());
        }
    }
}
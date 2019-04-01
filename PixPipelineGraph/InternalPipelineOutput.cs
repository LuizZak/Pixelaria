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

namespace PixPipelineGraph
{
    internal class InternalPipelineOutput: InternalPipelineNodeLink, IPipelineOutput
    {
        /// <summary>
        /// Gets the node that owns this pipeline output
        /// </summary>
        internal PipelineNode Node { get; }
        internal PipelineOutput Id { get; }
        public Type DataType { get; internal set; }

        public InternalPipelineOutput([NotNull] PipelineNode node, PipelineOutput id, [NotNull] string name, [NotNull] Type dataType)
            : base(node.Id, name)
        {
            Node = node;
            Id = id;
            DataType = dataType;
        }
    }
}
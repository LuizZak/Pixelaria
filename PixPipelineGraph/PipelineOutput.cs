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

namespace PixPipelineGraph
{
    /// <summary>
    /// Describes information for an output of a pipeline node
    /// </summary>
    public struct PipelineOutput : IEquatable<PipelineOutput>
    {
        /// <summary>
        /// The node this pipeline outputs is attached to
        /// </summary>
        public PipelineNodeId NodeId { get; }

        /// <summary>
        /// The index from the list of node outputs this output id represents
        /// </summary>
        public int Index { get; }

        public PipelineOutput(PipelineNodeId nodeId, int index)
        {
            Index = index;
            NodeId = nodeId;
        }

        public bool Equals(PipelineOutput other)
        {
            return NodeId.Equals(other.NodeId) && Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is PipelineOutput other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NodeId.GetHashCode() * 397) ^ Index;
            }
        }

        public static bool operator ==(PipelineOutput left, PipelineOutput right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PipelineOutput left, PipelineOutput right)
        {
            return !left.Equals(right);
        }
    }
}
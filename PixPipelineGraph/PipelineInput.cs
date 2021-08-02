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
    /// Describes information for an input of a pipeline node
    /// </summary>
    public readonly struct PipelineInput : IEquatable<PipelineInput>
    {
        /// <summary>
        /// The node this pipeline input is attached to
        /// </summary>
        public PipelineNodeId NodeId { get; }

        /// <summary>
        /// The index from the list of node inputs this input id represents
        /// </summary>
        public int Index { get; }

        public PipelineInput(PipelineNodeId nodeId, int index)
        {
            Index = index;
            NodeId = nodeId;
        }

        public bool Equals(PipelineInput other)
        {
            return NodeId.Equals(other.NodeId) && Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is PipelineInput other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (NodeId.GetHashCode() * 397) ^ Index;
            }
        }

        public static bool operator ==(PipelineInput left, PipelineInput right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PipelineInput left, PipelineInput right)
        {
            return !left.Equals(right);
        }
    }
}
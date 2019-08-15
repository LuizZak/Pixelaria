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
using System.Diagnostics;

namespace PixPipelineGraph
{
    /// <inheritdoc cref="IEquatable{T}" />
    /// <summary>
    /// Represents a unique identifier for a kind of a <see cref="T:PixPipelineGraph.PipelineNode" /> on a graph.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public readonly struct PipelineNodeKind : IEquatable<PipelineNodeKind>
    {
        public string Id { get; }

        public PipelineNodeKind(in string id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{{{nameof(Id)}: ${Id}}}";
        }

        public bool Equals(PipelineNodeKind other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is PipelineNodeId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(in PipelineNodeKind left, in PipelineNodeKind right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in PipelineNodeKind left, in PipelineNodeKind right)
        {
            return !left.Equals(right);
        }
    }
}
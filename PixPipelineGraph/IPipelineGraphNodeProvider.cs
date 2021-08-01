﻿/*
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
    /// <summary>
    /// Provider for pipeline bodies
    /// </summary>
    public interface IPipelineGraphNodeProvider
    {
        /// <summary>
        /// Gets a pipeline body with a given ID.
        /// </summary>
        [CanBeNull]
        PipelineBody GetBody(PipelineBodyId id);

        /// <summary>
        /// Returns whether this node provider has the ability to create a node of a specified kind.
        /// </summary>
        bool CanCreateNode(PipelineNodeKind kind);

        /// <summary>
        /// Asks this node provider to create a specific node kind into a provided node builder.
        ///
        /// Returns a value specifying whether the node kind is known and was properly created on
        /// the provided builder.
        /// </summary>
        bool CreateNode(PipelineNodeKind nodeKind, PipelineNodeBuilder builder);
    }

    /// <summary>
    /// Defines an identifier for a pipeline body ID.
    /// </summary>
    public struct PipelineBodyId: IEquatable<PipelineBodyId>
    {
        public string Id { get; }

        public PipelineBodyId(string id)
        {
            Id = id;
        }

        public bool Equals(PipelineBodyId other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is PipelineBodyId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(PipelineBodyId left, PipelineBodyId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PipelineBodyId left, PipelineBodyId right)
        {
            return !left.Equals(right);
        }
    }
}
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
using System.Collections.Generic;
using JetBrains.Annotations;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Base interface for pipeline nodes that accept and/or produce outputs.
    /// </summary>
    public interface IPipelineNode
    {
        /// <summary>
        /// Gets an identifier that uniquely identifies this node between different nodes within
        /// a node container.
        /// 
        /// A pipeline node that is initializing itself must always set this value to <see cref="Guid.NewGuid()"/>.
        /// </summary>
        PipelineNodeId Id { get; }

        /// <summary>
        /// The display name of this pipeline node
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets specific metadata for this pipeline node
        /// </summary>
        [CanBeNull]
        IPipelineMetadata GetMetadata();
    }

    /// <summary>
    /// Interface for a pipeline node that accepts one or more inputs
    /// </summary>
    public interface IPipelineNodeWithInputs : IPipelineNode
    {
        /// <summary>
        /// Accepted inputs for this pipeline node
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineInput> Input { get; }
    }

    /// <summary>
    /// Interface for a pipeline node that produces one or more outputs
    /// </summary>
    public interface IPipelineNodeWithOutputs : IPipelineNode
    {
        /// <summary>
        /// Accepted outputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineOutput> Output { get; }
    }

    /// <summary>
    /// Interface for a pipeline step
    /// </summary>
    public interface IPipelineStep : IPipelineNodeWithInputs, IPipelineNodeWithOutputs
    {
        
    }

    /// <summary>
    /// Interface for a pipeline step that has only outputs, and requires no inputs to produce
    /// values.
    /// </summary>
    public interface IPipelineStart : IPipelineNodeWithOutputs
    {
        
    }

    /// <summary>
    /// Represents a pipeline step that is the final output of the sequence of pipeline steps
    /// </summary>
    public interface IPipelineEnd : IPipelineNodeWithInputs, IDisposable
    {
        /// <summary>
        /// Starts the chain of consumption of pipeline steps linked to this pipeline end.
        /// </summary>
        void Begin();
    }

    /// <summary>
    /// An interface for a pipeline output that sends down static data values on subscription.
    /// </summary>
    public interface IStaticPipelineOutput : IPipelineOutput
    {
        
    }
}
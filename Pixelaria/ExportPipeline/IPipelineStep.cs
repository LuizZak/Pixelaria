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

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Base interface for IPipelineEnd and IPipelineStep nodes
    /// </summary>
    public interface IPipelineNode
    {
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
    /// Interface for a pipeline step
    /// </summary>
    public interface IPipelineStep : IPipelineNode
    {
        /// <summary>
        /// Accepted inputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineInput> Input { get; }

        /// <summary>
        /// Accepted outputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineOutput> Output { get; }
    }

    /// <summary>
    /// Represents a pipeline step that is the final output of the sequence of pipeline steps
    /// </summary>
    public interface IPipelineEnd : IPipelineNode, IDisposable
    {
        /// <summary>
        /// Accepted inputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineInput> Input { get; }

        /// <summary>
        /// Starts the chain of consumption of pipeline steps linked to this pipeline end.
        /// </summary>
        void Begin();
    }

    /// <summary>
    /// Base interface for pipeline step input/outputs
    /// </summary>
    public interface IPipelineNodeLink
    {
        /// <summary>
        /// The step of this link
        /// </summary>
        [CanBeNull]
        IPipelineNode Node { get; }

        /// <summary>
        /// An identifying name for this link on its parent pipeline step
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets specific metadata for this pipeline connection
        /// </summary>
        [CanBeNull]
        IPipelineMetadata GetMetadata();
    }

    /// <summary>
    /// An input for a pipeline step
    /// </summary>
    public interface IPipelineInput : IPipelineNodeLink
    {
        /// <summary>
        /// The types of data that can be consumed by this input
        /// </summary>
        [NotNull]
        Type[] DataTypes { get; }

        /// <summary>
        /// Gets an array of all connections this pipeline input has
        /// </summary>
        [NotNull]
        IPipelineOutput[] Connections { get; }

        /// <summary>
        /// Called to include a pipeline output on this pipeline input
        /// </summary>
        void Connect(IPipelineOutput output);

        /// <summary>
        /// Removes a given output from this input
        /// </summary>
        void Disconnect(IPipelineOutput output);
    }

    /// <summary>
    /// An output for a pipeline step
    /// </summary>
    public interface IPipelineOutput : IPipelineNodeLink
    {
        /// <summary>
        /// The type of data that is outputted by this pipeline connection
        /// </summary>
        [NotNull]
        Type DataType { get; }

        /// <summary>
        /// Gets an observable connection that spits item from this pipeline output.
        /// 
        /// Subscribing for this output awaits until one item is produced, which is then
        /// forwarded replicatively to all consumers.
        /// </summary>
        IObservable<object> GetConnection();
    }
}
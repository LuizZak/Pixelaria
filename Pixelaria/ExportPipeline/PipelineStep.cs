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

using System.Collections.Generic;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Interface for a pipeline step
    /// </summary>
    public interface IPipelineStep
    {
        /// <summary>
        /// The display name of this pipeline step
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Accepted inputs for this pipeline step
        /// </summary>
        IReadOnlyCollection<IPipelineInput> Input { get; }

        /// <summary>
        /// Accepted outputs for this pipeline step
        /// </summary>
        IReadOnlyCollection<IPipelineOutput> Output { get; }

        /// <summary>
        /// Gets specific metadata for this pipeline step
        /// </summary>
        object[] GetMetadata();
    }

    /// <summary>
    /// Base interface for pipeline step input/outputs
    /// </summary>
    public interface IPipelineConnection
    {
        PipelineStepDataType DataType { get; }

        /// <summary>
        /// Gets specific metadata for this pipeline connection
        /// </summary>
        object[] GetMetadata();
    }

    /// <summary>
    /// An input for a pipeline step
    /// </summary>
    public interface IPipelineInput: IPipelineConnection
    {

    }

    /// <summary>
    /// An output for a pipeline step
    /// </summary>
    public interface IPipelineOutput: IPipelineConnection
    {

    }

    /// <summary>
    /// Specifies the type of a pipeline data that pipeline steps can 
    /// receive or output
    /// </summary>
    public enum PipelineStepDataType
    {
        /// <summary>
        /// Pixel data; i.e. an image.
        /// </summary>
        Pixel,

        /// <summary>
        /// Metadata that is tied to a Pixel data type.
        /// </summary>
        Metadata,

        /// <summary>
        /// Pixel and metadata interlaced into one data type.
        /// </summary>
        PixelAndMetadata
    }
}

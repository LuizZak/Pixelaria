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

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// Base abstract pipeline step to start subclassing and specializing pipeline steps
    /// </summary>
    public abstract class AbstractPipelineStep : IPipelineStep
    {
        public abstract string Name { get; }

        public abstract IReadOnlyList<IPipelineInput> Input { get; }
        public abstract IReadOnlyList<IPipelineOutput> Output { get; }

        /// <summary>
        /// Default implementation for <see cref="IPipelineStep.GetMetadata"/>
        /// that returns an empty pipeline metadata object
        /// </summary>
        public virtual IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}
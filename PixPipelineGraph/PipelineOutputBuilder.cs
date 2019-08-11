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
    /// <summary>
    /// Provides an interface to create pipeline outputs with.
    /// </summary>
    public class PipelineOutputBuilder: IMetadataObjectBuilder
    {
        private readonly PipelineBuildStepCollection<InternalPipelineOutput> _stepCollection = new PipelineBuildStepCollection<InternalPipelineOutput>();

        internal PipelineOutputBuilder()
        {

        }

        /// <summary>
        /// Sets the name of the pipeline output.
        /// </summary>
        public void SetName(string name)
        {
            _stepCollection.AddClosureBuilderStep(output =>
            {
                output.Name = name;
            });
        }

        /// <summary>
        /// Sets the type the pipeline output will be expected to produce
        /// </summary>
        public void SetOutputType(Type type)
        {
            _stepCollection.AddClosureBuilderStep(output =>
            {
                output.DataType = type;
            });
        }

        /// <summary>
        /// Adds an entry for a metadata value for the created output.
        /// </summary>
        public void AddMetadataEntry(string key, object value)
        {
            _stepCollection.AddClosureBuilderStep(output =>
            {
                output.Metadata.SetValue(key, value);
            });
        }

        internal InternalPipelineOutput Build([NotNull] PipelineNode node, PipelineOutput id, [NotNull] string name)
        {
            var input = new InternalPipelineOutput(node, id, name, typeof(object));

            _stepCollection.Apply(input);

            return input;
        }
    }
}
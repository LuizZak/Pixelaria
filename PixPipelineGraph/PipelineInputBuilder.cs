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
    /// Provides an interface to create pipeline inputs with.
    /// </summary>
    public class PipelineInputBuilder
    {
        private readonly PipelineBuildStepCollection<InternalPipelineInput> _stepCollection = new PipelineBuildStepCollection<InternalPipelineInput>();

        internal PipelineInputBuilder()
        {

        }
        
        /// <summary>
        /// Sets the name of the pipeline input.
        /// </summary>
        public void SetName(string name)
        {
            _stepCollection.AddClosureBuilderStep(output =>
            {
                output.Name = name;
            });
        }

        /// <summary>
        /// Adds a type to the list of types the pipeline input can consume
        /// </summary>
        public void AddInputType(Type type)
        {
            _stepCollection.AddClosureBuilderStep(input =>
            {
                input.dataTypes.Add(type);
            });
        }

        internal InternalPipelineInput Build([NotNull] PipelineNode node, PipelineInput id, [NotNull] string name)
        {
            var input = new InternalPipelineInput(node, id, name, new Type[0]);

            _stepCollection.Apply(input);

            return input;
        }
    }
}
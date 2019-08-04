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
    /// Provides an interface to create pipeline nodes with.
    /// </summary>
    public class PipelineNodeBuilder
    {
        private readonly PipelineBuildStepCollection<PipelineNode> _stepCollection = new PipelineBuildStepCollection<PipelineNode>();

        internal PipelineNodeBuilder()
        {
            
        }

        public void SetTitle(string title)
        {
            _stepCollection.AddClosureBuilderStep(node => { node.Title = title; });
        }

        public void AddBody<TInput, TOutput>([NotNull] Func<TInput, TOutput> body)
        {
            AddBody(typeof(TInput), typeof(TOutput), input => body((TInput)input));
        }

        public void AddBody(Type inputType, Type outputType, [NotNull] Func<object, object> body)
        {
            _stepCollection.AddClosureBuilderStep(node =>
            {
                var pipelineBody = new PipelineBody(inputType, outputType, body);

                node.Bodies.Add(pipelineBody);
            });
        }

        public IPipelineLazyValue<PipelineInput> CreateInput([NotNull] string name, Action<PipelineInputBuilder> closure = null)
        {
            var lazyInput = new PipelineLazyValue<PipelineInput>();

            _stepCollection.AddClosureBuilderStep(node =>
            {
                var builder = new PipelineInputBuilder();

                closure?.Invoke(builder);

                var input = builder.Build(node, node.NextAvailableInputId(), name);

                node.Inputs.Add(input);

                lazyInput.SetLazyValue(input.Id);
            });

            return lazyInput;
        }

        public IPipelineLazyValue<PipelineOutput> CreateOutput([NotNull] string name, Action<PipelineOutputBuilder> closure = null)
        {
            var lazyOutput = new PipelineLazyValue<PipelineOutput>();

            _stepCollection.AddClosureBuilderStep(node =>
            {
                var builder = new PipelineOutputBuilder();

                closure?.Invoke(builder);

                var output = builder.Build(node, node.NextAvailableOutputId(), name);

                node.Outputs.Add(output);

                lazyOutput.SetLazyValue(output.Id);
            });

            return lazyOutput;
        }

        internal PipelineNode Build(PipelineNodeId id)
        {
            var node = new PipelineNode(id);

            _stepCollection.Apply(node);

            return node;
        }
    }
}

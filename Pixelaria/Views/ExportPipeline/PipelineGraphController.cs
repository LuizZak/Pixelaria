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

using JetBrains.Annotations;
using PixPipelineGraph;

namespace Pixelaria.Views.ExportPipeline
{
    public class PipelineGraphController
    {
        private readonly PipelineGraph _pipelineGraph;

        public PipelineGraphController(PipelineGraph pipelineGraph)
        {
            _pipelineGraph = pipelineGraph;
        }

        public PipelineNodeId CreateNode(PipelineNodeDescriptor descriptor)
        {
            return _pipelineGraph.CreateNode(node => { CreateNode(descriptor, node); });
        }

        private static void CreateNode([NotNull] PipelineNodeDescriptor descriptor, [NotNull] PipelineNodeBuilder node)
        {
            node.SetTitle(descriptor.Title);

            foreach (var input in descriptor.Inputs)
            {
                node.CreateInput(input.Title, builder =>
                {
                    foreach (var inputType in input.InputTypes)
                    {
                        builder.AddInputType(inputType);
                    }
                });
            }

            foreach (var output in descriptor.Outputs)
            {
                node.CreateOutput(output.Title, builder => { builder.SetOutputType(output.OutputType); });
            }

            foreach (var body in descriptor.Bodies)
            {
                node.AddBody(body);
            }
        }
    }
}
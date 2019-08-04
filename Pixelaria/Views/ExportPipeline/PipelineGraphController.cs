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
            return _pipelineGraph.CreateNode(node =>
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
                    node.CreateOutput(output.Title, builder =>
                    {
                        builder.SetOutputType(output.OutputType);
                    });
                }

                foreach (var body in descriptor.Bodies)
                {
                    node.AddBody(body.InputType, body.OutputType, body.Body);
                }
            });
        }
    }

    public class PipelineNodeDescriptor
    {
        public string Title { get; set; }

        public List<PipelineInputDescriptor> Inputs { get; } = new List<PipelineInputDescriptor>();
        public List<PipelineOutputDescriptor> Outputs { get; } = new List<PipelineOutputDescriptor>();

        public List<PipelineBody> Bodies { get; } = new List<PipelineBody>();
    }

    public class PipelineInputDescriptor
    {
        [NotNull]
        public string Title { get; set; }

        public List<Type> InputTypes { get; } = new List<Type>();

        public PipelineInputDescriptor([NotNull] string title)
        {
            Title = title;
        }
    }

    public class PipelineOutputDescriptor
    {
        [NotNull]
        public string Title { get; set; }

        public Type OutputType { get; set; }

        public PipelineOutputDescriptor([NotNull] string title)
        {
            Title = title;
        }
    }
}
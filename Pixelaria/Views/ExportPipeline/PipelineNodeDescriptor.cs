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
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixPipelineGraph;

namespace Pixelaria.Views.ExportPipeline
{
    public class PipelineNodeDescriptor
    {
        public PipelineNodeKind NodeKind { get; set; }

        [CanBeNull]
        public Bitmap Icon { get; set; }

        public string Title { get; set; } = "";

        [CanBeNull]
        public string BodyText { get; set; }

        public List<PipelineInputDescriptor> Inputs { get; } = new List<PipelineInputDescriptor>();
        public List<PipelineOutputDescriptor> Outputs { get; } = new List<PipelineOutputDescriptor>();

        public PipelineBody Body { get; set; }

        public void CreateIn([NotNull] PipelineNodeBuilder nodeBuilder)
        {
            nodeBuilder.SetTitle(Title);
            nodeBuilder.AddMetadataEntry(PipelineMetadataKeys.IconBitmap, Icon);
            nodeBuilder.SetBody(Body);

            foreach (var input in Inputs)
            {
                nodeBuilder.CreateInput(input.Title, builder => input.CreateIn(builder));
            }
            foreach (var output in Outputs)
            {
                nodeBuilder.CreateOutput(output.Title, builder => output.CreateIn(builder));
            }
        }

        /// <summary>
        /// Creates a view out of this pipeline node descriptor.
        ///
        /// <see cref="PipelineNodeId"/> featured in this structure are always equal to <see cref="Guid.Empty"/>
        /// </summary>
        public IPipelineNodeView CreateView()
        {
            return new InternalNodeView(this);
        }

        private class InternalNodeView : IPipelineNodeView
        {
            private readonly PipelineNodeDescriptor _nodeDescriptor;

            public PipelineNodeId NodeId => new PipelineNodeId(Guid.Empty);
            public string Title => _nodeDescriptor.Title;
            public PipelineNodeKind NodeKind => _nodeDescriptor.NodeKind;
            public PipelineBodyId BodyId => _nodeDescriptor.Body.Id;
            public IPipelineMetadata PipelineMetadata { get; } = new PipelineMetadata();

            public IReadOnlyList<IPipelineInput> Inputs => _nodeDescriptor.Inputs.Select((input, index) => new InternalInput(input, index)).ToList();
            public IReadOnlyList<IPipelineOutput> Outputs => _nodeDescriptor.Outputs.Select((output, index) => new InternalOutput(output, index)).ToList();

            public InternalNodeView(PipelineNodeDescriptor nodeDescriptor)
            {
                _nodeDescriptor = nodeDescriptor;
                PipelineMetadata.SetValue(PipelineMetadataKeys.PipelineStepBodyText, _nodeDescriptor.BodyText);
            }
        }

        private class InternalInput : IPipelineInput
        {
            private readonly PipelineInputDescriptor _descriptor;
            private readonly int _index;
            
            public PipelineNodeId NodeId => new PipelineNodeId(Guid.Empty);
            public string Name => _descriptor.Title;

            public PipelineInput Id => new PipelineInput(NodeId, _index);
            public Type DataType => _descriptor.InputType;
            
            public InternalInput(PipelineInputDescriptor descriptor, int index)
            {
                _descriptor = descriptor;
                _index = index;
            }

            public IPipelineMetadata GetMetadata()
            {
                return new PipelineMetadata();
            }
        }

        private class InternalOutput : IPipelineOutput
        {
            private readonly PipelineOutputDescriptor _descriptor;
            private readonly int _index;

            public PipelineNodeId NodeId => new PipelineNodeId(Guid.Empty);
            public string Name => _descriptor.Title;

            public PipelineOutput Id => new PipelineOutput(NodeId, _index);
            public Type DataType => _descriptor.OutputType;

            public InternalOutput(PipelineOutputDescriptor descriptor, int index)
            {
                _descriptor = descriptor;
                _index = index;
            }

            public IPipelineMetadata GetMetadata()
            {
                return new PipelineMetadata();
            }
        }
    }

    public class PipelineInputDescriptor
    {
        [NotNull]
        public string Title { get; set; }

        public Type InputType { get; set; }

        public PipelineInputDescriptor([NotNull] string title, [NotNull] Type inputType)
        {
            Title = title;
            InputType = inputType;
        }

        public void CreateIn([NotNull] PipelineInputBuilder inputBuilder)
        {
            inputBuilder.SetName(Title);
            inputBuilder.SetInputType(InputType);
        }
    }

    public class PipelineOutputDescriptor
    {
        [NotNull]
        public string Title { get; set; }

        public Type OutputType { get; set; }

        public PipelineOutputDescriptor([NotNull] string title, [NotNull] Type outputType)
        {
            Title = title;
            OutputType = outputType;
        }

        public void CreateIn([NotNull] PipelineOutputBuilder outputBuilder)
        {
            outputBuilder.SetName(Title);
            outputBuilder.SetOutputType(OutputType);
        }
    }
}
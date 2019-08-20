﻿/*
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
using Pixelaria.Views.ExportPipeline;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline
{
    class DefaultPipelineGraphNodeProvider : IPipelineGraphNodeProvider
    {
        private readonly List<PipelineNodeDescriptor> _nodeDescriptors = new List<PipelineNodeDescriptor>();

        public IReadOnlyList<PipelineNodeDescriptor> NodeDescriptors => _nodeDescriptors;

        public DefaultPipelineGraphNodeProvider()
        {
            CreateDefaultNodes();
        }

        private void CreateDefaultNodes()
        {
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                Title = "Animation Joiner",
                Inputs = {new PipelineInputDescriptor("animations")},
                Outputs = {new PipelineOutputDescriptor("animations")}
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                Title = "Bitmap Import",
                Outputs = {new PipelineOutputDescriptor("bitmap") {OutputType = typeof(Bitmap)}}
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                Title = "Bitmap Preview",
                Inputs = { new PipelineInputDescriptor("bitmap") { InputType = typeof(Bitmap) } },
                Outputs = { new PipelineOutputDescriptor("bitmap") { OutputType = typeof(Bitmap) } }
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                Title = "File Export",
                Inputs = { new PipelineInputDescriptor("bitmap") { InputType = typeof(Bitmap) } }
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                Title = "",
                Inputs = { new PipelineInputDescriptor("bitmap") { InputType = typeof(Bitmap) } }
            });
        }

        public PipelineBody GetBody(PipelineBodyId id)
        {
            return null;
        }

        public bool CanCreateNode(PipelineNodeKind kind)
        {
            return false;
        }

        public void CreateNode(PipelineNodeKind nodeKind, PipelineNodeBuilder builder)
        {

        }
    }
}

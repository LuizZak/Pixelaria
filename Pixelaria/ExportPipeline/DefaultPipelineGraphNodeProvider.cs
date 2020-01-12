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
using Pixelaria.Filters;
using Pixelaria.Properties;
using Pixelaria.Utils;
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
                NodeKind = PipelineNodeKinds.AnimationJoiner,
                Title = "Animation Joiner",
                Icon = Resources.anim_icon,
                Inputs = {new PipelineInputDescriptor("animations")},
                Outputs = {new PipelineOutputDescriptor("animations")}
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.BitmapImport,
                Title = "Bitmap Import",
                Outputs = {new PipelineOutputDescriptor("bitmap") {OutputType = typeof(Bitmap)}}
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.BitmapPreview,
                Title = "Bitmap Preview",
                Inputs = { new PipelineInputDescriptor("bitmap") { InputType = typeof(Bitmap) } },
                Outputs = { new PipelineOutputDescriptor("bitmap") { OutputType = typeof(Bitmap) } }
            });
            _nodeDescriptors.Add(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.FileExport,
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
            return _nodeDescriptors.FirstOrDefault(entry => entry.Body?.Id == id)?.Body;
        }

        public bool CanCreateNode(PipelineNodeKind kind)
        {
            return _nodeDescriptors.Any(node => node.NodeKind == kind);
        }

        public bool CreateNode(PipelineNodeKind nodeKind, PipelineNodeBuilder builder)
        {
            var nodeDescriptor = _nodeDescriptors.FirstOrDefault(descriptor => descriptor.NodeKind == nodeKind);
            if (nodeDescriptor == null)
                return false;

            nodeDescriptor.CreateIn(builder);

            return true;
        }
    }

    public static class PipelineNodeKinds
    {
        public static PipelineNodeKind AnimationJoiner { get; } = new PipelineNodeKind("animationJoiner");
        public static PipelineNodeKind BitmapImport { get; } = new PipelineNodeKind("bitmapImport");
        public static PipelineNodeKind BitmapPreview { get; } = new PipelineNodeKind("bitmapPreview");
        public static PipelineNodeKind FileExport { get; } = new PipelineNodeKind("fileExport");
    }
}

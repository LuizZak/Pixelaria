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
using JetBrains.Annotations;
using PixPipelineGraph;

namespace Pixelaria.Views.ExportPipeline
{
    public class PipelineNodeDescriptor
    {
        public Bitmap Icon { get; set; }
        public string Title { get; set; }
        public string BodyText { get; set; }

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
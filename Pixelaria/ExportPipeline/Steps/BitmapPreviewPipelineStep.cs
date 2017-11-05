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
using System.Reactive.Linq;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline.Inputs;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps.Abstract;

namespace Pixelaria.ExportPipeline.Steps
{
    // TODO: Make a generic of this guy for any input/output.
    
    /// <summary>
    /// A pipeline step that allows inspecting bitmaps that flow through it
    /// </summary>
    internal class BitmapPreviewPipelineStep: IPipelineStep
    {
        public string Name { get; set; } = "Bitmap Preview";
        public IReadOnlyList<IPipelineInput> Input { get; }
        public IReadOnlyList<IPipelineOutput> Output { get; }

        /// <summary>
        /// Gets or sets the callback to fire whenever a bitmap flows through this pipeline node.
        /// </summary>
        [CanBeNull]
        public Action<Bitmap> OnReceive { get; set; }

        public BitmapPreviewPipelineStep()
        {
            var input = new PipelineBitmapInput(this);

            var obs = 
                input.AnyConnection()
                    .Do(bitmap => OnReceive?.Invoke(bitmap));

            var output = new PipelineBitmapOutput(this, obs);

            Input = new[]
            {
                input
            };

            Output = new[]
            {
                output
            };
        }

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

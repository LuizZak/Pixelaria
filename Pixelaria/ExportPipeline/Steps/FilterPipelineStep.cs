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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline.Inputs;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.Filters;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// Pipeline step that applies a filter onto a Bitmap and passes a copy of it
    /// forward.
    /// </summary>
    public sealed class FilterPipelineStep : AbstractPipelineStep
    {
        [NotNull]
        public IFilter Filter;

        public override string Name => Filter.Name;
        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public FilterPipelineStep([NotNull] IFilter filter)
        {
            Filter = filter;
            Input = new IPipelineInput[] { new PipelineBitmapInput(this) };
            
            var connections =
                Input[0].Connections
                    .Select(o => o.GetConnection()).ToObservable()
                    .SelectMany(o => o)
                    .Repeat();

            var source = 
                connections
                    .OfType<Bitmap>()
                    .Select(bitmap =>
                    {
                        // Clone before applying filter
                        var bit = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

                        filter.ApplyToBitmap(bit);

                        return bit;
                    });
            
            Output = new IPipelineOutput[] {new PipelineBitmapOutput(this, source)};
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}
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
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Pixelaria.Data;
using Pixelaria.ExportPipeline.Inputs;
using Pixelaria.ExportPipeline.Outputs;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// Joins all connected animation sources into one Animation array
    /// </summary>
    public sealed class AnimationJoinerStep : AbstractPipelineStep
    {
        public override string Name { get; } = "Animation Joiner";

        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public AnimationJoinerStep()
        {
            var animationInput = new AnimationInput(this);
            Input = new IPipelineInput[] { animationInput };

            var connections =
                animationInput
                    .ConnectionsObservable
                    .SelectMany(o => o.GetConnection());

            var source =
                connections
                    .ObserveOn(NewThreadScheduler.Default)
                    .OfType<Animation>()
                    .ToArray();

            Output = new IPipelineOutput[] { new AnimationsOutput(this, source) };
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}
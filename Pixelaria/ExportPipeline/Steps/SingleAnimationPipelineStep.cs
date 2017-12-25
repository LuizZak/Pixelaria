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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.ExportPipeline.Steps.Abstract;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// A pipeline step that feeds a single animation down to consumers
    /// </summary>
    public class SingleAnimationPipelineStep : AbstractPipelineStart
    {
        public Animation Animation { get; }

        public override string Name => Animation.Name;
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public SingleAnimationPipelineStep(Animation animation)
        {
            var output = new BehaviorSubject<Animation>(animation);

            Animation = animation;
            
            Output = new[] { new PipelineOutput(this, output) };
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }

        public class PipelineOutput : IPipelineOutput
        {
            private readonly BehaviorSubject<Animation> _output;

            public string Name { get; } = "Animation";
            public IPipelineNode Node { get; }
            public Type DataType => typeof(Animation);

            public PipelineOutput([NotNull] IPipelineNode step, BehaviorSubject<Animation> output)
            {
                Node = step;
                _output = output;
            }

            public IObservable<object> GetObservable()
            {
                return _output.Take(1);
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }
        }
    }
}
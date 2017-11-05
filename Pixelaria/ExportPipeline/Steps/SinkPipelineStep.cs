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
using System.Reactive.Disposables;
using Pixelaria.ExportPipeline.Inputs.Abstract;
using Pixelaria.Utils;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// A pipeline step that simply subscribes to all connected inputs and consumes
    /// their outputs.
    /// 
    /// Can be used to force a pipeline to produce items (for e.g. testing/debugging/visualization/etc).
    /// </summary>
    internal sealed class SinkPipelineStep : IPipelineEnd
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();

        private readonly GenericPipelineInput<object> _input;
        public string Name => "Sink";
        public IReadOnlyList<IPipelineInput> Input { get; }

        public SinkPipelineStep()
        {
            _input = new GenericPipelineInput<object>(this, "Input");

            Input = new[] {_input};
        }

        public void Dispose()
        {
            _disposeBag.Dispose();
        }

        public void Begin()
        {
            _input
                .AnyConnection()
                .Subscribe()
                .AddToDisposable(_disposeBag);
        }
        
        public IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}

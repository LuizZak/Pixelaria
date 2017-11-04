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
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Pixelaria.Controllers.Exporters;
using Pixelaria.Data;
using Pixelaria.ExportPipeline.Inputs;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps.Abstract;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// A sprite sheet generation pipeline step.
    /// 
    /// From a list of animations and an accompanying sheet export settings struct,
    /// generates and passes forward one animation sheet.
    /// </summary>
    public sealed class SpriteSheetGenerationPipelineStep : AbstractPipelineStep
    {
        public override string Name => "Sprite Sheet Generation";
        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public AnimationsInput AnimationsInput { get; }
        public SheetSettingsInput SheetSettingsInput { get; }

        public SpriteSheetGenerationPipelineStep()
        {
            // Create a stream that takes an animation and outputs a sprite sheet
            var exporter = new DefaultPngExporter();

            AnimationsInput = new AnimationsInput(this);
            SheetSettingsInput = new SheetSettingsInput(this);
            Input = new IPipelineInput[]
            {
                AnimationsInput,
                SheetSettingsInput
            };
            
            var animConnections = AnimationsInput.AnyConnection();
            var settingsConnections = SheetSettingsInput.AnyConnection();

            var source =
                animConnections
                    .WithLatestFrom(settingsConnections.Take(1), (animations, settings) => (animations, settings))
                    .ObserveOn(NewThreadScheduler.Default)
                    .SelectMany((tuple, i, cancellation) =>
                    {
                        var provider = new BasicAnimationProvider(tuple.Item1.Cast<IAnimation>().ToArray(), tuple.Item2, "Sheet");

                        return exporter.ExportBundleSheet(provider, cancellation);
                    });

            Output = new[] {new BundleSheetExportOutput(this, source)};
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}
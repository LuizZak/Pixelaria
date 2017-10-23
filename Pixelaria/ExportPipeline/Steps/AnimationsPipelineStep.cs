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
using System.Reactive.Subjects;
using Pixelaria.Data;
using Pixelaria.ExportPipeline.Outputs;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// A pipeline step that feeds an array of animations.
    /// 
    /// Can be used in place of many AnimationStep + one AnimationJoinerStep groups.
    /// </summary>
    public class AnimationsPipelineStep : AbstractPipelineStep
    {
        private string _stepBodyText = "";

        public Animation[] Animations { get; }

        public override string Name => "Animations";
        public override IReadOnlyList<IPipelineInput> Input { get; } = new IPipelineInput[0];
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public AnimationsPipelineStep(Animation[] animations)
        {
            var output = new BehaviorSubject<Animation[]>(animations);

            Animations = animations;

            Output = new[] {new AnimationsOutput(this, output)};

            CreateStepBodyText();
        }

        private void CreateStepBodyText()
        {
            const int displayMax = 7;

            string text = "";
            foreach (var animation in Animations.Take(displayMax))
            {
                text += animation.Name + "\n";
            }
            if (Animations.Length > displayMax)
                text += $"...+{Animations.Length - displayMax} other(s)";

            text = text.Trim();

            _stepBodyText = text;
        }

        public override IPipelineMetadata GetMetadata()
        {
            // TODO: Provide the array of animations as its own PipelineMetadataKeys key and allow the
            // node view to come up with the proper display text
            return new PipelineMetadata(
                new Dictionary<string, object> {{PipelineMetadataKeys.PipelineStepBodyText, _stepBodyText}});
        }
    }
}

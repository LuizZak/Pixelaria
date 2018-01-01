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

using System.Drawing;
using System.IO;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps;
using Pixelaria.Filters;
using Pixelaria.Utils;
using PixelariaTests.Generators;

namespace PixelariaTests.ExportPipeline.Steps
{
    [TestClass]
    public class FilterPipelineStepTests
    {
        [TestMethod]
        [Timeout(1000)]
        public async Task TestFilterReflection()
        {
            var filter = new MockFilter();

            var filterSteps = new FilterPipelineStep<MockFilter>(filter);

            // Link inputs
            var bitmap = new StaticPipelineOutput<Bitmap>(BitmapGenerator.GenerateRandomBitmap(64, 64, 1), "Bitmap");
            var param1 = new StaticPipelineOutput<float>(5, "Param 1");
            var param2 = new StaticPipelineOutput<int>(10, "Param 2");
            var param3 = new StaticPipelineOutput<bool>(false, "Param 3");

            filterSteps.Input[0].Connect(bitmap);

            filterSteps.Input[1].Connect(param1);
            filterSteps.Input[2].Connect(param2);
            filterSteps.Input[3].Connect(param3);
            
            await filterSteps.Output[0].GetObservable();

            Assert.IsTrue(ImageUtilities.ImagesAreIdentical(bitmap.Value, filter.ReceivedBitmap));
            
            Assert.AreEqual(filter.Param1, param1.Value);
            Assert.AreEqual(filter.Param2, param2.Value);
            Assert.AreEqual(filter.Param3, param3.Value);
        }

        [TestMethod]
        public void TestComplexPipelineFlow()
        {
            var bundle = new Bundle("abc");
            var anim1 = new Animation("Anim 1", 48, 48);
            var controller = new AnimationController(bundle, anim1);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var anim2 = new Animation("Anim 2", 64, 64);
            controller = new AnimationController(bundle, anim2);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var anim3 = new Animation("Anim 3", 80, 80);
            controller = new AnimationController(bundle, anim3);

            controller.CreateFrame();
            controller.CreateFrame();
            controller.CreateFrame();

            var animSteps = new[]
            {
                new SingleAnimationPipelineStep(anim1),
                new SingleAnimationPipelineStep(anim2),
                new SingleAnimationPipelineStep(anim3)
            };

            var animJoinerStep = new AnimationJoinerStep();

            var exportSettings = new AnimationSheetExportSettings
            {
                FavorRatioOverArea = true,
                AllowUnorderedFrames = true,
                ExportJson = false,
                ForceMinimumDimensions = false,
                ForcePowerOfTwoDimensions = false,
                HighPrecisionAreaMatching = false,
                ReuseIdenticalFramesArea = false
            };

            var sheetSettingsOutput = new StaticPipelineOutput<AnimationSheetExportSettings>(exportSettings, "Sheet Export Settings");

            var sheetStep = new SpriteSheetGenerationPipelineStep();

            var finalStep = new FileExportPipelineStep();

            // Link stuff
            foreach (var step in animSteps)
            {
                step.ConnectTo(animJoinerStep);
            }

            animJoinerStep.ConnectTo(sheetStep);
            sheetStep.SheetSettingsInput.Connect(sheetSettingsOutput);

            sheetStep.ConnectTo(finalStep);

            finalStep.Begin();
        }

        internal class MockFilter : IFilter
        {
            public bool Modifying { get; } = true;
            public string Name { get; } = "Mock Filter";
            public int Version { get; } = 1;

            public float Param1 { get; set; }
            public int Param2 { get; set; }
            public bool Param3 { get; set; }

            public Bitmap ReceivedBitmap { get; set; }
            public float ReceivedParam1 { get; set; }
            public int ReceivedParam2 { get; set; }
            public bool ReceivedParam3 { get; set; }

            public void ApplyToBitmap(Bitmap target)
            {
                ReceivedBitmap = target;
                ReceivedParam1 = Param1;
                ReceivedParam2 = Param2;
                ReceivedParam3 = Param3;
            }

            public PropertyInfo[] InspectableProperties()
            {
                return new[]
                {
                    GetType().GetProperty("Param1"),
                    GetType().GetProperty("Param2"),
                    GetType().GetProperty("Param3")
                };
            }

            public void SaveToStream(Stream stream)
            {
                throw new System.NotImplementedException();
            }

            public void LoadFromStream(Stream stream, int version)
            {
                throw new System.NotImplementedException();
            }

            public bool Equals(IFilter filter)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}

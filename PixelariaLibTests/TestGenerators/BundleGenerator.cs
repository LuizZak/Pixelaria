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
using PixelariaLib.Controllers.DataControllers;
using PixelariaLib.Data;

namespace PixelariaLibTests.TestGenerators
{
    /// <summary>
    /// Contains methods related to bundle generation used in unit tests
    /// </summary>
    public static class BundleGenerator
    {
        /// <summary>
        /// Generates a bundle with all features used, with a seed used to generate the randomicity. The bundles and their respective
        /// inner objects generated with the same seed will be guaranteed to be considered equal by the respective equality unit tests
        /// </summary>
        /// <param name="seed">An integer to utilize as a seed for the random number generator used to fill in the bundle</param>
        /// <param name="animationSheetCount">Count of animation sheets to generate</param>
        /// <param name="animationsPerSheet">Count of animations per animation sheet</param>
        /// <param name="frameCount">Number of frames to create per animation. If null, a random value between 2-5 based on the provided seed is used.</param>
        /// <returns>A Bundle filled with randomized objects</returns>
        public static Bundle GenerateTestBundle(int seed, int animationSheetCount = 5, int animationsPerSheet = 5, int? frameCount = null)
        {
            var r = new Random(seed);

            var bundle = new Bundle("Bundle" + r.Next());

            var layersCount = 0;

            for (int i = 0; i < animationSheetCount; i++)
            {
                bundle.AddAnimationSheet(AnimationSheetGenerator.GenerateAnimationSheet("Sheet" + i, animationsPerSheet, r.Next(10, 128), r.Next(10, 128), frameCount ?? r.Next(2, 5), r.Next()));

                // Create some dummy layers
                foreach (var animation in bundle.Animations)
                {
                    foreach (var frame in animation.Frames)
                    {
                        var fr = (Frame)frame;

                        var controller = new FrameController(fr);

                        for (int j = 0; j < r.Next(1, 2); j++)
                        {
                            controller.CreateLayer(FrameGenerator.GenerateRandomBitmap(fr.Width, fr.Height, seed + j)).Name = $"Layer {++layersCount}";
                        }
                    }
                }

                // Add some repeated frames to a few animations
                if (frameCount == null && i % 2 == 0)
                {
                    if (bundle.AnimationSheets[i].AnimationCount > 0)
                    {
                        var controller1 = new AnimationController(bundle, bundle.AnimationSheets[i].Animations[0]);

                        controller1.CreateFrame();
                        controller1.CreateFrame();
                        controller1.CreateFrame();
                    }

                    if (bundle.AnimationSheets[i].AnimationCount > 1)
                    {
                        var controller2 = new AnimationController(bundle, bundle.AnimationSheets[i].Animations[1]);
                        controller2.GetFrameController(controller2.CreateFrame()).RandomizeBitmap(1);
                        controller2.GetFrameController(controller2.CreateFrame()).RandomizeBitmap(1);
                        controller2.GetFrameController(controller2.CreateFrame()).RandomizeBitmap(1);
                    }
                }
            }

            return bundle;
        }
    }
}
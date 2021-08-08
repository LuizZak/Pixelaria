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
using PixLib.Controllers.DataControllers;
using PixLib.Data;

namespace PixLibTests.Generators
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
        /// <returns>A Bundle filled with randomized objects</returns>
        public static Bundle GenerateTestBundle(int seed)
        {
            var r = new Random(seed);

            var bundle = new Bundle("Bundle" + r.Next());

            var layersCount = 0;

            for (int i = 0; i < 5; i++)
            {
                bundle.AddAnimationSheet(AnimationSheetGenerator.GenerateAnimationSheet("Sheet" + i, 5, r.Next(10, 128), r.Next(10, 128), r.Next(2, 5), r.Next()));

                // Create some dummy layers
                foreach (var animation in bundle.Animations)
                {
                    foreach (var frame in animation.Frames)
                    {
                        var fr = (Frame)frame;

                        var controller = new FrameController(fr);

                        for (int j = 0; j < r.Next(1, 2); j++)
                        {
                            controller.CreateLayer(BitmapGenerator.GenerateRandomBitmap(fr.Width, fr.Height, seed + j)).Name = $"Layer {++layersCount}";
                        }
                    }
                }

                // Add some repeated frames to a few animations
                if (i % 2 != 0) 
                    continue;

                var controller1 = new AnimationController(bundle, bundle.AnimationSheets[i].Animations[0]);
                var controller2 = new AnimationController(bundle, bundle.AnimationSheets[i].Animations[1]);

                controller1.CreateFrame();
                controller1.CreateFrame();
                controller1.CreateFrame();

                controller2.GetFrameController(controller2.CreateFrame()).RandomizeBitmap(1);
                controller2.GetFrameController(controller2.CreateFrame()).RandomizeBitmap(1);
                controller2.GetFrameController(controller2.CreateFrame()).RandomizeBitmap(1);
            }

            return bundle;
        }
    }
}
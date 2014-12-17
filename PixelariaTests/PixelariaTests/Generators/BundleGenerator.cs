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
using Pixelaria.Data;

namespace PixelariaTests.PixelariaTests.Generators
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
            Random r = new Random(seed);

            Bundle bundle = new Bundle("Bundle" + r.Next());

            for (int i = 0; i < 5; i++)
            {
                bundle.AddAnimationSheet(AnimationSheetGenerator.GenerateAnimationSheet("Sheet" + i, 5, r.Next(10, 128), r.Next(10, 128), r.Next(2, 5), r.Next()));

                // Add some repeated frames to a few animations
                if (i % 2 == 0)
                {
                    bundle.AnimationSheets[0].Animations[0].CreateFrame();
                    bundle.AnimationSheets[0].Animations[0].CreateFrame();
                    bundle.AnimationSheets[0].Animations[0].CreateFrame();

                    bundle.AnimationSheets[1].Animations[2].CreateFrame().RandomizeBitmap(1);
                    bundle.AnimationSheets[1].Animations[2].CreateFrame().RandomizeBitmap(1);
                    bundle.AnimationSheets[1].Animations[2].CreateFrame().RandomizeBitmap(1);
                }
            }

            return bundle;
        }
    }
}
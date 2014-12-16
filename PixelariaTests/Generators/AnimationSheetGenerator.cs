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
using Pixelaria.Data;

namespace Pixelaria.PixelariaTests.Generators
{
    /// <summary>
    /// Contains methods related to animation sheet generation used in unit tests
    /// </summary>
    public static class AnimationSheetGenerator
    {
        /// <summary>
        /// Generates an AnimationSheet with a given set of parameters.
        /// </summary>
        /// <param name="name">The name of the animation sheet to generate</param>
        /// <param name="animationCount">The number of animations to generate in the sheet</param>
        /// <param name="animationWidth">The width of the animations to generate</param>
        /// <param name="animationHeight">The height of the animations to generate</param>
        /// <param name="frameCount">The number of frames to add to each animation generated</param>
        /// <param name="seed">The seed for the animations' frames, used to seed the random number generator that will generate each of the frame's contents</param>
        /// <returns>An animation sheet with the passed parameters</returns>
        public static AnimationSheet GenerateAnimationSheet(string name, int animationCount, int animationWidth, int animationHeight, int frameCount, int seed = -1)
        {
            AnimationSheet sheet = new AnimationSheet(name);

            for (int i = 0; i < animationCount; i++)
            {
                sheet.AddAnimation(AnimationGenerator.GenerateAnimation(name + "Animation" + i, animationWidth, animationHeight, frameCount, seed == -1 ? seed : seed + i * frameCount));
            }

            return sheet;
        }

        /// <summary>
        /// Generates a default AnimationExportSettings object to be used when exporting animation sheets
        /// </summary>
        /// <returns>A default AnimationExportSettings object to be used when exporting animation sheets</returns>
        public static AnimationExportSettings GenerateDefaultAnimationExportSettings()
        {
            return new AnimationExportSettings
            {
                FavorRatioOverArea = false,
                ForcePowerOfTwoDimensions = false,
                ForceMinimumDimensions = true,
                ReuseIdenticalFramesArea = true,
                HighPrecisionAreaMatching = false,
                AllowUnorderedFrames = true,
                UseUniformGrid = false,
                UsePaddingOnXml = true,
                ExportXml = true,
                XPadding = 0,
                YPadding = 0
            };
        }

        /// <summary>
        /// Returns an array of value permutations for the AnimationExportSettings struct to use in tests
        /// </summary>
        /// <returns>An array of value permutations for the AnimationExportSettings struct to use in tests</returns>
        public static AnimationExportSettings[] GetExportSettingsPermutations()
        {
            List<AnimationExportSettings> settingsList = new List<AnimationExportSettings>
            {
                GenerateDefaultAnimationExportSettings(),
                new AnimationExportSettings
                {
                    FavorRatioOverArea = true,
                    ForcePowerOfTwoDimensions = false,
                    ForceMinimumDimensions = true,
                    ReuseIdenticalFramesArea = true,
                    HighPrecisionAreaMatching = false,
                    AllowUnorderedFrames = true,
                    UseUniformGrid = false,
                    UsePaddingOnXml = false,
                    ExportXml = true,
                    XPadding = 0,
                    YPadding = 0
                },
                new AnimationExportSettings
                {
                    FavorRatioOverArea = true,
                    ForcePowerOfTwoDimensions = false,
                    ForceMinimumDimensions = true,
                    ReuseIdenticalFramesArea = false,
                    HighPrecisionAreaMatching = false,
                    AllowUnorderedFrames = true,
                    UseUniformGrid = false,
                    UsePaddingOnXml = false,
                    ExportXml = true,
                    XPadding = 0,
                    YPadding = 0
                },
                new AnimationExportSettings
                {
                    FavorRatioOverArea = false,
                    ForcePowerOfTwoDimensions = true,
                    ForceMinimumDimensions = false,
                    ReuseIdenticalFramesArea = false,
                    HighPrecisionAreaMatching = false,
                    AllowUnorderedFrames = false,
                    UseUniformGrid = true,
                    UsePaddingOnXml = false,
                    ExportXml = true,
                    XPadding = 2,
                    YPadding = 1
                },
                new AnimationExportSettings
                {
                    FavorRatioOverArea = true,
                    ForcePowerOfTwoDimensions = true,
                    ForceMinimumDimensions = false,
                    ReuseIdenticalFramesArea = false,
                    HighPrecisionAreaMatching = true,
                    AllowUnorderedFrames = true,
                    UseUniformGrid = true,
                    UsePaddingOnXml = false,
                    ExportXml = true,
                    XPadding = 2,
                    YPadding = 1
                },
                new AnimationExportSettings
                {
                    FavorRatioOverArea = true,
                    ForcePowerOfTwoDimensions = false,
                    ForceMinimumDimensions = false,
                    ReuseIdenticalFramesArea = false,
                    HighPrecisionAreaMatching = true,
                    AllowUnorderedFrames = true,
                    UseUniformGrid = false,
                    UsePaddingOnXml = false,
                    ExportXml = true,
                    XPadding = 15,
                    YPadding = 10
                }
            };

            return settingsList.ToArray();
        }
    }
}
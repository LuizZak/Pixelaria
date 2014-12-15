using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixelaria.Data;

namespace PixelariaTests.Generators
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
            AnimationExportSettings exportSettings;

            exportSettings.FavorRatioOverArea = false;
            exportSettings.ForcePowerOfTwoDimensions = false;
            exportSettings.ForceMinimumDimensions = true;
            exportSettings.ReuseIdenticalFramesArea = true;
            exportSettings.HighPrecisionAreaMatching = false;
            exportSettings.AllowUnorderedFrames = true;
            exportSettings.UseUniformGrid = false;
            exportSettings.UsePaddingOnXml = true;
            exportSettings.ExportXml = true;
            exportSettings.XPadding = 0;
            exportSettings.YPadding = 0;

            return exportSettings;
        }
    }
}
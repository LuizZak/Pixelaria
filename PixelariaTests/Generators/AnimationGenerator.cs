using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pixelaria.Data;

namespace PixelariaTests.Generators
{
    /// <summary>
    /// Contains methods related to animation generation used in unit tests
    /// </summary>
    public static class AnimationGenerator
    {
        /// <summary>
        /// Generates an Animation with a given set of parameters.
        /// </summary>
        /// <param name="name">The name of the animation to generate</param>
        /// <param name="width">The width of the animation to generate</param>
        /// <param name="height">The height of the animation to generate</param>
        /// <param name="frameCount">The number of frames to add to the animation</param>
        /// <param name="seed">The seed for the animation's frames, used to seed the random number generator that will generate each of the frame's contents</param>
        /// <returns>An animation with the passed parameters</returns>
        public static Animation GenerateAnimation(string name, int width, int height, int frameCount, int seed = -1)
        {
            Animation anim = new Animation(name, width, height);

            for (int i = 0; i < frameCount; i++)
            {
                anim.CreateFrame().SetFrameBitmap(FrameGenerator.GenerateFrameImage(width, height, (seed == -1 ? seed : seed + i)));
            }

            return anim;
        }
    }
}
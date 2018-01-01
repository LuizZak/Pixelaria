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

using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Data.Factories;

namespace PixelariaTests.Generators
{
    /// <summary>
    /// Contains methods related to image generation used in unit tests
    /// </summary>
    public static class FrameGenerator
    {
        /// <summary>
        /// Next available unique ID to use in methods generating frames in this static class
        /// </summary>
        public static int NextId = 1;

        /// <summary>
        /// Generates a frame with a given set of parameters.
        /// The seed is used to randomize the frame, and any call with the same width, height and seed will generate the same frame
        /// </summary>
        /// <param name="width">The width of the frame to generate</param>
        /// <param name="height">The height of the frame to generate</param>
        /// <param name="seed">The seed for the frame's image, used to seed the random number generator that will generate the image contents</param>
        /// <param name="layerCount">The number of layers to create on the frame</param>
        /// <returns>A frame with the passed parameters</returns>
        public static Frame GenerateRandomFrame(int width, int height, int seed = -1, int layerCount = 3)
        {
            var frame = new Frame(null, width, height, false)
            {
                ID = NextId++
            };

            var controller = new FrameController(frame);

            frame.SetFrameBitmap(BitmapGenerator.GenerateRandomBitmap(width, height, seed));

            for (int i = 1; i < layerCount; i++)
            {
                controller.CreateLayer(BitmapGenerator.GenerateRandomBitmap(width, height, seed + 1));
            }

            return frame;
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Test frame ID generator that generates sequential numbers
    /// </summary>
    public class FrameIdGenerator : IFrameIdGenerator
    {
        private int _nextId;
        
        public int GetNextUniqueFrameId()
        {
            return ++_nextId;
        }
    }
}
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

using PixLib.Data;
using PixLib.Filters;

namespace PixLib.Algorithms.FrameOperations
{
    public class FlipHorizontalFrameOperation : IFrameOperation
    {
        public bool CanApply(IFrame frame)
        {
            return frame is Frame;
        }

        public void Apply(IFrame fr)
        {
            var frame = (Frame) fr;

            var flipFilter = new ScaleFilter
            {
                ScaleX = -1,
                ScaleY = 1,
                PixelQuality = true,
                Centered = true
            };
            
            // Flip all layers horizontally
            foreach (var layer in frame.Layers)
            {
                // Get bitmap and simply flip vertically using a filter preset
                var bitmap = layer.LayerBitmap;
                flipFilter.ApplyToBitmap(bitmap);
            }
        }
    }

    public class FlipVerticalFrameOperation : IFrameOperation
    {
        public bool CanApply(IFrame frame)
        {
            return frame is Frame;
        }

        public void Apply(IFrame fr)
        {
            var frame = (Frame)fr;

            var flipFilter = new ScaleFilter
            {
                ScaleX = 1,
                ScaleY = -1,
                PixelQuality = true,
                Centered = true
            };

            // Flip all layers horizontally
            foreach (var layer in frame.Layers)
            {
                // Get bitmap and simply flip vertically using a filter preset
                var bitmap = layer.LayerBitmap;
                flipFilter.ApplyToBitmap(bitmap);
            }
        }
    }
}

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

namespace Pixelaria.Timeline
{
    public class TimelinePlayer
    {
        private readonly Timeline _timeline;

        public int FrameCount => _timeline.FrameCount;

        public TimelinePlayer(Timeline timeline)
        {
            _timeline = timeline;
        }

        public object ValueForFrame(int frame, int layerIndex)
        {
            var layer = _timeline.LayerAtIndex(layerIndex);

            var range = layer.KeyframeRangeForFrame(frame);
            if (!range.HasValue)
                return layer.LayerController.DefaultKeyframeValue();

            var (value1, value2) = layer.KeyframeValuesBetween(frame);
            if (value1 == null || value2 == null)
                return layer.LayerController.DefaultKeyframeValue();

            return layer.LayerController.InterpolatedValue(value1, value2, range.Value.Ratio(frame));
        }
    }
}
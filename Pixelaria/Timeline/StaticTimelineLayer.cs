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
using System.Linq;

namespace Pixelaria.Timeline
{
    public class StaticTimelineLayer : ITimelineLayer
    {
        private readonly IKeyframeSource _keyframeSource;

        public event IKeyframeSource.KeyframeEventHandler KeyframeAdded
        {
            add => _keyframeSource.KeyframeAdded += value;
            remove => _keyframeSource.KeyframeAdded -= value;
        }

        public event IKeyframeSource.KeyframeEventHandler KeyframeRemoved
        {
            add => _keyframeSource.KeyframeRemoved += value;
            remove => _keyframeSource.KeyframeRemoved -= value;
        }

        public event IKeyframeSource.KeyframeValueChangedEventHandler KeyframeValueChanged
        {
            add => _keyframeSource.KeyframeValueChanged += value;
            remove => _keyframeSource.KeyframeValueChanged -= value;
        }

        public IReadOnlyList<int> KeyframeIndexes => _keyframeSource.KeyframeIndexes;
        public int FrameCount => _keyframeSource.FrameCount;
        public ITimelineLayerController LayerController { get; }

        public StaticTimelineLayer(IKeyframeSource keyframeSource, ITimelineLayerController layerController)
        {
            _keyframeSource = keyframeSource;
            LayerController = layerController;
        }

        public void SetKeyframeValue(int frame, object value)
        {
            _keyframeSource.SetKeyframeValue(frame, value);
        }

        public void AddKeyframe(int frame, object value = null)
        {
            // Find the interpolated value for the keyframe to store
            if (value == null)
            {
                var range = KeyframeRangeForFrame(frame);
                var (item1, item2) = KeyframeValuesBetween(frame);
                if (range.HasValue && item1 != null && item2 != null)
                {
                    value = LayerController.InterpolatedValue(item1, item2, range.Value.Ratio(frame));
                }
                else
                {
                    value = LayerController.DefaultKeyframeValue();
                }
            }

            _keyframeSource.AddKeyframe(frame, value);
        }

        public void RemoveKeyframe(int frame)
        {
            _keyframeSource.RemoveKeyframe(frame);
        }

        public object ValueForKeyframe(int frameIndex)
        {
            return _keyframeSource.ValueForKeyframe(frameIndex);
        }

        public Keyframe? KeyframeForFrame(int frame)
        {
            for (int i = 0; i < _keyframeSource.KeyframeIndexes.Count; i++)
            {
                if (_keyframeSource.KeyframeIndexes[i] == frame)
                    return new Keyframe(frame, _keyframeSource.ValueForKeyframe(frame));
                if (i > 0 && _keyframeSource.KeyframeIndexes[i] > frame)
                    return new Keyframe(frame - 1, _keyframeSource.ValueForKeyframe(frame - 1));
            }

            return null;
        }

        public KeyframeRange? KeyframeRangeForFrame(int frame)
        {
            for (int i = 0; i < _keyframeSource.KeyframeIndexes.Count; i++)
            {
                if (i > 0 && _keyframeSource.KeyframeIndexes[i] > frame)
                {
                    return new KeyframeRange(_keyframeSource.KeyframeIndexes[i - 1], _keyframeSource.KeyframeIndexes[i] - _keyframeSource.KeyframeIndexes[i - 1]);
                }
                if (i == _keyframeSource.KeyframeIndexes.Count - 1 && _keyframeSource.KeyframeIndexes[i] <= frame)
                {
                    return new KeyframeRange(_keyframeSource.KeyframeIndexes[i], FrameCount - _keyframeSource.KeyframeIndexes[i]);
                }
            }

            return null;
        }

        public Keyframe? KeyframeExactlyOnFrame(int frame)
        {
            if (_keyframeSource.KeyframeIndexes.Any(kf => kf == frame))
            {
                return new Keyframe(frame, _keyframeSource.ValueForKeyframe(frame));
            }

            return null;
        }

        public KeyframePosition RelationshipToFrame(int frame)
        {
            var kfRange = KeyframeRangeForFrame(frame);
            if (!kfRange.HasValue)
                return KeyframePosition.None;

            if (frame == kfRange.Value.Frame)
            {
                if (kfRange.Value.Span == 1)
                {
                    return KeyframePosition.Full;
                }

                return KeyframePosition.First;
            }

            if (frame == kfRange.Value.LastFrame)
            {
                return KeyframePosition.Last;
            }

            return KeyframePosition.Center;
        }

        /// <summary>
        /// Searches for the two keyframes immediately before and after a given frame, and
        /// returns their keyframe values.
        ///
        /// In case the frame lands exactly on a frame, the method returns that keyframe's
        /// value as the first element of the tuple, and the value for the next keyframe
        /// after that keyframe as the second element of the tuple.
        ///
        /// In case the frame lands after the last keyframe, both values represent the last
        /// keyframe's value.
        ///
        /// If there are no keyframes on this timeline, a (null, null) tuple is returned,
        /// instead.
        /// </summary>
        public (object, object) KeyframeValuesBetween(int frame)
        {
            for (int i = 0; i < _keyframeSource.KeyframeIndexes.Count; i++)
            {
                if (_keyframeSource.KeyframeIndexes[i] > frame)
                {
                    if (i > 0)
                    {
                        return (_keyframeSource.ValueForKeyframe(_keyframeSource.KeyframeIndexes[i - 1]),
                            _keyframeSource.ValueForKeyframe(_keyframeSource.KeyframeIndexes[i]));
                    }
                }
                else if (i == _keyframeSource.KeyframeIndexes.Count - 1)
                {
                    return (_keyframeSource.ValueForKeyframe(_keyframeSource.KeyframeIndexes[i]),
                        _keyframeSource.ValueForKeyframe(_keyframeSource.KeyframeIndexes[i]));
                }
            }

            return (null, null);
        }
    }
}

﻿/*
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
using System.Collections.Generic;
using System.Linq;

namespace Pixelaria.Timeline
{
    public class KeyframeCollectionSource : IKeyframeSource
    {
        private readonly List<Keyframe> _keyframes = new List<Keyframe>();

        public IReadOnlyList<Keyframe> Keyframes => _keyframes;

        public event IKeyframeSource.KeyframeEventHandler KeyframeAdded;
        public event IKeyframeSource.KeyframeEventHandler KeyframeRemoved;
        public event IKeyframeSource.KeyframeValueChangedEventHandler KeyframeValueChanged;

        public IReadOnlyList<int> KeyframeIndexes => _keyframes.Select(keyframe => keyframe.Frame).ToList();

        public int FrameCount { get; }

        public KeyframeCollectionSource(int frameCount)
        {
            FrameCount = frameCount;
        }

        public void AddKeyframe(int frameIndex, object value)
        {
            var keyframe = new Keyframe(frameIndex, value);

            int index = _keyframes.BinarySearch(keyframe, new KeyframeComparer());
            if (index >= 0)
            {
                var kf = _keyframes[index];
                kf.Value = value;
                _keyframes[index] = kf;
                return;
            }
            _keyframes.Insert(~index, keyframe);
            KeyframeAdded?.Invoke(this, new TimelineKeyframeEventArgs(frameIndex));
            KeyframeValueChanged?.Invoke(this, new TimelineKeyframeValueChangeEventArgs(frameIndex, value));
        }

        public void SetKeyframeValue(int frameIndex, object value)
        {
            int index = KeyframeIndex(frameIndex);

            var keyframe = _keyframes[index];
            keyframe.Value = value;
            _keyframes[index] = keyframe;
            KeyframeValueChanged?.Invoke(this, new TimelineKeyframeValueChangeEventArgs(frameIndex, value));
        }

        public object ValueForKeyframe(int frameIndex)
        {
            int index = KeyframeIndex(frameIndex);
            return index < 0 ? null : _keyframes[index].Value;
        }

        public void RemoveKeyframe(int frameIndex)
        {
            int index = _keyframes.FindIndex(kf => kf.Frame == frameIndex);
            if (index < 0)
                return;
            _keyframes.RemoveAt(index);
            KeyframeRemoved?.Invoke(this, new TimelineKeyframeEventArgs(frameIndex));
        }

        private int KeyframeIndex(int frame)
        {
            for (int i = 0; i < _keyframes.Count; i++)
            {
                if (i > 0 && _keyframes[i].Frame > frame)
                {
                    return i - 1;
                }
            }

            return _keyframes.Count - 1;
        }

        private class KeyframeComparer : IComparer<Keyframe>
        {
            public int Compare(Keyframe x, Keyframe y)
            {
                return Math.Max(-1, Math.Min(1, x.Frame - y.Frame));
            }
        }
    }
}
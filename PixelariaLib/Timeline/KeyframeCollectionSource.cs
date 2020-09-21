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
using System.Collections.Generic;
using System.Linq;

namespace PixelariaLib.Timeline
{
    public class KeyframeCollectionSource : IKeyframeSource
    {
        private readonly List<Keyframe> _keyframes = new List<Keyframe>();

        public IReadOnlyList<Keyframe> Keyframes => _keyframes;

        public int FrameCount => _keyframes.Select(k => k.KeyframeRange.LastFrame + 1).Prepend(0).Max();
        
        public void AddKeyframe(Keyframe keyframe)
        {
            // Remove keyframes that have their first frame within the range of the incoming keyframe's range
            _keyframes.RemoveAll(kf => keyframe.Contains(kf.Frame));

            // Find any existing keyframe that starts before the new keyframe but that has a length that overlaps
            // the new keyframe's range, and clip it
            int kfIndex = _keyframes.FindIndex(kf => kf.Contains(keyframe.Frame));
            if (kfIndex > -1)
            {
                var existingKf = _keyframes[kfIndex];
                _keyframes[kfIndex] = new Keyframe(existingKf.Frame, keyframe.Frame - existingKf.Frame, existingKf.Value);
            }

            int index = _keyframes.BinarySearch(keyframe, new KeyframeComparer());
            if (index >= 0)
            {
                _keyframes[index] = keyframe;
                return;
            }
            _keyframes.Insert(~index, keyframe);
        }

        public void InsertKeyframe(int frame, object value)
        {
            int index = _keyframes.FindIndex(kf => kf.Contains(frame));
            if (index == -1)
            {
                _keyframes.Add(new Keyframe(frame, 1, value));
                return;
            }

            if (_keyframes[index].Frame == frame)
            {
                var keyframe = _keyframes[index];
                keyframe.Value = value;
                _keyframes[index] = keyframe;
                return;
            }

            var firstKf = new Keyframe(_keyframes[index].Frame, frame - _keyframes[index].Frame, _keyframes[index].Value);
            int length = _keyframes[index].Length - (frame - _keyframes[index].Frame);
            var secondKf = new Keyframe(frame, length, value);

            _keyframes[index] = firstKf;
            _keyframes.Insert(index + 1, secondKf);
        }

        public void ChangeKeyframeLength(int frame, int length)
        {
            int index = _keyframes.FindIndex(kf => kf.Frame == frame);
            if (index == -1)
                return;

            length = Math.Max(length, 1);
            if (_keyframes.Count - 1 > index)
            {
                length = Math.Min(length, _keyframes[index + 1].Frame - frame);
            }

            _keyframes[index] = new Keyframe(frame, length, _keyframes[index].Value);
        }

        public void SetKeyframeValue(int frameIndex, object value)
        {
            int index = KeyframeIndex(frameIndex);

            var keyframe = _keyframes[index];
            keyframe.Value = value;
            _keyframes[index] = keyframe;
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
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
using System.Linq;

namespace Pixelaria.Timeline
{
    /// <summary>
    /// Represents a timeline containing layers and keyframes
    /// </summary>
    public class Timeline : ITimeline
    {
        private readonly IKeyframeSource _keyframeSource;
        private int _frameCount = 1;

        public int FrameCount
        {
            get => _frameCount;
            set => _frameCount = Math.Max(1, value);
        }

        public ITimelineLayerController LayerController { get; }

        public Timeline(IKeyframeSource keyframeSource, ITimelineLayerController layerController)
        {
            _keyframeSource = keyframeSource;
            LayerController = layerController;
        }

        public TimelinePlayer CreatePlayer()
        {
            return new TimelinePlayer(this);
        }

        public void SetKeyframeValue(int frame, object value)
        {
            _keyframeSource.SetKeyframeValue(frame, value ?? LayerController.DefaultKeyframeValue());
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

        public struct KeyframeRange
        {
            public int Frame { get; }
            public int Span { get; }

            public int LastFrame => Frame + Span - 1;

            public KeyframeRange(int frame, int span)
            {
                Frame = frame;
                Span = span;
            }

            public float Ratio(int input)
            {
                return Math.Min(1.0f, Math.Max(0.0f, (float) (input - Frame) / Span));
            }

            public bool Equals(KeyframeRange other)
            {
                return Frame == other.Frame && Span == other.Span;
            }

            public override bool Equals(object obj)
            {
                return obj is KeyframeRange other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Frame * 397) ^ Span;
                }
            }

            public override string ToString()
            {
                return $"{{Frame: {Frame}, Span: {Span}}}";
            }

            public static bool operator ==(KeyframeRange left, KeyframeRange right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(KeyframeRange left, KeyframeRange right)
            {
                return !left.Equals(right);
            }
        }
    }

    public struct Keyframe
    {
        public int Frame { get; }
        public object Value { get; set; }

        public Keyframe(int frame, object value)
        {
            Frame = frame;
            Value = value;
        }
    }

    /// <summary>
    /// Specifies the positioning of a frame on a keyframe
    /// </summary>
    public enum KeyframePosition
    {
        /// <summary>
        /// The frame specified is not contained within the key frame's span
        /// </summary>
        None,
        /// <summary>
        /// The frame specified covers the entire key frame's span.
        ///
        /// This indicates the key frame is one frame long and the query frame matches this frame.
        /// </summary>
        Full,
        /// <summary>
        /// The frame specified lands on the first frame of the key frame's span
        /// </summary>
        First,
        /// <summary>
        /// The frame specified lands on the center of the key frame's span
        /// </summary>
        Center,
        /// <summary>
        /// The frame specified lands on the last frame of the key frame's span
        /// </summary>
        Last
    }
}
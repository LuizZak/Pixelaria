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

using System.Linq;
using JetBrains.Annotations;

namespace PixelariaLib.Timeline
{
    /// <summary>
    /// A layer on a timeline
    /// </summary>
    public class TimelineLayer: ITimelineLayer
    {
        private readonly IKeyframeSource _keyframeSource;

        public int FrameCount => _keyframeSource.FrameCount;
        
        public string LayerName { get; }

        public ITimelineLayerController LayerController { get; }

        public TimelineLayer(string layerName, IKeyframeSource keyframeSource, ITimelineLayerController layerController)
        {
            LayerName = layerName;
            _keyframeSource = keyframeSource;
            LayerController = layerController;
        }

        /// <summary>
        /// Sets the value for a keyframe at a specific frame. If the frame is not
        /// a keyframe, a new keyframe is created and its value set as <see cref="value"/>
        /// </summary>
        public void SetKeyframeValue(int frame, object value)
        {
            _keyframeSource.SetKeyframeValue(frame, value);
        }

        /// <summary>
        /// Adds a new keyframe at a specified frame.
        /// </summary>
        public void AddKeyframe(Keyframe keyframe)
        {
            _keyframeSource.AddKeyframe(keyframe);
        }

        /// <summary>
        /// Inserts a new keyframe at a specified frame.
        ///
        /// If the keyframe overlaps an existing keyframe, the resulting keyframe ranges
        /// occupy the existing keyframe's range
        /// </summary>
        public void InsertKeyframe(int frame, object value)
        {
            _keyframeSource.InsertKeyframe(frame, value);
        }

        /// <summary>
        /// Changes the length of a keyframe at a specified frame.
        /// 
        /// If no keyframe exists at <see cref="frame"/>, nothing is changed.
        ///
        /// In case the new length overlaps another keyframe, the length is capped to the frame
        /// just before the next keyframe.
        /// 
        /// <see cref="length"/> must be greater than zero.
        /// </summary>
        public void ChangeKeyframeLength(int frame, int length)
        {
            _keyframeSource.ChangeKeyframeLength(frame, length);
        }

        /// <summary>
        /// Removes a keyframe at a specified frame. If no keyframes exists at <see cref="frame"/>,
        /// nothing is changed.
        /// </summary>
        public void RemoveKeyframe(int frame)
        {
            _keyframeSource.RemoveKeyframe(frame);
        }

        /// <summary>
        /// Gets the value for a keyframe on a given index.
        ///
        /// Returns null, in case the frame at the given index is not a keyframe or has no value associated.
        /// </summary>
        [CanBeNull]
        public object ValueForKeyframe(int frameIndex)
        {
            return _keyframeSource.ValueForKeyframe(frameIndex);
        }

        public Keyframe? KeyframeForFrame(int frame)
        {
            for (int i = 0; i < _keyframeSource.Keyframes.Count; i++)
            {
                if (_keyframeSource.Keyframes[i].Contains(frame))
                    return _keyframeSource.Keyframes[i];
            }

            return null;
        }

        /// <summary>
        /// Returns the keyframe relationship of a specific frame in this layer
        /// </summary>
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
        /// If the current frame at <see cref="frame"/> is a keyframe, returns that keyframe information,
        /// otherwise returns null.
        /// </summary>
        public Keyframe? KeyframeExactlyOnFrame(int frame)
        {
            foreach (var kf in _keyframeSource.Keyframes)
            {
                if (kf.Frame == frame) 
                    return kf;
            }

            return null;
        }

        /// <summary>
        /// Returns the range of a keyframe for a frame located at <see cref="frame"/>.
        ///
        /// The range matches the keyframe range that <see cref="frame"/> is contained at,
        /// or null, in case no keyframe is set before or after <see cref="frame"/>
        /// </summary>
        public KeyframeRange? KeyframeRangeForFrame(int frame)
        {
            return KeyframeForFrame(frame)?.KeyframeRange;
        }

        /// <summary>
        /// Returns all keyframes that precede <see cref="frame"/>.
        ///
        /// Does not include keyframes exactly on <see cref="frame"/>
        /// </summary>
        public Keyframe[] KeyframesBefore(int frame)
        {
            return _keyframeSource.Keyframes.Where(kf => kf.Frame < frame).ToArray();
        }

        /// <summary>
        /// Returns all keyframes that succeed <see cref="frame"/>.
        ///
        /// Does not include keyframes exactly on <see cref="frame"/>
        /// </summary>
        public Keyframe[] KeyframesAfter(int frame)
        {
            return _keyframeSource.Keyframes.Where(kf => kf.Frame > frame).ToArray();
        }
    }
}

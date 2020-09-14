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
using System.ComponentModel;

namespace Pixelaria.Timeline
{
    /// <summary>
    /// Represents a timeline containing layers and keyframes
    /// </summary>
    public class Timeline : ITimeline
    {
        private readonly ILayerSource _layerSource;

        /// <summary>
        /// Event fired when a new keyframe is added
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a new keyframe is added")]
        public event ITimeline.KeyframeEventHandler KeyframeAdded;
        /// <summary>
        /// Event fired when a keyframe is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a keyframe is removed")]
        public event ITimeline.KeyframeEventHandler KeyframeRemoved;

        /// <summary>
        /// Event fired when a keyframe's value has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the the value of a keyframe changes")]
        public event ITimeline.KeyframeValueChangedEventHandler KeyframeValueChanged;

        public int FrameCount
        {
            get
            {
                int frameCount = 0;
                for (int i = 0; i < _layerSource.LayerCount; i++)
                {
                    frameCount = Math.Max(frameCount, _layerSource.LayerAtIndex(i).FrameCount);
                }

                return frameCount;
            }
        }

        public int LayerCount => _layerSource.LayerCount;

        public Timeline(ILayerSource layerSource)
        {
            _layerSource = layerSource;
        }

        public TimelinePlayer CreatePlayer()
        {
            return new TimelinePlayer(this);
        }

        public ITimelineLayer LayerAtIndex(int index)
        {
            return _layerSource.LayerAtIndex(index);
        }

    }

    public readonly struct KeyframeRange
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
            return Math.Min(1.0f, Math.Max(0.0f, (float)(input - Frame) / Span));
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
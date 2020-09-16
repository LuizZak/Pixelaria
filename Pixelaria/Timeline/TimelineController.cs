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
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;

namespace Pixelaria.Timeline
{
    /// <summary>
    /// Represents a timeline containing layers and keyframes
    /// </summary>
    public class TimelineController : ITimeline
    {
        private readonly List<TimelineLayer> _layers = new List<TimelineLayer>();

        /// <summary>
        /// Event handler for keyframe-related events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void KeyframeEventHandler(object sender, TimelineRemoveKeyframeEventArgs e);

        /// <summary>
        /// Event handler for events related to keyframe value changes
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void KeyframeValueChangedEventHandler(object sender, TimelineKeyframeValueChangeEventArgs e);

        /// <summary>
        /// Event handler for layer-based events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void LayerEventHandler(object sender, TimelineLayerEventArgs e);

        /// <summary>
        /// Event fired when a new keyframe is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a new keyframe is added")]
        public event KeyframeValueChangedEventHandler WillAddKeyframe;
        /// <summary>
        /// Event fired when a keyframe is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a keyframe is removed")]
        public event KeyframeEventHandler WillRemoveKeyframe;
        /// <summary>
        /// Event fired when a keyframe's value has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the the value of a keyframe changes")]
        public event KeyframeValueChangedEventHandler WillChangeKeyframeValue;

        /// <summary>
        /// Event fired when a new layer is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a new layer is added")]
        public event LayerEventHandler WillAddLayer;
        /// <summary>
        /// Event fired when a layer is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a layer is removed")]
        public event LayerEventHandler WillRemoveLayer;

        /// <summary>
        /// Total frame count across all layers
        /// </summary>
        public int FrameCount
        {
            get
            {
                return _layers.Select(layer => layer.FrameCount).Prepend(0).Max();
            }
        }

        /// <summary>
        /// Total layer count
        /// </summary>
        public int LayerCount => _layers.Count;

        public TimelinePlayer CreatePlayer()
        {
            return new TimelinePlayer(this);
        }

        public ITimelineLayer LayerAtIndex(int index)
        {
            return _layers[index];
        }

        private TimelineLayer InternalLayerAtIndex(int index)
        {
            return _layers[index];
        }

        /// <summary>
        /// Adds a new keyframe at a specified frame.
        /// </summary>
        public void AddKeyframe(int frame, int layerIndex, object value = null)
        {
            var layer = InternalLayerAtIndex(layerIndex);

            // Find the interpolated value for the keyframe to store
            if (value == null)
            {
                var range = layer.KeyframeRangeForFrame(frame);
                var (item1, item2) = layer.KeyframeValuesBetween(frame);
                if (range.HasValue && item1 != null && item2 != null)
                {
                    value = layer.LayerController.InterpolatedValue(item1, item2, range.Value.Ratio(frame));
                }
                else
                {
                    value = layer.LayerController.DefaultKeyframeValue();
                }
            }

            var eventArgs = new TimelineKeyframeValueChangeEventArgs(frame, layerIndex, null, value);
            WillAddKeyframe?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                layer.AddKeyframe(frame, value);
            }
        }

        /// <summary>
        /// Removes a keyframe at a specified frame. If no keyframes exists at <see cref="frame"/>,
        /// nothing is changed.
        /// </summary>
        public void RemoveKeyframe(int frame, int layerIndex)
        {
            var layer = InternalLayerAtIndex(layerIndex);
            var kfValue = layer.ValueForKeyframe(frame);

            var eventArgs = new TimelineRemoveKeyframeEventArgs(frame, layerIndex, kfValue);
            WillRemoveKeyframe?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                layer.RemoveKeyframe(frame);
            }
        }

        /// <summary>
        /// Sets the value for a keyframe at a specific frame. If the frame is not
        /// a keyframe, a new keyframe is created and its value set as <see cref="value"/>
        /// </summary>
        public void SetKeyframeValue(int frame, int layerIndex, object value)
        {
            var layer = InternalLayerAtIndex(layerIndex);
            var kfValue = layer.ValueForKeyframe(frame);

            var eventArgs = new TimelineKeyframeValueChangeEventArgs(frame, layerIndex, kfValue, value);
            WillChangeKeyframeValue?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                layer.SetKeyframeValue(frame, value);
            }
        }

        /// <summary>
        /// Gets the value for a keyframe on a given index.
        ///
        /// Returns null, in case the frame at the given index is not a keyframe or has no value associated.
        /// </summary>
        [CanBeNull]
        public object ValueForKeyframe(int frameIndex, int layerIndex)
        {
            var layer = InternalLayerAtIndex(layerIndex);

            return layer.ValueForKeyframe(frameIndex);
        }

        /// <summary>
        /// Adds a new timeline layer with a given controller, optionally inserting at a specific index
        /// </summary>
        public void AddLayer(ITimelineLayerController controller, int? layerIndex = null)
        {
            AddLayer(new KeyframeCollectionSource(), controller, layerIndex);
        }

        /// <summary>
        /// Adds a new timeline layer with a given keyframe source and controller, optionally inserting at a specific index
        /// </summary>
        public void AddLayer(IKeyframeSource source, ITimelineLayerController controller, int? layerIndex = null)
        {
            var layer = new TimelineLayer(source, controller);

            var index = layerIndex ?? _layers.Count;
            var eventArgs = new TimelineLayerEventArgs(layer, index);

            WillAddLayer?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                _layers.Insert(index, layer);
            }
        }

        /// <summary>
        /// Removes a layer at a given index
        /// </summary>
        public void RemoveLayer(int layerIndex)
        {
            var layer = InternalLayerAtIndex(layerIndex);

            var eventArgs = new TimelineLayerEventArgs(layer, layerIndex);
            WillRemoveLayer?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                _layers.RemoveAt(layerIndex);
            }
        }
    }

    /// <summary>
    /// Represents the range of a keyframe on a timeline
    /// </summary>
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
            if (Span == 0)
                return input <= Frame ? 0 : 1;

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

    /// <summary>
    /// Represents a keyframe on a timeline
    /// </summary>
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

    /// <summary>
    /// Event arguments for a cancelable timeline keyframe event
    /// </summary>
    public class TimelineRemoveKeyframeEventArgs : EventArgs
    {
        public int FrameIndex { get; }

        public int LayerIndex { get; }

        public object Value { get; }

        public bool Cancel { get; set; }

        public TimelineRemoveKeyframeEventArgs(int frameIndex, int layerIndex, object value)
        {
            FrameIndex = frameIndex;
            LayerIndex = layerIndex;
            Value = value;
        }
    }

    /// <summary>
    /// Event arguments for a cancelable timeline keyframe value changed event
    /// </summary>
    public class TimelineKeyframeValueChangeEventArgs : EventArgs
    {
        public int FrameIndex { get; }

        public int LayerIndex { get; }

        public object OldValue { get; }

        public object NewValue { get; }

        public bool Cancel { get; set; }

        public TimelineKeyframeValueChangeEventArgs(int frameIndex, int layerIndex, object oldValue, object newValue)
        {
            FrameIndex = frameIndex;
            LayerIndex = layerIndex;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Event arguments for a cancelable timeline layer event
    /// </summary>
    public class TimelineLayerEventArgs : EventArgs
    {
        public ITimelineLayer Layer { get; }

        public int LayerIndex { get; }

        public bool Cancel { get; set; }

        public TimelineLayerEventArgs(ITimelineLayer layer, int layerIndex)
        {
            Layer = layer;
            LayerIndex = layerIndex;
        }
    }
}
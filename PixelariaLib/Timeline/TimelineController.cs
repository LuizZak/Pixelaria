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

namespace PixelariaLib.Timeline
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
        /// Event handler for cancelable keyframe-related events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void KeyframeCancelableEventHandler(object sender, TimelineCancelableRemoveKeyframeEventArgs e);

        /// <summary>
        /// Event handler for cancelable events related to keyframe value changes
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void KeyframeCancelableValueChangedEventHandler(object sender, TimelineCancelableKeyframeValueChangeEventArgs e);

        /// <summary>
        /// Event handler for cancelable layer-based events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void CancelableLayerEventHandler(object sender, TimelineCancelableLayerEventArgs e);

        /// <summary>
        /// Event fired before a new keyframe is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs before a new keyframe is added")]
        public event KeyframeCancelableValueChangedEventHandler WillAddKeyframe;
        /// <summary>
        /// Event fired before a keyframe is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs before a keyframe is removed")]
        public event KeyframeCancelableEventHandler WillRemoveKeyframe;
        /// <summary>
        /// Event fired before a keyframe's value has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs before the the value of a keyframe changes")]
        public event KeyframeCancelableValueChangedEventHandler WillChangeKeyframeValue;

        /// <summary>
        /// Event fired before a new layer is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs before a new layer is added")]
        public event CancelableLayerEventHandler WillAddLayer;
        /// <summary>
        /// Event fired before a layer is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs before a layer is removed")]
        public event CancelableLayerEventHandler WillRemoveLayer;

        /// <summary>
        /// Event fired before a new keyframe is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a new keyframe is added")]
        public event KeyframeValueChangedEventHandler DidAddKeyframe;
        /// <summary>
        /// Event fired before a keyframe is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a keyframe is removed")]
        public event KeyframeEventHandler DidRemoveKeyframe;
        /// <summary>
        /// Event fired before a keyframe's value has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the the value of a keyframe changes")]
        public event KeyframeValueChangedEventHandler DidChangeKeyframeValue;

        /// <summary>
        /// Event fired before a new layer is added
        /// </summary> 
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a new layer is added")]
        public event LayerEventHandler DidAddLayer;
        /// <summary>
        /// Event fired before a layer is removed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever a layer is removed")]
        public event LayerEventHandler DidRemoveLayer;

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
            AddKeyframe(frame, 1, layerIndex, value);
        }

        /// <summary>
        /// Adds a new keyframe at a specified frame.
        /// </summary>
        public void AddKeyframe(int frame, int length, int layerIndex, object value = null)
        {
            var layer = InternalLayerAtIndex(layerIndex);

            // Find the interpolated value for the keyframe to store
            value ??= layer.LayerController.DefaultKeyframeValue();

            var keyframe = new Keyframe(frame, length, value);

            var eventArgs = new TimelineCancelableKeyframeValueChangeEventArgs(keyframe, layerIndex, null);
            WillAddKeyframe?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                layer.AddKeyframe(keyframe);

                DidAddKeyframe?.Invoke(this, eventArgs);
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

            var keyframe = new Keyframe(frame, 0, kfValue);

            var eventArgs = new TimelineCancelableRemoveKeyframeEventArgs(keyframe, layerIndex);
            WillRemoveKeyframe?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                layer.RemoveKeyframe(frame);

                DidRemoveKeyframe?.Invoke(this, eventArgs);
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

            var keyframe = new Keyframe(frame, 0, value);

            var eventArgs = new TimelineCancelableKeyframeValueChangeEventArgs(keyframe, layerIndex, kfValue);
            WillChangeKeyframeValue?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                layer.SetKeyframeValue(frame, value);

                DidChangeKeyframeValue?.Invoke(this, eventArgs);
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
        public void AddLayer(string layerName, ITimelineLayerController controller, int? layerIndex = null)
        {
            AddLayer(layerName, new KeyframeCollectionSource(), controller, layerIndex);
        }

        /// <summary>
        /// Adds a new timeline layer with a given keyframe source and controller, optionally inserting at a specific index
        /// </summary>
        public void AddLayer(string layerName, IKeyframeSource source, ITimelineLayerController controller, int? layerIndex = null)
        {
            var layer = new TimelineLayer(layerName, source, controller);

            int index = layerIndex ?? _layers.Count;
            var eventArgs = new TimelineCancelableLayerEventArgs(layer, index);

            WillAddLayer?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                _layers.Insert(index, layer);

                DidAddLayer?.Invoke(this, eventArgs);
            }
        }

        /// <summary>
        /// Removes a layer at a given index
        /// </summary>
        public void RemoveLayer(int layerIndex)
        {
            var layer = InternalLayerAtIndex(layerIndex);

            var eventArgs = new TimelineCancelableLayerEventArgs(layer, layerIndex);
            WillRemoveLayer?.Invoke(this, eventArgs);

            if (!eventArgs.Cancel)
            {
                _layers.RemoveAt(layerIndex);

                DidRemoveLayer?.Invoke(this, eventArgs);
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
        public int Length { get; }

        public KeyframeRange KeyframeRange => new KeyframeRange(Frame, Length);

        public object Value { get; set; }

        public Keyframe(int frame, int length, object value)
        {
            Frame = frame;
            Value = value;
            Length = length;
        }

        public bool Contains(int frame)
        {
            return frame >= KeyframeRange.Frame && frame <= KeyframeRange.LastFrame;
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
    /// Event arguments for a timeline keyframe removal event
    /// </summary>
    public class TimelineRemoveKeyframeEventArgs : EventArgs
    {
        public Keyframe Keyframe { get; }

        public int LayerIndex { get; }

        public TimelineRemoveKeyframeEventArgs(Keyframe keyframe, int layerIndex)
        {
            Keyframe = keyframe;
            LayerIndex = layerIndex;
        }
    }

    /// <summary>
    /// Event arguments for a cancelable timeline keyframe removal event
    /// </summary>
    public class TimelineCancelableRemoveKeyframeEventArgs : TimelineRemoveKeyframeEventArgs
    {
        public bool Cancel { get; set; }

        public TimelineCancelableRemoveKeyframeEventArgs(Keyframe keyframe, int layerIndex) : base(keyframe, layerIndex)
        {

        }
    }

    /// <summary>
    /// Event arguments for a timeline keyframe value changed event
    /// </summary>
    public class TimelineKeyframeValueChangeEventArgs : EventArgs
    {
        public Keyframe Keyframe { get; }

        public int LayerIndex { get; }

        public object OldValue { get; }

        public TimelineKeyframeValueChangeEventArgs(Keyframe keyframe, int layerIndex, object oldValue)
        {
            Keyframe = keyframe;
            LayerIndex = layerIndex;
            OldValue = oldValue;
        }
    }

    /// <summary>
    /// Event arguments for a cancelable timeline keyframe value changed event
    /// </summary>
    public class TimelineCancelableKeyframeValueChangeEventArgs : TimelineKeyframeValueChangeEventArgs
    {
        public bool Cancel { get; set; }

        public TimelineCancelableKeyframeValueChangeEventArgs(Keyframe keyframe, int layerIndex, object oldValue) : base(keyframe, layerIndex, oldValue)
        {

        }
    }

    /// <summary>
    /// Event arguments for a timeline layer event
    /// </summary>
    public class TimelineLayerEventArgs : EventArgs
    {
        public ITimelineLayer Layer { get; }

        public int LayerIndex { get; }

        public TimelineLayerEventArgs(ITimelineLayer layer, int layerIndex)
        {
            Layer = layer;
            LayerIndex = layerIndex;
        }
    }

    /// <summary>
    /// Event arguments for a cancelable timeline layer event
    /// </summary>
    public class TimelineCancelableLayerEventArgs : TimelineLayerEventArgs
    {
        public bool Cancel { get; set; }

        public TimelineCancelableLayerEventArgs(ITimelineLayer layer, int layerIndex) : base(layer, layerIndex)
        {
        }
    }
}
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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Timeline;
using PixelariaLib.Views.Controls;

namespace Pixelaria.Views.Controls
{
    public partial class TimelineControl : Control
    {
        private readonly int _frameCountHeight = 16;
        private readonly int _layerHeight = 22;
        private readonly float _frameWidth = 12;
        private readonly Color _backgroundColor = Color.LightGray;
        private readonly Color _keyframeColor = Color.White;
        private readonly Color _selectedFrameColor = Color.CornflowerBlue;
        private readonly ContextMenuStrip _contextMenu;

        private int _currentFrame;
        private Timeline.Timeline _timeline;

        /// <summary>
        /// States the current frame in the seek bar.
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(0)]
        [Description("The current playing frame on the timeline")]
        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                // Invalidate last frame position:
                InvalidateCurrentFrame();

                _currentFrame = value;

                // Invalidate new frame position:
                InvalidateCurrentFrame();
            }
        }

        /// <summary>
        /// Event handler for the <see cref="FrameChanged"/>
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void FrameChangedEventHandler(object sender, FrameChangedEventArgs e);
        /// <summary>
        /// Event fired every time the frame range has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user changes the current frame")]
        public event FrameChangedEventHandler FrameChanged;

        /// <summary>
        /// Event handler for keyframe-related events
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void KeyframeEventHandler(object sender, TimelineControlKeyframeEventArgs e);
        /// <summary>
        /// Event fired when a new keyframe is added by the user
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user selects to add a new keyframe")]
        public event KeyframeEventHandler WillAddKeyframe;
        /// <summary>
        /// Event fired when a new keyframe is removed by the user
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user selects to remove a keyframe")]
        public event KeyframeEventHandler WillRemoveKeyframe;

        public Timeline.Timeline Timeline
        {
            get => _timeline;
            set
            {
                if (_timeline != null)
                {
                    _timeline.WillAddKeyframe -= TimelineOnKeyframeAdded;
                    _timeline.WillRemoveKeyframe -= TimelineOnKeyframeRemoved;
                    _timeline.WillChangeKeyframeValue -= TimelineOnKeyframeValueChanged;
                }

                _timeline = value;
                Invalidate();

                if (_timeline != null)
                {
                    _timeline.WillAddKeyframe += TimelineOnKeyframeAdded;
                    _timeline.WillRemoveKeyframe += TimelineOnKeyframeRemoved;
                    _timeline.WillChangeKeyframeValue += TimelineOnKeyframeValueChanged;
                }
            }
        }

        public TimelineControl()
        {
            _contextMenu = new ContextMenuStrip();

            Timeline = new Timeline.Timeline();

            InitializeComponent();

            // Set the control's style so it won't flicker at every draw call:
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);
        }

        private void TimelineOnKeyframeAdded(object sender, TimelineKeyframeValueChangeEventArgs e)
        {
            Invalidate();
        }

        private void TimelineOnKeyframeRemoved(object sender, TimelineRemoveKeyframeEventArgs e)
        {
            Invalidate();
        }

        private void TimelineOnKeyframeValueChanged(object sender, TimelineKeyframeValueChangeEventArgs e)
        {
            Invalidate();
        }

        private int FrameOnX(float x)
        {
            return Math.Max(0, Math.Min(Timeline.FrameCount - 1, (int) Math.Floor(x / _frameWidth)));
        }

        private int LayerOnY(float y)
        {
            return Math.Max(0, Math.Min(Timeline.LayerCount - 1, (int) Math.Floor((y - _frameCountHeight) / _layerHeight)));
        }

        private RectangleF BoundsForFrame(int frame, int layer)
        {
            return new RectangleF(frame * _frameWidth, _frameCountHeight + layer * _layerHeight, _frameWidth, _layerHeight);
        }

        private void InvalidateCurrentFrame()
        {
            for (int i = 0; i < Timeline.LayerCount; i++)
            {
                var boundsForFrame = BoundsForFrame(_currentFrame, i);
                boundsForFrame.Y = 0;
                boundsForFrame.Height = Bounds.Height;
                boundsForFrame.Inflate(2, 2);
                Invalidate(new Region(boundsForFrame));
            }
        }

        /// <summary>
        /// Changes the current frame being displayed
        /// </summary>
        /// <param name="newFrame">The new frame to display</param>
        private void ChangeFrame(int newFrame)
        {
            if (_currentFrame == newFrame)
                return;

            int oldFrame = _currentFrame;

            if (FrameChanged != null)
            {
                var evArgs = new FrameChangedEventArgs(oldFrame, newFrame);

                FrameChanged.Invoke(this, evArgs);

                if (evArgs.Cancel)
                    return;
            }

            CurrentFrame = newFrame;
        }

        private void ShowContextMenu(int frame, int layerIndex, Point position)
        {
            _contextMenu.Items.Clear();

            var layer = Timeline.LayerAtIndex(layerIndex);

            var kf = layer.KeyframeExactlyOnFrame(frame);
            if (frame > 0 && kf.HasValue)
            {
                _contextMenu.Items.Add("Remove Keyframe").Click += (sender, args) =>
                {
                    if (WillRemoveKeyframe != null)
                    {
                        var ev = new TimelineControlKeyframeEventArgs(frame);
                        WillRemoveKeyframe.Invoke(this, ev);

                        if (ev.Cancel)
                            return;
                    }

                    Timeline.RemoveKeyframe(kf.Value.Frame, layerIndex);
                    Invalidate();
                };
            }
            else if (frame != 0)
            {
                _contextMenu.Items.Add("Add Keyframe").Click += (sender, args) =>
                {
                    if (WillAddKeyframe != null)
                    {
                        var ev = new TimelineControlKeyframeEventArgs(frame);
                        WillAddKeyframe.Invoke(this, ev);

                        if (ev.Cancel)
                            return;
                    }

                    Timeline.AddKeyframe(frame, layerIndex);
                    Invalidate();
                };
            }

            _contextMenu.Show(this, position);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            var layer = LayerOnY(e.Y);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                ChangeFrame(FrameOnX(e.X));
            }

            if (e.Button == MouseButtons.Right && layer >= 0 && layer < Timeline.LayerCount)
            {
                ShowContextMenu(_currentFrame, layer, e.Location);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButtons.Left)
            {
                ChangeFrame(FrameOnX(e.X));
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(_backgroundColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            DrawFrameCounter(e);
            DrawTimelineBackground(e);
            DrawKeyframes(e);
        }

        private void DrawFrameCounter([NotNull] PaintEventArgs e)
        {
            var font = new Font(FontFamily.GenericSansSerif, 8);

            e.Graphics.DrawLine(Pens.White, 0, _frameCountHeight, Bounds.Width, _frameCountHeight);

            int frame = 0;
            for (float x = 0; x < Bounds.Width; x += _frameWidth)
            {
                if (frame == Timeline.FrameCount)
                    break;

                var currentFrameRect = new RectangleF(x, 0, _frameWidth, _frameCountHeight);
                if (frame == _currentFrame)
                {
                    using var brush = new SolidBrush(_selectedFrameColor);
                    e.Graphics.FillRectangle(brush, currentFrameRect);
                }

                if (frame == 0 || (frame + 1) % 5 == 0)
                {
                    string frameString = (frame + 1).ToString();
                    e.Graphics.DrawString(frameString, font, Brushes.DimGray, x, 0);

                    if (frame == _currentFrame)
                    {
                        var state = e.Graphics.Save();

                        e.Graphics.IntersectClip(currentFrameRect);
                        e.Graphics.DrawString(frameString, font, Brushes.White, x, 0);

                        e.Graphics.Restore(state);
                    }
                }

                frame += 1;
            }

            font.Dispose();
        }

        private void DrawTimelineBackground([NotNull] PaintEventArgs e)
        {
            var frameSeparatorPen = new Pen(Color.White)
            {
                DashPattern = new[] { 3.0f, 3.0f },
                DashCap = DashCap.Flat
            };
            var oddFrameSeparatorPen = new Pen(Color.White)
            {
                Width = 2
            };

            for (int layerIndex = 0; layerIndex < Timeline.LayerCount; layerIndex++)
            {
                int frame = 0;
                for (float x = 0; x < Bounds.Width; x += _frameWidth)
                {
                    if (frame == Timeline.FrameCount)
                        break;

                    int y = _frameCountHeight + layerIndex * _layerHeight;

                    var top = new Point((int)x, y);
                    var bottom = new Point((int)x, y + _layerHeight);

                    e.Graphics.DrawLine(frame % 5 == 0 ? oddFrameSeparatorPen : frameSeparatorPen, top, bottom);

                    if (frame == _currentFrame)
                    {
                        e.Graphics.FillRectangle(Brushes.CornflowerBlue, BoundsForFrame(frame, layerIndex));
                    }

                    frame += 1;
                }
            }
            
            frameSeparatorPen.Dispose();
            oddFrameSeparatorPen.Dispose();
        }

        private void DrawKeyframes([NotNull] PaintEventArgs e)
        {
            for (int layerIndex = 0; layerIndex < Timeline.LayerCount; layerIndex++)
            {
                var layer = Timeline.LayerAtIndex(layerIndex);

                int frame = 0;
                for (float x = 0; x < Bounds.Width; x += _frameWidth)
                {
                    if (frame == layer.FrameCount)
                        break;

                    DrawKeyframe(e, frame, layerIndex);

                    frame += 1;
                }
            }
        }

        private void DrawKeyframe([NotNull] PaintEventArgs e, int currentFrame, int currentLayer)
        {
            var frameBounds = new RectangleF(0, 0, _frameWidth, _layerHeight - 1);

            var state = e.Graphics.Save();

            float x = currentFrame * _frameWidth;
            float y = _frameCountHeight + currentLayer * _layerHeight;

            e.Graphics.TranslateTransform(x, y);

            var dashedPen = new Pen(Color.LightGray)
            {
                DashPattern = new[] { 3.0f, 3.0f },
                DashCap = DashCap.Flat
            };
            var outlinePen = new Pen(Color.Gray);
            var bodyFillBrush = new SolidBrush(currentFrame == _currentFrame ? Color.White : Color.Gray);

            var layer = Timeline.LayerAtIndex(currentLayer);
            var relationship = layer.RelationshipToFrame(currentFrame);

            if (relationship != KeyframePosition.None)
            {
                using var brush = new SolidBrush(currentFrame == _currentFrame ? _selectedFrameColor : _keyframeColor);
                e.Graphics.FillRectangle(brush, frameBounds);
            }

            if (relationship == KeyframePosition.First || relationship == KeyframePosition.Full)
            {
                var circleCenter = new PointF(frameBounds.Width / 2, frameBounds.Height / 2);
                const float circleRadius = 2.5f;
                var ellipse = new RectangleF(circleCenter.X - circleRadius, circleCenter.Y - circleRadius, circleRadius * 2, circleRadius * 2);
                e.Graphics.FillEllipse(bodyFillBrush, ellipse);
            }

            switch (relationship)
            {
                case KeyframePosition.None:
                    break;
                case KeyframePosition.Full:
                    e.Graphics.DrawRectangle(outlinePen, frameBounds.X, frameBounds.Y, frameBounds.Width, frameBounds.Height);
                    break;
                case KeyframePosition.First:
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Left, frameBounds.Bottom);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Right, frameBounds.Top);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Bottom, frameBounds.Right, frameBounds.Bottom);
                    e.Graphics.DrawLine(dashedPen, frameBounds.Right, frameBounds.Top, frameBounds.Right, frameBounds.Bottom);
                    break;
                case KeyframePosition.Center:
                    e.Graphics.DrawLine(dashedPen, frameBounds.Left, frameBounds.Top, frameBounds.Left, frameBounds.Bottom);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Right, frameBounds.Top);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Bottom, frameBounds.Right, frameBounds.Bottom);
                    e.Graphics.DrawLine(dashedPen, frameBounds.Right, frameBounds.Top, frameBounds.Right, frameBounds.Bottom);
                    break;
                case KeyframePosition.Last:
                    e.Graphics.DrawLine(dashedPen, frameBounds.Left, frameBounds.Top, frameBounds.Left, frameBounds.Bottom);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Right, frameBounds.Top, frameBounds.Right, frameBounds.Bottom);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Right, frameBounds.Top);
                    e.Graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Bottom, frameBounds.Right, frameBounds.Bottom);
                    break;
            }

            e.Graphics.Restore(state);

            dashedPen.Dispose();
            outlinePen.Dispose();
            bodyFillBrush.Dispose();
        }
    }

    public class TimelineControlKeyframeEventArgs : EventArgs
    {
        public int FrameIndex { get; }

        /// <summary>
        /// Whether to cancel this event and not modify the keyframe
        /// </summary>
        public bool Cancel { get; set; }

        public TimelineControlKeyframeEventArgs(int frameIndex)
        {
            FrameIndex = frameIndex;
        }
    }
}

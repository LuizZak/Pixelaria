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
using PixelariaLib.Timeline;
using PixelariaLib.Views.Controls;

namespace Pixelaria.Views.Controls
{
    public partial class TimelineControl : Control
    {
        private readonly int _frameCountHeight = 16;
        private readonly int _layerHeight = 22;
        private readonly float _frameWidth = 12;
        private readonly ContextMenuStrip _contextMenu;

        private readonly VScrollBar _vScrollBar = new VScrollBar();
        private readonly HScrollBar _hScrollBar = new HScrollBar();

        private readonly DoubleBufferedPanel _layersPanel = new DoubleBufferedPanel();
        private readonly DoubleBufferedPanel _framesPanel = new DoubleBufferedPanel();

        private int _currentFrame;
        private int _currentLayer;
        private TimelineController _timelineController;
        private bool _showExtendedTimeline;

        /// <summary>
        /// Whether to display an extended timeline that displays frames further than the current frame count of the timeline.
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description(
            "Whether to display an extended timeline that displays frames further than the current frame count of the timeline")]
        public bool ShowExtendedTimeline
        {
            get => _showExtendedTimeline;
            set
            {
                _showExtendedTimeline = value;
                UpdatePanelSizes();
            }
        }

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
        /// States the current layer being highlighted.
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(0)]
        [Description("States the current layer being highlighted")]
        public int CurrentLayer
        {
            get => _currentLayer;
            set
            {
                InvalidateCurrentLayer();
                
                _currentLayer = value;

                InvalidateCurrentLayer();
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
        /// Event handler for the <see cref="LayerChanged"/>
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void LayerChangedEventHandler(object sender, LayerChangedEventArgs e);
        /// <summary>
        /// Event fired every time the layer highlighted has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user changes the current highlighted layer")]
        public event LayerChangedEventHandler LayerChanged;

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

        public TimelineController TimelineController
        {
            get => _timelineController;
            set
            {
                if (_timelineController != null)
                {
                    _timelineController.WillAddKeyframe -= TimelineControllerOnWillAddKeyframe;
                    _timelineController.WillRemoveKeyframe -= TimelineControllerOnWillRemoveKeyframe;
                    _timelineController.WillChangeKeyframeValue -= TimelineControllerOnWillChangeKeyframeValue;
                    _timelineController.WillAddLayer -= TimelineControllerOnWillAddLayer;
                    _timelineController.WillRemoveLayer -= TimelineControllerOnWillRemoveLayer;

                    _timelineController.DidAddKeyframe -= TimelineControllerOnDidAddKeyframe;
                    _timelineController.DidRemoveKeyframe -= TimelineControllerOnDidRemoveKeyframe;
                    _timelineController.DidChangeKeyframeValue -= TimelineControllerOnDidChangeKeyframeValue;
                    _timelineController.DidAddLayer -= TimelineControllerOnDidAddLayer;
                    _timelineController.DidRemoveLayer -= TimelineControllerOnDidRemoveLayer;
                }

                _timelineController = value;
                Invalidate();

                if (_timelineController != null)
                {
                    _timelineController.WillAddKeyframe += TimelineControllerOnWillAddKeyframe;
                    _timelineController.WillRemoveKeyframe += TimelineControllerOnWillRemoveKeyframe;
                    _timelineController.WillChangeKeyframeValue += TimelineControllerOnWillChangeKeyframeValue;
                    _timelineController.WillAddLayer += TimelineControllerOnWillAddLayer;
                    _timelineController.WillRemoveLayer += TimelineControllerOnWillRemoveLayer;

                    _timelineController.DidAddKeyframe += TimelineControllerOnDidAddKeyframe;
                    _timelineController.DidRemoveKeyframe += TimelineControllerOnDidRemoveKeyframe;
                    _timelineController.DidChangeKeyframeValue += TimelineControllerOnDidChangeKeyframeValue;
                    _timelineController.DidAddLayer += TimelineControllerOnDidAddLayer;
                    _timelineController.DidRemoveLayer += TimelineControllerOnDidRemoveLayer;
                }
            }
        }

        public TimelineControl()
        {
            _contextMenu = new ContextMenuStrip();

            TimelineController = new TimelineController();

            InitializeComponent();

            // Set the control's style so it won't flicker at every draw call:
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint, true);

            SetupControl();
        }

        private void SetupControl()
        {
            Controls.Add(sc_container);

            sc_container.Panel2.Controls.Add(_framesPanel);
            sc_container.Panel2.AutoScroll = true;
            _framesPanel.Dock = DockStyle.None;

            SetupFramesPanel();
            SetupScrollBars();
            UpdatePanelSizes();
        }

        private void SetupScrollBars()
        {
            _hScrollBar.Scroll += HScrollBarOnScroll;
            _vScrollBar.Scroll += VScrollBarOnScroll;

            _vScrollBar.AutoSize = false;
        }
        
        private void SetupFramesPanel()
        {
            var framesPanel = FramesPanel();

            framesPanel.Paint += FramesPanelOnPaint;
            framesPanel.MouseDown += FramesPanelOnMouseDown;
            framesPanel.MouseMove += FramesPanelOnMouseMove;

            //framesPanel.Controls.Add(_hScrollBar);
            //framesPanel.Controls.Add(_vScrollBar);
            _hScrollBar.Dock = DockStyle.Bottom;
            _vScrollBar.Dock = DockStyle.Right;
        }

        private Panel LayersPanel()
        {
            return sc_container.Panel1;
        }

        private Panel FramesPanel()
        {
            return _framesPanel;
        }

        private void InvalidateCurrentFrame()
        {
            for (int i = 0; i < TimelineController.LayerCount; i++)
            {
                var boundsForFrame = BoundsForFrame(_currentFrame, i);
                boundsForFrame.Y = 0;
                boundsForFrame.Height = FramesPanel().Height;
                boundsForFrame.Inflate(2, 2);
                FramesPanel().Invalidate(new Region(boundsForFrame));
            }
        }

        private void InvalidateCurrentLayer()
        {
            int startY = _frameCountHeight + CurrentLayer * _layerHeight;
            int endY = startY + _layerHeight;

            var boundsForLayer = new Rectangle(0, startY, FramesPanel().Width, endY - startY);
            FramesPanel().Invalidate(boundsForLayer);
        }

        private void InvalidatePanels()
        {
            LayersPanel().Invalidate();
            FramesPanel().Invalidate();
        }

        private void UpdatePanelSizes()
        {
            _framesPanel.Size = Size.Ceiling(new SizeF(GetTimelineWidth(), GetTimelineHeight()));
        }

        /// <summary>
        /// Changes the current frame being displayed
        /// </summary>
        /// <param name="newFrame">The new frame to display</param>
        private void ChangeFrame(int newFrame)
        {
            newFrame = Math.Max(0, Math.Min(LastVisibleFrame() - 1, newFrame));

            if (_currentFrame == newFrame)
                return;

            if (FrameChanged != null)
            {
                var evArgs = new FrameChangedEventArgs(_currentFrame, newFrame);

                FrameChanged.Invoke(this, evArgs);

                if (evArgs.Cancel)
                    return;
            }

            CurrentFrame = newFrame;
        }

        /// <summary>
        /// Changes the current layer being highlighted
        /// </summary>
        /// <param name="layer">The new layer to highlight</param>
        private void ChangeLayer(int layer)
        {
            layer = Math.Min(TimelineController.LayerCount, layer);

            if (_currentLayer == layer)
                return;

            if (LayerChanged != null)
            {
                var evArgs = new LayerChangedEventArgs(_currentLayer, layer);

                LayerChanged.Invoke(this, evArgs);

                if (evArgs.Cancel)
                    return;
            }

            CurrentLayer = layer;
        }

        private void ShowContextMenu([NotNull] Control control, int frame, int layerIndex, Point position)
        {
            _contextMenu.Items.Clear();

            var layer = TimelineController.LayerAtIndex(layerIndex);

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

                    TimelineController.RemoveKeyframe(kf.Value.Frame, layerIndex);
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

                    TimelineController.AddKeyframe(frame, layerIndex);
                    Invalidate();
                };
            }

            _contextMenu.Show(control, position);
        }

        private int LastVisibleFrame()
        {
            if (ShowExtendedTimeline)
            {
                const int minimumFrames = 100;
                const int frameBuffer = 50;

                return Math.Max(TimelineController.FrameCount + frameBuffer, minimumFrames);
            }

            return TimelineController.FrameCount;
        }

        private void UpdateFramePanelMouseEvent(Point point)
        {
            ChangeFrame(FrameOnX(point.X));
            ChangeLayer(LayerOnY(point.Y));
        }

        #region Geometry

        private int FrameOnX(float x)
        {
            return Math.Max(0, (int)Math.Floor(x / _frameWidth));
        }

        private int LayerOnY(float y)
        {
            return Math.Max(0, Math.Min(TimelineController.LayerCount - 1, (int)Math.Floor((y - _frameCountHeight) / _layerHeight)));
        }

        private RectangleF BoundsForFrame(int frame, int layer)
        {
            return new RectangleF(frame * _frameWidth, _frameCountHeight + layer * _layerHeight, _frameWidth, _layerHeight);
        }

        private float GetTimelineHeight()
        {
            return _frameCountHeight + TimelineController.LayerCount * _layerHeight;
        }

        private float GetTimelineWidth()
        {
            return LastVisibleFrame() * _frameWidth;
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            UpdatePanelSizes();
        }

        #region ScrollBar Event Handling

        private void HScrollBarOnScroll(object sender, ScrollEventArgs e)
        {
            
        }

        private void VScrollBarOnScroll(object sender, [NotNull] ScrollEventArgs e)
        {
            UpdatePanelSizes();
        }

        #endregion

        #region Timeline Event Handling

        private void TimelineControllerOnWillAddKeyframe(object sender, TimelineCancelableKeyframeValueChangeEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnWillRemoveKeyframe(object sender, TimelineCancelableRemoveKeyframeEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnWillChangeKeyframeValue(object sender, TimelineCancelableKeyframeValueChangeEventArgs e)
        {
            InvalidatePanels();
        }

        private void TimelineControllerOnWillRemoveLayer(object sender, TimelineCancelableLayerEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnWillAddLayer(object sender, TimelineCancelableLayerEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnDidAddKeyframe(object sender, TimelineKeyframeValueChangeEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnDidRemoveKeyframe(object sender, TimelineRemoveKeyframeEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnDidRemoveLayer(object sender, TimelineLayerEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnDidChangeKeyframeValue(object sender, TimelineKeyframeValueChangeEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        private void TimelineControllerOnDidAddLayer(object sender, TimelineLayerEventArgs e)
        {
            InvalidatePanels();
            UpdatePanelSizes();
        }

        #endregion Timeline Event Handler

        #region Frames Panel Event Handling

        private void FramesPanelOnMouseDown(object sender, [NotNull] MouseEventArgs e)
        {
            int layer = LayerOnY(e.Y);

            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                UpdateFramePanelMouseEvent(e.Location);
            }

            if (e.Button == MouseButtons.Right && layer >= 0 && layer < TimelineController.LayerCount)
            {
                ShowContextMenu(FramesPanel(), _currentFrame, layer, e.Location);
            }
        }

        private void FramesPanelOnMouseMove(object sender, [NotNull] MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                UpdateFramePanelMouseEvent(e.Location);
            }
        }

        private void FramesPanelOnPaint(object sender, [NotNull] PaintEventArgs e)
        {
            DrawFramesPanel(_framesPanel, e);
        }

        #endregion

        #region Rendering

        private void DrawFramesPanel([NotNull] Control control, [NotNull] PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            TimelineRenderer.DrawBackground(e.Graphics);
            TimelineRenderer.DrawFrameCounter(e.Graphics, TimelineController, control.Bounds, _currentFrame);
            TimelineRenderer.DrawLayerBackground(e.Graphics, control.Bounds, _currentLayer);
            TimelineRenderer.DrawTimelineBackground(e.Graphics, TimelineController, control.Bounds, _currentFrame);
            TimelineRenderer.DrawKeyframes(e.Graphics, TimelineController, control.Bounds, _currentFrame);
        }

        #endregion

        private class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                // Set the control's style so it won't flicker at every draw call:
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            }
        }
    }

    public class TimelineRenderer
    {
        private const int FrameCountHeight = 16;
        private const int LayerHeight = 22;
        private const float FrameWidth = 12;
        private static readonly Color BackgroundColor = Color.LightGray;
        private static readonly Color KeyframeColor = Color.White;
        private static readonly Color SelectedFrameColor = Color.CornflowerBlue;
        private static readonly Color SelectedLayerColor = Color.LightSkyBlue;

        private static RectangleF BoundsForFrame(int frame, int layer)
        {
            return new RectangleF(frame * FrameWidth, FrameCountHeight + layer * LayerHeight, FrameWidth, LayerHeight);
        }

        public static void DrawBackground([NotNull] Graphics graphics)
        {
            graphics.Clear(BackgroundColor);
        }

        public static void DrawLayerBackground([NotNull] Graphics graphics, Rectangle bounds, int currentLayer)
        {
            int y = FrameCountHeight + currentLayer * LayerHeight;

            var rect = new Rectangle(0, y, bounds.Width, LayerHeight);

            using var solidBrush = new SolidBrush(SelectedLayerColor);
            graphics.FillRectangle(solidBrush, rect);
        }

        public static void DrawFrameCounter([NotNull] Graphics graphics, [NotNull] TimelineController timeline, Rectangle bounds, int currentFrame)
        {
            var font = new Font(FontFamily.GenericSansSerif, 8);

            graphics.DrawLine(Pens.White, 0, FrameCountHeight, bounds.Width, FrameCountHeight);

            int frame = 0;
            for (float x = 0; x < bounds.Width; x += FrameWidth)
            {
                var currentFrameRect = new RectangleF(x, 0, FrameWidth, FrameCountHeight);
                if (frame == currentFrame)
                {
                    using var brush = new SolidBrush(SelectedFrameColor);
                    graphics.FillRectangle(brush, currentFrameRect);
                }

                if (frame == 0 || (frame + 1) % 5 == 0)
                {
                    string frameString = (frame + 1).ToString();
                    graphics.DrawString(frameString, font, Brushes.DimGray, x, 0);

                    if (frame == currentFrame)
                    {
                        var state = graphics.Save();

                        graphics.IntersectClip(currentFrameRect);
                        graphics.DrawString(frameString, font, Brushes.White, x, 0);

                        graphics.Restore(state);
                    }
                }

                frame += 1;
            }

            font.Dispose();
        }

        public static void DrawTimelineBackground([NotNull] Graphics graphics, [NotNull] TimelineController timeline, Rectangle bounds, int currentFrame)
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

            for (int layerIndex = 0; layerIndex < timeline.LayerCount; layerIndex++)
            {
                int frame = 0;
                for (float x = 0; x < bounds.Width; x += FrameWidth)
                {
                    int y = FrameCountHeight + layerIndex * LayerHeight;

                    var top = new Point((int)x, y);
                    var bottom = new Point((int)x, y + LayerHeight);

                    graphics.DrawLine(frame % 5 == 0 ? oddFrameSeparatorPen : frameSeparatorPen, top, bottom);

                    if (frame == currentFrame)
                    {
                        graphics.FillRectangle(Brushes.CornflowerBlue, BoundsForFrame(frame, layerIndex));
                    }

                    frame += 1;
                }
            }

            frameSeparatorPen.Dispose();
            oddFrameSeparatorPen.Dispose();
        }

        public static void DrawKeyframes([NotNull] Graphics graphics, [NotNull] TimelineController timeline, Rectangle bounds, int currentFrame)
        {
            for (int layerIndex = 0; layerIndex < timeline.LayerCount; layerIndex++)
            {
                var layer = timeline.LayerAtIndex(layerIndex);

                int frame = 0;
                for (float x = 0; x < bounds.Width; x += FrameWidth)
                {
                    if (frame == layer.FrameCount)
                        break;

                    if (layer.KeyframeForFrame(frame) != null)
                    {
                        DrawKeyframe(graphics, frame, layerIndex, timeline, currentFrame);
                    }

                    frame += 1;
                }
            }
        }

        public static void DrawKeyframe([NotNull] Graphics graphics, int frame, int currentLayer, [NotNull] TimelineController timeline, int currentFrame)
        {
            var frameBounds = new RectangleF(0, 0, FrameWidth, LayerHeight - 1);

            var state = graphics.Save();

            float x = frame * FrameWidth;
            float y = FrameCountHeight + currentLayer * LayerHeight;

            graphics.TranslateTransform(x, y);

            var dashedPen = new Pen(Color.LightGray)
            {
                DashPattern = new[] { 3.0f, 3.0f },
                DashCap = DashCap.Flat
            };
            var outlinePen = new Pen(Color.Gray);
            var bodyFillBrush = new SolidBrush(frame == currentFrame ? Color.White : Color.Gray);

            var layer = timeline.LayerAtIndex(currentLayer);
            var relationship = layer.RelationshipToFrame(frame);

            if (relationship != KeyframePosition.None)
            {
                using var brush = new SolidBrush(frame == currentFrame ? SelectedFrameColor : KeyframeColor);
                graphics.FillRectangle(brush, frameBounds);
            }

            if (relationship == KeyframePosition.First || relationship == KeyframePosition.Full)
            {
                var circleCenter = new PointF(frameBounds.Width / 2, frameBounds.Height / 2);
                const float circleRadius = 2.5f;
                var ellipse = new RectangleF(circleCenter.X - circleRadius, circleCenter.Y - circleRadius, circleRadius * 2, circleRadius * 2);
                graphics.FillEllipse(bodyFillBrush, ellipse);
            }

            switch (relationship)
            {
                case KeyframePosition.None:
                    break;
                case KeyframePosition.Full:
                    graphics.DrawRectangle(outlinePen, frameBounds.X, frameBounds.Y, frameBounds.Width, frameBounds.Height);
                    break;
                case KeyframePosition.First:
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Left, frameBounds.Bottom);
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Right, frameBounds.Top);
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Bottom, frameBounds.Right, frameBounds.Bottom);
                    graphics.DrawLine(dashedPen, frameBounds.Right, frameBounds.Top, frameBounds.Right, frameBounds.Bottom);
                    break;
                case KeyframePosition.Center:
                    graphics.DrawLine(dashedPen, frameBounds.Left, frameBounds.Top, frameBounds.Left, frameBounds.Bottom);
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Right, frameBounds.Top);
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Bottom, frameBounds.Right, frameBounds.Bottom);
                    graphics.DrawLine(dashedPen, frameBounds.Right, frameBounds.Top, frameBounds.Right, frameBounds.Bottom);
                    break;
                case KeyframePosition.Last:
                    graphics.DrawLine(dashedPen, frameBounds.Left, frameBounds.Top, frameBounds.Left, frameBounds.Bottom);
                    graphics.DrawLine(outlinePen, frameBounds.Right, frameBounds.Top, frameBounds.Right, frameBounds.Bottom);
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Top, frameBounds.Right, frameBounds.Top);
                    graphics.DrawLine(outlinePen, frameBounds.Left, frameBounds.Bottom, frameBounds.Right, frameBounds.Bottom);
                    break;
            }

            graphics.Restore(state);

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

    /// <summary>
    /// Event args for a layer changed event
    /// </summary>
    public class LayerChangedEventArgs : EventArgs
    {
        public int OldLayer { get; }
        public int NewLayer { get; }
        public bool Cancel { get; set; }

        public LayerChangedEventArgs(int oldLayer, int newLayer)
        {
            OldLayer = oldLayer;
            NewLayer = newLayer;
        }
    }
}

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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Views.Controls;
using PixelariaLib.Controllers.DataControllers;
using PixelariaLib.Data;
using PixelariaLib.Timeline;
using PixelariaLib.Views.Controls;

namespace Pixelaria.Views.ModelViews
{
    public partial class FrameOriginView : Form
    {
        private readonly AnimationController _animation;
        private TimelineController _timelineController;

        public FrameOriginView(AnimationController animation)
        {
            _animation = animation;
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            Text = $@"Frame Origin - [{_animation.Name}]";

            _timelineController = new TimelineController();
            _timelineController.AddLayer("origin", new FrameOriginKeyframeSource(_animation), new FrameOriginTimelineController());
            if (_timelineController.LayerAtIndex(0).KeyframeExactlyOnFrame(0) == null)
            {
                _timelineController.AddKeyframe(0, 0, Point.Empty);
            }

            timelineControl.TimelineController = _timelineController;
            timelineControl.FrameChanged += TimelineScrubControlOnFrameChanged;
            timelineControl.WillAddKeyframe += TimelineControlOnWillAddKeyframe;
            timelineControl.WillRemoveKeyframe += TimelineControlOnWillRemoveKeyframe;

            timelineControl.ShowExtendedTimeline = false;

            zpb_framePreview.HookToControl(this);

            LoadFrame(0);
        }

        private void TimelineScrubControlOnFrameChanged(object sender, FrameChangedEventArgs e)
        {
            LoadFrame(e.NewFrame);
        }

        private void TimelineControlOnWillAddKeyframe(object sender, TimelineControlKeyframeEventArgs e)
        {
            zpb_framePreview.Invalidate();
        }

        private void TimelineControlOnWillRemoveKeyframe(object sender, TimelineControlKeyframeEventArgs e)
        {
            zpb_framePreview.Invalidate();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            _animation.ApplyChanges();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void LoadFrame(int index)
        {
            if (_animation.FrameCount <= index)
                return;

            var frame = _animation.GetFrameController(_animation.GetFrameAtIndex(index));
            zpb_framePreview.LoadFrame(frame, _timelineController);
        }

        internal class FrameOriginEditImageBox : ZoomablePictureBox
        {
            private FrameController _frame;
            private TimelineController _timelineController;

            public void LoadFrame([NotNull] FrameController frame, TimelineController timelineController)
            {
                _frame = frame;
                _timelineController = timelineController;
                Image = frame.GetComposedBitmap();
            }

            protected override void OnPaint(PaintEventArgs pe)
            {
                base.OnPaint(pe);

                if (_frame != null)
                {
                    var value = _timelineController.CreatePlayer().ValueForFrame(_frame.Index, 0);

                    if (value is Point p)
                    {
                        var halfSize = new Size(Properties.Resources.frame_origin_marker.Size.Width / 2, Properties.Resources.frame_origin_marker.Size.Height / 2);
                        var point = Point.Subtract(p, halfSize);
                        pe.Graphics.DrawImage(Properties.Resources.frame_origin_marker, point);
                    }
                }
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                base.OnMouseDown(e);
                if (_frame == null)
                    return;

                if (e.Button == MouseButtons.Left)
                {
                    _timelineController.SetKeyframeValue(_frame.Index, 0, ClippedPoint(e.Location));
                    Invalidate();
                }
            }

            protected override void OnMouseMove(MouseEventArgs e)
            {
                base.OnMouseMove(e);

                if (e.Button == MouseButtons.Left)
                {
                    _timelineController.SetKeyframeValue(_frame.Index, 0, ClippedPoint(e.Location));
                    Invalidate();
                }
            }

            private Point ClippedPoint(Point input)
            {
                if (_frame == null)
                    return GetAbsolutePoint(input);

                var absolute = GetAbsolutePoint(input);
                return new Point(Math.Max(0, Math.Min(_frame.Width, absolute.X)), Math.Max(0, Math.Min(_frame.Height, absolute.Y)));
            }
        }
    }

    public class FrameOriginKeyframeSource : IKeyframeSource
    {
        private readonly List<Keyframe> _keyframes = new List<Keyframe>();
        private readonly AnimationController _animation;
        public int FrameCount => _animation.FrameCount;

        public IReadOnlyList<Keyframe> Keyframes => _keyframes;

        public FrameOriginKeyframeSource(AnimationController animation)
        {
            _animation = animation;
            FillFrames();
        }

        private void FillFrames()
        {
            _keyframes.Clear();

            var keyframes = 
                Enumerable
                    .Range(0, _animation.FrameCount)
                    .Where(i => _animation.MetadataForFrame(i)[FrameMetadataKeys.FrameOrigin] != null).ToList();

            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                int start = keyframes[i];
                int length = keyframes[i + 1] - start;

                var kf = new Keyframe(start, length, _animation.MetadataForFrame(i)[FrameMetadataKeys.FrameOrigin]);

                _keyframes.Add(kf);
            }
        }

        public void AddKeyframe(Keyframe keyframe)
        {
            _animation.MetadataForFrame(keyframe.Frame)[FrameMetadataKeys.FrameOrigin] = keyframe.Value is Point p ? (object)p : null;

            FillFrames();
        }

        public void SetKeyframeValue(int frameIndex, object value)
        {
            _animation.MetadataForFrame(frameIndex)[FrameMetadataKeys.FrameOrigin] = value is Point p ? (object)p : null;
        }

        public object ValueForKeyframe(int frameIndex)
        {
            return _animation.MetadataForFrame(frameIndex)[FrameMetadataKeys.FrameOrigin] is Point p ? (object)p : null;
        }

        public void RemoveKeyframe(int frameIndex)
        {
            _animation.MetadataForFrame(frameIndex)[FrameMetadataKeys.FrameOrigin] = null;

            FillFrames();
        }
    }

    public class FrameOriginTimelineController : ITimelineLayerController
    {
        public object DefaultKeyframeValue()
        {
            return Point.Empty;
        }

        public object DuplicateKeyframeValue(object value)
        {
            return value;
        }

        public object InterpolatedValue(object start, object end, float ratio)
        {
            return start;
        }
    }
}

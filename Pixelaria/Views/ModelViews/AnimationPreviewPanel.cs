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
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;

using PixelariaLib.Data;
using PixelariaLib.Views.Controls;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Provides an interface to preview animations
    /// </summary>
    public partial class AnimationPreviewPanel : UserControl
    {
        /// <summary>
        /// The current animation set to play on this AnimationPreviewPanel
        /// </summary>
        private IAnimation _currentAnimation;

        /// <summary>
        /// Timer used to animate the animation preview
        /// </summary>
        private readonly Timer _animationTimer;

        /// <summary>
        /// The current animation frame being displayed
        /// </summary>
        private int _currentFrame = -1;

        /// <summary>
        /// Whether the current animation is playing
        /// </summary>
        private bool _playing;

        /// <summary>
        /// The current frame bitmap being displayed on this AnimationPreviewPanel
        /// </summary>
        private Bitmap _frameBitmap;

        /// <summary>
        /// The maximum zoom factor allowed on the control
        /// </summary>
        private const decimal MaxZoom = 15;

        /// <summary>
        /// AnimationPreviewPanel constructor
        /// </summary>
        public AnimationPreviewPanel()
        {
            InitializeComponent();

            _animationTimer = new Timer();
            _animationTimer.Tick += animationTimer_Tick;

            RefreshPreviewPanel();
        }

        /// <summary>
        /// Disables this preview panel
        /// </summary>
        public void Disable()
        {
            if (_playing)
                _animationTimer.Stop();
        }

        /// <summary>
        /// Enables this preview panel
        /// </summary>
        public void Enable()
        {
            if (_playing)
            {
                _animationTimer.Start();
            }
        }

        /// <summary>
        /// Loads the given animation into this AnimationPreviewPanel
        /// </summary>
        /// <param name="animation">The animation to load</param>
        /// <param name="resetPlayback">Whether to reset the playback of the animation back to the start</param>
        public void LoadAnimation(IAnimation animation, bool resetPlayback = true)
        {
            pnl_preview.Image = null;

            _currentAnimation = animation;

            if (animation != null && animation.FrameCount != 0 && animation.PlaybackSettings.FPS != 0)
            {
                // Clip the current frame to be within the range of the animation
                _currentFrame = Math.Max(0, Math.Min(animation.FrameCount - 1, _currentFrame));

                RefreshPreviewPanel();

                AutoAdjustZoom();

                if (resetPlayback)
                {
                    if (_currentAnimation.PlaybackSettings.FPS == 0 || _currentAnimation.FrameCount <= 1)
                    {
                        cb_playPreview.Checked = false;
                        InternalSetPlayback(false);
                    }
                    else
                    {
                        _currentFrame = 0;

                        _animationTimer.Interval = 1000 / (_currentAnimation.PlaybackSettings.FPS == -1 ? 60 : _currentAnimation.PlaybackSettings.FPS);
                        cb_playPreview.Checked = true;
                        InternalSetPlayback(true);
                    }
                }
            }
            else
            {
                // Set the control state to be disabled
                _currentFrame = 0;
                _playing = false;
                cb_playPreview.Checked = false;
                InternalSetPlayback(false);

                RefreshPreviewPanel();

                _animationTimer.Stop();
                _animationTimer.Interval = 1000;
            }
        }

        /// <summary>
        /// Changes the zoom of the preview animation
        /// </summary>
        /// <param name="newZoom">The new zoom size</param>
        /// <param name="updateControls">Whether to update the zoom controls after the zoom is set</param>
        public void ChangePreviewZoom(decimal newZoom, bool updateControls = false)
        {
            if (_currentAnimation == null || _currentAnimation.FrameCount == 0)
                return;

            pnl_preview.Width = (int)(_currentAnimation.Width * newZoom);
            pnl_preview.Height = (int)(_currentAnimation.Height * newZoom);

            if (updateControls)
            {
                nud_previewZoom.Value = newZoom;
                tb_zoomTrack.Value = (int)(newZoom * 4);
            }
        }

        /// <summary>
        /// Changes the current displayed frame
        /// </summary>
        /// <param name="newFrame">The new frame to display</param>
        public void ChangeFrame(int newFrame)
        {
            if (_currentAnimation == null || _currentAnimation.FrameCount == 0)
                return;

            _currentFrame = newFrame;

            lbl_currentFrame.Text = "" + (newFrame + 1);

            _frameBitmap?.Dispose();

            _frameBitmap = _currentAnimation.GetComposedBitmapForFrame(newFrame);

            pnl_preview.Image = _frameBitmap;

            tlc_timeline.CurrentFrame = _currentFrame;
        }

        /// <summary>
        /// Sets whether or not to play the current animation
        /// </summary>
        /// <param name="play">Whether or not to play the current animation</param>
        public void SetPlayback(bool play)
        {
            if (_playing == play)
                return;

            cb_playPreview.Checked = play;
            InternalSetPlayback(play);
        }

        /// <summary>
        /// Sets whether or not to play the current animation
        /// </summary>
        /// <param name="play">Whether or not to play the current animation</param>
        private void InternalSetPlayback(bool play)
        {
            if (_playing == play)
                return;

            _playing = play;

            if (play)
            {
                _animationTimer.Start();
            }
            else
            {
                _animationTimer.Stop();
            }
        }

        /// <summary>
        /// Automatically adjust the zoom to fit the whole panel
        /// </summary>
        private void AutoAdjustZoom()
        {
            if (_currentAnimation == null || _currentAnimation.FrameCount == 0)
                return;

            decimal zoom = (decimal)(Width - pnl_preview.Location.X) / _currentAnimation.Width;

            zoom = Math.Floor(zoom * 4) / 4;

            ChangePreviewZoom(Math.Min(nud_previewZoom.Maximum, Math.Max(zoom, nud_previewZoom.Minimum)), true);
        }

        /// <summary>
        /// Refreshes the preview panel
        /// </summary>
        private void RefreshPreviewPanel()
        {
            if (_currentAnimation == null || _currentAnimation.FrameCount == 0)
            {
                pnl_preview.Width = 32;
                pnl_preview.Height = 32;

                lbl_currentFrame.Text = @"0";
                lbl_frameCount.Text = @"0";

                tlc_timeline.Minimum = 0;
                tlc_timeline.Maximum = 1;
                tlc_timeline.Range = new Point(0, 1);
                tlc_timeline.CurrentFrame = 0;
                tlc_timeline.Enabled = false;

                nud_previewZoom.Maximum = 15;
                nud_previewZoom.Enabled = false;

                tb_zoomTrack.Minimum = 1;
                tb_zoomTrack.Value = 4;
                tb_zoomTrack.Maximum = (int)((nud_previewZoom.Maximum) * 4);
                tb_zoomTrack.Enabled = false;

                cb_playPreview.Checked = false;
                cb_playPreview.Enabled = false;

                return;
            }
            
            pnl_preview.Width = (int)(_currentAnimation.Width * nud_previewZoom.Value);
            pnl_preview.Height = (int)(_currentAnimation.Height * nud_previewZoom.Value);

            ChangeFrame(0);

            lbl_frameCount.Text = _currentAnimation.FrameCount + "";

            tlc_timeline.Minimum = 1;
            tlc_timeline.Maximum = _currentAnimation.FrameCount;
            tlc_timeline.Range = new Point(0, _currentAnimation.FrameCount);
            tlc_timeline.CurrentFrame = 0;
            tlc_timeline.Enabled = true;

            // Update the maximum scale for the zoom numeric up and down
            nud_previewZoom.Maximum = (int)Math.Min(Math.Min(MaxZoom, (decimal)4096 / _currentAnimation.Width), (decimal)4096 / _currentAnimation.Height);
            nud_previewZoom.Enabled = true;

            tb_zoomTrack.Minimum = 1;
            tb_zoomTrack.Value = 4;
            tb_zoomTrack.Maximum = (int)((nud_previewZoom.Maximum) * 4);
            tb_zoomTrack.Enabled = true;

            cb_playPreview.Enabled = true;
        }

        // 
        // Animation Timer tick
        // 
        void animationTimer_Tick(object sender, EventArgs e)
        {
            if (tlc_timeline.DraggingFrame || _currentAnimation == null || _currentAnimation.FrameCount <= 1 || ParentForm == null)
                return;

            int newFrame = _currentFrame + 1;

            if (newFrame > tlc_timeline.GetRange().X + tlc_timeline.GetRange().Y - 1)
            {
                newFrame = tlc_timeline.GetRange().X - 1;
            }

            ChangeFrame(newFrame);
            tlc_timeline.CurrentFrame = _currentFrame + 1;

            if (_currentAnimation.PlaybackSettings.FPS == 0)
            {
                InternalSetPlayback(false);

                cb_playPreview.Checked = false;
            }
            else
            {
                _animationTimer.Interval = 1000 / (_currentAnimation.PlaybackSettings.FPS == -1 ? 60 : _currentAnimation.PlaybackSettings.FPS);

                if (_playing == false)
                {
                    cb_playPreview.Checked = true;

                    InternalSetPlayback(true);
                }
            }
        }

        // 
        // Preview Zoom numeric up and down change
        // 
        private void nud_previewZoom_ValueChanged(object sender, EventArgs e)
        {
            ChangePreviewZoom(nud_previewZoom.Value);
        }

        // 
        // Timeline frame changed
        // 
        private void tlc_timeline_FrameChanged(object sender, [NotNull] FrameChangedEventArgs e)
        {
            ChangeFrame(e.NewFrame - 1);            
        }

        // 
        // Zoom Trackbar value change
        // 
        private void tb_zoomTrack_Scroll(object sender, EventArgs e)
        {
            nud_previewZoom.Value = (decimal)tb_zoomTrack.Value / 4;
            ChangePreviewZoom((decimal)tb_zoomTrack.Value / 4);
        }

        // 
        // Play Combobox check
        // 
        private void cb_playPreview_CheckedChanged(object sender, EventArgs e)
        {
            InternalSetPlayback(cb_playPreview.Checked);
        }

        // 
        // Preview Panel double click
        // 
        private void pnl_preview_DoubleClick(object sender, EventArgs e)
        {
            AutoAdjustZoom();
        }
    }
}
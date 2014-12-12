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

using Pixelaria.Data;
using Pixelaria.Views.Controls;

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
        private Animation currentAnimation;

        /// <summary>
        /// Timer used to animate the animation preview
        /// </summary>
        private Timer animationTimer;

        /// <summary>
        /// The current animation frame being displayed
        /// </summary>
        private int currentFrame;

        /// <summary>
        /// States whether the user is currently dragging the trackbar
        /// </summary>
        private bool draggingTrackbar;

        /// <summary>
        /// Whether the current animation is playing
        /// </summary>
        private bool playing = false;

        /// <summary>
        /// The maximum zoom factor allowed on the control
        /// </summary>
        private decimal maxZoom = 15;

        /// <summary>
        /// AnimationPreviewPanel constructor
        /// </summary>
        public AnimationPreviewPanel()
        {
            InitializeComponent();

            animationTimer = new Timer();
            animationTimer.Tick += new EventHandler(animationTimer_Tick);

            draggingTrackbar = false;

            RefreshPreviewPanel();
        }

        /// <summary>
        /// Disables this preview panel
        /// </summary>
        public void Disable()
        {
            if (playing)
                animationTimer.Stop();
        }

        /// <summary>
        /// Enables this preview panel
        /// </summary>
        public void Enable()
        {
            if (playing)
            {
                animationTimer.Start();
            }
        }

        /// <summary>
        /// Loads the given animation into this AnimationPreviewPanel
        /// </summary>
        /// <param name="animation">The animation to load</param>
        /// <param name="resetPlayback">Whether to reset the playback of the animation back to the start</param>
        public void LoadAnimation(Animation animation, bool resetPlayback = true)
        {
            pnl_preview.Image = null;

            currentAnimation = animation;

            if (animation != null && animation.FrameCount != 0)
            {
                // Clip the current frame to be within the range of the animation
                currentFrame = Math.Max(0, Math.Min(animation.FrameCount - 1, currentFrame));

                RefreshPreviewPanel();

                AutoAdjustZoom();

                if (resetPlayback)
                {
                    if (currentAnimation.PlaybackSettings.FPS == 0)
                    {
                        cb_playPreview.Checked = false;
                        InternalSetPlayback(false);
                    }
                    else
                    {
                        currentFrame = 0;

                        animationTimer.Interval = 1000 / (currentAnimation.PlaybackSettings.FPS == -1 ? 60 : currentAnimation.PlaybackSettings.FPS);
                        cb_playPreview.Checked = true;
                        InternalSetPlayback(true);
                    }
                }
            }
            else
            {
                // Set the control state to be disabled
                currentFrame = 0;
                playing = false;
                cb_playPreview.Checked = false;
                InternalSetPlayback(false);

                RefreshPreviewPanel();

                animationTimer.Interval = 1000;
            }
        }

        /// <summary>
        /// Changes the zoom of the preview animation
        /// </summary>
        /// <param name="newZoom">The new zoom size</param>
        /// <param name="updateControls">Whether to update the zoom controls after the zoom is set</param>
        public void ChangePreviewZoom(decimal newZoom, bool updateControls = false)
        {
            if (currentAnimation == null || currentAnimation.FrameCount == 0)
                return;

            pnl_preview.Width = (int)(currentAnimation.Width * newZoom);
            pnl_preview.Height = (int)(currentAnimation.Height * newZoom);

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
            if (currentAnimation == null || currentAnimation.FrameCount == 0)
                return;

            currentFrame = newFrame;

            lbl_currentFrame.Text = "" + (newFrame + 1);
            pnl_preview.Image = currentAnimation.GetFrameAtIndex(newFrame).GetComposedBitmap();
        }

        /// <summary>
        /// Sets whether or not to play the current animation
        /// </summary>
        /// <param name="play">Whether or not to play the current animation</param>
        public void SetPlayback(bool play)
        {
            if (playing == play)
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
            if (playing == play)
                return;

            playing = play;

            if (play)
            {
                animationTimer.Start();
            }
            else
            {
                animationTimer.Stop();
            }
        }

        /// <summary>
        /// Automatically adjust the zoom to fit the whoe panel
        /// </summary>
        private void AutoAdjustZoom()
        {
            if (currentAnimation == null || currentAnimation.FrameCount == 0)
                return;

            decimal zoom = (decimal)(this.Width - pnl_preview.Location.X) / currentAnimation.Width;

            zoom = Math.Floor(zoom * 4) / 4;

            ChangePreviewZoom(Math.Min(nud_previewZoom.Maximum, Math.Max(zoom, nud_previewZoom.Minimum)), true);
        }

        /// <summary>
        /// Refreshes the preview panel
        /// </summary>
        private void RefreshPreviewPanel()
        {
            if (currentAnimation == null || currentAnimation.FrameCount == 0)
            {
                pnl_preview.Width = 32;
                pnl_preview.Height = 32;

                lbl_currentFrame.Text = "0";
                lbl_frameCount.Text = "0";

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
            
            pnl_preview.Width = (int)(currentAnimation.Width * nud_previewZoom.Value);
            pnl_preview.Height = (int)(currentAnimation.Height * nud_previewZoom.Value);

            ChangeFrame(0);

            lbl_frameCount.Text = currentAnimation.FrameCount + "";

            tlc_timeline.Minimum = 1;
            tlc_timeline.Maximum = currentAnimation.FrameCount;
            tlc_timeline.Range = new Point(0, currentAnimation.FrameCount);
            tlc_timeline.CurrentFrame = 0;
            tlc_timeline.Enabled = true;

            // Update the maximum scale for the zoom numeric up and down
            nud_previewZoom.Maximum = Math.Min(Math.Min(maxZoom, 4096 / currentAnimation.Width), 4096 / currentAnimation.Height);
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
            if (draggingTrackbar || tlc_timeline.DraggingFrame || currentAnimation == null || currentAnimation.FrameCount == 0 || ParentForm == null)
                return;

            int newFrame = currentFrame + 1;

            if (newFrame > tlc_timeline.GetRange().X + tlc_timeline.GetRange().Y - 1)
            {
                newFrame = tlc_timeline.GetRange().X - 1;
            }

            ChangeFrame(newFrame);
            tlc_timeline.CurrentFrame = currentFrame + 1;

            if (currentAnimation.PlaybackSettings.FPS == 0)
            {
                InternalSetPlayback(false);
            }
            else
            {
                animationTimer.Interval = 1000 / (currentAnimation.PlaybackSettings.FPS == -1 ? 60 : currentAnimation.PlaybackSettings.FPS);

                if (playing == false)
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
        private void tlc_timeline_FrameChanged(object sender, FrameChangedEventArgs e)
        {
            ChangeFrame(e.NewFrame - 1);            
        }

        // 
        // Timeline Trackbar mouse down
        // 
        private void tb_timeline_MouseDown(object sender, MouseEventArgs e)
        {
            draggingTrackbar = true;
        }

        // 
        // Timeline Trackbar mouse up
        // 
        private void tb_timeline_MouseUp(object sender, MouseEventArgs e)
        {
            draggingTrackbar = false;
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
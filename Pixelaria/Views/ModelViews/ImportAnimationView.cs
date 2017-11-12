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
using System.IO;
using System.Windows.Forms;

using JetBrains.Annotations;

using Pixelaria.Controllers;
using Pixelaria.Controllers.Importers;
using Pixelaria.Data;
using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.PaintTools;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used as interface for new animation creation
    /// </summary>
    public partial class ImportAnimationView : Form
    {
        private readonly SheetPreviewHoverMouseTool _sheetPreviewTool;

        /// <summary>
        /// The main controller owning this form
        /// </summary>
        private readonly Controller _controller;

        /// <summary>
        /// The loaded image sheet
        /// </summary>
        private Bitmap _spriteSheet;

        /// <summary>
        /// The current SheetSettings being edited on the form
        /// </summary>
        private SheetSettings _sheetSettings;

        /// <summary>
        /// The current preview animation
        /// </summary>
        private PreviewAnimation _currentPreviewAnimation;
        
        /// <summary>
        /// Optional AnimationSheet that will own the newly created animation
        /// </summary>
        [CanBeNull]
        private readonly AnimationSheet _parentSheet;

        /// <summary>
        /// Intiializes a new instance of the ImportAnimationView class
        /// </summary>
        /// <param name="controller">The controller owning this form</param>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created animation</param>
        public ImportAnimationView([NotNull] Controller controller, [CanBeNull] AnimationSheet parentSheet = null)
        {
            _controller = controller;
            _parentSheet = parentSheet;

            InitializeComponent();

            cpb_sheetPreview.PictureBox.PanMode = PictureBoxPanMode.LeftMouseDrag;
            cpb_sheetPreview.PictureBox.SetBitmap(new Bitmap(32, 32));
            cpb_sheetPreview.Init();

            _sheetPreviewTool = new SheetPreviewHoverMouseTool();
            cpb_sheetPreview.CurrentPaintTool = _sheetPreviewTool;

            _sheetPreviewTool.Importer = controller.AnimationImporter;

            _spriteSheet = null;

            _sheetSettings = new SheetSettings(false, 32, 32);

            ValidateFields();
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        private void ValidateFields()
        {
            bool valid = true;
            const bool alert = false;

            if (_spriteSheet == null)
            {
                lbl_alertLabel.Text = @"Start by loading an image clicking the 'Browse...' button";

                pnl_alertPanel.Visible = true;

                btn_ok.Enabled = false;

                return;
            }

            // Animation name
            var validation = _controller.AnimationValidator.ValidateAnimationName(txt_animationName.Text);
            if (validation != "")
            {
                txt_animationName.BackColor = Color.LightPink;
                lbl_error.Text = validation;
                valid = false;
            }
            else
            {
                txt_animationName.BackColor = Color.White;
            }

            pnl_errorPanel.Visible = !valid;
            pnl_alertPanel.Visible = alert;

            btn_ok.Enabled = valid;
        }

        /// <summary>
        /// Shows the interface to load a new sprite sheet
        /// </summary>
        private void LoadSheet()
        {
            var image = _controller.ShowLoadImage(out string filePath, owner: this);

            if (image == null)
                return;

            // Dispose of the current sprite sheet if it's loaded
            _spriteSheet?.Dispose();

            txt_fileName.Text = filePath;

            _spriteSheet = new Bitmap(image);

            cpb_sheetPreview.PictureBox.SetBitmap(_spriteSheet);
            _sheetPreviewTool.LoadPreview(_spriteSheet, _sheetSettings);

            txt_animationName.Text = Path.GetFileNameWithoutExtension(filePath);

            // Setup the boundaries for the numeric up downs
            nud_width.Maximum = _spriteSheet.Width;
            nud_height.Maximum = _spriteSheet.Height;

            nud_startX.Maximum = _spriteSheet.Width - 1;
            nud_startY.Maximum = _spriteSheet.Height - 1;

            tssl_dimensions.Text = _spriteSheet.Width + @" x " + _spriteSheet.Height;

            // Enable the width/height fitting buttons
            btn_fitWidthLeft.Enabled = btn_fitWidthRight.Enabled = btn_fitHeightLeft.Enabled = btn_fitHeightRight.Enabled = true;

            RefreshSheetSettings();

            GenerateAnimationPreview();

            ValidateFields();
        }

        /// <summary>
        /// Generates an animation preview based on the current settings
        /// </summary>
        private void GenerateAnimationPreview()
        {
            if (_spriteSheet == null)
                return;

            _currentPreviewAnimation = null;

            if (nud_frameCount.Value != 0)
            {
                var rects = _controller.AnimationImporter.GenerateFrameBounds(_spriteSheet.Size, _sheetSettings);
                _currentPreviewAnimation = new PreviewAnimation(_spriteSheet, rects);

                var playback = _currentPreviewAnimation.PlaybackSettings;

                playback.FPS = (int) nud_fps.Value;
                playback.FrameSkip = cb_frameskip.Checked;

                _currentPreviewAnimation.PlaybackSettings = playback;
            }

            ap_animationPreview.LoadAnimation(_currentPreviewAnimation);
        }

        /// <summary>
        /// Refreshes the sheet settings object
        /// </summary>
        private void RefreshSheetSettings()
        {
            if (_spriteSheet == null)
                return;

            _sheetSettings = new SheetSettings
            {
                FrameWidth = (int)nud_width.Value,
                FrameHeight = (int)nud_height.Value,
                FrameCount = (int)nud_frameCount.Value,
                FirstFrame = (int)nud_skipCount.Value,
                FlipFrames = cb_reverseFrameOrder.Checked,
                OffsetX = (int)nud_startX.Value,
                OffsetY = (int)nud_startY.Value
            };

            _sheetPreviewTool.SheetSettings = _sheetSettings;

            GenerateAnimationPreview();
        }

        /// <summary>
        /// Imports the animation to the current bundle
        /// </summary>
        private void ImportAnimation()
        {
            // Quit if no valid animation is currently being displayed
            if (nud_frameCount.Value == 0 || _currentPreviewAnimation == null || _currentPreviewAnimation.FrameCount <= 0)
                return;

            var anim = _controller.AnimationImporter.ImportAnimationFromImage(txt_animationName.Text, _spriteSheet, _sheetSettings);

            var playback = _currentPreviewAnimation.PlaybackSettings;

            playback.FPS = (int)nud_fps.Value;
            playback.FrameSkip = cb_frameskip.Checked;

            anim.PlaybackSettings = playback;

            anim.Name = txt_animationName.Text;

            _controller.AddAnimation(anim, true, _parentSheet);

            _currentPreviewAnimation = null;
            ap_animationPreview.LoadAnimation(null);
        }

        // 
        // Form Closing event handler
        // 
        private void ImportAnimationView_FormClosed(object sender, FormClosedEventArgs e)
        {
            _spriteSheet?.Dispose();
            _currentPreviewAnimation = null;
            ap_animationPreview.LoadAnimation(null);
        }

        // 
        // Form Shown event handler
        // 
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            LoadSheet();
        }

        // 
        // Animation Name textbox input
        // 
        private void txt_animationName_TextChanged(object sender, EventArgs e)
        {
            ValidateFields();
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            ImportAnimation();
        }

        // 
        // Browse File button
        // 
        private void btn_browse_Click(object sender, EventArgs e)
        {
            LoadSheet();
        }

        // 
        // Frame Start X up down value change
        // 
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }
        // 
        // Frame Start Y up down value change
        // 
        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }

        // 
        // Frame Width up down value change
        // 
        private void nud_width_ValueChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }
        // 
        // Frame Height up down value change
        // 
        private void nud_height_ValueChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }

        // 
        // Frame Count numeric up down value change
        // 
        private void nud_frameCount_ValueChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }

        // 
        // Skip Count numeric up down value change
        // 
        private void nud_skipCount_ValueChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }

        // 
        // Reverse Frame Order combobox value change
        // 
        private void cb_reverseFrameOrder_CheckedChanged(object sender, EventArgs e)
        {
            RefreshSheetSettings();
        }

        // 
        // FPS numeric up down value change
        // 
        private void nud_fps_ValueChanged(object sender, EventArgs e)
        {
            if (_currentPreviewAnimation != null)
            {
                var playback = _currentPreviewAnimation.PlaybackSettings;

                playback.FPS = (int)nud_fps.Value;

                _currentPreviewAnimation.PlaybackSettings = playback;
            }
        }

        // 
        // Enable Frameskip combobox value change
        // 
        private void cb_frameskip_CheckedChanged(object sender, EventArgs e)
        {
            if (_currentPreviewAnimation != null)
            {
                var playback = _currentPreviewAnimation.PlaybackSettings;

                playback.FrameSkip = cb_frameskip.Checked;

                _currentPreviewAnimation.PlaybackSettings = playback;
            }
        }

        // 
        // Fit Width Left button click
        // 
        private void btn_fitWidthLeft_Click(object sender, EventArgs e)
        {
            float cw = (float)_spriteSheet.Width / _sheetSettings.FrameWidth;

            if (Math.Abs((int)cw - cw) > float.Epsilon)
            {
                cw = (float)Math.Floor(cw);
            }

            while (cw < _spriteSheet.Width)
            {
                cw++;

                if (Math.Abs(_spriteSheet.Width / cw - (int)(_spriteSheet.Width / cw)) < float.Epsilon)
                {
                    break;
                }
            }

            // ReSharper disable once PossibleLossOfFraction
            nud_width.Value = _spriteSheet.Width / (int)cw;

            RefreshSheetSettings();
        }
        // 
        // Fit Width Right button click
        // 
        private void btn_fitWidthRight_Click(object sender, EventArgs e)
        {
            float cw = (float)_spriteSheet.Width / _sheetSettings.FrameWidth;

            if (Math.Abs((int)cw - cw) > float.Epsilon)
            {
                cw = (float)Math.Ceiling(cw);
            }

            while (cw > 1)
            {
                cw--;

                if (Math.Abs(_spriteSheet.Width / cw - (int)(_spriteSheet.Width / cw)) < float.Epsilon)
                {
                    break;
                }
            }

            // ReSharper disable once PossibleLossOfFraction
            nud_width.Value = _spriteSheet.Width / (int)cw;

            RefreshSheetSettings();   
        }

        // 
        // Fit Height Left button click
        // 
        private void btn_fitHeightLeft_Click(object sender, EventArgs e)
        {
            float ch = (float)_spriteSheet.Height / _sheetSettings.FrameHeight;

            if (Math.Abs((int)ch - ch) > float.Epsilon)
            {
                ch = (float)Math.Floor(ch);
            }

            while (ch < _spriteSheet.Height)
            {
                ch++;

                if (Math.Abs(_spriteSheet.Height / ch - (int)(_spriteSheet.Height / ch)) < float.Epsilon)
                {
                    break;
                }
            }

            // ReSharper disable once PossibleLossOfFraction
            nud_height.Value = _spriteSheet.Height / (int)ch;

            RefreshSheetSettings();
        }
        // 
        // Fit Height Right button click
        // 
        private void btn_fitHeightRight_Click(object sender, EventArgs e)
        {
            float ch = (float)_spriteSheet.Height / _sheetSettings.FrameHeight;

            if (Math.Abs((int)ch - ch) > float.Epsilon)
            {
                ch = (float)Math.Ceiling(ch);
            }

            while (ch > 1)
            {
                ch--;

                if (Math.Abs(_spriteSheet.Height / ch - (int)(_spriteSheet.Height / ch)) < float.Epsilon)
                {
                    break;
                }
            }

            // ReSharper disable once PossibleLossOfFraction
            nud_height.Value = _spriteSheet.Height / (int)ch;

            RefreshSheetSettings();
        }

        /// <summary>
        /// Preview animation used to display preview of animation imports on the form's animation view
        /// </summary>
        private class PreviewAnimation : FrameSequencer, IAnimation
        {
            public string Name => "Preview";
            
            public AnimationPlaybackSettings PlaybackSettings { get; set; }

            public AnimationExportSettings ExportSettings => new AnimationExportSettings();
            
            public PreviewAnimation([NotNull] Bitmap sourceBitmap, [NotNull] Rectangle[] rectangles) : base(sourceBitmap, rectangles)
            {

            }

            public IFrame GetFrameAtIndex(int i)
            {
                throw new NotImplementedException();
            }
        }
    }
}
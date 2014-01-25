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
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Controllers;
using Pixelaria.Data;
using Pixelaria.Importers;
using Pixelaria.Views.Controls;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used as interface for new animation creation
    /// </summary>
    public partial class ImportAnimationView : Form
    {
        /// <summary>
        /// The main controller owning this form
        /// </summary>
        Controller controller;

        /// <summary>
        /// The loaded image sheet
        /// </summary>
        Image spriteSheet;

        /// <summary>
        /// The current SheetSettings being edited on the form
        /// </summary>
        SheetSettings sheetSettings;

        /// <summary>
        /// The current preview animation
        /// </summary>
        Animation currentPreviewAnimation;

        /// <summary>
        /// Optional AnimationSheet that will own the newly created animation
        /// </summary>
        AnimationSheet parentSheet;

        /// <summary>
        /// Intiializes a new instance of the ImportAnimationView class
        /// </summary>
        /// <param name="controller">The controller owning this form</param>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created animation</param>
        public ImportAnimationView(Controller controller, AnimationSheet parentSheet = null)
        {
            this.controller = controller;
            this.parentSheet = parentSheet;

            InitializeComponent();
            cpb_sheetPreview.HookToForm(this);

            this.cpb_sheetPreview.Importer = controller.DefaultImporter;

            spriteSheet = null;

            sheetSettings = new SheetSettings(false, 32, 32);

            ValidateFields();
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        private void ValidateFields()
        {
            bool valid = true;
            bool alert = false;
            string validation;

            if (spriteSheet == null)
            {
                lbl_alertLabel.Text = "Start by loading an image clicking the 'Browse...' button";

                pnl_alertPanel.Visible = true;

                btn_ok.Enabled = false;

                return;
            }

            // Animation name
            validation = controller.AnimationValidator.ValidateAnimationName(txt_animationName.Text);
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
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "All Images (*.png, *.jpg, *jpeg, *.bmp, *.gif, *.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff|Png Images (*.png)|*.png|Bitmap Images (*.bmp)|*.bmp|Jpeg Images (*.jpg, *.jpeg)|*.jpg;*.jpeg|Gif Images (*.gif)|*.giff|Tiff Images (*.tiff)|*.tiff";

            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    // Dispose of the current sprite sheet if it's loaded
                    if (spriteSheet != null)
                    {
                        spriteSheet.Dispose();
                    }

                    txt_fileName.Text = ofd.FileName;

                    spriteSheet = Image.FromFile(ofd.FileName);

                    cpb_sheetPreview.LoadPreview(spriteSheet, sheetSettings);

                    txt_animationName.Text = Path.GetFileNameWithoutExtension(ofd.FileName);

                    // Setup the boundaries for the numeric up downs
                    nud_width.Maximum = spriteSheet.Width;
                    nud_height.Maximum = spriteSheet.Height;

                    nud_startX.Maximum = spriteSheet.Width - 1;
                    nud_startY.Maximum = spriteSheet.Height - 1;

                    tssl_dimensions.Text = spriteSheet.Width + " x " + spriteSheet.Height;

                    // Enable the width/height fitting buttons
                    btn_fitWidthLeft.Enabled = btn_fitWidthRight.Enabled = btn_fitHeightLeft.Enabled = btn_fitHeightRight.Enabled = true;

                    RefreshSheetSettings();

                    GenerateAnimationPreview();

                    ValidateFields();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error loading image: " + e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Generates an animation preview based on the current settings
        /// </summary>
        private void GenerateAnimationPreview()
        {
            if (spriteSheet != null)
            {
                if (currentPreviewAnimation != null)
                {
                    currentPreviewAnimation.Dispose();
                    currentPreviewAnimation = null;
                }

                if (nud_frameCount.Value != 0)
                {
                    currentPreviewAnimation = controller.DefaultImporter.ImportAnimationFromImage(txt_animationName.Text, spriteSheet, sheetSettings);
                    currentPreviewAnimation.PlaybackSettings.FPS = (int)nud_fps.Value;
                    currentPreviewAnimation.PlaybackSettings.FrameSkip = cb_frameskip.Checked;
                }

                ap_animationPreview.LoadAnimation(currentPreviewAnimation);
            }
        }

        /// <summary>
        /// Refreshes the sheet settings object
        /// </summary>
        private void RefreshSheetSettings()
        {
            if (spriteSheet == null)
                return;

            sheetSettings.FrameWidth = (int)nud_width.Value;
            sheetSettings.FrameHeight = (int)nud_height.Value;
            sheetSettings.FrameCount = (int)nud_frameCount.Value;
            sheetSettings.FirstFrame = (int)nud_skipCount.Value;
            sheetSettings.FlipFrames = cb_reverseFrameOrder.Checked;
            sheetSettings.OffsetX = (int)nud_startX.Value;
            sheetSettings.OffsetY = (int)nud_startY.Value;

            cpb_sheetPreview.SheetSettings = sheetSettings;

            GenerateAnimationPreview();
        }

        /// <summary>
        /// Imports the animation to the current bundle
        /// </summary>
        private void ImportAnimation()
        {
            if (currentPreviewAnimation != null && currentPreviewAnimation.FrameCount > 0)
            {
                currentPreviewAnimation.Name = txt_animationName.Text;

                controller.AddAnimation(currentPreviewAnimation, true, parentSheet);

                currentPreviewAnimation = null;
                ap_animationPreview.LoadAnimation(null);
            }
        }

        // 
        // Form Closing event handler
        // 
        private void ImportAnimationView_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (spriteSheet != null)
            {
                spriteSheet.Dispose();
            }
            currentPreviewAnimation = null;
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
            if (currentPreviewAnimation != null)
            {
                currentPreviewAnimation.PlaybackSettings.FPS = (int)nud_fps.Value;
            }
        }

        // 
        // Enable Frameskip combobox value change
        // 
        private void cb_frameskip_CheckedChanged(object sender, EventArgs e)
        {
            if (currentPreviewAnimation != null)
            {
                currentPreviewAnimation.PlaybackSettings.FrameSkip = cb_frameskip.Checked;
            }
        }


        // 
        // Fit Width Left button click
        // 
        private void btn_fitWidthLeft_Click(object sender, EventArgs e)
        {
            float cw = (float)spriteSheet.Width / sheetSettings.FrameWidth;

            if ((int)cw != cw)
            {
                cw = (float)Math.Floor(cw);
            }

            while (cw < spriteSheet.Width)
            {
                cw++;

                if (spriteSheet.Width / cw == (int)(spriteSheet.Width / cw))
                {
                    break;
                }
            }

            nud_width.Value = spriteSheet.Width / (int)cw;

            RefreshSheetSettings();
        }
        // 
        // Fit Width Right button click
        // 
        private void btn_fitWidthRight_Click(object sender, EventArgs e)
        {
            float cw = (float)spriteSheet.Width / sheetSettings.FrameWidth;

            if ((int)cw != cw)
            {
                cw = (float)Math.Ceiling(cw);
            }

            while (cw > 1)
            {
                cw--;

                if (spriteSheet.Width / cw == (int)(spriteSheet.Width / cw))
                {
                    break;
                }
            }

            nud_width.Value = spriteSheet.Width / (int)cw;

            RefreshSheetSettings();   
        }

        // 
        // Fit Height Left button click
        // 
        private void btn_fitHeightLeft_Click(object sender, EventArgs e)
        {
            float ch = (float)spriteSheet.Height / sheetSettings.FrameHeight;

            if ((int)ch != ch)
            {
                ch = (float)Math.Floor(ch);
            }

            while (ch < spriteSheet.Height)
            {
                ch++;

                if (spriteSheet.Height / ch == (int)(spriteSheet.Height / ch))
                {
                    break;
                }
            }

            nud_height.Value = spriteSheet.Height / (int)ch;

            RefreshSheetSettings();
        }
        // 
        // Fit Height Right button click
        // 
        private void btn_fitHeightRight_Click(object sender, EventArgs e)
        {
            float ch = (float)spriteSheet.Height / sheetSettings.FrameHeight;

            if ((int)ch != ch)
            {
                ch = (float)Math.Ceiling(ch);
            }

            while (ch > 1)
            {
                ch--;

                if (spriteSheet.Height / ch == (int)(spriteSheet.Height / ch))
                {
                    break;
                }
            }

            nud_height.Value = spriteSheet.Height / (int)ch;

            RefreshSheetSettings();
        }
    }
}
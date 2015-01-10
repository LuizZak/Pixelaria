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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Pixelaria.Data;
using Pixelaria.Data.Exports;

using Pixelaria.Controllers;
using Pixelaria.Localization;
using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used as interface for creating a new Animation Sheet
    /// </summary>
    public partial class AnimationSheetView : ModifiableContentView
    {
        /// <summary>
        /// The current AnimationSheet being edited
        /// </summary>
        readonly AnimationSheet _sheetToEdit;

        /// <summary>
        /// The controller that owns this form
        /// </summary>
        readonly Controller _controller;
        
        /// <summary>
        /// The current export settings
        /// </summary>
        AnimationExportSettings _exportSettings;

        /// <summary>
        /// The current bundle sheet export
        /// </summary>
        BundleSheetExport _bundleSheetExport;

        /// <summary>
        /// Gets the current AnimationSheet being edited
        /// </summary>
        public AnimationSheet CurrentSheet { get { return _sheetToEdit; } }

        /// <summary>
        /// Initializes a new instance of the AnimationSheetEditor class
        /// </summary>
        /// <param name="controller">The controller that owns this form</param>
        /// <param name="sheetToEdit">The sheet to edit on this form. Leave null to show an interface to create a new sheet</param>
        public AnimationSheetView(Controller controller, AnimationSheet sheetToEdit = null)
        {
            InitializeComponent();

            zpb_sheetPreview.HookToControl(this);

            _controller = controller;
            _sheetToEdit = sheetToEdit;

            if (sheetToEdit != null)
                _exportSettings = sheetToEdit.ExportSettings;

            InitializeFiends();
            ValidateFields();
        }

        /// <summary>
        /// Initializes the fields of this form
        /// </summary>
        public void InitializeFiends()
        {
            // If no sheet is present, disable sheet preview
            if (_sheetToEdit == null)
            {
                btn_generatePreview.Visible = false;
                lbl_sheetPreview.Visible = false;
                zpb_sheetPreview.Visible = false;

                gb_sheetInfo.Visible = false;
                gb_exportSummary.Visible = false;

                lbl_zoomLevel.Visible = false;
                cb_showFrameBounds.Visible = false;
                cb_showReuseCount.Visible = false;
                btn_apply.Enabled = false;

                Text = AnimationMessages.TextNewAnimationSheet;

                return;
            }

            txt_sheetName.Text = _sheetToEdit.Name;

            cb_favorRatioOverArea.Checked = _sheetToEdit.ExportSettings.FavorRatioOverArea;
            cb_forcePowerOfTwoDimensions.Checked = _sheetToEdit.ExportSettings.ForcePowerOfTwoDimensions;
            cb_forceMinimumDimensions.Checked = _sheetToEdit.ExportSettings.ForceMinimumDimensions;
            cb_reuseIdenticalFrames.Checked = _sheetToEdit.ExportSettings.ReuseIdenticalFramesArea;
            cb_highPrecision.Checked = _sheetToEdit.ExportSettings.HighPrecisionAreaMatching;
            cb_allowUordering.Checked = _sheetToEdit.ExportSettings.AllowUnorderedFrames;
            cb_useUniformGrid.Checked = _sheetToEdit.ExportSettings.UseUniformGrid;
            cb_padFramesOnXml.Checked = _sheetToEdit.ExportSettings.UsePaddingOnXml;
            cb_exportXml.Checked = _sheetToEdit.ExportSettings.ExportXml;
            nud_xPadding.Value = _sheetToEdit.ExportSettings.XPadding;
            nud_yPadding.Value = _sheetToEdit.ExportSettings.YPadding;

            modified = false;

            Text = AnimationMessages.TextAnimationSheet + @" [" + _sheetToEdit.Name + @"]";

            UpdateCountLabels();
        }

        /// <summary>
        /// Updates the animation and frame count labels 
        /// </summary>
        public void UpdateCountLabels()
        {
            lbl_animCount.Text = _sheetToEdit.AnimationCount + "";
            lbl_frameCount.Text = _sheetToEdit.GetFrameCount() + "";
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        /// <returns>Whether the validation was successful</returns>
        public bool ValidateFields()
        {
            bool valid = true;
            const bool alert = false;

            // Animation name
            var validation = _controller.AnimationSheetValidator.ValidateAnimationSheetName(txt_sheetName.Text, _sheetToEdit);
            if (validation != "")
            {
                txt_sheetName.BackColor = Color.LightPink;
                lbl_error.Text = validation;
                valid = false;
            }
            else
            {
                txt_sheetName.BackColor = Color.White;
            }

            pnl_errorPanel.Visible = !valid;
            pnl_alertPanel.Visible = alert;

            btn_ok.Enabled = valid;
            btn_apply.Enabled = (valid && modified && _sheetToEdit != null);

            return valid;
        }

        /// <summary>
        /// Applies the changes made by this form to the affected objects
        /// </summary>
        public override void ApplyChanges()
        {
            if(!ValidateFields())
                return;

            _sheetToEdit.Name = txt_sheetName.Text;
            _sheetToEdit.ExportSettings = RepopulateExportSettings();

            _controller.UpdatedAnimationSheet(_sheetToEdit);

            Text = AnimationMessages.TextAnimationSheet + @" [" + _sheetToEdit.Name + @"]";
            btn_apply.Enabled = false;

            base.ApplyChanges();
        }

        /// <summary>
        /// Displays a confirmation to the user when changes have been made to the animation sheet.
        /// If the view is opened as a creation view, no confirmation is displayed to the user in any way
        /// </summary>
        /// <returns>The DialogResult of the confirmation MessageBox displayed to the user</returns>
        public override DialogResult ConfirmChanges()
        {
            if (_sheetToEdit != null)
            {
                return base.ConfirmChanges();
            }

            return DialogResult.OK;
        }

        /// <summary>
        /// Marks the contents of this view as Modified
        /// </summary>
        public override void MarkModified()
        {
            if (_sheetToEdit != null)
            {
                Text = AnimationMessages.TextAnimationSheet + @" [" + _sheetToEdit.Name + @"]*";
                btn_apply.Enabled = true;
            }

            base.MarkModified();
        }

        /// <summary>
        /// Repopulates the AnimationExportSettings field of this form with the form's fields and returns it
        /// </summary>
        /// <returns>The newly repopulated AnimationExportSettings</returns>
        public AnimationExportSettings RepopulateExportSettings()
        {
            _exportSettings.FavorRatioOverArea = cb_favorRatioOverArea.Checked;
            _exportSettings.ForcePowerOfTwoDimensions = cb_forcePowerOfTwoDimensions.Checked;
            _exportSettings.ForceMinimumDimensions = cb_forceMinimumDimensions.Checked;
            _exportSettings.ReuseIdenticalFramesArea = cb_reuseIdenticalFrames.Checked;
            _exportSettings.HighPrecisionAreaMatching = cb_highPrecision.Checked;
            _exportSettings.AllowUnorderedFrames = cb_allowUordering.Checked;
            _exportSettings.UseUniformGrid = cb_useUniformGrid.Checked;
            _exportSettings.UsePaddingOnXml = cb_padFramesOnXml.Checked;
            _exportSettings.ExportXml = cb_exportXml.Checked;
            _exportSettings.XPadding = (int)nud_xPadding.Value;
            _exportSettings.YPadding = (int)nud_yPadding.Value;

            return _exportSettings;
        }

        /// <summary>
        /// Generates a preview for the AnimationSheet currently loaded into this form
        /// </summary>
        public void GeneratePreview()
        {
            RepopulateExportSettings();
            UpdateCountLabels();

            // Dispose of current preview
            RemovePreview();

            if (_sheetToEdit.Animations.Length > 0)
            {
                // Time the bundle export
                pb_exportProgress.Visible = true;

                BundleExportProgressEventHandler handler = args =>
                {
                    pb_exportProgress.Value = args.StageProgress;
                    pb_exportProgress.Refresh();
                };

                var form = FindForm();
                if (form != null)
                    form.Cursor = Cursors.WaitCursor;

                Stopwatch sw = Stopwatch.StartNew();

                // Export the bundle
                _bundleSheetExport = _controller.GenerateBundleSheet(_exportSettings, handler, _sheetToEdit.Animations);

                Image img = _bundleSheetExport.Sheet;

                sw.Stop();

                if (form != null)
                    form.Cursor = Cursors.Default;

                zpb_sheetPreview.SetImage(img);

                pb_exportProgress.Visible = false;

                // Update labels
                lbl_sheetPreview.Text = AnimationMessages.TextSheetPreviewGenerated + sw.ElapsedMilliseconds + @"ms)";

                lbl_dimensions.Text = img.Width + @"x" + img.Height;
                lbl_pixelCount.Text = (img.Width * img.Height).ToString("N0");
                lbl_framesOnSheet.Text = (_bundleSheetExport.FrameCount - _bundleSheetExport.ReusedFrameCount) + "";
                lbl_reusedFrames.Text = (_bundleSheetExport.ReusedFrameCount) + "";
                lbl_memoryUsage.Text = Utilities.FormatByteSize(ImageUtilities.MemoryUsageOfImage(img));

                if (pnl_alertPanel.Visible && lbl_alertLabel.Text == AnimationMessages.TextNoAnimationInSheetToGeneratePreview)
                {
                    pnl_alertPanel.Visible = false;
                }

                if (cb_showFrameBounds.Checked)
                {
                    ShowFrameBounds();
                }
            }
            else
            {
                lbl_alertLabel.Text = AnimationMessages.TextNoAnimationInSheetToGeneratePreview;
                pnl_alertPanel.Visible = true;
            }
        }

        /// <summary>
        /// Shows the frame bounds for the exported image
        /// </summary>
        public void ShowFrameBounds()
        {
            if (_bundleSheetExport != null)
            {
                zpb_sheetPreview.LoadExportSheet(_bundleSheetExport);
            }

            cb_showReuseCount.Enabled = true;
        }

        /// <summary>
        /// Hides the frame bounds for the exported image
        /// </summary>
        public void HideFrameBounds()
        {
            zpb_sheetPreview.UnloadExportSheet();

            cb_showReuseCount.Enabled = false;
        }

        /// <summary>
        /// Shows the reuse count for the exported image
        /// </summary>
        public void ShowReuseCount()
        {
            zpb_sheetPreview.DisplayReusedCount = true;
        }

        /// <summary>
        /// Hides the reuse count for the exported image
        /// </summary>
        public void HideReuseCount()
        {
            if (_bundleSheetExport != null)
            {
                zpb_sheetPreview.DisplayReusedCount = false;
            }
        }

        /// <summary>
        /// Removes the currently displayed preview and disposes of the image
        /// </summary>
        public void RemovePreview()
        {
            if (zpb_sheetPreview.Image != null)
            {
                zpb_sheetPreview.Image.Dispose();
                zpb_sheetPreview.Image = null;
            }

            _bundleSheetExport = null;
        }

        /// <summary>
        /// Generates an AnimationSheet using the settings on this form's fields
        /// </summary>
        /// <returns>The new AnimationSheet object</returns>
        public AnimationSheet GenerateAnimationSheet()
        {
            AnimationSheet sheet = new AnimationSheet(txt_sheetName.Text) { ExportSettings = RepopulateExportSettings() };


            return sheet;
        }

        // 
        // Animation Sheet Name textfield change
        // 
        private void txt_sheetName_TextChanged(object sender, EventArgs e)
        {
            ValidateFields();

            MarkModified();
        }

        // 
        // Common event for all checkboxes on the form
        // 
        private void checkboxes_Change(object sender, EventArgs e)
        {
            MarkModified();
        }

        // 
        // Common event for all nuds on the form
        // 
        private void nuds_Common(object sender, EventArgs e)
        {
            MarkModified();
        }

        // 
        // Generate Preview button click
        // 
        private void btn_generatePreview_Click(object sender, EventArgs e)
        {
            GeneratePreview();
        }

        // 
        // Form Closing event handler
        // 
        private void AnimationSheetView_FormClosed(object sender, FormClosedEventArgs e)
        {
            RemovePreview();
        }

        // 
        // Show Frame Bounds checkbox check
        // 
        private void cb_showFrameBounds_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_showFrameBounds.Checked)
            {
                ShowFrameBounds();
            }
            else
            {
                HideFrameBounds();
            }
        }

        //
        // Show Reuse Count checkbox check
        //
        private void cb_showReuseCount_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_showReuseCount.Checked)
            {
                ShowReuseCount();
            }
            else
            {
                HideReuseCount();
            }
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            // Validate one more time before closing
            if (ValidateFields() == false)
            {
                return;
            }

            if (_sheetToEdit != null && modified)
            {
                ApplyChanges();
            }

            Close();
        }

        // 
        // Cancel button click
        // 
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DiscardChangesAndClose();
        }

        // 
        // Apply Changes button click
        // 
        private void btn_apply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        // 
        // Sheet Preview ZPB zoom changed
        // 
        private void zpb_sheetPreview_ZoomChanged(object sender, Controls.ZoomChangedEventArgs e)
        {
            lbl_zoomLevel.Text = @"Zoom: " + (Math.Ceiling(e.NewZoom * 100) / 100) + @"x";
        }
    }
}
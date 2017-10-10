﻿/*
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
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Data;
using Pixelaria.Data.Exports;

using Pixelaria.Controllers;
using Pixelaria.Localization;
using Pixelaria.Properties;
using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used as interface for creating a new Animation Sheet
    /// </summary>
    public partial class AnimationSheetView : ModifiableContentView
    {
        /// <summary>
        /// The controller that owns this form
        /// </summary>
        private readonly Controller _controller;
        
        /// <summary>
        /// The current export settings
        /// </summary>
        private AnimationExportSettings _exportSettings;

        /// <summary>
        /// The current bundle sheet export
        /// </summary>
        private BundleSheetExport _bundleSheetExport;

        /// <summary>
        /// Whether the current sheet export was generated with data from animations that where unsaved at the time
        /// </summary>
        private bool _generatedWhileUnsaved;

        /// <summary>
        /// Cancellation token for the sheet generation routine
        /// </summary>
        private CancellationTokenSource _sheetCancellation;

        /// <summary>
        /// Whether to automatically update the preview whenever changes to animations for a sprite sheet occur
        /// </summary>
        private bool _autoUpdatePreview = true;

        /// <summary>
        /// Gets the current AnimationSheet being edited
        /// </summary>
        public AnimationSheet CurrentSheet { get; }

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
            CurrentSheet = sheetToEdit;

            if (sheetToEdit != null)
                _exportSettings = sheetToEdit.ExportSettings;

            InitializeFiends();
            ValidateFields();
            
            if(sheetToEdit != null)
            {
                // TODO: Better abstract and decouple these references to AnimationView, which are, as of now, a hack.

                // Setup events
                controller.ViewModifiedChanged += OnControllerOnViewModifiedChanged;
                controller.ViewOpenedClosed += OnControllerOnViewOpenedClosed;
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (components != null)
                    components.Dispose();

                // ReSharper disable once UseNullPropagation
                if (_sheetCancellation != null)
                    _sheetCancellation.Dispose();

                _controller.ViewModifiedChanged -= OnControllerOnViewModifiedChanged;
                _controller.ViewOpenedClosed -= OnControllerOnViewOpenedClosed;
            }

            base.Dispose(disposing);
        }
        
        /// <summary>
        /// Initializes the fields of this form
        /// </summary>
        public void InitializeFiends()
        {
            // If no sheet is present, disable sheet preview
            if (CurrentSheet == null)
            {
                btn_generatePreview.Visible = false;
                lbl_sheetPreview.Visible = false;
                zpb_sheetPreview.Visible = false;

                gb_sheetInfo.Visible = false;
                gb_exportSummary.Visible = false;
                
                anud_zoom.Visible = false;
                pb_zoomIcon.Visible = false;

                cb_showFrameBounds.Visible = false;
                cb_showReuseCount.Visible = false;
                btn_apply.Enabled = false;

                Text = AnimationMessages.TextNewAnimationSheet;
                
                txt_sheetName.Text = _controller.GetUniqueUntitledAnimationSheetName();

                return;
            }

            txt_sheetName.Text = CurrentSheet.Name;

            cb_favorRatioOverArea.Checked = CurrentSheet.ExportSettings.FavorRatioOverArea;
            cb_forcePowerOfTwoDimensions.Checked = CurrentSheet.ExportSettings.ForcePowerOfTwoDimensions;
            cb_forceMinimumDimensions.Checked = CurrentSheet.ExportSettings.ForceMinimumDimensions;
            cb_reuseIdenticalFrames.Checked = CurrentSheet.ExportSettings.ReuseIdenticalFramesArea;
            cb_highPrecision.Checked = CurrentSheet.ExportSettings.HighPrecisionAreaMatching;
            cb_allowUordering.Checked = CurrentSheet.ExportSettings.AllowUnorderedFrames;
            cb_useUniformGrid.Checked = CurrentSheet.ExportSettings.UseUniformGrid;
            cb_padFramesOnJson.Checked = CurrentSheet.ExportSettings.UsePaddingOnJson;
            cb_exportJson.Checked = CurrentSheet.ExportSettings.ExportJson;
            nud_xPadding.Value = CurrentSheet.ExportSettings.XPadding;
            nud_yPadding.Value = CurrentSheet.ExportSettings.YPadding;

            zpb_sheetPreview.MaximumZoom = new PointF(100, 100);

            modified = false;

            Text = AnimationMessages.TextAnimationSheet + @" [" + CurrentSheet.Name + @"]";

            UpdateCountLabels();
        }

        /// <summary>
        /// Updates the animation and frame count labels 
        /// </summary>
        public void UpdateCountLabels()
        {
            lbl_animCount.Text = CurrentSheet.AnimationCount + "";
            lbl_frameCount.Text = CurrentSheet.GetFrameCount() + "";
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
            var validation = _controller.AnimationSheetValidator.ValidateAnimationSheetName(txt_sheetName.Text, CurrentSheet);
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
            btn_apply.Enabled = (valid && modified && CurrentSheet != null);

            return valid;
        }

        /// <summary>
        /// Applies the changes made by this form to the affected objects
        /// </summary>
        public override void ApplyChanges()
        {
            if(!ValidateFields())
                return;

            CurrentSheet.Name = txt_sheetName.Text;
            CurrentSheet.ExportSettings = RepopulateExportSettings();

            _controller.UpdatedAnimationSheet(CurrentSheet);

            Text = AnimationMessages.TextAnimationSheet + @" [" + CurrentSheet.Name + @"]";
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
            if (CurrentSheet != null)
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
            if (CurrentSheet != null)
            {
                Text = AnimationMessages.TextAnimationSheet + @" [" + CurrentSheet.Name + @"]*";
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
            _exportSettings.UsePaddingOnJson = cb_padFramesOnJson.Checked;
            _exportSettings.ExportJson = cb_exportJson.Checked;
            _exportSettings.XPadding = (int)nud_xPadding.Value;
            _exportSettings.YPadding = (int)nud_yPadding.Value;

            return _exportSettings;
        }
        
        private void OnControllerOnViewModifiedChanged(object sender, EventArgs args)
        {
            UpdateUnsavedAnimationsIconState();

            if (_autoUpdatePreview)
                GeneratePreview();
        }

        private void OnControllerOnViewOpenedClosed(object sender, ViewOpenCloseEventArgs args)
        {
            // Erase current bundle sheet export, in case the animation closed was previously from this view
            // This is a work-around 
            var animView = sender as AnimationView;
            if (animView != null && _controller.GetOwningAnimationSheet(animView.CurrentAnimation)?.ID == CurrentSheet.ID && _generatedWhileUnsaved)
            {
                zpb_sheetPreview.SheetExport = null;
                _bundleSheetExport = null;
            }

            UpdateUnsavedAnimationsIconState();
        }

        /// <summary>
        /// Updates state of unsaved animations icon
        /// </summary>
        private void UpdateUnsavedAnimationsIconState()
        {
            // Turn warn icon on if any animation from this sheet has unsaved changes
            pb_unsavedAnimWarning.Visible = HasUnsavedAnimations();
        }

        /// <summary>
        /// Returns whether any of the animations on this sheet is currently opened, with unsaved changes associated with them
        /// </summary>
        private bool HasUnsavedAnimations()
        {
            return CurrentSheet.Animations.Any(_controller.InterfaceStateProvider.HasUnsavedChangesForAnimation);
        }

        /// <summary>
        /// Generates a preview for the AnimationSheet currently loaded into this form
        /// </summary>
        public void GeneratePreview()
        {
            _generatedWhileUnsaved = HasUnsavedAnimations();

            RepopulateExportSettings();
            UpdateCountLabels();

            if (CurrentSheet.Animations.Length <= 0)
            {
                lbl_alertLabel.Text = AnimationMessages.TextNoAnimationInSheetToGeneratePreview;
                pnl_alertPanel.Visible = true;
                return;
            }

            // Time the bundle export
            pb_exportProgress.Visible = true;

            void Handler(BundleExportProgressEventArgs args)
            {
                Invoke(new Action(() => { pb_exportProgress.Value = args.StageProgress; }));
            }

            var form = FindForm();
            if (form != null)
                form.Cursor = Cursors.WaitCursor;

            btn_generatePreview.Enabled = false;

            var sw = Stopwatch.StartNew();

            _sheetCancellation?.Cancel();
            _sheetCancellation = new CancellationTokenSource();

            // Get a dynamic provider for better accuracy of animations to export
            var provider = _controller.GetDynamicProviderForSheet(CurrentSheet, _exportSettings);

            // Export the bundle
            var t = _controller.GenerateBundleSheet(provider, _sheetCancellation.Token, Handler);

            t.ContinueWith(task =>
            {
                Invoke(new Action(() =>
                {
                    btn_generatePreview.Enabled = true;

                    // Dispose of current preview
                    RemovePreview();

                    if (_sheetCancellation.IsCancellationRequested)
                    {
                        _sheetCancellation = null;
                        Close();
                        return;
                    }

                    _sheetCancellation = null;

                    _bundleSheetExport = task.Result;

                    var img = _bundleSheetExport.Sheet;

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

                    if (pnl_alertPanel.Visible &&
                        lbl_alertLabel.Text == AnimationMessages.TextNoAnimationInSheetToGeneratePreview)
                    {
                        pnl_alertPanel.Visible = false;
                    }

                    if (cb_showFrameBounds.Checked)
                    {
                        ShowFrameBounds();
                    }
                }));
            });
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
            return new AnimationSheet(txt_sheetName.Text) { ExportSettings = RepopulateExportSettings() };
        }
        
        /// <summary>
        /// Displays frame context menu for the given sheet preview click action
        /// </summary>
        public void DisplayFrameContextMenu([NotNull] Controls.SheetPreviewFrameBoundsClickEventArgs e)
        {
            var index = e.SheetBoundsIndex;
            var frameBoundsMap = _bundleSheetExport.Atlas.GetFrameBoundsMap();

            // Pull all frames found
            var frameIds = frameBoundsMap.FrameIdsAtSheetIndex(index);

            // This guy maps a list of tuples containing animations and their respective frames which are contained in the frameIds array above
            var framesPerAnimation =
                (
                    from
                        animation in _bundleSheetExport.Animations
                    let list = animation.Frames.Where(frame => frameIds.Contains(frame.ID)).ToList()
                    where
                        list.Count > 0
                    select
                        (animation, list)
                ).ToList();

            // TODO: This is a work-around for removing frames from an animation without updating the preview, leading to a
            // dangling frame that is still 'clickeable' on the sheet preview.
            if (framesPerAnimation.Count == 0)
                return;

            var menu = new ContextMenuStrip();
            
            // Summary
            var title = new ToolStripMenuItem($"Shared between {frameBoundsMap.CountOfFramesAtSheetBoundsIndex(index)} frame(s):")
            {
                Enabled = false
            };

            menu.Items.Add(title);
            menu.Items.Add(new ToolStripSeparator());

            foreach (var tuple in framesPerAnimation)
            {
                // Animation
                var animItem = new ToolStripMenuItem(tuple.Item1.Name)
                {
                    Image = Resources.anim_icon
                };
                
                foreach (var frame in tuple.Item2)
                {
                    var frameItem = new ToolStripMenuItem($"Frame {frame.Index + 1}")
                    {
                        Image = Resources.frame_icon
                    };

                    frameItem.Click += (sender, args) =>
                    {
                        // Open frame in controller
                        var view = _controller.OpenAnimationView(tuple.Item1, frame.Index);

                        view.SetAnimationControlPlayback(false);
                        view.SetAnimationControlFrameIndex(frame.Index);
                    };

                    // Add frame labels
                    animItem.DropDownItems.Add(frameItem);
                }
                
                menu.Items.Add(animItem);
            }

            // Used to fix drawing of selected frame
            menu.Closed += (sender, args) =>
            {
                zpb_sheetPreview.AllowMouseHover = false;
                zpb_sheetPreview.AllowMouseHover = true;
            };

            menu.Show(MousePosition);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_sheetCancellation != null)
            {
                _sheetCancellation.Cancel();
                e.Cancel = true;
                return;
            }

            base.OnFormClosing(e);
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
        // Zoom anud value changed
        // 
        private void anud_zoom_ValueChanged(object sender, EventArgs e)
        {
            zpb_sheetPreview.Zoom = new PointF((float)anud_zoom.Value, (float)anud_zoom.Value);
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

            if (CurrentSheet != null && modified)
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
        private void zpb_sheetPreview_ZoomChanged(object sender, [NotNull] Controls.ZoomChangedEventArgs e)
        {
            anud_zoom.Value = (decimal)e.NewZoom;
        }

        // 
        // Sheet Preview right clicked frame rectangle event
        // 
        private void sppb_clickedFrameRect(object sender, [NotNull] Controls.SheetPreviewFrameBoundsClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DisplayFrameContextMenu(e);
            }
        }

        //
        // Auto Update Preview checkbox check
        //
        private void cb_autoUpdatePreview_CheckedChanged(object sender, EventArgs e)
        {
            _autoUpdatePreview = cb_autoUpdatePreview.Checked;
        }
    }
}
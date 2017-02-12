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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;
using Pixelaria.Data.Clipboard;
using Pixelaria.Data.Undo;

using Pixelaria.Filters;

using Pixelaria.Utils;

using Pixelaria.Views.Controls;
using Pixelaria.Views.MiscViews;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used to edit and view animations
    /// </summary>
    public partial class AnimationView : ModifiableContentView
    {
        /// <summary>
        /// The common clipboard for all the AnimationViews
        /// </summary>
        public static DataClipboard Clipboard { get; }

        /// <summary>
        /// The Animation object that is currently used
        /// </summary>
        private readonly Animation _viewAnimation;

        /// <summary>
        /// The Controller object that owns this AnimationView
        /// </summary>
        private readonly Controller _controller;

        /// <summary>
        /// The undo system for this AnimationView
        /// </summary>
        private readonly UndoSystem _undoSystem;

        /// <summary>
        /// Event handler for the ClipboardChanged event
        /// </summary>
        private readonly DataClipboard.ClipboardEventHandler _clipboardHandler;

        /// <summary>
        /// Event handler for a filter item click
        /// </summary>
        private readonly EventHandler _filterClickEventHandler;

        /// <summary>
        /// Event handler for a filter preset item click
        /// </summary>
        private readonly EventHandler _presetClickEventHandler;

        /// <summary>
        /// Whether the animation name provided in the animation's name text box is valid
        /// </summary>
        private bool _nameValid;

        /// <summary>
        /// Gets the current animation being displayed on this AnimationView
        /// </summary>
        public Animation CurrentAnimation { get; }

        /// <summary>
        /// Static constructor for the AnimationView class
        /// </summary>
        static AnimationView()
        {
            Clipboard = new DataClipboard();
        }

        /// <summary>
        /// Creates a new AnimationView using an Animation to show
        /// </summary>
        /// <param name="controller">The controller that owns this AnimationView</param>
        /// <param name="animation">The animation to show on this AnimationView</param>
        public AnimationView(Controller controller, Animation animation)
        {
            InitializeComponent();

            _filterClickEventHandler = tsm_filterItem_Click;
            _presetClickEventHandler = tsm_presetItem_Click;

            UpdateFilterList();
            UpdateFilterPresetList();

            _controller = controller;
            _undoSystem = new UndoSystem();
            CurrentAnimation = animation;
            _viewAnimation = CurrentAnimation.Clone();

            for (int i = 0; i < _viewAnimation.FrameCount; i++)
            {
                _viewAnimation[i].ID = animation[i].ID;
            }

            _clipboardHandler = clipboard_ClipboardChanged;
            Clipboard.ClipboardChanged += _clipboardHandler;

            _undoSystem.UndoRegistered += undoSystem_UndoRegistered;

            RefreshView();

            if(animation.PlaybackSettings.FPS != 0)
                animationPreviewPanel.SetPlayback(true);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();

            // Mark unmodified because some operations updating the values of the interface on the constructor may change the value of the modified field
            MarkUnmodified();
        }

        #region Public-accessible methods

        /// <summary>
        /// Sets the animation state of the animation playback panel on this animation view
        /// </summary>
        public void SetAnimationControlPlayback(bool playing)
        {
            animationPreviewPanel.SetPlayback(playing);
        }

        /// <summary>
        /// Sets the currently selected frame on the animation control
        /// </summary>
        public void SetAnimationControlFrameIndex(int frame)
        {
            animationPreviewPanel.ChangeFrame(frame);
        }

        #endregion

        #region Interface Related Methods

        /// <summary>
        /// Refreshes the fields and views of this AnimationView
        /// </summary>
        public void RefreshView()
        {
            RefreshTitle();
            RefreshFramesView();
            RefreshPlaybackInfo();
            RefreshAnimationInfo();
            RefreshClipboardControls();
            RefreshUndoControls();
            animationPreviewPanel.LoadAnimation(_viewAnimation, false);
        }

        /// <summary>
        /// Refreshs the title of this view
        /// </summary>
        private void RefreshTitle()
        {
            Text = @"Animation [" + CurrentAnimation.Name + @"]" + (modified ? "*" : "");
        }

        /// <summary>
        /// Refreshes the buttons of the form
        /// </summary>
        private void RefreshButtons()
        {
            tsb_applyChanges.Enabled = modified;

            // Refreshes the 'Reverse Frames' menu item
            if (lv_frames.SelectedIndices.Count == 2)
                cmb_reverseFrames.Text = tsm_reverseFrames.Text = @"Swap Selected Frames";
            else if(lv_frames.SelectedItems.Count > 0)
                cmb_reverseFrames.Text = tsm_reverseFrames.Text = @"Reverse Selected Frames";
            else
                cmb_reverseFrames.Text = tsm_reverseFrames.Text = @"Reverse All Frames";

            tsm_insertFrame.Enabled = tsb_insertFrame.Enabled = lv_frames.SelectedItems.Count != 0;
            cmb_reverseFrames.Enabled = tsm_reverseFrames.Enabled = _viewAnimation.FrameCount > 1;

            RefreshClipboardControls();
            RefreshUndoControls();
        }

        /// <summary>
        /// Refreshes the playback information portion of the view
        /// </summary>
        private void RefreshPlaybackInfo()
        {
            nud_fps.Value = _viewAnimation.PlaybackSettings.FPS;
            cb_frameskip.Checked = _viewAnimation.PlaybackSettings.FrameSkip;
        }

        /// <summary>
        /// Refreshes the animation information portion of the view
        /// </summary>
        private void RefreshAnimationInfo()
        {
            txt_animName.Text = _viewAnimation.Name;

            tssl_dimensions.Text = _viewAnimation.Width + @" x " + _viewAnimation.Height;
            tssl_frameCount.Text = _viewAnimation.FrameCount + "";

            tssl_memory.Text = string.Format("Current: {0} Composed: {1}",
                Utilities.FormatByteSize(_viewAnimation.CalculateMemoryUsageInBytes(false)),
                Utilities.FormatByteSize(_viewAnimation.CalculateMemoryUsageInBytes(true)));
        }

        /// <summary>
        /// Refresh the frames view using the frames of the current animation
        /// </summary>
        private void RefreshFramesView()
        {
            il_framesThumbs.Images.Clear();

            lv_frames.Items.Clear();

            // Re-create the frame thumbs
            float scaleX = 1, scaleY = 1;
            const int width = 256;
            const int height = 256;

            if (_viewAnimation.Width >= _viewAnimation.Height)
            {
                if (width < _viewAnimation.Width)
                {
                    scaleX = (float)width / _viewAnimation.Width;
                    scaleY = scaleX;
                }
            }
            else
            {
                if (height < _viewAnimation.Height)
                {
                    scaleY = (float)height / _viewAnimation.Height;
                    scaleX = scaleY;
                }
            }

            il_framesThumbs.ImageSize = new Size(Math.Min((int)(_viewAnimation.Width * scaleX), width), Math.Min((int)(_viewAnimation.Height * scaleY), height));
            for (int i = 0; i < _viewAnimation.FrameCount; i++)
            {
                IFrame frame = _viewAnimation.GetFrameAtIndex(i);

                il_framesThumbs.Images.Add(frame.GenerateThumbnail(frame.Width, frame.Height, false, false, Color.White));

                ListViewItem frameItem = new ListViewItem
                {
                    Text = @"Frame " + (i + 1),
                    ImageIndex = i,
                    Tag = frame
                };

                lv_frames.Items.Add(frameItem);
            }
        }

        /// <summary>
        /// Refreshes the clipboard controls
        /// </summary>
        private void RefreshClipboardControls()
        {
            if (lv_frames.SelectedItems.Count == 0)
            {
                tsm_copy.Enabled = tsb_copyFrames.Enabled = false;
                tsm_cut.Enabled = tsb_cutFrames.Enabled = false;
            }
            else
            {
                tsm_copy.Enabled = tsb_copyFrames.Enabled = true;
                tsm_cut.Enabled = tsb_cutFrames.Enabled = true;
            }

            cmb_pasteFrames.Enabled = tsm_paste.Enabled = tsb_pasteFrames.Enabled = (Clipboard.CurrentDataType == FrameListClipboardObject.DataType || Clipboard.CurrentDataType == ImageStreamClipboardObject.DataType);
        }

        /// <summary>
        /// Refreshes the undo/redo toolbar buttons/menu items
        /// </summary>
        private void RefreshUndoControls()
        {
            tsm_undo.Enabled = tsb_undo.Enabled = _undoSystem.CanUndo;
            tsm_redo.Enabled = tsb_redo.Enabled = _undoSystem.CanRedo;

            if (tsb_undo.Enabled)
            {
                tsm_undo.Text = tsb_undo.ToolTipText = @"Undo " + _undoSystem.NextUndo.GetDescription();

            }
            else
            {
                tsb_undo.ToolTipText = "";
                tsm_undo.Text = @"Undo";
            }

            if (tsb_redo.Enabled)
            {
                tsm_redo.Text = tsb_redo.ToolTipText = @"Redo " + _undoSystem.NextRedo.GetDescription();
            }
            else
            {
                tsb_redo.ToolTipText = "";
                tsm_redo.Text = @"Redo";
            }
        }

        /// <summary>
        /// Marks the contents of this view as modified by the user
        /// </summary>
        public override void MarkModified()
        {
            base.MarkModified();

            RefreshTitle();
            RefreshButtons();
        }

        /// <summary>
        /// Marks the contents of this view as unmodified by the user
        /// </summary>
        public override void MarkUnmodified()
        {
            base.MarkUnmodified();

            RefreshTitle();
            RefreshButtons();
        }

        /// <summary>
        /// Apply the changes made to the animation on this form
        /// </summary>
        public override void ApplyChanges()
        {
            if (!ValidateFields())
                return;

            if (modified)
            {
                _viewAnimation.Name = txt_animName.Text;

                // APPLY CHANGES
                CurrentAnimation.CopyFrom(_viewAnimation, false);

                for (int i = 0; i < CurrentAnimation.FrameCount; i++)
                {
                    CurrentAnimation[i].ID = _viewAnimation[i].ID;
                    // Update invalid (negative) frame IDs
                    if (CurrentAnimation[i].ID == -1)
                        CurrentAnimation[i].ID = CurrentAnimation.FrameIdGenerator.GetNextUniqueFrameId();
                }

                _controller.UpdatedAnimation(CurrentAnimation);
                _controller.MarkUnsavedChanges(true);
            }

            base.ApplyChanges();
        }

        /// <summary>
        /// Applies the current changes and closes the form
        /// </summary>
        public override void ApplyChangesAndClose()
        {
            // Validate once more before closing the form to make sure the changes are valid
            if (!ValidateFields())
                return;

            ActiveControl = null;

            ApplyChanges();

            Close();
        }

        /// <summary>
        /// Validates the fields of this AnimationView
        /// </summary>
        /// <returns>Whether the validation was successful</returns>
        private bool ValidateFields()
        {
            // Animation name
            var validation = _controller.AnimationValidator.ValidateAnimationName(txt_animName.Text, CurrentAnimation);
            if (validation != "")
            {
                txt_animName.BackColor = Color.LightPink;
                tsl_error.Text = validation;
                _nameValid = false;
            }
            else
            {
                txt_animName.BackColor = Color.White;
                _nameValid = true;
            }

            var valid = _nameValid;

            tsl_error.Visible = !valid;
            tsb_applyChangesAndClose.Enabled = valid;

            return valid;
        }

        /// <summary>
        /// Gets the ListViewItem object that is currently representing the given Frame object
        /// </summary>
        /// <param name="frame">The frame object to get the respective list view item representation</param>
        /// <returns>A ListViewItem object that is currently representing the given Frame object</returns>
        private ListViewItem GetListViewItemForFrame(IFrame frame)
        {
            return lv_frames.Items.Cast<ListViewItem>().FirstOrDefault(item => ReferenceEquals(item.Tag, frame));
        }

        /// <summary>
        /// Returns a list of all currently selected frames on this AnimationView
        /// </summary>
        /// <returns>The list of selected frames</returns>
        private List<IFrame> GetSelectedFrames()
        {
            return (from ListViewItem selected in lv_frames.SelectedItems select selected.Tag).OfType<IFrame>().ToList();
        }

        /// <summary>
        /// Undoes the last done task recorded by the form's undo system
        /// </summary>
        private void Undo()
        {
            if (_undoSystem.CanUndo)
            {
                _undoSystem.Undo();

                MarkModified();

                RefreshView();
            }
        }

        /// <summary>
        /// Redoes the last undone task recorded by the form's undo system
        /// </summary>
        private void Redo()
        {
            if (_undoSystem.CanRedo)
            {
                _undoSystem.Redo();

                MarkModified();

                RefreshView();
            }
        }

        /// <summary>
        /// Updates the list of available filters
        /// </summary>
        private void UpdateFilterList()
        {
            // Remove old filter items
            while (tsm_filters.DropDownItems.Count > 3)
            {
                tsm_filters.DropDownItems.RemoveAt(3);
            }

            // Fetch the list of filters
            string[] filterNames = FilterStore.Instance.FiltersList;
            Image[] iconList = FilterStore.Instance.FilterIconList;

            // Create and add all the new filter items
            for (int i = 0; i < iconList.Length; i++)
            {
                ToolStripMenuItem tsmFilterItem = new ToolStripMenuItem(filterNames[i], iconList[i])
                {
                    Tag = filterNames[i]
                };

                tsmFilterItem.Click += _filterClickEventHandler;

                tsm_filters.DropDownItems.Add(tsmFilterItem);
            }
        }

        /// <summary>
        /// Updates the list of available filter presets
        /// </summary>
        private void UpdateFilterPresetList()
        {
            // Remove old filter items
            tsm_filterPresets.DropDownItems.Clear();

            // Fetch the list of filters
            FilterPreset[] presets = FilterStore.Instance.FilterPrests;

            if (presets.Length == 0)
            {
                ToolStripMenuItem tsmEmptyItem = new ToolStripMenuItem("Empty") { Enabled = false };

                tsm_filterPresets.DropDownItems.Add(tsmEmptyItem);
            }

            // Create and add all the new filter items
            foreach (FilterPreset preset in presets)
            {
                ToolStripMenuItem tsmPresetItem = new ToolStripMenuItem(preset.Name, tsm_filterPresets.Image)
                {
                    Tag = preset.Name
                };

                tsmPresetItem.Click += _presetClickEventHandler;

                tsm_filterPresets.DropDownItems.Add(tsmPresetItem);
            }
        }

        /// <summary>
        /// Displays a BaseFilterView with the given FilterPreset loaded
        /// </summary>
        /// <param name="filterPreset">The filter preset to load on the BaseFilterView</param>
        private void DisplayFilterPreset(FilterPreset filterPreset)
        {
            AnimationFilterView afv = new AnimationFilterView(filterPreset, _viewAnimation);

            AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

            if (afv.ShowDialog(this) == DialogResult.OK && afv.ChangesDetected())
            {
                undoTask.RecordChanges();

                _undoSystem.RegisterUndo(undoTask);

                MarkModified();
                RefreshView();
            }
            else
            {
                undoTask.Clear();
            }
        }

        /// <summary>
        /// Shows the context menu for the currently selected frames
        /// </summary>
        private void ShowFramesContextMenu()
        {
            // Determine visibility of the items
            if (lv_frames.SelectedItems.Count == 0)
            {
                cms_animationRightClick.Show(MousePosition);

                return;
            }

            cmb_replaceFromImage.Enabled = lv_frames.SelectedItems.Count == 1;

            cms_frameRightClick.Show(MousePosition);
        }

        #endregion

        #region Animation Related Methods

        /// <summary>
        /// Displays an interface where the user can input settings for resizing the current animation
        /// </summary>
        public void ResizeAnimation()
        {
            AnimationResizeView arv = new AnimationResizeView(_viewAnimation, _viewAnimation.Width, _viewAnimation.Height);

            if (arv.ShowDialog(this) == DialogResult.OK)
            {
                AnimationResizeSettings settings = arv.GeneratedSettings;

                AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

                _viewAnimation.Resize(settings);

                MarkModified();
                RefreshView();

                undoTask.RecordChanges();
                _undoSystem.RegisterUndo(undoTask);
            }
        }

        /// <summary>
        /// Reverses the currently selected frames in place. If no frames are selected, the whole animation is reversed
        /// </summary>
        public void ReverseFrames()
        {
            // Reversing cannot happen when only one frame is selected
            if (lv_frames.SelectedIndices.Count == 1)
                return;

            ///// Get the frames to reverse
            List<int> frameIndicesToReverse = new List<int>();

            // If no frames are selected, reverse all the frames
            if (lv_frames.SelectedIndices.Count == 0)
            {
                for (int i = 0; i < lv_frames.Items.Count; i++)
                {
                    frameIndicesToReverse.Add(i);
                }
            }
            else
            {
                // Add all the index for the frames selected
                frameIndicesToReverse.AddRange(from ListViewItem item in lv_frames.SelectedItems select item.Index);
            }
            
            ///// Reverse the frames

            // Create an undo task
            AnimationModifyUndoTask amu = new AnimationModifyUndoTask(_viewAnimation);

            for (int i = 0; i < frameIndicesToReverse.Count / 2; i++)
            {
                _viewAnimation.SwapFrameIndices(frameIndicesToReverse[i], frameIndicesToReverse[frameIndicesToReverse.Count - i - 1]);
            }

            // Record the undo operation
            amu.RecordChanges();
            _undoSystem.RegisterUndo(amu);

            MarkModified();

            RefreshView();
        }

        /// <summary>
        /// Updates the animation's name based on the text provided in the animation name's textbox
        /// </summary>
        private void UpdateAnimationName()
        {
            MarkModified();
            ValidateFields();

            if (_nameValid)
            {
                _viewAnimation.Name = txt_animName.Text;
                RefreshAnimationInfo();
                RefreshTitle();
            }
        }

        #endregion

        #region Frame Related Methods

        /// <summary>
        /// De-selects all currently selected frames
        /// </summary>
        public void DeselectFrames()
        {
            lv_frames.SelectedIndices.Clear();
        }

        /// <summary>
        /// Selects a given frame index on this view
        /// </summary>
        public void SelectFrameIndex(int index)
        {
            lv_frames.SelectedIndices.Add(index);
        }

        /// <summary>
        /// Deletes the currently selected frames
        /// </summary>
        private void DeleteSelectedFrames()
        {
            if (lv_frames.SelectedItems.Count <= 0)
                return;

            var undoTask = new AnimationModifyUndoTask(_viewAnimation);

            // Delete selected frames
            foreach (var frame in GetSelectedFrames())
            {
                _viewAnimation.RemoveFrame(frame);
            }

            undoTask.RecordChanges();
            _undoSystem.RegisterUndo(undoTask);

            MarkModified();

            RefreshView();
        }

        /// <summary>
        /// Copies the selected frames to the clipboard
        /// </summary>
        private void CopySelectedFrames()
        {
            // Struct initialization
            var frameListClip = new FrameListClipboardObject();
            var selected = GetSelectedFrames();

            for (int i = 0; i < selected.Count; i++)
            {
                IFrame frame = selected[i];
                IFrame clonedFrame = frame.Clone();

                // Copy first frame into clipboard
                if (i == 0)
                {
                    // Copy the frame to the clipboard too
                    var stream = new MemoryStream();

                    using (var bitmap = clonedFrame.GetComposedBitmap())
                    {
                        bitmap.Save(stream, ImageFormat.Png);
                        stream.Position = 0;

                        System.Windows.Forms.Clipboard.SetImage(bitmap);
                        System.Windows.Forms.Clipboard.SetData("PNG", stream);
                    }
                }

                frameListClip.AddFrame(clonedFrame);
            }

            Clipboard.SetObject(frameListClip);
        }

        /// <summary>
        /// Cuts the selected frames to the clipboard
        /// </summary>
        private void CutSelectedFrames()
        {
            // Struct initialization
            var frameListClip = new FrameListClipboardObject();
            var selected = GetSelectedFrames();

            foreach (var frame in selected)
            {
                frameListClip.AddFrame(frame.Clone());
            }

            Clipboard.SetObject(frameListClip);

            // Remove the selected frames
            DeleteSelectedFrames();

            MarkModified();
        }

        /// <summary>
        /// Selects all the frames
        /// </summary>
        private void SelectAll()
        {
            foreach(ListViewItem item in lv_frames.Items)
            {
                item.Selected = true;
            }
        }

        /// <summary>
        /// Paste the frames currently on the clipboard
        /// </summary>
        private void PasteFrames()
        {
            // Frame pasting
            if (Clipboard.CurrentDataType == FrameListClipboardObject.DataType)
            {
                // Frame clipboard data fetching
                var frameListClip = (FrameListClipboardObject)Clipboard.GetObject();

                var sizeMatching = new FrameSizeMatchingSettings()
                {
                    AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.UseNewSize,
                    InterpolationMode = InterpolationMode.NearestNeighbor,
                    PerFrameScalingMethod = PerFrameScalingMethod.PlaceAtTopLeft
                };

                if (_viewAnimation.FrameCount > 0)
                {
                    // Check if there are any frames with different dimensions than the current animation
                    if (frameListClip.Frames.Any(frame => frame.Width != _viewAnimation.Width || frame.Height != _viewAnimation.Height))
                    {
                        FramesRescaleSettingsView sizeMatchingForm = new FramesRescaleSettingsView();

                        if (sizeMatchingForm.ShowDialog(this) == DialogResult.OK)
                        {
                            sizeMatching = sizeMatchingForm.GeneratedSettings;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                // Gets the index to position the frames
                int index = -1;

                for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
                {
                    index = Math.Max(index, lv_frames.SelectedIndices[i]);
                }

                if (index == -1)
                    index = lv_frames.Items.Count;

                var undoTask = new AnimationModifyUndoTask(_viewAnimation);

                // Maintain a copy of the list of added frames so the control can select them after
                var copiedFrames =
                    frameListClip.Frames.Select(frame => _controller.FrameFactory.CloneFrame(frame))
                        .Cast<IFrame>()
                        .ToList();

                _viewAnimation.AddFrames(copiedFrames, sizeMatching, index);

                undoTask.RecordChanges();
                _undoSystem.RegisterUndo(undoTask);

                MarkModified();
                RefreshView();

                // Select the newly added frames
                foreach (var frame in copiedFrames)
                {
                    GetListViewItemForFrame(frame).Selected = true;
                }
            }
            // Image pasting
            else if (Clipboard.CurrentDataType == ImageStreamClipboardObject.DataType)
            {
                // Recreate the frame from the image stream
                var imgStr = Clipboard.GetObject() as ImageStreamClipboardObject;

                if (imgStr == null)
                    return;

                var bitmap = Image.FromStream(imgStr.ImageStream) as Bitmap;

                if (bitmap == null)
                    return;

                var frame = _controller.FrameFactory.CreateFrame(bitmap.Width, bitmap.Height, null, false);

                frame.SetFrameBitmap(bitmap);

                var sizeMatching = new FrameSizeMatchingSettings();

                // Check if there are any frames with different dimensions than the current animation
                if (frame.Width != _viewAnimation.Width || frame.Height != _viewAnimation.Height)
                {
                    var sizeMatchingForm = new FramesRescaleSettingsView();

                    if (sizeMatchingForm.ShowDialog(this) == DialogResult.OK)
                    {
                        sizeMatching = sizeMatchingForm.GeneratedSettings;
                    }
                    else
                    {
                        return;
                    }
                }

                // Gets the index to position the frames
                int index = -1;

                var copiedFrames = new List<Frame> { frame };

                var undoTask = new AnimationModifyUndoTask(_viewAnimation);

                for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
                {
                    index = Math.Max(index, lv_frames.SelectedIndices[i]);
                }

                if (index == -1)
                    index = lv_frames.Items.Count;

                _viewAnimation.AddFrames(copiedFrames, sizeMatching, index);

                undoTask.RecordChanges();
                _undoSystem.RegisterUndo(undoTask);

                MarkModified();
                RefreshView();

                // Select the newly added frames
                foreach (var f in copiedFrames)
                {
                    GetListViewItemForFrame(f).Selected = true;
                }
            }
        }

        /// <summary>
        /// Inserts a new frame to the currently selected frame range
        /// </summary>
        private void InsertFrame()
        {
            // Gets the index to position the frames
            int index = lv_frames.Items.Count;

            for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
            {
                index = Math.Min(index, lv_frames.SelectedIndices[i]);
            }

            if (index == lv_frames.Items.Count)
            {
                index = -1;
            }

            var undoTask = new AnimationModifyUndoTask(_viewAnimation);

            _viewAnimation.CreateFrame(index);
            undoTask.RecordChanges();
            _undoSystem.RegisterUndo(undoTask);

            MarkModified();

            RefreshView();

            lv_frames.Items[(index == -1 ? lv_frames.Items.Count -1 : index)].Selected = true;
        }

        /// <summary>
        /// Adds a new frame at the end of the frame range
        /// </summary>
        private void AddNewFrame()
        {
            AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

            _viewAnimation.CreateFrame();
            undoTask.RecordChanges();
            _undoSystem.RegisterUndo(undoTask);

            MarkModified();

            RefreshView();

            lv_frames.Items[lv_frames.Items.Count - 1].Selected = true;
        }

        /// <summary>
        /// Opens a view for editing the currently selected frames
        /// </summary>
        private void EditFrame()
        {
            if (lv_frames.SelectedItems.Count == 0)
                return;

            AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

            // Temporarely disable the panel while showing the form so we don't waste CPU
            animationPreviewPanel.Disable();

            // Get the currently selected frame
            Frame frame = lv_frames.SelectedItems[0].Tag as Frame;

            FrameView frameView = new FrameView(_controller, frame);
            frameView.ShowDialog(this);

            // Mark animation as modified if the frame view has modified any frames
            if (frameView.ModifiedFrames)
            {
                undoTask.RecordChanges();
                _undoSystem.RegisterUndo(undoTask);

                MarkModified();

                RefreshView();
            }
            else
            {
                undoTask.Clear();
            }

            // Select the frame that was edited
            lv_frames.SelectedIndices.Clear();
            lv_frames.SelectedIndices.Add(frameView.FrameLoaded.Index);

            // Re-enable animation preview panel
            animationPreviewPanel.Enable();
        }

        /// <summary>
        /// Displays an interface for adding frames from external files
        /// </summary>
        private void AddFramesFromFiles()
        {
            Image[] images = _controller.ShowLoadImages(this);

            // If the array is null, no images were chosen
            if (images == null)
            {
                return;
            }

            try
            {
                FrameSizeMatchingSettings sizeMatching = new FrameSizeMatchingSettings();

                foreach (var image in images)
                {
                    if (image.Size != _viewAnimation.Size)
                    {
                        FramesRescaleSettingsView sizeMatchingForm = new FramesRescaleSettingsView("The frames being loaded have a different resolution than the target animation. Please select the scaling options for the frames:");

                        if (sizeMatchingForm.ShowDialog(this) == DialogResult.OK)
                        {
                            sizeMatching = sizeMatchingForm.GeneratedSettings;
                            break;
                        }

                        return;
                    }
                }

                List<Frame> frames = new List<Frame>();

                foreach (Image image in images)
                {
                    Bitmap tempBit = (Bitmap)image;
                    var bit = tempBit.Clone(new Rectangle(Point.Empty, tempBit.Size), tempBit.PixelFormat);
                    tempBit.Dispose();

                    Frame frame = _controller.FrameFactory.CreateFrame(bit.Width, bit.Height, null, false);
                    frame.SetFrameBitmap(bit);

                    frames.Add(frame);
                }

                AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

                _viewAnimation.AddFrames(frames.ToArray(), sizeMatching);

                undoTask.RecordChanges();
                _undoSystem.RegisterUndo(undoTask);

                MarkModified();

                RefreshView();

                lv_frames.Items[lv_frames.Items.Count - 1].Selected = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(@"There was an error loading the selected image:\n" + e, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Displays an interface for saving all currently selected frames
        /// </summary>
        private void SaveSelectedFrames()
        {
            string path = _controller.ShowSaveImage(out ImageFormat format, fileName: _viewAnimation.Name, owner: this);

            if (string.IsNullOrEmpty(path))
                return;

            string extension = Path.GetExtension(path);

            // Save the files
            var selectedFrames = GetSelectedFrames();
            var padCount = _viewAnimation.FrameCount.ToString().Length;
            foreach (var frame in selectedFrames)
            {
                string fileName = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path);

                if (selectedFrames.Count > 1)
                {
                    fileName += "_" + frame.Index.ToString("D" + padCount);
                }

                frame.GetComposedBitmap().Save(fileName + extension, format);
            }
        }

        /// <summary>
        /// Replaces the currently selected frame's image with one loaded from a file.
        /// If the dimensions don't match, the method triggers input from the user to handle the resizing operation of the image.
        /// If the selection is not set to a one frame only, the method does nothing
        /// </summary>
        private void ReplaceFromFile()
        {
            if (lv_frames.SelectedItems.Count != 1)
                return;

            IFrame selectedFrame = GetSelectedFrames()[0];

            Image image = _controller.ShowLoadImage(owner: this);

            try
            {
                AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

                Bitmap tempBit = (Bitmap)image;
                var bit = new Bitmap(tempBit);
                tempBit.Dispose();

                Frame newFrame = new Frame(null, bit.Width, bit.Height);
                newFrame.SetFrameBitmap(bit);
                newFrame.ID = selectedFrame.ID;

                if (bit.Size != _viewAnimation.Size)
                {
                    FramesRescaleSettingsView sizeMatchingForm = new FramesRescaleSettingsView("The frame being loaded has a different resolution than the target animation. Please select the scaling options for the frame:");

                    if (sizeMatchingForm.ShowDialog(this) == DialogResult.OK)
                    {
                        var sizeMatching = sizeMatchingForm.GeneratedSettings;
                        _viewAnimation.AddFrames(new[] { newFrame }, sizeMatching, selectedFrame.Index);
                        _viewAnimation.RemoveFrame(selectedFrame);
                    }
                    else
                    {
                        undoTask.Clear();
                        return;
                    }
                }

                undoTask.RecordChanges();
                _undoSystem.RegisterUndo(undoTask);

                MarkModified();
                RefreshView();
            }
            catch (Exception e)
            {
                MessageBox.Show(@"There was an error loading the selected image:\n" + e, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Event Handlers

        #region Filters Menu

        // 
        // Empty Filter menu item click
        // 
        private void tsm_emptyFilter_Click(object sender, EventArgs e)
        {
            DisplayFilterPreset(new FilterPreset("New Preset", new IFilter[] { }));
        }

        // 
        // Filter menu item click
        // 
        private void tsm_filterItem_Click(object sender, EventArgs e)
        {
            DisplayFilterPreset(new FilterPreset("New Preset", new [] { FilterStore.Instance.CreateFilter(((ToolStripMenuItem)sender).Tag as string) }));
        }

        // 
        // Preset menu item click
        // 
        private void tsm_presetItem_Click(object sender, EventArgs e)
        {
            DisplayFilterPreset(FilterStore.Instance.GetFilterPresetByName(((ToolStripMenuItem)sender).Tag as string));
        }

        #endregion

        // 
        // Form Closed event handler
        // 
        private void AnimationView_FormClosed(object sender, FormClosedEventArgs e)
        {
            _undoSystem.Clear();

            // Unload the animation preview panel so no lost references remain to the animation that this form was displaying
            animationPreviewPanel.Disable();
            animationPreviewPanel.LoadAnimation(null);

            Clipboard.ClipboardChanged -= _clipboardHandler;

            // Dispose of the list view images
            foreach (Image image in il_framesThumbs.Images)
            {
                image.Dispose();
            }

            // Dispose of the view animation
            _viewAnimation?.Dispose();

            il_framesThumbs.Images.Clear();
            il_framesThumbs.Dispose();

            lv_frames.Items.Clear();

            // Run garbage collector now
            GC.Collect();
        }

        // 
        // Form Key Down event handler
        // 
        private void AnimationView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Close();
            }
        }

        // 
        // List View Drag Operation event handler
        // 
        private void lv_frames_DragOperation(ListViewItemDragEventArgs eventArgs)
        {
            if (eventArgs.EventType == ListViewItemDragEventType.DragEnd)
            {
                // Create the undo task
                AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(_viewAnimation);

                // Move the frames now
                int newIndex = eventArgs.TargetItem.Index;

                List<IFrame> frames = eventArgs.DraggedItems.Select(item => _viewAnimation[item.Index]).ToList();
                
                foreach (IFrame frame in frames)
                {
                    _viewAnimation.RemoveFrame(frame);
                }

                foreach (var frame in frames)
                {
                    _viewAnimation.AddFrame(frame, Math.Min(_viewAnimation.FrameCount, newIndex++));
                }

                undoTask.RecordChanges();

                _undoSystem.RegisterUndo(undoTask);

                RefreshFramesView();
                animationPreviewPanel.LoadAnimation(_viewAnimation, false);

                MarkModified();

                eventArgs.Cancel = true;
            }
        }

        // 
        // Undo System undo registered
        // 
        private void undoSystem_UndoRegistered(object sender, UndoEventArgs e)
        {
            RefreshUndoControls();
        }

        // 
        // Clipboard Changed event handler
        // 
        private void clipboard_ClipboardChanged(object sender, ClipboardEventArgs eventArgs)
        {
            RefreshClipboardControls();
        }

        #region Menu Strip

        #region File menu

        // 
        // Add Frame From File menu item click
        // 
        private void tsm_addFrameFromFile_Click(object sender, EventArgs e)
        {
            AddFramesFromFiles();
        }

        // 
        // Save Animation Strip
        // 
        private void tsm_saveAnimationStrip_Click(object sender, EventArgs e)
        {
            _controller.ShowSaveAnimationStrip(_viewAnimation);
        }

        #endregion

        #region Edit menu

        // 
        // Undo menu item click
        // 
        private void tsm_undo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        // 
        // Redo menu item click
        // 
        private void tsm_redo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        // 
        // Copy menu item click
        // 
        private void tsm_copy_Click(object sender, EventArgs e)
        {
            CopySelectedFrames();
        }

        // 
        // Cut menu item click
        // 
        private void tsm_cut_Click(object sender, EventArgs e)
        {
            CutSelectedFrames();
        }

        // 
        // Paste menu item click
        // 
        private void tsm_paste_Click(object sender, EventArgs e)
        {
            PasteFrames();
        }

        //
        // Delete menu item click
        //
        private void tsm_delete_Click(object sender, EventArgs e)
        {
            DeleteSelectedFrames();
        }

        //
        // Save Selected Frames menu item click
        //
        private void tsm_saveSelected_Click(object sender, EventArgs e)
        {
            SaveSelectedFrames();
        }

        //
        // Replace From File menu item click
        //
        private void tsm_replaceFromFile_Click(object sender, EventArgs e)
        {
            ReplaceFromFile();
        }

        #endregion

        #region Frames menu

        // 
        // Insert Frame toolstrip menu button click
        // 
        private void tsm_insertFrame_Click(object sender, EventArgs e)
        {
            InsertFrame();
        }

        // 
        // Add New Frame toolstip menu button click
        // 
        private void tsm_addNewFrame_Click(object sender, EventArgs e)
        {
            AddNewFrame();
        }

        // 
        // Select All item click
        // 
        private void tsm_selectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        #endregion

        #region Animation menu

        // 
        // Reverse Frames item click
        // 
        private void tsm_reverseFrames_Click(object sender, EventArgs e)
        {
            ReverseFrames();
        }

        #endregion

        #endregion

        #region Toolbar

        // 
        // Apply Changes and Close toolbar button click
        // 
        private void tsb_applyChangesAndClose_Click(object sender, EventArgs e)
        {
            ApplyChangesAndClose();
        }

        // 
        // Apply Changes toolbar button click
        // 
        private void tsb_applyChanges_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        /*
        // 
        // Discard Changes And Close toolbar button click
        // 
        private void tsb_discardChanges_Click(object sender, EventArgs e)
        {
            DiscardChangesAndClose();
        }
        */
        // 
        // Edit Frame toolbar button click
        // 
        private void tsb_editFrame_Click(object sender, EventArgs e)
        {
            EditFrame();
        }

        // 
        // Insert Frame toolbar button click
        // 
        private void tsb_insertFrame_Click(object sender, EventArgs e)
        {
            InsertFrame();
        }

        // 
        // Add New Frame toolbar button click
        // 
        private void tsb_addNewFrame_Click(object sender, EventArgs e)
        {
            AddNewFrame();
        }

        // 
        // Resize Animation toolbar button click
        // 
        private void tsb_resizeAnim_Click(object sender, EventArgs e)
        {
            ResizeAnimation();
        }

        // 
        // Copy Frames toolbar button click
        // 
        private void tsb_copyFrames_Click(object sender, EventArgs e)
        {
            CopySelectedFrames();
        }

        // 
        // Cut Frames toolbar button click
        // 
        private void tsb_cutFrames_Click(object sender, EventArgs e)
        {
            CutSelectedFrames();
        }

        // 
        // Paste Frames toolbar button click
        // 
        private void tsb_pasteFrames_Click(object sender, EventArgs e)
        {
            PasteFrames();
        }

        // 
        // Undo toolbar button click
        // 
        private void tsb_undo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        // 
        // Redo toolbar button click
        // 
        private void tsb_redo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        // 
        // Animation Name textbox change
        // 
        private void txt_animName_TextChanged(object sender, EventArgs e)
        {
            UpdateAnimationName();
        }

        #endregion

        #region Frames Right-Click Context Menu

        //
        // Delete context menu button click
        //
        private void cmb_deleteFrames_Click(object sender, EventArgs e)
        {
            DeleteSelectedFrames();
        }

        //
        // Save Selected context menu button click
        //
        private void cmb_saveSelected_Click(object sender, EventArgs e)
        {
            SaveSelectedFrames();
        }

        //
        // Replace From Image context menu button click
        //
        private void cmb_replaceFromImage_Click(object sender, EventArgs e)
        {
            ReplaceFromFile();
        }

        //
        // Reverse Frames context menu button click
        //
        private void cmb_reverseFrames_Click(object sender, EventArgs e)
        {
            ReverseFrames();
        }

        //
        // Copy context menu button click
        //
        private void cmb_copyFrames_Click(object sender, EventArgs e)
        {
            CopySelectedFrames();
        }

        //
        // Cut context menu button click
        //
        private void cmb_cutFrames_Click(object sender, EventArgs e)
        {
            CutSelectedFrames();
        }

        #endregion

        #region Animation Right-Click Context Menu

        // 
        // Add Frame From File context menu button click
        // 
        private void cmb_addFrameFromFile_Click(object sender, EventArgs e)
        {
            AddFramesFromFiles();
        }

        // 
        // Paste Frames context menu button click
        // 
        private void cmb_pasteFrames_Click(object sender, EventArgs e)
        {
            PasteFrames();
        }

        #endregion

        // 
        // Enable Preview Checkbox tick
        // 
        private void cb_enablePreview_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_enablePreview.Checked)
            {
                animationPreviewPanel.Enable();
                panel1.Visible = true;
            }
            else
            {
                animationPreviewPanel.Disable();
                panel1.Visible = false;
            }
        }

        // 
        // FPS numeric up and down change
        // 
        private void nud_fps_ValueChanged(object sender, EventArgs e)
        {
            var playback = _viewAnimation.PlaybackSettings;

            playback.FPS = (int)nud_fps.Value;

            _viewAnimation.PlaybackSettings = playback;

            MarkModified();
        }

        // 
        // Frameskip checkbox check
        // 
        private void cb_frameskip_CheckedChanged(object sender, EventArgs e)
        {
            var playback = _viewAnimation.PlaybackSettings;

            playback.FrameSkip = cb_frameskip.Checked;

            _viewAnimation.PlaybackSettings = playback;

            MarkModified();
        }

        // 
        // Frames List View selected index changed event handler
        // 
        private void lv_frames_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Refreshes the toolbar where the Copy/Cut/Paste buttons are located
            RefreshButtons();
        }

        // 
        // Frames List View key down event handler
        // 
        private void lv_frames_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedFrames();
            }
        }

        //
        // Frames List View mouse click
        // 
        private void lv_frames_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                ShowFramesContextMenu();
            }
        }

        // 
        // Frames List View double click event handler
        // 
        private void lv_frames_DoubleClick(object sender, EventArgs e)
        {
            EditFrame();
        }

        #endregion

        /// <summary>
        /// Implements an animation modify undo task that undoes/redoes changes in the animation properties, including frame bitmap modifications.
        /// Currently, modifications that including and removing several frames in the same operation is a little glitchy, and may not function properly
        /// </summary>
        public class AnimationModifyUndoTask : IUndoTask
        {
            /// <summary>
            /// The animation to affect with this AnimationModifyUndoTask instance
            /// </summary>
            private readonly Animation _animation;

            /// <summary>
            /// A clone of the animation that was made before changes were made
            /// </summary>
            private readonly Animation _oldAnimation;

            /// <summary>
            /// The derivated compound task
            /// </summary>
            private IUndoTask _compoundTask;

            /// <summary>
            /// Initializes a new instance of the FramesModifyUndoTask class
            /// </summary>
            /// <param name="animation">The animation to affect with this FramesModifyUndoTask instance</param>
            public AnimationModifyUndoTask(Animation animation)
            {
                _animation = animation;
                _oldAnimation = DeepCloneAnimation(_animation);

                //_compoundTask = new GroupUndoTask(GetDescription());
            }

            /// <summary>
            /// Records the changes made in the animation
            /// </summary>
            public void RecordChanges()
            {
                LazyAnimationModifyUndoTask lazyUndoTask = new LazyAnimationModifyUndoTask(_animation, _oldAnimation, DeepCloneAnimation(_animation));

                _compoundTask = lazyUndoTask;
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                _oldAnimation?.Dispose();

                _compoundTask?.Clear();
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                _compoundTask.Undo();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                _compoundTask.Redo();
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                return "Frames modify";
            }

            /// <summary>
            /// Performs a deep clone of an animation, by copying all the data from the animation and IDs over
            /// </summary>
            /// <param name="animation">The animation to deep clone</param>
            /// <returns>A deep clone of the provided animation</returns>
            private Animation DeepCloneAnimation(Animation animation)
            {
                Animation anim = animation.Clone();
                
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    anim[i].ID = animation[i].ID;
                }

                anim.ID = animation.ID;

                return anim;
            }

            /// <summary>
            /// Represents an undo task that undoes and redoes modifications to an animation through a simple copy operation
            /// </summary>
            private class LazyAnimationModifyUndoTask : IUndoTask
            {
                /// <summary>
                /// The animation to modify
                /// </summary>
                private readonly Animation _animation;

                /// <summary>
                /// A copy of the old animation to copy to when undoing
                /// </summary>
                private readonly Animation _oldAnimation;

                /// <summary>
                /// A copy of the new animation to copy to when redoing
                /// </summary>
                private readonly Animation _newAnimation;

                /// <summary>
                /// Initializes a new class of the LazyAnimationModifyUndoTask
                /// </summary>
                /// <param name="animation">The animation to modify</param>
                /// <param name="oldAnimation">A copy of the old animation to copy to when undoing</param>
                /// <param name="newAnimation">A copy of the new animation to copy to when redoing</param>
                public LazyAnimationModifyUndoTask(Animation animation, Animation oldAnimation, Animation newAnimation)
                {
                    _animation = animation;
                    _oldAnimation = oldAnimation;
                    _newAnimation = newAnimation;
                }

                /// <summary>
                /// Clears this undo task and cleans any utilized resource
                /// </summary>
                public void Clear()
                {
                    _oldAnimation.Dispose();
                    _newAnimation.Dispose();
                }

                /// <summary>
                /// Undoes this LazyAnimationModifyUndoTask
                /// </summary>
                public void Undo()
                {
                    _animation.CopyFrom(_oldAnimation, false);

                    for (int i = 0; i < _animation.FrameCount; i++)
                    {
                        _animation[i].ID = _oldAnimation[i].ID;
                    }
                }

                /// <summary>
                /// Redoes this LazyAnimationModifyUndoTask
                /// </summary>
                public void Redo()
                {
                    _animation.CopyFrom(_newAnimation, false);

                    for (int i = 0; i < _animation.FrameCount; i++)
                    {
                        _animation[i].ID = _newAnimation[i].ID;
                    }
                }

                /// <summary>
                /// Gets the description for this undo task
                /// </summary>
                /// <returns>The description for this undo task</returns>
                public string GetDescription()
                {
                    return "";
                }
            }

            /// <summary>
            /// Implements an animation modify undo task that undoes/redoes adding and deleting of frames
            /// </summary>
            public class FramesAddDeleteUndoTask : IUndoTask
            {
                /// <summary>
                /// The animation that will be modified by this FramesDeleteUndoTask
                /// </summary>
                private readonly Animation _animation;

                /// <summary>
                /// The description for this FramesAddDeleteUndoTask
                /// </summary>
                private readonly string _description;

                /// <summary>
                /// Whether this action has been undone
                /// </summary>
                private bool _undone;

                /// <summary>
                /// The type of this operation
                /// </summary>
                private readonly FrameAddDeleteOperationType _operationType;

                /// <summary>
                /// The indices of the frames being deleted
                /// </summary>
                private readonly List<int> _frameIndices;

                /// <summary>
                /// The frames that were deleted
                /// </summary>
                private readonly List<IFrame> _frames;

                /// <summary>
                /// Initializes a new instance of a FramesDeleteUndoTask class
                /// </summary>
                /// <param name="animation">The animation that will be modified by this FramesDeleteUndoTask</param>
                /// <param name="operationType">The type of operation to perform on this FramesAddDeleteUndoTask</param>
                /// <param name="description">The description for this FramesAddDeleteUndoTask</param>
                public FramesAddDeleteUndoTask(Animation animation, FrameAddDeleteOperationType operationType, string description)
                {
                    _animation = animation;
                    _description = description;
                    _operationType = operationType;
                    _frameIndices = new List<int>();
                    _frames = new List<IFrame>();
                }

                /// <summary>
                /// Registers a frame that is being deleted on this FramesDeleteUndoTask instance
                /// </summary>
                /// <param name="frame">The frame being deleted</param>
                /// <param name="index">The index to register. In case this is a delete undo operation, this value is not used</param>
                public void RegisterFrame(IFrame frame, int index = 0)
                {
                    if (frame.Animation == null)
                        _frameIndices.Add(index);
                    else
                        _frameIndices.Add((_operationType == FrameAddDeleteOperationType.Delete ? frame.Index : index));

                    _frames.Add(frame);
                }

                /// <summary>
                /// Clears this UndoTask object
                /// </summary>
                public void Clear()
                {
                    if (!_undone)
                    {
                        // Dispose of frames that are not in animations
                        foreach (IFrame frame in _frames)
                        {
                            if (frame.Animation == null)
                            {
                                frame.Dispose();
                            }
                        }
                    }

                    _frames.Clear();
                    _frameIndices.Clear();
                }

                /// <summary>
                /// Undoes this task
                /// </summary>
                public void Undo()
                {
                    if (_operationType == FrameAddDeleteOperationType.Delete)
                    {
                        for (int i = _frameIndices.Count - 1; i >= 0; i--)
                        {
                            _animation.AddFrame(_frames[i], _frameIndices[i]);
                        }
                    }
                    else
                    {
                        for (int i = _frameIndices.Count - 1; i >= 0; i--)
                        {
                            _animation.RemoveFrameIndex(_frameIndices[i]);
                        }
                    }

                    _undone = true;
                }

                /// <summary>
                /// Redoes this task
                /// </summary>
                public void Redo()
                {
                    if (_operationType == FrameAddDeleteOperationType.Delete)
                    {
                        for (int i = _frameIndices.Count - 1; i >= 0; i--)
                        {
                            _animation.RemoveFrameIndex(_frameIndices[i]);
                        }
                    }
                    else
                    {
                        //for (int i = 0; i < frames.Count; i++)
                        for (int i = _frameIndices.Count - 1; i >= 0; i--)
                        {
                            _animation.AddFrame(_frames[i], _frameIndices[i]);
                        }
                    }

                    _undone = false;
                }

                /// <summary>
                /// Returns a short string description of this UndoTask
                /// </summary>
                /// <returns>A short string description of this UndoTask</returns>
                public string GetDescription()
                {
                    return _description;
                }
            }

            /// <summary>
            /// Implements a frame modify undo task that undoes/redoes the modification of a frame's contents
            /// </summary>
            public class FrameEditUndoTask : IUndoTask
            {
                private readonly Animation _animation;

                /// <summary>
                /// The frames that were deleted
                /// </summary>
                private readonly IFrame _frame;

                /// <summary>
                /// The old (undo) bitmap
                /// </summary>
                private readonly IFrame _oldFrame;

                /// <summary>
                /// The new (redo) bitmap
                /// </summary>
                private IFrame _newFrame;

                /// <summary>
                /// Initializes a new instance of a FrameEditUndoTask class
                /// </summary>
                /// <param name="animation">The animation that holds the frame</param>
                /// <param name="frame">The frame to record the changes made to</param>
                /// <param name="oldFrame">An (optional) starting value for the old frame</param>
                public FrameEditUndoTask(Animation animation, IFrame frame, IFrame oldFrame = null)
                {
                    _animation = animation;
                    _frame = frame;
                    _oldFrame = (oldFrame ?? frame).Clone();
                    _oldFrame.ID = frame.ID;
                }

                /// <summary>
                /// Records the changes made to the frame's bitmap
                /// </summary>
                /// <param name="newFrame">An (optional) value for the new frame</param>
                public void RecordChanges(IFrame newFrame = null)
                {
                    _newFrame = (newFrame ?? _frame).Clone();
                    _newFrame.ID = _frame.ID;
                }

                /// <summary>
                /// Clears this UndoTask object
                /// </summary>
                public void Clear()
                {
                    _newFrame.Dispose();
                    _oldFrame.Dispose();
                }

                /// <summary>
                /// Undoes this task
                /// </summary>
                public void Undo()
                {
                    //_frame.CopyFrom(_oldFrame);
                    _animation.GetFrameByID(_frame.ID).CopyFrom(_oldFrame);
                }

                /// <summary>
                /// Redoes this task
                /// </summary>
                public void Redo()
                {
                    //_frame.CopyFrom(_newFrame);
                    _animation.GetFrameByID(_frame.ID).CopyFrom(_newFrame);
                }

                /// <summary>
                /// Returns a short string description of this UndoTask
                /// </summary>
                /// <returns>A short string description of this UndoTask</returns>
                public string GetDescription()
                {
                    return "Frame edit";
                }
            }

            /// <summary>
            /// Implements an aniation undo task that undoes/redoes the modification on the order of frames
            /// </summary>
            public class FrameReoderUndoTask : IUndoTask
            {
                /// <summary>
                /// The animation to affect
                /// </summary>
                private readonly Animation _animation;

                /// <summary>
                /// The old (undo) index
                /// </summary>
                private readonly int _oldIndex;

                /// <summary>
                /// The new (redo) index
                /// </summary>
                private readonly int _newIndex;

                /// <summary>
                /// Initializes a new instance of the FramesReorderUndoTask class
                /// </summary>
                /// <param name="anim">The animation to affect</param>
                /// <param name="oldIndex">The old (undo) index</param>
                /// <param name="newIndex">The new (redo) index</param>
                public FrameReoderUndoTask(Animation anim, int oldIndex, int newIndex)
                {
                    _animation = anim;
                    _oldIndex = oldIndex;
                    _newIndex = newIndex;
                }

                /// <summary>
                /// Clears this UndoTask object
                /// </summary>
                public void Clear()
                {

                }

                /// <summary>
                /// Undoes this task
                /// </summary>
                public void Undo()
                {
                    _animation.SwapFrameIndices(_oldIndex, _newIndex);
                }

                /// <summary>
                /// Redoes this task
                /// </summary>
                public void Redo()
                {
                    _animation.SwapFrameIndices(_oldIndex, _newIndex);
                }

                /// <summary>
                /// Returns a short string description of this UndoTask
                /// </summary>
                /// <returns>A short string description of this UndoTask</returns>
                public string GetDescription()
                {
                    return "Frame Reoder";
                }
            }

            /// <summary>
            /// Implements an animation resize undo task that undoes/redoes animation resize operations
            /// </summary>
            public class AnimationResizeUndoTask : IUndoTask
            {
                /// <summary>
                /// The animation to affect with this AnimationResizeUndoTask instance
                /// </summary>
                private readonly Animation _animation;
                
                /// <summary>
                /// The old size of the animation before resizing
                /// </summary>
                private readonly AnimationResizeSettings _oldResizeSettings;

                /// <summary>
                /// The resize settings for the operation
                /// </summary>
                private readonly AnimationResizeSettings _newResizeSettings;

                /// <summary>
                /// Initializes a new instance of the AnimationResizeUndoTask class
                /// </summary>
                /// <param name="animation">The animation to affect with this AnimationResizeUndoTask instance</param>
                /// <param name="oldSize">The old size of the animation before resizing</param>
                /// <param name="resizeSettings">The resize settings for the operation</param>
                public AnimationResizeUndoTask(Animation animation, Size oldSize, AnimationResizeSettings resizeSettings)
                {
                    _animation = animation;
                    _oldResizeSettings = new AnimationResizeSettings { InterpolationMode = resizeSettings.InterpolationMode, NewWidth = oldSize.Width, NewHeight = oldSize.Height, PerFrameScalingMethod = resizeSettings.PerFrameScalingMethod };
                    _newResizeSettings = resizeSettings;
                }

                /// <summary>
                /// Clears this UndoTask object
                /// </summary>
                public void Clear()
                {
                    
                }

                /// <summary>
                /// Undoes this task
                /// </summary>
                public void Undo()
                {
                    _animation.Resize(_oldResizeSettings);
                }

                /// <summary>
                /// Redoes this task
                /// </summary>
                public void Redo()
                {
                    _animation.Resize(_newResizeSettings);

                    // Apply the frame contents now
                    for (int i = 0; i < _animation.FrameCount; i++)
                    {
                        IFrame frame = _animation[i];
                        frame.CopyFrom(frame);
                    }
                }

                /// <summary>
                /// Returns a short string description of this UndoTask
                /// </summary>
                /// <returns>A short string description of this UndoTask</returns>
                public string GetDescription()
                {
                    return "Animation Resize";
                }
            }
        }

        /// <summary>
        /// Specifies which operation a FrameAddDeleteUndoTask is currently performing
        /// </summary>
        public enum FrameAddDeleteOperationType
        {
            /// <summary>
            /// Specifies a Delete Frames operation
            /// </summary>
            Delete,
            /// <summary>
            /// Specifies an Add Frames operaiton
            /// </summary>
            Add
        }
    }
}
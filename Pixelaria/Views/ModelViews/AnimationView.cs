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
        public static DataClipboard Clipboard { get; private set; }

        /// <summary>
        /// The current animation being displayed
        /// </summary>
        private Animation currentAnimation;

        /// <summary>
        /// The Animation object that is currently used
        /// </summary>
        private Animation viewAnimation;

        /// <summary>
        /// The Controller object that owns this AnimationView
        /// </summary>
        private Controller controller;

        /// <summary>
        /// The undo system for this AnimationView
        /// </summary>
        private UndoSystem undoSystem;

        /// <summary>
        /// Event handler for the ClipboardChanged event
        /// </summary>
        private DataClipboard.ClipboardEventHandler clipboardHandler;

        /// <summary>
        /// Event handler for a filter item click
        /// </summary>
        private EventHandler filterClickEventHandler;

        /// <summary>
        /// Event handler for a filter preset item click
        /// </summary>
        private EventHandler presetClickEventHandler;

        /// <summary>
        /// Gets the current animation being displayed on this AnimationView
        /// </summary>
        public Animation CurrentAnimation { get { return currentAnimation; } }

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

            this.filterClickEventHandler = tsm_filterItem_Click;
            this.presetClickEventHandler = tsm_presetItem_Click;

            this.UpdateFilterList();
            this.UpdateFilterPresetList();

            this.controller = controller;
            this.undoSystem = new UndoSystem();
            this.currentAnimation = animation;
            this.viewAnimation = currentAnimation.Clone();

            for (int i = 0; i < this.viewAnimation.FrameCount; i++)
            {
                this.viewAnimation[i].ID = animation[i].ID;
            }

            clipboardHandler = clipboard_ClipboardChanged;
            Clipboard.ClipboardChanged += clipboardHandler;

            this.undoSystem.UndoRegistered += undoSystem_UndoRegistered;

            RefreshView();

            // Mark unmodified because some operations on the constructor may change the value of the modified field for some reason
            // TODO: Check why the 'modified' field starts true when it reaches this point of the constructor
            MarkUnmodified();

            animationPreviewPanel.SetPlayback(true);
        }

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
            animationPreviewPanel.LoadAnimation(viewAnimation, false);
        }

        /// <summary>
        /// Refreshs the title of this view
        /// </summary>
        private void RefreshTitle()
        {
            this.Text = "Animation [" + currentAnimation.Name + "]" + (modified ? "*" : "");
        }

        /// <summary>
        /// Refreshes the buttons of the form
        /// </summary>
        private void RefreshButtons()
        {
            this.tsb_applyChanges.Enabled = this.modified;

            // Refreshes the 'Reverse Frames' menu item
            if (lv_frames.SelectedIndices.Count == 2)
                tsm_reverseFrames.Text = "Swap Frames";
            else
                tsm_reverseFrames.Text = "Reverse Frames";

            RefreshClipboardControls();
            RefreshUndoControls();
        }

        /// <summary>
        /// Refreshes the playback information portion of the view
        /// </summary>
        private void RefreshPlaybackInfo()
        {
            nud_fps.Value = viewAnimation.PlaybackSettings.FPS;
            cb_frameskip.Checked = viewAnimation.PlaybackSettings.FrameSkip;
        }

        /// <summary>
        /// Refreshes the animation information portion of the view
        /// </summary>
        private void RefreshAnimationInfo()
        {
            txt_animName.Text = viewAnimation.Name;

            tssl_dimensions.Text = viewAnimation.Width + " x " + viewAnimation.Height;
            tssl_frameCount.Text = viewAnimation.FrameCount + "";
            tssl_memory.Text = Utilities.FormatByteSize(viewAnimation.CalculateMemoryUsageInBytes());
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
            int width = 256;
            int height = 256;

            if (viewAnimation.Width >= viewAnimation.Height)
            {
                if (width < viewAnimation.Width)
                {
                    scaleX = (float)width / viewAnimation.Width;
                    scaleY = scaleX;
                }
            }
            else
            {
                if (height < viewAnimation.Height)
                {
                    scaleY = (float)height / viewAnimation.Height;
                    scaleX = scaleY;
                }
            }

            il_framesThumbs.ImageSize = new Size(Math.Min((int)(viewAnimation.Width * scaleX), 256), Math.Min((int)(viewAnimation.Height * scaleY), 256));
            for (int i = 0; i < viewAnimation.FrameCount; i++)
            {
                Frame frame = viewAnimation.GetFrameAtIndex(i);

                il_framesThumbs.Images.Add(frame.GenerateThumbnail(frame.Width, frame.Height, false, false, Color.White));

                ListViewItem frameItem = new ListViewItem();

                frameItem.Text = "Frame " + (i + 1);
                frameItem.ImageIndex = i;
                frameItem.Tag = frame;

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

            tsm_paste.Enabled = tsb_pasteFrames.Enabled = (Clipboard.CurrentDataType == FrameListClipboardObject.DataType || Clipboard.CurrentDataType == ImageStreamClipboardObject.DataType);
        }

        /// <summary>
        /// Refreshes the undo/redo toolbar buttons/menu items
        /// </summary>
        private void RefreshUndoControls()
        {
            tsm_undo.Enabled = tsb_undo.Enabled = undoSystem.CanUndo;
            tsm_redo.Enabled = tsb_redo.Enabled = undoSystem.CanRedo;

            if (this.tsb_undo.Enabled)
            {
                this.tsm_undo.Text = this.tsb_undo.ToolTipText = "Undo " + undoSystem.NextUndo.GetDescription();

            }
            else
            {
                this.tsb_undo.ToolTipText = "";
                this.tsm_undo.Text = "Undo";
            }

            if (this.tsb_redo.Enabled)
            {
                this.tsm_redo.Text = this.tsb_redo.ToolTipText = "Redo " + undoSystem.NextRedo.GetDescription();
            }
            else
            {
                this.tsb_redo.ToolTipText = "";
                this.tsm_redo.Text = "Redo";
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
                viewAnimation.Name = txt_animName.Text;

                // APPLY CHANGES
                currentAnimation.CopyFrom(viewAnimation, false);

                for (int i = 0; i < currentAnimation.FrameCount; i++)
                {
                    currentAnimation[i].ID = viewAnimation[i].ID;
                }

                controller.UpdatedAnimation(currentAnimation);
                controller.MarkUnsavedChanges(true);
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

            ApplyChanges();

            this.Close();
        }

        /// <summary>
        /// Validates the fields of this AnimationView
        /// </summary>
        /// <returns>Whether the validation was successful</returns>
        private bool ValidateFields()
        {
            bool valid = true;
            string validation;

            // Animation name
            validation = controller.AnimationValidator.ValidateAnimationName(txt_animName.Text, currentAnimation);
            if (validation != "")
            {
                txt_animName.BackColor = Color.LightPink;
                tsl_error.Text = validation;
                valid = false;
            }
            else
            {
                txt_animName.BackColor = Color.White;
            }

            tsl_error.Visible = !valid;

            tsb_applyChangesAndClose.Enabled = valid;

            return valid;
        }

        /// <summary>
        /// Gets the ListViewItem object that is currently representing the given Frame object
        /// </summary>
        /// <param name="frame">The frame object to get the respective list view item representation</param>
        /// <returns>A ListViewItem object that is currently representing the given Frame object</returns>
        private ListViewItem GetListViewItemForFrame(Frame frame)
        {
            foreach (ListViewItem item in lv_frames.Items)
            {
                if (item.Tag == frame)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Undoes the last done task recorded by the form's undo system
        /// </summary>
        private void Undo()
        {
            if (undoSystem.CanUndo)
            {
                undoSystem.Undo();

                MarkModified();

                RefreshView();
            }
        }

        /// <summary>
        /// Redoes the last undone task recorded by the form's undo system
        /// </summary>
        private void Redo()
        {
            if (undoSystem.CanRedo)
            {
                undoSystem.Redo();

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
                ToolStripMenuItem tsm_filterItem = new ToolStripMenuItem(filterNames[i], iconList[i]);

                tsm_filterItem.Tag = filterNames[i];
                tsm_filterItem.Click += filterClickEventHandler;

                tsm_filters.DropDownItems.Add(tsm_filterItem);
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
                ToolStripMenuItem tsm_emptyItem = new ToolStripMenuItem("Empty");

                tsm_emptyItem.Enabled = false;

                tsm_filterPresets.DropDownItems.Add(tsm_emptyItem);
            }

            // Create and add all the new filter items
            for (int i = 0; i < presets.Length; i++)
            {
                ToolStripMenuItem tsm_presetItem = new ToolStripMenuItem(presets[i].Name, tsm_filterPresets.Image);

                tsm_presetItem.Tag = presets[i].Name;
                tsm_presetItem.Click += presetClickEventHandler;

                tsm_filterPresets.DropDownItems.Add(tsm_presetItem);
            }
        }

        /// <summary>
        /// Displays a BaseFilterView with the given FilterPreset loaded
        /// </summary>
        /// <param name="filterPreset">The filter preset to load on the BaseFilterView</param>
        private void DisplayFilterPreset(FilterPreset filterPreset)
        {
            AnimationFilterView afv = new AnimationFilterView(filterPreset, this.viewAnimation);

            AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(this.viewAnimation);

            if (afv.ShowDialog(this) == DialogResult.OK && afv.ChangesDetected())
            {
                undoTask.RecordChanges();

                undoSystem.RegisterUndo(undoTask);

                RefreshView();
            }
            else
            {
                undoTask.Clear();
            }
        }

        #endregion

        #region Animation Related Methods

        /// <summary>
        /// Displays an interface where the user can input settings for resizing the current animation
        /// </summary>
        public void ResizeAnimation()
        {
            AnimationResizeView arv = new AnimationResizeView(viewAnimation, viewAnimation.Width, viewAnimation.Height);

            if (arv.ShowDialog(this) == DialogResult.OK)
            {
                AnimationResizeSettings settings = arv.GeneratedSettings;

                AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(viewAnimation);

                viewAnimation.Resize(settings);

                MarkModified();
                RefreshView();

                undoTask.RecordChanges();
                undoSystem.RegisterUndo(undoTask);
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
                foreach (ListViewItem item in lv_frames.SelectedItems)
                {
                    frameIndicesToReverse.Add(item.Index);
                }
            }
            
            ///// Reverse the frames

            // Create an undo task
            AnimationModifyUndoTask amu = new AnimationModifyUndoTask(viewAnimation);

            for (int i = 0; i < frameIndicesToReverse.Count / 2; i++)
            {
                viewAnimation.SwapFrameIndices(frameIndicesToReverse[i], frameIndicesToReverse[frameIndicesToReverse.Count - i - 1]);
            }

            // Record the undo operation
            amu.RecordChanges();
            undoSystem.RegisterUndo(amu);

            MarkModified();

            RefreshView();
        }

        #endregion

        #region Frame Related Methods

        /// <summary>
        /// Deletes the currently selected frames
        /// </summary>
        private void DeleteSelectedFrames()
        {
            if (lv_frames.SelectedItems.Count > 0)
            {
                FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(viewAnimation, FrameAddDeleteOperationType.Delete, "Frames Deleted");

                // Delete selected frames
                foreach (ListViewItem item in lv_frames.SelectedItems)
                {
                    undoTask.RegisterFrame(item.Tag as Frame);

                    viewAnimation.RemoveFrame(item.Tag as Frame);
                }

                undoSystem.RegisterUndo(undoTask);

                MarkModified();

                RefreshView();
            }
        }

        /// <summary>
        /// Copies the selected frames to the clipboard
        /// </summary>
        private void CopySelectedFrames()
        {
            // Struct initialization
            FrameListClipboardObject frameListClip = new FrameListClipboardObject();

            int cnt = 0;

            foreach (ListViewItem item in lv_frames.SelectedItems)
            {
                Frame frame = (item.Tag as Frame).Clone();

                if (cnt == 0)
                {
                    // Copy the frame to the clipboard too
                    MemoryStream stream = new MemoryStream();

                    frame.GetComposedBitmap().Save(stream, ImageFormat.Png);
                    stream.Position = 0;

                    System.Windows.Forms.Clipboard.SetImage(frame.GetComposedBitmap());
                    System.Windows.Forms.Clipboard.SetData("PNG", stream);
                }

                frameListClip.AddFrame(frame);

                cnt++;
            }
            
            Clipboard.SetObject(frameListClip);
        }

        /// <summary>
        /// Cuts the selected frames to the clipboard
        /// </summary>
        private void CutSelectedFrames()
        {
            // Struct initialization
            FrameListClipboardObject frameListClip = new FrameListClipboardObject();

            foreach (ListViewItem item in lv_frames.SelectedItems)
            {
                frameListClip.AddFrame((item.Tag as Frame).Clone());
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
                FrameListClipboardObject frameListClip = (FrameListClipboardObject)Clipboard.GetObject();

                FrameSizeMatchingSettings sizeMatching = new FrameSizeMatchingSettings() {  AnimationDimensionMatchMethod = AnimationDimensionMatchMethod.UseNewSize,
                                                                                            InterpolationMode = InterpolationMode.NearestNeighbor,
                                                                                            PerFrameScalingMethod = PerFrameScalingMethod.PlaceAtTopLeft };

                if(viewAnimation.FrameCount > 0)
                {
                    // Check if there are any frames with different dimensions than the current animation
                    foreach (Frame frame in frameListClip.Frames)
                    {
                        if (frame.Width != viewAnimation.Width || frame.Height != viewAnimation.Height)
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
                            break;
                        }
                    }
                }

                // Gets the index to position the frames
                int index = -1;
                int undoAddIndex;

                for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
                {
                    index = Math.Max(index, lv_frames.SelectedIndices[i]);
                }

                if (index == -1)
                    index = lv_frames.Items.Count;

                undoAddIndex = index;

                FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(viewAnimation, FrameAddDeleteOperationType.Add, "Frames Pasted");

                // Maintain a copy of the list of added frames so the control can select them after
                List<Frame> copiedFrames = new List<Frame>();
                foreach (Frame frame in frameListClip.Frames)
                {
                    Frame newFrame = controller.FrameFactory.CloneFrame(frame);
                    copiedFrames.Add(newFrame);

                    undoTask.RegisterFrame(newFrame, undoAddIndex++);
                }

                viewAnimation.AddFrames(copiedFrames, sizeMatching, index);

                MarkModified();

                undoSystem.RegisterUndo(undoTask);

                RefreshView();

                // Select the newly added frames
                foreach (Frame frame in copiedFrames)
                {
                    GetListViewItemForFrame(frame).Selected = true;
                }
            }
            // Image pasting
            else if (Clipboard.CurrentDataType == ImageStreamClipboardObject.DataType)
            {
                // Recreate the frame from the image stream
                ImageStreamClipboardObject imgStr = Clipboard.GetObject() as ImageStreamClipboardObject;

                Bitmap bitmap = Bitmap.FromStream(imgStr.ImageStream) as Bitmap;

                Frame frame = controller.FrameFactory.CreateFrame(bitmap.Width, bitmap.Height, null, false);

                frame.SetFrameBitmap(bitmap);

                FrameSizeMatchingSettings sizeMatching = new FrameSizeMatchingSettings();

                // Check if there are any frames with different dimensions than the current animation
                if (frame.Width != viewAnimation.Width || frame.Height != viewAnimation.Height)
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

                // Gets the index to position the frames
                int index = -1;

                List<Frame> copiedFrames = new List<Frame>();
                copiedFrames.Add(frame);

                FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(viewAnimation, FrameAddDeleteOperationType.Add, "Frames Pasted");

                for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
                {
                    index = Math.Max(index, lv_frames.SelectedIndices[i]);
                }

                if (index == -1)
                    index = lv_frames.Items.Count;

                undoTask.RegisterFrame(copiedFrames[0], index);

                viewAnimation.AddFrames(copiedFrames, sizeMatching, index);

                MarkModified();

                undoSystem.RegisterUndo(undoTask);

                RefreshView();

                // Select the newly added frames
                foreach (Frame f in copiedFrames)
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

            FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(viewAnimation, FrameAddDeleteOperationType.Add, "Frame Inserted");

            undoTask.RegisterFrame(viewAnimation.CreateFrame(index), index);

            undoSystem.RegisterUndo(undoTask);

            MarkModified();

            RefreshView();

            lv_frames.Items[(index == -1 ? lv_frames.Items.Count -1 : index)].Selected = true;
        }

        /// <summary>
        /// Adds a new frame at the end of the frame range
        /// </summary>
        private void AddNewFrame()
        {
            FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(viewAnimation, FrameAddDeleteOperationType.Add, "Frame Added");

            int index = viewAnimation.FrameCount;

            undoTask.RegisterFrame(viewAnimation.CreateFrame(), index);

            undoSystem.RegisterUndo(undoTask);

            MarkModified();

            RefreshView();

            lv_frames.Items[lv_frames.Items.Count - 1].Selected = true;
        }

        /// <summary>
        /// Opens a view for editing the currentl
        /// y selected frames
        /// </summary>
        private void EditFrame()
        {
            if (lv_frames.SelectedItems.Count == 0)
                return;

            AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(viewAnimation);

            // Temporarely disable the panel while showing the form so we don't waste CPU
            animationPreviewPanel.Disable();

            // Get the currently selected frame
            Frame frame = lv_frames.SelectedItems[0].Tag as Frame;

            FrameView frameView = new FrameView(controller, frame);
            frameView.ShowDialog(this);

            // Mark animation as modified if the frame view has modified any frames
            if (frameView.ModifiedFrames)
            {
                undoTask.RecordChanges();
                undoSystem.RegisterUndo(undoTask);

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
        private void AddFrameFromFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "All Images (*.png, *.jpg, *jpeg, *.bmp, *.gif, *.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tiff|Png Images (*.png)|*.png|Bitmap Images (*.bmp)|*.bmp|Jpeg Images (*.jpg, *.jpeg)|*.jpg;*.jpeg|Gif Images (*.gif)|*.giff|Tiff Images (*.tiff)|*.tiff";

            if(ofd.ShowDialog(this) == DialogResult.OK)
            {
                Bitmap bit = null;

                try
                {
                    Bitmap tempBit = (Bitmap)Image.FromFile(ofd.FileName);

                    bit = new Bitmap(tempBit);

                    tempBit.Dispose();

                    FrameSizeMatchingSettings sizeMatching = new FrameSizeMatchingSettings();

                    if (bit.Width != viewAnimation.Width || bit.Height != viewAnimation.Height)
                    {
                        FramesRescaleSettingsView sizeMatchingForm = new FramesRescaleSettingsView("The frame being loaded has a different resolution than the target animation. Please select the scaling options for the frame:");

                        if (sizeMatchingForm.ShowDialog(this) == DialogResult.OK)
                        {
                            sizeMatching = sizeMatchingForm.GeneratedSettings;
                        }
                        else
                        {
                            return;
                        }
                    }

                    AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(this.viewAnimation);

                    Frame frame = controller.FrameFactory.CreateFrame(bit.Width, bit.Height, null, false);
                    frame.SetFrameBitmap(bit);

                    viewAnimation.AddFrames(new Frame[] { frame }, sizeMatching, -1);

                    MarkModified();

                    RefreshView();

                    lv_frames.Items[lv_frames.Items.Count - 1].Selected = true;

                    undoTask.RecordChanges();
                    this.undoSystem.RegisterUndo(undoTask);
                }
                catch (Exception e)
                {
                    MessageBox.Show("There was an error loading the selected image:\n" + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            DisplayFilterPreset(new FilterPreset("New Preset", new IFilter[] { FilterStore.Instance.CreateFilter(((ToolStripMenuItem)sender).Tag as string) }));
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
            undoSystem.Clear();

            // Unload the animation preview panel so no lost references remain to the animation that this form was displaying
            animationPreviewPanel.Disable();
            animationPreviewPanel.LoadAnimation(null);

            Clipboard.ClipboardChanged -= clipboardHandler;

            // Dispose of the list view images
            foreach (Image image in il_framesThumbs.Images)
            {
                image.Dispose();
            }

            // Dispose of the view animation
            if (viewAnimation != null)
            {
                viewAnimation.Dispose();
            }

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
                this.Close();
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
                AnimationModifyUndoTask undoTask = new AnimationModifyUndoTask(viewAnimation);

                // Move the frames now
                int newIndex = eventArgs.TargetItem.Index;

                List<Frame> frames = new List<Frame>();

                foreach (ListViewItem item in eventArgs.DraggedItems)
                {
                    frames.Add(viewAnimation[item.Index]);
                }

                foreach (Frame frame in frames)
                {
                    viewAnimation.RemoveFrame(frame);
                    viewAnimation.AddFrame(frame, newIndex++);
                }

                undoTask.RecordChanges();

                undoSystem.RegisterUndo(undoTask);

                RefreshFramesView();
                animationPreviewPanel.LoadAnimation(this.viewAnimation, false);

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

        // 
        // Add Frame From File menu item click
        // 
        private void tsm_addFrameFromFile_Click(object sender, EventArgs e)
        {
            AddFrameFromFile();
        }

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
            viewAnimation.PlaybackSettings.FPS = (int)nud_fps.Value;
            MarkModified();
        }

        // 
        // Frameskip checkbox check
        // 
        private void cb_frameskip_CheckedChanged(object sender, EventArgs e)
        {
            viewAnimation.PlaybackSettings.FrameSkip = cb_frameskip.Checked;
            MarkModified();
        }

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

        // 
        // Discard Changes And Close toolbar button click
        // 
        private void tsb_discardChanges_Click(object sender, EventArgs e)
        {
            DiscardChangesAndClose();
        }

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
        // Undo toolbar button click
        // 
        private void tsb_undo_Click(object sender, EventArgs e)
        {
            Undo();
        }
        // 
        // Undo menu item click
        // 
        private void tsm_undo_Click(object sender, EventArgs e)
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
        // Redo menu item click
        // 
        private void tsm_redo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        // 
        // Reverse Frames item click
        // 
        private void tsm_reverseFrames_Click(object sender, EventArgs e)
        {
            ReverseFrames();
        }

        // 
        // Select All item click
        // 
        private void tsm_selectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        // 
        // Animation Name textbox change
        // 
        private void txt_animName_TextChanged(object sender, EventArgs e)
        {
            MarkModified();
            ValidateFields();
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
        // Frames List View double click event handler
        // 
        private void lv_frames_DoubleClick(object sender, EventArgs e)
        {
            EditFrame();
        }

        #endregion

        /// <summary>
        /// Implements an animation modify undo task that undoes/redoes adding and deleting of frames
        /// </summary>
        public class FramesAddDeleteUndoTask : IUndoTask
        {
            /// <summary>
            /// The animation that will be modified by this FramesDeleteUndoTask
            /// </summary>
            private Animation animation;

            /// <summary>
            /// The description for this FramesAddDeleteUndoTask
            /// </summary>
            private string description;

            /// <summary>
            /// Whether this action has been undone
            /// </summary>
            private bool undone = false;

            /// <summary>
            /// The type of this operation
            /// </summary>
            private FrameAddDeleteOperationType operationType;

            /// <summary>
            /// The indices of the frames being deleted
            /// </summary>
            private List<int> frameIndices;

            /// <summary>
            /// The frames that were deleted
            /// </summary>
            private List<Frame> frames;

            /// <summary>
            /// Initializes a new instance of a FramesDeleteUndoTask class
            /// </summary>
            /// <param name="animation">The animation that will be modified by this FramesDeleteUndoTask</param>
            /// <param name="operationType">The type of operation to perform on this FramesAddDeleteUndoTask</param>
            /// <param name="description">The description for this FramesAddDeleteUndoTask</param>
            public FramesAddDeleteUndoTask(Animation animation, FrameAddDeleteOperationType operationType, string description)
            {
                this.animation = animation;
                this.description = description;
                this.operationType = operationType;
                this.frameIndices = new List<int>();
                this.frames = new List<Frame>();
            }

            /// <summary>
            /// Registers a frame that is being deleted on this FramesDeleteUndoTask instance
            /// </summary>
            /// <param name="frame">The frame being deleted</param>
            /// <param name="index">The index to register. In case this is a delete undo operation, this value is not used</param>
            public void RegisterFrame(Frame frame, int index = 0)
            {
                if(frame.Animation == null)
                    frameIndices.Add(index);
                else
                    frameIndices.Add((operationType == FrameAddDeleteOperationType.Delete ? frame.Index : index));

                frames.Add(frame);
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                if (!undone)
                {
                    // Dispose of frames that are not in animations
                    foreach (Frame frame in frames)
                    {
                        if (frame.Animation == null)
                        {
                            frame.Dispose();
                        }
                    }
                }

                frames.Clear();
                frameIndices.Clear();
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                if (operationType == FrameAddDeleteOperationType.Delete)
                {
                    for (int i = frameIndices.Count - 1; i >= 0; i--)
                    {
                        animation.AddFrame(frames[i], frameIndices[i]);
                    }
                }
                else
                {
                    for (int i = frameIndices.Count - 1; i >= 0; i--)
                    {
                        animation.RemoveFrameIndex(frameIndices[i]);
                    }
                }

                undone = true;
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                if (operationType == FrameAddDeleteOperationType.Delete)
                {
                    for(int i = frameIndices.Count - 1; i >= 0; i--)
                    {
                        animation.RemoveFrameIndex(frameIndices[i]);
                    }
                }
                else
                {
                    //for (int i = 0; i < frames.Count; i++)
                    for (int i = frameIndices.Count - 1; i >= 0; i--)
                    {
                        animation.AddFrame(frames[i], frameIndices[i]);
                    }
                }

                undone = false;
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                return description;
            }
        }

        /// <summary>
        /// Implements a frame modify undo task that undoes/redoes the modification of a frame's image
        /// </summary>
        public class FrameEditUndoTask : IUndoTask
        {
            /// <summary>
            /// The frames that were deleted
            /// </summary>
            private Frame frame;

            /// <summary>
            /// The old (undo) bitmap
            /// </summary>
            private Bitmap oldBitmap;

            /// <summary>
            /// The new (redo) bitmap
            /// </summary>
            private Bitmap newBitmap;

            /// <summary>
            /// Initializes a new instance of a FrameEditUndoTask class
            /// </summary>
            /// <param name="frame">The frame to record the changes made to</param>
            /// <param name="oldBitmap">An (optional) starting value for the old bitmap</param>
            public FrameEditUndoTask(Frame frame, Bitmap oldBitmap = null)
            {
                this.frame = frame;
                this.oldBitmap = (oldBitmap == null ? this.frame.GetComposedBitmap() : oldBitmap);
                this.oldBitmap = this.oldBitmap.Clone(new Rectangle(0, 0, this.oldBitmap.Width, this.oldBitmap.Height), this.oldBitmap.PixelFormat);
            }

            /// <summary>
            /// Records the changes made to the frame's bitmap
            /// </summary>
            /// <param name="newBitmap">An (optional) value for the new bitmap</param>
            public void RecordChanges(Bitmap newBitmap = null)
            {
                this.newBitmap = (newBitmap == null ? frame.GetComposedBitmap() : newBitmap);
                this.newBitmap = this.newBitmap.Clone(new Rectangle(0, 0, this.newBitmap.Width, this.newBitmap.Height), this.newBitmap.PixelFormat);
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                newBitmap.Dispose();
                oldBitmap.Dispose();

                frame = null;
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                frame.SetFrameBitmap(oldBitmap.Clone(new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height), oldBitmap.PixelFormat));
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                frame.SetFrameBitmap(newBitmap.Clone(new Rectangle(0, 0, oldBitmap.Width, oldBitmap.Height), oldBitmap.PixelFormat));
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
            private Animation animation;

            /// <summary>
            /// The old (undo) index
            /// </summary>
            private int oldIndex;

            /// <summary>
            /// The new (redo) index
            /// </summary>
            private int newIndex;

            /// <summary>
            /// Initializes a new instance of the FramesReorderUndoTask class
            /// </summary>
            /// <param name="anim">The animation to affect</param>
            /// <param name="oldIndex">The old (undo) index</param>
            /// <param name="newIndex">The new (redo) index</param>
            public FrameReoderUndoTask(Animation anim, int oldIndex, int newIndex)
            {
                this.animation = anim;
                this.oldIndex = oldIndex;
                this.newIndex = newIndex;
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                this.animation = null;
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                animation.SwapFrameIndices(oldIndex, newIndex);
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                animation.SwapFrameIndices(oldIndex, newIndex);
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
            private Animation animation;

            /// <summary>
            /// The list of new frames
            /// </summary>
            private Bitmap[] newFrames;

            /// <summary>
            /// The old size of the animation before resizing
            /// </summary>
            private AnimationResizeSettings oldResizeSettings;

            /// <summary>
            /// The resize settings for the operation
            /// </summary>
            private AnimationResizeSettings newResizeSettings;

            /// <summary>
            /// Initializes a new instance of the FramesModifyUndoTask class
            /// </summary>
            /// <param name="animation">The animation to affect with this FramesModifyUndoTask instance</param>
            /// <param name="oldSize">The old size of the animation before resizing</param>
            /// <param name="resizeSettings">The resize settings for the operation</param>
            public AnimationResizeUndoTask(Animation animation, Size oldSize, AnimationResizeSettings resizeSettings)
            {
                this.animation = animation;
                this.newFrames = animation.Frames.ToBitmapArray(true);
                this.oldResizeSettings = new AnimationResizeSettings() { InterpolationMode = resizeSettings.InterpolationMode, NewWidth = oldSize.Width, NewHeight = oldSize.Height, PerFrameScalingMethod = resizeSettings.PerFrameScalingMethod };
                this.newResizeSettings = resizeSettings;
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                foreach (Bitmap bit in this.newFrames)
                {
                    bit.Dispose();
                }

                this.newFrames = null;
                this.animation = null;
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                animation.Resize(oldResizeSettings);
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                animation.Resize(newResizeSettings);

                // Apply the frame contents now
                for(int i = 0; i < animation.FrameCount; i++)
                {
                    Frame frame = animation[i];

                    frame.SetFrameBitmap((Bitmap)newFrames[i].Clone());
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

        /// <summary>
        /// Implements an animation modify undo task that undoes/redoes changes in the animation properties, including frame bitmap modifications.
        /// Currently, modifications that including and removing several frames in the same operation is a little glitchy, and may not function properly
        /// </summary>
        public class AnimationModifyUndoTask : IUndoTask
        {
            /// <summary>
            /// The animation to affect with this AnimationModifyUndoTask instance
            /// </summary>
            private Animation animation;

            /// <summary>
            /// A copy of the old list of frames
            /// </summary>
            private List<Frame> oldAnimationFrames;

            /// <summary>
            /// A clone of the animation that was made before changes were made
            /// </summary>
            private Animation oldAnimation;

            /// <summary>
            /// The derivated compound task
            /// </summary>
            private GroupUndoTask compoundTask;

            /// <summary>
            /// Initializes a new instance of the FramesModifyUndoTask class
            /// </summary>
            /// <param name="animation">The animation to affect with this FramesModifyUndoTask instance</param>
            public AnimationModifyUndoTask(Animation animation)
            {
                this.animation = animation;
                this.oldAnimationFrames = new List<Frame>(animation.Frames);
                this.oldAnimation = animation.Clone();

                for (int i = 0; i < this.oldAnimation.FrameCount; i++)
                {
                    this.oldAnimation[i].ID = this.animation[i].ID;
                }

                this.compoundTask = new GroupUndoTask(this.GetDescription());
            }

            /// <summary>
            /// Records the changes made in the animation
            /// </summary>
            public void RecordChanges()
            {
                /*
                    Steps necessary to track frame changes:
                    
                    1. Verify the frames that were removed
                    2. Verify the frames that were added
                    3. Verify the frames that were modified
                    4. Verify the frame orders
                    
                    To achieve this:
                    
                    1.1 For each current frame, check against the old animation frames. If it is not present, it is a new frame. Mark it with a FramesAddDeleteUndoTask
                    2.1 For each old frame, check against the current animation frames. If it is not present, it is a deleted frame. Mark it with a FramesAddDeleteUndoTask
                    3.1 For each current frame, check against the old animation frames. If it is present but it's content is different, it has been modified. Mark it with a FrameEditUndoTask
                    4.1 For each frame that was not added or removed, check against the old animation, and note down the new and old frame indices. Mark them down with a FrameReorderUndoTask
                */

                // To track for reordering of frames
                List<Frame> unmodified = new List<Frame>(animation.Frames);
                List<IUndoTask> undoList = new List<IUndoTask>();

                // 1.1 For each current frame, check against the old animation frames. If it is not present, it is a new frame. Mark it with a FramesAddDeleteUndoTask
                for (int i = 0; i < animation.FrameCount; i++)
                {
                    Frame newFrame = animation[i];
                    bool found = false;

                    int j = 0;

                    for (j = 0; j < oldAnimation.FrameCount; j++)
                    {
                        Frame oldFrame = oldAnimation[j];

                        if (newFrame.ID == oldFrame.ID)
                        {
                            found = true;
                            break;
                        }
                    }

                    if(!found)
                    {
                        FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(animation, FrameAddDeleteOperationType.Add, "Frame Added");

                        undoTask.RegisterFrame(newFrame, i);

                        compoundTask.AddTask(undoTask);

                        unmodified.Remove(newFrame);
                    }
                }

                // 2.1 For each old frame, check against the current animation frames. If it is not present, it is a deleted frame. Mark it with a FramesAddDeleteUndoTask
                undoList.Clear();
                for (int i = 0; i < oldAnimationFrames.Count; i++)
                {
                    Frame oldFrame = oldAnimationFrames[i];
                    bool found = false;

                    int j = 0;
                    int jFound = 0;

                    for (j = 0; j < animation.FrameCount; j++)
                    {
                        Frame newFrame = animation[j];

                        if (oldFrame.ID == newFrame.ID)
                        {
                            jFound = j;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        FramesAddDeleteUndoTask undoTask = new FramesAddDeleteUndoTask(animation, FrameAddDeleteOperationType.Delete, "Frame Removed");
                        undoTask.RegisterFrame(oldFrame, i);

                        undoList.Add(undoTask);

                        unmodified.Remove(oldFrame);
                    }
                }
                undoList.Reverse();
                compoundTask.AddTasks(undoList);

                // 3.1 For each current frame, check against the old animation frames. If it is present but it's content is different, it has been modified. Mark it with a FrameEditUndoTask
                for (int i = 0; i < animation.FrameCount; i++)
                {
                    Frame newFrame = animation[i];
                    Frame oldFrame = null;
                    bool found = false;

                    for (int j = 0; j < oldAnimation.FrameCount; j++)
                    {
                        oldFrame = oldAnimation[j];

                        if (newFrame.ID == oldFrame.ID && !newFrame.Equals(oldFrame))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        FrameEditUndoTask undoTask = new FrameEditUndoTask(newFrame, oldFrame.GetComposedBitmap());
                        undoTask.RecordChanges(newFrame.GetComposedBitmap());

                        compoundTask.AddTask(undoTask);
                    }
                }

                // 4.1 For each frame that was not added or removed, check against the old animation, and note down the new and old frame indices. Mark them down with a FrameReorderUndoTask
                // To keep track of dups
                List<int> framesFound = new List<int>();
                for (int i = 0; i < unmodified.Count; i++)
                {
                    Frame newFrame = unmodified[i];
                    Frame oldFrame = null;
                    bool found = false;

                    int j = 0;

                    for (j = 0; j < oldAnimation.FrameCount; j++)
                    {
                        oldFrame = oldAnimation[j];

                        if (newFrame.ID == oldFrame.ID)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found && i != j)
                    {
                        bool dup = false;
                        foreach (int fr in framesFound)
                        {
                            if (fr == j)
                            {
                                dup = true;
                                break;
                            }
                        }

                        if (dup)
                            continue;

                        framesFound.Add(i);

                        FrameReoderUndoTask undoTask = new FrameReoderUndoTask(animation, j, i);
                        compoundTask.AddTask(undoTask);
                    }
                }

                // Register the animation resize operation
                if (oldAnimation.Width != animation.Width || oldAnimation.Height != animation.Height)
                {
                    GroupUndoTask wrapTask = new GroupUndoTask(compoundTask.GetDescription());
                    wrapTask.ReverseOnUndo = true;

                    AnimationResizeSettings settings = new AnimationResizeSettings();
                    settings.NewWidth = animation.Width;
                    settings.NewHeight = animation.Height;
                    settings.InterpolationMode = InterpolationMode.Low;
                    settings.PerFrameScalingMethod = PerFrameScalingMethod.Zoom;

                    AnimationResizeUndoTask undoTask = new AnimationResizeUndoTask(animation, new Size(oldAnimation.Width, oldAnimation.Height), settings);
                    wrapTask.AddTask(compoundTask);
                    wrapTask.AddTask(undoTask);

                    compoundTask = wrapTask;
                }
            }

            /// <summary>
            /// Clears this UndoTask object
            /// </summary>
            public void Clear()
            {
                this.oldAnimationFrames = null;

                if (this.oldAnimation != null)
                {
                    this.oldAnimation.Dispose();
                    this.oldAnimation = null;
                }

                this.compoundTask.Clear();
            }

            /// <summary>
            /// Undoes this task
            /// </summary>
            public void Undo()
            {
                compoundTask.Undo();
            }

            /// <summary>
            /// Redoes this task
            /// </summary>
            public void Redo()
            {
                compoundTask.Redo();
            }

            /// <summary>
            /// Returns a short string description of this UndoTask
            /// </summary>
            /// <returns>A short string description of this UndoTask</returns>
            public string GetDescription()
            {
                return "Frames modify";
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
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;
using Pixelaria.Data.Clipboard;
using Pixelaria.Data.Exports;

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
        /// Event handler for the ClipboardChanged event
        /// </summary>
        private DataClipboard.ClipboardEventHandler clipboardHandler;

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

            this.controller = controller;
            this.currentAnimation = animation;
            this.viewAnimation = currentAnimation.Clone();

            clipboardHandler = new DataClipboard.ClipboardEventHandler(clipboard_ClipboardChanged);
            Clipboard.ClipboardChanged += clipboardHandler;

            RefreshView();

            this.MarkUnmodified();
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
            RefreshToolbar();
            animationPreviewPanel.LoadAnimation(viewAnimation);
        }

        /// <summary>
        /// Refresh the title of this view
        /// </summary>
        private void RefreshTitle()
        {
            this.Text = "Animation [" + currentAnimation.Name + "]" + (modified ? "*" : "");
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

                frameItem.Text = "Frame " + i;
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
        /// Refreshes the state of the toolbar
        /// </summary>
        private void RefreshToolbar()
        {
            
        }

        /// <summary>
        /// Marks the contents of this view as modified by the user
        /// </summary>
        public override void MarkModified()
        {
            base.MarkModified();

            RefreshTitle();
        }

        /// <summary>
        /// Marks the contents of this view as unmodified by the user
        /// </summary>
        public override void MarkUnmodified()
        {
            base.MarkUnmodified();

            RefreshTitle();
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

                controller.UpdatedAnimation(currentAnimation);
                controller.MarkUnsavedChanges(true);
            }

            base.ApplyChanges();

            RefreshTitle();
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

                viewAnimation.Resize(settings);

                MarkModified();

                RefreshView();
            }
        }

        #endregion

        #region Frame Related Methods

        /// <summary>
        /// Deletes the currently selected frames
        /// </summary>
        /// <param name="confirm">Whether to display a confirmation interface before deleting the frames</param>
        private void DeleteSelectedFrames(bool confirm = true)
        {
            if (lv_frames.SelectedItems.Count > 0)
            {
                if (!confirm || MessageBox.Show("Delete selected frames?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    // Delete selected frames
                    foreach (ListViewItem item in lv_frames.SelectedItems)
                    {
                        viewAnimation.RemoveFrame(item.Tag as Frame);
                        (item.Tag as Frame).Dispose();
                    }

                    MarkModified();

                    RefreshView();
                }
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
            DeleteSelectedFrames(false);

            MarkModified();
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

                for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
                {
                    index = Math.Max(index, lv_frames.SelectedIndices[i]);
                }
                
                // Maintain a copy of the list of added frames so the control can select them after
                List<Frame> copiedFrames = new List<Frame>();
                foreach (Frame frame in frameListClip.Frames)
                {
                    Frame newFrame = frame.Clone();
                    copiedFrames.Add(newFrame);
                }

                viewAnimation.AddFrames(copiedFrames, sizeMatching, index);

                MarkModified();

                RefreshView();

                // Select the newly added frames
                foreach (Frame frame in copiedFrames)
                {
                    GetListViewItemForFrame(frame).Selected = true;
                }
            }
            else if (Clipboard.CurrentDataType == ImageStreamClipboardObject.DataType)
            {
                // Recreate the frame from the image stream
                ImageStreamClipboardObject imgStr = Clipboard.GetObject() as ImageStreamClipboardObject;

                Bitmap bitmap = Bitmap.FromStream(imgStr.ImageStream) as Bitmap;

                Frame frame = new Frame(null, bitmap.Width, bitmap.Height, false);

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

                for (int i = 0; i < lv_frames.SelectedIndices.Count; i++)
                {
                    index = Math.Max(index, lv_frames.SelectedIndices[i]);
                }

                viewAnimation.AddFrames(copiedFrames, sizeMatching, index);

                MarkModified();

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

            viewAnimation.CreateFrame(index);

            MarkModified();

            RefreshView();

            lv_frames.Items[(index == -1 ? lv_frames.Items.Count -1 : index)].Selected = true;
        }

        /// <summary>
        /// Adds a new frame at the end of the frame range
        /// </summary>
        private void AddNewFrame()
        {
            viewAnimation.CreateFrame();

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

            // Temprarely disable the panel while showing the form so we don't waste CPU
            animationPreviewPanel.Disable();

            // Get the currently selected frame
            Frame frame = lv_frames.SelectedItems[0].Tag as Frame;

            FrameView frameView = new FrameView(controller, frame);

            frameView.ShowDialog(this);

            // Mark animation as modified if the frame view has modified any frames
            if (frameView.ModifiedFrames)
            {
                MarkModified();
                RefreshView();
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
                    bit = (Bitmap)Image.FromFile(ofd.FileName);

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

                    Frame frame = new Frame(null, bit.Width, bit.Height, false);
                    frame.SetFrameBitmap(bit);
                    frame.UpdateHash();

                    viewAnimation.AddFrames(new Frame[] { frame }, sizeMatching, -1);

                    MarkModified();

                    RefreshView();

                    lv_frames.Items[lv_frames.Items.Count - 1].Selected = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show("There was an error loading the selected image:\n" + e, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Event Handlers

        // 
        // Form Closed event handler
        // 
        private void AnimationView_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Unload the animation preview panel so no loose references remain to the animation that this form was displaying
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

                RefreshFramesView();

                MarkModified();

                eventArgs.Cancel = true;
            }
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
            RefreshClipboardControls();
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
    }
}
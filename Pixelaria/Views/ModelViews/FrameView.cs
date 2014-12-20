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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;
using Pixelaria.Data.Clipboard;

using Pixelaria.Filters;

using Pixelaria.Utils;

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.ColorControls;
using Pixelaria.Views.Controls.PaintTools;
using Pixelaria.Views.Controls.PaintTools.Interfaces;
using Pixelaria.Views.MiscViews;
using Pixelaria.Views.ModelViews.Miscs;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used to edit individual frames
    /// </summary>
    public partial class FrameView : ModifiableContentView
    {
        /// <summary>
        /// The Controller that owns this FrameView instance
        /// </summary>
        private readonly Controller _controller;

        /// <summary>
        /// The frame to edit on this form
        /// </summary>
        private Frame _frameToEdit;

        /// <summary>
        /// The copy of the frame that is actually edited by this form
        /// </summary>
        private Frame _viewFrame;

        /// <summary>
        /// Previous frame index
        /// </summary>
        private int _oldFrameIndex;

        /// <summary>
        /// The current onion skin
        /// </summary>
        private Bitmap _onionSkin;

        /// <summary>
        /// Whether the frame preview is enabled
        /// </summary>
        private bool _framePreviewEnabled;

        /// <summary>
        /// Event handler for a filter item click
        /// </summary>
        private readonly EventHandler _filterClickEventHandler;

        /// <summary>
        /// Event handler for a filter preset item click
        /// </summary>
        private readonly EventHandler _presetClickEventHandler;

        /// <summary>
        /// The first edit color
        /// </summary>
        public static Color FirstColor;

        /// <summary>
        /// The second edit color
        /// </summary>
        public static Color SecondColor;

        /// <summary>
        /// The current compositing mode
        /// </summary>
        public static CompositingMode CurrentCompositingMode;

        /// <summary>
        /// The default brush size for the control
        /// </summary>
        public static int BrushSize;

        /// <summary>
        /// Whether onion skin is currently enabled
        /// </summary>
        public static bool OnionSkinEnabled;

        /// <summary>
        /// The depth of the onion skin, in frames
        /// </summary>
        public static int OnionSkinDepth;

        /// <summary>
        /// The transparency of the onion skin
        /// </summary>
        public static float OnionSkinTransparency;

        /// <summary>
        /// Whether to show the current frame on onion skin mode
        /// </summary>
        public static bool OnionSkinShowCurrentFrame;

        /// <summary>
        /// The mode to use on the onion skin
        /// </summary>
        public static OnionSkinMode OnionSkinMode;

        /// <summary>
        /// The onion skin decorator used to display the onion skin on this frame view
        /// </summary>
        public OnionSkinDecorator OnionSkinDecorator;

        /// <summary>
        /// Gets whether this FrameView has modified any frames while open
        /// </summary>
        public bool ModifiedFrames { get; private set; }

        /// <summary>
        /// Gets the frame currently loaded on this form
        /// </summary>
        public Frame FrameLoaded { get { return _frameToEdit; } }

        /// <summary>
        /// Delegate for the EdirFrameChanged event
        /// </summary>
        /// <param name="sender">The object that fired the event</param>
        /// <param name="args">The arguments for the event</param>
        public delegate void EditFrameChangedEventHandler(object sender, EditFrameChangedEventArgs args);

        /// <summary>
        /// Occurs whenever the current edit frame has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the current edit frame has changed")]
        public event EditFrameChangedEventHandler EditFrameChanged;

        /// <summary>
        /// Static initializer for the FrameView class
        /// </summary>
        static FrameView()
        {
            FirstColor = Color.Black;
            SecondColor = Color.White;
            CurrentCompositingMode = CompositingMode.SourceOver;
            BrushSize = 1;

            OnionSkinEnabled = false;
            OnionSkinDepth = 3;
            OnionSkinShowCurrentFrame = true;
            OnionSkinMode = OnionSkinMode.PreviousAndNextFrames;
            OnionSkinTransparency = 0.25f;
        }

        /// <summary>
        /// Initializes a new instance of the FrameView class
        /// </summary>
        /// <param name="controller">The controller owning this form</param>
        /// <param name="frameToEdit">The frame to edit on this form</param>
        public FrameView(Controller controller, Frame frameToEdit)
        {
            InitializeComponent();

            _oldFrameIndex = frameToEdit.Index;

            _controller = controller;

            _filterClickEventHandler = tsm_filterItem_Click;
            _presetClickEventHandler = tsm_presetItem_Click;

            UpdateFilterList();
            UpdateFilterPresetList();

            // Image editor panel
            iepb_frame.Init();
            iepb_frame.NotifyTo = this;
            iepb_frame.PictureBox.ZoomChanged += PictureBox_ZoomChanged;
            iepb_frame.PictureBox.MouseMove += iepb_frame_MouseMove;
            iepb_frame.PictureBox.MouseLeave += iepb_frame_MouseLeave;
            iepb_frame.PictureBox.MouseEnter += iepb_frame_MouseEnter;
            iepb_frame.UndoSystem.UndoRegistered += UndoSystem_UndoRegistered;
            iepb_frame.UndoSystem.UndoPerformed += UndoSystem_UndoPerformed;
            iepb_frame.UndoSystem.RedoPerformed += UndoSystem_RedoPerformed;

            ChangePaintOperation(new PencilPaintTool(FirstColor, SecondColor, BrushSize));

            iepb_frame.DefaultCompositingMode = CurrentCompositingMode;

            cp_mainColorPicker.FirstColor = FirstColor;
            cp_mainColorPicker.SecondColor = SecondColor;

            rb_fillMode_2.Checked = true;

            // Frame preview
            _framePreviewEnabled = false;
            pnl_framePreview.Visible = _framePreviewEnabled;
            zpb_framePreview.HookToControl(this);

            tsb_onionSkin.Checked = OnionSkinEnabled;
            tsb_osPrevFrames.Checked = OnionSkinMode == OnionSkinMode.PreviousFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;
            tsb_osShowCurrentFrame.Checked = OnionSkinShowCurrentFrame;
            tsb_osNextFrames.Checked = OnionSkinMode == OnionSkinMode.NextFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;

            OnionSkinDecorator = new OnionSkinDecorator(this, iepb_frame.PictureBox)
            {
                OnionSkinDepth = OnionSkinDepth,
                OnionSkinMode = OnionSkinMode,
                OnionSkinShowCurrentFrame = OnionSkinShowCurrentFrame,
                OnionSkinTransparency = OnionSkinTransparency
            };

            iepb_frame.PictureBox.AddDecorator(OnionSkinDecorator);

            if (OnionSkinEnabled)
            {
                ShowOnionSkin();
            }

            if (CurrentCompositingMode == CompositingMode.SourceOver)
            {
                rb_blendingBlend.Checked = true;
            }
            else
            {
                rb_blendingReplace.Checked = true;
            }

            UpdateMouseLocationLabel();

            LoadFrame(frameToEdit);
        }

        /// <summary>
        /// Marks this FrameView form as modified
        /// </summary>
        public override void MarkModified()
        {
            base.MarkModified();

            // Update the image preview if enabled
            if (_framePreviewEnabled)
            {
                zpb_framePreview.Image = _viewFrame.GetComposedBitmap();
            }

            RefreshTitleBar();
        }

        /// <summary>
        /// Called to apply the changes made on this view
        /// </summary>
        public override void ApplyChanges()
        {
            if (modified)
            {
                // Update selection paint operations
                var operation = iepb_frame.CurrentPaintTool as SelectionPaintTool;
                if (operation != null && operation.SelectionBitmap != null)
                {
                    operation.FinishOperation(true);
                }

                ModifiedFrames = true;

                _viewFrame.UpdateHash();

                // Apply changes made to the frame
                _frameToEdit.CopyFrom(_viewFrame);

                RefreshTitleBar();
            }

            base.ApplyChanges();
        }

        /// <summary>
        /// Refreshes the content of this form
        /// </summary>
        private void RefreshView()
        {
            // Update the enabled state of the Previous Frame and Next Frame buttons
            tsm_prevFrame.Enabled = tsb_prevFrame.Enabled = _frameToEdit.Index > 0;
            tsm_nextFrame.Enabled = tsb_nextFrame.Enabled = _frameToEdit.Index < _frameToEdit.Animation.FrameCount - 1;

            // Update the frame display
            tc_currentFrame.Minimum = 1;
            tc_currentFrame.Maximum = _frameToEdit.Animation.FrameCount;
            tc_currentFrame.CurrentFrame = (_frameToEdit.Index + 1);

            // Refresh the undo and redo buttons
            RefreshUndoRedo();
        }

        /// <summary>
        /// Refreshes the undo/redo portion of the interface
        /// </summary>
        private void RefreshUndoRedo()
        {
            tsm_undo.Enabled = tsb_undo.Enabled = iepb_frame.UndoSystem.CanUndo;
            tsm_redo.Enabled = tsb_redo.Enabled = iepb_frame.UndoSystem.CanRedo;

            if (tsb_undo.Enabled)
            {
                tsm_undo.Text = tsb_undo.ToolTipText = @"Undo " + iepb_frame.UndoSystem.NextUndo.GetDescription();
            }
            else
            {
                tsb_undo.ToolTipText = "";
                tsm_undo.Text = @"Undo";
            }

            if (tsb_redo.Enabled)
            {
                tsm_redo.Text = tsb_redo.ToolTipText = @"Redo " + iepb_frame.UndoSystem.NextRedo.GetDescription();
            }
            else
            {
                tsb_redo.ToolTipText = "";
                tsm_redo.Text = @"Redo";
            }
        }

        /// <summary>
        /// Refreshes the form's title bar
        /// </summary>
        private void RefreshTitleBar()
        {
            Text = @"Frame Editor [" + (_frameToEdit.Index + 1) + @"/" + _frameToEdit.Animation.FrameCount + @"] - [" + _frameToEdit.Animation.Name + @"]" + (modified ? "*" : "");
        }

        /// <summary>
        /// Changes the paint operation with the given one
        /// </summary>
        /// <param name="paintTool">The new paint operation to replace the current one</param>
        private void ChangePaintOperation(IPaintTool paintTool)
        {
            iepb_frame.CurrentPaintTool = paintTool;

            gb_sizeGroup.Visible = paintTool is ISizedPaintTool;
            gb_fillMode.Visible = paintTool is IFillModePaintTool;
        }

        /// <summary>
        /// Loads the given frame to be edited on this FrameView form
        /// </summary>
        /// <param name="frame">The frame to edit on this form</param>
        private void LoadFrame(Frame frame)
        {
            // Dispose of the current view frame
            if (_viewFrame != null)
            {
                _viewFrame.Dispose();
            }

            if (_onionSkin != null)
            {
                _onionSkin.Dispose();
            }

            _frameToEdit = frame;
            _viewFrame = _frameToEdit.Clone();

            RefreshTitleBar();

            iepb_frame.LoadBitmap(_viewFrame.GetComposedBitmap());

            RefreshView();

            // Update the preview box if enabled
            if (_framePreviewEnabled)
            {
                zpb_framePreview.Image = _viewFrame.GetComposedBitmap();
            }

            if (EditFrameChanged != null)
            {
                EditFrameChanged.Invoke(this, new EditFrameChangedEventArgs(_oldFrameIndex, frame.Index));
            }

            _oldFrameIndex = frame.Index;
        }

        /// <summary>
        /// Opens an interface where the user can export the current frame to an image
        /// </summary>
        private void ExportFrame()
        {
            Image img = _viewFrame.GetComposedBitmap();
            string fileName;

            if (_frameToEdit.Animation.FrameCount > 1)
            {
                fileName = _frameToEdit.Animation.Name + "_" + _frameToEdit.Index;
            }
            else
            {
                fileName = _frameToEdit.Animation.Name;
            }

            _controller.ShowSaveImage(img, fileName, this);
        }

        /// <summary>
        /// Opens an interface where the user can import a frame form an image
        /// </summary>
        private void ImportFrame()
        {
            Image img = _controller.ShowLoadImage("", this);

            if (img == null)
                return;

            if (img.Width > _viewFrame.Width || img.Height > _viewFrame.Height)
            {
                FramesRescaleSettingsView frs = new FramesRescaleSettingsView("The selected image is larger than the current image. Please select the scaling mode to apply to the new image:", FramesRescalingOptions.ShowFrameScale | FramesRescalingOptions.ShowDrawingMode);

                if (frs.ShowDialog(this) == DialogResult.OK)
                {
                    FrameSizeMatchingSettings settings = frs.GeneratedSettings;

                    img = ImageUtilities.Resize(img, _viewFrame.Width, _viewFrame.Height, settings.PerFrameScalingMethod, settings.InterpolationMode);
                }
            }

            ClearFrame();

            if(!(iepb_frame.CurrentPaintTool is SelectionPaintTool))
                ChangePaintOperation(new SelectionPaintTool());

            ((SelectionPaintTool)iepb_frame.CurrentPaintTool).CancelOperation(false);
            ((SelectionPaintTool)iepb_frame.CurrentPaintTool).StartOperation(new Rectangle(0, 0, img.Width, img.Height), (Bitmap)img, SelectionPaintTool.SelectionOperationType.Paste);
        }

        /// <summary>
        /// Moves to the previous frame
        /// </summary>
        /// <returns>Whether the frame was sucessfully retroceeded</returns>
        private void PrevFrame()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                LoadFrame(_frameToEdit.Animation.Frames[_frameToEdit.Index - 1]);
            }
        }

        /// <summary>
        /// Moves to the next frame
        /// </summary>
        /// <returns>Whether the frame was successfully advanced</returns>
        private void NextFrame()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                LoadFrame(_frameToEdit.Animation.Frames[_frameToEdit.Index + 1]);
            }
        }

        /// <summary>
        /// Moves to the given frame
        /// </summary>
        /// <param name="index">The frame to move the edit window to</param>
        /// <returns>Whether the frame view sucessfully selected the provided frame</returns>
        private void SetFrameIndex(int index)
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                LoadFrame(_frameToEdit.Animation.Frames[index]);
            }
        }

        /// <summary>
        /// Inserts a new frame after the currently frame being edited and loads it for editing
        /// </summary>
        private void InsertNewFrame()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                Frame frame = _controller.FrameFactory.CloneFrame(_frameToEdit);

                _frameToEdit.Animation.AddFrame(frame, _frameToEdit.Index + 1);

                LoadFrame(_frameToEdit.Animation[_frameToEdit.Index + 1]);

                ModifiedFrames = true;
            }
        }

        /// <summary>
        /// Adds a new frame at the end of the animation and loads it for editing
        /// </summary>
        private void AddFrameAtEnd()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                Frame frame = _controller.FrameFactory.CloneFrame(_frameToEdit);

                _frameToEdit.Animation.AddFrame(frame);

                LoadFrame(_frameToEdit.Animation[_frameToEdit.Animation.FrameCount - 1]);

                ModifiedFrames = true;
            }
        }

        /// <summary>
        /// Toggles the enabled/disabled state of the onion skin
        /// </summary>
        private void ToggleOnionSkin()
        {
            if (OnionSkinEnabled)
            {
                HideOnionSkin();
            }
            else
            {
                ShowOnionSkin();
            }
        }

        /// <summary>
        /// Toggles the enabled/disables state of the current frame on onion skin mode
        /// </summary>
        private void ToggleCurrentFrameOnOnionSkin()
        {
            OnionSkinShowCurrentFrame = !OnionSkinShowCurrentFrame;

            iepb_frame.PictureBox.DisplayImage = OnionSkinShowCurrentFrame;
        }

        /// <summary>
        /// Shows the onion skin for the current frame
        /// </summary>
        private void ShowOnionSkin()
        {
            OnionSkinEnabled = true;

            // Update the toolbar
            tsb_osPrevFrames.Checked = OnionSkinMode == OnionSkinMode.PreviousFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;
            tsb_osNextFrames.Checked = OnionSkinMode == OnionSkinMode.NextFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;

            if (tscb_osFrameCount.SelectedIndex != OnionSkinDepth - 1)
            {
                _ignoreOnionSkinDepthComboboxEvent = true;
                tscb_osFrameCount.SelectedIndex = OnionSkinDepth - 1;
                _ignoreOnionSkinDepthComboboxEvent = false;
            }

            if (!tsl_onionSkinDepth.Visible)
                tsl_onionSkinDepth.Visible = tscb_osFrameCount.Visible = tsb_osPrevFrames.Visible = tsb_osShowCurrentFrame.Visible = tsb_osNextFrames.Visible = true;

            OnionSkinDecorator.ShowOnionSkin();
        }

        /// <summary>
        /// Hides the onion skin for the current frame
        /// </summary>
        private void HideOnionSkin()
        {
            OnionSkinEnabled = false;

            OnionSkinDecorator.HideOnionSkin();

            if (tsl_onionSkinDepth.Visible)
                tsl_onionSkinDepth.Visible = tscb_osFrameCount.Visible = tsb_osPrevFrames.Visible = tsb_osShowCurrentFrame.Visible = tsb_osNextFrames.Visible = false;
        }

        /// <summary>
        /// Clears the frame. This method also registers an undo task for the clearing process
        /// </summary>
        /// <param name="registerUndo">Whether to register an undo for the clear operation</param>
        private void ClearFrame(bool registerUndo = true)
        {
            BitmapUndoTask bud = null;

            if (registerUndo)
            {
                bud = new BitmapUndoTask(iepb_frame.PictureBox, iepb_frame.PictureBox.Bitmap, "Clear");
            }

            FastBitmap.ClearBitmap(iepb_frame.PictureBox.Bitmap, 0);

            if (registerUndo)
            {
                bud.SetNewBitmap(iepb_frame.PictureBox.Bitmap);
            }

            iepb_frame.PictureBox.Invalidate();

            if (registerUndo)
            {
                iepb_frame.UndoSystem.RegisterUndo(bud);
            }

            MarkModified();
        }

        /// <summary>
        /// Undoes a task
        /// </summary>
        private void Undo()
        {
            iepb_frame.UndoSystem.Undo();
        }

        /// <summary>
        /// Redoes a task
        /// </summary>
        private void Redo()
        {
            iepb_frame.UndoSystem.Redo();
        }

        /// <summary>
        /// Copies a content to the clipboard
        /// </summary>
        private void Copy()
        {
            var clipboardPaintOperation = iepb_frame.CurrentPaintTool as IClipboardPaintTool;

            if (clipboardPaintOperation != null)
            {
                ActiveControl = iepb_frame.PictureBox;
                clipboardPaintOperation.Copy();
            }
        }

        /// <summary>
        /// Cuts a content to the clipboard
        /// </summary>
        private void Cut()
        {
            var clipboardPaintOperation = iepb_frame.CurrentPaintTool as IClipboardPaintTool;

            if (clipboardPaintOperation != null)
            {
                ActiveControl = iepb_frame.PictureBox;
                clipboardPaintOperation.Cut();
            }
        }

        /// <summary>
        /// Pastes content from the clipboard
        /// </summary>
        private void Paste()
        {
            if (!Clipboard.ContainsData("PNG") && !Clipboard.ContainsImage())
                return;

            if (!(iepb_frame.CurrentPaintTool is IClipboardPaintTool))
            {
                rb_selection.Checked = true;
            }

            var clipboardPaintOperation = iepb_frame.CurrentPaintTool as IClipboardPaintTool;

            if (clipboardPaintOperation != null)
            {
                ActiveControl = iepb_frame.PictureBox;
                clipboardPaintOperation.Paste();
            }

            iepb_frame.PictureBox.Invalidate();
        }

        /// <summary>
        /// Selects the whole image area on this FrameView
        /// </summary>
        private void SelectAll()
        {
            if (!(iepb_frame.CurrentPaintTool is SelectionPaintTool))
            {
                rb_selection.Checked = true;
            }

            var selectionPaintOperation = iepb_frame.CurrentPaintTool as SelectionPaintTool;

            if (selectionPaintOperation != null)
                selectionPaintOperation.SelectAll();

            // Select the picture box so it receives keyboard input
            var findForm = FindForm();

            if (findForm != null)
                findForm.ActiveControl = iepb_frame.PictureBox;
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
            Bitmap filterTarget;

            BitmapUndoTask but = null;

            var undoTarget = filterTarget = _viewFrame.GetComposedBitmap();

            // Apply the filter to a selection
            var operation = iepb_frame.CurrentPaintTool as SelectionPaintTool;
            if (operation != null && operation.SelectionBitmap != null)
            {
                SelectionPaintTool op = operation;

                if (op.OperationType == SelectionPaintTool.SelectionOperationType.Moved)
                {
                    Rectangle area = op.SelectionArea;
                    Rectangle startArea = op.SelectionStartArea;
                    
                    op.CancelOperation(true, false);

                    but = new BitmapUndoTask(iepb_frame.PictureBox, undoTarget, "Filter");

                    op.StartOperation(startArea, SelectionPaintTool.SelectionOperationType.Moved);
                    op.SelectionArea = area;
                }
                else if (op.OperationType == SelectionPaintTool.SelectionOperationType.Paste)
                {
                    but = new BitmapUndoTask(iepb_frame.PictureBox, undoTarget, "Filter");
                }

                filterTarget = op.SelectionBitmap;
            }
            else
            {
                but = new BitmapUndoTask(iepb_frame.PictureBox, undoTarget, "Filter");
            }

            ImageFilterView bfv = new ImageFilterView(filterPreset, filterTarget);

            if (bfv.ShowDialog(this) == DialogResult.OK)
            {
                if (bfv.ChangesDetected())
                {
                    bool registerUndo = true;

                    iepb_frame.PictureBox.Invalidate();
                    MarkModified();

                    var paintOperation = iepb_frame.CurrentPaintTool as SelectionPaintTool;
                    if (paintOperation != null && paintOperation.SelectionBitmap != null)
                    {
                        SelectionPaintTool op = paintOperation;

                        switch (op.OperationType)
                        {
                            case SelectionPaintTool.SelectionOperationType.Moved:
                                Rectangle area = op.SelectionArea;
                                Rectangle startArea = op.SelectionStartArea;

                                op.CancelOperation(true, false);

                                if (but != null)
                                    but.SetNewBitmap(undoTarget);

                                op.StartOperation(startArea, SelectionPaintTool.SelectionOperationType.Moved);
                                op.SelectionArea = area;
                                break;
                            case SelectionPaintTool.SelectionOperationType.Paste:
                                registerUndo = false;
                                break;
                        }

                        op.ForceApplyChanges = true;
                    }
                    else
                    {
                        if (but != null)
                            but.SetNewBitmap(undoTarget);
                    }

                    if (registerUndo)
                        iepb_frame.UndoSystem.RegisterUndo(but);
                }
            }
            else
            {
                if (but != null)
                    but.Clear();
            }

            UpdateFilterPresetList();
        }

        /// <summary>
        /// Update the toolstrip status label that represents the position the mouse is currently at
        /// </summary>
        private void UpdateMouseLocationLabel()
        {
            Point mouseP = iepb_frame.PictureBox.PointToClient(MousePosition);

            if (iepb_frame.PictureBox.MouseOverImage && iepb_frame.PictureBox.ClientRectangle.Contains(mouseP))
            {
                if (!tsl_coordinates.Visible)
                    tsl_coordinates.Visible = true;

                tsl_coordinates.Text = (iepb_frame.PictureBox.MousePoint.X + 1) + @" x " + (iepb_frame.PictureBox.MousePoint.Y + 1);
            }
            else
            {
                if (tsl_coordinates.Visible)
                    tsl_coordinates.Visible = false;
            }
        }

        #region Event Handlers

        #region Undo System

        // 
        // ImageEditPanel Undo System undo registered event handler
        // 
        private void UndoSystem_UndoRegistered(object sender, Data.Undo.UndoEventArgs e)
        {
            RefreshUndoRedo();
        }

        // 
        // ImageEditPanel Undo System undo performed event handler
        // 
        private void UndoSystem_UndoPerformed(object sender, Data.Undo.UndoEventArgs e)
        {
            RefreshUndoRedo();

            MarkModified();
        }

        // 
        // ImageEditPanel Undo System redo performed event handler
        // 
        private void UndoSystem_RedoPerformed(object sender, Data.Undo.UndoEventArgs e)
        {
            RefreshUndoRedo();

            MarkModified();
        }

        #endregion

        #region Drawing Tools Menu

        // 
        // Pencil tool button click
        // 
        private void rb_pencil_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_pencil.Checked)
            {
                ChangePaintOperation(new PencilPaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor, BrushSize));
            }
        }

        // 
        // Eraser tool button click
        // 
        private void rb_eraser_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_eraser.Checked)
            {
                ChangePaintOperation(new EraserPaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor, BrushSize));
            }
        }

        // 
        // Picket tool button click
        // 
        private void rb_picker_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_picker.Checked)
            {
                ChangePaintOperation(new PickerPaintTool());
            }
        }

        // 
        // Spray Paint tool button click
        // 
        private void rb_sprayPaint_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_sprayPaint.Checked)
            {
                ChangePaintOperation(new SprayPaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor, BrushSize));
            }
        }

        // 
        // Line tool button click
        // 
        private void rb_line_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_line.Checked)
            {
                ChangePaintOperation(new LinePaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
            }
        }

        // 
        // Rectangle tool button click
        // 
        private void rb_rectangle_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_rectangle.Checked)
            {
                ChangePaintOperation(new RectanglePaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
            }
        }

        // 
        // Circle tool button click
        // 
        private void rb_circle_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_circle.Checked)
            {
                ChangePaintOperation(new EllipsePaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
            }
        }

        // 
        // Bucket tool button click
        // 
        private void rb_bucket_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_bucket.Checked)
            {
                ChangePaintOperation(new BucketPaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
            }
        }

        // 
        // Selection tool button click
        // 
        private void rb_selection_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_selection.Checked)
            {
                ChangePaintOperation(new SelectionPaintTool());
            }
        }

        // 
        // Zoom tool button click
        // 
        private void rb_zoom_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_zoom.Enabled && rb_zoom.Checked)
            {
                ChangePaintOperation(new ZoomPaintTool());
            }
        }

        // 
        // Blend Blending Mode radio box check
        // 
        private void rb_blendingBlend_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_blendingBlend.Checked)
            {
                iepb_frame.DefaultCompositingMode = CurrentCompositingMode = CompositingMode.SourceOver;
            }
        }

        // 
        // Replace Blending Mode radio box check
        // 
        private void rb_blendingReplace_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_blendingReplace.Checked)
            {
                iepb_frame.DefaultCompositingMode = CurrentCompositingMode = CompositingMode.SourceCopy;
            }
        }

        #region Brush Size Control

        // 
        // Brush Size NUD value changed
        // 
        private void anud_brushSize_ValueChanged(object sender, EventArgs e)
        {
            BrushSize = (int)anud_brushSize.Value;

            var operation = iepb_frame.CurrentPaintTool as ISizedPaintTool;
            if (operation != null)
            {
                operation.Size = BrushSize;
            }
        }

        // 
        // Brush Size 1 button click
        // 
        private void btn_brushSize_1_Click(object sender, EventArgs e)
        {
            anud_brushSize.Value = 1;
        }
        // 
        // Brush Size 2 button click
        // 
        private void btn_brushSize_2_Click(object sender, EventArgs e)
        {
            anud_brushSize.Value = 2;
        }
        // 
        // Brush Size 3 button click
        // 
        private void btn_brushSize_3_Click(object sender, EventArgs e)
        {
            anud_brushSize.Value = 3;
        }
        // 
        // Brush Size 4 button click
        // 
        private void btn_brushSize_4_Click(object sender, EventArgs e)
        {
            anud_brushSize.Value = 4;
        }
        // 
        // Brush Size 5 button click
        // 
        private void btn_brushSize_5_Click(object sender, EventArgs e)
        {
            anud_brushSize.Value = 5;
        }
        // 
        // Brush Size 6 button click
        // 
        private void btn_brushSize_6_Click(object sender, EventArgs e)
        {
            anud_brushSize.Value = 6;
        }

        #endregion

        #region Fill Mode Control

        // 
        // Fill Mode Outline radio box check
        // 
        private void rb_fillMode_1_CheckedChanged(object sender, EventArgs e)
        {
            iepb_frame.DefaultFillMode = OperationFillMode.OutlineFirstColor;
        }
        // 
        // Fill Mode Outline And Fill radio box check
        // 
        private void rb_fillMode_2_CheckedChanged(object sender, EventArgs e)
        {
            iepb_frame.DefaultFillMode = OperationFillMode.OutlineFirstColorFillSecondColor;
        }
        // 
        // Fill Mode Fill radio box check
        // 
        private void rb_fillMode_3_CheckedChanged(object sender, EventArgs e)
        {
            iepb_frame.DefaultFillMode = OperationFillMode.SolidFillFirstColor;
        }

        #endregion

        #endregion

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
            DisplayFilterPreset(new FilterPreset("New Preset", new[] { FilterStore.Instance.CreateFilter(((ToolStripMenuItem)sender).Tag as string) }));
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
        private void FrameView_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Toggle the display of the image back before closing the form
            if (!OnionSkinShowCurrentFrame)
            {
                ToggleCurrentFrameOnOnionSkin();
            }

            // Dispose of the image edit panel
            iepb_frame.Dispose();

            // Dispose of the view frame
            _viewFrame.Dispose();

            // Dispose of the onion skin
            if (_onionSkin != null)
            {
                _onionSkin.Dispose();
                _onionSkin = null;
            }

            // Run garbage collector now
            GC.Collect();
        }

        // 
        // Form Key Down event handler
        // 
        private void FrameView_KeyDown(object sender, KeyEventArgs e)
        {
            // Switch the tool
            switch (e.KeyCode)
            {
                // Pencil
                case Keys.D:
                    rb_pencil.Checked = true;
                    break;
                // Eraser
                case Keys.E:
                    rb_eraser.Checked = true;
                    break;
                // Color Picker
                case Keys.C:
                    rb_picker.Checked = true;
                    break;
                // Line
                case Keys.V:
                    rb_line.Checked = true;
                    break;
                // Rectangle
                case Keys.R:
                    rb_rectangle.Checked = true;
                    break;
                // Ellipse
                case Keys.Q:
                    rb_circle.Checked = true;
                    break;
                // Bucket Fill
                case Keys.F:
                    rb_bucket.Checked = true;
                    break;
                // Selection
                case Keys.S:
                    rb_selection.Checked = true;
                    break;
                // Zoom
                case Keys.Z:
                    rb_zoom.Checked = true;
                    break;
            }
        }

        // 
        // Main Color Picker color pick event handler
        // 
        private void cp_mainColorPicker_ColorPick(object sender, ColorPickEventArgs eventArgs)
        {
            var operation = iepb_frame.CurrentPaintTool as IColoredPaintTool;
            if (operation != null)
            {
                switch (eventArgs.TargetColor)
                {
                    // 
                    case ColorPickerColor.FirstColor:
                        FirstColor = eventArgs.NewColor;
                        operation.FirstColor = eventArgs.NewColor;
                        break;
                    // 
                    case ColorPickerColor.SecondColor:
                        SecondColor = eventArgs.NewColor;
                        operation.SecondColor = eventArgs.NewColor;
                        break;
                }
            }
        }

        // 
        // Color Swatch color select event handler
        // 
        private void cs_colorSwatch_ColorSelect(object sender, ColorSelectEventArgs eventArgs)
        {
            cp_mainColorPicker.SetCurrentColor(eventArgs.Color);
        }

        // 
        // Image Edit Panel color select event handler
        // 
        private void iepb_frame_ColorSelect(object sender, ColorPickEventArgs eventArgs)
        {
            if (eventArgs.TargetColor == ColorPickerColor.CurrentColor)
            {
                if (cp_mainColorPicker.SelectedColor == ColorPickerColor.FirstColor)
                {
                    FirstColor = eventArgs.NewColor;
                }
                else
                {
                    SecondColor = eventArgs.NewColor;
                }
                cp_mainColorPicker.SetCurrentColor(eventArgs.NewColor);
            }
            else if (eventArgs.TargetColor == ColorPickerColor.FirstColor)
            {
                FirstColor = eventArgs.NewColor;
                cp_mainColorPicker.FirstColor = eventArgs.NewColor;
            }
            else if (eventArgs.TargetColor == ColorPickerColor.SecondColor)
            {
                SecondColor = eventArgs.NewColor;
                cp_mainColorPicker.SecondColor = eventArgs.NewColor;
            }
        }

        // 
        // Image Edit Panel clipboard state event handler
        // 
        private void iepb_frame_ClipboardStateChanged(object sender, ClipboardStateEventArgs eventArgs)
        {
            tsm_copy.Enabled  = tsb_copy.Enabled  = eventArgs.CanCopy;
            tsm_cut.Enabled   = tsb_cut.Enabled = eventArgs.CanCut;
            tsm_paste.Enabled = tsb_paste.Enabled = eventArgs.CanPaste || (Clipboard.ContainsData("PNG"));

            if (Clipboard.ContainsData("PNG"))
            {
                AnimationView.Clipboard.SetObject(new ImageStreamClipboardObject(Clipboard.GetData("PNG") as System.IO.Stream));
            }
        }

        // 
        // Image Edit Panel status changed event handler
        // 
        private void iepb_frame_OperationStatusChanged(object sender, OperationStatusEventArgs eventArgs)
        {
            tsl_operationLabel.Text = eventArgs.Status;
        }

        // 
        // Image Editor Panel mouse move event handler
        // 
        private void iepb_frame_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateMouseLocationLabel();
        }

        // 
        // Image Editor Panel mouse enter event handler
        // 
        private void iepb_frame_MouseEnter(object sender, EventArgs e)
        {
            UpdateMouseLocationLabel();
        }

        // 
        // Image Editor Panel mouse leave event handler
        // 
        private void iepb_frame_MouseLeave(object sender, EventArgs e)
        {
            UpdateMouseLocationLabel();
        }

        // 
        // Current Frame timeline control frame changed event handler
        // 
        private void tc_currentFrame_FrameChanged(object sender, FrameChangedEventArgs eventArgs)
        {
            SetFrameIndex(eventArgs.NewFrame - 1);

            eventArgs.Cancel = true;
        }

        // 
        // Image Panel zoom change event
        // 
        private void PictureBox_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            anud_zoom.Value = (decimal)e.NewZoom;
        }

        // 
        // Zoom assisted numeric up down value change
        // 
        private void anud_zoom_ValueChanged(object sender, EventArgs e)
        {
            iepb_frame.PictureBox.Zoom = new PointF((float)anud_zoom.Value, (float)anud_zoom.Value);
        }

        // 
        // Export Frame menu item click
        // 
        private void tsm_exportFrame_Click(object sender, EventArgs e)
        {
            ExportFrame();
        }

        // 
        // Import Frame menu item click
        // 
        private void tsm_importFrame_Click(object sender, EventArgs e)
        {
            ImportFrame();
        }

        // 
        // Switch Blending Mode menu item click
        // 
        private void tsm_switchBlendingMode_Click(object sender, EventArgs e)
        {
            if (iepb_frame.DefaultCompositingMode == CompositingMode.SourceCopy)
            {
                rb_blendingBlend.PerformClick();
            }
            else
            {
                rb_blendingReplace.PerformClick();
            }
        }

        // 
        // Apply Changes And Close button click
        // 
        private void tsb_applyChangesAndClose_Click(object sender, EventArgs e)
        {
            ApplyChangesAndClose();
        }

        // 
        // Apply Changes button click
        // 
        private void tsb_applyChanges_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }

        // 
        // Prev Frame toolbar button click
        // 
        private void tsb_prevFrame_Click(object sender, EventArgs e)
        {
            PrevFrame();
        }
        // 
        // Previous Frame menu item click
        // 
        private void tsm_prevFrame_Click(object sender, EventArgs e)
        {
            PrevFrame();
        }

        // 
        // Next Frame toolbar button click
        // 
        private void tsb_nextFrame_Click(object sender, EventArgs e)
        {
            NextFrame();
        }
        // 
        // Next Frame menu item click
        // 
        private void tsm_nextFrame_Click(object sender, EventArgs e)
        {
            NextFrame();
        }

        // 
        // Insert New Frame toolbar button click
        // 
        private void tsb_insertNewframe_Click(object sender, EventArgs e)
        {
            InsertNewFrame();
        }

        // 
        // Add Frame At End toolbar button click
        // 
        private void tsb_addFrameAtEnd_Click(object sender, EventArgs e)
        {
            AddFrameAtEnd();
        }

        // 
        // Clear Frame toolbar button click
        // 
        private void tsb_clearFrame_Click(object sender, EventArgs e)
        {
            ClearFrame();
        }

        // 
        // Copy toolbar button click
        // 
        private void tsb_copy_Click(object sender, EventArgs e)
        {
            Copy();
        }
        // 
        // Cut toolbar button click
        // 
        private void tsb_cut_Click(object sender, EventArgs e)
        {
            Cut();
        }
        // 
        // Paste toolbar button click
        // 
        private void tsb_paste_Click(object sender, EventArgs e)
        {
            Paste();
        }

        // 
        // Copy menu item click
        // 
        private void tsm_copy_Click(object sender, EventArgs e)
        {
            Copy();
        }
        // 
        // Cut menu item click
        // 
        private void tsm_cut_Click(object sender, EventArgs e)
        {
            Cut();
        }
        // 
        // Paste menu item click
        // 
        private void tsm_paste_Click(object sender, EventArgs e)
        {
            Paste();
        }

        // 
        // Select All menu item click
        // 
        private void tsm_selectAll_Click(object sender, EventArgs e)
        {
            SelectAll();
        }

        // 
        // Undo toolstrip menu item click
        // 
        private void tsm_undo_Click(object sender, EventArgs e)
        {
            Undo();
        }
        // 
        // Undo toolbar button click
        // 
        private void tsb_undo_Click(object sender, EventArgs e)
        {
            Undo();
        }

        // 
        // Redo toolstrip menu item click
        // 
        private void tsm_redo_Click(object sender, EventArgs e)
        {
            Redo();
        }
        // 
        // Redo toolbar button click
        // 
        private void tsb_redo_Click(object sender, EventArgs e)
        {
            Redo();
        }

        // 
        // Enable/Disable Grid toolbar button click
        // 
        private void tsb_grid_Click(object sender, EventArgs e)
        {
            iepb_frame.PictureBox.DisplayGrid = tsb_grid.Checked;
        }

        // 
        // Enable/Disable preview
        // 
        private void tsb_previewFrame_Click(object sender, EventArgs e)
        {
            tsb_previewFrame.Checked = !tsb_previewFrame.Checked;

            pnl_framePreview.Visible = _framePreviewEnabled = tsb_previewFrame.Checked;

            // Update the image preview if enabled
            if (_framePreviewEnabled)
            {
                zpb_framePreview.Image = _viewFrame.GetComposedBitmap();
            }
        }

        // 
        // Enable/Disable Onion Skin
        // 
        private void tsb_onionSkin_Click(object sender, EventArgs e)
        {
            ToggleOnionSkin();
        }

        // 
        // Show Previous Frames On Onion Skin toolbar button click
        // 
        private void tsb_osPrevFrames_Click(object sender, EventArgs e)
        {
            // Update the OnionSkinMode flag
            if (tsb_osPrevFrames.Checked && tsb_osNextFrames.Checked)
            {
                OnionSkinMode = OnionSkinMode.PreviousAndNextFrames;
            }
            else if (tsb_osNextFrames.Checked)
            {
                OnionSkinMode = OnionSkinMode.NextFrames;
            }
            else if (tsb_osPrevFrames.Checked)
            {
                OnionSkinMode = OnionSkinMode.PreviousFrames;
            }
            else
            {
                OnionSkinMode = OnionSkinMode.None;
            }

            ShowOnionSkin();
        }

        // 
        // Show Current Frame On Onion Skin toolbar button click
        // 
        private void tsb_hideCurrentFrame_Click(object sender, EventArgs e)
        {
            ToggleCurrentFrameOnOnionSkin();
        }

        // 
        // Show Next Frames On Onion Skin toolbar button click
        // 
        private void tsb_osNextFrames_Click(object sender, EventArgs e)
        {
            // Update the OnionSkinMode flag
            if (tsb_osPrevFrames.Checked && tsb_osNextFrames.Checked)
            {
                OnionSkinMode = OnionSkinMode.PreviousAndNextFrames;
            }
            else if (tsb_osNextFrames.Checked)
            {
                OnionSkinMode = OnionSkinMode.NextFrames;
            }
            else if (tsb_osPrevFrames.Checked)
            {
                OnionSkinMode = OnionSkinMode.PreviousFrames;
            }
            else
            {
                OnionSkinMode = OnionSkinMode.None;
            }

            ShowOnionSkin();
        }

        // 
        // Onion Skin Depth Combobox selection index changed
        // 
        private void tscb_osFrameCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_ignoreOnionSkinDepthComboboxEvent)
                return;

            var selectedItem = tscb_osFrameCount.SelectedItem as string;
            if (selectedItem != null)
            {
                int depth = int.Parse(selectedItem);

                if (depth != OnionSkinDepth)
                {
                    OnionSkinDepth = depth;

                    ShowOnionSkin();
                }
            }
        }

        /// <summary>
        /// Whether to ignore the tscb_osFrameCount_SelectedIndexChanged event
        /// </summary>
        private bool _ignoreOnionSkinDepthComboboxEvent;

        #endregion
    }

    /// <summary>
    /// Event arguments for a EditFrameChanged event
    /// </summary>
    public class EditFrameChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the old frame index
        /// </summary>
        public int OldFrameIndex { get; private set; }

        /// <summary>
        /// Gets the new frame index
        /// </summary>
        public int NewFrameIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the EditFrameChangedEventArgs class
        /// </summary>
        /// <param name="oldIndex">The old frame index</param>
        /// <param name="newIndex">The new frame index</param>
        public EditFrameChangedEventArgs(int oldIndex, int newIndex)
        {
            OldFrameIndex = oldIndex;
            NewFrameIndex = newIndex;
        }
    }

    /// <summary>
    /// Specifies the mode of an onion skin display
    /// </summary>
    public enum OnionSkinMode
    {
        /// <summary>
        /// Display no frames on the onion skin
        /// </summary>
        None,
        /// <summary>
        /// Displays only the previous frames on the onion skin
        /// </summary>
        PreviousFrames,
        /// <summary>
        /// Displays both the previous and next frames on the onion skin
        /// </summary>
        PreviousAndNextFrames,
        /// <summary>
        /// Displays only the next frames on the onion skin
        /// </summary>
        NextFrames
    }
}
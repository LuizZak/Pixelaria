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

using Pixelaria.Algorithms.PaintOperations.Abstracts;
using Pixelaria.Controllers;
using Pixelaria.Controllers.LayerControlling;
using Pixelaria.Data;
using Pixelaria.Data.Clipboard;
using Pixelaria.Data.Undo;
using Pixelaria.Filters;

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.ColorControls;
using Pixelaria.Views.Controls.PaintTools;
using Pixelaria.Views.Controls.PaintTools.Interfaces;
using Pixelaria.Views.MiscViews;
using Pixelaria.Views.ModelViews.Decorators;

using Pixelaria.Utils;

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
        /// The controller for the layers of the currently active frame
        /// </summary>
        private readonly LayerController _layerController;

        /// <summary>
        /// The undo system for the operations on the frame
        /// </summary>
        private readonly UndoSystem _undoSystem;

        /// <summary>
        /// The object that binds this frame view with the layer controller
        /// </summary>
        private readonly FrameViewLayerControllerBinder _binder;

        /// <summary>
        /// The frame to edit on this form
        /// </summary>
        private Frame _frameToEdit;

        /// <summary>
        /// The copy of the frame that is actually edited by this form
        /// </summary>
        private Frame _viewFrame;

        /// <summary>
        /// The bitmap for the current frame being displayed
        /// </summary>
        private Bitmap _viewFrameBitmap;

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
        /// Whether this form ahs been completely loaded. Mostly used by interface callbacks to know whether to avoid updating any values
        /// </summary>
        private readonly bool _formLoaded;

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
        /// The settings to apply to the onion skin, saved accross form closings
        /// </summary>
        public static OnionSkinSettings GlobalOnionSkinSettings;

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

            GlobalOnionSkinSettings = new OnionSkinSettings
            {
                OnionSkinEnabled = false,
                OnionSkinDepth = 3,
                OnionSkinShowCurrentFrame = true,
                OnionSkinMode = OnionSkinMode.PreviousAndNextFrames,
                OnionSkinTransparency = 0.25f,
                DisplayOnFront = false
            };
        }

        /// <summary>
        /// Initializes a new instance of the FrameView class
        /// </summary>
        /// <param name="controller">The controller owning this form</param>
        /// <param name="frameToEdit">The frame to edit on this form</param>
        public FrameView(Controller controller, Frame frameToEdit)
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.UserPaint, true);

            _oldFrameIndex = frameToEdit.Index;

            _controller = controller;

            _filterClickEventHandler = tsm_filterItem_Click;
            _presetClickEventHandler = tsm_presetItem_Click;

            UpdateFilterList();
            UpdateFilterPresetList();

            // Image editor panel
            _undoSystem = new UndoSystem();

            iepb_frame.UndoSystem = _undoSystem;
            iepb_frame.Init();
            iepb_frame.NotifyTo = this;
            iepb_frame.PictureBox.ZoomChanged += PictureBox_ZoomChanged;
            iepb_frame.PictureBox.MouseMove += iepb_frame_MouseMove;
            iepb_frame.PictureBox.MouseLeave += iepb_frame_MouseLeave;
            iepb_frame.PictureBox.MouseEnter += iepb_frame_MouseEnter;
            iepb_frame.UndoSystem.UndoRegistered += UndoSystem_UndoRegistered;
            iepb_frame.UndoSystem.UndoPerformed += UndoSystem_UndoPerformed;
            iepb_frame.UndoSystem.RedoPerformed += UndoSystem_RedoPerformed;
            iepb_frame.UndoSystem.Cleared += UndoSystem_Cleared;

            // Set the paint operation to the default
            ChangePaintOperation(new PencilPaintTool(FirstColor, SecondColor, BrushSize));
            iepb_frame.DefaultCompositingMode = CurrentCompositingMode;

            cp_mainColorPicker.FirstColor = FirstColor;
            cp_mainColorPicker.SecondColor = SecondColor;

            // Reset the fill mode
            rb_fillMode_2.Checked = true;

            // Update the interface to have its previous value before the form was closed
            anud_brushSize.Value = BrushSize;
            rb_blendingBlend.Checked = CurrentCompositingMode == CompositingMode.SourceOver;
            rb_blendingReplace.Checked = !rb_blendingBlend.Checked;

            // Frame preview
            _framePreviewEnabled = false;
            pnl_framePreview.Visible = _framePreviewEnabled;
            zpb_framePreview.HookToControl(this);

            // Setup the onion skin configuration interface
            tsb_onionSkin.Checked = GlobalOnionSkinSettings.OnionSkinEnabled;
            tsb_osPrevFrames.Checked = GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.PreviousFrames || GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;
            tsb_osShowCurrentFrame.Checked = GlobalOnionSkinSettings.OnionSkinShowCurrentFrame;
            tsb_osNextFrames.Checked = GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.NextFrames || GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;

            // Create the layer controller
            _layerController = new LayerController(null);
            lcp_layers.SetController(_layerController);

            _binder = new FrameViewLayerControllerBinder(this, _layerController);

            // Setup the onion skin decorator
            OnionSkinDecorator = new OnionSkinDecorator(this, iepb_frame.PictureBox)
            {
                Settings =
                {
                    OnionSkinDepth = GlobalOnionSkinSettings.OnionSkinDepth,
                    OnionSkinMode = GlobalOnionSkinSettings.OnionSkinMode,
                    OnionSkinShowCurrentFrame = GlobalOnionSkinSettings.OnionSkinShowCurrentFrame,
                    OnionSkinTransparency = GlobalOnionSkinSettings.OnionSkinTransparency
                }
            };

            iepb_frame.PictureBox.AddDecorator(OnionSkinDecorator);

            if (GlobalOnionSkinSettings.OnionSkinEnabled)
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

            _formLoaded = true;
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
                RefreshFramePreview();
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

                // Apply changes made to the frame
                _frameToEdit.CopyFrom(_viewFrame);

                base.ApplyChanges();

                RefreshTitleBar();
            }
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
        /// Refreshes the frame preview for the form
        /// </summary>
        private void RefreshFramePreview()
        {
            zpb_framePreview.Image = _viewFrame.GetComposedBitmap();
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
            gb_otherGroup.Visible = paintTool is IAirbrushPaintTool;

            if (paintTool is IAirbrushPaintTool)
            {
                (paintTool as IAirbrushPaintTool).AirbrushMode = cb_airbrushMode.Checked;
            }
        }

        /// <summary>
        /// Loads the given frame to be edited on this FrameView form.
        /// The provided frame must be derived from the Frame class, otherwise an exception is thrown
        /// </summary>
        /// <param name="frame">The frame to edit on this form</param>
        /// <exception cref="ArgumentException">The provided frame object is not derived from the Frame class</exception>
        private void LoadFrame(IFrame frame)
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

            if (!(frame is Frame))
            {
                throw new ArgumentException(@"The provided frame object must be derived from the Frame class", "frame");
            }

            _frameToEdit = (Frame)frame;
            _viewFrame = _frameToEdit.Clone();
            _layerController.Frame = _viewFrame;

            RefreshTitleBar();

            if (_viewFrameBitmap != null)
            {
                _viewFrameBitmap.Dispose();
            }

            _viewFrameBitmap = _viewFrame.GetLayerAt(_layerController.ActiveLayerIndex).LayerBitmap;
            iepb_frame.LoadBitmap(_viewFrameBitmap);

            RefreshView();

            // Update the preview box if enabled
            if (_framePreviewEnabled)
            {
                RefreshFramePreview();
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
            string fileName = _frameToEdit.Animation.Name;

            if (_frameToEdit.Animation.FrameCount > 1)
            {
                fileName += "_" + _frameToEdit.Index;
            }

            _controller.ShowSaveImage(_viewFrameBitmap, fileName, this);
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
            if (GlobalOnionSkinSettings.OnionSkinEnabled)
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
            GlobalOnionSkinSettings.OnionSkinShowCurrentFrame = !GlobalOnionSkinSettings.OnionSkinShowCurrentFrame;

            OnionSkinDecorator.Settings = GlobalOnionSkinSettings;
            iepb_frame.PictureBox.Invalidate();
        }

        /// <summary>
        /// Shows the onion skin for the current frame
        /// </summary>
        private void ShowOnionSkin()
        {
            GlobalOnionSkinSettings.OnionSkinEnabled = true;

            // Update the toolbar
            tsb_osPrevFrames.Checked = GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.PreviousFrames || GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;
            tsb_osNextFrames.Checked = GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.NextFrames || GlobalOnionSkinSettings.OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;
            tsb_osDisplayOnFront.Checked = GlobalOnionSkinSettings.DisplayOnFront;

            if (tscb_osFrameCount.SelectedIndex != GlobalOnionSkinSettings.OnionSkinDepth - 1)
            {
                _ignoreOnionSkinDepthComboboxEvent = true;
                tscb_osFrameCount.SelectedIndex = GlobalOnionSkinSettings.OnionSkinDepth - 1;
                _ignoreOnionSkinDepthComboboxEvent = false;
            }

            if (!tsl_onionSkinDepth.Visible)
                tsl_onionSkinDepth.Visible = tscb_osFrameCount.Visible = tsb_osPrevFrames.Visible = tsb_osShowCurrentFrame.Visible = tsb_osDisplayOnFront.Visible = tsb_osNextFrames.Visible = true;

            OnionSkinDecorator.Settings = GlobalOnionSkinSettings;

            OnionSkinDecorator.ShowOnionSkin();
        }

        /// <summary>
        /// Hides the onion skin for the current frame
        /// </summary>
        private void HideOnionSkin()
        {
            GlobalOnionSkinSettings.OnionSkinEnabled = false;

            OnionSkinDecorator.HideOnionSkin();

            if (tsl_onionSkinDepth.Visible)
                tsl_onionSkinDepth.Visible = tscb_osFrameCount.Visible = tsb_osPrevFrames.Visible = tsb_osShowCurrentFrame.Visible = tsb_osDisplayOnFront.Visible = tsb_osNextFrames.Visible = false;
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
                bud = new BitmapUndoTask(iepb_frame.PictureBox.Bitmap, "Clear");
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

            var undoTarget = filterTarget = _viewFrameBitmap;

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

                    but = new BitmapUndoTask(undoTarget, "Filter");

                    op.StartOperation(startArea, SelectionPaintTool.SelectionOperationType.Moved);
                    op.SelectionArea = area;
                }
                else if (op.OperationType == SelectionPaintTool.SelectionOperationType.Paste)
                {
                    but = new BitmapUndoTask(undoTarget, "Filter");
                }

                filterTarget = op.SelectionBitmap;
            }
            else
            {
                but = new BitmapUndoTask(undoTarget, "Filter");
            }

            // Apply the filter
            ImageFilterView bfv = new ImageFilterView(filterPreset, filterTarget);
            if (bfv.ShowDialog(this) == DialogResult.OK && bfv.ChangesDetected())
            {
                bool registerUndo = true;

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

                // Update the display
                iepb_frame.PictureBox.Invalidate();
                lcp_layers.UpdateLayersDisplay();
                MarkModified();

                if (registerUndo)
                    iepb_frame.UndoSystem.RegisterUndo(but);
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
        // ImageEditPanel Undo System Task Registered event handler
        // 
        private void UndoSystem_UndoRegistered(object sender, UndoEventArgs e)
        {
            RefreshUndoRedo();
        }

        // 
        // ImageEditPanel Undo System Undo Performed event handler
        // 
        private void UndoSystem_UndoPerformed(object sender, UndoEventArgs e)
        {
            RefreshUndoRedo();

            MarkModified();
            iepb_frame.PictureBox.Invalidate();
        }

        // 
        // ImageEditPanel Undo System Redo Performed event handler
        // 
        private void UndoSystem_RedoPerformed(object sender, UndoEventArgs e)
        {
            RefreshUndoRedo();

            MarkModified();
            iepb_frame.PictureBox.Invalidate();
        }

        // 
        // ImageEditPanel Undo System Cleared event handler
        // 
        private void UndoSystem_Cleared(object sender, EventArgs eventArgs)
        {
            RefreshUndoRedo();
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
                ChangePaintOperation(new LinePaintTool(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor, BrushSize));
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
            if (!_formLoaded)
                return;

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
            if (!_formLoaded)
                return;

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
            if (!_formLoaded)
                return;

            BrushSize = (int)anud_brushSize.Value;

            var operation = iepb_frame.CurrentPaintTool as ISizedPaintTool;
            if (operation != null)
            {
                operation.Size = BrushSize;
            }

            // Re-focus the control layer
            ActiveControl = iepb_frame.PictureBox;
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

        #region Other Group Box

        // 
        // Airbrush checkbox check
        // 
        private void cb_enablePencilFlow_CheckedChanged(object sender, EventArgs e)
        {
            var airbrushTool = iepb_frame.CurrentPaintTool as IAirbrushPaintTool;

            if (airbrushTool != null)
            {
                airbrushTool.AirbrushMode = cb_airbrushMode.Checked;
            }
        }

        #endregion

        #endregion

        #region Menu Bar Button Events

        #region File menu

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

        #endregion

        #region Edit menu

        // 
        // Previous Frame menu item click
        // 
        private void tsm_prevFrame_Click(object sender, EventArgs e)
        {
            PrevFrame();
        }
        // 
        // Next Frame menu item click
        // 
        private void tsm_nextFrame_Click(object sender, EventArgs e)
        {
            NextFrame();
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
        // Redo toolstrip menu item click
        // 
        private void tsm_redo_Click(object sender, EventArgs e)
        {
            Redo();
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

        #endregion

        #region Filters menu

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
            var item = sender as ToolStripMenuItem;
            if (item != null && item.Tag is string)
                DisplayFilterPreset(new FilterPreset("New Preset", new[] { FilterStore.Instance.CreateFilter((string)item.Tag) }));
        }

        // 
        // Preset menu item click
        // 
        private void tsm_presetItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item != null && item.Tag is string)
                DisplayFilterPreset(FilterStore.Instance.GetFilterPresetByName((string)item.Tag));
        }

        #endregion

        #endregion

        #region Toolbar Button Events

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
        // Next Frame toolbar button click
        // 
        private void tsb_nextFrame_Click(object sender, EventArgs e)
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
        // Enable/Disable Grid toolbar button click
        // 
        private void tsb_grid_Click(object sender, EventArgs e)
        {
            iepb_frame.PictureBox.DisplayGrid = tsb_grid.Checked;
        }

        // 
        // Enable/Disable toolbar button click
        // 
        private void tsb_previewFrame_Click(object sender, EventArgs e)
        {
            tsb_previewFrame.Checked = !tsb_previewFrame.Checked;

            pnl_framePreview.Visible = _framePreviewEnabled = tsb_previewFrame.Checked;

            // Update the image preview if enabled
            if (_framePreviewEnabled)
            {
                RefreshFramePreview();
            }
        }

        //
        // Preview Animation toolbar button click
        //
        private void tsb_previewAnimation_Click(object sender, EventArgs e)
        {

        }

        // 
        // Enable/Disable Onion Skin toolbar button click
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
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.PreviousAndNextFrames;
            }
            else if (tsb_osNextFrames.Checked)
            {
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.NextFrames;
            }
            else if (tsb_osPrevFrames.Checked)
            {
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.PreviousFrames;
            }
            else
            {
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.None;
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
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.PreviousAndNextFrames;
            }
            else if (tsb_osNextFrames.Checked)
            {
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.NextFrames;
            }
            else if (tsb_osPrevFrames.Checked)
            {
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.PreviousFrames;
            }
            else
            {
                GlobalOnionSkinSettings.OnionSkinMode = OnionSkinMode.None;
            }

            ShowOnionSkin();
        }

        //
        // Display On Front toolbar button click
        //
        private void tsb_osDisplayOnFront_Click(object sender, EventArgs e)
        {
            GlobalOnionSkinSettings.DisplayOnFront = tsb_osDisplayOnFront.Checked;

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
            if (selectedItem == null)
                return;

            int depth = int.Parse(selectedItem);

            if (depth != GlobalOnionSkinSettings.OnionSkinDepth)
            {
                GlobalOnionSkinSettings.OnionSkinDepth = depth;

                ShowOnionSkin();
            }
        }

        #endregion

        // 
        // Form Closed event handler
        // 
        private void FrameView_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Toggle the display of the image back before closing the form
            if (!GlobalOnionSkinSettings.OnionSkinShowCurrentFrame)
            {
                ToggleCurrentFrameOnOnionSkin();
            }

            // Dispose of the image edit panel
            iepb_frame.Dispose();

            // Dispose of the undo system
            _undoSystem.Clear();

            // Dispose of the view frame
            _viewFrame.Dispose();

            // Dispose of the onion skin
            if (_onionSkin != null)
            {
                _onionSkin.Dispose();
                _onionSkin = null;
            }

            // Clear the binder
            _binder.Clear();

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
            if (operation == null)
                return;

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
            tsm_copy.Enabled = tsb_copy.Enabled = eventArgs.CanCopy;
            tsm_cut.Enabled = tsb_cut.Enabled = eventArgs.CanCut;
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

        /// <summary>
        /// Whether to ignore the tscb_osFrameCount_SelectedIndexChanged event
        /// </summary>
        private bool _ignoreOnionSkinDepthComboboxEvent;

        #endregion

        /// <summary>
        /// Class that binds the frame view and layer controller
        /// </summary>
        private class FrameViewLayerControllerBinder
        {
            /// <summary>
            /// The frame view being binded
            /// </summary>
            private readonly FrameView _frameView;

            /// <summary>
            /// The layer controller that is binding to the frame view
            /// </summary>
            private readonly LayerController _layerController;

            /// <summary>
            /// The decorator used to display the frame's layers on the picture box
            /// </summary>
            private readonly LayerDecorator _decorator;

            /// <summary>
            /// Whether to generate undo tasks for operations on the layers 
            /// </summary>
            private bool _generateUndos;

            /// <summary>
            /// Initializes a new instance of the FrameViewLayercontrollerBinder class
            /// </summary>
            /// <param name="frameView">The frame view to bind</param>
            /// <param name="layerController">The layer controller to bind</param>
            public FrameViewLayerControllerBinder(FrameView frameView, LayerController layerController)
            {
                _frameView = frameView;
                _layerController = layerController;
                _generateUndos = true;

                _decorator = new LayerDecorator(_frameView.iepb_frame.PictureBox, layerController);

                _frameView.iepb_frame.PictureBox.AddDecorator(_decorator);

                HookEvents();
            }

            /// <summary>
            /// Clears this binder object
            /// </summary>
            public void Clear()
            {
                UnhookEvents();
            }

            /// <summary>
            /// Hooks the event listeners that are required to bind the frame view and layer controller
            /// </summary>
            private void HookEvents()
            {
                _frameView.lcp_layers.LayerStatusesUpdated += OnLayerStatusesUpdated;

                _frameView.iepb_frame.PictureBox.Modified += PictureBoxOnModified;

                _frameView.iepb_frame.UndoSystem.UndoPerformed += OnUndoTaskPerformed;
                _frameView.iepb_frame.UndoSystem.RedoPerformed += OnUndoTaskPerformed;

                _layerController.ActiveLayerIndexChanged += OnActiveLayerIndexChanged;
                _layerController.FrameChanged += OnFrameChanged;
                _layerController.LayerCreated += OnLayerCreated;
                _layerController.LayerRemoved += OnLayerRemoved;
                _layerController.LayersSwapped += OnLayersSwapped;
            }

            /// <summary>
            /// Unhooks the event listeners that are required to bind the frame view and layer controller
            /// </summary>
            private void UnhookEvents()
            {
                _frameView.lcp_layers.LayerStatusesUpdated -= OnLayerStatusesUpdated;

                _frameView.iepb_frame.PictureBox.Modified -= PictureBoxOnModified;

                _frameView.iepb_frame.UndoSystem.UndoPerformed -= OnUndoTaskPerformed;
                _frameView.iepb_frame.UndoSystem.RedoPerformed -= OnUndoTaskPerformed;

                _layerController.ActiveLayerIndexChanged -= OnActiveLayerIndexChanged;
                _layerController.LayerCreated -= OnLayerCreated;
                _layerController.LayerRemoved -= OnLayerRemoved;
                _layerController.LayersSwapped -= OnLayersSwapped;
            }

            // 
            // Undo/Redo Performed event handler
            // 
            private void OnUndoTaskPerformed(object sender, UndoEventArgs undoEventArgs)
            {
                // Switch layers based on the bitmap that was modified
                BasicPaintOperationUndoTask task = undoEventArgs.Task as BasicPaintOperationUndoTask;
                if (task != null)
                {
                    // Find the layer that the bitmap belongs to
                    foreach (var layer in _layerController.FrameLayers)
                    {
                        if (layer.LayerBitmap == task.TargetBitmap)
                        {
                            _layerController.ActiveLayerIndex = layer.Index;
                            break;
                        }
                    }
                }

                // Update the layer image
                _frameView.lcp_layers.UpdateLayersDisplay();
            }

            #region ImageEditPanel.PictureBox event handlers

            // 
            // Picture Box Modified event handler
            // 
            private void PictureBoxOnModified(object sender, EventArgs eventArgs)
            {
                // Update the layer image
                _decorator.LayerStatuses = _frameView.lcp_layers.LayerStatuses;

                _frameView.lcp_layers.UpdateLayersDisplay();
            }

            #endregion

            #region Layer Control Panel event handlers

            // 
            // Layer Status Update event handler
            // 
            private void OnLayerStatusesUpdated(object sender, EventArgs eventArgs)
            {
                _decorator.LayerStatuses = _frameView.lcp_layers.LayerStatuses;

                // Update editability of the current layer
                var status = _frameView.lcp_layers.LayerStatuses[_layerController.ActiveLayerIndex];
                _frameView.iepb_frame.EditingEnabled = !status.Locked && status.Visible;

                // Invalidate frame to update visibility
                _frameView.iepb_frame.PictureBox.Invalidate();
            }

            #endregion

            #region Layer Controller event handlers

            // 
            // Layers Swapped event handler
            // 
            private void OnLayersSwapped(object sender, LayerControllerLayersSwappedEventArgs args)
            {
                _frameView.MarkModified();

                // Add the undo task
                if (_generateUndos)
                    _frameView._undoSystem.RegisterUndo(new LayerSwappedUndoTask(args.FirstLayerIndex, args.SecondLayerIndex, this));
            }

            // 
            // Layers Swapped event handler
            // 
            private void OnLayerRemoved(object sender, LayerControllerLayerRemovedEventArgs args)
            {
                // Deal with operations going on on the current frame
                var operation = _frameView.iepb_frame.CurrentPaintTool as IAreaOperation;
                if (operation != null)
                {
                    operation.FinishOperation(true);
                }

                // Add the undo task
                if (_generateUndos)
                    _frameView._undoSystem.RegisterUndo(new RemoveLayerUndoTask(args.FrameLayer, this));

                // Check if the active layer has not been modified
                if (!ReferenceEquals(args.FrameLayer, _layerController.ActiveLayer))
                {
                    UpdateEditActiveLayer();
                }

                _frameView.MarkModified();
            }

            // 
            // Layers Swapped event handler
            // 
            private void OnLayerCreated(object sender, LayerControllerLayerCreatedEventArgs args)
            {
                // Update the layer image
                _decorator.LayerStatuses = _frameView.lcp_layers.LayerStatuses;

                // Add the undo task
                if (_generateUndos)
                    _frameView._undoSystem.RegisterUndo(new AddLayerUndoTask(args.FrameLayer, this));

                _frameView.MarkModified();
            }

            // 
            // Frame Changed event handler
            // 
            private void OnFrameChanged(object sender, LayerControllerFrameChangedEventArgs args)
            {
                _decorator.LayerStatuses = _frameView.lcp_layers.LayerStatuses;
            }

            // 
            // Active Layer Index Changed event handler
            // 
            private void OnActiveLayerIndexChanged(object sender, ActiveLayerIndexChangedEventArgs args)
            {
                // Finish any pending operations left
                var operation = _frameView.iepb_frame.CurrentPaintTool as IAreaOperation;
                if (operation != null)
                {
                    operation.FinishOperation(true);
                }

                UpdateEditActiveLayer();
            }

            /// <summary>
            /// Updates the bitmap being currently edited based on the active layer of the layer control
            /// </summary>
            private void UpdateEditActiveLayer()
            {
                // Update the bitmap being edited
                _frameView._viewFrameBitmap = _frameView._viewFrame.GetLayerAt(_layerController.ActiveLayerIndex).LayerBitmap;
                _frameView.iepb_frame.LoadBitmap(_frameView._viewFrameBitmap, false);

                // Update editability of the current layer
                var status = _frameView.lcp_layers.LayerStatuses[_layerController.ActiveLayerIndex];
                _frameView.iepb_frame.EditingEnabled = !status.Locked && status.Visible;
            }

            #endregion

            #region Layer Undo Tasks

            /// <summary>
            /// Represents an interface to be implemented by classes that undo/redo layer management
            /// </summary>
            private interface ILayerUndoTask : IUndoTask
            {
                
            }

            /// <summary>
            /// Represents an undo operation for an Add Layer operation
            /// </summary>
            private class AddLayerUndoTask : ILayerUndoTask
            {
                /// <summary>
                /// The binder controller
                /// </summary>
                private readonly FrameViewLayerControllerBinder _binder;

                /// <summary>
                /// The layer that was added
                /// </summary>
                private readonly IFrameLayer _layer;

                /// <summary>
                /// Whether the task has been undone
                /// </summary>
                private bool _undone;

                /// <summary>
                /// Initializes a new instance of the AddLayerUndoTask class
                /// </summary>
                /// <param name="layer">The layer that was added</param>
                /// <param name="binder">The binder controller</param>
                public AddLayerUndoTask(IFrameLayer layer, FrameViewLayerControllerBinder binder)
                {
                    _layer = layer;
                    _binder = binder;
                }

                /// <summary>
                /// Clears this AddLayerUndoTask
                /// </summary>
                public void Clear()
                {
                    if (_undone)
                    {
                        _layer.Dispose();
                    }
                }

                /// <summary>
                /// Undoes the Add Layer task
                /// </summary>
                public void Undo()
                {
                    _undone = true;

                    // Remove the layer
                    _binder._generateUndos = false;
                    _binder._layerController.RemoveLayer(_layer.Index, false);
                    _binder._generateUndos = true;
                }

                /// <summary>
                /// Redoes the Add Layer task
                /// </summary>
                public void Redo()
                {
                    _undone = false;

                    _binder._generateUndos = false;
                    _binder._layerController.AddLayer(_layer, _layer.Index);
                    _binder._generateUndos = true;
                }
                
                /// <summary>
                /// Returns the description for this undo task
                /// </summary>
                /// <returns>The description for this undo task</returns>
                public string GetDescription()
                {
                    return "Add Layer";
                }
            }

            /// <summary>
            /// Represents an undo operation for a Remove Layer operation
            /// </summary>
            private class RemoveLayerUndoTask : ILayerUndoTask
            {
                /// <summary>
                /// The binder controller
                /// </summary>
                private readonly FrameViewLayerControllerBinder _binder;

                /// <summary>
                /// The layer that was removed
                /// </summary>
                private readonly IFrameLayer _layer;

                /// <summary>
                /// Whether the task has been undone
                /// </summary>
                private bool _undone;

                /// <summary>
                /// Initializes a new instance of the RemoveLayerUndoTask class
                /// </summary>
                /// <param name="layer">The layer that was removed</param>
                /// <param name="binder">The binder controller</param>
                public RemoveLayerUndoTask(IFrameLayer layer, FrameViewLayerControllerBinder binder)
                {
                    _layer = layer;
                    _binder = binder;
                }

                /// <summary>
                /// Clears this RemoveLayerUndoTask
                /// </summary>
                public void Clear()
                {
                    if (_undone)
                    {
                        _layer.Dispose();
                    }
                }

                /// <summary>
                /// Undoes the Remove Layer task
                /// </summary>
                public void Undo()
                {
                    _undone = true;

                    _binder._generateUndos = false;
                    _binder._layerController.AddLayer(_layer, _layer.Index);
                    _binder._generateUndos = true;
                }

                /// <summary>
                /// Redoes the Remove Layer task
                /// </summary>
                public void Redo()
                {
                    _undone = false;

                    // Remove the layer
                    _binder._generateUndos = false;
                    _binder._layerController.RemoveLayer(_layer.Index, false);
                    _binder._generateUndos = true;
                }
                
                /// <summary>
                /// Returns the description for this undo task
                /// </summary>
                /// <returns>The description for this undo task</returns>
                public string GetDescription()
                {
                    return "Remove Layer";
                }
            }

            /// <summary>
            /// Represents an undo operation for a Layer Swap operation
            /// </summary>
            private class LayerSwappedUndoTask : ILayerUndoTask
            {
                /// <summary>
                /// The binder controller
                /// </summary>
                private readonly FrameViewLayerControllerBinder _binder;

                /// <summary>
                /// The first layer that was swapped
                /// </summary>
                private readonly int _firstIndex;

                /// <summary>
                /// The second layer that was swapped
                /// </summary>
                private readonly int _secondIndex;

                /// <summary>
                /// Initializes a new instance of the LayerSwappedUndoTask class
                /// </summary>
                /// <param name="firstIndex">The first layer that was swapped</param>
                /// <param name="secondIndex">The second layer that was swapped</param>
                /// <param name="binder">The binder controller</param>
                public LayerSwappedUndoTask(int firstIndex, int secondIndex, FrameViewLayerControllerBinder binder)
                {
                    _firstIndex = firstIndex;
                    _secondIndex = secondIndex;
                    _binder = binder;
                }

                /// <summary>
                /// Clears this LayerSwappedUndoTask
                /// </summary>
                public void Clear()
                {
                    
                }

                /// <summary>
                /// Undoes the Swap Layer task
                /// </summary>
                public void Undo()
                {
                    _binder._generateUndos = false;
                    _binder._layerController.SwapLayers(_secondIndex, _firstIndex);
                    _binder._generateUndos = true;
                }

                /// <summary>
                /// Redoes the Swap Layer task
                /// </summary>
                public void Redo()
                {
                    // Remove the layer
                    _binder._generateUndos = false;
                    _binder._layerController.SwapLayers(_firstIndex, _secondIndex);
                    _binder._generateUndos = true;
                }
                
                /// <summary>
                /// Returns the description for this undo task
                /// </summary>
                /// <returns>The description for this undo task</returns>
                public string GetDescription()
                {
                    return "Swap Layers";
                }
            }

            #endregion
        }
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
}
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
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;
using Pixelaria.Data.Clipboard;

using Pixelaria.Filters;

using Pixelaria.Utils;

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used to edit individual frames
    /// </summary>
    public partial class FrameView : ModifiableContentView
    {
        /// <summary>
        /// The frame to edit on this form
        /// </summary>
        private Frame frameToEdit;

        /// <summary>
        /// The copy of the frame that is actually edited by this form
        /// </summary>
        private Frame viewFrame;

        /// <summary>
        /// The current onion skin
        /// </summary>
        private Bitmap onionSkin;

        /// <summary>
        /// Whether the frame preview is enabled
        /// </summary>
        private bool framePreviewEnabled;

        /// <summary>
        /// Event handler for a filter item click
        /// </summary>
        private EventHandler filterClickEventHandler;

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
        /// The mode to use on the onion skin
        /// </summary>
        public static OnionSkinMode OnionSkinMode;

        /// <summary>
        /// Gets whether this FrameView has modified any frames while open
        /// </summary>
        public bool ModifiedFrames { get; private set; }

        /// <summary>
        /// Gets the frame currently loaded on this form
        /// </summary>
        public Frame FrameLoaded { get { return frameToEdit; } }

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

            this.filterClickEventHandler = new EventHandler(tsm_filterItem_Click);

            this.UpdateFilterList();

            // Image editor panel
            this.iepb_frame.Init();
            this.iepb_frame.NotifyTo = this;
            this.iepb_frame.PictureBox.ZoomChanged += new ZoomablePictureBox.ZoomChangedEventHandler(PictureBox_ZoomChanged);
            this.iepb_frame.UndoSystem.UndoRegistered += new Data.Undo.UndoSystem.UndoEventHandler(UndoSystem_UndoRegistered);
            this.iepb_frame.UndoSystem.UndoPerformed += new Data.Undo.UndoSystem.UndoEventHandler(UndoSystem_UndoPerformed);
            this.iepb_frame.UndoSystem.RedoPerformed += new Data.Undo.UndoSystem.UndoEventHandler(UndoSystem_RedoPerformed);

            this.ChangePaintOperation(new PencilPaintOperation(FirstColor, SecondColor, BrushSize));

            this.iepb_frame.DefaultCompositingMode = CurrentCompositingMode;

            this.cp_mainColorPicker.FirstColor = FirstColor;
            this.cp_mainColorPicker.SecondColor = SecondColor;

            this.rb_fillMode_2.Checked = true;

            // Frame preview
            this.framePreviewEnabled = false;
            this.pnl_framePreview.Visible = this.framePreviewEnabled;
            this.zpb_framePreview.HookToForm(this);

            this.tsb_onionSkin.Checked = OnionSkinEnabled;
            this.tsb_osPrevFrames.Checked = OnionSkinMode == OnionSkinMode.PreviousFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;
            this.tsb_osNextFrames.Checked = OnionSkinMode == OnionSkinMode.NextFrames || OnionSkinMode == OnionSkinMode.PreviousAndNextFrames;

            if (CurrentCompositingMode == CompositingMode.SourceOver)
            {
                rb_blendingBlend.Checked = true;
            }
            else
            {
                rb_blendingReplace.Checked = true;
            }

            LoadFrame(frameToEdit);
        }

        /// <summary>
        /// Marks this FrameView form as modified
        /// </summary>
        public override void MarkModified()
        {
            base.MarkModified();

            // Update the image preview if enabled
            if (framePreviewEnabled)
            {
                this.zpb_framePreview.Image = viewFrame.GetComposedBitmap();
            }

            this.Text = "Frame Editor [" + (frameToEdit.Index + 1) + "/" + frameToEdit.Animation.FrameCount + "]*";
        }

        /// <summary>
        /// Called to apply the changes made on this view
        /// </summary>
        public override void ApplyChanges()
        {
            if (modified)
            {
                // Update selection paint operations
                if (this.iepb_frame.CurrentPaintOperation is SelectionPaintOperation && (this.iepb_frame.CurrentPaintOperation as SelectionPaintOperation).SelectionBitmap != null)
                {
                    (this.iepb_frame.CurrentPaintOperation as SelectionPaintOperation).FinishOperation(true);
                }

                ModifiedFrames = true;

                viewFrame.UpdateHash();

                // Apply changes made to the frame
                frameToEdit.CopyFrom(viewFrame);

                this.Text = "Frame Editor [" + (frameToEdit.Index + 1) + "/" + frameToEdit.Animation.FrameCount + "]";
            }

            base.ApplyChanges();
        }

        /// <summary>
        /// Refreshes the content of this form
        /// </summary>
        private void RefreshView()
        {
            tsm_prevFrame.Enabled = tsb_prevFrame.Enabled = frameToEdit.Index > 0;
            tsm_nextFrame.Enabled = tsb_nextFrame.Enabled = frameToEdit.Index < frameToEdit.Animation.FrameCount - 1;

            RefreshUndoRedo();
        }

        /// <summary>
        /// Refreshes the undo/redo portion of the interface
        /// </summary>
        private void RefreshUndoRedo()
        {
            this.tsm_undo.Enabled = this.tsb_undo.Enabled = this.iepb_frame.UndoSystem.CanUndo;
            this.tsm_redo.Enabled = this.tsb_redo.Enabled = this.iepb_frame.UndoSystem.CanRedo;

            if (this.tsb_undo.Enabled)
            {
                this.tsm_undo.Text = this.tsb_undo.ToolTipText = "Undo " + this.iepb_frame.UndoSystem.NextUndo.GetDescription();
                
            }
            else
            {
                this.tsb_undo.ToolTipText = "";
                this.tsm_undo.Text = "Undo";
            }

            if (this.tsb_redo.Enabled)
            {
                this.tsm_redo.Text = this.tsb_redo.ToolTipText = "Redo " + this.iepb_frame.UndoSystem.NextRedo.GetDescription();
            }
            else
            {
                this.tsb_redo.ToolTipText = "";
                this.tsm_redo.Text = "Redo";
            }
        }

        /// <summary>
        /// Changes the paint operation with the given one
        /// </summary>
        /// <param name="paintOperation">The new paint operation to replace the current one</param>
        private void ChangePaintOperation(PaintOperation paintOperation)
        {
            this.iepb_frame.CurrentPaintOperation = paintOperation;

            //gb_sizeGroup.Visible = paintOperation is SizedPaintOperation;
            gb_fillMode.Visible = paintOperation is FillModePaintOperation;
        }

        /// <summary>
        /// Loads the given frame to be edited on this FrameView form
        /// </summary>
        /// <param name="frame">The frame to edit on this form</param>
        private void LoadFrame(Frame frame)
        {
            // Dispose of the current view frame
            if (viewFrame != null)
            {
                viewFrame.Dispose();
            }

            if (onionSkin != null)
            {
                onionSkin.Dispose();
            }

            frameToEdit = frame;
            viewFrame = frameToEdit.Clone();

            Text = "Frame Editor [" + (frameToEdit.Index + 1) + "/" + frameToEdit.Animation.FrameCount + "]";

            iepb_frame.LoadBitmap(viewFrame.GetComposedBitmap());

            RefreshView();

            // Update the preview box if enabled
            if (framePreviewEnabled)
            {
                zpb_framePreview.Image = viewFrame.GetComposedBitmap();
            }

            // Update onion skin if enabled
            if (OnionSkinEnabled)
            {
                DestroyOnionSkin();
                ShowOnionSkin();
            }
        }

        /// <summary>
        /// Opens an interface where the user can export the current frame to an image
        /// </summary>
        private void ExportFrame()
        {
            SaveFileDialog sfd = new SaveFileDialog();

            sfd.Filter = "PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|GIF Image (*.gif)|*.gif|JPEG Image (*.jpg)|*.jpg|TIFF Image (*.tiff)|*.tiff";

            if (frameToEdit.Animation.FrameCount > 1)
            {
                sfd.FileName = frameToEdit.Animation.Name + "_" + frameToEdit.Index;
            }
            else
            {
                sfd.FileName = frameToEdit.Animation.Name;
            }

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                string savePath = sfd.FileName;

                viewFrame.GetComposedBitmap().Save(savePath);
            }
        }

        /// <summary>
        /// Opens an interface where the user can import a frame form an image
        /// </summary>
        private void ImportFrame()
        {

        }

        /// <summary>
        /// Moves to the previous frame
        /// </summary>
        private void PrevFrame()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                LoadFrame(frameToEdit.Animation.Frames[frameToEdit.Index - 1]);
            }
        }

        /// <summary>
        /// Moves to the next frame
        /// </summary>
        private void NextFrame()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                LoadFrame(frameToEdit.Animation.Frames[frameToEdit.Index + 1]);
            }
        }

        /// <summary>
        /// Inserts a new frame after the currently frame being edited and loads it for editing
        /// </summary>
        private void InsertNewFrame()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                Frame frame = frameToEdit.Clone();

                frameToEdit.Animation.AddFrame(frame, frameToEdit.Index + 1);

                LoadFrame(frameToEdit.Animation[frameToEdit.Index + 1]);

                this.ModifiedFrames = true;
            }
        }

        /// <summary>
        /// Adds a new frame at the end of the animation and loads it for editing
        /// </summary>
        private void AddFrameAtEnd()
        {
            if (ConfirmChanges() != DialogResult.Cancel)
            {
                Frame frame = frameToEdit.Clone();

                frameToEdit.Animation.AddFrame(frame);

                LoadFrame(frameToEdit.Animation[frameToEdit.Animation.FrameCount - 1]);

                this.ModifiedFrames = true;
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
                ignoreOnionSkinDepthComboboxEvent = true;
                tscb_osFrameCount.SelectedIndex = OnionSkinDepth - 1;
                ignoreOnionSkinDepthComboboxEvent = false;
            }

            if (!tsl_onionSkinDepth.Visible)
                tsl_onionSkinDepth.Visible = tscb_osFrameCount.Visible = tsb_osPrevFrames.Visible = tsb_osNextFrames.Visible = true;

            if (onionSkin != null && (onionSkin.Width != frameToEdit.Width || onionSkin.Height != frameToEdit.Height))
            {
                onionSkin.Dispose();
                onionSkin = null;
            }
            else if(onionSkin != null)
            {
                FastBitmap.ClearBitmap(onionSkin, 0);
            }

            if (onionSkin == null)
            {
                // Create the new onion skin
                onionSkin = new Bitmap(frameToEdit.Width, frameToEdit.Height, PixelFormat.Format32bppArgb);
            }

            Graphics og = Graphics.FromImage(onionSkin);

            og.CompositingMode = CompositingMode.SourceOver;

            Rectangle bounds = new Rectangle(0, 0, frameToEdit.Width, frameToEdit.Height);

            // Create image attributes
            ImageAttributes attributes = new ImageAttributes();

            // Create a color matrix object
            ColorMatrix matrix = new ColorMatrix();

            //float multDecay = 0.3f + (0.7f / OnionSkinDepth);
            float multDecay = 0.5f + (OnionSkinDepth / 50.0f);

            // Draw the previous frames
            if(OnionSkinMode == OnionSkinMode.PreviousFrames || OnionSkinMode == ModelViews.OnionSkinMode.PreviousAndNextFrames)
            {
                int fi = frameToEdit.Index;
                float mult = 1;
                for (int i = fi - 1; i > fi - OnionSkinDepth - 1 && i >= 0; i--)
                {
                    matrix.Matrix33 = OnionSkinTransparency * mult;
                    mult *= multDecay;
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    og.DrawImage(frameToEdit.Animation[i].GetComposedBitmap(), bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            // Draw the next frames
            if (OnionSkinMode == ModelViews.OnionSkinMode.NextFrames || OnionSkinMode == ModelViews.OnionSkinMode.PreviousAndNextFrames)
            {
                int fi = frameToEdit.Index;
                float mult = 1;
                for (int i = fi + 1; i < fi + OnionSkinDepth + 1 && i < frameToEdit.Animation.FrameCount; i++)
                {
                    matrix.Matrix33 = OnionSkinTransparency * mult;
                    mult *= multDecay;
                    attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    og.DrawImage(frameToEdit.Animation[i].GetComposedBitmap(), bounds, 0, 0, bounds.Width, bounds.Height, GraphicsUnit.Pixel, attributes);
                }
            }

            og.Flush();
            og.Dispose();

            iepb_frame.PictureBox.UnderImage = onionSkin;
        }

        /// <summary>
        /// Hides the onion skin for the current frame
        /// </summary>
        private void HideOnionSkin()
        {
            OnionSkinEnabled = false;

            if (tsl_onionSkinDepth.Visible)
                tsl_onionSkinDepth.Visible = tscb_osFrameCount.Visible = tsb_osPrevFrames.Visible = tsb_osNextFrames.Visible = false;

            DestroyOnionSkin();
        }

        /// <summary>
        /// Destroys the current onion skin
        /// </summary>
        private void DestroyOnionSkin()
        {
            iepb_frame.PictureBox.UnderImage = null;

            // Dispose of the onion skin
            if (onionSkin != null)
            {
                onionSkin.Dispose();
                onionSkin = null;
            }
        }

        /// <summary>
        /// Undoes a task
        /// </summary>
        private void Undo()
        {
            this.iepb_frame.UndoSystem.Undo();
        }

        /// <summary>
        /// Redoes a task
        /// </summary>
        private void Redo()
        {
            this.iepb_frame.UndoSystem.Redo();
        }

        /// <summary>
        /// Copies a content to the clipboard
        /// </summary>
        private void Copy()
        {
            (this.iepb_frame.CurrentPaintOperation as ClipboardPaintOperation).Copy();
        }

        /// <summary>
        /// Cuts a content to the clipboard
        /// </summary>
        private void Cut()
        {
            (this.iepb_frame.CurrentPaintOperation as ClipboardPaintOperation).Cut();
        }

        /// <summary>
        /// Pastes content from the clipboard
        /// </summary>
        private void Paste()
        {
            if (!Clipboard.ContainsData("PNG") && !Clipboard.ContainsImage())
                return;

            if (!(this.iepb_frame.CurrentPaintOperation is ClipboardPaintOperation))
            {
                this.rb_selection.Checked = true;
            }

            (this.iepb_frame.CurrentPaintOperation as ClipboardPaintOperation).Paste();

            this.iepb_frame.PictureBox.Invalidate();
        }

        /// <summary>
        /// Selects the whole image area on this FrameView
        /// </summary>
        private void SelectAll()
        {
            if (!(this.iepb_frame.CurrentPaintOperation is SelectionPaintOperation))
            {
                this.rb_selection.Checked = true;
            }

            (this.iepb_frame.CurrentPaintOperation as SelectionPaintOperation).SelectAll();
            // Select the picture box so it receives keyboard input
            this.FindForm().ActiveControl = this.iepb_frame.PictureBox;
        }

        /// <summary>
        /// Updates the list of available filters
        /// </summary>
        private void UpdateFilterList()
        {
            tsm_filters.DropDownItems.Clear();

            // Fetch the list of filters
            string[] filterNames = FilterStore.Instance.FiltersList;
            Image[] iconList = FilterStore.Instance.FilterIconList;

            //foreach (string filter in filterNames)
            for (int i = 0; i < iconList.Length; i++)
            {
                ToolStripMenuItem tsm_filterItem = new ToolStripMenuItem(filterNames[i], iconList[i]);

                tsm_filterItem.Tag = filterNames[i];
                tsm_filterItem.Click += filterClickEventHandler;

                tsm_filters.DropDownItems.Add(tsm_filterItem);
            }
        }

        /// <summary>
        /// Displays a BaseFilterView with the given FilterControls attached
        /// </summary>
        /// <param name="filterControl">The FilterControls to display on the BaseFilterView</param>
        private void DisplayFilterControls(FilterControl[] filterControls)
        {
            Bitmap target = null;

            target = viewFrame.GetComposedBitmap();

            if (iepb_frame.CurrentPaintOperation is SelectionPaintOperation && (iepb_frame.CurrentPaintOperation as SelectionPaintOperation).SelectionBitmap != null)
            {
                //target = (iepb_frame.CurrentPaintOperation as SelectionPaintOperation).SelectionBitmap;
                //(iepb_frame.CurrentPaintOperation as SelectionPaintOperation).ForceApplyChanges = true;
                (iepb_frame.CurrentPaintOperation as SelectionPaintOperation).CancelOperation(true);
            }
            /*else
            {
                target = viewFrame.GetComposedBitmap();
            }*/

            BitmapUndoTask but = new BitmapUndoTask(this.iepb_frame.PictureBox, target, "Filter");

            BaseFilterView bfv = new BaseFilterView(filterControls, target);

            if (bfv.ShowDialog(this) == DialogResult.OK)
            {
                if (bfv.FilterCount > 0)
                {
                    iepb_frame.PictureBox.Invalidate();
                    MarkModified();

                    but.RegisterNewBitmap(target);

                    iepb_frame.UndoSystem.RegisterUndo(but);
                }
            }
            else
            {
                but.Clear();
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
                ChangePaintOperation(new PencilPaintOperation(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor, BrushSize));
            }
        }

        // 
        // Eraser tool button click
        // 
        private void rb_eraser_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new EraserPaintOperation());
        }

        // 
        // Picket tool button click
        // 
        private void rb_picker_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new PickerPaintOperation());
        }

        // 
        // Line tool button click
        // 
        private void rb_line_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new LinePaintOperation(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
        }

        // 
        // Rectangle tool button click
        // 
        private void rb_rectangle_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new RectanglePaintOperation(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
        }

        // 
        // Circle tool button click
        // 
        private void rb_circle_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new EllipsePaintOperation(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
        }

        // 
        // Bucket tool button click
        // 
        private void rb_bucket_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new BucketPaintOperation(cp_mainColorPicker.FirstColor, cp_mainColorPicker.SecondColor));
        }

        // 
        // Selection tool button click
        // 
        private void rb_selection_CheckedChanged(object sender, EventArgs e)
        {
            ChangePaintOperation(new SelectionPaintOperation());
        }

        // 
        // Zoom tool button click
        // 
        private void rb_zoom_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_zoom.Enabled)
            {
                ChangePaintOperation(new ZoomPaintOperation());
            }
        }

        // 
        // Blend Blending Mode radio box check
        // 
        private void rb_blendingBlend_CheckedChanged(object sender, EventArgs e)
        {
            this.iepb_frame.DefaultCompositingMode = CurrentCompositingMode = CompositingMode.SourceOver;
        }

        // 
        // Replace Blending Mode radio box check
        // 
        private void rb_blendingReplace_CheckedChanged(object sender, EventArgs e)
        {
            this.iepb_frame.DefaultCompositingMode = CurrentCompositingMode = CompositingMode.SourceCopy;
        }

        #region Brush Size Control

        // 
        // Brush Size NUD value changed
        // 
        private void anud_brushSize_ValueChanged(object sender, EventArgs e)
        {
            BrushSize = (int)anud_brushSize.Value;

            if (this.iepb_frame.CurrentPaintOperation is SizedPaintOperation)
            {
                (this.iepb_frame.CurrentPaintOperation as SizedPaintOperation).Size = BrushSize;
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
        // Filter menu item click
        // 
        private void tsm_filterItem_Click(object sender, EventArgs e)
        {
            DisplayFilterControls(new FilterControl[] { FilterStore.Instance.CreateFilterControl(((ToolStripMenuItem)sender).Tag as string) });
        }

        #endregion

        // 
        // Form Closed event handler
        // 
        private void FrameView_FormClosed(object sender, FormClosedEventArgs e)
        {
            // Dispose of the image edit panel
            iepb_frame.Dispose();

            // Dispose of the view frame
            viewFrame.Dispose();

            // Dispose of the onion skin
            if (onionSkin != null)
            {
                onionSkin.Dispose();
                onionSkin = null;
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
            if (iepb_frame.CurrentPaintOperation is ColoredPaintOperation)
            {
                switch (eventArgs.TargetColor)
                {
                    // 
                    case ColorPickerColor.FirstColor:
                        FirstColor = eventArgs.NewColor;
                        (iepb_frame.CurrentPaintOperation as ColoredPaintOperation).FirstColor = eventArgs.NewColor;
                        break;
                    // 
                    case ColorPickerColor.SecondColor:
                        SecondColor = eventArgs.NewColor;
                        (iepb_frame.CurrentPaintOperation as ColoredPaintOperation).SecondColor = eventArgs.NewColor;
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
            this.tsl_operationLabel.Text = eventArgs.Status;
        }

        // 
        // Image Panel zoom change event
        // 
        private void PictureBox_ZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            this.anud_zoom.Value = (decimal)e.NewZoom;
        }

        // 
        // Zoom assisted numeric up down value change
        // 
        private void anud_zoom_ValueChanged(object sender, EventArgs e)
        {
            this.iepb_frame.PictureBox.Zoom = new PointF((float)anud_zoom.Value, (float)anud_zoom.Value);
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
            if (this.iepb_frame.DefaultCompositingMode == CompositingMode.SourceCopy)
            {
                this.rb_blendingBlend.PerformClick();
            }
            else
            {
                this.rb_blendingReplace.PerformClick();
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
            BitmapUndoTask bud = new BitmapUndoTask(this.iepb_frame.PictureBox, this.iepb_frame.PictureBox.Bitmap, "Clear");

            FastBitmap.ClearBitmap(this.iepb_frame.PictureBox.Bitmap, 0);

            bud.RegisterNewBitmap(this.iepb_frame.PictureBox.Bitmap);

            this.iepb_frame.PictureBox.Invalidate();

            this.iepb_frame.UndoSystem.RegisterUndo(bud);

            this.MarkModified();
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
            this.iepb_frame.PictureBox.DisplayGrid = tsb_grid.Checked;
        }

        // 
        // Enable/Disable preview
        // 
        private void tsb_previewFrame_Click(object sender, EventArgs e)
        {
            tsb_previewFrame.Checked = !tsb_previewFrame.Checked;

            this.pnl_framePreview.Visible = framePreviewEnabled = tsb_previewFrame.Checked;

            // Update the image preview if enabled
            if (framePreviewEnabled)
            {
                this.zpb_framePreview.Image = viewFrame.GetComposedBitmap();
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
                OnionSkinMode = ModelViews.OnionSkinMode.PreviousAndNextFrames;
            }
            else if (tsb_osNextFrames.Checked)
            {
                OnionSkinMode = ModelViews.OnionSkinMode.NextFrames;
            }
            else if (tsb_osPrevFrames.Checked)
            {
                OnionSkinMode = ModelViews.OnionSkinMode.PreviousFrames;
            }
            else
            {
                OnionSkinMode = ModelViews.OnionSkinMode.None;
            }

            ShowOnionSkin();
        }

        // 
        // Show Next Frames On Onion Skin toolbar button click
        // 
        private void tsb_osNextFrames_Click(object sender, EventArgs e)
        {
            // Update the OnionSkinMode flag
            if (tsb_osPrevFrames.Checked && tsb_osNextFrames.Checked)
            {
                OnionSkinMode = ModelViews.OnionSkinMode.PreviousAndNextFrames;
            }
            else if (tsb_osNextFrames.Checked)
            {
                OnionSkinMode = ModelViews.OnionSkinMode.NextFrames;
            }
            else if (tsb_osPrevFrames.Checked)
            {
                OnionSkinMode = ModelViews.OnionSkinMode.PreviousFrames;
            }
            else
            {
                OnionSkinMode = ModelViews.OnionSkinMode.None;
            }

            ShowOnionSkin();
        }

        // 
        // Onion Skin Depth Combobox selection index changed
        // 
        private void tscb_osFrameCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ignoreOnionSkinDepthComboboxEvent)
                return;

            int depth = int.Parse(tscb_osFrameCount.SelectedItem as string);

            if (depth != OnionSkinDepth)
            {
                OnionSkinDepth = depth;

                ShowOnionSkin();
            }
        }

        /// <summary>
        /// Whether to ignore the tscb_osFrameCount_SelectedIndexChanged event
        /// </summary>
        private bool ignoreOnionSkinDepthComboboxEvent = false;

        #endregion

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
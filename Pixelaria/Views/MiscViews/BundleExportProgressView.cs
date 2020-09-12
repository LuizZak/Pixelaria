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
using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Utils;
using PixelariaLib.Controllers.Exporters;
using PixelariaLib.Data;
using PixelariaLib.Data.Exports;

namespace Pixelaria.Views.MiscViews
{
    /// <summary>
    /// Form used to display the progress of a bundle export
    /// </summary>
    public partial class BundleExportProgressView : Form
    {
        /// <summary>
        /// The bundle to export
        /// </summary>
        private readonly Bundle _bundle;

        /// <summary>
        /// The exporter to use when exporting the bundle
        /// </summary>
        private readonly IBundleExporter _exporter;

        /// <summary>
        /// Whether the user can close this form
        /// </summary>
        private bool _canClose = true;

        /// <summary>
        /// Cancellation token used during bundle sheet export
        /// </summary>
        private CancellationTokenSource _cancellationToken;

        /// <summary>
        /// Timer used to count elapsed time exporting the bundle
        /// </summary>
        private readonly System.Windows.Forms.Timer _timer;

        /// <summary>
        /// Date export began
        /// </summary>
        private DateTime _exportStart = DateTime.Now;

        /// <summary>
        /// Dictionary used to keep track of changes to sheet exports.
        /// Used to invalidate only the treeview region related to a specific sheet
        /// </summary>
        private Dictionary<int, float> _progressTrack = new Dictionary<int, float>(); 

        /// <summary>
        /// Initializes a new instance of the BundleExportProgressView class
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="exporter">The exporter to use when exporting the bundle</param>
        public BundleExportProgressView([NotNull] Bundle bundle, IBundleExporter exporter)
        {
            InitializeComponent();

            _bundle = bundle;
            _exporter = exporter;

            tv_sheets.BeforeSelect += (sender, args) => { args.Cancel = true; };

            CreateTreeView(bundle);

            _timer = new System.Windows.Forms.Timer { Interval = 50 };
            _timer.Tick += TimerOnTick;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ReSharper disable once UseNullPropagation
                if (components != null)
                    components.Dispose();

                // ReSharper disable once UseNullPropagation
                if (_cancellationToken != null)
                    _cancellationToken.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Starts the bundle export process
        /// </summary>
        public void StartExport()
        {
            btn_ok.Text = @"Cancel";
            _canClose = false;

            _progressTrack = new Dictionary<int, float>();
            foreach (var sheet in _bundle.AnimationSheets)
            {
                _progressTrack[sheet.ID] = 0;
            }

            _cancellationToken = new CancellationTokenSource();

            _timer.Start();
            _exportStart = DateTime.Now;

            _exporter.ExportBundleConcurrent(_bundle, _cancellationToken.Token, ExportHandler).ContinueWith((task) =>
            {
                // Re-enable interface
                Invoke(new Action(() =>
                {
                    _timer.Stop();
                    UpdateElapsedTime();

                    _canClose = true;

                    if (_cancellationToken != null && _cancellationToken.IsCancellationRequested)
                    {
                        Close();
                        return;
                    }

                    _cancellationToken = null;

                    btn_ok.Text = @"Ok";
                    btn_ok.Enabled = true;
                }));
            });
        }

        /// <summary>
        /// Updates Elapsed Timer label
        /// </summary>
        private void UpdateElapsedTime()
        {
            var time = DateTime.Now.Subtract(_exportStart);

            lbl_elapsed.Text = $@"{time.Minutes:00}:{time.Seconds:00}";
        }

        // 
        // Export Timer event handler
        // 
        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            UpdateElapsedTime();
        }

        // 
        // Bundle Export event handler
        // 
        private void ExportHandler(BundleExportProgressEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<BundleExportProgressEventArgs>(ExportHandler), args);
                return;
            }

            pb_progress.Value = args.TotalProgress;
            pb_stageProgress.Value = args.StageProgress;

            if (args.ExportStage == BundleExportStage.TextureAtlasGeneration)
            {
                lbl_progress.Text = args.StageDescription;
            }
            else if (args.ExportStage == BundleExportStage.SavingToDisk)
            {
                lbl_progress.Text = @"Saving to disk...";
            }
            else if (args.ExportStage == BundleExportStage.Ended)
            {
                lbl_progress.Text = @"Export successful!";
            }
            
            var sheetArgs = args as SheetGenerationBundleExportProgressEventArgs;
            if (sheetArgs?.Provider is AnimationSheet)
            {
                InvalidateSheetNode((AnimationSheet)sheetArgs.Provider);
            }
            else
            {
                InvalidateTreeView();
            }
        }

        // 
        // Form Shown event handler
        // 
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            StartExport();
        }

        // 
        // Form Closing event handler
        // 
        protected override void OnFormClosing([NotNull] FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !_canClose)
            {
                System.Media.SystemSounds.Beep.Play();

                e.Cancel = true;
            }

            base.OnFormClosing(e);
        }

        //
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            if (_cancellationToken != null)
            {
                _cancellationToken.Cancel();
                btn_ok.Enabled = false;
                return;
            }

            Close();
        }

        #region Tree View

        private void InvalidateTreeView()
        {
            foreach (var sheet in _bundle.AnimationSheets)
            {
                // Invalidate bounds for redrawing of tree view
                InvalidateSheetNode(sheet);
            }
        }

        private void InvalidateSheetNode([NotNull] AnimationSheet sheet)
        {
            // Verify progress for this sheet has changed
            if (!_progressTrack.TryGetValue(sheet.ID, out float cur))
                return;

            float real = _exporter.ProgressForAnimationSheet(sheet);

            if (Math.Abs(real - 1) < float.Epsilon || Math.Abs(cur - real) < float.Epsilon)
                return;

            _progressTrack[sheet.ID] = real;

            // Invalidate bounds for redrawing of tree view
            var node = NodeForSheet(sheet);
            if (node == null)
                return;

            var bounds = node.Bounds;

            bounds.X = bounds.Right;
            bounds.Width = tv_sheets.Width - bounds.Left;

            tv_sheets.Invalidate(bounds);
        }

        private void CreateTreeView([NotNull] Bundle bundle)
        {
            foreach (var sheet in bundle.AnimationSheets)
            {
                var node = new TreeNode(sheet.Name)
                {
                    ImageIndex = 0,
                    Tag = sheet
                };

                tv_sheets.Nodes.Add(node);
            }
        }

        [CanBeNull]
        private AnimationSheet SheetForNode([NotNull] TreeNode node)
        {
            return node.Tag as AnimationSheet;
        }

        private TreeNode NodeForSheet(AnimationSheet sheet)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var node in tv_sheets.Nodes)
            {
                if (Equals((node as TreeNode)?.Tag as AnimationSheet, sheet))
                    return (TreeNode)node;
            }

            return null;
        }

        private float ProgressForSheet(AnimationSheet sheet)
        {
            return _exporter.ProgressForAnimationSheet(sheet);
        }

        #region Rendering

        private void tv_sheets_DrawNode(object sender, [NotNull] DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;

            var sheet = SheetForNode(e.Node);
            if (sheet == null)
                return;

            float progress = ProgressForSheet(sheet);
                
            int width = tv_sheets.Width - e.Bounds.Right;
            if (width > 200)
                width = 200;

            // Figure out bounds
            var boundsForProgress = new Rectangle(tv_sheets.Width - width - 5, e.Bounds.Top, width - 10, e.Bounds.Height);
            boundsForProgress.Inflate(0, -3);

            // Draw background
            ProgressBarRenderer.DrawHorizontalBar(e.Graphics, boundsForProgress);

            // Draw foreground
            var fillForProgress = boundsForProgress;
            fillForProgress.Width = (int)(progress * fillForProgress.Width);

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(6, 176, 37)), fillForProgress);
        }

        #endregion

        #endregion

        /// <summary>
        /// Buffered tree view suitable for flicker-less rendering
        /// </summary>
        public class BufferedTreeView : TreeView 
        {
            protected override void OnHandleCreated(EventArgs e)
            {
                UnsafeNativeMethods.SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
                base.OnHandleCreated(e);
            }
            // Pinvoke:
            private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
            private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
            private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        }
    }
}
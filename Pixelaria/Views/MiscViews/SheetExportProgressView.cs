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

using System.Threading;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Controllers.Exporters;
using Pixelaria.Data;
using Pixelaria.Data.Exports;

namespace Pixelaria.Views.MiscViews
{
    /// <summary>
    /// Form used to display the progress of a bundle export
    /// </summary>
    public partial class SheetExportProgressView : Form
    {
        /// <summary>
        /// The animation sheet to export
        /// </summary>
        private readonly AnimationSheet _sheet;

        /// <summary>
        /// The save path to save the animation sheet to
        /// </summary>
        private readonly string _savePath;

        /// <summary>
        /// The exporter to use when exporting the animation sheet
        /// </summary>
        private readonly IBundleExporter _exporter;

        /// <summary>
        /// Whether the user can close this form
        /// </summary>
        private bool _canClose = true;

        /// <summary>
        /// Initializes a new instance of the SheetExportProgressView class
        /// </summary>
        /// <param name="sheet">The animation sheet to export</param>
        /// <param name="savePath">The path to save the animation sheet to</param>
        /// <param name="exporter">The exporter to use when exporting the animation sheet</param>
        public SheetExportProgressView(AnimationSheet sheet, string savePath, IBundleExporter exporter)
        {
            InitializeComponent();

            _sheet = sheet;
            _savePath = savePath;
            _exporter = exporter;
        }

        /// <summary>
        /// Starts the bundle export process
        /// </summary>
        public void StartExport()
        {
            btn_ok.Visible = false;
            _canClose = false;
            
            _exporter.ExportBundleSheet(_sheet, new CancellationToken(), ExportHandler).ContinueWith(
                task =>
                {
                    Invoke(new Action(() =>
                    {
                        var img = task.Result.Sheet;

                        // Save the image now
                        lbl_progress.Text = @"Saving to disk...";

                        img.Save(_savePath);

                        lbl_progress.Text = @"Export successful!";

                        _canClose = true;
                        btn_ok.Visible = true;
                    }));
                });
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

            pb_progress.Value = args.StageProgress;

            if (args.ExportStage == BundleExportStage.TextureAtlasGeneration)
            {
                lbl_progress.Text = @"Exporting atlas for " + _sheet.Name + @"...";
            }
            else if (args.ExportStage == BundleExportStage.SavingToDisk)
            {
                lbl_progress.Text = @"Saving to disk...";
            }
            else if (args.ExportStage == BundleExportStage.Ended)
            {
                lbl_progress.Text = @"Export successful!";
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
            Close();
        }
    }
}
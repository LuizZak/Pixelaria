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
using System.Windows.Forms;

using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Exporters;

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
        private Bundle bundle;

        /// <summary>
        /// The exporter to use when exporting the bundle
        /// </summary>
        private IDefaultExporter exporter;

        /// <summary>
        /// Whether the user can close this form
        /// </summary>
        private bool canClose = true;

        /// <summary>
        /// Initializes a new instance of the BundleExportProgressView class
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="exporter">The exporter to use when exporting the bundle</param>
        public BundleExportProgressView(Bundle bundle, IDefaultExporter exporter)
        {
            InitializeComponent();

            this.bundle = bundle;
            this.exporter = exporter;
        }

        /// <summary>
        /// Starts the bundle export process
        /// </summary>
        public void StartExport()
        {
            btn_ok.Visible = false;
            canClose = false;
            
            exporter.ExportBundle(bundle, new BundleExportProgressEventHandler(ExportHandler));

            canClose = true;
            btn_ok.Visible = true;
        }

        // 
        // Bundle Export event handler
        // 
        private void ExportHandler(BundleExportProgressEventArgs args)
        {
            pb_progress.Value = args.TotalProgress;
            pb_stageProgress.Value = args.StageProgress;

            if (args.ExportStage == BundleExportStage.TextureAtlasGeneration)
            {
                lbl_progress.Text = "Exporting atlas for " + args.StageDescription + "...";
            }
            else if (args.ExportStage == BundleExportStage.SavingToDisk)
            {
                lbl_progress.Text = "Saving to disk...";
            }
            else if (args.ExportStage == BundleExportStage.Ended)
            {
                lbl_progress.Text = "Export successful!";
            }

            this.Update();
            Application.DoEvents();
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
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !canClose)
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
            this.Close();
        }
    }
}
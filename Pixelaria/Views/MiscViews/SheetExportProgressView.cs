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
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;
using Pixelaria.Data.Exports;

using Pixelaria.Controllers.Exporters;

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
        private AnimationSheet sheet;

        /// <summary>
        /// The save path to save the animation sheet to
        /// </summary>
        private string savePath;

        /// <summary>
        /// The exporter to use when exporting the animation sheet
        /// </summary>
        private IBundleExporter exporter;

        /// <summary>
        /// Whether the user can close this form
        /// </summary>
        private bool canClose = true;

        /// <summary>
        /// Initializes a new instance of the SheetExportProgressView class
        /// </summary>
        /// <param name="sheet">The animation sheet to export</param>
        /// <param name="savePath">The path to save the animation sheet to</param>
        /// <param name="exporter">The exporter to use when exporting the animation sheet</param>
        public SheetExportProgressView(AnimationSheet sheet, string savePath, IBundleExporter exporter)
        {
            InitializeComponent();

            this.sheet = sheet;
            this.savePath = savePath;
            this.exporter = exporter;
        }

        /// <summary>
        /// Starts the bundle export process
        /// </summary>
        public void StartExport()
        {
            btn_ok.Visible = false;
            canClose = false;

            Image img = exporter.ExportAnimationSheet(sheet, ExportHandler);

            // Save the image now
            lbl_progress.Text = "Saving to disk...";

            img.Save(savePath);

            lbl_progress.Text = "Export successful!";

            canClose = true;
            btn_ok.Visible = true;
        }

        // 
        // Bundle Export event handler
        // 
        private void ExportHandler(BundleExportProgressEventArgs args)
        {
            pb_progress.Value = args.StageProgress;

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
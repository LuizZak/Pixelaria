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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Controllers;
using PixelariaLib.Controllers.Exporters;
using PixelariaLib.Data;

namespace Pixelaria.Views.SettingsViews
{
    /// <summary>
    /// Form used to edit the settings of a bundle
    /// </summary>
    public partial class BundleSettingsView : Form
    {
        /// <summary>
        /// The controller that owns this form
        /// </summary>
        private readonly Controller _controller;

        /// <summary>
        /// The bundle being edited
        /// </summary>
        private readonly Bundle _bundle;

        /// <summary>
        /// Creates a new instance of the BundleSettingsView, and starts editing the given bundle
        /// </summary>
        /// <param name="controller">The controller that owns this form</param>
        /// <param name="bundle">The bundle to edit</param>
        public BundleSettingsView(Controller controller, [NotNull] Bundle bundle)
        {
            InitializeComponent();
            PopulateExportMethods();

            _controller = controller;
            _bundle = bundle;

            txt_bundleName.Text = bundle.Name;
            txt_exportPath.Text = bundle.ExportPath;
            var exporter = ExporterController.Instance.Exporters.FirstOrDefault(e => e.SerializationName == bundle.ExporterSerializedName) ?? ExporterController.Instance.DefaultExporter;
            cb_exportMethod.SelectedIndex = cb_exportMethod.FindString(exporter.DisplayName);
            btn_configureExporter.Enabled = SelectedExporter().HasSettings;

            ValidateFields();
        }

        private void PopulateExportMethods()
        {
            cb_exportMethod.BeginUpdate();
            cb_exportMethod.Items.Clear();
            foreach (var exporter in ExporterController.Instance.Exporters)
            {
                cb_exportMethod.Items.Add(exporter.DisplayName);
            }
            cb_exportMethod.EndUpdate();
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        private void ValidateFields()
        {
            bool valid = true;
            bool alert = false;

            // Bundle name
            if (txt_bundleName.Text.Trim() == "" || _controller.AnimationValidator.ValidateAnimationName(txt_bundleName.Text) != "")
            {
                txt_bundleName.BackColor = Color.LightPink;
                lbl_error.Text = (txt_bundleName.Text.Trim() == "" ? "The bundle name cannot be empty" : "The bundle name is not valid");
                valid = false;
            }
            else
            {
                txt_bundleName.BackColor = Color.White;
            }

            // Verify export path is valid

            try
            {
                var fullPath = Path.GetFullPath(txt_exportPath.Text);

                // Export path
                if (!Directory.Exists(fullPath))
                {
                    txt_exportPath.BackColor = Color.LightYellow;
                    lbl_alertLabel.Text = @"The export path does not exist - it will be created on export.";
                    alert = true;
                }
                else
                {
                    txt_exportPath.BackColor = Color.White;
                }
            }
            catch (Exception)
            {
                txt_exportPath.BackColor = Color.LightPink;
                lbl_error.Text = @"The export path is not valid";
                valid = false;
            }

            pnl_errorPanel.Visible = !valid;
            pnl_alertPanel.Visible = alert;

            btn_ok.Enabled = valid;
        }

        private IKnownExporterEntry SelectedExporter()
        {
            var exporter = ExporterController.Instance.Exporters[cb_exportMethod.SelectedIndex];
            return exporter;
        }

        // 
        // Bundle Name textbox change
        // 
        private void txt_bundleName_TextChanged(object sender, EventArgs e)
        {
            ValidateFields();
        }

        // 
        // Export Path textbox change
        // 
        private void txt_exportPath_TextChanged(object sender, EventArgs e)
        {
            ValidateFields();
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            _bundle.Name = txt_bundleName.Text;
            _bundle.ExportPath = txt_exportPath.Text;

            _controller.SetExporter(SelectedExporter());
            _controller.MarkUnsavedChanges(true);
        }

        // 
        // Browse path button
        // 
        private void btn_browse_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                txt_exportPath.Text = fbd.SelectedPath;
            }
        }

        //
        // Configure Exporter button
        //
        private void btn_configureExporter_Click(object sender, EventArgs e)
        {
            _controller.ShowExporterSettings(SelectedExporter().SerializationName);
        }

        //
        // Export Method selection changed
        //
        private void cb_exportMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            btn_configureExporter.Enabled = SelectedExporter().HasSettings;
        }
    }
}
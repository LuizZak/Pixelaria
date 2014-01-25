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
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Controllers;

using Pixelaria.Data;

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
        Controller controller;

        /// <summary>
        /// The bundle being edited
        /// </summary>
        Bundle bundle;

        /// <summary>
        /// Creates a new instance of the BundleSettingsView, and starts editing the given bundle
        /// </summary>
        /// <param name="controller">The controller that owns this form</param>
        /// <param name="bundle">The bundle to edit</param>
        public BundleSettingsView(Controller controller, Bundle bundle)
        {
            InitializeComponent();

            this.controller = controller;
            this.bundle = bundle;

            this.txt_bundleName.Text = bundle.Name;
            this.txt_exportPath.Text = bundle.ExportPath;

            ValidateFields();
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        private void ValidateFields()
        {
            bool valid = true;
            bool alert = false;

            // Bundle name
            if (txt_bundleName.Text.Trim() == "" || controller.AnimationValidator.ValidateAnimationName(txt_bundleName.Text) != "")
            {
                txt_bundleName.BackColor = Color.LightPink;
                lbl_error.Text = (txt_bundleName.Text.Trim() == "" ? "The bundle name cannot be empty" : "The bundle name is not valid");
                valid = false;
            }
            else
            {
                txt_bundleName.BackColor = Color.White;
            }

            // Export path
            if (txt_exportPath.Text != "" && !Directory.Exists(txt_exportPath.Text))
            {
                txt_exportPath.BackColor = Color.LightPink;
                lbl_error.Text = "The export path is not valid";
                valid = false;
            }
            else
            {
                txt_exportPath.BackColor = Color.White;
            }

            if (valid)
            {
                
            }

            pnl_errorPanel.Visible = !valid;
            pnl_alertPanel.Visible = alert;

            btn_ok.Enabled = valid;
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
            bundle.Name = this.txt_bundleName.Text;
            bundle.ExportPath = this.txt_exportPath.Text;

            controller.MarkUnsavedChanges(true);
        }

        // 
        // Browse path button
        // 
        private void btn_browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog(this) == DialogResult.OK)
            {
                txt_exportPath.Text = fbd.SelectedPath;
            }
        }
    }
}
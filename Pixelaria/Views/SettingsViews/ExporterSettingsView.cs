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
using JetBrains.Annotations;
using Pixelaria.Views.ModelViews;
using PixelariaLib.Controllers.Exporters;

namespace Pixelaria.Views.SettingsViews
{
    public partial class ExporterSettingsView : ModifiableContentView
    {
        /// <summary>
        /// Whether modifications where made while this form was active.
        /// </summary>
        public bool ModifiedContents { get; private set; }

        /// <summary>
        /// The settings object being configured by this view.
        /// </summary>
        public IBundleExporterSettings Settings { get; }

        public ExporterSettingsView([NotNull] IBundleExporterSettings settings)
        {
            Settings = settings.Clone();

            InitializeComponent();

            LoadProperties(settings);

            propertyGrid.PropertyValueChanged += PropertyGridOnPropertyValueChanged;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            var exporterName = ExporterController.Instance.DisplayNameForExporter(Settings.ExporterSerializedName);
            if (exporterName != null)
            {
                Text = $@"{exporterName} Exporter Settings";
            }
        }

        private void LoadProperties(IBundleExporterSettings settings)
        {
            propertyGrid.SelectedObject = settings;
        }

        private void PropertyGridOnPropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            MarkModified();
        }
        
        public override void ApplyChanges()
        {
            base.ApplyChanges();

            ModifiedContents = true;
        }

        public override void MarkModified()
        {
            base.MarkModified();

            btn_ok.Enabled = true;
            btn_apply.Enabled = true;
        }

        public override void MarkUnmodified()
        {
            base.MarkUnmodified();

            btn_ok.Enabled = false;
            btn_apply.Enabled = false;
        }

        //
        // Ok button click
        //
        private void btn_ok_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ApplyChangesAndClose();
        }

        //
        // Cancel button click
        //
        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            DiscardChangesAndClose();
        }

        //
        // Apply button click
        //
        private void btn_apply_Click(object sender, EventArgs e)
        {
            ApplyChanges();
        }
    }
}

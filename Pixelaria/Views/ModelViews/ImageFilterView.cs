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
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Controllers;
using Pixelaria.Filters;

using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Implements an interface that the user can use to tweak settings of and apply a filter to an image
    /// </summary>
    internal partial class ImageFilterView : Form
    {
        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="bitmap">A bitmap to apply the filter to</param>
        private ImageFilterView([NotNull] Bitmap bitmap)
        {
            InitializeComponent();

            fs_filters.SetImage(bitmap);

            pnl_errorPanel.Visible = false;

            btn_ok.Enabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="filters">The array of FilterControls to use as interface to mediate the interaction between the filters to be applied and the user</param>
        /// <param name="bitmap">A bitmap to apply the filter to</param>
        public ImageFilterView([NotNull] IFilterControl[] filters, [NotNull] Bitmap bitmap)
            : this(bitmap)
        {
            fs_filters.LoadFilters(filters);
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="preset">A FilterPreset that contains data about filters to load on this BaseFilterView</param>
        /// <param name="bitmap">A bitmap to apply the filter to</param>
        public ImageFilterView([NotNull] FilterPreset preset, [NotNull] Bitmap bitmap)
            : this(bitmap)
        {
            fs_filters.LoadFilterPreset(preset);
        }

        /// <summary>
        /// Returns whether the current filter configuration can make any significant changes to the bitmap loaded
        /// </summary>
        /// <returns>Whether the current filter configuration can make any significant changes to the bitmap loaded</returns>
        public bool ChangesDetected()
        {
            return fs_filters.ChangesDetected();
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            if (!fs_filters.ChangesDetected())
            {
                MessageBox.Show(
                    @"No changes will be made with the filter presets present (i.e. all frames will look the same!)",
                    @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            fs_filters.ApplyFilter();

            FiltersController.Instance.AddFilters(fs_filters.Filters);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
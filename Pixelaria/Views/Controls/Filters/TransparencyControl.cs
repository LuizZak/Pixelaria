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

using System.ComponentModel;
using System.Drawing;
using PixCore.Controls.ColorControls;
using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a <see cref="FilterControl{T}"/> that handles a <see cref="TransparencyFilter"/>
    /// </summary>
    internal class TransparencyControl : FilterControl<TransparencyFilter>
    {
        /// <summary>
        /// Initializes a new class of the <see cref="TransparencyControl"/> class
        /// </summary>
        public TransparencyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this <see cref="TransparencyControl"/>
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new TransparencyFilter {Transparency = 1};
            }
        }

        /// <summary>
        /// Updates the fields from this <see cref="TransparencyControl"/> based on the data from the
        /// given <see cref="TransparencyFilter"/> instance
        /// </summary>
        /// <param name="referenceFilter">The <see cref="TransparencyFilter"/> instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(TransparencyFilter referenceFilter)
        {
            cs_transparency.CurrentValue = referenceFilter.Transparency;
        }

        // 
        // Transparency slider value changed
        //
        private void cs_transparency_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            filter.Transparency = cs_transparency.CurrentValue;

            FireFilterUpdated();
        }

        private ColorSlider cs_transparency;

        #region Designer Required Code

        #region Component Designer generated code

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        protected IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (preview != null)
            {
                preview.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        protected void InitializeComponent()
        {
            this.cs_transparency = new PixCore.Controls.ColorControls.ColorSlider();
            this.SuspendLayout();
            // 
            // cs_transparency
            // 
            this.cs_transparency.ActiveColor = new PixCore.Colors.AhslColor(1F, 0F, 0F, 0F);
            this.cs_transparency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cs_transparency.CurrentValue = 1F;
            this.cs_transparency.CustomColorTitle = "Custom";
            this.cs_transparency.CustomEndColor = new PixCore.Colors.AhslColor(1F, 0F, 0F, 1F);
            this.cs_transparency.CustomStartColor = new PixCore.Colors.AhslColor(1F, 0F, 0F, 0F);
            this.cs_transparency.Location = new System.Drawing.Point(1, 0);
            this.cs_transparency.Name = "cs_transparency";
            this.cs_transparency.Size = new System.Drawing.Size(479, 38);
            this.cs_transparency.TabIndex = 0;
            this.cs_transparency.ColorChanged += new PixCore.Controls.ColorControls.ColorSlider.ColorChangedEventHandler(this.cs_transparency_ColorChanged);
            // 
            // TransparencyControl
            // 
            this.Controls.Add(this.cs_transparency);
            this.Name = "TransparencyControl";
            this.Size = new System.Drawing.Size(481, 40);
            this.ResumeLayout(false);

        }

        #endregion
        
        #endregion
    }
}
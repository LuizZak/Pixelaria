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
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

using Pixelaria.Filters;

namespace Pixelaria.Views.Controls.Filters
{
    /// <summary>
    /// Represents a FilterControl that handles a TransparencyFilter
    /// </summary>
    internal class TransparencyControl : FilterControl
    {
        /// <summary>
        /// Initializes a new class of the TransparencyControl class
        /// </summary>
        public TransparencyControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes this TransparencyControl
        /// </summary>
        /// <param name="bitmap">The Bitmap to generate the visualization for</param>
        public override void Initialize(Bitmap bitmap)
        {
            base.Initialize(bitmap);

            if (filter == null)
            {
                filter = new TransparencyFilter();
                ((TransparencyFilter)filter).Transparency = 1;
            }
        }

        /// <summary>
        /// Updates the fields from this FilterControl based on the data from the
        /// given IFilter instance
        /// </summary>
        /// <param name="referenceFilter">The IFilter instance to update the fields from</param>
        public override void UpdateFieldsFromFilter(IFilter referenceFilter)
        {
            if (!(referenceFilter is TransparencyFilter))
                return;

            anud_transparency.Value = (decimal)((TransparencyFilter)referenceFilter).Transparency * 255;
        }

        // 
        // Transparency nud value changed
        // 
        private void anud_transparency_ValueChanged(object sender, EventArgs e)
        {
            ((TransparencyFilter)filter).Transparency = (float)anud_transparency.Value / 255;

            FireFilterUpdated();
        }

        #region Designer Required Code

        #region Component Designer generated code

        private Label label1;
        private AssistedNumericUpDown anud_transparency;

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
            this.anud_transparency = new Pixelaria.Views.Controls.AssistedNumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // anud_transparency
            // 
            this.anud_transparency.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.anud_transparency.AssistBarColor = System.Drawing.Color.CornflowerBlue;
            this.anud_transparency.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.anud_transparency.Increment = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.anud_transparency.Location = new System.Drawing.Point(78, 3);
            this.anud_transparency.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_transparency.Minimum = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.anud_transparency.Name = "anud_transparency";
            this.anud_transparency.Size = new System.Drawing.Size(400, 32);
            this.anud_transparency.TabIndex = 1;
            this.anud_transparency.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.anud_transparency.ValueChanged += new System.EventHandler(this.anud_transparency_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transparency:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // TransparencyControl
            // 
            this.Controls.Add(this.anud_transparency);
            this.Controls.Add(this.label1);
            this.Name = "TransparencyControl";
            this.Size = new System.Drawing.Size(481, 40);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private void label1_Click(object sender, EventArgs e)
        {

        }

        #endregion
    }
}
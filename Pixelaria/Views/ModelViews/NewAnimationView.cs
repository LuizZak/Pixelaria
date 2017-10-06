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

using Pixelaria.Data;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Form used as interface for new animation creation
    /// </summary>
    public partial class NewAnimationView : Form
    {
        /// <summary>
        /// Controller owning this form
        /// </summary>
        readonly Controller _controller;

        /// <summary>
        /// Optional AnimationSheet that will own the newly created animation
        /// </summary>
        [CanBeNull]
        readonly AnimationSheet _parentSheet;

        /// <summary>
        /// Creates a new NewAnimationView form
        /// </summary>
        /// <param name="controller">The controller that owns this view</param>
        /// <param name="parentSheet">Optional AnimationSheet that will own the newly created Animation</param>
        public NewAnimationView(Controller controller, [CanBeNull] AnimationSheet parentSheet = null)
        {
            _controller = controller;
            _parentSheet = parentSheet;

            InitializeComponent();

            txt_animationName.Text = controller.GetUniqueUntitledAnimationName();

            ValidateFields();
        }

        /// <summary>
        /// Validates the fields from this form, and disables the saving of changes if one or more of the fields is invalid
        /// </summary>
        private void ValidateFields()
        {
            bool valid = true;
            const bool alert = false;

            // Animation name
            var validation = _controller.AnimationValidator.ValidateAnimationName(txt_animationName.Text);
            if (validation != "")
            {
                txt_animationName.BackColor = Color.LightPink;
                lbl_error.Text = validation;
                valid = false;
            }
            else
            {
                txt_animationName.BackColor = Color.White;
            }

            pnl_errorPanel.Visible = !valid;
            pnl_alertPanel.Visible = alert;

            btn_ok.Enabled = valid;
        }

        // 
        // Animation Name textbox input
        // 
        private void txt_animationName_TextChanged(object sender, EventArgs e)
        {
            ValidateFields();
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            _controller.CreateAnimation(txt_animationName.Text, (int)nud_width.Value, (int)nud_height.Value, (int)nud_fps.Value, cb_frameskip.Checked, true, _parentSheet);
        }
    }
}
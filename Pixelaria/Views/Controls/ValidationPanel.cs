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
using Pixelaria.Properties;

namespace Pixelaria.Views.Controls
{
    public partial class ValidationPanel : UserControl
    {
        public ValidationPanel()
        {
            InitializeComponent();
            Visible = false;
        }

        public void ClearState()
        {
            Visible = false;
        }

        public void ShowErrorState(string message)
        {
            Visible = true;
            pb_state.Image = Resources.error_22;
            lbl_alertLabel.Text = message;
        }

        public void ShowWarningState(string message)
        {
            Visible = true;
            pb_state.Image = Resources.important_22;
            lbl_alertLabel.Text = message;
        }

        //
        // SetBoundsCore override used to lock the control's height
        //
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, Math.Max(80, width), 28, specified);
        }
    }
}

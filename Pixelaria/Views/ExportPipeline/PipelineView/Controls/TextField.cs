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

using System.Diagnostics;
using System.Windows.Forms;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A Text Field that accepts user inputs via keyboard to alter a text content within.
    /// </summary>
    internal class TextField : ControlView, IKeyboardEventHandler
    {
        /// <summary>
        /// Gets or sets the text of this textfield.
        /// 
        /// As keyboard input is received, this value is updated accordingly.
        /// </summary>
        public string Text { get; set; } = "";

        public override bool CanBecomeFirstResponder => true;

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            BecomeFirstResponder();
        }

        public void OnKeyPress(KeyPressEventArgs e)
        {
            Debug.WriteLine(e);

            Text += e.KeyChar;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            Debug.WriteLine(e);
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            
        }

        public void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            
        }

        public override bool CanHandle(IEventRequest eventRequest)
        {
            if (eventRequest is IKeyboardEventRequest)
                return true;

            return base.CanHandle(eventRequest);
        }
    }
}

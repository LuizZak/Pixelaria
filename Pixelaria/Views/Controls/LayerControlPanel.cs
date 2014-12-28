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

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Control that is used to display an interface for the user to manage a frame's layers
    /// </summary>
    public class LayerControlPanel : Panel
    {
        /// <summary>
        /// The title for this panel
        /// </summary>
        protected string panelTitle = "Layers";

        // 
        // OnPaintBackground event handler
        // 
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);

            // Paint the title bar
            Rectangle titleRectangle = new Rectangle(Point.Empty, new Size(Width, 17));
            LinearGradientBrush brush = new LinearGradientBrush(Point.Empty, new Point(0, titleRectangle.Height), Color.White, Color.FromArgb(255, 236, 236, 236));

            e.Graphics.FillRectangle(brush, titleRectangle);

            Pen linePen = new Pen(Color.FromArgb(255, 160, 160, 160));
            e.Graphics.DrawLine(linePen, titleRectangle.Left, titleRectangle.Bottom, titleRectangle.Right,titleRectangle.Bottom);

            e.Graphics.DrawString(panelTitle, Font, Brushes.Black, titleRectangle, new StringFormat {Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center});
        }
    }
}
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
using System.IO;
using System.Windows.Forms;
using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements a Pencil paint operation
    /// </summary>
    public class PencilPaintTool : BasePencilPaintTool, IColoredPaintTool, ICompositingPaintTool
    {
        /// <summary>
        /// Initializes a new instance of the PencilPaintTool class
        /// </summary>
        public PencilPaintTool()
        {
            undoDecription = "Pencil";
        }

        /// <summary>
        /// Initializes a new instance of the PencilPaintTool class, initializing the object
        /// with the two pencil colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public PencilPaintTool(Color firstColor, Color secondColor, int pencilSize)
            : this()
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
            size = 1;
        }

        /// <summary>
        /// Initializes this PencilPaintTool
        /// </summary>
        /// <param name="targetPictureBox"></param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.pencil_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }
    }
}
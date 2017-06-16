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
using System.IO;
using System.Windows.Forms;

using Pixelaria.Algorithms.PaintOperations;

using Pixelaria.Views.Controls.PaintTools.Abstracts;
using Pixelaria.Views.Controls.PaintTools.Interfaces;

namespace Pixelaria.Views.Controls.PaintTools
{
    /// <summary>
    /// Implements an Eraser paint operation
    /// </summary>
    public class EraserPaintTool : BasePencilPaintTool, IColoredPaintTool, IColorBlender, ISizedPaintTool
    {
        /// <summary>
        /// Gets or sets the compositing mode for the pen
        /// </summary>
        public override CompositingMode CompositingMode
        {
            get => base.CompositingMode;
            set
            {
                base.CompositingMode = value;
                // When the blend mode is SourceCopy, the transparency of the pixels should be set only once per pass
                accumulateAlpha = value != CompositingMode.SourceCopy;
            }
        }

        /// <summary>
        /// Initializes a new instance of the PencilPaintTool class, initializing the object
        /// with the two pencil colors to use
        /// </summary>
        /// <param name="firstColor">The first pencil color</param>
        /// <param name="secondColor">The second pencil color</param>
        /// <param name="pencilSize">The size of the pencil</param>
        public EraserPaintTool(Color firstColor, Color secondColor, int pencilSize)
        {
            this.firstColor = firstColor;
            this.secondColor = secondColor;
            accumulateAlpha = true;
            size = pencilSize;
        }

        /// <summary>
        /// Initializes this EraserPaintTool
        /// </summary>
        /// <param name="targetPictureBox">The target picture box</param>
        public override void Initialize(ImageEditPanel.InternalPictureBox targetPictureBox)
        {
            base.Initialize(targetPictureBox);

            pencilOperation.ColorBlender = this;

            undoDecription = "Eraser";

            // Initialize the operation cursor
            MemoryStream cursorMemoryStream = new MemoryStream(Properties.Resources.eraser_cursor);
            ToolCursor = new Cursor(cursorMemoryStream);
            cursorMemoryStream.Dispose();
        }

        /// <summary>
        /// Blends two colors together with an alpha erase mode
        /// </summary>
        /// <param name="backColor">The back color to blend</param>
        /// <param name="foreColor">The fore color to blend</param>
        /// <param name="blendCompositingMode">The compositing mode to blend with</param>
        /// <returns>The two colors, blended with an alpha erase mode</returns>
        public Color BlendColors(Color backColor, Color foreColor, CompositingMode blendCompositingMode)
        {
            if (backColor.A == 0)
            {
                return backColor;
            }

            if (foreColor.A == 0)
            {
                return Color.FromArgb(0, 0, 0, 0);
            }

            float newAlpha = (((float)backColor.A / 255) * (1 - (float)foreColor.A / 255));
            return Color.FromArgb((int)(newAlpha * 255), backColor);
        }
    }
}
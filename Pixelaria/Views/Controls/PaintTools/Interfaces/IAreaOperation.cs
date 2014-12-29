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

namespace Pixelaria.Views.Controls.PaintTools.Interfaces
{
    /// <summary>
    /// Specifies an operation that has start/finish/cancel and operates over an area of the target bitmap
    /// </summary>
    public interface IAreaOperation
    {
        /// <summary>
        /// Starts the area operation with the given area
        /// </summary>
        /// <param name="area">The area of the bitmap to start operating on</param>
        void StartOperation(Rectangle area);

        /// <summary>
        /// Cancels the current area operation
        /// </summary>
        void CancelOperation(bool drawOnCanvas, bool cancelGroup = true);

        /// <summary>
        /// Finishes the selection area operation
        /// </summary>
        /// <param name="drawToCanvas">Whether to draw the image to canvas before deleting it</param>
        void FinishOperation(bool drawToCanvas);
    }
}
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
using JetBrains.Annotations;
using Pixelaria.Algorithms.PaintOperations.UndoTasks;

namespace Pixelaria.Algorithms.PaintOperations.Interfaces
{
    /// <summary>
    /// Specifies the basic operation interface for pencil-type operations, which are based around 'MoveTo's and 'DrawTo's
    /// </summary>
    public interface IPencilOperation
    {
        /// <summary>
        /// Moves the pencil tip to point to a specific spot, without drawing in the process
        /// </summary>
        /// <param name="x">The position to move the pencil tip to</param>
        /// <param name="y">The position to move the pencil tip to</param>
        void MoveTo(int x, int y);

        /// <summary>
        /// Moves the pencil tip to point to a specific spot, drawing between the last and new positions
        /// </summary>
        /// <param name="x">The position to move the pencil tip to</param>
        /// <param name="y">The position to move the pencil tip to</param>
        void DrawTo(int x, int y);
    }

    /// <summary>
    /// Interface to be implemented by objects that are notified when a plotting-type operation plots pixels on a bitmap
    /// </summary>
    public interface IPlottingOperationNotifier
    {
        /// <summary>
        /// Method called whenever the pencil operation has plotted a pixel on the underlying bitmap
        /// </summary>
        /// <param name="point">The position of the plot</param>
        /// <param name="oldColor">The old color of the pixel, before the plot</param>
        /// <param name="newColor">The new color of the pixel, after the plot</param>
        void PlottedPixel(Point point, int oldColor, int newColor);

        /// <summary>
        /// Method called to notify the plotting operation has started
        /// </summary>
        /// <param name="accumulateAlpha">Whether the plotting operation has alpha accumulation mode on</param>
        void OperationStarted(bool accumulateAlpha);

        /// <summary>
        /// Method called to notify the plotting operation was finished
        /// </summary>
        /// <param name="pixelHistory">The pixel history tracker containing the information about the pixels that were modified during the operation</param>
        void OperationFinished([CanBeNull] PixelHistoryTracker pixelHistory);
    }
}
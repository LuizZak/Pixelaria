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

using PixCore.Geometry;

namespace PixRendering
{
    /// <summary>
    /// Represents a sink that provides drawing operations that <see cref="IRenderer"/>
    /// instances can forward their path-related drawing operations to.
    /// </summary>
    public interface IPathInputSink
    {
        /// <summary>
        /// Begins a new figure on this sink, specifying whether to start a filled or closed figure.
        /// </summary>
        void BeginFigure(Vector location, bool filled);

        /// <summary>
        /// Moves the pen position to a given point without performing a drawing operation.
        /// </summary>
        void MoveTo(Vector point);

        /// <summary>
        /// Draws a line from the current pen position to the given point.
        /// </summary>
        /// <param name="point">Point to add the line to, starting from the current pen position.</param>
        void LineTo(Vector point);

        /// <summary>
        /// Adds a cubic bezier path from the current pen position, through the two anchors, ending at a given point.
        /// </summary>
        void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint);

        /// <summary>
        /// Adds a rectangle to this path sink.
        /// 
        /// The operation doesn't continue from the current pen position, moving the pen to the origin before starting
        /// the drawing operation.
        /// </summary>
        void AddRectangle(AABB rectangle);

        /// <summary>
        /// Closes the current figure on this path input, optionally specifying whether to close the current path such
        /// that it loops back to the beginning.
        /// </summary>
        void EndFigure(bool closePath);
    }
}
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

using JetBrains.Annotations;

namespace PixCore.Geometry
{
    /// <summary>
    /// Represents an interface for an object that has its own local coordinates,
    /// and is capable of converting to and from other reference points.
    /// </summary>
    public interface ISpatialReference
    {
        /// <summary>
        /// Converts a point from a given <see cref="ISpatialReference"/>'s local coordinates to this
        /// reference's coordinates.
        /// 
        /// If <see cref="from"/> is null, converts from screen coordinates.
        /// </summary>
        Vector ConvertFrom(Vector point, [CanBeNull] ISpatialReference from);

        /// <summary>
        /// Converts a point from this <see cref="ISpatialReference"/>'s local coordinates to a given
        /// reference's coordinates.
        /// 
        /// If <see cref="to"/> is null, converts from this node to screen coordinates.
        /// </summary>
        Vector ConvertTo(Vector point, [CanBeNull] ISpatialReference to);

        /// <summary>
        /// Converts an AABB from a given <see cref="ISpatialReference"/>'s local coordinates to this
        /// reference's coordinates.
        /// 
        /// If <see cref="from"/> is null, converts from screen coordinates.
        /// </summary>
        AABB ConvertFrom(AABB aabb, [CanBeNull] ISpatialReference from);

        /// <summary>
        /// Converts an AABB from this <see cref="ISpatialReference"/>'s local coordinates to a given
        /// reference's coordinates.
        /// 
        /// If <see cref="to"/> is null, converts from this node to screen coordinates.
        /// </summary>
        AABB ConvertTo(AABB aabb, [CanBeNull] ISpatialReference to);

        /// <summary>
        /// Gets a matrix that represents the absolute transform of this reference point.
        /// </summary>
        Matrix2D GetAbsoluteTransform();
    }
}
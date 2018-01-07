﻿/*
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
using PixCore.Geometry;
using PixCore.Text;

namespace PixDirectX.Rendering
{
    /// <summary>
    /// Provides an interface for objects to request metrics about positions of glyphs in a string.
    /// </summary>
    public interface ITextMetricsProvider
    {
        /// <summary>
        /// Gets the bounding box for a single character at a given absolute string offset
        /// </summary>
        AABB LocationOfCharacter(int offset, [NotNull] IAttributedText text, TextLayoutAttributes textLayoutAttributes);

        /// <summary>
        /// Gets the bounding box for a set of characters at a given absolute string offset + length
        /// </summary>
        AABB[] LocationOfCharacters(int offset, int length, [NotNull] IAttributedText text, TextLayoutAttributes textLayoutAttributes);
    }
}
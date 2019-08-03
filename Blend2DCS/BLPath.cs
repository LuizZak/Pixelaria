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

using Blend2DCS.Internal;

namespace Blend2DCS
{
    public class BLPath
    {
        private BLPathCore _pathCore;

        /// <summary>
        /// Returns path size (count of vertices used).
        /// </summary>
        public int Size => UnsafePathCore.blPathGetSize(ref _pathCore);

        /// <summary>
        /// Returns path capacity (count of allocated vertices).
        /// </summary>
        public int Capacity => UnsafePathCore.blPathGetCapacity(ref _pathCore);

        public BLPath()
        {
            _pathCore = new BLPathCore();
            UnsafePathCore.blPathInit(ref _pathCore);
        }

        ~BLPath()
        {
            UnsafePathCore.blPathReset(ref _pathCore);
        }
    }
}

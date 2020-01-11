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

using System;
using Blend2DCS.Geometry;
using Blend2DCS.Internal;

namespace Blend2DCS
{
    public class BLPath : IDisposable
    {
        internal BLPathCore Path;

        /// <summary>
        /// Returns path size (count of vertices used).
        /// </summary>
        public int Size => UnsafePathCore.blPathGetSize(ref Path);

        /// <summary>
        /// Returns path capacity (count of allocated vertices).
        /// </summary>
        public int Capacity => UnsafePathCore.blPathGetCapacity(ref Path);

        public BLPath()
        {
            Path = new BLPathCore();
            UnsafePathCore.blPathInit(ref Path);
        }

        ~BLPath()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            UnsafePathCore.blPathReset(ref Path);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void MoveTo(double x, double y)
        {
            UnsafePathCore.blPathMoveTo(ref Path, x, y);
        }
        public void LineTo(double x, double y)
        {
            UnsafePathCore.blPathLineTo(ref Path, x, y);
        }

        public void CubicTo(double x1, double y1, double x2, double y2, double x3, double y3)
        {
            UnsafePathCore.blPathCubicTo(ref Path, x1, y1, x2, y2, x3, y3);
        }

        public void AddRectangle(BLRect rectangle, BLGeometryDirection direction = BLGeometryDirection.Cw)
        {
            UnsafePathCore.blPathAddRectD(ref Path, ref rectangle, direction);
        }

        public void Close()
        {
            UnsafePathCore.blPathClose(ref Path);
        }
    }
}

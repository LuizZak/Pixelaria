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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blend2DCS.Geometry;
using Blend2DCS.Internal;
using JetBrains.Annotations;

namespace Blend2DCS
{
    public class BLPattern : IDisposable
    {
        internal BLPatternCore Pattern;

        public BLPattern()
        {
            UnsafePatternCore.blPatternInit(ref Pattern);
        }

        public BLPattern([NotNull] BLImage image, BLRectI area = new BLRectI(), BLExtendMode extendMode = BLExtendMode.Pad, BLMatrix2D? matrix = null)
        {
            var realMatrix = matrix ?? BLMatrix2D.Identity();

            UnsafePatternCore.blPatternInitAs(ref Pattern, ref image.Image, ref area, extendMode, ref realMatrix);
        }

        private void ReleaseUnmanagedResources()
        {
            UnsafePatternCore.blPatternReset(ref Pattern);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~BLPattern()
        {
            ReleaseUnmanagedResources();
        }

        public void SetArea(BLRectI area)
        {
            UnsafePatternCore.blPatternSetArea(ref Pattern, ref area);
        }

        public void SetExtendMode(BLExtendMode mode)
        {
            UnsafePatternCore.blPatternSetExtendMode(ref Pattern, mode);
        }
    }
}

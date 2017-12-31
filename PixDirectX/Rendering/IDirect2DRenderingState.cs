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
using JetBrains.Annotations;
using PixCore.Geometry;
using SharpDX;
using SharpDX.Direct2D1;

namespace PixDirectX.Rendering
{
    public interface IDirect2DRenderingState : IDisposable
    {
        /// <summary>
        /// Direct2D factory instance
        /// </summary>
        Factory D2DFactory { get; }

        /// <summary>
        /// DirectWrite factory instance
        /// </summary>
        SharpDX.DirectWrite.Factory DirectWriteFactory { get; }

        /// <summary>
        /// Gets the render target for this rendering state
        /// </summary>
        RenderTarget D2DRenderTarget { get; }
        
        /// <summary>
        /// Gets the time span since the last frame rendered
        /// </summary>
        TimeSpan FrameRenderDeltaTime { get; }

        void PushingTransform([InstantHandle, NotNull] Action execute);
        void PushMatrix(Matrix3x2 matrix);
        void PopMatrix();
        void WithTemporaryClipping(AABB clipping, [InstantHandle, NotNull] Action execute);
    }
}
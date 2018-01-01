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
using System.Drawing;

namespace PixDirectX.Rendering
{
    /// <inheritdoc cref="ID2DImageResourceManager" />
    /// <summary>
    /// Helper class for dealing with Direct2D image resource loading
    /// </summary>
    internal sealed class D2DImageResources : IDisposable, ID2DImageResourceManager
    {
        private readonly Dictionary<string, SharpDX.Direct2D1.Bitmap> _bitmapResources = new Dictionary<string, SharpDX.Direct2D1.Bitmap>();

        public void Dispose()
        {
            foreach (var value in _bitmapResources.Values)
            {
                value.Dispose();
            }

            _bitmapResources.Clear();
        }

        public void AddImageResource(IDirect2DRenderingState state, Bitmap bitmap, string resourceName)
        {
            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = BaseDirect2DRenderer.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
        }

        public void RemoveImageResources()
        {
            foreach (var value in _bitmapResources.Values)
            {
                value.Dispose();
            }

            _bitmapResources.Clear();
        }

        public void RemoveImageResource(string resourceName)
        {
            if (_bitmapResources.ContainsKey(resourceName))
            {
                _bitmapResources.Remove(resourceName);
            }
        }

        public ImageResource AddPipelineNodeImageResource(IDirect2DRenderingState state,
            Bitmap bitmap, string resourceName)
        {
            var res = new ImageResource(resourceName, bitmap.Width, bitmap.Height);

            AddImageResource(state, bitmap, resourceName);

            return res;
        }

        public ImageResource? PipelineNodeImageResource(string resourceName)
        {
            var res = BitmapForResource(resourceName);
            if (res != null)
                return new ImageResource(resourceName, res.PixelSize.Width, res.PixelSize.Height);

            return null;
        }
        
        public SharpDX.Direct2D1.Bitmap BitmapForResource(ImageResource resource)
        {
            return BitmapForResource(resource.ResourceName);
        }

        public SharpDX.Direct2D1.Bitmap BitmapForResource(string name)
        {
            return _bitmapResources.TryGetValue(name, out SharpDX.Direct2D1.Bitmap bitmap) ? bitmap : null;
        }
    }
}
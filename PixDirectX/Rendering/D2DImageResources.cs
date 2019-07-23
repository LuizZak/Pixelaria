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
using JetBrains.Annotations;
using Bitmap = System.Drawing.Bitmap;

namespace PixDirectX.Rendering
{
    /// <inheritdoc cref="IImageResourceManager" />
    /// <summary>
    /// Helper class for dealing with Direct2D image resource loading
    /// </summary>
    public sealed class ImageResources : IDisposable, IImageResourceManager
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

        public ImageResource AddImageResource(IRenderLoopState renderLoopState, Bitmap bitmap, string resourceName)
        {
            var state = (IDirect2DRenderingState) renderLoopState;

            var res = new ImageResource(resourceName, bitmap.Width, bitmap.Height);

            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = BaseDirect2DRender.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);

            return res;
        }

        public ImageResource AddImageResource([NotNull] IDirect2DRenderingState state, [NotNull] SharpDX.WIC.Bitmap bitmap, [NotNull] string resourceName)
        {
            var res = new ImageResource(resourceName, bitmap.Size.Width, bitmap.Size.Height);

            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = BaseDirect2DRender.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);

            return res;
        }

        public IManagedImageResource CreateManagedImageResource(IRenderLoopState renderLoopState, Bitmap bitmap)
        {
            var state = (IDirect2DRenderingState)renderLoopState;

            var dxBitmap = BaseDirect2DRender.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);

            return new DirectXBitmap(dxBitmap);
        }

        public void UpdateManagedImageResource(IRenderLoopState renderLoopState, ref IManagedImageResource managedImage, Bitmap bitmap)
        {
            var state = (IDirect2DRenderingState)renderLoopState;
            if(!(managedImage is DirectXBitmap dxBitmap))
                throw new ArgumentException($"Expected bitmap to be of type ${typeof(DirectXBitmap)}");

            dxBitmap.bitmap.Dispose();
            dxBitmap.bitmap = BaseDirect2DRender.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
        }

        public void RemoveAllImageResources()
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

        public IReadOnlyList<ImageResource> AllImageResources()
        {
            return _bitmapResources.Select(pair =>
                    new ImageResource(pair.Key, pair.Value.PixelSize.Width, pair.Value.PixelSize.Height)
                ).ToArray();
        }

        public ImageResource? GetImageResource(string resourceName)
        {
            var res = BitmapForResource(resourceName);
            if (res != null)
                return new ImageResource(resourceName, res.PixelSize.Width, res.PixelSize.Height);

            return null;
        }
        
        [CanBeNull]
        public SharpDX.Direct2D1.Bitmap BitmapForResource(ImageResource resource)
        {
            return BitmapForResource(resource.ResourceName);
        }

        [CanBeNull]
        public SharpDX.Direct2D1.Bitmap BitmapForResource([NotNull] string name)
        {
            return _bitmapResources.TryGetValue(name, out var bitmap) ? bitmap : null;
        }
    }

    public class DirectXBitmap : IManagedImageResource
    {
        internal SharpDX.Direct2D1.Bitmap bitmap;

        public int Width => bitmap.PixelSize.Width;
        public int Height => bitmap.PixelSize.Height;

        public DirectXBitmap(SharpDX.Direct2D1.Bitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public void Dispose()
        {
            bitmap.Dispose();
        }
    }
}
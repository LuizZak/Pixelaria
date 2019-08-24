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
using System.Linq;
using System.Runtime.InteropServices;
using Blend2DCS;
using FastBitmapLib;
using JetBrains.Annotations;
using PixRendering;

namespace PixDirectX.Rendering.Blend2D
{
    public class Blend2DImageResources: IDisposable, IImageResourceManager
    {
        private readonly Dictionary<string, BLImage> _bitmapResources = new Dictionary<string, BLImage>();

        public void Dispose()
        {
            _bitmapResources.Clear();
        }

        public ImageResource AddImageResource(IRenderLoopState renderLoopState, Bitmap bitmap, string resourceName)
        {
            var res = new ImageResource(resourceName, bitmap.Width, bitmap.Height);

            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = CreateBLImage(bitmap);

            return res;
        }

        public IManagedImageResource CreateManagedImageResource(IRenderLoopState renderLoopState, Bitmap bitmap)
        {
            var blImage = CreateBLImage(bitmap);

            return new Blend2DBitmap(blImage);
        }

        public void UpdateManagedImageResource(IRenderLoopState renderLoopState, ref IManagedImageResource managedImage, Bitmap bitmap)
        {
            if (!(managedImage is Blend2DBitmap dxBitmap))
                throw new ArgumentException($"Expected bitmap to be of type ${typeof(Blend2DBitmap)}");

            dxBitmap.Bitmap = CreateBLImage(bitmap);
        }

        public void RemoveAllImageResources()
        {
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
                    new ImageResource(pair.Key, pair.Value.Size.Width, pair.Value.Size.Height)
                ).ToArray();
        }

        public ImageResource? GetImageResource(string resourceName)
        {
            var res = BitmapForResource(resourceName);
            if (res != null)
                return new ImageResource(resourceName, res.Size.Width, res.Size.Height);

            return null;
        }

        [CanBeNull]
        public BLImage BitmapForResource(ImageResource resource)
        {
            return BitmapForResource(resource.ResourceName);
        }

        [CanBeNull]
        public BLImage BitmapForResource([NotNull] string name)
        {
            return _bitmapResources.TryGetValue(name, out var bitmap) ? bitmap : null;
        }

        private static BLImage CreateBLImage([NotNull] Bitmap bitmap)
        {
            var image = new BLImage(bitmap.Width, bitmap.Height, BLFormat.Prgb32);
            var imageData = image.GetData();

            using (var fastBitmap = bitmap.FastLock(FastBitmapLockFormat.Format32bppPArgb))
            {
                Marshal.Copy(fastBitmap.DataArray, 0, imageData.PixelData, fastBitmap.Width * fastBitmap.Height);
            }

            return image;
        }
    }

    public class Blend2DBitmap : IManagedImageResource
    {
        internal BLImage Bitmap;

        public int Width => Bitmap.Size.Width;
        public int Height => Bitmap.Size.Height;
        public Size Size => new Size(Width, Height);

        public Blend2DBitmap(BLImage bitmap)
        {
            this.Bitmap = bitmap;
        }

        public void Dispose()
        {
            
        }
    }
}

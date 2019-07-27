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
using JetBrains.Annotations;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiImageResourceManager : IImageResourceManager
    {
        private readonly Dictionary<string, Bitmap> _bitmapResources = new Dictionary<string, Bitmap>();

        public ImageResource? GetImageResource(string resourceName)
        {
            var res = BitmapForResource(resourceName);
            if (res != null)
                return new ImageResource(resourceName, res.Width, res.Height);

            return null;
        }

        public ImageResource AddImageResource(IRenderLoopState state, Bitmap bitmap, string resourceName)
        {
            var res = new ImageResource(resourceName, bitmap.Width, bitmap.Height);

            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = bitmap;

            return res;
        }

        public IManagedImageResource CreateManagedImageResource(IRenderLoopState state, Bitmap bitmap)
        {
            var managed = new ManagedBitmap(new Bitmap(bitmap, bitmap.Width, bitmap.Height));
            return managed;
        }

        public void UpdateManagedImageResource(IRenderLoopState state, ref IManagedImageResource managedImage, Bitmap bitmap)
        {
            if (!(managedImage is ManagedBitmap managedBitmap))
                throw new ArgumentException($"Expected bitmap to be of type ${typeof(ManagedBitmap)}");

            managedBitmap.bitmap.Dispose();
            managedBitmap.bitmap = new Bitmap(bitmap, bitmap.Width, bitmap.Height);
        }

        public void RemoveImageResource(string resourceName)
        {
            if (_bitmapResources.ContainsKey(resourceName))
            {
                _bitmapResources.Remove(resourceName);
            }
        }

        public void RemoveAllImageResources()
        {
            foreach (var value in _bitmapResources.Values)
            {
                value.Dispose();
            }

            _bitmapResources.Clear();
        }

        [CanBeNull]
        public Bitmap BitmapForResource(ImageResource resource)
        {
            return BitmapForResource(resource.ResourceName);
        }

        [CanBeNull]
        public Bitmap BitmapForResource([NotNull] string name)
        {
            return _bitmapResources.TryGetValue(name, out var bitmap) ? bitmap : null;
        }

        internal class ManagedBitmap : IManagedImageResource
        {
            internal Bitmap bitmap;

            public int Width => bitmap.Width;
            public int Height => bitmap.Height;

            public ManagedBitmap(Bitmap bitmap)
            {
                this.bitmap = bitmap;
            }

            public void Dispose()
            {
                bitmap.Dispose();
            }
        }
    }
}
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
using Pixelaria.Views.ModelViews.PipelineView;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Helper class for dealing with Direct2D image resource loading
    /// </summary>
    public sealed class D2DImageResources : IDisposable, ID2DImageResourceManager
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

        public void AddImageResource(Direct2DRenderingState state, Bitmap bitmap, string resourceName)
        {
            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = Direct2DRenderer.CreateSharpDxBitmap(state.D2DRenderTarget, bitmap);
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

        public PipelineNodeView.ImageResource AddPipelineNodeImageResource(Direct2DRenderingState state,
            Bitmap bitmap, string resourceName)
        {
            var res = new PipelineNodeView.ImageResource(resourceName, bitmap.Width, bitmap.Height);

            AddImageResource(state, bitmap, resourceName);

            return res;
        }

        public PipelineNodeView.ImageResource? PipelineNodeImageResource(string resourceName)
        {
            var res = ImageResource(resourceName);
            if (res != null)
                return new PipelineNodeView.ImageResource(resourceName, res.PixelSize.Width, res.PixelSize.Height);

            return null;
        }

        [CanBeNull]
        public SharpDX.Direct2D1.Bitmap ImageResource([NotNull] string named)
        {
            return _bitmapResources.TryGetValue(named, out SharpDX.Direct2D1.Bitmap bitmap) ? bitmap : null;
        }
    }
}
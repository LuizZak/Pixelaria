using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using PixDirectX.Rendering.DirectX;
using PixRendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace PixDirectX.Rendering.Skia
{
    public sealed class SkiaImageResources : IDisposable, IImageResourceManager
    {
        private readonly Dictionary<string, SKBitmap> _bitmapResources = new Dictionary<string, SKBitmap>();

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
            var res = new ImageResource(resourceName, bitmap.Width, bitmap.Height);

            if (_bitmapResources.ContainsKey(resourceName))
                throw new ArgumentException($@"An image resource named '{resourceName}' already exists.", nameof(resourceName));

            _bitmapResources[resourceName] = bitmap.ToSKBitmap();

            return res;
        }

        public IManagedImageResource CreateManagedImageResource(IRenderLoopState renderLoopState, Bitmap bitmap)
        {
            return new SkiaBitmap(bitmap.ToSKBitmap());
        }

        public void UpdateManagedImageResource(IRenderLoopState renderLoopState, ref IManagedImageResource managedImage, Bitmap bitmap)
        {
            if (!(managedImage is SkiaBitmap skBitmap))
                throw new ArgumentException($"Expected bitmap to be of type ${typeof(SkiaBitmap)}");

            skBitmap.bitmap.Dispose();
            skBitmap.bitmap = bitmap.ToSKBitmap();
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
                    new ImageResource(pair.Key, pair.Value.Width, pair.Value.Height)
                ).ToArray();
        }

        public ImageResource? GetImageResource(string resourceName)
        {
            var res = BitmapForResource(resourceName);
            if (res != null)
                return new ImageResource(resourceName, res.Width, res.Height);

            return null;
        }

        [CanBeNull]
        public SKBitmap BitmapForResource(ImageResource resource)
        {
            return BitmapForResource(resource.ResourceName);
        }

        [CanBeNull]
        public SKBitmap BitmapForResource([NotNull] string name)
        {
            return _bitmapResources.TryGetValue(name, out var bitmap) ? bitmap : null;
        }
    }

    public class SkiaBitmap : IManagedImageResource
    {
        internal SKBitmap bitmap;

        public int Width => bitmap.Width;
        public int Height => bitmap.Height;
        public Size Size => new Size(Width, Height);

        public SkiaBitmap(SKBitmap bitmap)
        {
            this.bitmap = bitmap;
        }

        public void Dispose()
        {
            bitmap.Dispose();
        }
    }
}

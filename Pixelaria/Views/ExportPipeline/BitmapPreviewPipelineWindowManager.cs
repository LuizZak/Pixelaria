
#if false

using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using PixCore.Geometry;
using PixRendering;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// A 'picture-in-picture' style manager for bitmap preview pipeline steps.
    /// </summary>
    internal class BitmapPreviewPipelineWindowManager : ExportPipelineUiFeature, IRenderListener
    {
        [CanBeNull]
        private IRenderLoopState _latestRenderState;

        private readonly Font _font = new Font(FontFamily.GenericSansSerif, 12);

        private readonly List<BitmapPreviewPipelineStep> _previewSteps = new List<BitmapPreviewPipelineStep>();
        private readonly Dictionary<BitmapPreviewPipelineStep, IManagedImageResource> _latestPreviews = new Dictionary<BitmapPreviewPipelineStep, IManagedImageResource>();
        private readonly List<PreviewBounds> _previewBounds = new List<PreviewBounds>();
        private readonly InsetBounds _titleInset = new InsetBounds(5, 5, 5, 5);
        private InsetBounds _screenInsetBounds = new InsetBounds(5, 5, 5, 5);

        public InsetBounds ScreenInsetBounds
        {
            get => _screenInsetBounds;
            set
            {
                if (_screenInsetBounds == value)
                    return;

                _screenInsetBounds = value;
                ReloadBoundsCache();
            }
        }

        public int RenderOrder => RenderOrdering.UserInterface;

        public BitmapPreviewPipelineWindowManager([NotNull] IExportPipelineControl control, [NotNull] IRenderManager renderManager) : base(control)
        {
            control.PipelineContainer.NodeAdded += PipelineContainerOnNodeAdded;
            control.PipelineContainer.NodeRemoved += PipelineContainerOnNodeRemoved;
            control.SizeChanged += ControlOnSizeChanged;

            renderManager.AddRenderListener(this);
        }

        private void ControlOnSizeChanged(object sender, EventArgs e)
        {
            ReloadBoundsCache();
        }

        ~BitmapPreviewPipelineWindowManager()
        {
            foreach (var bitmap in _latestPreviews.Values)
            {
                bitmap?.Dispose();
            }

            _font.Dispose();
        }
        
        private void PipelineContainerOnNodeAdded(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.PipelineNode is BitmapPreviewPipelineStep step))
                return;

            AddPreview(step, e.Control);
        }

        private void PipelineContainerOnNodeRemoved(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.PipelineNode is BitmapPreviewPipelineStep step))
                return;

            RemovePreview(step, e.Control);
        }

        private void OnBitmapStepOnRenamed(object sender, EventArgs args)
        {
            ReloadBoundsCache();
            Control.InvalidateAll();
        }

        private void AddPreview([NotNull] BitmapPreviewPipelineStep step, [NotNull] IExportPipelineControl control)
        {
            _previewSteps.Add(step);
            _latestPreviews[step] = null;

            ReloadBoundsCache();

            step.OnReceive = bitmap =>
            {
                UpdatePreview(step, bitmap, control);
            };

            step.Renamed += OnBitmapStepOnRenamed;

            control.InvalidateAll();
        }

        private void RemovePreview([NotNull] BitmapPreviewPipelineStep step, [NotNull] IExportPipelineControl control)
        {
            control.InvalidateAll();

            step.Renamed -= OnBitmapStepOnRenamed;

            _previewSteps.Remove(step);
            _latestPreviews[step]?.Dispose();
            _latestPreviews.Remove(step);

            ReloadBoundsCache();
        }

        private void UpdatePreview([NotNull] BitmapPreviewPipelineStep step, Bitmap bitmap, [NotNull] IExportPipelineControl control)
        {
            if (_latestRenderState == null)
                return;

            IManagedImageResource managedBitmap;

            if (_latestPreviews.TryGetValue(step, out var old))
            {
                managedBitmap = old;
                control.ImageResources.UpdateManagedImageResource(_latestRenderState, ref managedBitmap, bitmap);
            }
            else
            {
                managedBitmap = control.ImageResources.CreateManagedImageResource(_latestRenderState, bitmap);
            }

            _latestPreviews[step] = managedBitmap;
            
            ReloadBoundsCache();

            control.InvalidateAll();
        }

        public void RecreateState(IRenderLoopState state)
        {
            _latestRenderState = state;
        }

        public void Render(IRenderListenerParameters parameters)
        {
            var state = parameters.State;

            _latestRenderState = state;

            for (int i = 0; i < _previewSteps.Count; i++)
            {
                var step = _previewSteps[i];

                var bounds = BoundsForPreview(i);

                _latestPreviews.TryGetValue(step, out var bitmap);

                // Draw image, or opaque background
                if (bitmap != null)
                {
                    parameters.Renderer.DrawBitmap(bitmap, bounds.ImageBounds, 1, ImageInterpolationMode.Linear);
                }
                else
                {
                    parameters.Renderer.SetFillColor(Color.DimGray);
                    parameters.Renderer.FillArea(bounds.ImageBounds);
                }

                parameters.Renderer.SetStrokeColor(Color.Gray);
                parameters.Renderer.StrokeArea(bounds.ImageBounds);

                // Draw title
                var format = new TextFormatAttributes(_font.FontFamily.Name, _font.Size)
                {
                    TextEllipsisTrimming = new TextEllipsisTrimming
                    {
                        Granularity = TextTrimmingGranularity.Character
                    }
                };

                parameters.Renderer.SetFillColor(Color.Black);
                parameters.Renderer.FillArea(bounds.TitleBounds);

                parameters.TextRenderer.Draw(step.Name, format, bounds.TitleBounds.Inset(_titleInset), Color.White);
            }
        }

        private void ReloadBoundsCache()
        {
            _previewBounds.Clear();
            
            float y = 0;

            foreach (var step in _previewSteps)
            {
                string name = step.Name;

                var nameSize = Control.TextSizeProvider.CalculateTextSize(name, _font);
                nameSize.Width += _titleInset.Left + _titleInset.Right;
                nameSize.Height += _titleInset.Top + _titleInset.Bottom;

                _latestPreviews.TryGetValue(step, out var bitmap);

                var size = new Vector(120, 90);
                if (bitmap != null)
                    size = new Vector(120 * ((float) bitmap.Width / bitmap.Height), 90);

                var availableBounds = 
                    AABB.FromRectangle(Vector.Zero, Control.Size)
                        .Inset(ScreenInsetBounds);

                var titleBounds = AABB.FromRectangle(availableBounds.Width - size.X,
                    availableBounds.Height - y - size.Y - nameSize.Height, 
                    size.X,
                    nameSize.Height);

                var bounds = AABB.FromRectangle(availableBounds.Width - size.X, 
                    availableBounds.Height - y - size.Y,
                    size.X,
                    size.Y);

                var previewBounds = new PreviewBounds(titleBounds, bounds);

                y += previewBounds.TotalBounds.Height + 5;

                _previewBounds.Add(previewBounds);
            }
        }

        private PreviewBounds BoundsForPreview(int index)
        {
            return _previewBounds[index];
        }

        private struct PreviewBounds
        {
            public AABB TotalBounds => TitleBounds.Union(ImageBounds);
            public AABB TitleBounds { get; }
            public AABB ImageBounds { get; }

            public PreviewBounds(AABB titleBounds, AABB imageBounds)
            {
                TitleBounds = titleBounds;
                ImageBounds = imageBounds;
            }
        }
    }
}
#endif
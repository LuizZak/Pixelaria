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
using PixCore.Geometry;
using Pixelaria.ExportPipeline;
using PixPipelineGraph;
using PixRendering;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// A 'picture-in-picture' style manager for bitmap preview pipeline steps.
    /// </summary>
    internal class BitmapPreviewPipelineWindowManager : ExportPipelineUiFeature, IRenderListener
    {
        private readonly Font _font = new Font(FontFamily.GenericSansSerif, 12);

        private readonly List<IPipelineNodeView> _previewSteps = new List<IPipelineNodeView>();
        private readonly Dictionary<IPipelineNodeView, IManagedImageResource> _latestPreviews = new Dictionary<IPipelineNodeView, IManagedImageResource>();
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
            if (!(e.Node.NodeView.NodeKind == PipelineNodeKinds.BitmapPreview))
                return;

            AddPreview(e.Node.NodeView, e.Control);
        }

        private void PipelineContainerOnNodeRemoved(object sender, [NotNull] PipelineNodeViewEventArgs e)
        {
            if (!(e.Node.NodeView.NodeKind == PipelineNodeKinds.BitmapPreview))
                return;

            RemovePreview(e.Node.NodeView, e.Control);
        }

        private void OnBitmapStepOnRenamed(object sender, EventArgs args)
        {
            ReloadBoundsCache();
            Control.InvalidateAll();
        }

        private void AddPreview([NotNull] IPipelineNodeView step, [NotNull] IExportPipelineControl control)
        {
            _previewSteps.Add(step);
            _latestPreviews[step] = null;

            ReloadBoundsCache();

            // TODO: Enable value observing
            //step.OnReceive = bitmap =>
            //{
            //    UpdatePreview(step, bitmap, control);
            //};

            // TODO: Enable title renaming observing
            //step.Renamed += OnBitmapStepOnRenamed;

            control.InvalidateAll();
        }

        private void RemovePreview([NotNull] IPipelineNodeView step, [NotNull] IExportPipelineControl control)
        {
            control.InvalidateAll();

            // TODO: Enable title renaming observing
            //step.Renamed -= OnBitmapStepOnRenamed;

            _previewSteps.Remove(step);
            _latestPreviews[step]?.Dispose();
            _latestPreviews.Remove(step);

            ReloadBoundsCache();
        }

        private void UpdatePreview([NotNull] IPipelineNodeView nodeId, [NotNull] Bitmap bitmap, [NotNull] IExportPipelineControl control)
        {
            IManagedImageResource managedBitmap;

            if (_latestPreviews.TryGetValue(nodeId, out var old))
            {
                managedBitmap = old;
                control.ImageResources.UpdateManagedImageResource(ref managedBitmap, bitmap);
            }
            else
            {
                managedBitmap = control.ImageResources.CreateManagedImageResource(bitmap);
            }

            _latestPreviews[nodeId] = managedBitmap;
            
            ReloadBoundsCache();

            control.InvalidateAll();
        }

        public void RecreateState(IRenderLoopState state)
        {

        }

        public void Render(IRenderListenerParameters parameters)
        {
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

                parameters.TextRenderer.Draw(step.Title, format, bounds.TitleBounds.Inset(_titleInset), Color.White);
            }
        }

        private void ReloadBoundsCache()
        {
            _previewBounds.Clear();
            
            float y = 0;

            foreach (var step in _previewSteps)
            {
                string name = step.Title;

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

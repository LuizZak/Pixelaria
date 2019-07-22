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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using Pixelaria.ExportPipeline;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI;
using PixUI.Controls;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;
using PixelFormat = SharpDX.WIC.PixelFormat;

namespace Pixelaria.Views.ExportPipeline.PipelineNodePanel
{
    internal sealed partial class ExportPipelineNodesPanelManager
    {
        /// <summary>
        /// Specifies the action for a pipeline node drag and drop when the node is hovered over a point on screen.
        ///
        /// Used when the user starts dragging a pipeline node button.
        /// </summary>
        private enum PipelineNodeDragAndDropAction
        {
            Create,
            Delete
        }

        private interface IPipelineNodeButtonDragAndDropHandlerDelegate
        {
            PipelineNodeDragAndDropAction ActionForDropPoint(PipelineNodeButtonDragAndDropHandler handler, Vector screenPoint);
        }

        private sealed class PipelineNodeButtonDragAndDropHandler: IDisposable
        {
            private const float DistanceToDrag = 5;

            [NotNull] 
            private readonly IExportPipelineDirect2DRenderer _pipelineDirect2DRenderer;

            [NotNull] 
            private readonly IInvalidatableControl _invalidatableControl;

            private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
            private ButtonControl _buttonControl;
            private readonly PipelineNodeSpec _nodeSpec;
            [NotNull] 
            private readonly IPipelineNodeButtonDragAndDropHandlerDelegate _delegate;

            public Action<Vector> MouseUp { private get; set; }

            public PipelineNodeButtonDragAndDropHandler([NotNull] ButtonControl buttonControl,
                [NotNull] PipelineNodeSpec nodeSpec,
                [NotNull] IInvalidatableControl invalidatableControl,
                [NotNull] IExportPipelineDirect2DRenderer pipelineDirect2DRenderer, 
                [NotNull] IPipelineNodeButtonDragAndDropHandlerDelegate @delegate)
            {
                _buttonControl = buttonControl;
                _nodeSpec = nodeSpec;
                _invalidatableControl = invalidatableControl;
                _pipelineDirect2DRenderer = pipelineDirect2DRenderer;
                _delegate = @delegate;

                Setup(buttonControl);
            }

            public void Dispose()
            {
                _disposeBag?.Dispose();
            }

            void Setup([NotNull] ButtonControl button)
            {
                void InvalidateScreen(PipelineNodeDragRenderListener dragListener)
                {
                    _invalidatableControl.InvalidateRegion(new RedrawRegion(dragListener.NodeScreenArea, null));
                }

                PipelineNodeDragAndDropAction ActionForDropPoint(Vector screenPoint)
                {
                    return _delegate.ActionForDropPoint(this, screenPoint);
                }

                var renderListener =
                    new PipelineNodeDragRenderListener(_nodeSpec, _pipelineDirect2DRenderer.ImageResources);

                _disposeBag.Add(renderListener);

                var pressPoint = Vector.Zero;
                var isDraggingPreview = false;

                var onPress = button.Rx.IsMouseDown().DistinctUntilChanged().Where(b => b).PxlWithLatestFrom(button.Rx.MouseMove, (b, args) => args);
                var onRelease = button.Rx.IsMouseDown().DistinctUntilChanged().Where(b => !b).PxlWithLatestFrom(button.Rx.MouseMove, (b, args) => args);
                var onHoverOutWhileDragging =
                    button.Rx.MouseMove
                        .PxlWithLatestFrom(button.Rx.IsMouseDown(), (args, b) => (args, b))
                        .Where(tuple => tuple.b)
                        .Select(tuple => tuple.args);
                var onHoverOnWhileDragging =
                    button.Rx.MouseMove
                        .PxlWithLatestFrom(button.Rx.IsMouseDown(), (args, b) => (args, b))
                        .Where(tuple => tuple.b && button.Bounds.Contains(tuple.args.Location))
                        .Select(tuple => tuple.args);

                // On press - register render listener
                onPress
                    .Subscribe(args =>
                    {
                        pressPoint = args.Location;
                    }).AddToDisposable(_disposeBag);

                // On release - remove render listener
                onRelease
                    .Subscribe(args =>
                    {
                        if (isDraggingPreview)
                        {
                            var screenPoint = button.ConvertTo(args.Location, null);

                            InvalidateScreen(renderListener);

                            switch (ActionForDropPoint(screenPoint))
                            {
                                case PipelineNodeDragAndDropAction.Create:
                                    MouseUp(screenPoint - renderListener.NodeScreenArea.Size / 2);
                                    break;
                            }

                            _pipelineDirect2DRenderer.RemoveRenderListener(renderListener);
                        }

                        isDraggingPreview = false;
                    }).AddToDisposable(_disposeBag);

                // As the user moves the mouse while out of the control
                onHoverOutWhileDragging
                    .Subscribe(e =>
                    {
                        if (!isDraggingPreview && pressPoint.Distance(e.Location) > DistanceToDrag)
                        {
                            isDraggingPreview = true;

                            _pipelineDirect2DRenderer.AddRenderListener(renderListener);
                        }

                        if (isDraggingPreview)
                        {
                            InvalidateScreen(renderListener);

                            var screenPoint = button.ConvertTo(e.Location, null);

                            renderListener.MousePosition = screenPoint;

                            switch (ActionForDropPoint(screenPoint))
                            {
                                case PipelineNodeDragAndDropAction.Create:
                                    renderListener.Visible = true;
                                    break;

                                case PipelineNodeDragAndDropAction.Delete:
                                    renderListener.Visible = true;
                                    break;
                            }

                            InvalidateScreen(renderListener);
                        }
                    }).AddToDisposable(_disposeBag);

                // As the user enters the mouse on top of the control while dragging
                onHoverOnWhileDragging
                    .Subscribe(_ =>
                    {
                        if (isDraggingPreview)
                        {
                            renderListener.Visible = false;

                            InvalidateScreen(renderListener);
                        }
                    }).AddToDisposable(_disposeBag);
            }

            private sealed class PipelineNodeDragRenderListener : IRenderListener, IDisposable
            {
                public int RenderOrder { get; } = RenderOrdering.UserInterface + 10;

                public bool Visible { get; set; }
                
                public Vector MousePosition
                {
                    set => _nodeView.Location = Vector.Round(value - _nodeView.Size / 2);
                }

                /// <summary>
                /// The area the node occupies in screen-space
                /// </summary>
                public AABB NodeScreenArea => _nodeView.Bounds.TransformedBounds(_nodeView.GetAbsoluteTransform());

                private readonly PipelineNodeView _nodeView;

                public PipelineNodeDragRenderListener([NotNull] PipelineNodeSpec nodeSpec, [NotNull] IImageResourceProvider imageProvider)
                {
                    var node = nodeSpec.CreateNode();
                    _nodeView = PipelineNodeView.Create(node);
                    _nodeView.Icon = IconForPipelineNode(node, imageProvider);

                    var nodeViewSizer = new DefaultPipelineNodeViewSizer();
                    nodeViewSizer.AutoSize(_nodeView, LabelView.DefaultLabelViewSizeProvider);
                }

                public void Dispose()
                {

                }

                public void RecreateState(IRenderLoopState state)
                {

                }

                public void Render(IRenderListenerParameters parameters)
                {
                    if (!Visible)
                        return;

                    parameters.Renderer.PushingTransform(() =>
                    {
                        var renderer = new InternalNodeViewRenderer(_nodeView, parameters, true);

                        renderer.RenderView(new IRenderingDecorator[0]);
                    });
                }
            }
        }
    }
}
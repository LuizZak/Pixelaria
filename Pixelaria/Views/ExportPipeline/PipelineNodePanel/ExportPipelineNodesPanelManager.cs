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
using System.Reactive.Disposables;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Colors;
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Rendering.DirectX;
using PixDirectX.Utils;
using Pixelaria.DXSupport;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixPipelineGraph;
using PixRendering;
using PixUI;
using PixUI.Controls;
using SharpDX.WIC;
using Bitmap = SharpDX.WIC.Bitmap;
using Color = System.Drawing.Color;

namespace Pixelaria.Views.ExportPipeline.PipelineNodePanel
{
    /// <summary>
    /// Manages export pipeline node items available on the side panel of an export pipeline view
    /// </summary>
    internal sealed partial class ExportPipelineNodesPanelManager: IDisposable
    {
        private readonly List<PipelineNodeButtonDragAndDropHandler> _buttonHandlers = new List<PipelineNodeButtonDragAndDropHandler>();

        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        private readonly IRenderManager _pipelineRenderManager;
        [NotNull] 
        private readonly IInvalidatableControl _invalidateTarget;

        private readonly IPipelineContainer _pipelineContainer;
        private readonly IImageResourceProvider _imageResourceProvider;
        private readonly IPipelineNodeBitmapGenerator _bitmapGenerator;

        private List<PipelineNodeDescriptor> LoadedDescriptors { get; } = new List<PipelineNodeDescriptor>();
        private List<ButtonControl> SpecButtons { get; } = new List<ButtonControl>();

        private ControlView _container;
        private SearchBarControl _searchBar;
        private ScrollViewControl _scrollViewControl;

        public delegate void PipelineNodeSelectedEventHandler(object sender, PipelineNodeSelectedEventArgs e);
        public delegate void DeletePipelineNodeEventHandler(object sender, DeletePipelineNodeEventArgs e);

        public event PipelineNodeSelectedEventHandler PipelineNodeSelected;
        public event DeletePipelineNodeEventHandler DeletePipelineNode;

        public ExportPipelineNodesPanelManager([NotNull] IExportPipelineControl control, [NotNull] IRenderManager renderManager)
            : this(control.ControlContainer, renderManager, control, control.PipelineContainer, new PipelineNodeBitmapGenerator(control))
        {
            
        }

        public ExportPipelineNodesPanelManager([NotNull] IControlContainer container, [NotNull] IRenderManager pipelineRenderManager, [NotNull] IInvalidatableControl invalidateTarget, IPipelineContainer pipelineContainer, [NotNull] IPipelineNodeBitmapGenerator bitmapGenerator)
        {
            _pipelineRenderManager = pipelineRenderManager;
            _invalidateTarget = invalidateTarget;
            _pipelineContainer = pipelineContainer;
            _imageResourceProvider = _pipelineRenderManager.ImageResources;
            _bitmapGenerator = bitmapGenerator;

            Setup(container);
        }
        
        public void Dispose()
        {
            foreach (var handler in _buttonHandlers)
            {
                handler.Dispose();
            }

            _buttonHandlers.Clear();
            _disposeBag?.Dispose();
        }

        public void RegisterResizeEvent([NotNull] Control control)
        {
            control.SizeChanged += (sender, args) =>
            {
                AdjustSize();
            };

            AdjustSize();
        }

        private void Setup([NotNull] IControlContainer container)
        {
            container.AddControl(_container = new ControlView());

            _container.Size = new Vector(300, _container.Parent?.Size.Y ?? 0);
            _container.BackColor = Color.Black.WithTransparency(0.7f);

            _searchBar = SearchBarControl.Create();
            _searchBar.PlaceholderText = "Search node";

            _scrollViewControl = ScrollViewControl.Create();
            _scrollViewControl.Location = new Vector(0, 50);
            _scrollViewControl.Size = new Vector(300, _container.Parent?.Size.Y ?? 0);
            _scrollViewControl.ContentSize = new Vector(0, 1800);
            _scrollViewControl.BackColor = Color.Transparent;
            _scrollViewControl.ScrollBarsMode = ScrollViewControl.VisibleScrollBars.Vertical;

            _container.AddChild(_scrollViewControl);
            _container.AddChild(_searchBar);
            
            AdjustSize();

            SetupReactiveSearch();
        }

        private void SetupReactiveSearch()
        {
            _searchBar
                .RxSearchTextUpdated
                .Subscribe(s =>
                {
                    var buttonPairs = SpecButtons.Zip(LoadedDescriptors, (control, spec) => (control, spec)).ToArray();

                    foreach (var (button, descriptor) in buttonPairs)
                    {
                        button.Visible = false;
                        button.Text = descriptor.Title;
                    }

                    var visible = s == "" ? buttonPairs : buttonPairs.Where(b => b.Item1.Text.ToLower().Contains(s.ToLower())).ToArray();

                    var highlightAttribute = new BackgroundColorAttribute(Color.CornflowerBlue.WithTransparency(0.6f), new Vector(2, 2));

                    foreach (var (button, descriptor) in visible)
                    {
                        button.Visible = true;
                        if (s == "")
                            continue;

                        int index = descriptor.Title.IndexOf(s, StringComparison.InvariantCultureIgnoreCase);
                        button.AttributedText.SetAttributes(new TextRange(index, s.Length), highlightAttribute);
                    }
                    
                    ArrangeButtons(visible.Select(p => p.Item1).ToArray());
                }).AddToDisposable(_disposeBag);
        }

        private void AdjustSize()
        {
            _container.Size = new Vector(300, _container.Parent?.Size.Y ?? 0);

            var textFieldBounds = new AABB(0, 0, 40, _container.Size.X);

            _searchBar.Location = textFieldBounds.Minimum;
            _searchBar.Size = textFieldBounds.Size;

            var scrollViewBounds = _container.Bounds;
            scrollViewBounds = scrollViewBounds.Inset(new InsetBounds(0, 40, 0, 0));

            _scrollViewControl.Location = scrollViewBounds.Minimum;
            _scrollViewControl.Size = scrollViewBounds.Size;
        }

        /// <summary>
        /// Loads a list of pipeline nodes that can be created from the given array of pipeline node specs
        /// </summary>
        public void LoadCreatablePipelineNodes([NotNull] PipelineNodeDescriptor[] nodeDescriptors)
        {
            LoadedDescriptors.AddRange(nodeDescriptors);

            foreach (var spec in nodeDescriptors)
            {
                var button = ButtonForNodeDescriptor(spec);
                button.Tag = spec;

                SpecButtons.Add(button);

                _scrollViewControl.ContainerView.AddChild(button);

                var handler = DragAndDropHandlerForButton(button, spec);
                _buttonHandlers.Add(handler);
                _disposeBag.Add(handler);

                handler.MouseUp = (mousePosition, action) =>
                {
                    switch (action)
                    {
                        case PipelineNodeDragAndDropAction.Create:
                            if (mousePosition == null)
                            {
                                var center = (Vector)_pipelineContainer.ScreenSize / 2;
                                PipelineNodeSelected?.Invoke(this, new PipelineNodeSelectedEventArgs(spec, center));
                            }
                            else if (!_container.Contains(mousePosition.Value))
                            {
                                PipelineNodeSelected?.Invoke(this, new PipelineNodeSelectedEventArgs(spec, mousePosition.Value));
                            }

                            break;

                        case PipelineNodeDragAndDropAction.Delete:
                            
                            break;
                    }
                };
            }

            // Adjust buttons
            ArrangeButtons(SpecButtons);
        }

        private void ArrangeButtons([NotNull] IReadOnlyList<ButtonControl> buttons)
        {
            var buttonSize = GetButtonSize();
            var sepSize = new Vector(15, 10);

            float minCellWidth = buttonSize.X + sepSize.X / 2;

            int buttonsPerRow = (int)(_scrollViewControl.VisibleContentBounds.Width / minCellWidth);

            float xStep = _scrollViewControl.VisibleContentBounds.Width / buttonsPerRow;
            float yStep = buttonSize.Y + sepSize.Y;

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];

                float x = xStep / 2 + i % buttonsPerRow * xStep;
                float y = yStep / 2 + (float)Math.Floor((float)i / buttonsPerRow) * yStep;

                button.Center = new Vector(x, y);
            }
        }

        private ButtonControl ButtonForNodeDescriptor([NotNull] PipelineNodeDescriptor descriptor)
        {
            var buttonSize = GetButtonSize();

            var button = ButtonControl.Create();
            button.Size = buttonSize;
            button.Text = descriptor.Title;
            button.BackColor = Color.Black.WithTransparency(0.3f);
            button.NormalColor = Color.Black.WithTransparency(0.3f);
            button.HighlightColor = Color.Black.WithTransparency(0.3f).BlendedOver(Color.White);
            button.SelectedColor = Color.Black.WithTransparency(0.3f);
            button.StrokeWidth = 2;
            button.CornerRadius = 3;
            button.StrokeColor = Color.Gray.WithTransparency(0.8f);
            button.TextColor = Color.White;
            button.ClipToBounds = false;
            button.HorizontalTextAlignment = HorizontalTextAlignment.Center;
            button.TextInset = new InsetBounds(5, 5, 5, 5);
            button.ImageInset = new InsetBounds(7, 0, 0, 0);
            //button.Image = IconForPipelineNodeType(spec.NodeType, _imageResourceProvider);
            button.TextFont = new Font(FontFamily.GenericSansSerif.Name, 12);

            return button;
        }

        private PipelineNodeButtonDragAndDropHandler DragAndDropHandlerForButton([NotNull] ButtonControl button, [NotNull] PipelineNodeDescriptor nodeDesc)
        {
            return new PipelineNodeButtonDragAndDropHandler(button, nodeDesc, _invalidateTarget,
                _pipelineRenderManager, new PipelineNodeButtonDragAndDropHandlerDelegate(this));
        }

        private Vector GetButtonSize()
        {
            return new Vector(_scrollViewControl.VisibleContentBounds.Width / 2 - 20, 40);
        }

#if false

        /// <summary>
        /// Gets an image resource from a given image resources provider that matches the given pipeline node.
        /// 
        /// This image is used for representing the node's type visually in a small icon form.
        /// </summary>
        public static ImageResource? IconForPipelineNode([NotNull] IPipelineNode node, [NotNull] IImageResourceProvider resourcesProvider)
        {
            return IconForPipelineNodeType(node.GetType(), resourcesProvider);
        }

        /// <summary>
        /// Gets an image resource from a given image resources provider that matches the given pipeline node.
        /// 
        /// This image is used for representing the node's type visually in a small icon form.
        /// </summary>
        public static ImageResource? IconForPipelineNodeType(Type nodeType, [NotNull] IImageResourceProvider resourcesProvider)
        {
            string iconName = null;

            // Automatically setup icons for known pipeline nodes
            if (nodeType == typeof(TransparencyFilterPipelineStep))
            {
                iconName = "filter_transparency_icon";
            }
            else if (nodeType == typeof(FilterPipelineStep<HueFilter>))
            {
                iconName = "filter_hue";
            }
            else if (nodeType == typeof(FilterPipelineStep<SaturationFilter>))
            {
                iconName = "filter_saturation";
            }
            else if (nodeType == typeof(FilterPipelineStep<LightnessFilter>))
            {
                iconName = "filter_lightness";
            }
            else if (nodeType == typeof(FilterPipelineStep<OffsetFilter>))
            {
                iconName = "filter_offset_icon";
            }
            else if (nodeType == typeof(FilterPipelineStep<RotationFilter>))
            {
                iconName = "filter_rotation_icon";
            }
            else if (nodeType == typeof(FilterPipelineStep<ScaleFilter>))
            {
                iconName = "filter_scale_icon";
            }
            else if (nodeType == typeof(FilterPipelineStep<StrokeFilter>))
            {
                iconName = "filter_stroke";
            }
            else if (nodeType == typeof(SingleAnimationPipelineStep))
            {
                iconName = "anim_icon";
            }
            else if (nodeType == typeof(FileExportPipelineStep))
            {
                iconName = "sheet_save_icon";
            }
            else if (nodeType == typeof(SpriteSheetGenerationPipelineStep))
            {
                iconName = "sheet_new";
            }

            return iconName != null ? resourcesProvider.GetImageResource(iconName) : null;
        }

#endif

        /// <summary>
        /// Arguments for event fired when user selects a pipeline node from the pipeline node panels.
        /// 
        /// Receives the pre-instantiated pipeline node for the event.
        /// </summary>
        public class PipelineNodeSelectedEventArgs : EventArgs
        {
            public PipelineNodeDescriptor NodeDescriptor { get; }
            public Vector? ScreenPosition { get; }

            public PipelineNodeSelectedEventArgs(PipelineNodeDescriptor nodeDescriptor, Vector? screenPosition)
            {
                NodeDescriptor = nodeDescriptor;
                ScreenPosition = screenPosition;
            }
        }

        /// <summary>
        /// Arguments for event fired when user drags an existing pipeline node to the pipeline node panels (delete action).
        /// 
        /// Receives the pipeline node for the event.
        /// </summary>
        public class DeletePipelineNodeEventArgs : EventArgs
        {
            public PipelineNodeId Node { get; }
            public Vector? ScreenPosition { get; }

            public DeletePipelineNodeEventArgs(PipelineNodeId node, Vector? screenPosition)
            {
                Node = node;
                ScreenPosition = screenPosition;
            }
        }
    }

    /// <summary>
    /// <see cref="PipelineNodeButtonDragAndDropHandler"/> extensions
    /// </summary>
    internal partial class ExportPipelineNodesPanelManager
    {
        private class PipelineNodeButtonDragAndDropHandlerDelegate : IPipelineNodeButtonDragAndDropHandlerDelegate
        {
            private readonly ExportPipelineNodesPanelManager _nodesPanelManager;

            public PipelineNodeButtonDragAndDropHandlerDelegate(ExportPipelineNodesPanelManager nodesPanelManager)
            {
                _nodesPanelManager = nodesPanelManager;
            }

            public PipelineNodeDragAndDropAction ActionForDropPoint(PipelineNodeButtonDragAndDropHandler handler, Vector screenPoint)
            {
                if (_nodesPanelManager._container.Contains(_nodesPanelManager._container.ConvertFromScreen(screenPoint)))
                {
                    return PipelineNodeDragAndDropAction.Delete;
                }

                return PipelineNodeDragAndDropAction.Create;
            }

            public Matrix2D TransformMatrixForDropPoint(PipelineNodeButtonDragAndDropHandler handler, Vector screenPoint)
            {
                return _nodesPanelManager._pipelineContainer.ContentsView.LocalTransform;
            }
        }
    }

    internal class PipelineNodeBitmapGenerator : IPipelineNodeBitmapGenerator
    {
        readonly IExportPipelineControl _exportPipelineControl;

        public PipelineNodeBitmapGenerator([NotNull] IExportPipelineControl exportPipelineControl)
        {
            _exportPipelineControl = exportPipelineControl;
        }

        public Bitmap BitmapForPipelineNode(PipelineNodeDescriptor node)
        {
            var container = _exportPipelineControl.PipelineContainer;
            const BitmapCreateCacheOption bitmapCreateCacheOption = BitmapCreateCacheOption.CacheOnDemand;
            var pixelFormat = PixelFormat.Format32bppPBGRA;

            var view = PipelineNodeView.Create(node, null);
            //view.Icon = ExportPipelineNodesPanelManager.IconForPipelineNode(node, _exportPipelineControl.ImageResources);

            container.AutoSizeNode(view);

            var margins = new Vector(2, 2);
            var bitmapOffset = margins;
            var bitmapSize = view.Size + margins * 2;

            // Automatically adjust view to be on center of view port
            using (var imgFactory = new ImagingFactory())
            {
                var wicBitmap = new Bitmap(imgFactory, (int)bitmapSize.X, (int)bitmapSize.Y, pixelFormat, bitmapCreateCacheOption);

                using (var renderLoop = new Direct2DWicBitmapRenderManager(wicBitmap, DxSupport.D2DFactory, DxSupport.D3DDevice))
                using (var renderer = new Direct2DRenderManager())
                {
                    var listener = new InternalRenderListener(container, _exportPipelineControl);

                    ControlView.TextLayoutRenderer = renderer;

                    var last = LabelView.defaultTextSizeProvider;
                    LabelView.defaultTextSizeProvider = renderer.TextSizeProvider;

                    renderer.ClippingRegion = new FullClippingRegion();

                    renderLoop.Initialize();
                    renderLoop.RenderSingleFrame(state =>
                    {
                        renderLoop.D2DRenderState.Transform = Matrix2D.Translation(bitmapOffset).ToRawMatrix3X2();
                        var parameters = renderer.CreateRenderListenerParameters(renderLoop.D2DRenderState);

                        listener.RenderStepView(view, parameters, new IRenderingDecorator[0]);
                    });

                    LabelView.defaultTextSizeProvider = last;

                    return wicBitmap;
                }
            }
        }
    }
}
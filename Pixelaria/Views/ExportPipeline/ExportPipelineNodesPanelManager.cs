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

using JetBrains.Annotations;

using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Steps;
using Pixelaria.Filters;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline.ExportPipelineFeatures;
using Pixelaria.Views.ExportPipeline.PipelineView;
using Pixelaria.Views.ExportPipeline.PipelineView.Controls;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Manages export pipeline node items available on the side panel of an export pipeline view
    /// </summary>
    internal sealed class ExportPipelineNodesPanelManager: IDisposable
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        private readonly ExportPipelineControl _pipelineControl;
        private readonly ControlViewFeature _controlView;

        private List<PipelineNodeSpec> LoadedSpecs { get; } = new List<PipelineNodeSpec>();
        private List<ButtonControl> SpecButtons { get; } = new List<ButtonControl>();

        private ScrollViewControl _scrollViewControl;

        public delegate void PipelineNodeSelectedEventHandler(object sender, PipelineNodeSelectedEventArgs e);

        public event PipelineNodeSelectedEventHandler PipelineNodeSelected;

        public ExportPipelineNodesPanelManager([NotNull] ExportPipelineControl pipelineControl, [NotNull] ControlViewFeature controlView)
        {
            _pipelineControl = pipelineControl;
            _controlView = controlView;

            Setup();
        }
        
        public void Dispose()
        {
            _disposeBag?.Dispose();
            _pipelineControl?.Dispose();
        }

        private void Setup()
        {
            _scrollViewControl = new ScrollViewControl
            {
                Size = new Vector(300, _pipelineControl.Size.Height),
                ContentSize = new Vector(300, 1800),
                BackColor = Color.Black.WithTransparency(0.7f),
                ScrollBarsMode = ScrollViewControl.VisibleScrollBars.Vertical
            };

            _controlView.AddControl(_scrollViewControl);

            _pipelineControl.SizeChanged += (sender, args) =>
            {
                _scrollViewControl.Size = new Vector(300, _pipelineControl.Size.Height);
            };
        }

        /// <summary>
        /// Loads a list of pipeline nodes that can be created from the given array of pipeline node specs
        /// </summary>
        public void LoadCreatablePipelineNodes([NotNull] PipelineNodeSpec[] nodeSpecs)
        {
            LoadedSpecs.AddRange(nodeSpecs);

            foreach (var spec in nodeSpecs)
            {
                var button = ButtonForPipelineSpec(spec);
                button.Tag = spec;

                SpecButtons.Add(button);

                _scrollViewControl.ContainerView.AddChild(button);
            }

            // Adjust buttons
            ArrangeButtons();
        }

        private void ArrangeButtons()
        {
            var buttonSize = GetButtonSize();
            var sepSize = new Vector(15, 10);

            float minCellWidth = buttonSize.X + sepSize.X / 2;

            int buttonsPerRow = (int)(_scrollViewControl.VisibleContentBounds.Width / minCellWidth);

            float xStep = _scrollViewControl.VisibleContentBounds.Width / buttonsPerRow;
            float yStep = buttonSize.Y + sepSize.Y;

            for (int i = 0; i < SpecButtons.Count; i++)
            {
                var button = SpecButtons[i];

                float x = xStep / 2 + i % buttonsPerRow * xStep;
                float y = yStep / 2 + (float)Math.Floor((float)i / buttonsPerRow) * yStep;

                button.Center = new Vector(x, y);
            }
        }

        private ButtonControl ButtonForPipelineSpec([NotNull] PipelineNodeSpec spec)
        {
            var buttonSize = GetButtonSize();

            var button = new ButtonControl
            {
                Size = buttonSize,
                Text = spec.Name,
                BackColor = Color.Black.WithTransparency(0.3f),
                NormalColor = Color.Black.WithTransparency(0.3f),
                HighlightColor = Color.Black.WithTransparency(0.3f).BlendedOver(Color.White),
                SelectedColor = Color.Black.WithTransparency(0.3f),
                StrokeWidth = 2,
                CornerRadius = 3,
                StrokeColor = Color.Gray.WithTransparency(0.8f),
                TextColor = Color.White,
                ClipToBounds = false,
                HorizontalTextAlignment = HorizontalTextAlignment.Center,
                TextInset = new InsetBounds(5, 5, 5, 5),
                ImageInset = new InsetBounds(7, 0, 0, 0),
                Image = IconForPipelineNodeType(spec.NodeType, _pipelineControl.D2DRenderer.ImageResources)
            };
            
            button.Rx
                .MouseClick
                .Subscribe(_ =>
                {
                    var node = spec.CreateNode();
                    PipelineNodeSelected?.Invoke(this, new PipelineNodeSelectedEventArgs(node));
                }).AddToDisposable(_disposeBag);

            return button;
        }

        private Vector GetButtonSize()
        {
            return new Vector(_scrollViewControl.VisibleContentBounds.Width / 2 - 20, 40);
        }

        [CanBeNull]
        private ButtonControl ButtonForSpec(PipelineNodeSpec spec)
        {
            return SpecButtons.FirstOrDefault(b => b.Tag == spec);
        }

        /// <summary>
        /// Gets an image resource from a given image resources provider that matches the given pipeline node.
        /// 
        /// This image is used for representing the node's type visually in a small icon form.
        /// </summary>
        public static ImageResource? IconForPipelineNode([NotNull] IPipelineNode node, ID2DImageResourceProvider resourcesProvider)
        {
            return IconForPipelineNodeType(node.GetType(), resourcesProvider);
        }

        /// <summary>
        /// Gets an image resource from a given image resources provider that matches the given pipeline node.
        /// 
        /// This image is used for representing the node's type visually in a small icon form.
        /// </summary>
        public static ImageResource? IconForPipelineNodeType(Type nodeType, ID2DImageResourceProvider resourcesProvider)
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

            return iconName != null ? resourcesProvider.PipelineNodeImageResource(iconName) : null;
        }

        /// <summary>
        /// Arguments for event fired when user selects a pipeline node from the pipeline node panels.
        /// 
        /// Receives the pre-instantiated pipeline node for the event.
        /// </summary>
        public class PipelineNodeSelectedEventArgs : EventArgs
        {
            public IPipelineNode Node { get; }

            public PipelineNodeSelectedEventArgs(IPipelineNode node)
            {
                Node = node;
            }
        }
    }
}
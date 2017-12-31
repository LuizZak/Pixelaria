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
using PixCore.Colors;
using PixCore.Geometry;
using PixCore.Text;
using PixUI;
using PixUI.Controls;
using PixUI.Rendering;

using Pixelaria.ExportPipeline;
using Pixelaria.ExportPipeline.Steps;
using Pixelaria.Filters;
using Pixelaria.Utils;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Manages export pipeline node items available on the side panel of an export pipeline view
    /// </summary>
    internal sealed class ExportPipelineNodesPanelManager: IDisposable
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        private readonly ExportPipelineControl _control;

        private List<PipelineNodeSpec> LoadedSpecs { get; } = new List<PipelineNodeSpec>();
        private List<ButtonControl> SpecButtons { get; } = new List<ButtonControl>();

        private ControlView _container;
        private TextField _searchField;
        private ScrollViewControl _scrollViewControl;

        public delegate void PipelineNodeSelectedEventHandler(object sender, PipelineNodeSelectedEventArgs e);

        public event PipelineNodeSelectedEventHandler PipelineNodeSelected;

        public ExportPipelineNodesPanelManager([NotNull] ExportPipelineControl control)
        {
            _control = control;

            Setup();
        }
        
        public void Dispose()
        {
            _disposeBag?.Dispose();
            _control?.Dispose();
        }

        private void Setup()
        {
            _container = new ControlView
            {
                Size = new Vector(300, _control.Size.Height),
                BackColor = Color.Black.WithTransparency(0.7f)
            };

            _searchField = new TextField();

            _scrollViewControl = new ScrollViewControl
            {
                Location = new Vector(0, 50),
                Size = new Vector(300, _control.Size.Height),
                ContentSize = new Vector(0, 1800),
                BackColor = Color.Transparent,
                ScrollBarsMode = ScrollViewControl.VisibleScrollBars.Vertical
            };

            _container.AddChild(_scrollViewControl);
            _container.AddChild(_searchField);
            
            _control.ControlContainer.AddControl(_container);

            _control.SizeChanged += (sender, args) =>
            {
                AdjustSize();
            };

            AdjustSize();

            SetupReactiveSearch();
        }

        private void SetupReactiveSearch()
        {
            _searchField
                .RxTextUpdated
                .Subscribe(s =>
                {
                    var buttonPairs = SpecButtons.Zip(LoadedSpecs, (control, spec) => (control, spec)).ToArray();

                    foreach (var (button, spec) in buttonPairs)
                    {
                        button.Visible = false;
                        button.Text = spec.Name;
                    }

                    var visible = s == "" ? buttonPairs : buttonPairs.Where(b => b.Item1.Text.ToLower().Contains(s.ToLower())).ToArray();

                    var highlightAttribute = 
                    new BackgroundColorAttribute(Color.CornflowerBlue.WithTransparency(0.6f), new Vector(2, 2));

                    foreach (var (button, spec) in visible)
                    {
                        button.Visible = true;
                        if (s == "")
                            continue;

                        int index = spec.Name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase);
                        button.AttributedText.SetAttributes(new TextRange(index, s.Length), highlightAttribute);
                    }
                    
                    ArrangeButtons(visible.Select(p => p.Item1).ToArray());
                }).AddToDisposable(_disposeBag);
        }

        private void AdjustSize()
        {
            _container.Size = new Vector(300, _control.Size.Height);

            var textFieldBounds = new AABB(0, 0, 40, _container.Size.X);

            _searchField.Location = textFieldBounds.Minimum;
            _searchField.Size = textFieldBounds.Size;

            var scrollViewBounds = _container.Bounds;
            scrollViewBounds = scrollViewBounds.Inset(new InsetBounds(0, 40, 0, 0));

            _scrollViewControl.Location = scrollViewBounds.Minimum;
            _scrollViewControl.Size = scrollViewBounds.Size;
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
                Image = IconForPipelineNodeType(spec.NodeType, _control.D2DRenderer.ImageResources),
                TextFont = new Font(FontFamily.GenericSansSerif.Name, 12)
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
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
using System.Drawing;
using System.Linq;
using System.Reactive.Disposables;

using JetBrains.Annotations;

using PixCore.Colors;
using PixCore.Geometry;
using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineNodePanel;
using PixPipelineGraph;
using PixUI.Controls;
using PixUI.Controls.PropertyGrid;

namespace Pixelaria.Views.ExportPipeline
{
    internal sealed class PropertiesPanel : IDisposable
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        private readonly IExportPipelineControl _control;

        private ControlView _container;
        private PropertyGridControl _propertiesGrid;

        public float PanelWidth
        {
            get => _container.Width;
            set
            {
                _container.Width = value;
                AdjustSize();
            }
        }

        public delegate void PipelineNodeSelectedEventHandler(object sender, ExportPipelineNodesPanelManager.PipelineNodeSelectedEventArgs e);
        
        public PropertiesPanel([NotNull] IExportPipelineControl control)
        {
            _control = control;

            Setup();
        }

        public void Dispose()
        {
            _disposeBag.Dispose();
        }
        
        private void Setup()
        {
            _container = new ControlView
            {
                Size = new Vector(300, _control.Size.Height),
                BackColor = Color.Black.WithTransparency(0.7f)
            };
            
            _propertiesGrid = PropertyGridControl.Create();
            _propertiesGrid.Location = new Vector(0, 50);
            _propertiesGrid.Size = new Vector(300, _control.Size.Height);
            _propertiesGrid.InspectablePropertyChanged += PropertiesGridOnInspectablePropertyChanged;

            _container.AddChild(_propertiesGrid);
            
            _control.ControlContainer.AddControl(_container);

            _control.SizeChanged += (sender, args) =>
            {
                AdjustSize();
            };

            _control.PipelineContainer.SelectionModel.OnSelectionChanged += SelectionModelOnOnSelectionChanged;

            AdjustSize();
        }

        private void PropertiesGridOnInspectablePropertyChanged([NotNull] object sender, [NotNull] InspectablePropertyChangedEventArgs e)
        {
            /*
            if (typeof(IPipelineNode).IsAssignableFrom(e.InspectableProperty.TargetType))
            {
                // Find all pipeline nodes affected by the change
                foreach (var node in e.InspectableProperty.GetTargets().OfType<IPipelineNode>())
                {
                    var nodeView = _control.PipelineContainer.ViewForPipelineNode(node);
                    if (nodeView == null)
                        continue;

                    _control.PipelineNodeViewSizer.AutoSize(nodeView, _control.TextSizeProvider);
                    
                }
            }
            else if (typeof(IPipelineNodeLink).IsAssignableFrom(e.InspectableProperty.TargetType))
            {
                foreach (var nodeLink in e.InspectableProperty.GetTargets().OfType<IPipelineNodeLink>())
                {
                    var nodeLinkView = _control.PipelineContainer.ViewForPipelineNodeLink(nodeLink);
                    if (nodeLinkView?.NodeView == null)
                        continue;

                    nodeLinkView.UpdateDisplay();
                    _control.PipelineNodeViewSizer.AutoSize(nodeLinkView.NodeView, _control.TextSizeProvider);
                }
            }
            */
        }

        private void SelectionModelOnOnSelectionChanged(object o, EventArgs eventArgs)
        {
            _propertiesGrid.SelectedObjects = _control.PipelineContainer.Selection;
        }

        private void AdjustSize()
        {
            float containerWidth = _container.Width;
            var containerRegion = AABB.FromRectangle(_control.Size.Width - containerWidth, 0, containerWidth, _control.Size.Height);

            _container.SetFrame(containerRegion);
            
            var scrollViewBounds = _container.Bounds;

            _propertiesGrid.Location = scrollViewBounds.Minimum;
            _propertiesGrid.Size = scrollViewBounds.Size;
        }
    }
}
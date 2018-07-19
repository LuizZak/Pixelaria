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
using System.Reactive.Disposables;

using JetBrains.Annotations;

using PixCore.Colors;
using PixCore.Geometry;

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

            _container.AddChild(_propertiesGrid);
            
            _control.ControlContainer.AddControl(_container);

            _control.SizeChanged += (sender, args) =>
            {
                AdjustSize();
            };

            _control.PipelineContainer.SelectionModel.OnSelectionChanged += SelectionModelOnOnSelectionChanged;

            AdjustSize();
        }

        private void SelectionModelOnOnSelectionChanged(object o, EventArgs eventArgs)
        {
            _propertiesGrid.SelectedObjects = _control.PipelineContainer.Selection;
        }

        private void AdjustSize()
        {
            const float containerWidth = 300;
            var containerRegion = AABB.FromRectangle(_control.Size.Width - containerWidth, 0, containerWidth, _control.Size.Height);

            _container.SetFrame(containerRegion);
            
            var scrollViewBounds = _container.Bounds;

            _propertiesGrid.Location = scrollViewBounds.Minimum;
            _propertiesGrid.Size = scrollViewBounds.Size;
        }
    }
}
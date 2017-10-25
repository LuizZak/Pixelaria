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
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Utils;
using Pixelaria.Views.ModelViews.PipelineView;

namespace Pixelaria.Views.ModelViews.ExportPipelineFeatures
{
    internal class ViewPanAndZoomUiFeature : ExportPipelineUiFeature
    {
        private readonly Vector _minZoom = new Vector(0.25f);
        private Vector _dragStart;
        private Point _mouseDownPoint;
        private bool _dragging;

        public ViewPanAndZoomUiFeature([NotNull] ExportPipelineControl control)
            : base(control)
        {

        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!OtherFeatureHasExclusiveControl() && e.Button == MouseButtons.Middle)
            {
                // Detect we are not dragging
                if (_dragging && e.Location.Distance(_mouseDownPoint) > 5)
                    return;

                SetZoom(Vector.Unit, ((AABB)Control.Bounds).Center);
            }
        }

        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            _mouseDownPoint = e.Location;

            if (e.Button == MouseButtons.Middle && RequestExclusiveControl())
            {
                _dragStart = contentsView.Location - e.Location / contentsView.Scale;
                _dragging = true;
            }
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_dragging)
            {
                var point = (Vector)e.Location;

                contentsView.Location = _dragStart + point / contentsView.Scale;
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            ReleaseExclusiveControl();

            _dragging = false;
        }

        public override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!OtherFeatureHasExclusiveControl())
            {
                var scale = contentsView.Scale;
                scale *= new Vector(1.0f + Math.Sign(e.Delta) * 0.1f);
                if (scale < _minZoom)
                    scale = _minZoom;
                if (scale > new Vector(25f))
                    scale = new Vector(25f);

                SetZoom(scale, e.Location);
            }
        }

        private void SetZoom(Vector newZoom, Vector focusPosition, bool repositioning = true)
        {
            var priorPivot = contentsView.ConvertFrom(focusPosition, null);

            contentsView.Scale = newZoom;

            if (repositioning)
            {
                var afterPivot = contentsView.ConvertFrom(focusPosition, null);

                contentsView.Location += afterPivot - priorPivot;
            }
        }
    }
}
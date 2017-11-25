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

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    internal class SmoothViewPanAndZoomUiFeature : ExportPipelineUiFeature
    {
        private readonly Vector _minZoom = new Vector(0.25f);
        private readonly Vector _maxZoom = new Vector(25f);
        private Vector _dragStart;
        private Point _mouseDownPoint;
        private bool _dragging;

        private Vector _targetScale;
        private Vector _panTarget;

        public SmoothViewPanAndZoomUiFeature([NotNull] ExportPipelineControl control)
            : base(control)
        {
            _targetScale = contentsView.Scale;
        }

        public override void OnFixedFrame(EventArgs e)
        {
            base.OnFixedFrame(e);

            if (_targetScale.Distance(contentsView.Scale) >= 0.001f)
            {
                var newZoom = (_targetScale - contentsView.Scale) * 0.35f;

                SetZoom(contentsView.Scale + newZoom, _panTarget);
            }
        }
        
        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!OtherFeatureHasExclusiveControl() && e.Button == MouseButtons.Middle)
            {
                // Detect we are not dragging
                if (_dragging && e.Location.Distance(_mouseDownPoint) > 5)
                    return;

                SetTargetScale(Vector.Unit, e.Location);
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

                ConsumeEvent();
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
                var scale = _targetScale;
                scale *= new Vector(1.0f + Math.Sign(e.Delta) * 0.1f);

                scale = Vector.Min(scale, _maxZoom);
                scale = Vector.Max(scale, _minZoom);

                var center = (Vector)Control.Size / 2;

                SetTargetScale(scale, center);
            }
        }

        public void SetTargetScale(Vector newScale, Vector targetFocusPosition)
        {
            _targetScale = newScale;
            _panTarget = targetFocusPosition;
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
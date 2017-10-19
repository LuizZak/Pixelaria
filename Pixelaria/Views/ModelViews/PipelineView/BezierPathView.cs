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
using System.Drawing.Drawing2D;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline;
using Pixelaria.Utils;

namespace Pixelaria.Views.ModelViews.PipelineView
{
    /// <summary>
    /// A view for a graphics path.
    /// </summary>
    public class BezierPathView : BaseView
    {
        private readonly GraphicsPath _path = new GraphicsPath();
        private Color _fillColor = Color.Transparent;

        public override AABB Bounds => _path.GetBounds().Inflated(StrokeWidth, StrokeWidth);

        /// <summary>
        /// The stroke color of this bezier path
        /// </summary>
        public sealed override Color StrokeColor
        {
            set
            {
                base.StrokeColor = value;

                MarkDirtyPath();
            }
        }

        /// <summary>
        /// The width of the line for this bezier path view
        /// </summary>
        public sealed override int StrokeWidth
        {
            set
            {
                MarkingDirtyRegion(() =>
                {
                    base.StrokeWidth = value;
                });
            }
        }

        /// <summary>
        /// Whether to force rendering this bezier path view on top of all other views.
        /// 
        /// Used at discretion of Renderer, and may be ignored by it.
        /// </summary>
        public bool RenderOnTop { get; set; } = false;

        /// <summary>
        /// The fill color of this bezier path.
        /// 
        /// Defaults to <see cref="Color.Transparent"/>
        /// </summary>
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                
                MarkDirtyPath();
            }
        }

        public BezierPathView()
        {
            StrokeColor = Color.Orange;
            StrokeWidth = 2;
        }

        /// <summary>
        /// Clears the underlying path
        /// </summary>
        public void ClearPath()
        {
            MarkDirtyPath();

            _path.Reset();
        }

        /// <summary>
        /// Gets a copy of the underlying path of this bezier path view
        /// </summary>
        public GraphicsPath GetPath()
        {
            return (GraphicsPath)_path.Clone();
        }

        /// <summary>
        /// Sets the bezier points of this bezier path view
        /// </summary>
        public void AddBezierPoints(Vector pt1, Vector pt2, Vector pt3, Vector pt4)
        {
            MarkingDirtyRegion(() =>
            {
                _path.AddBezier(pt1, pt2, pt3, pt4);
            });
        }

        /// <summary>
        /// Adds a Rectangle to this bezier path view
        /// </summary>
        public void AddRectangle(AABB area)
        {
            MarkingDirtyRegion(() =>
            {
                _path.AddRectangle((RectangleF)area);
            });
        }

        /// <summary>
        /// Sets a Rectangle as the path.
        /// 
        /// This erases any previous path that was present before.
        /// </summary>
        public void SetAsRectangle(AABB area)
        {
            MarkingDirtyRegion(() =>
            {
                _path.Reset();
                _path.AddRectangle((RectangleF)area);
            });
        }

        public override bool Contains(Vector point, Vector inflatingArea)
        {
            using (var pen = new Pen(StrokeColor, StrokeWidth + inflatingArea.X))
            {
                return _path.IsOutlineVisible(point, pen);
            }
        }

        /// <summary>
        /// Records the region taken by the current path, then runs a path-modifying closure, and
        /// on return, performs a differential invalidation routine on the parent view's region.
        /// </summary>
        private void MarkingDirtyRegion([NotNull] Action execute)
        {
            // No before to dirty
            if (_path.GetBounds().IsEmpty)
            {
                execute();
                MarkDirtyPath();
                return;
            }
            
            var prev = new Region(_path);

            execute();

            prev.Xor(_path);

            MarkDirty(prev);
        }

        /// <summary>
        /// Marks the wholeregion of this view's path as dirty on its parent view
        /// </summary>
        private void MarkDirtyPath()
        {
            if (Math.Abs(Bounds.Area) < float.Epsilon)
                return;

            MarkDirty(Bounds);
        }
    }

    /// <summary>
    /// View that represents a connection between two pipeline step nodes using a bezier path
    /// </summary>
    public class PipelineNodeConnectionLineView : BezierPathView
    {
        public PipelineNodeLinkView Start { get; set; }
        public PipelineNodeLinkView End { get; set; }

        public PipelineNodeConnectionLineView(PipelineNodeLinkView start, PipelineNodeLinkView end)
        {
            Start = start;
            End = end;

            UpdateBezier();
        }

        /// <summary>
        /// Updates the bezier line for the connection
        /// </summary>
        public void UpdateBezier()
        {
            // Convert coordinates first
            var center1 = Start.ConvertTo(Start.Bounds.Center, this);
            var center2 = End.ConvertTo(End.Bounds.Center, this);

            bool startToRight = Start.NodeLink is IPipelineOutput;
            bool endToRight   = End.NodeLink is IPipelineOutput;

            var pt1 = center1;
            var pt4 = center2;
            var pt2 = new Vector(startToRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
            var pt3 = new Vector(endToRight ? pt4.X + 75 : pt4.X - 75, pt4.Y);

            ClearPath();
            AddBezierPoints(pt1, pt2, pt3, pt4);
        }
    }
}

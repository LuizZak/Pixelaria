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

using System.Drawing;
using System.Drawing.Drawing2D;
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
        private Color _strokeColor = Color.Orange;
        private int _strokeWidth = 2;

        public override AABB Bounds => _path.GetBounds().Inflated(StrokeWidth, StrokeWidth);

        /// <summary>
        /// The stroke color of this bezier path
        /// </summary>
        public Color StrokeColor
        {
            get => _strokeColor;
            set
            {
                _strokeColor = value;

                MarkDirtyPath();
            }
        }

        /// <summary>
        /// The width of the line for this bezier path view
        /// </summary>
        public int StrokeWidth
        {
            get => _strokeWidth;
            set
            {
                MarkDirtyPath();

                _strokeWidth = value;

                MarkDirtyPath();
            }
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
            MarkDirtyPath();

            _path.AddBezier(pt1, pt2, pt3, pt4);

            MarkDirtyPath();
        }

        public override bool IntersectsThis(Vector point, Vector inflatingArea)
        {
            using (var pen = new Pen(StrokeColor, StrokeWidth + inflatingArea.X))
            {
                return _path.IsOutlineVisible(point, pen);
            }
        }

        private void MarkDirtyPath()
        {
            MarkDirty(Bounds);
        }
    }

    /// <summary>
    /// View that represents a connection between two pipeline step nodes using
    /// a bezier path
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
            var center1 = Start.ConvertTo(Start.Center, this);
            var center2 = End.ConvertTo(End.Center, this);

            var startToRight = Start.NodeLink is IPipelineOutput;
            var endToRight   = End.NodeLink is IPipelineOutput;

            var pt1 = center1;
            var pt4 = center2;
            var pt2 = new Vector(startToRight ? pt1.X + 75 : pt1.X - 75, pt1.Y);
            var pt3 = new Vector(endToRight ? pt4.X + 75 : pt4.X - 75, pt4.Y);

            ClearPath();
            AddBezierPoints(pt1, pt2, pt3, pt4);
        }
    }
}

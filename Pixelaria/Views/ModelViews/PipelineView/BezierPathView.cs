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
using System.Drawing.Drawing2D;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline;

namespace Pixelaria.Views.ModelViews.PipelineView
{
    /// <summary>
    /// A view for a graphics path.
    /// </summary>
    public class BezierPathView : BaseView
    {
        /// <summary>
        /// Cached path for Contains calls
        /// </summary>
        [CanBeNull]
        private GraphicsPath _containsPath;

        private AABB _inputsBounds = AABB.Empty;

        private readonly List<IPathInput> _inputs = new List<IPathInput>();

        public override AABB Bounds => _inputsBounds.Inflated(StrokeWidth, StrokeWidth);
        
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
        public Color FillColor { get; set; } = Color.Transparent;

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
            _containsPath?.Dispose();
            _containsPath = null;

            _inputsBounds = AABB.Empty;
            _inputs.Clear();
        }

        /// <summary>
        /// Gets the original path instructions that where used to create the path since
        /// the last ClearPath() call.
        /// 
        /// Every call to <see cref="AddBezierPoints"/> and <see cref="AddRectangle"/>
        /// (and <see cref="SetAsRectangle"/>, which clears the path first) pushes a
        /// matching IPathInput to the internal path history array and can be extracted 
        /// through this method.
        /// </summary>
        public IPathInput[] GetPathInputs()
        {
            return _inputs.ToArray();
        }

        /// <summary>
        /// Sets the bezier points of this bezier path view
        /// </summary>
        public void AddBezierPoints(Vector pt1, Vector pt2, Vector pt3, Vector pt4)
        {
            _containsPath?.Dispose();
            _containsPath = null;
            
            _inputsBounds = _inputsBounds.Union(new AABB(new []{pt1, pt2, pt3, pt4}));

            _inputs.Add(new BezierPathInput(pt1, pt2, pt3, pt4));
        }

        /// <summary>
        /// Adds a Rectangle to this bezier path view
        /// </summary>
        public void AddRectangle(AABB area)
        {
            _containsPath?.Dispose();
            _containsPath = null;

            _inputsBounds = _inputsBounds.Union(area);
            _inputs.Add(new RectanglePathInput(area));
        }

        /// <summary>
        /// Sets a Rectangle as the path.
        /// 
        /// This erases any previous path that was present before.
        /// </summary>
        public void SetAsRectangle(AABB area)
        {
            ClearPath();
            AddRectangle(area);
        }

        public override bool Contains(Vector point, Vector inflatingArea)
        {
            if (!_inputsBounds.Inflated(inflatingArea).Contains(point))
                return false;

            var path = _containsPath ?? (_containsPath = GetPath());

            using (var pen = new Pen(StrokeColor, StrokeWidth + inflatingArea.X))
            {
                return path.IsOutlineVisible(point, pen);
            }
        }

        /// <summary>
        /// Gets a copy of the underlying path of this bezier path view
        /// </summary>
        private GraphicsPath GetPath()
        {
            var path = new GraphicsPath();

            // Re-create path
            foreach (var input in _inputs)
            {
                if (input is RectanglePathInput rectangle)
                {
                    path.AddRectangle((Rectangle)rectangle.Rectangle);
                }
                else if (input is BezierPathInput bezier)
                {
                    path.AddBezier(bezier.Start, bezier.ControlPoint1, bezier.ControlPoint2, bezier.End);
                }
            }

            return path;
        }

        /// <summary>
        /// An interface that specifies an input path of a bezier path view
        /// </summary>
        public interface IPathInput
        {
            
        }

        public struct RectanglePathInput : IPathInput
        {
            public AABB Rectangle { get; }

            public RectanglePathInput(AABB rectangle)
            {
                Rectangle = rectangle;
            }
        }

        public struct BezierPathInput : IPathInput
        {
            public Vector Start { get; }
            public Vector ControlPoint1 { get; }
            public Vector ControlPoint2 { get; }
            public Vector End { get; }

            public BezierPathInput(Vector start, Vector controlPoint1, Vector controlPoint2, Vector end) : this()
            {
                Start = start;
                ControlPoint1 = controlPoint1;
                ControlPoint2 = controlPoint2;
                End = end;
            }
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
            var bezier = PathInputForConnection();

            ClearPath();
            AddBezierPoints(bezier.Start, bezier.ControlPoint1, bezier.ControlPoint2, bezier.End);
        }

        public BezierPathInput PathInputForConnection()
        {
            // Convert coordinates first
            var center1 = Start.ConvertTo(Start.Bounds.Center, this);
            var center2 = End.ConvertTo(End.Bounds.Center, this);

            bool startToRight = Start.NodeLink is IPipelineOutput;
            bool endToRight = End.NodeLink is IPipelineOutput;

            float maxSep = Math.Min(75, Math.Abs(center1.Distance(center2)));

            var pt1 = center1;
            var pt4 = center2;
            var pt2 = new Vector(startToRight ? pt1.X + maxSep : pt1.X - maxSep, pt1.Y);
            var pt3 = new Vector(endToRight ? pt4.X + maxSep : pt4.X - maxSep, pt4.Y);

            return new BezierPathInput(pt1, pt2, pt3, pt4);
        }
    }
}

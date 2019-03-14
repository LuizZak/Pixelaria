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
using PixCore.Geometry;

namespace PixUI
{
    /// <summary>
    /// A view for a graphics path.
    /// </summary>
    public class BezierPathView : BaseView
    {
        private float _outerStrokeWidth;
        private Color _outerStrokeColor = Color.Transparent;

        /// <summary>
        /// Cached path for Contains calls
        /// </summary>
        [CanBeNull]
        private GraphicsPath _containsPath;

        private AABB _inputsBounds = AABB.Empty;

        private readonly List<IPathInput> _inputs = new List<IPathInput>();
        private Color _fillColor = Color.Transparent;

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
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The outer stroke color applied under the <see cref="BaseView.StrokeColor"/> of this view.
        /// </summary>
        public Color OuterStrokeColor
        {
            get => _outerStrokeColor;
            set
            {
                _outerStrokeColor = value;
                Invalidate();
            }
        }

        /// <summary>
        /// The width of the outer stroke applied under this view's stroke color.
        /// 
        /// This width is added to the value of <see cref="BaseView.StrokeWidth"/> when rendering, so
        /// values &gt; 0 will always draw an outer stroke on top of the existing stroke.
        /// </summary>
        public float OuterStrokeWidth
        {
            get => _outerStrokeWidth;
            set
            {
                _outerStrokeWidth = value;
                Invalidate();
            }
        }
        
        /// <summary>
        /// Factory method for creation of bezier path views.
        /// </summary>
        public static BezierPathView Create()
        {
            var pathView = new BezierPathView();
            pathView.Initialize();
            
            return pathView;
        }

        /// <summary>
        /// Factory method for creation of bezier path views.
        /// </summary>
        /// <param name="creator">A creator block that is invoked passing in the created bezier path view for further configuration.</param>
        public static BezierPathView Create([NotNull, InstantHandle] Action<BezierPathView> creator)
        {
            var pathView = Create();

            creator(pathView);

            return pathView;
        }

        protected BezierPathView()
        {
            
        }

        /// <summary>
        /// Post-construction common initialization point.
        /// </summary>
        protected void Initialize()
        {
            StrokeColor = Color.Orange;
            StrokeWidth = 2;
        }

        /// <summary>
        /// Clears the underlying path
        /// </summary>
        public void ClearPath()
        {
            Invalidate();

            _containsPath?.Dispose();
            _containsPath = null;

            _inputsBounds = AABB.Invalid;
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
        /// Gets a compound path input for this bezier path view's inputs.
        /// </summary>
        public ICompletePathInput GetCompletePathInput()
        {
            return new CompletePathInput(_inputs, false);
        }

        /// <summary>
        /// Adds a line to this bezier path view
        /// </summary>
        public void AddLine(Vector start, Vector end)
        {
            _containsPath?.Dispose();
            _containsPath = null;
            
            AddInputBounds(new AABB(new[] {start, end}));
            _inputs.Add(new LinePathInput(start, end));

            Invalidate();
        }

        /// <summary>
        /// Adds a bezier curve on this bezier path view
        /// </summary>
        public void AddBezierPoints(Vector pt1, Vector pt2, Vector pt3, Vector pt4)
        {
            _containsPath?.Dispose();
            _containsPath = null;
            
            AddInputBounds(new AABB(new[] {pt1, pt2, pt3, pt4}));
            _inputs.Add(new BezierPathInput(pt1, pt2, pt3, pt4));

            Invalidate();
        }
        
        /// <summary>
        /// Adds a Rectangle to this bezier path view
        /// </summary>
        public void AddRectangle(AABB area)
        {
            _containsPath?.Dispose();
            _containsPath = null;

            AddInputBounds(area);
            _inputs.Add(new RectanglePathInput(area));

            Invalidate();
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

        private void AddInputBounds(AABB area)
        {
            _inputsBounds = _inputsBounds.Union(in area);
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
            var sink = new GraphicsPathInputSink(path);

            var pathInput = GetCompletePathInput();
            pathInput.ApplyOnSink(sink);
            
            return path;
        }

        /// <summary>
        /// An interface that specifies an input path of a bezier path view
        /// </summary>
        public interface IPathInput
        {
            /// <summary>
            /// Requests that this path input apply itself onto a given path input sink.
            /// </summary>
            void ApplyOnSink([NotNull] IPathInputSink sink);
        }
        
        /// <summary>
        /// A compound path input that when applied to a path input sink deals with opening
        /// and closing of the sink by itself.
        /// </summary>
        public interface ICompletePathInput : IPathInput
        {

        }

        protected readonly struct RectanglePathInput : IPathInput
        {
            public AABB Rectangle { get; }

            public RectanglePathInput(AABB rectangle)
            {
                Rectangle = rectangle;
            }

            public void ApplyOnSink(IPathInputSink sink)
            {
                sink.BeginFigure(Rectangle.Minimum, true);
                sink.AddRectangle(Rectangle);
                sink.EndFigure(true);
            }
        }

        protected readonly struct BezierPathInput : IPathInput
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

            public void ApplyOnSink(IPathInputSink sink)
            {
                sink.MoveTo(Start);
                sink.BezierTo(ControlPoint1, ControlPoint2, End);
            }
        }

        protected readonly struct LinePathInput : IPathInput
        {
            public Vector Start { get; }
            public Vector End { get; }
            
            public LinePathInput(Vector start, Vector end) : this()
            {
                Start = start;
                End = end;
            }

            public void ApplyOnSink(IPathInputSink sink)
            {
                sink.MoveTo(Start);
                sink.LineTo(End);
            }
        }

        private class GraphicsPathInputSink: IPathInputSink
        {
            private readonly GraphicsPath _graphicsPath;
            private Vector _point;

            public GraphicsPathInputSink(GraphicsPath graphicsPath)
            {
                _graphicsPath = graphicsPath;
            }

            public void BeginFigure(Vector location, bool filled)
            {
                _graphicsPath.StartFigure();
            }

            public void MoveTo(Vector point)
            {
                _point = point;
            }

            public void LineTo(Vector point)
            {
                _graphicsPath.AddLine(_point, point);
                _point = point;
            }

            public void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint)
            {
                _graphicsPath.AddBezier(_point, anchor1, anchor2, endPoint);
                _point = endPoint;
            }

            public void AddRectangle(AABB rectangle)
            {
                _graphicsPath.AddRectangle((Rectangle)rectangle);
                _point = rectangle.Minimum;
            }

            public void EndFigure(bool closePath)
            {
                if (closePath)
                    _graphicsPath.CloseFigure();
            }
        }

        protected class CompletePathInput : ICompletePathInput
        {
            private readonly IReadOnlyList<IPathInput> _inputs;
            private readonly bool _closed;

            public CompletePathInput(IReadOnlyList<IPathInput> inputs, bool closed)
            {
                _inputs = inputs;
                _closed = closed;
            }

            public void ApplyOnSink(IPathInputSink sink)
            {
                foreach (var input in _inputs)
                {
                    input.ApplyOnSink(sink);
                }

                sink.EndFigure(_closed);
            }
        }
    }

    /// <summary>
    /// Represents a sink that provides drawing operations that <see cref="BezierPathView.IPathInput"/>
    /// instances can forward their drawing operations to.
    /// </summary>
    public interface IPathInputSink
    {
        /// <summary>
        /// Begins a new figure on this sink, specifying whether to start a filled or closed figure.
        /// </summary>
        void BeginFigure(Vector location, bool filled);

        /// <summary>
        /// Moves the pen position to a given point without performing a drawing operation.
        /// </summary>
        void MoveTo(Vector point);

        /// <summary>
        /// Draws a line from the current pen position to the given point.
        /// </summary>
        /// <param name="point">Point to add the line to, starting from the current pen position.</param>
        void LineTo(Vector point);

        /// <summary>
        /// Adds a cubic bezier path from the current pen position, through the two anchors, ending at a given point.
        /// </summary>
        void BezierTo(Vector anchor1, Vector anchor2, Vector endPoint);

        /// <summary>
        /// Adds a rectangle to this path sink.
        /// 
        /// The operation doesn't continue from the current pen position, moving the pen to the origin before starting
        /// the drawing operation.
        /// </summary>
        void AddRectangle(AABB rectangle);

        /// <summary>
        /// Closes the current figure on this path input, optionally specifying whether to close the current path such
        /// that it loops back to the beginning.
        /// </summary>
        void EndFigure(bool closePath);
    }
}

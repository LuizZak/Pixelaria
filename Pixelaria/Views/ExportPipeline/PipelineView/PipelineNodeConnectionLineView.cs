﻿/*
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
using PixCore.Geometry;
using PixPipelineGraph;
using PixUI;

namespace Pixelaria.Views.ExportPipeline.PipelineView
{
    /// <summary>
    /// View that represents a connection between two pipeline step nodes using a bezier path
    /// </summary>
    internal class PipelineNodeConnectionLineView : BezierPathView
    {
        public PipelineNodeLinkView Start { get; private set; }
        public PipelineNodeLinkView End { get; private set; }
        public IPipelineConnection Connection { get; private set; }

        public static PipelineNodeConnectionLineView Create(PipelineNodeLinkView start, PipelineNodeLinkView end, IPipelineConnection connection)
        {
            var instance = new PipelineNodeConnectionLineView();

            instance.Initialize();

            instance.Start = start;
            instance.End = end;
            instance.Connection = connection;
            instance.OuterStrokeWidth = 1;
            instance.OuterStrokeColor = Color.Black;

            instance.UpdateBezier();

            return instance;
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

        protected BezierPathInput PathInputForConnection()
        {
            // Convert coordinates first
            var center1 = Start.ConvertTo(Start.Bounds.Center, this);
            var center2 = End.ConvertTo(End.Bounds.Center, this);

            bool startToRight = Start is PipelineNodeOutputLinkView;
            bool endToRight = End is PipelineNodeOutputLinkView;

            float maxSep = Math.Min(75, Math.Abs(center1.Distance(center2)));

            var pt1 = center1;
            var pt4 = center2;
            var pt2 = new Vector(startToRight ? pt1.X + maxSep : pt1.X - maxSep, pt1.Y);
            var pt3 = new Vector(endToRight ? pt4.X + maxSep : pt4.X - maxSep, pt4.Y);

            return new BezierPathInput(pt1, pt2, pt3, pt4);
        }
    }
}
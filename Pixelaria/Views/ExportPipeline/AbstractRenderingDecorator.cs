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
using PixUI;
using Pixelaria.Views.ExportPipeline.PipelineView;

namespace Pixelaria.Views.ExportPipeline
{
    internal abstract class AbstractRenderingDecorator : IRenderingDecorator
    {
        public virtual void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
        {

        }

        public virtual void DecoratePipelineStepInput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {

        }

        public virtual void DecoratePipelineStepOutput(PipelineNodeView nodeView, PipelineNodeLinkView link,
            ref PipelineStepViewLinkState state)
        {

        }

        public virtual void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
        {

        }

        public virtual void DecorateLabelView(LabelView pathView, ref LabelViewState state)
        {

        }
    }

    internal struct PipelineStepViewState
    {
        public float StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color TitleFillColor { get; set; }
        public Color StrokeColor { get; set; }
        public Color TitleFontColor { get; set; }
        public Color BodyFontColor { get; set; }
    }

    internal struct PipelineStepViewLinkState
    {
        public float StrokeWidth { get; set; }
        public Color FillColor { get; set; }
        public Color StrokeColor { get; set; }
    }

    internal struct BezierPathViewState
    {
        public float StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color FillColor { get; set; }
        public float OuterStrokeWidth { get; set; }
        public Color OuterStrokeColor { get; set; }
    }

    internal struct LabelViewState
    {
        public float StrokeWidth { get; set; }
        public Color StrokeColor { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
    }
}

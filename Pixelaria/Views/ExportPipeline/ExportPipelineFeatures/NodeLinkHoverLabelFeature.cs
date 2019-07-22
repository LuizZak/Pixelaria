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
using System.Windows.Forms;

using JetBrains.Annotations;

using PixCore.Colors;
using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixDirectX.Rendering;
using PixUI;

using Pixelaria.ExportPipeline;
using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI.Controls;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    internal class NodeLinkHoverLabelFeature : ExportPipelineUiFeature
    {
        private readonly LabelView _labelView;
        private readonly RenderListener _renderListener = new RenderListener();

        public NodeLinkHoverLabelFeature([NotNull] IExportPipelineControl control) : base(control)
        {
            _labelView = new LabelView
            {
                TextColor = Color.White,
                TextFont = new Font(FontFamily.GenericSansSerif, 17),
                BackgroundColor = Color.Black.Faded(Color.Transparent, 0.1f, true),
                StrokeColor = Color.White,
                Text = "",
                TextInsetBounds = new InsetBounds(5, 5, 5, 5)
            };

            Control.D2DRenderer.AddRenderListener(_renderListener);
        }

        public override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _labelView.RemoveFromParent();
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            var mouseLoc = e.Location;

            UpdateWithMousePosition(mouseLoc);
        }

        public override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            UpdateWithMousePosition(e.Location);
        }

        private void UpdateWithMousePosition(Point mouseLoc)
        {
            if (OtherFeatureHasExclusiveControl())
            {
                _renderListener.View = null;
                Control.InvalidateRegion(new RedrawRegion(_labelView.Bounds, _labelView));
            }
            else
            {
                var relativeLoc = contentsView.ConvertFrom(mouseLoc, null);

                var view = contentsView.ViewUnder(relativeLoc, new Vector(5));
                if (view is PipelineNodeLinkView linkView)
                {
                    Control.InvalidateRegion(new RedrawRegion(_labelView.Bounds, _labelView));
                    DisplayLabelForLink(linkView);
                    Control.InvalidateRegion(new RedrawRegion(_labelView.Bounds, _labelView));
                }
                else
                {
                    _renderListener.View = null;
                    Control.InvalidateRegion(new RedrawRegion(_labelView.Bounds, _labelView));
                }
            }
        }

        private void DisplayLabelForLink([NotNull] PipelineNodeLinkView linkView)
        {
            Type[] types;
            if (linkView.NodeLink is IPipelineOutput output)
            {
                types = new[] { output.DataType };
            }
            else if (linkView.NodeLink is IPipelineInput input)
            {
                types = input.DataTypes;
            }
            else
            {
                types = new Type[0];
            }

            var labelText = new AttributedText();

            // Construct accepted types string
            var typeListText = new AttributedText();

            foreach (var type in types)
            {
                var typeText = new AttributedText(NameForType(type), new ForegroundColorAttribute(ColorForType(type)));

                if (typeListText.Length > 0)
                {
                    typeListText += ", ";
                }

                typeListText += typeText;
            }

            // Assign label text
            bool isOutput = linkView.NodeLink is IPipelineOutput;

            labelText.Append(isOutput ? "output:" : "input:",
                new ITextAttribute[]
                {
                    new ForegroundColorAttribute(isOutput ? Color.MediumVioletRed : Color.ForestGreen),
                    new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 13))
                });

            labelText.Append("\n");
            labelText.Append(linkView.NodeLink.Name);

            if (types.Length > 0)
            {
                labelText.Append(": ");
                labelText.Append(typeListText);
            }

            // Append connection count, if available
            int count = container.GetLinksConnectedTo(linkView).Count();
            if (count > 0)
            {
                labelText.Append("\n");
                labelText.Append($"{count} connection{(count == 1 ? "" : "s")}", new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 13)));

                if (count == 1)
                {
                    labelText.Append(": " + container.GetLinksConnectedTo(linkView).First().NodeView.Name, new TextFontAttribute(new Font(FontFamily.GenericSansSerif, 13)));
                }
            }

            _labelView.AttributedText.SetText(labelText);

            var absoluteLinkBounds = uiContainerView.ConvertFrom(linkView.Bounds, linkView);

            float xOffset = isOutput
                ? absoluteLinkBounds.Width / 2 + 5
                : -absoluteLinkBounds.Width / 2 - _labelView.Bounds.Width - 5;

            _labelView.Location =
                uiContainerView.ConvertFrom(linkView.Bounds.Center, linkView) +
                new Vector(xOffset, -_labelView.Bounds.Height / 2);

            _renderListener.View = _labelView;
        }

        private static string NameForType([NotNull] Type type)
        {
            if (type == typeof(float) || type == typeof(double))
                return "Number";
            if (type == typeof(int))
                return "Integer";

            return type.Name;
        }

        private static Color ColorForType(Type type)
        {
            return Color.CornflowerBlue;
        }

        private class RenderListener : IRenderListener
        {
            public int RenderOrder { get; } = RenderOrdering.UserInterface;

            [CanBeNull]
            public LabelView View { get; set; }

            public void RecreateState(IRenderLoopState state)
            {
                
            }

            public void Render(IRenderListenerParameters parameters)
            {
                if (View == null)
                    return;

                InternalNodeViewRenderer.DrawLabelView(parameters, View, new IRenderingDecorator[0]);
            }
        }
    }
}
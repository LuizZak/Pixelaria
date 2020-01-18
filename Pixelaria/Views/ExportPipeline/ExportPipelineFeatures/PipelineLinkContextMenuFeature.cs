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
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using PixCore.Geometry;

using Pixelaria.Views.ExportPipeline.PipelineView;
using PixUI;

namespace Pixelaria.Views.ExportPipeline.ExportPipelineFeatures
{
    /// <summary>
    /// Allows the user to right-click pipeline nodes and inspect their properties with a context menu.
    /// </summary>
    internal class PipelineLinkContextMenuFeature : ExportPipelineUiFeature
    {
        public PipelineLinkContextMenuFeature([NotNull] IExportPipelineControl control) : base(control)
        {

        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button != MouseButtons.Right)
                return;
            
            ShowContextMenuForLinkUnder(e.Location);
        }

        public void ShowContextMenuForLinkUnder(Vector location)
        {
            if (!(Control is Control control))
                return;

            // Search input node under mouse
            var point = contentsView.ConvertFrom(location, null);

            var view = contentsView.ViewUnder(point, Vector.Zero);
            if (!(view is PipelineNodeLinkView linkView))
                return;
            
            var menu = new ContextMenuStrip();

            ConfigureContextMenu(menu, linkView);

            menu.Show(control, Control.MousePoint);
        }

        public void ConfigureContextMenu([NotNull] ContextMenuStrip menu, [NotNull] PipelineNodeLinkView linkView)
        {
            menu.Items.Add(linkView.Title).Enabled = false;
            menu.Items.Add("-");

            var decorator = new ConnectionHighlightDecorator
            {
                NodeView = linkView.NodeView
            };
            Control.RenderingDecoratorTarget.AddDecorator(decorator);

            menu.MouseEnter += (sender, args) =>
            {
                Control.InvalidateAll();
            };

            menu.Closing += (sender, args) =>
            {
                Control.RenderingDecoratorTarget.RemoveDecorator(decorator);
            };
            
            var connections = container.GetConnections(linkView).ToArray();

            if (connections.Length > 0)
            {
                var connectionsMenu = new ToolStripMenuItem("Connections");

                foreach (var connection in connections)
                {
                    var link = connection.Start.Equals(linkView) ? connection.End : connection.Start;

                    string name = $"{link.NodeView.Name} - {link.Title}";
                    var item = connectionsMenu.DropDownItems.Add(name);

                    item.MouseEnter += (sender, args) =>
                    {
                        decorator.LineView = connection;
                        connection.Invalidate();
                        Control.InvalidateAll();
                    };

                    item.MouseLeave += (sender, args) =>
                    {
                        decorator.LineView = null;
                        connection.Invalidate();
                        Control.InvalidateAll();
                    };

                    item.Click += (sender, args) =>
                    {
                        container.ClearSelection();
                        container.SelectConnection(connection.Connection);
                    };
                }

                menu.Items.Add(connectionsMenu);
            }
            else
            {
                var item = new ToolStripMenuItem("No connections");
                item.Enabled = false;

                menu.Items.Add(item);
            }
        }

        private class ConnectionHighlightDecorator : AbstractRenderingDecorator
        {
            [CanBeNull]
            public PipelineNodeConnectionLineView LineView { get; set; }
            [CanBeNull]
            public PipelineNodeView NodeView { get; set; }

            public override void DecoratePipelineStep(PipelineNodeView nodeView, ref PipelineStepViewState state)
            {
                if (LineView == null || Equals(nodeView, NodeView))
                    return;

                if (nodeView.GetLinkViews().Contains(LineView.Start) || nodeView.GetLinkViews().Contains(LineView.End))
                {
                    state.StrokeColor = Color.Orange;
                    state.StrokeWidth = 3;
                }
            }

            public override void DecorateBezierPathView(BezierPathView pathView, ref BezierPathViewState state)
            {
                if (!pathView.Equals(LineView))
                    return;

                state.OuterStrokeColor = Color.White;
                state.OuterStrokeWidth = 3;
            }
        }
    }
}

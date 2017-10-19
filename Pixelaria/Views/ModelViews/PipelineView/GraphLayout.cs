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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixelaria.Views.ModelViews.PipelineView
{
    /// <summary>
    /// Used to auto-layout graph nodes for the export pipeline view
    /// </summary>
    internal class GraphLayout
    {

    }

    /// <summary>
    /// A graph that is created and fed to the GraphLayout class for layoutting.
    /// </summary>
    internal class Graph
    {
        List<Node> _nodes = new List<Node>();

        /// <summary>
        /// Creates a new node on the graph.
        /// 
        /// Returns the identifier for the node to use to add connections to.
        /// </summary>
        /// <param name="tag">Identifies this node once the connection is retrieved after a graph layout operation</param>
        /// <returns></returns>
        public INode CreateNode(object tag)
        {
            var node = new Node();

            _nodes.Add(node);

            return node;
        }

        /// <summary>
        /// A node of the graph
        /// </summary>
        public interface INode
        {
            
        }
        
        private class Node : INode
        {
            public object Tag { get; set; }
        }
    }
}

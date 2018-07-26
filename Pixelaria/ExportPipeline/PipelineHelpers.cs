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
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Pixelaria.ExportPipeline.Inputs.Abstract;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Helper static extensions for IPipeline* implementers
    /// </summary>
    public static class PipelineHelpers
    {
        /// <summary>
        /// Connects the first Output from <see cref="step"/> that matches the first Input from
        /// <see cref="other"/>.
        /// </summary>
        [CanBeNull]
        public static IPipelineLinkConnection ConnectTo([NotNull] this IPipelineNodeWithOutputs step, IPipelineNodeWithInputs other)
        {
            // Find first matching output from this that matches an input from other
            foreach (var output in step.Output)
            {
                foreach (var input in other.Input)
                {
                    if (input.Connections.Contains(output))
                        continue;

                    if (!input.CanConnect(output)) continue;

                    return input.Connect(output);
                }
            }

            return null;
        }
        
        /// <summary>
        /// Returns whether a pipeline input can be connected to an output.
        /// 
        /// The method looks through the proper data types accepted by the input and the data type
        /// of the output to make the decision.
        /// </summary>
        public static bool CanConnect([NotNull] this IPipelineInput input, [NotNull] IPipelineOutput output)
        {
            return
                input.Node != output.Node &&
                input.DataTypes.Any(type => type.IsAssignableFrom(output.DataType));
        }

        /// <summary>
        /// From a given node, traverses all parent nodes (connected via outputs) in breadth-first order until
        /// either <see cref="closure"/> returns false or all nodes have been traversed.
        /// </summary>
        /// <param name="node">Node to start traversing from.</param>
        /// <param name="closure">
        /// A visitor closure that will be called for <see cref="node"/> and all parent nodes.
        /// If this closure returns false, traversal stops earlier.
        /// </param>
        public static void TraverseInputs([NotNull] this IPipelineNode node, Func<IPipelineNode, bool> closure)
        {
            var visited = new HashSet<IPipelineNode>();
            var queue = new Queue<IPipelineNode>();

            queue.Enqueue(node);

            // Do a breadth-first search
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();

                if (!closure(cur))
                    return;

                visited.Add(cur);

                if (!(cur is IPipelineNodeWithInputs inputNode))
                    continue;

                foreach (var input in inputNode.Input)
                {
                    foreach (var connection in input.Connections)
                    {
                        if(!visited.Contains(connection.Node))
                            queue.Enqueue(connection.Node);
                    }
                }
            }
        }

        /// <summary>
        /// Returns if a node is directly connected to another node either via inputs or outputs.
        /// 
        /// Used to detect cycles before they can be made.
        /// </summary>
        public static bool IsDirectlyConnected([NotNull] this IPipelineNode node, [NotNull] IPipelineNode target)
        {
            bool connected = false;

            // Try target -> node cycle first
            node.TraverseInputs(n =>
            {
                if (n == target)
                    connected = true;
                return !connected;
            });

            if (connected)
                return true;

            // Now try again just to see if we're not connected from node -> target instead
            target.TraverseInputs(n =>
            {
                if (n == node)
                    connected = true;
                return !connected;
            });
            
            return connected;
        }
        
        /// <summary>
        /// Returns a one-off observable that fetches the latest value of the Connections
        /// field every time it is subscribed to.
        /// </summary>
        public static IObservable<IPipelineOutput> ConnectionsObservable(this IPipelineInput input)
        {
            return Observable.Create<IPipelineOutput>(obs => input.Connections.ToObservable().Subscribe(obs));
        }

        /// <summary>
        /// Returns an observable sequence that is equal to the flatmap of <see cref="ConnectionsObservable"/>
        /// filtered by types <see cref="T"/>.
        /// </summary>
        public static IObservable<T> AnyConnection<T>(this IPipelineInput input)
        {
            return
                input.ConnectionsObservable()
                    .SelectMany(o => o.GetObservable())
                    .OfType<T>();
        }

        /// <summary>
        /// Returns an observable sequence that is equal to the flatmap of <see cref="ConnectionsObservable"/>
        /// filtered by types <see cref="T"/>.
        /// </summary>
        public static IObservable<T> AnyConnection<T>(this AbstractSinglePipelineInput<T> input)
        {
            return
                input.ConnectionsObservable()
                    .SelectMany(o => o.GetObservable())
                    .OfType<T>();
        }
    }
}

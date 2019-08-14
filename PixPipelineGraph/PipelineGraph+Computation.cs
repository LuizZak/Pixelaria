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
using System.Linq;
using System.Reactive;
using JetBrains.Annotations;

namespace PixPipelineGraph
{
    public partial class PipelineGraph
    {
        /// <summary>
        /// Returns the result of the computation when awaiting for a given pipeline output.
        /// </summary>
        public AnyObservable Compute(PipelineOutput output)
        {
            var inputs = InputsForNode(output.NodeId);
            var bodyInvocationContext = new PipelineBodyInvocationContext();

            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                var pipelineInput = GetInput(input);
                if (pipelineInput == null)
                    return PipelineBodyInvocationResponse.UnknownInputId(input);

                var response = ResponsesForInput(input);
                
                var argInput = new PipelineBodyInvocationContext.Input(pipelineInput.DataType, response, i);

                bodyInvocationContext.AddArgument(argInput);
            }

            var body = BodyForNode(output.NodeId);
            if (body == null)
                return PipelineBodyInvocationResponse.UnknownNodeId(output.NodeId);

            return body.Body.Invoke(bodyInvocationContext);
        }

        [CanBeNull]
        private AnyObservable ResponsesForInput(PipelineInput input)
        {
            var connections = ConnectionsTowardsInput(input);
            if (connections.Count == 0)
                return null;

            return connections.Aggregate(AnyObservable.FromObservables(new IObservable<Unit>[]{}), (observable, connection) => AnyObservable.Combine(observable, Compute(connection.Start)));
        }
    }
}

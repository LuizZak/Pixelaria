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
using JetBrains.Annotations;

namespace PixPipelineGraph
{
    /// <summary>
    /// Describes the response of an invocation to a <see cref="PipelineBody"/>.
    /// </summary>
    public class PipelineBodyInvocationResponse
    {
        /// <summary>
        /// Whether this response has an associated response object at <see cref="Output"/>.
        ///
        /// Is <c>false</c>, in case this response represents an exception response.
        /// </summary>
        public bool HasOutput { get; }

        /// <summary>
        /// The response object for this invocation.
        ///
        /// May be <c>null</c> even if <see cref="HasOutput"/> is <c>true</c>,
        /// in case an user registered a <c>null</c> response object.
        /// </summary>
        [CanBeNull]
        public object Output { get; }

        /// <summary>
        /// Gets an exception-like error that was raised by an invocation to the pipeline body.
        /// </summary>
        public Exception Error { get; }

        public PipelineBodyInvocationResponse(object output)
        {
            HasOutput = true;
            Output = output;
        }

        public PipelineBodyInvocationResponse(Exception error)
        {
            HasOutput = false;
            Error = error;
        }
    }
}
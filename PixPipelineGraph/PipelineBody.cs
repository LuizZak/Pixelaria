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
    /// Represents the transformation body of a pipeline node.
    /// </summary>
    public class PipelineBody
    {
        /// <summary>
        /// Gets the ID for this pipeline body
        /// </summary>
        public PipelineBodyId Id { get; }

        /// <summary>
        /// The input type of the body.
        ///
        /// If <c>null</c>, this body takes no parameters.
        /// </summary>
        [CanBeNull]
        public Type InputType { get; set; }

        /// <summary>
        /// The output type of the body.
        ///
        /// If <c>null</c>, this body produces no output and is a consumer only.
        /// </summary>
        [CanBeNull]
        public Type OutputType { get; set; }

        /// <summary>
        /// The untyped delegate for this body.
        /// </summary>
        [NotNull]
        public Func<object, object> Body { get; set; }

        public PipelineBody(PipelineBodyId id, Type inputType, Type outputType, [NotNull] Func<object, object> body)
        {
            Id = id;
            InputType = inputType;
            OutputType = outputType;
            Body = body;
        }
    }
}
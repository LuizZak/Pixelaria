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
        /// The indexed input types of the body.
        /// </summary>
        public Type[] InputTypes { get; set; }

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
        public Func<IPipelineBodyInvocationContext, object> Body { get; set; }

        public PipelineBody(PipelineBodyId id, Type[] inputTypes, Type outputType, [NotNull] Func<IPipelineBodyInvocationContext, object> body)
        {
            Id = id;
            InputTypes = inputTypes;
            OutputType = outputType;
            Body = body;
        }
    }

    /// <summary>
    /// Describes the invocation context of a pipeline body
    /// </summary>
    public interface IPipelineBodyInvocationContext
    {
        /// <summary>
        /// Gets the number of inputs available to consume.
        /// </summary>
        int InputCount { get; }

        /// <summary>
        /// Returns <c>true</c> if there's any input available on a given input index.
        ///
        /// Input values still be <c>null</c>, in case <c>null</c> was the value itself provided
        /// to the input.
        /// </summary>
        bool HasInputAtIndex(int index);

        /// <summary>
        /// Gets an object at a given input index on this invocation context.
        ///
        /// This value may be <c>null</c>, in case the input was itself provided null, or not connected to an output.
        /// </summary>
        [CanBeNull]
        T GetIndexedInput<T>(int index);

        /// <summary>
        /// Helper method for fetching indexed inputs by matching type.
        ///
        /// Returns <c>true</c> iff all input types match the provided generic signatures by type cast.
        /// </summary>
        bool GetIndexedInputs<T1>(out T1 t1);
        /// <summary>
        /// Helper method for fetching indexed inputs by matching type.
        ///
        /// Returns <c>true</c> iff all input types match the provided generic signatures by type cast.
        /// </summary>
        bool GetIndexedInputs<T1, T2>(out T1 t1, out T2 t2);
        /// <summary>
        /// Helper method for fetching indexed inputs by matching type.
        ///
        /// Returns <c>true</c> iff all input types match the provided generic signatures by type cast.
        /// </summary>
        bool GetIndexedInputs<T1, T2, T3>(out T1 t1, out T2 t2, out T3 t3);
        /// <summary>
        /// Helper method for fetching indexed inputs by matching type.
        ///
        /// Returns <c>true</c> iff all input types match the provided generic signatures by type cast.
        /// </summary>
        bool GetIndexedInputs<T1, T2, T3, T4>(out T1 t1, out T2 t2, out T3 t3, out T4 t4);
        /// <summary>
        /// Helper method for fetching indexed inputs by matching type.
        ///
        /// Returns <c>true</c> iff all input types match the provided generic signatures by type cast.
        /// </summary>
        bool GetIndexedInputs<T1, T2, T3, T4, T5>(out T1 t1, out T2 t2, out T3 t3, out T4 t4, out T5 t5);

        /// <summary>
        /// Returns <c>true</c> if all indexed inputs match the given expected types, using <see cref="Type.IsAssignableFrom"/>
        /// as a test of compatibility.
        ///
        /// If the count of inputs mismatch, <c>false</c> is returned, instead.
        /// </summary>
        bool MatchesInputTypes([NotNull] IReadOnlyList<Type> inputTypes);
    }
}
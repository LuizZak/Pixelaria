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
using System.Reactive;
using System.Reactive.Disposables;
using JetBrains.Annotations;

namespace PixPipelineGraph
{
    /// <summary>
    /// Describes the response of an invocation to a <see cref="PipelineBody"/>.
    /// </summary>
    public class PipelineBodyInvocationResponse
    {
        /// <summary>
        /// A common response to invoke when a pipeline invocation was performed with an unknown node ID.
        /// </summary>
        public static AnyObservable UnknownNodeId(PipelineNodeId nodeId)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<Unit>(observer =>
            {
                observer.OnError(new Exception($"Non-existing node ID {nodeId}"));
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// A common response to invoke when a pipeline invocation was performed with an unknown input ID.
        /// </summary>
        public static AnyObservable UnknownInputId(PipelineInput inputId)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<Unit>(observer =>
            {
                observer.OnError(new Exception($"Non-existing input ID {inputId}"));
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// A common response to invoke when a pipeline invocation was performed in an output that has no input connections.
        /// </summary>
        public static AnyObservable NotConnected
        {
            get
            {
                return AnyObservable.FromObservable(new AnonymousObservable<Unit>(observer =>
                {
                    observer.OnError(new NotConnectedException());
                    return Disposable.Empty;
                }));
            }
        }

        /// <summary>
        /// A common response to invoke when a pipeline invocation was performed with unexpected input types.
        /// </summary>
        public static AnyObservable MismatchedInputType(Type expected)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<Unit>(observer =>
            {
                observer.OnError(new Exception($"Mismatched input types: Expected {expected}"));
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// Whether this response has an associated response object at <see cref="Output"/>.
        ///
        /// Is <c>false</c>, in case this response represents an exception response.
        /// </summary>
        public bool HasOutput { get; }

        /// <summary>
        /// Gets the type of this response.
        ///
        /// Is <see cref="Exception"/>, in case this is an exception response.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// The response object for this invocation.
        ///
        /// May be <c>null</c> even if <see cref="HasOutput"/> is <c>true</c>,
        /// in case an user registered a <c>null</c> response object.
        /// </summary>
        [CanBeNull]
        public AnyObservable Output { get; }

        /// <summary>
        /// Gets an exception-like error that was raised by an invocation to the pipeline body.
        /// </summary>
        public Exception Error { get; }

        private PipelineBodyInvocationResponse(AnyObservable output, Type type)
        {
            HasOutput = true;
            Output = output;
            Type = type;
        }

        public PipelineBodyInvocationResponse(Exception error)
        {
            HasOutput = false;
            Error = error;
            Type = typeof(Exception);
        }

        public static PipelineBodyInvocationResponse Response<T>([NotNull] IObservable<T> output)
        {
            var response = new PipelineBodyInvocationResponse(AnyObservable.FromObservable(output), typeof(T));
            return response;
        }

        public static AnyObservable Exception<T>(Exception e)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<T>(observer =>
            {
                observer.OnError(e);
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// Combines all provided responses into a single condensed response.
        ///
        /// The resulting response will combine all output observables, as well as be marked with the exception
        /// from the first exceptional response in the enumerable.
        ///
        /// The type of the responses should be the same, otherwise the resulting response type is undefined.
        /// </summary>
        public static PipelineBodyInvocationResponse Combine([NotNull] IEnumerable<PipelineBodyInvocationResponse> responses)
        {
            var type = typeof(object);
            var result = AnyObservable.FromObservables(new IObservable<Unit>[] {});
            foreach (var response in responses)
            {
                type = response.Type;

                if (response.Error != null)
                    return new PipelineBodyInvocationResponse(response.Error);

                if (response.Output != null)
                    result = AnyObservable.Combine(result, response.Output);
            }

            return new PipelineBodyInvocationResponse(result, type);
        }
    }

    public class NotConnectedException : Exception
    {
        public NotConnectedException() : base("Not Connected")
        {
            
        }
    }
}
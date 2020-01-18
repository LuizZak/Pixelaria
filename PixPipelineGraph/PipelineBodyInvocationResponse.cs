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
using System.Reactive;
using System.Reactive.Disposables;

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
        public static AnyObservable UnknownNodeId<T>(PipelineNodeId nodeId)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<T>(observer =>
            {
                observer.OnError(new Exception($"Non-existing node ID {nodeId}"));
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// A common response to invoke when a pipeline invocation was performed with an unknown input ID.
        /// </summary>
        public static AnyObservable UnknownInputId<T>(PipelineInput inputId)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<T>(observer =>
            {
                observer.OnError(new Exception($"Non-existing input ID {inputId}"));
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// Creates aa response to use when a pipeline computation resulted in less output values being
        /// produced than expected.
        /// </summary>
        public static AnyObservable MissingOutput<T>(PipelineOutput output)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<T>(observer =>
            {
                observer.OnError(new Exception($"Missing output for pipeline output {output}"));
                return Disposable.Empty;
            }));
        }

        /// <summary>
        /// A common response to invoke when a pipeline invocation was performed with unexpected input types.
        /// </summary>
        public static AnyObservable MismatchedInputType<T>(Type expected)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<T>(observer =>
            {
                observer.OnError(new Exception($"Mismatched input types: Expected {expected}"));
                return Disposable.Empty;
            }));
        }

        public static AnyObservable Exception<T>(Exception e)
        {
            return AnyObservable.FromObservable(new AnonymousObservable<T>(observer =>
            {
                observer.OnError(e);
                return Disposable.Empty;
            }));
        }
    }

    public class NotConnectedException : Exception
    {
        public NotConnectedException() : base("Not Connected")
        {
            
        }
    }
}
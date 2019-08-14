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
using System.Reactive.Subjects;
using JetBrains.Annotations;
using PixPipelineGraph;

namespace PixPipelineGraphTests
{
    public class MockPipelineBodyProvider : IPipelineGraphBodyProvider
    {
        public Dictionary<PipelineBodyId, PipelineBody> Bodies = new Dictionary<PipelineBodyId, PipelineBody>();

        public PipelineBodyId Register(Type[] inputTypes, Type outputType, [NotNull] Func<IPipelineBodyInvocationContext, AnyObservable> body)
        {
            var bodyId = new PipelineBodyId(Guid.NewGuid().ToString());

            var pipelineBody = new PipelineBody(bodyId, inputTypes, outputType, body);

            Bodies[bodyId] = pipelineBody;

            return bodyId;
        }

        public PipelineBodyId Register<T>(Type[] inputTypes, [NotNull] Func<IPipelineBodyInvocationContext, T> body)
        {
            var bodyId = new PipelineBodyId(Guid.NewGuid().ToString());

            var pipelineBody = new PipelineBody(bodyId, inputTypes, typeof(T), context =>
            {
                try
                {
                    var subject = new ReplaySubject<T>();
                    subject.OnNext(body(context));
                    subject.OnCompleted();
                    return new AnyObservable(subject);
                }
                catch (Exception e)
                {
                    return new AnyObservable(new AnonymousObservable<T>(observer =>
                    {
                        observer.OnError(e);
                        return Disposable.Empty;
                    }));
                }
            });

            Bodies[bodyId] = pipelineBody;

            return bodyId;
        }

        public PipelineBody GetBody(PipelineBodyId id)
        {
            if (Bodies.TryGetValue(id, out var value))
            {
                return value;
            }

            return new PipelineBody(id, new[] { typeof(int) }, typeof(int), o => new AnyObservable(new Subject<object>()));
        }
    }
}
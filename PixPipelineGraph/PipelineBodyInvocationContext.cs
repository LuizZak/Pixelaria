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
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace PixPipelineGraph
{
    public class PipelineBodyInvocationContext: IPipelineBodyInvocationContext
    {
        private readonly List<LazyInput> _lazyInputs = new List<LazyInput>();

        public int InputCount => _lazyInputs.Count;

        public PipelineBodyInvocationContext([NotNull] IEnumerable<PipelineInput> inputs, [NotNull] PipelineGraph graph)
        {
            foreach (var input in inputs)
            {
                _lazyInputs.Add(new LazyInput(input, graph));
            }
        }
        
        public bool HasInputAtIndex(int index)
        {
            return _lazyInputs.Any(i => i.PipelineInput.Index == index);
        }

        public IObservable<T> GetIndexedInput<T>(int index)
        {
            return _lazyInputs.FirstOrDefault(i => i.PipelineInput.Index == index)?.Input.Observable?.ToObservable<T>();
        }

        #region Exception Get

        public void GetIndexedInputs<T1>(out IObservable<T1> t1)
        {
            var inputs = OrderedInputValues(1);
            if (inputs.Count != 1 || inputs.Any(i => i == null))
                throw new NotConnectedException();

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast)
            {
                t1 = t1Cast;
                return;
            }

            throw new AnyObservable.TypeMismatchException($"Expected input type {typeof(T1)}");
        }

        public void GetIndexedInputs<T1, T2>(out IObservable<T1> t1, out IObservable<T2> t2)
        {
            var inputs = OrderedInputValues(2);
            if (inputs.Count != 2 || inputs.Any(i => i == null))
                throw new NotConnectedException();

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast && inputs[1].ToObservable<T2>() is IObservable<T2> t2Cast)
            {
                t1 = t1Cast;
                t2 = t2Cast;
                return;
            }

            throw new AnyObservable.TypeMismatchException($"Expected input type {typeof(T1)}");
        }

        #endregion

        #region Try Get

        public bool TryGetIndexedInputs<T1>(out IObservable<T1> t1)
        {
            t1 = default;

            var inputs = OrderedInputValues(1);
            if (inputs.Count != 1 || inputs.Any(i => i == null))
                return false;

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast)
            {
                t1 = t1Cast;
                return true;
            }

            return false;
        }

        public bool TryGetIndexedInputs<T1, T2>(out IObservable<T1> t1, out IObservable<T2> t2)
        {
            t1 = default;
            t2 = default;

            var inputs = OrderedInputValues(2);
            if (inputs.Count != 2 || inputs.Any(i => i == null))
                return false;

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast && inputs[1].ToObservable<T2>() is IObservable<T2> t2Cast)
            {
                t1 = t1Cast;
                t2 = t2Cast;
                return true;
            }

            return false;
        }

        public bool TryGetIndexedInputs<T1, T2, T3>(out IObservable<T1> t1, out IObservable<T2> t2, out IObservable<T3> t3)
        {
            t1 = default;
            t2 = default;
            t3 = default;

            var inputs = OrderedInputValues(3);
            if (inputs.Count != 3 || inputs.Any(i => i == null))
                return false;

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast && inputs[1].ToObservable<T2>() is IObservable<T2> t2Cast && inputs[2].ToObservable<T3>() is IObservable<T3> t3Cast)
            {
                t1 = t1Cast;
                t2 = t2Cast;
                t3 = t3Cast;
                return true;
            }

            return false;
        }

        public bool TryGetIndexedInputs<T1, T2, T3, T4>(out IObservable<T1> t1, out IObservable<T2> t2, out IObservable<T3> t3, out IObservable<T4> t4)
        {
            t1 = default;
            t2 = default;
            t3 = default;
            t4 = default;

            var inputs = OrderedInputValues(4);
            if (inputs.Count != 4 || inputs.Any(i => i == null))
                return false;

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast && inputs[1].ToObservable<T2>() is IObservable<T2> t2Cast && inputs[2].ToObservable<T3>() is IObservable<T3> t3Cast && inputs[3].ToObservable<T4>() is IObservable<T4> t4Cast)
            {
                t1 = t1Cast;
                t2 = t2Cast;
                t3 = t3Cast;
                t4 = t4Cast;
                return true;
            }

            return false;
        }

        public bool TryGetIndexedInputs<T1, T2, T3, T4, T5>(out IObservable<T1> t1, out IObservable<T2> t2, out IObservable<T3> t3, out IObservable<T4> t4, out IObservable<T5> t5)
        {
            t1 = default;
            t2 = default;
            t3 = default;
            t4 = default;
            t5 = default;

            var inputs = OrderedInputValues(5);
            if (inputs.Count != 5 || inputs.Any(i => i == null))
                return false;

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast && inputs[1].ToObservable<T2>() is IObservable<T2> t2Cast && inputs[2].ToObservable<T3>() is IObservable<T3> t3Cast && inputs[3].ToObservable<T4>() is IObservable<T4> t4Cast && inputs[4].ToObservable<T5>() is IObservable<T5> t5Cast)
            {
                t1 = t1Cast;
                t2 = t2Cast;
                t3 = t3Cast;
                t4 = t4Cast;
                t5 = t5Cast;
                return true;
            }

            return false;
        }

        #endregion

        private IEnumerable<LazyInput> OrderedInputs()
        {
            int maxIndex = _lazyInputs.Max(i => i.PipelineInput.Index);
            var result = new List<LazyInput>();

            for (int i = 0; i <= maxIndex; i++)
            {
                result.Add(null);
            }

            foreach (var input in _lazyInputs)
            {
                result[input.PipelineInput.Index] = input;
            }

            return result;
        }

        private IReadOnlyList<AnyObservable> OrderedInputValues(int maxCount)
        {
            return OrderedInputs().Take(maxCount).Select(v => v?.Input.Observable).ToArray();
        }

        public bool MatchesInputTypes(IReadOnlyList<Type> inputTypes)
        {
            var orderedInputTypes = OrderedInputs().Select(i => i?.Input.Type).ToArray();
            if (orderedInputTypes.Length != inputTypes.Count)
                return false;

            return orderedInputTypes.Zip(inputTypes, (param, valueType) => (param, valueType)).All(types => types.valueType.IsAssignableFrom(types.param));
        }

        public struct Input
        {
            public Type Type { get; }
            [CanBeNull]
            public AnyObservable Observable { get; }
            public int Index { get; }

            public Input(Type type, [CanBeNull] AnyObservable observable, int index)
            {
                Type = type;
                Observable = observable;
                Index = index;
            }
        }

        private class LazyInput
        {
            private readonly PipelineGraph _graph;
            private Input? _input;

            public PipelineInput PipelineInput { get; }

            public Input Input
            {
                get
                {
                    if(_input == null)
                        _input = Instantiate();

                    return _input.Value;
                }
            }

            public LazyInput(PipelineInput pipelineInput, PipelineGraph graph)
            {
                PipelineInput = pipelineInput;
                _graph = graph;
            }

            private Input Instantiate()
            {
                var pipelineInput = _graph.GetInput(PipelineInput);
                Input argInput;
                if (pipelineInput == null)
                {
                    argInput = new Input(typeof(object), PipelineBodyInvocationResponse.UnknownInputId<Unit>(PipelineInput), PipelineInput.Index);
                }
                else
                {
                    var response = _graph.ResponsesForInput(PipelineInput);
                    argInput = new Input(pipelineInput.DataType, response, PipelineInput.Index);
                }

                return argInput;
            }
        }
    }

    public class AnyObservable
    {
        public static AnyObservable Empty = new AnyObservable(new object[0]);

        private readonly object[] _underlying;

        private AnyObservable(object underlying)
        {
            _underlying = new []{ underlying };
        }

        private AnyObservable(object[] underlying)
        {
            _underlying = underlying;
        }

        public IObservable<T> ToObservable<T>()
        {
            var list = new List<IObservable<T>>();

            foreach (var o in _underlying)
            {
                if (o is IObservable<T> observable)
                {
                    list.Add(observable);
                }
                else
                {
                    return new AnonymousObservable<T>(observer =>
                    {
                        observer.OnError(new TypeMismatchException($"Mismatched observable type IObservable<{typeof(T)}> (have observable of type {_underlying.GetType()})"));

                        return Disposable.Empty;
                    });
                }
            }

            return list.Concat();
        }

        public static AnyObservable Combine([NotNull] AnyObservable first, [NotNull] AnyObservable second)
        {
            return new AnyObservable(first._underlying.Concat(second._underlying).ToArray());
        }

        public static AnyObservable FromObservable<T>(IObservable<T> observable)
        {
            return new AnyObservable(observable);
        }

        public static AnyObservable FromObservables<T>([NotNull] IReadOnlyList<IObservable<T>> observables)
        {
            return new AnyObservable(observables.Cast<object>().ToArray());
        }

        public class TypeMismatchException : Exception
        {
            public TypeMismatchException(string message) : base(message)
            {

            }
        }
    }
}
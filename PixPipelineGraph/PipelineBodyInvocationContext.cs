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

namespace PixPipelineGraph
{
    public class PipelineBodyInvocationContext: IPipelineBodyInvocationContext
    {
        private readonly List<Input> _inputs = new List<Input>();

        public int InputCount => _inputs.Count;

        public void AddArgument(Input input)
        {
            _inputs.Add(input);
        }

        public bool HasInputAtIndex(int index)
        {
            return _inputs.Any(i => i.Index == index);
        }

        public IObservable<T> GetIndexedInput<T>(int index)
        {
            return _inputs.FirstOrDefault(i => i.Index == index).Observable?.ToObservable<T>();
        }

        public bool GetIndexedInputs<T1>(out IObservable<T1> t1)
        {
            t1 = default;

            var inputs = OrderedInputValues();
            if (inputs.Count != 1 || inputs.Any(i => i == null))
                return false;

            if (inputs[0].ToObservable<T1>() is IObservable<T1> t1Cast)
            {
                t1 = t1Cast;
                return true;
            }

            return false;
        }

        public bool GetIndexedInputs<T1, T2>(out IObservable<T1> t1, out IObservable<T2> t2)
        {
            t1 = default;
            t2 = default;

            var inputs = OrderedInputValues();
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

        public bool GetIndexedInputs<T1, T2, T3>(out IObservable<T1> t1, out IObservable<T2> t2, out IObservable<T3> t3)
        {
            t1 = default;
            t2 = default;
            t3 = default;

            var inputs = OrderedInputValues();
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

        public bool GetIndexedInputs<T1, T2, T3, T4>(out IObservable<T1> t1, out IObservable<T2> t2, out IObservable<T3> t3, out IObservable<T4> t4)
        {
            t1 = default;
            t2 = default;
            t3 = default;
            t4 = default;

            var inputs = OrderedInputValues();
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

        public bool GetIndexedInputs<T1, T2, T3, T4, T5>(out IObservable<T1> t1, out IObservable<T2> t2, out IObservable<T3> t3, out IObservable<T4> t4, out IObservable<T5> t5)
        {
            t1 = default;
            t2 = default;
            t3 = default;
            t4 = default;
            t5 = default;

            var inputs = OrderedInputValues();
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

        private IEnumerable<Input?> OrderedInputs()
        {
            var maxIndex = _inputs.Max(i => i.Index);
            var result = new List<Input?>();

            for (int i = 0; i <= maxIndex; i++)
            {
                result.Add(null);
            }

            foreach (var input in _inputs)
            {
                result[input.Index] = input;
            }

            return result;
        }

        private IReadOnlyList<AnyObservable> OrderedInputValues()
        {
            return OrderedInputs().Select(v => v?.Observable).ToArray();
        }

        public bool MatchesInputTypes(IReadOnlyList<Type> inputTypes)
        {
            var orderedInputTypes = OrderedInputs().Select(i => i?.Type).ToArray();
            if (orderedInputTypes.Length != inputTypes.Count)
                return false;

            return orderedInputTypes.Zip(inputTypes, (param, valueType) => (param, valueType)).All(types => types.valueType.IsAssignableFrom(types.param));
        }

        public struct Input
        {
            public Type Type { get; }
            public AnyObservable Observable { get; }
            public int Index { get; }

            public Input(Type type, AnyObservable observable, int index)
            {
                Type = type;
                Observable = observable;
                Index = index;
            }
        }
    }

    public class AnyObservable
    {
        private readonly object _underlying;

        public AnyObservable(object underlying)
        {
            _underlying = underlying;
        }

        public IObservable<T> ToObservable<T>()
        {
            if (_underlying is IObservable<T> observable)
            {
                return observable;
            }

            return new AnonymousObservable<T>(observer =>
            {
                observer.OnError(new TypeMismatchException($"Mismatched observable type IObservable<{typeof(T)}> (have observable of type {_underlying.GetType()})"));

                return Disposable.Empty;
            });
        }

        public class TypeMismatchException : Exception
        {
            public TypeMismatchException(string message) : base(message)
            {

            }
        }
    }
}
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
using JetBrains.Annotations;

namespace PixPipelineGraph
{
    internal class PipelineBuildStepCollection<T>
    {
        private readonly List<IPipelineBuildStep<T>> _steps = new List<IPipelineBuildStep<T>>();

        internal void Apply(T obj)
        {
            foreach (var step in _steps)
            {
                step.Perform(obj);
            }
        }

        internal void AddClosureBuilderStep(Action<T> closure)
        {
            var step = new ClosureBuilderStep<T>(closure);
            _steps.Add(step);
        }
    }

    internal interface IPipelineBuildStep<in T>
    {
        void Perform([NotNull] T obj);
    }

    internal struct ClosureBuilderStep<T> : IPipelineBuildStep<T>
    {
        private readonly Action<T> _closure;

        internal ClosureBuilderStep(Action<T> closure)
        {
            _closure = closure;
        }

        public void Perform(T node)
        {
            _closure(node);
        }
    }

    public interface IPipelineLazyValue<out T>
    {
        /// <summary>
        /// Gets a value specifying whether this lazy container has a proper value set.
        /// </summary>
        bool HasValue { get; }

        /// <summary>
        /// Gets the associated lazy value for this lazy container.
        /// </summary>
        /// <exception cref="InvalidOperationException">If no value has been set yet, an exception is raised. Use <see cref="HasValue"/> to inspect the existence of a value before attempting to read it.</exception>
        T LazyValue { get; }
    }

    internal class PipelineLazyValue<T> : IPipelineLazyValue<T>
    {
        public bool HasValue { get; private set; }
        private T Value { get; set; }

        public void SetLazyValue(T value)
        {
            HasValue = true;
            Value = value;
        }

        public T LazyValue
        {
            get
            {
                if (!HasValue)
                {
                    throw new InvalidOperationException(
                        "Cannot extract lazy value until it has been set");
                }

                return Value;
            }
        }
    }
}
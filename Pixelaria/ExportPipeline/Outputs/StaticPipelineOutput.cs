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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline.Outputs
{
    /// <summary>
    /// A simple output source that feeds a single static value on every subscription.
    /// </summary>
    /// <typeparam name="T">The type of object output by this static pipeline output</typeparam>
    public class StaticPipelineOutput<T> : IPipelineOutput
    {
        private readonly T _value;
        
        public virtual string Name { get; }
        public IPipelineNode Node { get; } = null;

        public Type DataType { get; } = typeof(T);

        public StaticPipelineOutput(T value, [NotNull] string name)
        {
            _value = value;
            Name = name;
        }

        public IObservable<object> GetObservable()
        {
            return Observable.Create<T>(obs =>
            {
                obs.OnNext(_value);
                obs.OnCompleted();

                return Disposable.Empty;
            }).Select(o => (object)o);
        }

        public IPipelineMetadata GetMetadata()
        {
            return new PipelineMetadata("static");
        }
    }
}
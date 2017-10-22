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
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline.Outputs
{
    /// <summary>
    /// A base implementation for a simple pipeline output that outputs a single type.
    /// 
    /// Implements most of the boilerplate related to implementing a basic pipeline output
    /// </summary>
    /// <typeparam name="T">The type of objects that this class will output</typeparam>
    public abstract class AbstractPipelineOutput<T> : IPipelineOutput
    {
        /// <summary>
        /// Observable that content will be pushed to.
        /// </summary>
        [NotNull]
        public IObservable<T> Source;

        public string Name { get; protected set; } = "";
        public IPipelineNode Node { get; }
        public Type DataType => typeof(T);

        protected AbstractPipelineOutput([NotNull] IPipelineNode step, [NotNull] IObservable<T> source)
        {
            Node = step;
            Source = source;
        }

        public IObservable<object> GetConnection()
        {
            return Source.Select(value => (object)value);
        }

        public abstract IPipelineMetadata GetMetadata();
    }
}
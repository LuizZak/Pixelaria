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
using System.Reactive.Linq;
using JetBrains.Annotations;

namespace Pixelaria.ExportPipeline.Inputs
{
    /// <summary>z
    /// A base implementation for a simple pipeline input that accepts a single type.
    /// 
    /// Implements most of the boilerplate related to implementing a basic pipeline input
    /// </summary>
    /// <typeparam name="T">The type of objects this input will accept</typeparam>
    public abstract class AbstractSinglePipelineInput<T> : IPipelineInput
    {
        private readonly List<IPipelineOutput> _connections = new List<IPipelineOutput>();

        public string Name { get; protected set; } = "";
        public IPipelineNode Node { get; }
        public Type[] DataTypes => new[] { typeof(T) };
        public IPipelineOutput[] Connections => _connections.ToArray();

        /// <summary>
        /// Returns a one-off observable that fetches the latest value of the Connections
        /// field everytime it is subscribed to.
        /// </summary>
        public IObservable<IPipelineOutput> ConnectionsObservable
        {
            get
            {
                return Observable.Create<IPipelineOutput>(obs => Connections.ToObservable().Subscribe(obs));
            }
        }

        /// <summary>
        /// Returns an observable sequence that is equal to the flatmap of <see cref="ConnectionsObservable"/>
        /// filtered by types <see cref="T"/>.
        /// </summary>
        public IObservable<T> AnyConnection()
        {
            return
                ConnectionsObservable
                    .SelectMany(o => o.GetObservable())
                    .OfType<T>();
        }

        protected AbstractSinglePipelineInput([NotNull] IPipelineNode step)
        {
            Node = step;
        }
        
        public IPipelineLinkConnection Connect(IPipelineOutput output)
        {
            if (_connections.Contains(output))
                return null;

            _connections.Add(output);
            return new PipelineLinkConnection(this, output);
        }

        public void Disconnect(IPipelineOutput output)
        {
            _connections.Remove(output);
        }

        public abstract IPipelineMetadata GetMetadata();
    }

    /// <summary>
    /// A generic pipeline output.
    /// Used for basic input value types that don't need special handling.
    /// </summary>
    public sealed class GenericPipelineInput<T> : AbstractSinglePipelineInput<T>
    {
        public GenericPipelineInput([NotNull] IPipelineNode step, [NotNull] string name) : base(step)
        {
            Name = name;
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}
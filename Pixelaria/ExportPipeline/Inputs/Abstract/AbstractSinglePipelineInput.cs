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
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline.Inputs.Abstract
{
    /// <summary>z
    /// A base implementation for a simple pipeline input that accepts a single type.
    /// 
    /// Implements most of the boilerplate related to implementing a basic pipeline input
    /// </summary>
    /// <typeparam name="T">The type of objects this input will accept</typeparam>
    public abstract class AbstractSinglePipelineInput<T> : IPipelineInput
    {
        public PipelineNodeId NodeId => Node.Id;

        public string Name { get; set; } = "";
        public IPipelineNode Node { get; }
        public PipelineInput Id { get; }
        public IReadOnlyList<Type> DataTypes => new[] { typeof(T) };
        
        protected AbstractSinglePipelineInput([NotNull] IPipelineNode step, PipelineInput id)
        {
            Node = step;
            Id = id;
        }
        
        public abstract IPipelineMetadata GetMetadata();
    }

    /// <summary>
    /// A generic pipeline output.
    /// Used for basic input value types that don't need special handling.
    /// </summary>
    public sealed class GenericPipelineInput<T> : AbstractSinglePipelineInput<T>
    {
        public GenericPipelineInput([NotNull] IPipelineNode step, [NotNull] string name, PipelineInput id) : base(step, id)
        {
            Name = name;
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }
}
﻿/*
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
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Pixelaria.Filters;
using Pixelaria.Utils;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline.NodeBodies
{
    class DefaultPipelineGraphBodyProvider: IPipelineGraphBodyProvider
    {
        List<BasePipelineNodeEntry> _entries = new List<BasePipelineNodeEntry>();

        public PipelineBody GetBody(PipelineBodyId id)
        {
            return _entries.FirstOrDefault(entry => entry.BodyId == id)?.Create();
        }
    }

    public abstract class BasePipelineNodeEntry
    {
        /// <summary>
        /// Gets the ID for this pipeline node entry.
        ///
        /// This ID will be the same for all pipeline bodies created with <see cref="Create"/>.
        /// </summary>
        public PipelineBodyId BodyId { get; protected set; }

        protected BasePipelineNodeEntry()
        {
            BodyId = new PipelineBodyId(GetType().Name);
        }

        /// <summary>
        /// Configures a given <see cref="PipelineNodeBuilder"/> to the specifications of this
        /// pipeline node's entry.
        /// </summary>
        public abstract void CreateNode([NotNull] PipelineNodeBuilder builder);

        /// <summary>
        /// Invokes the initializer for a new pipeline body from this pipeline body entry.
        /// </summary>
        [NotNull]
        public abstract PipelineBody Create();
    }

    public class FilterNodeEntry<T> : BasePipelineNodeEntry where T : IFilter
    {
        public override void CreateNode(PipelineNodeBuilder builder)
        {
            var newFilter = (IFilter)Activator.CreateInstance(typeof(T));
            var properties = newFilter.InspectableProperties();

            builder.CreateInput("Bitmap", input => { input.SetInputType(typeof(Bitmap)); });

            foreach (var propertyInfo in properties)
            {
                string name = Utilities.DePascalCase(propertyInfo.Name);
                builder.CreateInput(name, input =>
                {
                    input.SetInputType(propertyInfo.PropertyType);
                });
            }

            builder.CreateOutput("Bitmap", output =>
            {
                output.SetOutputType(typeof(Bitmap));
            });
        }

        public override PipelineBody Create()
        {
            var inputList = new List<Type> {typeof(Bitmap)};

            var newFilter = (IFilter)Activator.CreateInstance(typeof(T));

            var properties = newFilter.InspectableProperties();
            foreach (var info in properties)
            {
                inputList.Add(info.PropertyType);
            }

            return new PipelineBody(BodyId, inputList.ToArray(), typeof(Bitmap), context =>
            {
                if (context.GetIndexedInputs(out IObservable<Bitmap> bitmap))
                {
                    //newFilter.ApplyToBitmap(bitmap);
                }

                return new PipelineBodyInvocationResponse(null);
            });
        }
    }
}

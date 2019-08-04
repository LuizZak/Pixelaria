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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using FastBitmapLib;
using JetBrains.Annotations;

using Pixelaria.Data.Exports;
using Pixelaria.ExportPipeline.Inputs;
using Pixelaria.ExportPipeline.Inputs.Abstract;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps.Abstract;
using Pixelaria.Filters;
using Pixelaria.Utils;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// Pipeline step that applies a filter onto a Bitmap and passes a copy of it
    /// forward.
    /// </summary>
    public sealed class FilterPipelineStep<T> : AbstractPipelineStep where T: IFilter, new()
    {
        [NotNull]
        public T Filter;

        public override string Name => Filter.Name;
        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        /// <summary>
        /// Creates a filter pipeline step with an empty filter instance constructed from <see cref="T"/>'s default constructor.
        /// </summary>
        public FilterPipelineStep() : this(new T())
        {

        }

        public FilterPipelineStep([NotNull] T filter)
        {
            var props = filter.InspectableProperties();

            Filter = filter;

            var bitmapInput = new PipelineBitmapInput(this);

            var inputs = new IPipelineInput[props.Length + 1];

            // First input is always the input bitmap
            inputs[0] = bitmapInput;

            var inputObs = new List<IObservable<object>>();

            for (int i = 0; i < props.Length; i++)
            {
                var propertyInfo = props[i];
                string name = Utilities.DePascalCase(propertyInfo.Name);

                var pipelineInput = typeof(GenericPipelineInput<>).MakeGenericType(propertyInfo.PropertyType);
                var constructor =
                    pipelineInput.GetConstructor(new[] {typeof(IPipelineNode), typeof(string)});

                Debug.Assert(constructor != null, "constructor != null");

                var input = (IPipelineInput) constructor.Invoke(new object[] {this, name});
                
                // Tie input to property
                var obs =
                    input.ConnectionsObservable()
                        .SelectMany(o => o.GetObservable())
                        .Where(o => propertyInfo.PropertyType.IsInstanceOfType(o));

                inputObs.Add(obs);

                inputs[i + 1] = input;
            }

            Input = inputs;

            var bitmaps = bitmapInput.AnyConnection();

            var source =
                bitmaps
                    .PxlWithLatestFrom(inputObs.Zip(), (bitmap, list) => (bitmap, list))
                    .Select(res =>
                    {
                        var (bitmap, propList) = res;

                        foreach (var (o, prop) in propList.Zip(props, (o, info) => (o, info)))
                        {
                            prop.SetValue(filter, o);
                        }

                        // Clone before applying filter
                        var bit = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);
                        FastBitmap.CopyPixels(bitmap, bit);

                        filter.ApplyToBitmap(bit);

                        return bit;
                    });
            
            Output = new IPipelineOutput[] {new PipelineBitmapOutput(this, source)};
        }

        public override IPipelineMetadata GetMetadata()
        {
            return PipelineMetadata.Empty;
        }
    }

    /// <summary>
    /// Pipeline step that applies a filter onto a Bitmap and passes a copy of it
    /// forward.
    /// </summary>
    internal sealed class TransparencyFilterPipelineStep : AbstractPipelineStep
    {
        public override string Name => "Transparency Filter";
        
        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public TransparencyFilterPipelineStep()
        {
            var alphaInput = new GenericPipelineInput<float>(this, "Alpha");
            var bitmapInput = new FilterBitmapInput(this);

            Input = new IPipelineInput[]
            {
                bitmapInput,
                alphaInput
            };
            
            var bitmapConnections = bitmapInput.AnyConnectionBitmap();
            var transp = alphaInput.AnyConnection();

            var source =
                bitmapConnections
                    .PxlWithLatestFrom(transp, (bitmap, alpha) => (bitmap, alpha))
                    .Select(tup =>
                    {
                        var (bitmap, alpha) = tup;

                        // Clone before applying filter
                        var bit = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

                        var filter = new TransparencyFilter {Transparency = alpha};
                        filter.ApplyToBitmap(bit);

                        return bit;
                    });

            Output = new IPipelineOutput[] { new PipelineBitmapOutput(this, source) };
        }

        /// <summary>
        /// Input that accepts bitmaps
        /// </summary>
        public class FilterBitmapInput : IPipelineInput
        {
            private readonly List<IPipelineOutput> _connections = new List<IPipelineOutput>();

            public IPipelineNode Node { get; }
            public string Name { get; }

            public Type[] DataTypes { get; } = {typeof(Bitmap), typeof(BundleSheetExport)};
            public IPipelineOutput[] Connections => _connections.ToArray();

            public FilterBitmapInput([NotNull] IPipelineNode step)
            {
                Node = step;
                Name = "Image";
            }
            
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
            /// filtered by types <see cref="Bitmap"/>.
            /// </summary>
            public IObservable<Bitmap> AnyConnectionBitmap()
            {
                return
                    ConnectionsObservable
                        .SelectMany(o => o.GetObservable())
                        .Where(input => input is BundleSheetExport || input is Bitmap)
                        .Select(input =>
                        {
                            if (input is BundleSheetExport sheet)
                            {
                                return (Bitmap)sheet.Sheet;
                            }
                            return (Bitmap) input;
                        });
            }

            public IPipelineLinkConnection Connect(IPipelineOutput output)
            {
                if (_connections.Contains(output))
                    return null;

                _connections.Add(output);
                return new PipelineLinkConnection(this, output, _ => { Disconnect(output); });
            }

            public void Disconnect(IPipelineOutput output)
            {
                _connections.Remove(output);
            }

            public IPipelineMetadata GetMetadata()
            {
                return PipelineMetadata.Empty;
            }
        }
    }
}
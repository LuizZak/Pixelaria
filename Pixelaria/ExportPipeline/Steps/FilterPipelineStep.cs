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
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Pixelaria.Data.Exports;
using Pixelaria.ExportPipeline.Inputs;
using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.Filters;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// Pipeline step that applies a filter onto a Bitmap and passes a copy of it
    /// forward.
    /// </summary>
    public sealed class FilterPipelineStep : AbstractPipelineStep
    {
        [NotNull]
        public IFilter Filter;

        public override string Name => Filter.Name;
        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public FilterPipelineStep([NotNull] IFilter filter)
        {
            Filter = filter;
            Input = new IPipelineInput[] { new PipelineBitmapInput(this) };
            
            var connections =
                Input[0].Connections
                    .Select(o => o.GetObservable()).ToObservable()
                    .SelectMany(o => o)
                    .Repeat();

            var source = 
                connections
                    .OfType<Bitmap>()
                    .Select(bitmap =>
                    {
                        // Clone before applying filter
                        var bit = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

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
            var alphaInput = new GenericPipelineInput<float>(this, "Transparency factor");
            var bitmapInput = new FilterBitmapInput(this);

            Input = new IPipelineInput[]
            {
                alphaInput,
                bitmapInput
            };

            var bitmapConnections = bitmapInput.AnyConnectionBitmap();
            var transp = alphaInput.AnyConnection();

            var source =
                bitmapConnections
                    .WithLatestFrom(transp, (bitmap, alpha) => (bitmap, alpha))
                    .Select(tup =>
                    {
                        var (bitmap, alpha) = tup;

                        // Clone before applying filter
                        var bit = new Bitmap(bitmap.Width, bitmap.Height, bitmap.PixelFormat);

                        var filter = new TransparencyFilter {Transparency = alpha};
                        filter.ApplyToBitmap(bit);

                        return bit;
                    });

            Output = new IPipelineOutput[] { new PipelineBitmapOutput(this, source, "Filtered Bitmap") };
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
                return new PipelineLinkConnection(this, output);
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
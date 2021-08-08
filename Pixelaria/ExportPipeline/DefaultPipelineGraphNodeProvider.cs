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
using Pixelaria.Filters;
using Pixelaria.Properties;
using Pixelaria.Utils;
using Pixelaria.Views.ExportPipeline;
using PixLib.Filters;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline
{
    internal class DefaultPipelineGraphNodeProvider : IPipelineGraphNodeProvider
    {
        private readonly List<PipelineNodeDescriptor> _nodeDescriptors = new List<PipelineNodeDescriptor>();

        public IReadOnlyList<PipelineNodeDescriptor> NodeDescriptors => _nodeDescriptors;

        public DefaultPipelineGraphNodeProvider()
        {
            CreateDefaultNodes();
        }

        private void CreateDefaultNodes()
        {
            RegisterNode(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.AnimationJoiner,
                Title = "Animation Joiner",
                Icon = Resources.anim_icon,
                Inputs = {new PipelineInputDescriptor("animations", typeof(object)) },
                Outputs = {new PipelineOutputDescriptor("animations", typeof(object)) }
            });
            RegisterNode(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.BitmapImport,
                Title = "Bitmap Import",
                Outputs = {new PipelineOutputDescriptor("bitmap", typeof(Bitmap))}
            });
            RegisterNode(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.BitmapPreview,
                Title = "Bitmap Preview",
                Inputs = { new PipelineInputDescriptor("bitmap", typeof(Bitmap)) },
                Outputs = { new PipelineOutputDescriptor("bitmap", typeof(Bitmap)) },
                Body = new PipelineBody(new PipelineBodyId("bitmapPreview"), new[] { typeof(Bitmap) }, new[] { typeof(Bitmap) },
                    context => AnyObservable.FromObservable(context.GetIndexedInput<Bitmap>(0)))
            });
            RegisterNode(new PipelineNodeDescriptor
            {
                NodeKind = PipelineNodeKinds.FileExport,
                Title = "File Export",
                Inputs = { new PipelineInputDescriptor("bitmap", typeof(Bitmap)) }
            });

            // Filters
            RegisterFilterNode<TransparencyFilter>(PipelineNodeKinds.TransparencyFilter, Resources.filter_transparency_icon);
            RegisterFilterNode<OffsetFilter>(PipelineNodeKinds.OffsetFilter, Resources.filter_offset_icon);
            RegisterFilterNode<HueFilter>(PipelineNodeKinds.HueFilter, Resources.filter_hue);
            RegisterFilterNode<SaturationFilter>(PipelineNodeKinds.SaturationFilter, Resources.filter_saturation);
            RegisterFilterNode<LightnessFilter>(PipelineNodeKinds.LightnessFilter, Resources.filter_lightness);
            RegisterFilterNode<StrokeFilter>(PipelineNodeKinds.StrokeFilter, Resources.filter_stroke);
            RegisterFilterNode<ScaleFilter>(PipelineNodeKinds.ScaleFilter, Resources.filter_scale_icon);
            RegisterFilterNode<RotationFilter>(PipelineNodeKinds.RotationFilter, Resources.filter_rotation_icon);
        }

        private void RegisterNode([NotNull] PipelineNodeDescriptor descriptor)
        {
            Debug.Assert(_nodeDescriptors.All(node => node.NodeKind != descriptor.NodeKind), $"Registering duplicated node kind ${descriptor.NodeKind}");

            _nodeDescriptors.Add(descriptor);
        }

        private void RegisterFilterNode<T>(PipelineNodeKind nodeKind, Bitmap icon) where T: IFilter, new()
        {
            var tempInstance = new T();
            var nodeDesc = new PipelineNodeDescriptor
            {
                NodeKind = nodeKind,
                Title = $"{tempInstance.Name} Filter",
                Icon = icon
            };

            nodeDesc.Inputs.Add(new PipelineInputDescriptor("Bitmap", typeof(Bitmap)));
           
            var props = tempInstance.InspectableProperties();

            foreach (var property in props)
            {
                var input = new PipelineInputDescriptor(Utilities.DePascalCase(property.Name), property.PropertyType);
                nodeDesc.Inputs.Add(input);
            }

            nodeDesc.Outputs.Add(new PipelineOutputDescriptor("Bitmap", typeof(Bitmap)));

            nodeDesc.Body 
                = new PipelineBody(
                    new PipelineBodyId(nodeKind.Id), 
                    nodeDesc.Inputs.Select(i => i.InputType).ToArray(), 
                    new [] {typeof(Bitmap)}, context =>
                    {
                        var bitmapInput = context.GetIndexedInput<Bitmap>(0);
                        if (bitmapInput == null)
                            return AnyObservable.Empty;

                        var inputs = new IObservable<object>[props.Length];

                        for (int i = 0; i < props.Length; i++)
                        {
                            var o = context.GetAnyIndexedInput(i + 1);

                            if(o == null)
                                return AnyObservable.Empty;

                            inputs[i] = o;
                        }

                        return AnyObservable.FromObservable(
                            bitmapInput.PxlWithLatestFrom(inputs.Zip(), (b, i) => (b, i))
                                .Select(res =>
                                {
                                    var filter = new T();
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
                                })
                            );
                    });

            RegisterNode(nodeDesc);
        }

        public PipelineBody GetBody(PipelineBodyId id)
        {
            return _nodeDescriptors.FirstOrDefault(entry => entry.Body?.Id == id)?.Body;
        }

        public bool CanCreateNode(PipelineNodeKind kind)
        {
            return _nodeDescriptors.Any(node => node.NodeKind == kind);
        }

        public bool CreateNode(PipelineNodeKind nodeKind, PipelineNodeBuilder builder)
        {
            var nodeDescriptor = _nodeDescriptors.FirstOrDefault(descriptor => descriptor.NodeKind == nodeKind);
            if (nodeDescriptor == null)
                return false;

            nodeDescriptor.CreateIn(builder);

            return true;
        }
    }

    public static class PipelineNodeKinds
    {
        public static PipelineNodeKind AnimationJoiner { get; } = new PipelineNodeKind("animationJoiner");
        public static PipelineNodeKind BitmapImport { get; } = new PipelineNodeKind("bitmapImport");
        public static PipelineNodeKind BitmapPreview { get; } = new PipelineNodeKind("bitmapPreview");
        public static PipelineNodeKind FileExport { get; } = new PipelineNodeKind("fileExport");
        public static PipelineNodeKind TransparencyFilter { get; } = new PipelineNodeKind("transparencyFilter");
        public static PipelineNodeKind OffsetFilter { get; } = new PipelineNodeKind("offsetFilter");
        public static PipelineNodeKind HueFilter { get; } = new PipelineNodeKind("hueFilter");
        public static PipelineNodeKind SaturationFilter { get; } = new PipelineNodeKind("saturationFilter");
        public static PipelineNodeKind StrokeFilter { get; } = new PipelineNodeKind("strokeFilter");
        public static PipelineNodeKind LightnessFilter { get; } = new PipelineNodeKind("lightnessFilter");
        public static PipelineNodeKind ScaleFilter { get; } = new PipelineNodeKind("scaleFilter");
        public static PipelineNodeKind RotationFilter { get; } = new PipelineNodeKind("rotationFilter");
    }
}

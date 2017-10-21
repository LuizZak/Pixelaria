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
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

using JetBrains.Annotations;

using Pixelaria.Controllers.Exporters;
using Pixelaria.Data;
using Pixelaria.Data.Exports;
using Pixelaria.Filters;
using Pixelaria.Utils;

namespace Pixelaria.ExportPipeline
{
    /// <summary>
    /// Base interface for IPipelineEnd and IPipelineStep nodes
    /// </summary>
    public interface IPipelineNode
    {
        /// <summary>
        /// The display name of this pipeline node
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets specific metadata for this pipeline node
        /// </summary>
        [CanBeNull]
        object[] GetMetadata();
    }

    /// <summary>
    /// Represents a pipeline step that is the final output of the sequence of pipeline steps
    /// </summary>
    public interface IPipelineEnd : IPipelineNode, IDisposable
    {
        /// <summary>
        /// Accepted inputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineInput> Input { get; }

        /// <summary>
        /// Starts the chain of consumption of pipeline steps linked to this pipeline end.
        /// </summary>
        void Begin();
    }

    /// <summary>
    /// Interface for a pipeline step
    /// </summary>
    public interface IPipelineStep : IPipelineNode
    {
        /// <summary>
        /// Accepted inputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineInput> Input { get; }

        /// <summary>
        /// Accepted outputs for this pipeline step
        /// </summary>
        [NotNull]
        IReadOnlyList<IPipelineOutput> Output { get; }
    }

    /// <summary>
    /// Base interface for pipeline step input/outputs
    /// </summary>
    public interface IPipelineNodeLink
    {
        /// <summary>
        /// The step of this link
        /// </summary>
        [CanBeNull]
        IPipelineNode Node { get; }

        /// <summary>
        /// An identifying name for this link on its parent pipeline step
        /// </summary>
        [NotNull]
        string Name { get; }

        /// <summary>
        /// Gets specific metadata for this pipeline connection
        /// </summary>
        [CanBeNull]
        object[] GetMetadata();
    }
    
    /// <summary>
    /// An input for a pipeline step
    /// </summary>
    public interface IPipelineInput: IPipelineNodeLink
    {
        /// <summary>
        /// The types of data that can be consumed by this input
        /// </summary>
        [NotNull]
        Type[] DataTypes { get; }

        /// <summary>
        /// Gets an array of all connections this pipeline input has
        /// </summary>
        [NotNull]
        IPipelineOutput[] Connections { get; }

        /// <summary>
        /// Called to include a pipeline output on this pipeline input
        /// </summary>
        void Connect(IPipelineOutput output);

        /// <summary>
        /// Removes a given output from this input
        /// </summary>
        void Disconnect(IPipelineOutput output);
    }

    /// <summary>
    /// An output for a pipeline step
    /// </summary>
    public interface IPipelineOutput : IPipelineNodeLink
    {
        /// <summary>
        /// The type of data that is outputted by this pipeline connection
        /// </summary>
        [NotNull]
        Type DataType { get; }

        /// <summary>
        /// Gets an observable connection that spits item from this pipeline output.
        /// 
        /// Subscribing for this output awaits until one item is produced, which is then
        /// forwarded replicatively to all consumers.
        /// </summary>
        IObservable<object> GetConnection();
    }

    /// <summary>
    /// Helper static extensions for IPipeline* implementers
    /// </summary>
    public static class PipelineHelpers
    {
        /// <summary>
        /// Connects the first Output from <see cref="step"/> that matches the first Input from
        /// <see cref="other"/>.
        /// </summary>
        /// <returns>true if a connection was made; false otherwise.</returns>
        public static bool ConnectTo([NotNull] this IPipelineStep step, IPipelineStep other)
        {
            // Find first matching output from this that matches an input from other
            foreach (var output in step.Output)
            {
                foreach (var input in other.Input)
                {
                    if (input.Connections.Contains(output))
                        continue;

                    if (!input.CanConnect(output)) continue;

                    input.Connect(output);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Connects the first Output from <see cref="step"/> that matches the first Input from
        /// <see cref="other"/>.
        /// </summary>
        /// <returns>true if a connection was made; false otherwise.</returns>
        public static bool ConnectTo([NotNull] this IPipelineStep step, IPipelineEnd other)
        {
            // Find first matching output from this that matches an input from other
            foreach (var output in step.Output)
            {
                foreach (var input in other.Input)
                {
                    if (input.Connections.Contains(output))
                        continue;

                    if (!input.CanConnect(output)) continue;

                    input.Connect(output);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns whether a pipeline input can be connected to an output.
        /// 
        /// The method looks through the proper data types accepted by the input and the data type
        /// of the output to make the decision.
        /// </summary>
        public static bool CanConnect([NotNull] this IPipelineInput input, IPipelineOutput output)
        {
            return input.DataTypes.Any(type => type.IsAssignableFrom(output.DataType));
        }
    }

    /// <summary>
    /// Base abstract pipeline step to start subclassing and specializing pipeline steps
    /// </summary>
    public abstract class AbstractPipelineStep : IPipelineStep
    {
        public abstract string Name { get; }

        public abstract IReadOnlyList<IPipelineInput> Input { get; }
        public abstract IReadOnlyList<IPipelineOutput> Output { get; }
        
        public abstract object[] GetMetadata();
    }

    /// <summary>
    /// A pipeline step that feeds a single animation down to consumers
    /// </summary>
    public class AnimationPipelineStep : AbstractPipelineStep
    {
        public Animation Animation { get; }

        public override string Name => Animation.Name;
        public override IReadOnlyList<IPipelineInput> Input { get; } = new IPipelineInput[0];
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public AnimationPipelineStep(Animation animation)
        {
            var output = new BehaviorSubject<Animation>(animation);

            Animation = animation;
            
            Output = new[] { new PipelineOutput(this, output) };
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }

        public class PipelineOutput : IPipelineOutput
        {
            private readonly BehaviorSubject<Animation> _output;

            public string Name { get; } = "Animation";
            public IPipelineNode Node { get; }
            public Type DataType => typeof(Animation);

            public PipelineOutput([NotNull] IPipelineNode step, BehaviorSubject<Animation> output)
            {
                Node = step;
                _output = output;
            }

            public object[] GetMetadata()
            {
                return new object[0];
            }

            public IObservable<object> GetConnection()
            {
                return _output.Take(1);
            }
        }
    }

    /// <summary>
    /// Joins all connected animation sources into one Animation array
    /// </summary>
    public sealed class AnimationJoinerStep : AbstractPipelineStep
    {
        public override string Name { get; } = "Animation Joiner";

        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public AnimationJoinerStep()
        {
            var animationInput = new AnimationInput(this);
            Input = new IPipelineInput[] { animationInput };

            var connections =
                animationInput
                    .ConnectionsObservable
                    .SelectMany(o => o.GetConnection());

            var source =
                connections
                    .ObserveOn(NewThreadScheduler.Default)
                    .OfType<Animation>()
                    .ToArray();

            Output = new IPipelineOutput[] { new AnimationsOutput(this, source) };
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// A sprite sheet generation pipeline step.
    /// 
    /// From a list of animations and an accompanying sheet export settings struct,
    /// generates and passes forward one animation sheet.
    /// </summary>
    public sealed class SpriteSheetGenerationPipelineStep : AbstractPipelineStep
    {
        public override string Name => "Sprite Sheet Generation";
        public override IReadOnlyList<IPipelineInput> Input { get; }
        public override IReadOnlyList<IPipelineOutput> Output { get; }

        public AnimationsInput AnimationsInput { get; }
        public SheetSettingsInput SheetSettingsInput { get; }

        public SpriteSheetGenerationPipelineStep()
        {
            // Create a stream that takes an animation and outputs a sprite sheet
            var exporter = new DefaultPngExporter();

            AnimationsInput = new AnimationsInput(this);
            SheetSettingsInput = new SheetSettingsInput(this);
            Input = new IPipelineInput[]
            {
                AnimationsInput,
                SheetSettingsInput
            };
            
            var animConnections =
                AnimationsInput
                    .ConnectionsObservable
                    .SelectMany(o => o.GetConnection())
                    .OfType<Animation[]>();

            var settingsConnections =
                SheetSettingsInput
                    .ConnectionsObservable
                    .Select(o => o.GetConnection())
                    .SelectMany(o => o)
                    .OfType<AnimationSheetExportSettings>();

            var source =
                animConnections
                    .WithLatestFrom(settingsConnections.Take(1), (animations, settings) => (animations, settings))
                    .ObserveOn(NewThreadScheduler.Default)
                    .SelectMany((tuple, i, cancellation) =>
                    {
                        var provider = new BasicAnimationProvider(tuple.Item1.Cast<IAnimation>().ToArray(), tuple.Item2, "Sheet");

                        return exporter.ExportBundleSheet(provider, cancellation);
                    });

            Output = new[] {new BundleSheetExportOutput(this, source)};
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

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
                    .Select(o => o.GetConnection()).ToObservable()
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

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }
    
    /// <summary>
    /// A pipeline step that exports any and all resulting BundleSheetExports
    /// that come in.
    /// </summary>
    public sealed class FileExportPipelineStep : IPipelineEnd
    {
        private readonly CompositeDisposable _disposeBag = new CompositeDisposable();
        public string Name { get; } = "Export to File";

        public IReadOnlyList<IPipelineInput> Input { get; }

        public FileExportPipelineStep()
        {
            Input = new[]
            {
                new FileExportPipelineInput(this)
            };
        }

        public void Begin()
        {
            Input[0]
                .Connections
                .ToObservable()
                .SelectMany(o => o.GetConnection())
                .OfType<BundleSheetExport>()
                .Subscribe(sheet =>
                {
                    System.Diagnostics.Debug.WriteLine(sheet.Atlas.Name);
                }, error =>
                {
                    System.Diagnostics.Debug.WriteLine(error);
                }, () =>
                {
                    System.Diagnostics.Debug.WriteLine("Completed.");
                }).AddToDisposable(_disposeBag);
        }

        public void Dispose()
        {
            _disposeBag.Dispose();
        }

        public object[] GetMetadata()
        {
            return new object[0];
        }

        public sealed class FileExportPipelineInput : IPipelineInput
        {
            private readonly List<IPipelineOutput> _connections = new List<IPipelineOutput>();

            public string Name { get; } = "Generated Sprite Sheet";
            public IPipelineNode Node { get; }

            public Type[] DataTypes { get; } = {typeof(BundleSheetExport)};

            public IPipelineOutput[] Connections => _connections.ToArray();

            public FileExportPipelineInput([NotNull] IPipelineNode step)
            {
                Node = step;
            }

            public void Connect(IPipelineOutput output)
            {
                if (!_connections.Contains(output))
                    _connections.Add(output);
            }

            public void Disconnect(IPipelineOutput output)
            {
                _connections.Remove(output);
            }

            public object[] GetMetadata()
            {
                return new object[0];
            }
        }
    }

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

        protected AbstractSinglePipelineInput([NotNull] IPipelineNode step)
        {
            Node = step;
        }
        
        public void Connect(IPipelineOutput output)
        {
            if (!_connections.Contains(output))
                _connections.Add(output);
        }

        public void Disconnect(IPipelineOutput output)
        {
            _connections.Remove(output);
        }

        public abstract object[] GetMetadata();
    }

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

        public abstract object[] GetMetadata();
    }

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

        public StaticPipelineOutput(T value, string name)
        {
            _value = value;
            Name = name;
        }

        public IObservable<object> GetConnection()
        {
            return Observable.Create<T>(obs =>
            {
                obs.OnNext(_value);
                obs.OnCompleted();

                return Disposable.Empty;
            }).Select(o => (object)o);
        }

        public object[] GetMetadata()
        {
            return new object[] { "static", typeof(StaticPipelineOutput<T>) };
        }
    }

    /// <summary>
    /// Accepts AnimationSheetExportSettings instances
    /// </summary>
    public class SheetSettingsInput : AbstractSinglePipelineInput<AnimationSheetExportSettings>
    {
        public SheetSettingsInput([NotNull] IPipelineNode step) : base(step)
        {
            Name = "Sprite Sheet Settings";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// Outputs BundleSheetExport objects
    /// </summary>
    public class BundleSheetExportOutput : AbstractPipelineOutput<BundleSheetExport>
    {
        public BundleSheetExportOutput([NotNull] IPipelineNode step, [NotNull] IObservable<BundleSheetExport> source)
            : base(step, source)
        {
            Name = "Generated Sprite Sheet";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// Accepts Animation object
    /// </summary>
    public class AnimationInput : AbstractSinglePipelineInput<Animation>
    {
        public AnimationInput([NotNull] IPipelineNode step) : base(step)
        {
            Name = "Animation";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// Accepts Animation[] arrays
    /// </summary>
    public class AnimationsInput : AbstractSinglePipelineInput<Animation[]>
    {
        public AnimationsInput([NotNull] IPipelineNode step) : base(step)
        {
            Name = "Animations";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// Outputs Animation[] arrays
    /// </summary>
    public class AnimationsOutput : AbstractPipelineOutput<Animation[]>
    {
        public AnimationsOutput([NotNull] IPipelineNode step, [NotNull] IObservable<Animation[]> source) : base(step, source)
        {
            Name = "Animations";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// Input that accepts bitmaps
    /// </summary>
    public class PipelineBitmapInput : AbstractSinglePipelineInput<Bitmap>
    {
        public PipelineBitmapInput([NotNull] IPipelineNode step) : base(step)
        {
            Name = "Bitmap";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    /// <summary>
    /// Output that outputs bitmaps
    /// </summary>
    public class PipelineBitmapOutput : AbstractPipelineOutput<Bitmap>
    {
        public PipelineBitmapOutput([NotNull] IPipelineNode step, [NotNull] IObservable<Bitmap> source) : base(step, source)
        {
            Name = "Bitmap";
        }

        public override object[] GetMetadata()
        {
            return new object[0];
        }
    }

    // TODO: Test stuff - remove me later
    public static class ObsExt
    {
        public static IObservable<T> Debug<T>(this IObservable<T> obs)
        {
            return obs.Do(next =>
            {
                System.Diagnostics.Debug.WriteLine($"OnNext: {next}");
            }, error =>
            {
                System.Diagnostics.Debug.WriteLine($"OnError: {error}");
            }, () =>
            {
                System.Diagnostics.Debug.WriteLine("OnCompleted");
            });
        }
    }
}

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

using System.Collections.Generic;
using System.Drawing;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Pixelaria.ExportPipeline.Outputs;
using Pixelaria.ExportPipeline.Steps.Abstract;
using PixPipelineGraph;

namespace Pixelaria.ExportPipeline.Steps
{
    /// <summary>
    /// A pipeline step that imports a bitmap file from a path or stream and provides it
    /// through its only output.
    /// </summary>
    internal class BitmapImportPipelineStep: AbstractPipelineStart
    {
        public override string Name => "Bitmap Import";

        public override IReadOnlyList<IPipelineOutput> Output { get; }

        /// <summary>
        /// Gets the source to import the bitmap from
        /// </summary>
        public IBitmapImportSource ImportSource { get; }

        public BitmapImportPipelineStep(IBitmapImportSource importSource)
        {
            ImportSource = importSource;

            var source =
                Observable.Create<Bitmap>(obs =>
                {
                    var bitmap = ImportSource.LoadBitmap();

                    obs.OnNext(bitmap);
                    obs.OnCompleted();

                    return Disposable.Empty;
                }).Replay(1).RefCount();
            
            Output = new[] {new PipelineBitmapOutput(this, source, new PipelineOutput())};
        }
    }

    /// <summary>
    /// An abstract interface for fetching bitmaps from
    /// </summary>
    internal interface IBitmapImportSource
    {
        /// <summary>
        /// Loads the bitmap image to use
        /// </summary>
        Bitmap LoadBitmap();
    }

    /// <summary>
    /// Loads a bitmap from a file path
    /// </summary>
    internal class BitmapFileImportSource : IBitmapImportSource
    {
        /// <summary>
        /// Gets the path to load the bitmap from
        /// </summary>
        public string FilePath { get; }

        public BitmapFileImportSource(string filePath)
        {
            FilePath = filePath;
        }

        public Bitmap LoadBitmap()
        {
            var image = Image.FromFile(FilePath);
            
            return new Bitmap(image);
        }
    }
}

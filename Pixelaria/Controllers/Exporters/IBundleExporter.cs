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

using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Data.Exports;

namespace Pixelaria.Controllers.Exporters
{
    /// <summary>
    /// Defines the behavior that must be implemented by exporters in the program
    /// </summary>
    public interface IBundleExporter
    {
        /// <summary>
        /// Exports a given bundle in concurrent fashion, performing multiple bundle sheet encodings at once
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="cancellationToken">A cancelation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        Task ExportBundleConcurrent(Bundle bundle, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null);
        
        /// <summary>
        /// Exports the given animation sheet into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animation sheet passed ready to be saved to disk</returns>
        Task<BundleSheetExport> ExportBundleSheet(AnimationSheet sheet, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Exports the given animations into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="provider">The provider for animations and their respective export settings</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animations passed ready to be saved to disk</returns>
        [ItemCanBeNull]
        Task<BundleSheetExport> ExportBundleSheet(IAnimationProvider provider, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Generates a TextureAtlas from the given AnimationSheet object
        /// </summary>
        /// <param name="sheet">The AnimationSheet to generate the TextureAtlas of</param>
        /// <param name="cancellationToken">A cancelation token that can be used to cancel the process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A TextureAtlas generated from the given AnimationSheet</returns>
        Task<TextureAtlas> GenerateAtlasFromAnimationSheet(AnimationSheet sheet, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Generates an image that represents the sequential sprite strip from the specified animation.
        /// If the animation contains no frames, an empty 1x1 image is returned
        /// </summary>
        /// <param name="animation">The animation to generate the sprite strip image from</param>
        /// <returns>An image that represents the sequential sprite strip from the specified animation</returns>
        Image GenerateSpriteStrip(AnimationController animation);

        /// <summary>
        /// Returns a number from 0-1 describing the export progress for a given animation sheet.
        /// 0 means unstarted, 1 means the animation sheet was generated.
        /// </summary>
        /// <param name="sheet">The animation sheet to get the progress of</param>
        float ProgressForAnimationSheet(AnimationSheet sheet);
    }
}
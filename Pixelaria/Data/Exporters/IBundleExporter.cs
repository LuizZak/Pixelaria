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
using Pixelaria.Data.Exports;

namespace Pixelaria.Data.Exporters
{
    /// <summary>
    /// Defines the behavior that must be implemented by exporters in the program
    /// </summary>
    public interface IBundleExporter
    {
        /// <summary>
        /// Exports the given Bundle
        /// </summary>
        /// <param name="bundle">The bundle to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        void ExportBundle(Bundle bundle, BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Exports the given animations into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="exportSettings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>An image sheet representing the animations passed</returns>
        Image ExportAnimationSheet(AnimationExportSettings exportSettings, Animation[] anims, BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Exports the given animation sheet into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>An image sheet representing the animation sheet passed</returns>
        Image ExportAnimationSheet(AnimationSheet sheet, BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Exports the given animation sheet into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animation sheet passed ready to be saved to disk</returns>
        BundleSheetExport ExportBundleSheet(AnimationSheet sheet, BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Exports the given animations into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="settings">The export settings for the sheet</param>
        /// <param name="anims">The list of animations to export</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animations passed ready to be saved to disk</returns>
        BundleSheetExport ExportBundleSheet(AnimationExportSettings settings, Animation[] anims, BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Generates a TextureAtlas from the given AnimationSheet object
        /// </summary>
        /// <param name="sheet">The AnimationSheet to generate the TextureAtlas of</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A TextureAtlas generated from the given AnimationSheet</returns>
        TextureAtlas GenerateAtlasFromAnimationSheet(AnimationSheet sheet, BundleExportProgressEventHandler progressHandler = null);

        /// <summary>
        /// Generates an image that represents the sequential sprite strip from the specified animation.
        /// If the animation contains no frames, an empty 1x1 image is returned
        /// </summary>
        /// <param name="animation">The animation to generate the sprite strip image from</param>
        /// <returns>An image that represents the sequential sprite strip from the specified animation</returns>
        Image GenerateSpriteStrip(Animation animation);
    }
}
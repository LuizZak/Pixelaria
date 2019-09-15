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
using System.Drawing.Imaging;
using System.Threading;
using System.Threading.Tasks;
using FastBitmapLib;
using JetBrains.Annotations;
using Pixelaria.Algorithms;
using Pixelaria.Algorithms.Packers;
using Pixelaria.Controllers.DataControllers;
using Pixelaria.Data;
using Pixelaria.Data.Exports;

namespace Pixelaria.Controllers.Exporters
{
    public class DefaultSheetExporter : ISheetExporter
    {
        /// <summary>
        /// Exports the given animations into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="sheet">The sheet to export</param>
        /// <param name="cancellationToken">A cancellation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animation sheet passed ready to be saved to disk</returns>
        public async Task<BundleSheetExport> ExportBundleSheet([NotNull] AnimationSheet sheet, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null)
        {
            //
            // 1. Generate texture atlas
            //
            using (var atlas = await GenerateAtlasFromAnimationSheet(sheet, cancellationToken, progressHandler))
            {
                //
                // 2. Generate an export sheet from the texture atlas
                //
                return BundleSheetExport.FromAtlas(atlas);
            }
        }

        /// <summary>
        /// Exports the given animations into a BundleSheetExport and returns the created sheet
        /// </summary>
        /// <param name="provider">The provider for animations and their respective export settings</param>
        /// <param name="cancellationToken">A cancellation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A BundleSheetExport representing the animations passed ready to be saved to disk</returns>
        public async Task<BundleSheetExport> ExportBundleSheet([NotNull] IAnimationProvider provider, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null)
        {
            var atlas = await GenerateAtlasFromAnimations(provider, "", cancellationToken, progressHandler);
            if (cancellationToken.IsCancellationRequested)
                return null;

            return BundleSheetExport.FromAtlas(atlas);
        }

        /// <summary>
        /// Generates a TextureAtlas from the given AnimationSheet object
        /// </summary>
        /// <param name="sheet">The AnimationSheet to generate the TextureAtlas of</param>
        /// <param name="cancellationToken">A cancellation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>A TextureAtlas generated from the given AnimationSheet</returns>
        public async Task<TextureAtlas> GenerateAtlasFromAnimationSheet([NotNull] AnimationSheet sheet, CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null)
        {
            return await GenerateAtlasFromAnimations(sheet, sheet.Name, cancellationToken, args =>
            {
                progressHandler?.Invoke(args);
            });
        }

        /// <summary>
        /// Exports the given animations into an image sheet and returns the created sheet
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="name">The name for the generated texture atlas. Used for progress reports</param>
        /// <param name="cancellationToken">A cancellation token that is passed to the exporters and can be used to cancel the export process mid-way</param>
        /// <param name="progressHandler">Optional event handler for reporting the export progress</param>
        /// <returns>An image sheet representing the animations passed</returns>
        public async Task<TextureAtlas> GenerateAtlasFromAnimations([NotNull] IAnimationProvider provider, string name = "", CancellationToken cancellationToken = new CancellationToken(), BundleExportProgressEventHandler progressHandler = null)
        {
            var atlas = new TextureAtlas(provider.ExportSettings, name);

            //
            // 1. Add the frames to the texture atlas
            //
            foreach (var anim in provider.GetAnimations())
            {
                for (int i = 0; i < anim.FrameCount; i++)
                {
                    atlas.InsertFrame(anim.GetFrameAtIndex(i));
                }
            }

            //
            // 2. Pack the frames into the atlas
            //
            ITexturePacker packer = new DefaultTexturePacker();
            await packer.Pack(atlas, cancellationToken, progressHandler);

            return atlas;
        }

        /// <summary>
        /// Generates an image that represents the sequential sprite strip from the specified animation.
        /// If the animation contains no frames, an empty 1x1 image is returned
        /// </summary>
        /// <param name="animation">The animation to generate the sprite strip image from</param>
        /// <returns>An image that represents the sequential sprite strip from the specified animation</returns>
        public Image GenerateSpriteStrip([NotNull] AnimationController animation)
        {
            // If the frame count is 0, return an empty 1x1 image
            if (animation.FrameCount == 0)
            {
                return new Bitmap(1, 1, PixelFormat.Format32bppArgb);
            }

            // Create the image
            var stripBitmap = new Bitmap(animation.Width * animation.FrameCount, animation.Height);

            // Copy the frames into the strip bitmap now
            //foreach (var frame in animation.Frames)
            for (int i = 0; i < animation.FrameCount; i++)
            {
                var frame = animation.GetFrameController(animation.GetFrameAtIndex(i));

                using (var composed = frame.GetComposedBitmap())
                {
                    FastBitmap.CopyRegion(composed, stripBitmap, new Rectangle(Point.Empty, frame.Size), new Rectangle(new Point(frame.Index * frame.Width, 0), frame.Size));
                }
            }

            return stripBitmap;
        }
    }
}
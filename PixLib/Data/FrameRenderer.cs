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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using JetBrains.Annotations;
using PixCore.Imaging;
using PixLib.Controllers.DataControllers;

namespace PixLib.Data
{
    /// <summary>
    /// Provides the capability of rendering (or composing) a Frame into a flat final image
    /// </summary>
    public class FrameRenderer
    {
        /// <summary>
        /// Composes the specified frame into a flat bitmap
        /// </summary>
        /// <param name="frame">The frame to compose</param>
        /// <param name="statuses">The layer status information to use when composing the frame</param>
        /// <param name="ignoreStatusTransparency">Whether to ignore the Transparency of a layer when composing</param>
        /// <returns>A new Bitmap object that represents the composed frame</returns>
        public static Bitmap ComposeFrame([NotNull] FrameController frame, [NotNull] IEnumerable<ILayerStatus> statuses, bool ignoreStatusTransparency = false)
        {
            var bitmap = new Bitmap(frame.Width, frame.Height, PixelFormat.Format32bppArgb);
            using var statusEnumerator = statuses.GetEnumerator();

            for (int i = 0; i < frame.LayerCount; i++)
            {
                var status = statusEnumerator.Current;
                statusEnumerator.MoveNext();

                Debug.Assert(status != null, nameof(status) + " != null");

                if (!status.Visible || (!ignoreStatusTransparency && !(status.Transparency > 0)))
                    continue;

                var layerBitmap = frame.GetLayerAt(i).LayerBitmap;

                if (ignoreStatusTransparency || status.Transparency >= 1)
                {
                    ImageUtilities.FlattenBitmaps(bitmap, layerBitmap, false);
                }
                else
                {
                    using var g = Graphics.FromImage(bitmap);

                    var cm = new ColorMatrix
                    {
                        Matrix33 = status.Transparency
                    };

                    var attributes = new ImageAttributes();
                    attributes.SetColorMatrix(cm, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

                    g.DrawImage(layerBitmap, new Rectangle(Point.Empty, layerBitmap.Size), 0, 0, layerBitmap.Width, layerBitmap.Height, GraphicsUnit.Pixel, attributes);

                    g.Flush();
                }
            }

            return bitmap;
        }
    }

    /// <summary>
    /// Interface for objects that provide layer visibility information for a <see cref="FrameRenderer"/>
    /// </summary>
    public interface ILayerStatus
    {
         bool Visible { get; }
         float Transparency { get; }
    }
}
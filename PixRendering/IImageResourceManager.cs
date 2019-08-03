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
using JetBrains.Annotations;

namespace PixRendering
{
    /// <summary>
    /// Interface for objects capable of creating, updating, providing and destroying Direct2D bitmap resources.
    /// </summary>
    public interface IImageResourceManager : IImageResourceProvider
    {
        /// <summary>
        /// Creates a new image from a given <see cref="Bitmap"/> instance, assigns it with a given resource name, and
        /// then returns it to be used.
        /// </summary>
        ImageResource AddImageResource([NotNull] IRenderLoopState state, [NotNull] Bitmap bitmap, [NotNull] string resourceName);

        /// <summary>
        /// Creates a managed image resource for rendering in ad-hoc fashion in a compatible <see cref="IRenderer"/> interface.
        /// </summary>
        IManagedImageResource CreateManagedImageResource([NotNull] IRenderLoopState state, [NotNull] Bitmap bitmap);

        /// <summary>
        /// Updates a managed image resource.
        /// </summary>
        void UpdateManagedImageResource([NotNull] IRenderLoopState state, [NotNull] ref IManagedImageResource managedImage, [NotNull] Bitmap bitmap);

        void RemoveImageResource([NotNull] string resourceName);

        void RemoveAllImageResources();
    }

    /// <summary>
    /// Interface for objects that can provide encapsulated access to image resources.
    /// </summary>
    public interface IImageResourceProvider
    {
        /// <summary>
        /// Fetches an image resource formatted as an image resource struct.
        /// 
        /// Returns null, if no resource with the given name is found.
        /// </summary>
        ImageResource? GetImageResource([NotNull] string resourceName);
    }
}
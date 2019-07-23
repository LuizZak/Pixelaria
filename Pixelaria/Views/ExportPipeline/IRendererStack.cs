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
using System.Windows.Forms;
using JetBrains.Annotations;
using PixDirectX.Rendering;

namespace Pixelaria.Views.ExportPipeline
{
    /// <summary>
    /// Implements a full stack of a rendering API + render loop
    /// </summary>
    public interface IRendererStack: IDisposable
    {
        /// <summary>
        /// The latest valid rendering state.
        ///
        /// May be null, in case rendering state has been invalidated or the renderer stack
        /// has not been initialized.
        /// </summary>
        [CanBeNull]
        IRenderLoopState RenderingState { get; }

        /// <summary>
        /// Initializes this renderer stack using a given host control.
        ///
        /// In exchange, a render manager is returned which can be used to access rendering
        /// resources and general text layout machinery.
        /// </summary>
        IExportPipelineRenderManager Initialize(Control control);

        /// <summary>
        /// Starts a render loop, executing until the application closes.
        ///
        /// The clipping region provided should be populated with the redraw regions of the
        /// frame to be rendered.
        ///
        /// Frame rendering occurs after the closure is executed.
        /// </summary>
        void StartRenderLoop(Action<IRenderLoopState, ClippingRegion> execute);
    }
}
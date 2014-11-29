using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Pixelaria.Data.Exports;

namespace Pixelaria.Algorithms.Packers
{
    /// <summary>
    /// Interface to be implemented by texture packers
    /// </summary>
    public interface ITexturePacker
    {
        /// <summary>
        /// Packs a given atlas with a specified progress event handler
        /// </summary>
        /// <param name="atlas">The texture atlas to pack</param>
        /// <param name="handler">The event handler for the packing process</param>
        void Pack(TextureAtlas atlas, BundleExportProgressEventHandler handler);
    }
}
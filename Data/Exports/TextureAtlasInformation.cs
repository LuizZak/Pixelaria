using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelaria.Data.Exports
{
    /// <summary>
    /// Structure that lists information about a specific texture atlas
    /// </summary>
    public struct TextureAtlasInformation
    {
        /// <summary>
        /// The number of frames that had their areas reused
        /// </summary>
        public int ReusedFrameOriginsCount;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blend2DCS
{
    /// <summary>
    /// File read flags used by `BLFileSystem::readFile()`.
    /// </summary>
    public enum BLFileReadFlags : uint
    {
        /// <summary>
        /// Use memory mapping to read the content of the file.
        ///
        /// The destination buffer `BLArray<>` would be configured to use the memory
        /// mapped buffer instead of allocating its own.
        /// </summary>
        MmapEnabled = 0x00000001u,

        /// <summary>
        /// Avoid memory mapping of small files.
        ///
        /// The size of small file is determined by Blend2D, however, you should
        /// expect it to be 16kB or 64kB depending on host operating system.
        /// </summary>
        MmapAvoidSmall = 0x00000002u,

        /// <summary>
        /// Do not fallback to regular read if memory mapping fails. It's worth noting
        /// that memory mapping would fail for files stored on filesystem that is not
        /// local (like a mounted network filesystem, etc...).
        /// </summary>
        MmapNoFallback = 0x00000008u
    };
}

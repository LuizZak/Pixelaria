using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelaria.Data.Persistence.ProjectTreeBlocks
{
    /// <summary>
    /// Represents a base class for nodes in the Project Tree Persistence system
    /// </summary>
    public class NodeBlock : BaseBlock
    {


        /// <summary>Represents a trailing code (meaning, no more blocks)</summary>
        public const short BLOCKID_TRAILING   = 0x000;
        /// <summary>Represents a normal block</summary>
        public const short BLOCKID_NODE       = 0x001;
        /// <summary>Represents a child start block</summary>
        public const short BLOCKID_CHILDSTART = 0x002;
        /// <summary>Represents a child end block</summary>
        public const short BLOCKID_CHILDEND   = 0x002;
    }
}
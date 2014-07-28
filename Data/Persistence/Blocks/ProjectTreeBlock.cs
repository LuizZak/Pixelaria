using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pixelaria.Data.Persistence.Blocks
{
    /// <summary>
    /// Contains information about the project tree to be displayed on the main interface
    /// </summary>
    public class ProjectTreeBlock : Block
    {
        /// <summary>
        /// Initializes a new instance of the ProjectTreeBlock class
        /// </summary>
        public ProjectTreeBlock()
        {
            this.blockID = BLOCKID_PROJECTTREE;
        }
    }
}
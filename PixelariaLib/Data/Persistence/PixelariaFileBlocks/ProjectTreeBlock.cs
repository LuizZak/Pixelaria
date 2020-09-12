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

namespace PixelariaLib.Data.Persistence.PixelariaFileBlocks
{
    // TODO: Fill in this class once the project tree feature is done

    /// <summary>
    /// Contains information about the project tree to be displayed on the main interface
    /// </summary>
    public class ProjectTreeBlock : FileBlock
    {
        /// <summary>
        /// Gets or sets the current project tree inside this ProjectTreeBlock
        /// </summary>
        public ProjectTree Tree { get; set; }

        /// <summary>
        /// Initializes a new instance of the ProjectTreeBlock class
        /// </summary>
        public ProjectTreeBlock()
        {
            blockID = BLOCKID_PROJECTTREE;
        }

        /// <summary>
        /// Prepares the contents of this block to be saved based on the contents of the given Bundle
        /// </summary>
        /// <param name="bundle">The bundle to prepare this block from</param>
        public override void PrepareFromBundle(Bundle bundle)
        {
            base.PrepareFromBundle(bundle);

            Tree = bundle.BundleProjectTree;
        }
    }
}
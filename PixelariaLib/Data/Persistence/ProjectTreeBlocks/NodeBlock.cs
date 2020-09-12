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

namespace PixelariaLib.Data.Persistence.ProjectTreeBlocks
{
    /// <summary>
    /// Represents a base class for nodes in the Project Tree Persistence system
    /// </summary>
    public class NodeBlock : GenericFileBlock
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
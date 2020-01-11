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

using System.Runtime.InteropServices;

namespace Blend2DCS
{
    /// <summary>
    /// Holds an arbitrary 128-bit value (cookie) that can be used to match other
    /// cookies. Blend2D uses cookies in places where it allows to "lock" some
    /// state that can only be unlocked by a matching cookie. Please don't confuse
    /// cookies with a security of any kind, it's just an arbitrary data that must
    /// match to proceed with a certain operation.
    ///
    /// Cookies can be used with `BLContext.Save()` and `BLContext.Restore()`
    /// functions
    /// </summary>
    public struct BLContextCookie
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        private ulong[] Data;
    }
}

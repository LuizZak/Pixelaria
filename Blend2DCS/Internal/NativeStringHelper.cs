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

using JetBrains.Annotations;
using System;
using System.Text;

namespace Blend2DCS.Internal
{
    /// <summary>
    /// Helper to use when dealing with native C++ interop that required UTF-8 string encoding.
    /// </summary>
    public static class NativeStringHelper
    {
        /// <summary>
        /// Executes a closure while pointing to a temporarily-allocated null-terminated UTF-8 string buffer.
        ///
        /// Callers should not store the pointer beyond the lifetime of the closure's invocation.
        ///
        /// The closure <see cref="!:execute"/> receives a pointer to the start of a null-terminated UTF-8 buffer,
        /// and a size value that indicates the length of the buffer, minus the trailing null character.
        /// </summary>
        public static unsafe void WithNullTerminatedUtf8String([NotNull] string text, [InstantHandle, NotNull] Action<IntPtr, int> execute)
        {
            var bytes = Encoding.UTF8.GetBytes(text + "\0");
            fixed (byte* pointer = bytes)
            {
                execute((IntPtr) pointer, bytes.Length - 1);
            }
        }
    }
}

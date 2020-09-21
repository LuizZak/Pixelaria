﻿/*
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PixelariaLibTests.TestUtils
{
    public static class StreamTestUtils
    {
        public static void MemoryStreamMatches(this Assert assert, [NotNull] MemoryStream stream, [NotNull] IEnumerable<byte> expected)
        {
            static string StringFromStream(IEnumerable<byte> byteSequence)
            {
                return string.Join(", ", byteSequence.Select(b => $"0x{b:X2}"));
            }

            var bytes = stream.GetBuffer().Take((int) stream.Length).ToArray();
            var expectedBytes = expected.ToArray();
            
            Assert.IsTrue(bytes.SequenceEqual(expectedBytes), 
                $"Streams mismatch: expected [{StringFromStream(expectedBytes)}], but received [{StringFromStream(bytes)}]");
        }
    }
}
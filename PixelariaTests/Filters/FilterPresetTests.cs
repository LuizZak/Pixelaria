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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Filters;
using PixLib.Filters;

namespace PixelariaTests.Filters
{
    /// <summary>
    /// Tests the FilterPreset class and related components
    /// </summary>
    [TestClass]
    public class FilterPresetTests
    {
        [TestMethod]
        public void TestEquality()
        {
            var filter1 = new TransparencyFilter { Transparency = 1.0f };
            var filter2 = new TransparencyFilter { Transparency = 0.0f };

            var preset1 = new FilterPreset("Preset 1", new IFilter[] { filter1 });
            var preset2 = new FilterPreset("Preset 2", new IFilter[] { filter2 });
            var preset3 = new FilterPreset("Preset 3", new IFilter[] { filter1 });

            Assert.IsFalse(preset1.Equals(preset2));
            Assert.IsTrue(preset1.Equals(preset3));
        }

        [TestMethod]
        public void TestEqualityUnordered()
        {
            var filter1 = new TransparencyFilter { Transparency = 1.0f };
            var filter2 = new TransparencyFilter { Transparency = 0.0f };

            var preset1 = new FilterPreset("Preset 1", new IFilter[] { filter1, filter2 });
            var preset2 = new FilterPreset("Preset 2", new IFilter[] { filter2, filter1 });

            Assert.IsTrue(preset1.Equals(preset2));
        }
    }
}

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

using System.IO;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Utils;

namespace PixelariaTests.Tests.Utils
{
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void TestInitialize()
        {
            string path = Path.GetTempFileName();
            var sut = Settings.GetSettings(path);

            Assert.IsNotNull(sut);
        }
        
        [TestMethod]
        public void TestGetSettings()
        {
            string path1 = Path.GetTempFileName();
            string path2 = Path.GetTempFileName();
            var settings1 = Settings.GetSettings(path1);
            settings1.SetValue("test", "test value");

            var settings2 = Settings.GetSettings(path2);

            Assert.IsNull(settings2.GetValue("test"));
        }
    }
}

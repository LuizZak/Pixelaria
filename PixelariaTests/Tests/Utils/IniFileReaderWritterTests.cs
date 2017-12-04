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
using Pixelaria.Utils;

namespace PixelariaTests.Tests.Utils
{
    [TestClass]
    public class IniFileReaderWritterTests
    {
        [TestMethod]
        public void TestInitialize()
        {
            var sut = new IniFileReaderWritter(new TestIniFile(""));

            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void TestGetValue()
        {
            const string file = "test=test value";
            var settings = new IniFileReaderWritter(new TestIniFile(file));
            settings.LoadSettings();

            Assert.AreEqual("test value", settings.GetValue("test"));
        }

        [TestMethod]
        public void TestGetValueOnEmptySubpath()
        {
            const string file = "[]\ntest=test value";
            var settings = new IniFileReaderWritter(new TestIniFile(file));
            settings.LoadSettings();

            Assert.AreEqual("test value", settings.GetValue("test"));
        }

        [TestMethod]
        public void TestGetValueNested()
        {
            const string file = "[child 1]\ntest=test value\n[child 2]\ntest=other test value";
            var settings = new IniFileReaderWritter(new TestIniFile(file));
            settings.LoadSettings();

            Assert.AreEqual("test value", settings.GetValue("child 1\\test"));
            Assert.AreEqual("other test value", settings.GetValue("child 2\\test"));
        }

        [TestMethod]
        public void TestLoadSettingsReplacing()
        {
            const string file = "test=test value\ntest=other test value";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();
            
            Assert.AreEqual("other test value", settings.GetValue("test"));
        }

        [TestMethod]
        public void TestLoadSettingsMissingValue()
        {
            const string file = "test=";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();

            Assert.AreEqual("", settings.GetValue("test"));
        }

        [TestMethod]
        public void TestLoadSettingsMissingEquals()
        {
            const string file = "test";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();

            Assert.IsNull(settings.GetValue("test"));
        }

        [TestMethod]
        public void TestLoadSettingsMissingValueName()
        {
            const string file = "=test";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();

            Assert.IsNull(settings.GetValue("test"));
        }

        [TestMethod]
        public void TestLoadSettingsNamespaceMissingEndBracket()
        {
            const string file = "[test";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();
        }

        [TestMethod]
        public void TestLoadSettingsNamespaceMissingStartBracket()
        {
            const string file = "test]";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();
        }

        [TestMethod]
        public void TestLoadSettingsNamespaceAsValue()
        {
            const string file = "[test]=5";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();

            Assert.IsNull(settings.GetValue("[test]"));
        }

        [TestMethod]
        public void TestLoadSettingsNamespaceMissingStartBracketAsValue()
        {
            const string file = "test]=abc";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();

            Assert.IsNull(settings.GetValue("test]"));
        }

        [TestMethod]
        public void TestLoadSettingsNamespaceWithEmptyComponent()
        {
            const string file = "[test\\\\space]\ntestValue=value";
            var settings = new IniFileReaderWritter(new TestIniFile(file));

            settings.LoadSettings();

            Assert.AreEqual("value", settings.GetValue("test", "space", "testValue"));
        }

        internal class TestIniFile : IIniFileInterface
        {
            public string Contents { get; set; }

            public TestIniFile(string contents)
            {
                Contents = contents;
            }

            public TestIniFile()
            {
                Contents = "";
            }

            public void Save(string data)
            {
                Contents = data;
            }

            public string Load()
            {
                return Contents;
            }
        }
    }
}

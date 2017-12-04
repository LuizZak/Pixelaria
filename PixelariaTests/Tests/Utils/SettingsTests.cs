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

using System;
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
        public void TestSingletonSameFileAlways()
        {
            string path = Path.GetTempFileName();
            var settings1 = Settings.GetSettings(path);
            var settings2 = Settings.GetSettings(path);

            settings1.SetValue("test", "value");

            Assert.AreEqual("value", settings2.GetValue("test"));
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

        [TestMethod]
        public void TestTargetFileIsNotHeldOpenAfterInitializing()
        {
            string path = Path.GetTempFileName();

            var sut = Settings.GetSettings(path);

            Assert.IsFalse(IsHeldOpen(path));
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void TestTargetFileIsNotHeldOpenAfterLoadSettings()
        {
            string path = Path.GetTempFileName();

            var sut = Settings.GetSettings(path);
            
            Assert.IsFalse(IsHeldOpen(path));
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void TestTargetFileIsNotHeldOpenAfterSaveSettings()
        {
            string path = Path.GetTempFileName();
            var sut = Settings.GetSettings(path);

            sut.SaveSettings();

            Assert.IsFalse(IsHeldOpen(path));
            Assert.IsNotNull(sut);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingString()
        {
            var file = new TestIniFile("");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.String, "abc");

            Assert.AreEqual("test = abc", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingStringDoesNotReplaceExisting()
        {
            var file = new TestIniFile("test=value");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.String, "abc");

            Assert.AreEqual("test=value", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingDeepNodeCreation()
        {
            var file = new TestIniFile("");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test\\deep\\node", EnsureValueType.String, "abc");

            Assert.AreEqual("[test\\deep]\r\nnode = abc", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingInt()
        {
            var file = new TestIniFile("");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Int, "123");

            Assert.AreEqual("test = 123", file.Contents);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetDefaultIfMissingIntExceptionIfNotInt()
        {
            var file = new TestIniFile("");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Int, "abc");
        }

        [TestMethod]
        public void TestSetDefaultIfMissingIntDoesNotReplaceExisting()
        {
            var file = new TestIniFile("test=456");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Int, "123");

            Assert.AreEqual("test=456", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingIntReplacesIfNotInteger()
        {
            var file = new TestIniFile("test=abc");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Int, "123");

            Assert.AreEqual("test = 123", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingBoolean()
        {
            var file = new TestIniFile("");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Boolean, "true");

            Assert.AreEqual("test = true", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingBooleanDoesNotReplaceExisting()
        {
            var file = new TestIniFile("test=false");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Boolean, "true");

            Assert.AreEqual("test=false", file.Contents);
        }

        [TestMethod]
        public void TestSetDefaultIfMissingBooleanReplacesIfNotBoolean()
        {
            var file = new TestIniFile("test=123");
            var settings = new Settings(file);

            settings.SetDefaultIfMissing("test", EnsureValueType.Boolean, "true");

            Assert.AreEqual("test = true", file.Contents);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void TestSetDefaultIfMissingBooleanExceptionIfNotBoolean()
        {
            var file = new TestIniFile("");
            var settings = new Settings(file);
            
            settings.SetDefaultIfMissing("test", EnsureValueType.Boolean, "123");
        }

        private static bool IsHeldOpen(string path)
        {
            try
            {
                var file = File.OpenWrite(path);
                file.Close();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
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

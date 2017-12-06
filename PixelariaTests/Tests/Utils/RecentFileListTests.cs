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
    public class RecentFileListTests
    {
        [TestMethod]
        public void TestInitialization()
        {
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 10);

            Assert.AreEqual(10, recentFileList.FileCount);
            foreach (string file in recentFileList.FileList)
            {
                Assert.AreEqual(file, "");
            }
        }

        [TestMethod]
        public void TestSettingsInitialization()
        {
            const string expected = "[Recent Files]\r\nFile0 = \r\nFile1 = \r\nFile2 = ";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            Assert.IsNotNull(recentFileList); // Silence warning
            Assert.AreEqual(expected, iniFile.Contents);
        }

        [TestMethod]
        public void TestStoreFile()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file.ext\r\nFile1 = \r\nFile2 = ";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            recentFileList.StoreFile(@"c:\abc\file.ext");
            
            Assert.AreEqual(expected, iniFile.Contents);
        }

        [TestMethod]
        public void TestStoreFileStoresAsQueue()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file3.ext\r\nFile1 = c:\\abc\\file2.ext\r\nFile2 = c:\\abc\\file1.ext";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file2.ext");
            recentFileList.StoreFile(@"c:\abc\file3.ext");

            Assert.AreEqual(expected, iniFile.Contents);
        }

        [TestMethod]
        public void TestStoreFilePushesFileOutOfList()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file4.ext\r\nFile1 = c:\\abc\\file3.ext\r\nFile2 = c:\\abc\\file2.ext";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file2.ext");
            recentFileList.StoreFile(@"c:\abc\file3.ext");
            recentFileList.StoreFile(@"c:\abc\file4.ext");

            Assert.AreEqual(expected, iniFile.Contents);
        }

        [TestMethod]
        public void TestStoreFilePushesSameFilesToTop()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file1.ext\r\nFile1 = c:\\abc\\file3.ext\r\nFile2 = c:\\abc\\file2.ext";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file2.ext");
            recentFileList.StoreFile(@"c:\abc\file3.ext");
            recentFileList.StoreFile(@"c:\abc\file1.ext");

            Assert.AreEqual(expected, iniFile.Contents);
        }

        [TestMethod]
        public void TestStoreFileIdempotent()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file1.ext\r\nFile1 = \r\nFile2 = ";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file1.ext");

            Assert.AreEqual(expected, iniFile.Contents);
        }

        /*
         * TODO: Add this functionality to RecentFileList
         * 
         * 
        [TestMethod]
        public void TestStoreFileNormalizesPaths()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file1.ext\r\nFile1 = \r\nFile2 = ";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);

            recentFileList.StoreFile(@"C:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"C:\abc\file1.ext");

            Assert.AreEqual(expected, iniFile.Contents);
        }
        */

        [TestMethod]
        public void TestRemoveFromList()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file3.ext\r\nFile1 = c:\\abc\\file1.ext\r\nFile2 = ";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);
            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file2.ext");
            recentFileList.StoreFile(@"c:\abc\file3.ext");

            recentFileList.RemoveFromList(1);

            Assert.AreEqual(expected, iniFile.Contents);
        }

        [TestMethod]
        public void TestRemoveFromListOnEmptyItemIndex()
        {
            const string expected = "[Recent Files]\r\nFile0 = c:\\abc\\file2.ext\r\nFile1 = c:\\abc\\file1.ext\r\nFile2 = ";
            var iniFile = new TestIniFile();
            var settings = new Settings(iniFile);
            var recentFileList = new RecentFileList(settings, 3);
            recentFileList.StoreFile(@"c:\abc\file1.ext");
            recentFileList.StoreFile(@"c:\abc\file2.ext");

            recentFileList.RemoveFromList(2);

            Assert.AreEqual(expected, iniFile.Contents);
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

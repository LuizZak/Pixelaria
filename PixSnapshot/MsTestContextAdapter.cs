/*
    PixSnapshot
    The MIT License (MIT)

    Copyright (c) 2018 Luiz Fernando

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PixSnapshot
{
    /// <summary>
    /// Context adapter over MSTest library's <see cref="TestContext"/> structure.
    /// </summary>
    public class MsTestContextAdapter : ITestContext
    {
        private readonly TestContext _testContext;

        public string TestName => _testContext.TestName;

        public string TestRunDirectory => _testContext.TestRunDirectory;

        public string FullyQualifiedTestClassName => _testContext.FullyQualifiedTestClassName;

        public MsTestContextAdapter(TestContext testContext)
        {
            _testContext = testContext;
        }

        public void AddResultFile(string fileName)
        {
            _testContext.AddResultFile(fileName);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Test adapter over MSTest library's <see cref="Assert" />.
    /// </summary>
    public class MsTestAdapter : IBitmapSnapshotTestAdapter
    {
        public void AssertFailure(string message)
        {
            Assert.Fail(message);
        }

        public string TestResultsSavePath()
        {
            string path = Path.GetFullPath(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, ".."));
            
            if(!path.EndsWith("bin\\Debug") && !path.EndsWith("bin\\Release"))
                Assert.Fail($"Invalid/unrecognized test assembly path {path}: Path must end in either bin\\Debug or bin\\Release");
            
            path = Path.GetFullPath(Path.Combine(path, "..\\..\\Snapshot\\Files"));

            return path;
        }

        public bool ReferenceImageExists(string filePath)
        {
            return File.Exists(filePath);
        }

        public Bitmap LoadReferenceImage(string filePath)
        {
            return (Bitmap)Image.FromFile(filePath);
        }
        
        public void SaveBitmapFile(Bitmap bitmap, string path)
        {
            string directoryName = Path.GetDirectoryName(path);
            Assert.IsNotNull(directoryName, "directoryName != null");

            // Verify path exists
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            using (var fileStream = File.OpenWrite(path))
            {
                bitmap.Save(fileStream, ImageFormat.Png);
            }
        }

        public void SaveComparisonBitmapFiles(Bitmap expected, string expectedPath, Bitmap actual, string actualPath, Bitmap diff, string diffPath)
        {
            SaveBitmapFile(expected, expectedPath);
            SaveBitmapFile(actual, actualPath);
            SaveBitmapFile(diff, diffPath);
        }
    }
}
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
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
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
        private readonly string _basePath;

        public MsTestAdapter()
        {
            
        }

        /// <summary>
        /// Instantiates a new MSTest adapter instance with the base path for
        /// the snapshot files configured to be relative to a given type's assembly
        /// </summary>
        /// <param name="assemblyType">A type from the assembly invoking this PixSnapshot assembly</param>
        public MsTestAdapter(Type assemblyType)
        {
            _basePath = GetPath(assemblyType);
        }

        public void AssertFailure(string message)
        {
            Assert.Fail(message);
        }

        public string TestResultsSavePath()
        {
            string path = _basePath ?? GetPath(null);

            if (path.EndsWith(@"bin\Debug") || path.EndsWith(@"bin\Release"))
            {
                return Path.GetFullPath(Path.Combine(path, @"..\..\Snapshot\Files"));
            }

            if (Regex.IsMatch(path, @"bin\\Debug\\net\d+") || Regex.IsMatch(path, @"bin\\Release\\net\d+"))
            {
                return Path.GetFullPath(Path.Combine(path, @"..\..\..\Snapshot\Files"));
            }
            if (path.EndsWith(@"bin\Debug") || path.EndsWith(@"bin\Release"))
            {
                return Path.GetFullPath(Path.Combine(path, @"..\..\Snapshot\Files"));
            }

            Assert.Fail($@"Invalid/unrecognized test assembly path {path}: Path must end in either bin\[Debug|Release] or bin\[Debug|Release]\[netxyz|netcore|netstandard]");

            return path;
        }

        private static string GetPath([CanBeNull] Type type)
        {
            var assembly = type?.Assembly ?? System.Reflection.Assembly.GetExecutingAssembly();
            var location = new Uri(assembly.GetName().CodeBase);

            var directoryInfo = new FileInfo(location.LocalPath).Directory;
            Debug.Assert(directoryInfo != null, nameof(directoryInfo) + " != null");

            return directoryInfo.FullName;
        }

        public bool ReferenceImageExists(string filePath)
        {
            return File.Exists(RootLongPath(filePath));
        }

        public Bitmap LoadReferenceImage(string filePath)
        {
            using (var fileStream = File.OpenRead(RootLongPath(filePath)))
            {
                return (Bitmap)Image.FromStream(fileStream);
            }
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

            using (var fileStream = File.OpenWrite(RootLongPath(path)))
            {
                bitmap.Save(fileStream, ImageFormat.Png);
            }
        }

        private static string RootLongPath(string path)
        {
            return !Path.IsPathRooted(path) ? path : $@"\\?\{path}";
        }

        public void SaveComparisonBitmapFiles(Bitmap expected, string expectedPath, Bitmap actual, string actualPath, Bitmap diff, string diffPath)
        {
            SaveBitmapFile(expected, expectedPath);
            SaveBitmapFile(actual, actualPath);
            SaveBitmapFile(diff, diffPath);
        }
    }
}
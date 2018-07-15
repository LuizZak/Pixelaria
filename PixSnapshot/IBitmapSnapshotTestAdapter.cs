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

using System.Drawing;
using JetBrains.Annotations;

namespace PixSnapshot
{
    /// <summary>
    /// An adapter over assertion frameworks for plugging into <see cref="BitmapSnapshotTesting"/>.
    /// </summary>
    public interface IBitmapSnapshotTestAdapter
    {
        /// <summary>
        /// Signals an assertion failure during a Snapshot test.
        /// </summary>
        void AssertFailure([NotNull] string message);

        /// <summary>
        /// Gets a string that indicates the base path to save reference images for tests into.
        /// </summary>
        string TestResultsSavePath();

        /// <summary>
        /// Asks the test fixture whether a reference image exists at a given path on disk.
        /// </summary>
        bool ReferenceImageExists([NotNull] string filePath);

        /// <summary>
        /// Asks the test fixture to load the reference bitmap image to compare to from a given filepath
        /// </summary>
        [NotNull]
        Bitmap LoadReferenceImage([NotNull] string filePath);

        /// <summary>
        /// Asks the test fixture to record a new/overwrite an existing bitmap file onto the file system.
        /// </summary>
        void SaveBitmapFile([NotNull] Bitmap bitmap, [NotNull] string path);

        /// <summary>
        /// Asks the test fixture to save two comparison result bitmaps into disk.
        /// </summary>
        /// <remarks>
        /// The <see cref="diff"/> bitmaps reported by <see cref="BitmapSnapshotTesting"/> are temporary
        /// and will be disposed after the method returns control.
        /// </remarks>
        void SaveComparisonBitmapFiles([NotNull] Bitmap expected, [NotNull] string expectedPath,
            [NotNull] Bitmap actual, [NotNull] string actualPath, [NotNull] Bitmap diff, [NotNull] string diffPath);
    }

    /// <summary>
    /// Provides encapsulation needed over a test context to automate work related to saving recorded
    /// test bitmaps and comparison results etc.
    /// </summary>
    public interface ITestContext
    {
        /// <summary>
        /// Gets a string that represents the currently running test's name.
        /// </summary>
        string TestName { get; }

        /// <summary>
        /// The base test execution directory.
        /// </summary>
        string TestRunDirectory { get;  }

        /// <summary>
        /// Gets the fully qualified test class name currently running the test.
        /// </summary>
        string FullyQualifiedTestClassName { get; }

        /// <summary>
        /// Records a test result file name.
        /// </summary>
        void AddResultFile(string fileName);
    }
}
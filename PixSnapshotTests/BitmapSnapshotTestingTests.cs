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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FastBitmapLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixSnapshot;

namespace PixSnapshotTests
{
    [TestClass]
    public class BitmapSnapshotTestingTests
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            BitmapSnapshotTesting.SeparateDirectoriesPerNamespace = false;
        }

        [TestMethod]
        public void TestSnapshotSuccess()
        {
            var bitmapRef = new Bitmap(16, 16);
            var bitmapAct = new Bitmap(16, 16);
            var testContext = new MockTestContext();
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = true
            };
            
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false);
            
            Assert.IsNull(testAdapter.AssertFailure_message);
            Assert.IsNull(testAdapter.SaveBitmapFile_bitmap);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actualPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_diff);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_diffPath);
        }
        
        [TestMethod]
        public void TestSnapshotSuccessWithTolerance()
        {
            var bitmapRef = new Bitmap(10, 10);
            var bitmapAct = new Bitmap(10, 10); bitmapAct.SetPixel(0, 0, Color.Red);
            var testContext = new MockTestContext();
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = true
            };
            
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false, tolerance: 0.02f);
            
            Assert.IsNull(testAdapter.AssertFailure_message);
            Assert.IsNull(testAdapter.SaveBitmapFile_bitmap);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actualPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_diff);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_diffPath);
        }

        [TestMethod]
        public void TestSnapshotFailureWhenNotEqual()
        {
            var bitmapRef = new Bitmap(16, 16);
            var bitmapAct = new Bitmap(16, 16); bitmapAct.SetPixel(0, 0, Color.Red);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = true
            };
            
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false);
            
            Assert.IsNotNull(testAdapter.AssertFailure_message);
            Assert.AreEqual(bitmapRef, testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.AreEqual(@"C:\Test\Artifacts\Test.TestClass\TestName-expected.png", testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.AreEqual(bitmapAct, testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.AreEqual(@"C:\Test\Artifacts\Test.TestClass\TestName-actual.png", testAdapter.SaveComparisonBitmapFiles_actualPath);
            Assert.IsNotNull(testAdapter.SaveComparisonBitmapFiles_diff);
            Assert.AreEqual(@"C:\Test\Artifacts\Test.TestClass\TestName-diff.png", testAdapter.SaveComparisonBitmapFiles_diffPath);
            Assert.IsTrue(testContext.AddResultFile_fileNames.Contains(testAdapter.SaveComparisonBitmapFiles_expectedPath));
            Assert.IsTrue(testContext.AddResultFile_fileNames.Contains(testAdapter.SaveComparisonBitmapFiles_actualPath));
            Assert.IsTrue(testContext.AddResultFile_fileNames.Contains(testAdapter.SaveComparisonBitmapFiles_diffPath));
        }

        [TestMethod]
        public void TestSnapshotFailureWhenNotEqualWithTolerance()
        {
            var bitmapRef = new Bitmap(10, 10);
            var bitmapAct = new Bitmap(10, 10); bitmapAct.SetPixel(0, 0, Color.Red); bitmapAct.SetPixel(1, 0, Color.Red);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = true
            };
            
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false, tolerance: 0.01f);
            
            Assert.IsNotNull(testAdapter.AssertFailure_message);
            Assert.AreEqual(bitmapRef, testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.AreEqual(@"C:\Test\Artifacts\Test.TestClass\TestName-expected.png", testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.AreEqual(bitmapAct, testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.AreEqual(@"C:\Test\Artifacts\Test.TestClass\TestName-actual.png", testAdapter.SaveComparisonBitmapFiles_actualPath);
            Assert.IsNotNull(testAdapter.SaveComparisonBitmapFiles_diff);
            Assert.AreEqual(@"C:\Test\Artifacts\Test.TestClass\TestName-diff.png", testAdapter.SaveComparisonBitmapFiles_diffPath);
            Assert.IsTrue(testContext.AddResultFile_fileNames.Contains(testAdapter.SaveComparisonBitmapFiles_expectedPath));
            Assert.IsTrue(testContext.AddResultFile_fileNames.Contains(testAdapter.SaveComparisonBitmapFiles_actualPath));
            Assert.IsTrue(testContext.AddResultFile_fileNames.Contains(testAdapter.SaveComparisonBitmapFiles_diffPath));
        }

        [DataRow(DiffTestCase.Rainbow)]
        [DataRow(DiffTestCase.Square)]
        [DataRow(DiffTestCase.Panel)]
        [DataTestMethod]
        public void TestDiffImageGeneration(DiffTestCase testCase)
        {
            DiffTest(testCase);
        }
        
        [TestMethod]
        public void TestSnapshotFailureWhenReferenceImageNotFound()
        {
            var bitmapRef = new Bitmap(16, 16);
            var bitmapAct = new Bitmap(16, 16); bitmapAct.SetPixel(0, 0, Color.Red);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = false
            };
            
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false);
            
            Assert.IsNotNull(testAdapter.AssertFailure_message);
            Assert.IsNull(testAdapter.SaveBitmapFile_bitmap);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actualPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_diff);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_diffPath);
        }

        [TestMethod]
        public void TestSnapshotRecordingAlwaysFails()
        {
            var bitmapAct = new Bitmap(16, 16);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                ReferenceImageExists_return = false
            };
            
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, true);
            
            Assert.IsNotNull(testAdapter.AssertFailure_message);
        }

        [TestMethod]
        public void TestSnapshotRecordingModeRecordsReferenceImage()
        {
            // Arrange
            var bitmapAct = new Bitmap(16, 16);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                ReferenceImageExists_return = false,
                TestResultsSavePath_return = @"C:\Test\TestFilesPath"
            };

            // Act
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, true);

            // Assert
            Assert.IsNotNull(testAdapter.AssertFailure_message);
            
            Assert.AreEqual(bitmapAct, testAdapter.SaveBitmapFile_bitmap);
            Assert.AreEqual(@"C:\Test\TestFilesPath\Test.TestClass\TestName.png", testAdapter.SaveBitmapFile_path);

            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actualPath);
        }
        
        [TestMethod]
        public void TestSnapshotSuccessSeparateDirectoriesPerNamespace()
        {
            var bitmapRef = new Bitmap(16, 16);
            var bitmapAct = new Bitmap(16, 16);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = true,
                TestResultsSavePath_return = @"C:\Test\TestFilesPath"
            };

            BitmapSnapshotTesting.SeparateDirectoriesPerNamespace = true;
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false);
            
            Assert.AreEqual(@"C:\Test\TestFilesPath\Test\TestClass\TestName.png", testAdapter.LoadReferenceImage_filePath);
        }

        [TestMethod]
        public void TestSnapshotRecordingModeSeparateDirectoriesPerNamespace()
        {
            // Arrange
            var bitmapAct = new Bitmap(16, 16);
            var testContext = new MockTestContext
            {
                TestRunDirectory = "C:\\Test\\Artifacts",
                FullyQualifiedTestClassName = "Test.TestClass",
                TestName = "TestName"
            };
            var testAdapter = new MockTestAdapter
            {
                ReferenceImageExists_return = false,
                TestResultsSavePath_return = @"C:\Test\TestFilesPath"
            };

            // Act
            BitmapSnapshotTesting.SeparateDirectoriesPerNamespace = true;
            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, true);

            // Assert
            Assert.IsNotNull(testAdapter.AssertFailure_message);
            
            Assert.AreEqual(bitmapAct, testAdapter.SaveBitmapFile_bitmap);
            Assert.AreEqual(@"C:\Test\TestFilesPath\Test\TestClass\TestName.png", testAdapter.SaveBitmapFile_path);

            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expected);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_expectedPath);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actual);
            Assert.IsNull(testAdapter.SaveComparisonBitmapFiles_actualPath);
        }

        private void DiffTest(DiffTestCase testCase)
        {
            var bitmapRef = ExpectedBitmapForDiffTest(testCase);
            var bitmapAct = ActualBitmapForDiffTest(testCase);
            var bitmapDiff = DiffBitmapForDiffTest(testCase);
            var testContext = new MockTestContext();
            var testAdapter = new MockTestAdapter
            {
                LoadReferenceImage_return = bitmapRef,
                ReferenceImageExists_return = true
            };

            BitmapSnapshotTesting.Snapshot<MockSnapshotProvider, Bitmap>(bitmapAct, testAdapter, testContext, false);
            
            using (var expBitDiff = bitmapDiff.FastLock())
            using (var actBitDiff = testAdapter.SaveComparisonBitmapFiles_diff.FastLock())
            {
                var expBitDiffArray = expBitDiff.DataArray;
                var actBitDiffArray = actBitDiff.DataArray;

                bool match = expBitDiffArray.SequenceEqual(actBitDiffArray);

                if (match)
                    return; // Success!

                string expPath = Path.Combine(TestContext.TestDir, $"diff-test-{testCase}-expected.png");
                string actPath = Path.Combine(TestContext.TestDir, $"diff-test-{testCase}-actual.png");
                string diffPath = Path.Combine(TestContext.TestDir, $"diff-test-{testCase}-diff.png");

                var diffBit = new Bitmap(expBitDiff.Width, expBitDiff.Height);
                using (var diffBitFast = diffBit.FastLock())
                {
                    diffBitFast.Clear(unchecked((int)0xFFFFFFFF));
                    for (int i = 0; i < expBitDiffArray.Length; i++)
                    {
                        if (expBitDiffArray[i] != actBitDiffArray[i])
                        {
                            int x = i % expBitDiff.Width;
                            int y = i / expBitDiff.Width;

                            diffBitFast.SetPixel(x, y, unchecked((int)0xFFFF0000));
                        }
                    }
                }

                bitmapDiff.Save(expPath, ImageFormat.Png);
                testAdapter.SaveComparisonBitmapFiles_diff.Save(actPath, ImageFormat.Png);
                diffBit.Save(diffPath, ImageFormat.Png);

                Assert.Fail($"Images do not match! Inspect the resulting image at {TestContext.TestDir}");
            }
        }

        private static Bitmap ExpectedBitmapForDiffTest(DiffTestCase testCase)
        {
            switch (testCase)
            {
                case DiffTestCase.Rainbow:
                    return Resource.diff_test_rainbow_expected;
                case DiffTestCase.Square:
                    return Resource.diff_test_square_expected;
                case DiffTestCase.Panel:
                    return Resource.diff_test_panel_expected;
                default:
                    throw new ArgumentOutOfRangeException(nameof(testCase), testCase, null);
            }
        }
        
        private static Bitmap ActualBitmapForDiffTest(DiffTestCase testCase)
        {
            switch (testCase)
            {
                case DiffTestCase.Rainbow:
                    return Resource.diff_test_rainbow_actual;
                case DiffTestCase.Square:
                    return Resource.diff_test_square_actual;
                case DiffTestCase.Panel:
                    return Resource.diff_test_panel_actual;
                default:
                    throw new ArgumentOutOfRangeException(nameof(testCase), testCase, null);
            }
        }

        private static Bitmap DiffBitmapForDiffTest(DiffTestCase testCase)
        {
            switch (testCase)
            {
                case DiffTestCase.Rainbow:
                    return Resource.diff_test_rainbow_diff;
                case DiffTestCase.Square:
                    return Resource.diff_test_square_diff;
                case DiffTestCase.Panel:
                    return Resource.diff_test_panel_diff;
                default:
                    throw new ArgumentOutOfRangeException(nameof(testCase), testCase, null);
            }
        }

        private class MockSnapshotProvider : ISnapshotProvider<Bitmap>
        {
            public Bitmap GenerateBitmap(Bitmap context)
            {
                return context;
            }
        }
        
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class MockTestAdapter : IBitmapSnapshotTestAdapter
        {
            public bool ReferenceImageExists_return = true;

            public Bitmap LoadReferenceImage_return = new Bitmap(1, 1);
            public string LoadReferenceImage_filePath;

            public string AssertFailure_message;

            public string TestResultsSavePath_return = @"C:\Test\TestFilesPath";

            public Bitmap SaveComparisonBitmapFiles_expected;
            public string SaveComparisonBitmapFiles_expectedPath;
            public Bitmap SaveComparisonBitmapFiles_actual;
            public string SaveComparisonBitmapFiles_actualPath;
            public Bitmap SaveComparisonBitmapFiles_diff;
            public string SaveComparisonBitmapFiles_diffPath;
            
            public Bitmap SaveBitmapFile_bitmap;
            public string SaveBitmapFile_path;

            public void AssertFailure(string message)
            {
                AssertFailure_message = message;
            }
            
            public string TestResultsSavePath()
            {
                return TestResultsSavePath_return;
            }

            public bool ReferenceImageExists(string filePath)
            {
                return ReferenceImageExists_return;
            }

            public Bitmap LoadReferenceImage(string filePath)
            {
                LoadReferenceImage_filePath = filePath;
                return LoadReferenceImage_return;
            }

            public void SaveComparisonBitmapFiles(Bitmap expected, string expectedPath, Bitmap actual, string actualPath, Bitmap diff, string diffPath)
            {
                SaveComparisonBitmapFiles_expected = expected;
                SaveComparisonBitmapFiles_expectedPath = expectedPath;
                SaveComparisonBitmapFiles_actual = actual;
                SaveComparisonBitmapFiles_actualPath = actualPath;
                SaveComparisonBitmapFiles_diff = diff.DeepClone(); // Copy bitmap since this will be disposed after this method returns
                SaveComparisonBitmapFiles_diffPath = diffPath;
            }

            public void SaveBitmapFile(Bitmap bitmap, string path)
            {
                SaveBitmapFile_bitmap = bitmap;
                SaveBitmapFile_path = path;
            }
        }
        
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        private class MockTestContext: ITestContext
        {
            public readonly List<string> AddResultFile_fileNames = new List<string>();

            public string TestName { get; set; } = "TestName";
            public string TestRunDirectory { get; set; } = "C:\\Path";
            public string FullyQualifiedTestClassName { get; set; } = "Test.TestClass";

            public void AddResultFile(string fileName)
            {
                AddResultFile_fileNames.Add(fileName);
            }
        }

        public enum DiffTestCase
        {
            Rainbow,
            Square,
            Panel
        }
    }
}

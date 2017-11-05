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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Algorithms.PaintOperations;
using Pixelaria.Algorithms.PaintOperations.Interfaces;
using PixelariaTests.Generators;
using PixelariaTests.Tests.Utils;
using Rhino.Mocks;

namespace PixelariaTests.Tests.PaintOperations
{
    /// <summary>
    /// Tests the pencil paint operation and related components
    /// </summary>
    [TestClass]
    public class PencilPaintOperationTests
    {
        /// <summary>
        /// Specifies the name of the operation currently being tested
        /// </summary>
        public const string OperationName = "Pencil";

        /// <summary>
        /// Tests the property return behavior for the PencilPaintOperation
        /// </summary>
        [TestMethod]
        public void TestPaintOperationProperties()
        {
            var target = new Bitmap(64, 64);
            var target2 = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            var operation = new PencilPaintOperation(target) { Color = Color.Black };

            // Check TargetBitmap property
            Assert.AreEqual(operation.TargetBitmap, target, "The TargetBitmap property for the paint operation must point to the bitmap that was passed on its constructor");

            // Modify target bitmap
            operation.TargetBitmap = target2;

            // Check OperationStarted property
            operation.StartOpertaion();
            Assert.IsTrue(operation.OperationStarted, "After a call to StartOperation(), an operation's OperationStarted property should return true");

            operation.FinishOperation();
            Assert.IsFalse(operation.OperationStarted, "After a call to FinishOperation(), an operation's OperationStarted property should return false");
        }

        /// <summary>
        /// Tests a simple scribble composed of lines
        /// </summary>
        [TestMethod]
        public void TestBasicPaintOperation()
        {
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            var operation = new PencilPaintOperation(target) { Color = Color.Black };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x83, 0xF6, 0x30, 0x43, 0x15, 0xE0, 0x5C, 0x92, 0xDE, 0x39, 0x7E, 0x5D, 0x36, 0x5D, 0x4, 0xCB, 0xE9,
                0xBA, 0xDC, 0xE0, 0xFC, 0xF4, 0x25, 0x1C, 0x69, 0xE, 0x88, 0xE5, 0x30, 0xC7, 0x26, 0xE4
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_BasicPaint");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests a simple scribble composed of lines with a different sized brush
        /// </summary>
        [TestMethod]
        public void TestBasicSizedPaintOperation()
        {
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            var operation = new PencilPaintOperation(target) { Color = Color.Black, Size = 5 };

            operation.StartOpertaion();

            Assert.IsTrue(operation.OperationStarted, "After a call to StartOperation(), an operation's OperationStarted property should return true");

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);

            operation.FinishOperation();

            Assert.IsFalse(operation.OperationStarted, "After a call to FinishOperation(), an operation's OperationStarted property should return false");

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x58, 0x68, 0x8, 0x95, 0xAB, 0x56, 0xA5, 0x94, 0x9B, 0xBA, 0xF5, 0xBE, 0xD, 0xE6, 0xCF, 0x39, 0x8B,
                0x22, 0xDB, 0x2F, 0x96, 0x66, 0x17, 0x31, 0x9C, 0x6, 0xA3, 0xFC, 0x42, 0x3C, 0xF8, 0x68
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_BasicSizedPaintBrush");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests a paint operation that crosses the boundaries of the affected bitmap
        /// </summary>
        [TestMethod]
        public void TestOutOfBoundsPaintOperation()
        {
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            var operation = new PencilPaintOperation(target) { Color = Color.Black };

            operation.StartOpertaion();

            operation.MoveTo(-20, 40);
            operation.DrawTo(40, -20);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x54, 0x86, 0x93, 0xA8, 0x8C, 0xC4, 0xDD, 0xCC, 0xC9, 0xEB, 0x0, 0x65, 0x30, 0xD, 0x4D, 0x9C, 0x86,
                0xC8, 0x30, 0x1B, 0x9F, 0xE9, 0x1D, 0x9D, 0x9B, 0x94, 0xD0, 0x8, 0xBA, 0xDF, 0x6F, 0xE9
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_OutOfBoundsPaint");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests the PlotPoint method
        /// </summary>
        [TestMethod]
        public void TestPlotPixelOperation()
        {
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);
            
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver
            };

            operation.StartOpertaion();

            operation.PlotPixel(5, 5);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x16, 0x49, 0x46, 0x19, 0x5D, 0xA4, 0xE7, 0x28, 0x41, 0x6, 0xC5, 0xB2, 0x3, 0x59, 0x45, 0x28, 0x65,
                0x34, 0xE0, 0xD, 0x74, 0x19, 0x5D, 0xD8, 0xDE, 0x3A, 0x3D, 0x18, 0x3C, 0xC6, 0xDB, 0xD8
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_PlotPixel");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests a single plot on the bitmap
        /// </summary>
        [TestMethod]
        public void TestSinglePixelPaintOperation()
        {
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            var operation = new PencilPaintOperation(target, true)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x16, 0x49, 0x46, 0x19, 0x5D, 0xA4, 0xE7, 0x28, 0x41, 0x06, 0xC5, 0xB2, 0x03, 0x59, 0x45, 0x28, 0x65,
                0x34, 0xE0, 0x0D, 0x74, 0x19, 0x5D, 0xD8, 0xDE, 0x3A, 0x3D, 0x18, 0x3C, 0xC6, 0xDB, 0xD8
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_SinglePlotPaint");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests that the pencil operation is working correctly with a transparent color and a source copy compositing mode
        /// </summary>
        [TestMethod]
        public void TestSourceCopyTransparentOperation()
        {
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            var operation = new PencilPaintOperation(target, true)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceCopy
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x32, 0xC8, 0x4C, 0x43, 0x0D, 0x90, 0x1B, 0x12, 0xC7, 0xB1, 0xB6, 0x30, 0x08, 0x86, 0xCF, 0xB6, 0x49,
                0xDD, 0x09, 0x5D, 0xAA, 0x6D, 0x41, 0x0D, 0x27, 0xE1, 0x2D, 0x70, 0x68, 0xED, 0xA4, 0x66
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_SourceCopyTransparentPaint");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests that the pencil operation is working correctly with a transparent color and a source over compositing mode
        /// </summary>
        [TestMethod]
        public void TestSourceOverTransparentOperation()
        {
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x57, 0x9F, 0xBD, 0x24, 0xEE, 0x05, 0x6E, 0xB8, 0xAA, 0xB4, 0xB6, 0x45, 0x50, 0x7A, 0x4A, 0xC1, 0xD5,
                0x90, 0xB1, 0x4F, 0xF6, 0xD5, 0x97, 0xFB, 0xD2, 0xD3, 0x7E, 0xF0, 0x7B, 0x09, 0x92, 0x06
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_SourceOverTransparentPaint");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation must match the good hash stored");
        }

        /// <summary>
        /// Tests that the pencil operation is working correctly with a transparent color, a source over compositing mode, and accumulate alpha set to false
        /// </summary>
        [TestMethod]
        public void TestSourceOverAlphaAccumulationOffTransparentOperation()
        {
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver
            };

            operation.StartOpertaion(false);

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0x7E, 0x3C, 0xA4, 0xBC, 0x24, 0xC2, 0x50, 0x1B, 0x82, 0xE7, 0xFD, 0xCD, 0x37, 0x85, 0x79, 0x40, 0xD2,
                0x7F, 0x04, 0xD8, 0xCE, 0xEB, 0x80, 0x9D, 0x4F, 0x58, 0xBC, 0x21, 0xF4, 0x49, 0x59, 0xAB
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_SourceOverTransparentPaint_AccumulateAlphaOff");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation must match the good hash stored");
        }

        /// <summary>
        /// Tests that the pencil operation is working correctly with a different size, transparent color and a source over compositing mode
        /// </summary>
        [TestMethod]
        public void TestSourceOverSizedPaintOperation()
        {
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            var operation = new PencilPaintOperation(target) { Color = Color.FromArgb(127, 0, 0, 0), Size = 5, CompositingMode = CompositingMode.SourceOver };

            operation.StartOpertaion();

            Assert.IsTrue(operation.OperationStarted, "After a call to StartOperation(), an operation's OperationStarted property should return true");

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);

            operation.FinishOperation();

            Assert.IsFalse(operation.OperationStarted, "After a call to FinishOperation(), an operation's OperationStarted property should return false");

            // Hash of the .png image that represents the target result of the paint operation. Generated through the 'RegisterResultBitmap' method
            byte[] goodHash =
            {
                0xD1, 0xDF, 0x77, 0x3B, 0x39, 0x61, 0x9B, 0x3D, 0xB5, 0x0D, 0x25, 0x63, 0x98, 0x91, 0xD8, 0x28, 0x38,
                0x86, 0x55, 0xD6, 0xD1, 0x54, 0xBF, 0x3D, 0x80, 0x16, 0x4E, 0x84, 0xC8, 0x5D, 0x2F, 0x6E
            };
            byte[] currentHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_SourceOverSizedPaintBrush");

            Assert.IsTrue(goodHash.SequenceEqual(currentHash), "The hash for the paint operation does not match the good hash stored. Verify the output image for an analysis of what went wrong");
        }

        /// <summary>
        /// Tests whether the notification system is working properly
        /// </summary>
        [TestMethod]
        public void TestOperationNotification()
        {
            // Setup the test by performing a few pencil strokes
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            // Stub a notifier
            var stubNotifier = MockRepository.GenerateStub<IPlottingOperationNotifier>(null);

            // Create the operation and perform it
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.Black,
                Notifier = stubNotifier
            };

            // Perform the operation
            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Test the results
            // Test line from 5x5 -> 10x10
            for (int i = 5; i <= 10; i++)
            {
                var i1 = i;
                stubNotifier.AssertWasCalled(x => x.PlottedPixel(new Point(i1, i1), Color.Transparent.ToArgb(), Color.Black.ToArgb()));
            }
            // Test line that goes back from 9x9 -> 5x5, in which the black pixels due to the previous DrawTo() are ignored
            for (int i = 5; i < 10; i++)
            {
                var i1 = i;
                stubNotifier.AssertWasNotCalled(x => x.PlottedPixel(new Point(i1, i1), Color.Black.ToArgb(), Color.Black.ToArgb()));
            }
        }

        #region Undo Operation

        /// <summary>
        /// Tests the undo for the pencil paint operation
        /// </summary>
        [TestMethod]
        public void TestUndoOperation()
        {
            // Create the objects
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target) { Color = Color.Black, Notifier = generator };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Test the undo task target
            Assert.AreEqual(generator.UndoTask.TargetBitmap, target, "The target for a bitmap undo should be the bitmap that was operated upon");

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndo");

            Assert.IsTrue(originalHash.SequenceEqual(afterUndoHash), "After undoing a paint operation's task, its pixels must return to their original state before the operation was applied");
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation with a different brush size
        /// </summary>
        [TestMethod]
        public void TestUndoOperationSized()
        {
            // Create the objects
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target) { Color = Color.Black, Notifier = generator, Size = 5 };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndoSized");

            Assert.IsTrue(originalHash.SequenceEqual(afterUndoHash), "After undoing a paint operation's task, its pixels must return to their original state before the operation was applied");
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceOver compositing mode
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceOverAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndo_SourceOverAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterUndoHash), "After undoing a paint operation's task, its pixels must return to their original state before the operation was applied");
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceCopy compositing mode
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceCopyAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceCopy,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndo_SourceCopyAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterUndoHash), "After undoing a paint operation's task, its pixels must return to their original state before the operation was applied");
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceCopy compositing mode and having no accumulation of transparency on
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceOverAlpha_NoAccumulateAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Notifier = generator
            };

            operation.StartOpertaion(false);

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndo_SourceCopyAlpha_NoAccumulateAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterUndoHash), "After undoing a paint operation's task, its pixels must return to their original state before the operation was applied");
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceOver compositing mode with a different brush size
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SizedSourceOverAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Size = 5,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndo_SizedSourceOverAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterUndoHash), "After undoing a paint operation's task, its pixels must return to their original state before the operation was applied");
        }

        #endregion

        #region Redo Operation

        /// <summary>
        /// Tests the redo for the pencil paint operation
        /// </summary>
        [TestMethod]
        public void TestRedoOperation()
        {
            // Create the objects
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target) { Color = Color.Black, Notifier = generator };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            byte[] afterRedoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterRedo");

            Assert.IsTrue(originalHash.SequenceEqual(afterRedoHash), "After redoing a paint operation's task, its pixels must return to their original state after the operation was applied");
        }

        /// <summary>
        /// Tests the redo for the pencil paint operation with a sized brush
        /// </summary>
        [TestMethod]
        public void TestRedoOperationSized()
        {
            // Create the objects
            var target = new Bitmap(64, 64);
            FastBitmap.ClearBitmap(target, Color.Transparent);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target) { Color = Color.Black, Notifier = generator, Size = 5 };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            byte[] afterRedoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterRedoSized");

            Assert.IsTrue(originalHash.SequenceEqual(afterRedoHash), "After redoing a paint operation's task, its pixels must return to their original state after the operation was applied");
        }

        /// <summary>
        /// Tests the redo for the pencil paint operation using a SourceOver compositing mode
        /// </summary>
        [TestMethod]
        public void TestRedoOperation_SourceOverAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            byte[] afterRedoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterRedo_SourceOverAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterRedoHash), "After redoing a paint operation's task, its pixels must return to their original state after the operation was applied");
        }

        /// <summary>
        /// Tests the redo for the pencil paint operation using a SourceCopy compositing mode
        /// </summary>
        [TestMethod]
        public void TestRedoOperation_SourceCopyAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceCopy,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            byte[] afterRedoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterRedo_SourceCopyAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterRedoHash), "After redoing a paint operation's task, its pixels must return to their original state after the operation was applied");
        }

        /// <summary>
        /// Tests the redo for the pencil paint operation using a SourceCopy compositing mode and having no accumulation of transparency on
        /// </summary>
        [TestMethod]
        public void TestRedoOperation_SourceOverAlpha_NoAccumulateAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Notifier = generator
            };

            operation.StartOpertaion(false);

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            byte[] afterRedoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterRedo_SourceCopyAlpha_NoAccumulateAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterRedoHash), "After redoing a paint operation's task, its pixels must return to their original state after the operation was applied");
        }

        /// <summary>
        /// Tests the redo for the pencil paint operation using a SourceOver compositing mode and a different sized brush
        /// </summary>
        [TestMethod]
        public void TestRedoOperation_SizedSourceOverAlpha()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil");
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Size = 5,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            byte[] afterRedoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterRedo_SizedSourceOverAlpha");

            Assert.IsTrue(originalHash.SequenceEqual(afterRedoHash), "After redoing a paint operation's task, its pixels must return to their original state after the operation was applied");
        }

        #endregion

        #region Force-Failed Undo Operations

        /// <summary>
        /// Tests the undo for the pencil paint operation failing by specifying an undo task that replaces pixel colors that are being drawn over repeatedly
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceOverAlphaFailing()
        {
            // Create the objects
            var target = BitmapGenerator.GenerateRandomBitmap(64, 64, 10);
            byte[] originalHash = UtilsTests.GetHashForBitmap(target);

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil", keepReplacedUndos: false);
            var operation = new PencilPaintOperation(target)
            {
                Color = Color.FromArgb(127, 0, 0, 0),
                CompositingMode = CompositingMode.SourceOver,
                Notifier = generator
            };

            operation.StartOpertaion();

            operation.MoveTo(5, 5);
            operation.DrawTo(10, 10);
            operation.DrawTo(15, 17);
            operation.DrawTo(20, 25);
            operation.DrawTo(25, 37);
            operation.DrawTo(5, 5);

            operation.FinishOperation();

            // Undo the task
            generator.UndoTask.Undo();

            byte[] afterUndoHash = UtilsTests.GetHashForBitmap(target);

            RegisterResultBitmap(target, "PencilOperation_AfterUndo_SourceOverAlpha_Failed");

            Assert.IsFalse(originalHash.SequenceEqual(afterUndoHash),
                "Plotting the same pixel repeatedly with an undo generator that has keepReplacedOriginals should fail, since the redrawn pixels have their undo color replaced");
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Trying to call DrawTo() with an unstarted pencil paint operation should result in an InvalidOperationException")]
        public void TestUnstartedOperationException()
        {
            var bitmap = new Bitmap(64, 64);
            var paintOperation = new PencilPaintOperation(bitmap);

            paintOperation.DrawTo(5, 5);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException), "Trying to modify the target bitmap while under operation should result in an InvalidOperationException")]
        public void TestChangeBitmapUnderOperationException()
        {
            var bitmap = new Bitmap(64, 64);
            var paintOperation = new PencilPaintOperation(bitmap);

            paintOperation.StartOpertaion();

            paintOperation.DrawTo(5, 5);

            paintOperation.TargetBitmap = new Bitmap(64, 64);
        }

        /// <summary>
        /// Saves the specified bitmap on a desktop folder used to store resulting operations' bitmaps with the specified file name.
        /// The method saves both a .png format of the image, and a .txt file containing an array of bytes for the image's SHA256 hash
        /// </summary>
        /// <param name="bitmap">The bitmap to save</param>
        /// <param name="name">The file name to use on the bitmap</param>
        public void RegisterResultBitmap([NotNull] Bitmap bitmap, string name)
        {
            string folder = "PaintToolResults" + Path.DirectorySeparatorChar + OperationName;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + Path.DirectorySeparatorChar + folder;
            string file = path + Path.DirectorySeparatorChar + name;

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            bitmap.Save(file + ".png", ImageFormat.Png);

            // Also save a .txt file containing the hash
            byte[] hashBytes = UtilsTests.GetHashForBitmap(bitmap);
            string hashString = "";
            hashBytes.ToList().ForEach(b => hashString += (hashString.Length == 0 ? "" : ",") +"0x" + b.ToString("X2"));
            File.WriteAllText(file + ".txt", hashString);
        }
    }
}
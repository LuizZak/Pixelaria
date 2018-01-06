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
using FastBitmapLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Algorithms.PaintOperations;
using Pixelaria.Algorithms.PaintOperations.Interfaces;
using PixelariaTests.Generators;
using PixSnapshot;
using Rhino.Mocks;

namespace PixelariaTests.Algorithms.PaintOperations
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

        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            BitmapSnapshot.RecordMode = false;
        }

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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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
                int i1 = i;
                stubNotifier.AssertWasCalled(x => x.PlottedPixel(new Point(i1, i1), Color.Transparent.ToArgb(), Color.Black.ToArgb()));
            }
            // Test line that goes back from 9x9 -> 5x5, in which the black pixels due to the previous DrawTo() are ignored
            for (int i = 5; i < 10; i++)
            {
                int i1 = i;
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
            
            BitmapSnapshot.Snapshot(target, TestContext);
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

            BitmapSnapshot.Snapshot(target, TestContext);
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceOver compositing mode
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceOverAlpha()
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

            // Undo the task
            generator.UndoTask.Undo();

            BitmapSnapshot.Snapshot(target, TestContext);
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceCopy compositing mode
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceCopyAlpha()
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

            // Undo the task
            generator.UndoTask.Undo();

            BitmapSnapshot.Snapshot(target, TestContext);
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceCopy compositing mode and having no accumulation of transparency on
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SourceOverAlpha_NoAccumulateAlpha()
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

            // Undo the task
            generator.UndoTask.Undo();

            BitmapSnapshot.Snapshot(target, TestContext);
        }

        /// <summary>
        /// Tests the undo for the pencil paint operation using a SourceOver compositing mode with a different brush size
        /// </summary>
        [TestMethod]
        public void TestUndoOperation_SizedSourceOverAlpha()
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

            // Undo the task
            generator.UndoTask.Undo();

            BitmapSnapshot.Snapshot(target, TestContext);
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
            
            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            BitmapSnapshot.Snapshot(target, TestContext);
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
            
            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            BitmapSnapshot.Snapshot(target, TestContext);
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
            
            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            BitmapSnapshot.Snapshot(target, TestContext);
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
            
            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            BitmapSnapshot.Snapshot(target, TestContext);
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
            
            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            BitmapSnapshot.Snapshot(target, TestContext);
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
            
            // Undo and redo the task back
            generator.UndoTask.Undo();
            generator.UndoTask.Redo();

            BitmapSnapshot.Snapshot(target, TestContext);
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

            // Create the test subjects
            var generator = new PlottingPaintUndoGenerator(target, "Pencil", false);
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

            BitmapSnapshot.Snapshot(target, TestContext);
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
    }
}
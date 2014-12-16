using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Utils;
using PixelariaTests.Generators;

namespace PixelariaTests.Tests.Utils
{
    /// <summary>
    /// Contains tests for the FastBitmap class and related components
    /// </summary>
    [TestClass]
    public class FastBitmapTests
    {
        [TestMethod]
        public void TestFastBitmapCreation()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);
            fastBitmap.Lock();
            fastBitmap.Unlock();
        }

        /// <summary>
        /// Tests sequential instances of FastBitmaps on the same Bitmap.
        /// As long as all the operations pending on a fast bitmap are finished, the original bitmap can be used in as many future fast bitmaps as needed.
        /// </summary>
        [TestMethod]
        public void TestSequentialFastBitmap()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);
            fastBitmap.Lock();
            fastBitmap.Unlock();

            fastBitmap = new FastBitmap(bitmap);
            fastBitmap.Lock();
            fastBitmap.Unlock();
        }

        [TestMethod]
        public void TestClearBitmap()
        {
            Bitmap bitmap = FrameGenerator.GenerateRandomBitmap(64, 64);
            FastBitmap.ClearBitmap(bitmap, Color.Red);

            // Loop through the image checking the pixels now
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).ToArgb() != Color.Red.ToArgb())
                    {
                        Assert.Fail("Immediately after a call to FastBitmap.Clear(), all of the bitmap's pixels must be of the provided color");
                    }
                }
            }

            // Test an arbitratry color now
            FastBitmap.ClearBitmap(bitmap, Color.FromArgb(25, 12, 0, 42));

            // Loop through the image checking the pixels now
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    if (bitmap.GetPixel(x, y).ToArgb() != Color.FromArgb(25, 12, 0, 42).ToArgb())
                    {
                        Assert.Fail("Immediately after a call to FastBitmap.Clear(), all of the bitmap's pixels must be of the provided color");
                    }
                }
            }
        }

        /// <summary>
        /// Tests the behavior of the GetPixel() method by comparing the results from it to the results of the native Bitmap.GetPixel()
        /// </summary>
        [TestMethod]
        public void TestGetPixel()
        {
            Bitmap original = FrameGenerator.GenerateRandomBitmap(64, 64);
            Bitmap copy = original.Clone(new Rectangle(0, 0, 64, 64), original.PixelFormat);

            FastBitmap fastOriginal = new FastBitmap(original);
            fastOriginal.Lock();

            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    Assert.AreEqual(fastOriginal.GetPixel(x, y).ToArgb(), copy.GetPixel(x, y).ToArgb(),
                        "Calls to FastBitmap.GetPixel() must return the same value as returned by Bitmap.GetPixel()");
                }
            }

            fastOriginal.Unlock();
        }

        /// <summary>
        /// Tests the behavior of the SetPixel() method by randomly filling two bitmaps via native SetPixel and the implemented SetPixel, then comparing the output similarity
        /// </summary>
        [TestMethod]
        public void TestSetPixel()
        {
            Bitmap bitmap1 = new Bitmap(64, 64);
            Bitmap bitmap2 = new Bitmap(64, 64);

            FastBitmap fastBitmap1 = new FastBitmap(bitmap1);
            fastBitmap1.Lock();

            Random r = new Random();

            for (int y = 0; y < bitmap1.Height; y++)
            {
                for (int x = 0; x < bitmap1.Width; x++)
                {
                    int intColor = r.Next(0xFFFFFF);
                    Color color = Color.FromArgb(intColor);

                    fastBitmap1.SetPixel(x, y, color);
                    bitmap2.SetPixel(x, y, color);
                }
            }

            for (int y = 0; y < bitmap1.Height; y++)
            {
                for (int x = 0; x < bitmap1.Width; x++)
                {
                    Assert.AreEqual(fastBitmap1.GetPixel(x, y).ToArgb(), bitmap2.GetPixel(x, y).ToArgb(),
                        "Calls to FastBitmap.SetPixel() must be equivalent to calls to Bitmap.SetPixel()");
                }
            }

            fastBitmap1.Unlock();
        }

        /// <summary>
        /// Tests the behavior of the SetPixel() integer overload method by randomly filling two bitmaps via native SetPixel and the implemented SetPixel, then comparing the output similarity
        /// </summary>
        [TestMethod]
        public void TestSetPixelInt()
        {
            Bitmap bitmap1 = new Bitmap(64, 64);
            Bitmap bitmap2 = new Bitmap(64, 64);

            FastBitmap fastBitmap1 = new FastBitmap(bitmap1);
            fastBitmap1.Lock();

            Random r = new Random();

            for (int y = 0; y < bitmap1.Height; y++)
            {
                for (int x = 0; x < bitmap1.Width; x++)
                {
                    int intColor = r.Next(0xFFFFFF);
                    Color color = Color.FromArgb(intColor);

                    fastBitmap1.SetPixel(x, y, intColor);
                    bitmap2.SetPixel(x, y, color);
                }
            }

            for (int y = 0; y < bitmap1.Height; y++)
            {
                for (int x = 0; x < bitmap1.Width; x++)
                {
                    Assert.AreEqual(fastBitmap1.GetPixel(x, y).ToArgb(), bitmap2.GetPixel(x, y).ToArgb(),
                        "Calls to FastBitmap.SetPixel() with an integer overload must be equivalent to calls to Bitmap.SetPixel() with a Color with the same ARGB value as the interger");
                }
            }

            fastBitmap1.Unlock();
        }

        #region Exception Tests

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException),
            "When trying to unlock a FastBitmap that is not locked, an exception must be thrown")]
        public void TestFastBitmapUnlockingException()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.Unlock();
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException),
            "When trying to lock a FastBitmap that is already locked, an exception must be thrown")]
        public void TestFastBitmapLockingException()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.Lock();
            fastBitmap.Lock();
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException),
            "When trying to read or write to the FastBitmap via GetPixel while it is unlocked, an exception must be thrown"
            )]
        public void TestFastBitmapUnlockedGetAccessException()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.GetPixel(0, 0);
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException),
            "When trying to read or write to the FastBitmap via SetPixel while it is unlocked, an exception must be thrown"
            )]
        public void TestFastBitmapUnlockedSetAccessException()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.SetPixel(0, 0, 0);
        }

        [TestMethod]
        public void TestFastBitmapGetPixelBoundsException()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.Lock();

            try
            {
                fastBitmap.GetPixel(-1, -1);
                Assert.Fail("When trying to access a coordinate that is out of bounds via GetPixel, an exception must be thrown");
            } catch (ArgumentException) { }

            try
            {
                fastBitmap.GetPixel(fastBitmap.Width, 0);
                Assert.Fail("When trying to access a coordinate that is out of bounds via GetPixel, an exception must be thrown");
            }
            catch (ArgumentException) { }

            try
            {
                fastBitmap.GetPixel(0, fastBitmap.Height);
                Assert.Fail("When trying to access a coordinate that is out of bounds via GetPixel, an exception must be thrown");
            }
            catch (ArgumentException) { }

            fastBitmap.GetPixel(fastBitmap.Width - 1, fastBitmap.Height - 1);
        }

        [TestMethod]
        public void TestFastBitmapSetPixelBoundsException()
        {
            Bitmap bitmap = new Bitmap(64, 64);
            FastBitmap fastBitmap = new FastBitmap(bitmap);

            fastBitmap.Lock();

            try
            {
                fastBitmap.SetPixel(-1, -1, 0);
                Assert.Fail("When trying to access a coordinate that is out of bounds via GetPixel, an exception must be thrown");
            }
            catch (ArgumentException) { }

            try
            {
                fastBitmap.SetPixel(fastBitmap.Width, 0, 0);
                Assert.Fail("When trying to access a coordinate that is out of bounds via GetPixel, an exception must be thrown");
            }
            catch (ArgumentException) { }

            try
            {
                fastBitmap.SetPixel(0, fastBitmap.Height, 0);
                Assert.Fail("When trying to access a coordinate that is out of bounds via GetPixel, an exception must be thrown");
            }
            catch (ArgumentException) { }

            fastBitmap.SetPixel(fastBitmap.Width - 1, fastBitmap.Height - 1, 0);
        }

        #endregion
    }
}
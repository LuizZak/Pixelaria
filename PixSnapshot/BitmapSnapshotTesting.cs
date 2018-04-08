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

using FastBitmapLib;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PixSnapshot
{
    /// <summary>
    /// Helper static class to perform bitmap-based rendering comparisons as assertions.
    /// </summary>
    public static class BitmapSnapshotTesting
    {
        /// <summary>
        /// If true, when generating output folders for test results, paths are created for each segment of the namespace
        /// of the target test class, e.g. 'PixUI.Controls.LabelControlViewTests' becomes '...\PixUI\Controls\LabelControlViewtests\',
        /// otherwise a single folder with the fully-qualified class name is used instead.
        /// 
        /// If this property is changed across test recordings, the tests must be re-recorded to account for the new directory paths
        /// expected by the snapshot class.
        /// 
        /// Defaults to false.
        /// </summary>
        public static bool SeparateDirectoriesPerNamespace = false;

        /// <summary>
        /// Performs a snapshot text with a given test context/object pair, using an instantiable snapshot provider.
        /// </summary>
        public static void Snapshot<TProvider, TObject>([NotNull] TObject source, [NotNull] TestContext context, bool recordMode, string suffix = "") where TProvider : ISnapshotProvider<TObject>, new()
        {
            var provider = new TProvider();

            Snapshot(provider, source, context, recordMode, suffix);
        }

        /// <summary>
        /// Performs a snapshot text with a given test context/object pair, using a given instantiated snapshot provider.
        /// </summary>
        public static void Snapshot<T>([NotNull] ISnapshotProvider<T> provider, [NotNull] T target, [NotNull] TestContext context, bool recordMode, string suffix = "")
        {
            string targetPath = CombinedTestResultPath(TestResultsPath(), context);

            // Verify path exists
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            string testFileName;
            if (string.IsNullOrEmpty(suffix))
                testFileName = $"{context.TestName}.png";
            else
                testFileName = $"{context.TestName}-{suffix}.png";

            string testFilePath = Path.Combine(targetPath, testFileName);

            // Verify comparison file's existence (if not in record mode)
            if (!recordMode)
            {
                if(!File.Exists(testFilePath))
                    Assert.Fail($"Could not find reference image file {testFilePath} to compare. Please re-run the test with {nameof(recordMode)} set to true to record a test result to compare later.");
            }
            
            var image = provider.GenerateBitmap(target);

            if (recordMode)
            {
                image.Save(testFilePath, ImageFormat.Png);

                Assert.Fail(
                    $"Saved image to path {testFilePath}. Re-run test mode with {nameof(recordMode)} set to false to start comparing with record test result.");
            }
            else
            {
                // Load recorded image and compare
                using (var expected = (Bitmap)Image.FromFile(testFilePath))
                using (var expLock = expected.FastLock())
                using (var actLock = image.FastLock())
                {
                    bool areEqual = expLock.Width == actLock.Width && expLock.DataArray.SequenceEqual(actLock.DataArray);
                    
                    if (areEqual)
                        return; // Success!

                    // Save to test results directory for further inspection
                    string directoryName = CombinedTestResultPath(context.TestDir, context);
                    string baseFileName = Path.ChangeExtension(testFileName, null);

                    string savePathExpected = Path.Combine(directoryName, Path.ChangeExtension(baseFileName + "-expected", ".png"));
                    string savePathActual = Path.Combine(directoryName, Path.ChangeExtension(baseFileName + "-actual", ".png"));
                    string savePathDiff = Path.Combine(directoryName, Path.ChangeExtension(baseFileName + "-diff", ".png"));

                    // Ensure path exists
                    if (!Directory.Exists(directoryName))
                    {
                        Assert.IsNotNull(directoryName, "directoryName != null");
                        Directory.CreateDirectory(directoryName);
                    }

                    image.Save(savePathActual, ImageFormat.Png);
                    expected.Save(savePathExpected, ImageFormat.Png);

                    using (var diff = GenerateDiff(actLock, expLock))
                    {
                        diff.Save(savePathDiff, ImageFormat.Png);
                    }

                    context.AddResultFile(savePathActual);
                    context.AddResultFile(savePathExpected);
                    context.AddResultFile(savePathDiff);

                    Assert.Fail($"Resulted image did not match expected image. Inspect results under directory {directoryName} for info about results");
                }
            }
        }
        
        private static string CombinedTestResultPath([NotNull] string basePath, [NotNull] TestContext context)
        {
            if(!SeparateDirectoriesPerNamespace)
                return Path.Combine(basePath, context.FullyQualifiedTestClassName);

            var segments = context.FullyQualifiedTestClassName.Split('.');

            return Path.Combine(new[] {basePath}.Concat(segments).ToArray());
        }

        private static string TestResultsPath()
        {
            string path = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory);
            
            if(!path.EndsWith("bin\\Debug") && !path.EndsWith("bin\\Release"))
                Assert.Fail($"Invalid/unrecognized test assembly path {path}: Path must end in either bin\\Debug or bin\\Release");

            path = Path.GetFullPath(Path.Combine(path, "..\\..\\Snapshot\\Files"));

            return path;
        }

        private static Bitmap GenerateDiff([NotNull] FastBitmap bitmap1, [NotNull] FastBitmap bitmap2)
        {
            PixelF ColorAt(FastBitmap bitmap, int x, int y)
            {
                if (x >= bitmap.Width || y >= bitmap.Height)
                    return PixelF.White;

                return new PixelF(bitmap.GetPixelUInt(x, y));
            }

            var result = new Bitmap(Math.Max(bitmap1.Width, bitmap2.Width), Math.Max(bitmap1.Height, bitmap2.Height));

            using (var fastBitmap = result.FastLock())
            {
                for (int y = 0; y < result.Height; y++)
                {
                    for (int x = 0; x < result.Width; x++)
                    {
                        var baseColor = ColorAt(bitmap1, x, y);
                        var topColor = ColorAt(bitmap2, x, y);

                        // Basic idea:
                        // 1. Draw base bitmap
                        // 2. Compute a diff blend between the top bitmap and a fully white pixel
                        // 3. Draw computed diff with 50% alpha over the base bitmap
                        var finalPixel = baseColor;
                        finalPixel = PixelF.White.ColorBlendOver(topColor, PixelF.BlendDifference).WithAlpha(0.5f).ColorBlendOver(finalPixel);
                        
                        fastBitmap.SetPixel(x, y, finalPixel.ToColor());
                    }
                }
            }

            return result;
        }
        
        [DebuggerDisplay("A: {Alpha}, R: {Red}, G: {Green}, B: {Blue}")]
        [StructLayout(LayoutKind.Sequential)]
        private readonly struct PixelF
        {
            internal static readonly Func<float, float, float> BlendNormal = (p1, p2) => p2;
            internal static readonly Func<float, float, float> BlendDifference = (p1, p2) => Math.Abs(p1 - p2);

            internal static readonly PixelF White = new PixelF(1, 1, 1, 1);
            internal static readonly PixelF Black = new PixelF(1, 0, 0, 0);

            readonly float Alpha;
            readonly float Red;
            readonly float Green;
            readonly float Blue;

            public PixelF(uint color)
                : this(((color >> 24) & 0xFF) / 255.0f, ((color >> 16) & 0xFF) / 255.0f, ((color >> 8) & 0xFF) / 255.0f, (color & 0xFF) / 255.0f)
            {
                
            }

            public PixelF(float alpha, float red, float green, float blue)
            {
                Alpha = Clamp(alpha);
                Red = Clamp(red);
                Green = Clamp(green);
                Blue = Clamp(blue);
            }

            public static PixelF operator -(in PixelF value)
            {
                return new PixelF(-value.Alpha, -value.Red, -value.Green, -value.Blue);
            }

            public static PixelF operator +(in PixelF lhs, in PixelF rhs)
            {
                return new PixelF(lhs.Alpha + rhs.Alpha, lhs.Red + rhs.Red, lhs.Green + rhs.Green, lhs.Blue + rhs.Blue);
            }

            public static PixelF operator -(in PixelF lhs, in PixelF rhs)
            {
                return lhs + -rhs;
            }

            public static PixelF operator *(in PixelF lhs, in PixelF rhs)
            {
                return new PixelF(lhs.Alpha * rhs.Alpha, lhs.Red * rhs.Red, lhs.Green * rhs.Green, lhs.Blue * rhs.Blue);
            }

            public static PixelF operator /(in PixelF lhs, in PixelF rhs)
            {
                return new PixelF(lhs.Alpha / rhs.Alpha, lhs.Red / rhs.Red, lhs.Green / rhs.Green, lhs.Blue / rhs.Blue);
            }
            
            public static PixelF operator *(in PixelF lhs, in float rhs)
            {
                return new PixelF(lhs.Alpha * rhs, lhs.Red * rhs, lhs.Green * rhs, lhs.Blue * rhs);
            }

            public static PixelF operator /(in PixelF lhs, in float rhs)
            {
                return new PixelF(lhs.Alpha / rhs, lhs.Red / rhs, lhs.Green / rhs, lhs.Blue / rhs);
            }

            [Pure]
            public PixelF ColorBlendOver(in PixelF backdrop)
            {
                return ColorBlendOver(backdrop, BlendNormal);
            }

            [Pure]
            public PixelF ColorBlendOver(in PixelF backdrop, [NotNull] Func<float, float, float> blend)
            {
                var source = this;

                float sourceAlpha = source.Alpha;
                float backdropAlpha = backdrop.Alpha;
                float resultingAlpha = source.Alpha + backdrop.Alpha * (1 - source.Alpha);
                
                float ColorCompositingFormula(float @as, float ab, float ar, float cs, float cb) => 
                    (1 - @as / ar) * cb + @as / ar * ((1 - ab) * cs + ab * blend(cb, cs));

                float Composite(float cs, float cb) =>
                    ColorCompositingFormula(sourceAlpha, backdropAlpha, resultingAlpha, cs, cb);

                float alpha = resultingAlpha;
                float red = Composite(source.Red, backdrop.Red);
                float green = Composite(source.Green, backdrop.Green);
                float blue = Composite(source.Blue, backdrop.Blue);

                return new PixelF(alpha, red, green, blue);
            }

            [Pure]
            public PixelF WithAlpha(float alpha)
            {
                return new PixelF(alpha, Red, Green, Blue);
            }

            public uint ToColor()
            {
                uint ialpha = (uint) (Clamp(Alpha) * 255);
                uint ired = (uint) (Clamp(Red) * 255);
                uint igreen = (uint) (Clamp(Green) * 255);
                uint iblue = (uint) (Clamp(Blue) * 255);

                return (ialpha << 24) | (ired << 16) | (igreen << 8) | iblue;
            }

            private static float Clamp(float component)
            {
                return Math.Max(0, Math.Min(1, component));
            }
        }
    }

    /// <summary>
    /// Base interface for objects instantiated to provide bitmaps for snapshot tests
    /// </summary>
    /// <typeparam name="T">The type of object this snapshot provider receives in order to produce snapshots.</typeparam>
    public interface ISnapshotProvider<in T>
    {
        /// <summary>
        /// Asks this snapshot provider to create a <see cref="T:System.Drawing.Bitmap"/> from a given object context.
        /// </summary>
        [NotNull]
        Bitmap GenerateBitmap([NotNull] T context);
    }
}

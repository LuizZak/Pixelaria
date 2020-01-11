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
using System.Runtime.InteropServices;
using Blend2DCS.Geometry;
using Blend2DCS.Internal;
using JetBrains.Annotations;

namespace Blend2DCS
{
    public class BLContext : IDisposable
    {
        internal BLContextCore Context;

        public BLMatrix2D UserMatrix
        {
            get
            {
                var matrix = new BLMatrix2D();
                UnsafeContextCore.blContextGetUserMatrix(ref Context, ref matrix);
                return matrix;
            }
        }

        public BLContext()
        {
            Context = new BLContextCore();
            UnsafeContextCore.blContextInit(ref Context);
        }

        public BLContext([NotNull] BLImage image, BLContextCreateInfo createInfo = null)
        {
            Context = new BLContextCore();
            UnsafeContextCore.blContextInitAs(ref Context, ref image.Image, createInfo);
        }

        ~BLContext()
        {
            ReleaseUnmanagedResources();
        }

        private void ReleaseUnmanagedResources()
        {
            UnsafeContextCore.blContextReset(ref Context);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        public void SetMatrix(BLMatrix2D matrix)
        {
            UnsafeContextCore.blContextMatrixOp(ref Context, (uint) BLMatrix2DOp.Reset, IntPtr.Zero);
            UnsafeContextCore.blContextMatrixOp(ref Context, (uint) BLMatrix2DOp.Transform, ref matrix);
        }

        public void Flush()
        {
            UnsafeContextCore.blContextFlush(ref Context, 0);
        }

        public BLContextCookie Save()
        {
            var cookie = new BLContextCookie();
            UnsafeContextCore.blContextSave(ref Context, ref cookie);
            return cookie;
        }

        public void Restore(BLContextCookie? cookie = null)
        {
            if (cookie != null)
            {
                var local = cookie.Value;
                UnsafeContextCore.blContextRestore(ref Context, ref local);
            }
            else
            {
                UnsafeContextCore.blContextRestore(ref Context, IntPtr.Zero);
            }
        }

        public void SetPatternQuality(BLPatternQuality quality)
        {
            UnsafeContextCore.blContextSetHint(ref Context, BLContextHint.PatternQuality, (uint) quality);
        }

        public void SetGradientQuality(BLGradientQuality quality)
        {
            UnsafeContextCore.blContextSetHint(ref Context, BLContextHint.GradientQuality, (uint)quality);
        }

        public void SetRenderingQuality(BLRenderingQuality quality)
        {
            UnsafeContextCore.blContextSetHint(ref Context, BLContextHint.RenderingQuality, (uint)quality);
        }

        #region Stroke

        public void SetStrokeWidth(double strokeWidth)
        {
            UnsafeContextCore.blContextSetStrokeWidth(ref Context, strokeWidth);
        }

        public void SetStrokeStyle(uint color)
        {
            UnsafeContextCore.blContextSetStrokeStyleRgba32(ref Context, color);
        }

        public void SetStrokeStyle([NotNull] BLGradient gradient)
        {
            UnsafeContextCore.blContextSetStrokeStyle(ref Context, ref gradient.Gradient);
        }

        public void SetStrokeStyle([NotNull] BLPattern pattern)
        {
            UnsafeContextCore.blContextSetStrokeStyle(ref Context, ref pattern.Pattern);
        }

        public void StrokePath([NotNull] BLPath path)
        {
            UnsafeContextCore.blContextStrokePathD(ref Context, ref path.Path);
        }

        public void StrokeCircle(BLCircle circle)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.Circle, ref circle);
        }

        public void StrokeEllipse(BLEllipse ellipse)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.Ellipse, ref ellipse);
        }

        public void StrokeRectangle(BLRect rect)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.RectD, ref rect);
        }

        public void StrokeRoundRectangle(BLRoundRect rect)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.RoundRect, ref rect);
        }

        #endregion

        #region Fill

        public void SetFillStyle(uint color)
        {
            UnsafeContextCore.blContextSetFillStyleRgba32(ref Context, color);
        }

        public void SetFillStyle([NotNull] BLGradient gradient)
        {
            UnsafeContextCore.blContextSetFillStyle(ref Context, ref gradient.Gradient);
        }

        public void SetFillStyle([NotNull] BLPattern pattern)
        {
            UnsafeContextCore.blContextSetFillStyle(ref Context, ref pattern.Pattern);
        }

        public void FillPath([NotNull] BLPath path)
        {
            UnsafeContextCore.blContextFillPathD(ref Context, ref path.Path);
        }

        public void FillCircle(BLCircle circle)
        {
            UnsafeContextCore.blContextFillGeometry(ref Context, BLGeometryType.Circle, ref circle);
        }

        public void FillEllipse(BLEllipse ellipse)
        {
            UnsafeContextCore.blContextFillGeometry(ref Context, BLGeometryType.Ellipse, ref ellipse);
        }

        public void FillRectangle(BLRect rect)
        {
            UnsafeContextCore.blContextFillGeometry(ref Context, BLGeometryType.RectD, ref rect);
        }

        public void FillRoundRectangle(BLRoundRect rect)
        {
            UnsafeContextCore.blContextFillGeometry(ref Context, BLGeometryType.RoundRect, ref rect);
        }

        #endregion

        #region Clipping

        public void ClipToRect(BLRect rect)
        {
            UnsafeContextCore.blContextClipToRectD(ref Context, ref rect);
        }

        public void RestoreClipping()
        {
            UnsafeContextCore.blContextRestoreClipping(ref Context);
        }

        #endregion

        #region Text
        
        public void FillText(BLPoint pt, [NotNull] BLFont font, [NotNull] string text)
        {
            pt.Y += font.GetMetrics().Ascent;

            NativeStringHelper.WithNullTerminatedUtf8String(text, (ptr, size) =>
            {
                UnsafeContextCore.blContextFillTextD(ref Context, ref pt, ref font.Font, ptr, size, BLTextEncoding.UTF8);
            });
        }

        #endregion

        #region Image

        public void BlitImage([NotNull] BLImage image, BLRectI imageArea, BLPoint point)
        {
            UnsafeContextCore.blContextBlitImageD(ref Context, ref point, ref image.Image, ref imageArea);
        }

        public void BlitImage([NotNull] BLImage image, BLRectI imageArea, BLRect area)
        {
            UnsafeContextCore.blContextBlitScaledImageD(ref Context, ref area, ref image.Image, ref imageArea);
        }

        #endregion

        #region State

        #endregion
    }

    /// <summary>
    /// Information that can be used to customize the rendering context.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BLContextCreateInfo
    {
        /// <summary>
        /// Create flags, see <see cref="BLContextCreateFlags"/>.
        /// </summary>
        public BLContextCreateFlags Flags;

        /// <summary>
        /// Number of threads to acquire from thread-pool and use for rendering.
        ///
        /// If <see cref="ThreadCount"/> is zero it means to initialize the context for synchronous
        /// rendering. This means that every operation will take effect immediately.
        /// If the number is <c>1</c> or greater it means to initialize the context for
        /// asynchronous rendering - in this case <see cref="ThreadCount"/> specifies how many
        /// threads can execute in parallel.
        /// </summary>
        public uint ThreadCount;

        /// <summary>
        /// CPU features to use in isolated JIT runtime (if supported), only used
        /// when <see cref="Flags"/> contains <see cref="BLContextCreateFlags.OverrideCpuFeatures"/>.
        /// </summary>
        public uint CpuFeatures;

        //! Reserved for future use, must be zero.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] Reserved;
    }

    /// <summary>
    /// Rendering context create-flags.
    /// </summary>
    public enum BLContextCreateFlags: uint
    {
        /// <summary>
        /// When creating an asynchronous rendering context that uses threads for
        /// rendering, the rendering context can sometimes allocate less threads
        /// than specified if the built-in thread-pool doesn't have enough threads 
        /// available. This flag will force the thread-pool to override the thread 
        /// limit temporarily to fulfill the thread count requirement.
        /// </summary>
        /// <remarks>This flag is ignored if <c>BLContextCreateInfo.ThreadCount == 0</c> </remarks>
        ForceThreads = 0x00000001u,

        /// <summary>
        /// Fallback to synchronous rendering in case that acquiring threads from
        /// thread-pool failed. This flag only makes sense when asynchronous mode
        /// was specified by having non-zero thread count. In that case if the
        /// rendering context fails to acquire at least one thread it would fallback
        /// to synchronous mode instead.
        /// </summary>
        /// <remarks>This flag is ignored if <c>BLContextCreateInfo.ThreadCount == 0</c></remarks>
        FallbackToSync = 0x00000002u,

        /// <summary>
        /// If this flag is specified and asynchronous rendering is enabled then
        /// the context would create its own isolated thread-pool, which is useful
        /// for debugging purposes.
        ///
        /// Do not use this flag in production as rendering contexts with isolated
        /// thread-pool have to create and destroy all threads they use. This flag
        /// is only useful for testing, debugging, and isolated benchmarking.
        /// </summary>
        IsolatedThreads = 0x00000010u,

        /// <summary>
        /// If this flag is specified and JIT pipeline generation enabled then the
        /// rendering context would create its own isolated JIT runtime. which is
        /// useful for debugging purposes. This flag will be ignored if JIT pipeline
        /// generation is either not supported or was disabled by other flags.
        ///
        /// Do not use this flag in production as rendering contexts with isolated
        /// JIT runtime do not use global pipeline cache, that's it, after the
        /// rendering context is destroyed the JIT runtime is destroyed with it with
        /// all compiled pipelines. This flag is only useful for testing, debugging,
        /// and isolated benchmarking.
        /// </summary>
        IsolatedJit = 0x00000020u,

        /// <summary>
        /// Override CPU features when creating isolated context.
        /// </summary>
        OverrideCpuFeatures = 0x00000040u
    }

    /// <summary>
    /// Rendering context hint.
    /// </summary>
    public enum BLContextHint : uint
    {
        /// <summary>
        /// Rendering quality.
        /// </summary>
        RenderingQuality = 0,
        /// <summary>
        /// Gradient quality.
        /// </summary>
        GradientQuality = 1,
        /// <summary>
        /// Pattern quality.
        /// </summary>
        PatternQuality = 2
    }

    /// <summary>
    /// Gradient rendering quality.
    /// </summary>
    public enum BLGradientQuality : uint
    {
        /// <summary>
        /// Nearest neighbor.
        /// </summary>
        Nearest = 0
    }

    /// <summary>
    /// Pattern quality.
    /// </summary>
    public enum BLPatternQuality : uint
    {
        /// <summary>
        /// Nearest neighbor.
        /// </summary>
        Nearest = 0,
        /// <summary>
        /// Bilinear.
        /// </summary>
        Bilinear = 1
    }

    /// <summary>
    /// Rendering quality.
    /// </summary>
    public enum BLRenderingQuality : uint
    {
        /// <summary>
        /// Render using anti-aliasing.
        /// </summary>
        Antialias = 0
    }
}

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

namespace Blend2DCS
{
    public class BLContext
    {
        internal BLContextCore Context;

        public BLMatrix UserMatrix
        {
            get
            {
                var matrix = new BLMatrix();
                UnsafeContextCore.blContextGetUserMatrix(ref Context, ref matrix);
                return matrix;
            }
        }

        public BLContext()
        {
            Context = new BLContextCore();
            UnsafeContextCore.blContextInit(ref Context);
        }

        public BLContext(BLImage image, BLContextCreateInfo createInfo = null)
        {
            Context = new BLContextCore();
            UnsafeContextCore.blContextInitAs(ref Context, ref image.Image, createInfo);
        }

        ~BLContext()
        {
            UnsafeContextCore.blContextReset(ref Context);
        }

        public void SetMatrix(BLMatrix matrix)
        {
            UnsafeContextCore.blContextMatrixOp(ref Context, (uint) BLMatrix2DOp.Reset, IntPtr.Zero);
            UnsafeContextCore.blContextMatrixOp(ref Context, (uint) BLMatrix2DOp.Transform, ref matrix);
        }

        public void SetStrokeWidth(double strokeWidth)
        {
            UnsafeContextCore.blContextSetStrokeWidth(ref Context, strokeWidth);
        }

        public void StrokePath(BLPath path)
        {
            UnsafeContextCore.blContextStrokePathD(ref Context, ref path.Path);
        }

        public void StrokeCircle(BLCircle circle)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.Circle, ref circle);
        }

        public void StrokeEllipse(BLEllipse ellipse)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.Circle, ref ellipse);
        }

        public void StrokeRectangle(BLRect rect)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.RectD, ref rect);
        }

        public void StrokeRoundRectangle(BLRoundRect rect)
        {
            UnsafeContextCore.blContextStrokeGeometry(ref Context, BLGeometryType.RoundRect, ref rect);
        }
    }

    /// <summary>
    /// Information that can be used to customize the rendering context.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class BLContextCreateInfo
    {
        /// <summary>
        /// Create flags, see `BLContextCreateFlags`.
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
}

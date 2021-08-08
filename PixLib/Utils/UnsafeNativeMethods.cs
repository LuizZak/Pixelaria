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
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace PixLib.Utils
{
    /// <summary>
    /// Contains P/Invoke references to native C methods
    /// </summary>
    internal static class UnsafeNativeMethods
    {
        [DllImport("comctl32.dll")]
        internal static extern bool InitCommonControls();

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ImageList_BeginDrag(
            IntPtr himlTrack, // Handler of the image list containing the image to drag
            int iTrack,       // Index of the image to drag 
            int dxHotspot,    // x-delta between mouse position and drag image
            int dyHotspot     // y-delta between mouse position and drag image
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ImageList_DragMove(
            int x,            // X-coordinate (relative to the form, not the treeview) at which to display the drag image.
            int y             // Y-coordinate (relative to the form, not the treeview) at which to display the drag image.
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        internal static extern void ImageList_EndDrag();

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ImageList_DragEnter(
            IntPtr hwndLock,  // Handle to the control that owns the drag image.
            int x,            // X-coordinate (relative to the treeview) at which to display the drag image. 
            int y             // Y-coordinate (relative to the treeview) at which to display the drag image. 
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ImageList_DragLeave(
            IntPtr hwndLock  // Handle to the control that owns the drag image.
        );

        [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
        internal static extern bool ImageList_DragShowNolock(
            bool fShow       // False to hide, true to show the image
        );

        /// <summary>
        /// Compares two memory sections and returns 0 if the memory segments are identical
        /// </summary>
        /// <param name="b1">The pointer to the first memory segment</param>
        /// <param name="b2">The pointer to the second memory segment</param>
        /// <param name="count">The number of bytes to compare</param>
        /// <returns>0 if the memory segments are identical</returns>
        [Pure]
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int memcmp(IntPtr b1, IntPtr b2, long count);
        
        /// <summary>
        /// Compares two arrays of bytes and returns 0 if they are memory identical
        /// </summary>
        /// <param name="b1">The first array of bytes</param>
        /// <param name="b2">The second array of bytes</param>
        /// <param name="count">The number of bytes to compare</param>
        /// <returns>0 if the byte arrays are identical</returns>
        [Pure]
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int memcmp(byte[] b1, byte[] b2, long count);

        [DllImport("user32.dll")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
    }
}
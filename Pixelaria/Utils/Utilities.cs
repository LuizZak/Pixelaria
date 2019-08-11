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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using JetBrains.Annotations;

using Pixelaria.Data;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Contains static utility methods
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Adds a disposable object into a collection of disposable (usually a CompositeDisposable)
        /// </summary>
        public static void AddToDisposable<T>(this IDisposable disposable, [NotNull] T target) where T : ICollection<IDisposable>, IDisposable
        {
            target.Add(disposable);
        }

        /// <summary>
        /// Helper method used to create relative paths
        /// </summary>
        /// <param name="filespec">The file path</param>
        /// <param name="folder">The base folder to create the relative path</param>
        /// <returns>A relative path between folder and filespec</returns>
        [Pure]
        public static string GetRelativePath([NotNull] string filespec, string folder)
        {
            var pathUri = new Uri(filespec);
            
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            return Uri.UnescapeDataString(new Uri(folder).MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Returns the given uint value snapped to the next highest power of two value
        /// </summary>
        /// <param name="value">The value to snap to the closest power of two value</param>
        /// <returns>The given uint value snapped to the next highest power of two value</returns>
        [Pure]
        public static uint SnapToNextPowerOfTwo(uint value)
        {
            value--;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            value++;

            return value;
        }

        /// <summary>
        /// Returns a formated sting that contains the most significant magnitude
        /// representation of the given number of bytes
        /// </summary>
        /// <param name="bytes">The number of bytes</param>
        /// <returns>A formated string with the byte count converted to the most significant magnitude</returns>
        [Pure]
        public static string FormatByteSize(long bytes)
        {
            int magnitude = 0;
            string[] sulfixes = { "b", "kb", "mb", "gb", "tb", "pt", "eb", "zb", "yb" };

            float b = bytes;

            while (b > 1024)
            {
                if (magnitude == sulfixes.Length - 1)
                    break;

                b /= 1024;
                magnitude++;
            }

            return Math.Round(b * 100) / 100 + sulfixes[magnitude];
        }

        /// <summary>
        /// Compares two arrays of bytes and returns true if they are identical
        /// </summary>
        /// <param name="b1">The first array of bytes</param>
        /// <param name="b2">The second array of bytes</param>
        /// <returns>True if the byte arrays are identical</returns>
        [Pure]
        public static bool ByteArrayCompare([NotNull] byte[] b1, [NotNull] byte[] b2)
        {
            // Validate buffers are the same length.
            if (b1.Length != b2.Length)
                return false;

            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Transforms a given list of Frames into a list of Bitmaps.
        /// The list of bitmaps will be equivalent to taking the Frame.GetComposedBitmap() of each frame
        /// </summary>
        /// <param name="frameList">A list of frames to transform into bitmaps</param>
        /// <returns>The given list of frames, turned into a list of bitmaps</returns>
        public static List<Bitmap> ToBitmapList(this List<IFrame> frameList)
        {
            return new List<Bitmap>(
                                from frame in frameList
                                select frame.GetComposedBitmap()
                                );
        }

        /// <summary>
        /// Transforms a given array of Frames into a array of Bitmaps.
        /// The array of bitmaps will be equivalent to taking the Frame.GetComposedBitmap() of each frame
        /// </summary>
        /// <param name="frames">An array of frames to transform into bitmaps</param>
        /// <returns>The given array of frames, turned into a array of bitmaps</returns>
        public static Bitmap[] ToBitmapArray(this IFrame[] frames)
        {
            return (from frame in frames
                    select frame.GetComposedBitmap()
                    ).ToArray();
        }

        /// <summary>
        /// Finds the control that is currently focused under the given control.
        /// If no other control is focused, the passed control is returned
        /// </summary>
        /// <param name="control">The control to start searching under</param>
        /// <returns>The control that is currently focused under the specified control</returns>
        public static Control FindFocusedControl(Control control)
        {
            var container = control as IContainerControl;
            while (container != null)
            {
                control = container.ActiveControl;
                container = control as IContainerControl;
            }
            return control;
        }

        /// <summary>
        /// Returns the smallest Rectangle object that encloses all points provided
        /// </summary>
        /// <param name="pointList">An array of points to convert</param>
        /// <returns>The smallest Rectangle object that encloses all points provided</returns>
        [Pure]
        public static Rectangle GetRectangleArea([NotNull] Point[] pointList)
        {
            int minX = pointList[0].X;
            int minY = pointList[0].Y;

            int maxX = pointList[0].X;
            int maxY = pointList[0].Y;

            foreach (var p in pointList)
            {
                minX = Math.Min(p.X, minX);
                minY = Math.Min(p.Y, minY);

                maxX = Math.Max(p.X, maxX);
                maxY = Math.Max(p.Y, maxY);
            }

            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Returns the smallest Rectangle object that encloses all points provided
        /// </summary>
        /// <param name="pointList">An array of points to convert</param>
        /// <returns>The smallest Rectangle object that encloses all points provided</returns>
        [Pure]
        public static RectangleF GetRectangleArea([NotNull] IReadOnlyList<PointF> pointList)
        {
            float minX = pointList[0].X;
            float minY = pointList[0].Y;

            float maxX = pointList[0].X;
            float maxY = pointList[0].Y;

            foreach (var p in pointList)
            {
                minX = Math.Min(p.X, minX);
                minY = Math.Min(p.Y, minY);

                maxX = Math.Max(p.X, maxX);
                maxY = Math.Max(p.Y, maxY);
            }

            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Converts a PascalCase string into a Title Case sentence string.
        /// 
        /// E.g.:
        /// 
        /// SampleCase          -> Sample Case
        /// PerformHTTPRequest  -> Perform HTTP Request
        /// AnotherSample       -> Another Sample
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        [NotNull]
        public static string DePascalCase([NotNull] string name)
        {
            string replace = Regex.Replace(name, "(.)([A-Z][a-z]+)", "$1 $2");
            replace = Regex.Replace(replace, "([a-z0-9])([A-Z])", "$1 $2");

            return replace;
        }
    }
}
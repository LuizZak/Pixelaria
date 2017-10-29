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
        /// Converts a Color instance into an AHSL color
        /// </summary>
        /// <param name="color">The Color to convert to AHSL</param>
        /// <returns>An AHSL (alpha hue saturation and lightness) color</returns>
        [Pure]
        public static AhslColor ToAhsl(this Color color)
        {
            return AhslColor.FromArgb(color.ToArgb());
        }

        /// <summary>
        /// Returns a Color instance with a new transparency value (and all other components kept the same).
        /// </summary>
        [Pure]
        public static Color WithTransparency(this Color color, float alpha)
        {
            return Color.FromArgb((int) (alpha * 255), color.R, color.G, color.B);
        }

        /// <summary>
        /// Gets the hue-saturation-lightness (HSL) lightness value for this System.Drawing.Color structure.
        /// </summary>
        /// <param name="color">The Color to convert to get the lightness component from</param>
        /// <returns>The lightness of this System.Drawing.Color. The lightness ranges from 0.0 through 1.0, where 0.0 is black and 1.0 is white.</returns>
        [Pure]
        public static float GetLightness(this Color color)
        {
            return color.ToAhsl().Lightness / 100.0f;
        }

        /// <summary>
        /// Gets an inverted version of this Color object. Note the Alpha channel is left intact in the inversion process
        /// </summary>
        /// <param name="color">The color to invert</param>
        /// <returns>An inverted version of this Color object</returns>
        [Pure]
        public static Color Invert(this Color color)
        {
            const int rgbmax = 255;
            return Color.FromArgb(color.A, rgbmax - color.R, rgbmax - color.G, rgbmax - color.B);
        }

        /// <summary>
        /// Fades the first color with the second, using the given factor to decide
        /// how much of each color will be used. The alpha channel is optionally changed.
        /// 
        /// Unlike <see cref="Blend"/>, this method allows specifying whether the alpha channel is to be mixed in as well.
        /// </summary>
        /// <param name="color">The color to fade</param>
        /// <param name="fadeColor">The color to fade the first color to</param>
        /// <param name="factor">A number from [0 - 1] that decides how much the first color will fade into the second</param>
        /// <param name="blendAlpha">Whether to fade the alpha channel as well. If left false, the first color's alpha channel will be used</param>
        /// <returns>The faded color</returns>
        [Pure]
        public static Color Fade(this Color color, Color fadeColor, float factor = 0.5f, bool blendAlpha = false)
        {
            float from = 1 - factor;

            int a = (int)(blendAlpha ? color.A * from + fadeColor.A * factor : color.A);
            int r = (int)(color.R * from + fadeColor.R * factor);
            int g = (int)(color.G * from + fadeColor.G * factor);
            int b = (int)(color.B * from + fadeColor.B * factor);
	        
	        return Color.FromArgb(Math.Abs(a), Math.Abs(r), Math.Abs(g), Math.Abs(b));
        }

        /// <summary>
        /// Blends the specified colors together
        /// </summary>
        /// <param name="color">Color to blend onto the background color.</param>
        /// <param name="backColor">Color to blend the other color onto.</param>
        /// <param name="factor">The factor to blend the two colors on. 0.0 will return the first color, 1.0 will return the back color, any values in between will blend the two colors accordingly</param>
        /// <returns>The blended color</returns>
        [Pure]
        public static Color Blend(this Color color, Color backColor, float factor = 0.5f)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (factor == 1 || color.A == 0)
                return backColor;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (factor == 0 || backColor.A == 0)
                return color;
            if (color.A == 255)
                return color;
            
            int alpha = color.A + 1;

            int b = alpha * color.B + (255 - alpha) * backColor.B >> 8;
            int g = alpha * color.G + (255 - alpha) * backColor.G >> 8;
            int r = alpha * color.R + (255 - alpha) * backColor.R >> 8;

            float alphaFg = (float)color.A / 255;
            float alphaBg = (float)backColor.A / 255;

            int a = (int)((alphaBg + alphaFg - alphaBg * alphaFg) * 255);

            if (backColor.A == 255)
            {
                a = 255;
            }
            if (a > 255)
            {
                a = 255;
            }
            if (r > 255)
            {
                r = 255;
            }
            if (g > 255)
            {
                g = 255;
            }
            if (b > 255)
            {
                b = 255;
            }

            return Color.FromArgb(Math.Abs(a), Math.Abs(r), Math.Abs(g), Math.Abs(b));
        }

        /// <summary>
        /// Flattens two colors using a GDI+ like color blending mode
        /// </summary>
        /// <param name="backColor">The back color to blend</param>
        /// <param name="foreColor">The fore color to blend</param>
        /// <returns>The two colors, blended with a GDI+ like color bleding mode</returns>
        [Pure]
        public static Color FlattenColor(Color backColor, Color foreColor)
        {
            // Based off an answer by an anonymous user on StackOverlow http://stackoverflow.com/questions/1718825/blend-formula-for-gdi/2223241#2223241
            byte foreA = foreColor.A;

            if (foreA == 0)
                return backColor;
            if (foreA == 255)
                return foreColor;

            float backAlphaFloat = backColor.A;
            float foreAlphaFloat = foreA;

            float foreAlphaNormalized = foreAlphaFloat / 255;
            float backColorMultiplier = backAlphaFloat * (1 - foreAlphaNormalized);

            float alpha = backAlphaFloat + foreAlphaFloat - backAlphaFloat * foreAlphaNormalized;

            return Color.FromArgb((int)Math.Min(255, alpha), (int)(Math.Min(255, (foreColor.R * foreAlphaFloat + backColor.R * backColorMultiplier) / alpha)), (int)(Math.Min(255, (foreColor.G * foreAlphaFloat + backColor.G * backColorMultiplier) / alpha)), (int)(Math.Min(255, (foreColor.B * foreAlphaFloat + backColor.B * backColorMultiplier) / alpha)));
        }

        /// <summary>
        /// Flattens two colors using a GDI+ like color blending mode
        /// </summary>
        /// <param name="backColor">The back color to blend</param>
        /// <param name="foreColor">The fore color to blend</param>
        /// <returns>The two colors, blended with a GDI+ like color bleding mode</returns>
        [Pure]
        public static uint FlattenColor(uint backColor, uint foreColor)
        {
            // Based off an answer by an anonymous user on StackOverlow http://stackoverflow.com/questions/1718825/blend-formula-for-gdi/2223241#2223241
            
            byte foreA = (byte)((foreColor >> 24) & 0xFF);
            
            if (foreA == 0)
                return backColor;
            if (foreA == 255)
                return foreColor;

            byte foreR = (byte)((foreColor >> 16) & 0xFF);
            byte foreG = (byte)((foreColor >> 8) & 0xFF);
            byte foreB = (byte)(foreColor & 0xFF);

            byte backA = (byte)((backColor >> 24) & 0xFF);
            byte backR = (byte)((backColor >> 16) & 0xFF);
            byte backG = (byte)((backColor >> 8) & 0xFF);
            byte backB = (byte)(backColor & 0xFF);

            float backAlphaFloat = backA;
            float foreAlphaFloat = foreA;

            float foreAlphaNormalized = foreAlphaFloat / 255;
            float backColorMultiplier = backAlphaFloat * (1 - foreAlphaNormalized);

            float alpha = backAlphaFloat + foreAlphaFloat - backAlphaFloat * foreAlphaNormalized;

            uint finalA = (uint)Math.Min(255, alpha);
            uint finalR = (uint)Math.Min(255, (foreR * foreAlphaFloat + backR * backColorMultiplier) / alpha);
            uint finalG = (uint)Math.Min(255, (foreG * foreAlphaFloat + backG * backColorMultiplier) / alpha);
            uint finalB = (uint)Math.Min(255, (foreB * foreAlphaFloat + backB * backColorMultiplier) / alpha);

            return finalA << 24 | finalR << 16 | finalG << 8 | finalB;
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
    }
}
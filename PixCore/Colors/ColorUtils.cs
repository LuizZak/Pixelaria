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
using JetBrains.Annotations;

namespace PixCore.Colors
{
    /// <summary>
    /// Global utilities related to color manipulation
    /// </summary>
    public static class ColorUtils
    {
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
            return Color.FromArgb((int)(alpha * 255), color.R, color.G, color.B);
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
            const int rgbMax = 255;
            return Color.FromArgb(color.A, rgbMax - color.R, rgbMax - color.G, rgbMax - color.B);
        }

        /// <summary>
        /// Fades the first color with the second, using the given factor to decide
        /// how much of each color will be used. The alpha channel is optionally changed.
        /// 
        /// Unlike <see cref="BlendedOver"/>, this method allows specifying whether the alpha channel is to be mixed in as well.
        /// </summary>
        /// <param name="color">The color to fade</param>
        /// <param name="fadeColor">The color to fade the first color to</param>
        /// <param name="factor">A number from [0 - 1] that decides how much the first color will fade into the second</param>
        /// <param name="blendAlpha">Whether to fade the alpha channel as well. If left false, the first color's alpha channel will be used</param>
        /// <returns>The faded color</returns>
        [Pure]
        public static Color Faded(this Color color, Color fadeColor, float factor = 0.5f, bool blendAlpha = false)
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
        public static Color BlendedOver(this Color color, Color backColor, float factor = 0.5f)
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
    }
}

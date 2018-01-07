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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using PixCore.Controls.ColorControls;

namespace PixCore.Colors
{
    /// <inheritdoc cref="IEquatable{T}"/>
    /// <summary>
    /// Represents an HSL color with an alpha channel
    /// </summary>
    [Editor(typeof(AhslColorEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(AhslColorTypeConverter))]
    public readonly struct AhslColor : IEquatable<AhslColor>
    {
        /// <summary>
        /// Gets or sets the alpha component as a value ranging from 0 - 255
        /// </summary>
        public int Alpha => (int) (FloatAlpha * 255.0f);

        /// <summary>
        /// Gets or sets the hue component as a value ranging from 0 - 360
        /// </summary>
        public int Hue => (int) (FloatHue * 360.0f);

        /// <summary>
        /// Gets or sets the saturation component as a value ranging from 0 - 100
        /// </summary>
        public int Saturation => (int) (FloatSaturation * 100.0f);

        /// <summary>
        /// Gets or sets the lightness component as a value ranging from 0 - 100
        /// </summary>
        public int Lightness => (int) (FloatLightness * 100.0f);

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public int Red => ToColor().R;

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public int Green => ToColor().G;

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public int Blue => ToColor().B;

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float FloatRed => FloatArgbFromAhsl(FloatHue, FloatSaturation, FloatLightness, FloatAlpha)[1];

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float FloatGreen => FloatArgbFromAhsl(FloatHue, FloatSaturation, FloatLightness, FloatAlpha)[2];

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float FloatBlue => FloatArgbFromAhsl(FloatHue, FloatSaturation, FloatLightness, FloatAlpha)[3];

        /// <summary>
        /// Gets or sets the alpha component as a value ranging from 0 - 1
        /// </summary>
        public float FloatAlpha { get; }

        /// <summary>
        /// Gets or sets the hue component as a value ranging from 0 - 1
        /// </summary>
        public float FloatHue { get; }

        /// <summary>
        /// Gets or sets the saturation component as a value ranging from 0 - 1
        /// </summary>
        public float FloatSaturation { get; }

        /// <summary>
        /// Gets or sets the lightness component as a value ranging from 0 - 1
        /// </summary>
        public float FloatLightness { get; }

        /// <inheritdoc />
        /// <summary>
        /// Creates a new AHSL color
        /// </summary>
        /// <param name="alpha">The Alpha component, ranging from 0-255</param>
        /// <param name="hue">The Hue component, ranging from 0-360</param>
        /// <param name="saturation">The Saturation component, ranging from 0-100</param>
        /// <param name="lightness">The Lightness component, ranging from 0-100</param>
        public AhslColor(int alpha, int hue, int saturation, int lightness)
            : this(alpha / 255.0f, hue / 360.0f, saturation / 100.0f, lightness / 100.0f) { }

        /// <summary>
        /// Creates a new AHSL color
        /// </summary>
        /// <param name="alpha">The Alpha component, ranging from 0-1</param>
        /// <param name="hue">The Hue component, ranging from 0-1</param>
        /// <param name="saturation">The Saturation component, ranging from 0-1</param>
        /// <param name="lightness">The Lightness component, ranging from 0-1</param>
        public AhslColor(float alpha, float hue, float saturation, float lightness)
        {
            FloatAlpha = Math.Max(0, Math.Min(1, alpha));
            FloatHue = Math.Max(0, Math.Min(1, hue));
            FloatSaturation = Math.Max(0, Math.Min(1, saturation));
            FloatLightness = Math.Max(0, Math.Min(1, lightness));
        }

        /// <summary>
        /// Tests whether two AHSL color structures are different
        /// </summary>
        /// <param name="color1">The first AHSL color to test</param>
        /// <param name="color2">The second AHSL color to test</param>
        /// <returns>Whether two AHSL color structures are different</returns>
        public static bool operator !=(AhslColor color1, AhslColor color2)
        {
            return !(color1 == color2);
        }

        /// <summary>
        /// Tests whether two AHSL color structures are the same
        /// </summary>
        /// <param name="color1">The first AHSL color to test</param>
        /// <param name="color2">The second AHSL color to test</param>
        /// <returns>Whether two AHSL color structures are the same</returns>
        public static bool operator==(AhslColor color1, AhslColor color2)
        {
            return Math.Abs(color1.FloatAlpha - color2.FloatAlpha) < float.Epsilon &&
                   Math.Abs(color1.FloatHue - color2.FloatHue) < float.Epsilon &&
                   Math.Abs(color1.FloatSaturation - color2.FloatSaturation) < float.Epsilon &&
                   Math.Abs(color1.FloatLightness - color2.FloatLightness) < float.Epsilon;
        }

        public static explicit operator AhslColor(Color source)
        {
            return source.ToAhsl();
        }
        public static explicit operator Color(AhslColor source)
        {
            return source.ToColor();
        }

        /// <summary>
        /// Converts this AHSL color to a Color object
        /// </summary>
        /// <returns>The Color object that represents this AHSL color</returns>
        [Pure]
        public Color ToColor()
        {
            return Color.FromArgb(ToArgb());
        }

        /// <summary>
        /// Converts this AHSL color to a ARGB color
        /// </summary>
        /// <param name="revertByteOrder">Whether to revert the byte order so the alpha component is the most significant and the blue component the least</param>
        /// <returns>The ARGB color that represents this AHSL color</returns>
        [Pure]
        public int ToArgb(bool revertByteOrder = false)
        {
            return ArgbFromAhsl(FloatHue, FloatSaturation, FloatLightness, FloatAlpha, revertByteOrder);
        }

        /// <summary>
        /// Returns a copy of this Ahsl color with the Alpha transparency set to a given value
        /// </summary>
        [Pure]
        public AhslColor WithTransparency(float alpha)
        {
            return new AhslColor(alpha, FloatHue, FloatSaturation, FloatLightness);
        }

        public override string ToString()
        {
            return $"A: {Alpha}, H: {Hue}, S: {Saturation}, L: {Lightness}";
        }

        /// <summary>
        /// Returns whether this AHSL color object equals another AHSL color
        /// </summary>
        /// <param name="other">The other color to test</param>
        /// <returns>Whether this AHSL color object equals another AHSL color</returns>
        public bool Equals(AhslColor other)
        {
            return FloatAlpha.Equals(other.FloatAlpha) && FloatHue.Equals(other.FloatHue) &&
                   FloatSaturation.Equals(other.FloatSaturation) && FloatLightness.Equals(other.FloatLightness);
        }

        // Override Equals
        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            return obj is AhslColor color && Equals(color);
        }

        // Overrided GetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = FloatAlpha.GetHashCode();
                hashCode = (hashCode * 397) ^ FloatHue.GetHashCode();
                hashCode = (hashCode * 397) ^ FloatSaturation.GetHashCode();
                hashCode = (hashCode * 397) ^ FloatLightness.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Creates an AHSL object from the given AHSL color
        /// </summary>
        /// <param name="a">The Alpha component, ranging from 0-255</param>
        /// <param name="h">The Hue component, ranging from 0-360</param>
        /// <param name="s">The Saturation component, ranging from 0-100</param>
        /// <param name="l">The Lightness component, ranging from 0-100</param>
        /// <returns>The AHSL color representing the given AHSL value</returns>
        [Pure]
        public static AhslColor FromAhsl(int a, int h, int s, int l)
        {
            return new AhslColor(a, h, s, l);
        }

        /// <summary>
        /// Creates an AHSL object from the given ARGB color
        /// </summary>
        /// <param name="a">The Alpha component</param>
        /// <param name="r">The Red component</param>
        /// <param name="g">The Green component</param>
        /// <param name="b">The Blue component</param>
        /// <returns>The AHSL color representing the given ARGB value</returns>
        [Pure]
        public static AhslColor FromArgb(int a, int r, int g, int b)
        {
            return FromArgb((a << 24) | (r << 16) | (g << 8) | b);
        }
        
        /// <summary>
        /// Creates an AHSL object from the given ARGB color
        /// </summary>
        /// <param name="argb">The ARGB color to convert to AHSL</param>
        /// <returns>The AHSL color representing the given ARGB value</returns>
        [Pure]
        public static AhslColor FromArgb(int argb)
        {
            float a = (int)((uint)argb >> 24);
            float r = (argb >> 16) & 0xFF;
            float g = (argb >> 8) & 0xFF;
            float b = argb & 0xFF;

            a /= 255;
            r /= 255;
            g /= 255;
            b /= 255;

            return FromArgb(a, r, g, b);
        }
        
        /// <summary>
        /// Converts the given ARGB color to an AHSL color
        /// </summary>
        /// <param name="a">The alpha component</param>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        /// <returns>An AHSL (alpha hue saturation and lightness) color</returns>
        [Pure]
        public static AhslColor FromArgb(float a, float r, float g, float b)
        {
            // ReSharper disable once InconsistentNaming
            float M = b;
            float m = b;

            if (m > g)
                m = g;
            if (m > r)
                m = r;

            if (M < g)
                M = g;
            if (M < r)
                M = r;

            float d = M - m;

            float h;
            float s;
            
            if (Math.Abs(d) < float.Epsilon)
            {
                h = 0;
            }
            else if (Math.Abs(M - r) < float.Epsilon)
            {
                h = (g - b) / d % 6 * 60;
            }
            else if (Math.Abs(M - g) < float.Epsilon)
            {
                h = ((b - r) / d + 2) * 60;
            }
            else
            {
                h = ((r - g) / d + 4) * 60;
            }

            if (h < 0)
            {
                h += 360;
            }

            float l = (M + m) / 2;

            if (Math.Abs(d) < float.Epsilon)
            {
                s = 0;
            }
            else
            {
                s = d / (1 - Math.Abs(2 * l - 1));
            }
            
            return new AhslColor(a, h / 360, s, l);
        }

        /// <summary>
        /// Linearly interpolates between two colors with a given factor.
        /// 
        /// Interpolation happens across ARGB space (which is more precise and doesn't interpolate colors between greenish
        /// shared in between).
        /// </summary>
        /// <param name="start">Starting color to interpolate from</param>
        /// <param name="end">End color to interpolate to</param>
        /// <param name="factor">
        /// An interpolation ratio between 0 - 1 which indicates how much of each color should be present in the final color.
        /// 
        /// Values outside of range 0 - 1 (inclusive) are clamped.
        /// </param>
        /// <returns>An AHSL (alpha hue saturation and lightness) color</returns>
        [Pure]
        public static AhslColor LinearInterpolate(in AhslColor start, in AhslColor end, float factor)
        {
            // Clamp
            factor = Math.Max(0, Math.Min(1, factor));

            float Lerp(float min, float max, float v)
            {
                return (1 - v) * min + v * max;
            }
            
            float a = Lerp(start.FloatAlpha, end.FloatAlpha, factor);
            float r = Lerp(start.FloatRed, end.FloatRed, factor);
            float g = Lerp(start.FloatGreen, end.FloatGreen, factor);
            float b = Lerp(start.FloatBlue, end.FloatBlue, factor);

            return FromArgb(a, r, g, b);
        }
        
        #region Helper Methods

        /// <summary>
        /// Creates an ARGB color form the given HSL color components
        /// </summary>
        /// <param name="h">The hue</param>
        /// <param name="s">The saturation</param>
        /// <param name="l">The lightness</param>
        /// <param name="alpha">An optional alpha component</param>
        /// <param name="revertByteOrder">Whether to revert the byte order so the alpha component is the most significant and the blue component the least</param>
        /// <returns>An ARGB color from the given HSL color components</returns>
        [Pure]
        public static int ArgbFromAhsl(int h, int s, int l, int alpha = 255, bool revertByteOrder = false)
        {
            float af = alpha / 255.0f;
            float hf = h / 360.0f;
            float sf = s / 100.0f;
            float lf = l / 100.0f;

            return ArgbFromAhsl(hf, sf, lf, af, revertByteOrder);
        }

        /// <summary>
        /// Creates an ARGB color form the given HSL color components
        /// </summary>
        /// <param name="h">The hue</param>
        /// <param name="s">The saturation</param>
        /// <param name="l">The lightness</param>
        /// <param name="alpha">An optional alpha component</param>
        /// <param name="revertByteOrder">Whether to revert the byte order so the alpha component is the most significant and the blue component the least</param>
        /// <returns>An ARGB color from the given HSL color components</returns>
        [Pure]
        public static int ArgbFromAhsl(float h, float s, float l, float alpha = 1, bool revertByteOrder = false)
        {
            float[] components = FloatArgbFromAhsl(h, s, l, alpha, revertByteOrder);

            return ((int)(components[0] * 255) << 24) | ((int)(components[1] * 255) << 16) | ((int)(components[2] * 255) << 8) | (int)(components[3] * 255);
        }

        /// <summary>
        /// Creates an ARGB color form the given HSL color components
        /// </summary>
        /// <param name="h">The hue, ranging from 0-1</param>
        /// <param name="s">The saturation, ranging from 0-1</param>
        /// <param name="l">The lightness, ranging from 0-1</param>
        /// <param name="alpha">An optional alpha component, ranging from 0-1</param>
        /// <param name="reverseColorOrder">Whether to revert the color order so the alpha component appearns last and the blue appears first</param>
        /// <returns>An ARGB color from the given HSL color components</returns>
        [Pure]
        public static float[] FloatArgbFromAhsl(float h, float s, float l, float alpha = 1, bool reverseColorOrder = false)
        {
            if (h < 0) h = 0;
            if (s < 0) s = 0;
            if (l < 0) l = 0;
            if (h >= 1) h = 0.99999999f;
            if (s > 1) s = 1;
            if (l > 1) l = 1;

            // ReSharper disable once InconsistentNaming
            float C = (1 - Math.Abs(2 * l - 1)) * s;
            float hh = h / (60 / 360.0f);
            // ReSharper disable once InconsistentNaming
            float X = C * (1 - Math.Abs(hh % 2 - 1));

            float r = 0, g = 0, b = 0;

            if (hh >= 0 && hh < 1)
            {
                r = C;
                g = X;
            }
            else if (hh >= 1 && hh < 2)
            {
                r = X;
                g = C;
            }
            else if (hh >= 2 && hh < 3)
            {
                g = C;
                b = X;
            }
            else if (hh >= 3 && hh < 4)
            {
                g = X;
                b = C;
            }
            else if (hh >= 4 && hh < 5)
            {
                r = X;
                b = C;
            }
            else
            {
                r = C;
                b = X;
            }

            float m = l - C / 2;

            r = r + m;
            g = g + m;
            b = b + m;

            float[] colors = { 0, 0, 0, 0 };

            if (reverseColorOrder)
            {
                colors[0] = b;
                colors[1] = g;
                colors[2] = r;
                colors[3] = alpha;
            }
            else
            {
                colors[0] = alpha;
                colors[1] = r;
                colors[2] = g;
                colors[3] = b;
            }

            return colors;
        }

        /// <summary>
        /// Creates an ARGB color form the given HSL color components
        /// </summary>
        /// <param name="h">The hue</param>
        /// <param name="s">The saturation</param>
        /// <param name="l">The lightness</param>
        /// <param name="alpha">An optional alpha component</param>
        /// <returns>An ARGB color from the given HSL color components</returns>
        [Pure]
        public static Color ColorFromAhsl(float h, float s, float l, float alpha = 1)
        {
            return Color.FromArgb(ArgbFromAhsl(h, s, l, alpha));
        }

        #endregion
    }
    
    #region UITypeEditor
    
    /// <summary>
    /// A type editor for AhslColor property fields in a PropertyGrid
    /// </summary>
    public class AhslColorEditor : UITypeEditor
    {
        private IWindowsFormsEditorService _service;
        
        /// <summary>
        /// Displays a color picker
        /// </summary>
        /// <param name="context">An ITypeDescriptorContext that can be used to gain additional context information.</param>
        /// <param name="provider">A service provider object through which editing services may be obtained.</param>
        /// <param name="value">An instance of the value being edited.</param>
        /// <returns>The new value of the object. If the value of the object hasn't changed, this method should return the same object it was passed.</returns>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider == null)
                return value;
            
            // This service is in charge of popping our ListBox.
            _service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;

            if (value == null)
                value = AhslColors.White;

            if (_service == null || !(value is AhslColor color))
                return value;

            Application.EnableVisualStyles();

            using (var colorPicker = new ColorPickerDialog {SelectedColor = color})
            {
                if (_service.ShowDialog(colorPicker) == DialogResult.OK)
                    value = colorPicker.SelectedColor;
            }

            return value;
        }
        
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        
        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return true;
        }
        
        public override void PaintValue(PaintValueEventArgs e)
        {
            var color = (AhslColor)e.Value;
            using (var brush = new SolidBrush(color.ToColor()))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }
        }
    }

    public class AhslColorTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor) && value is AhslColor color)
            {
                // This assumes you have a public default constructor on your type.
                var args = new object[] {color.FloatAlpha, color.FloatHue, color.FloatSaturation, color.FloatLightness};

                var ctor = typeof(AhslColor).GetConstructor(Type.GetTypeArray(args));
                if (ctor != null)
                    return new InstanceDescriptor(ctor, args, true);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    
    #endregion
}
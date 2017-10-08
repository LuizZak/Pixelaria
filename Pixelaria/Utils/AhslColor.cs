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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Pixelaria.Views.Controls.ColorControls;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Represents an HSL color with an alpha channel
    /// </summary>
    [Editor(typeof(AhslColorEditor), typeof(UITypeEditor))]
    public struct AhslColor : IEquatable<AhslColor>
    {
        /// <summary>
        /// Gets or sets the alpha component as a value ranging from 0 - 255
        /// </summary>
        public int Alpha => (int) (_floatAlpha * 255.0f);

        /// <summary>
        /// Gets or sets the hue component as a value ranging from 0 - 360
        /// </summary>
        public int Hue => (int) (_floatHue * 360.0f);

        /// <summary>
        /// Gets or sets the saturation component as a value ranging from 0 - 100
        /// </summary>
        public int Saturation => (int) (_floatSaturation * 100.0f);

        /// <summary>
        /// Gets or sets the lightness component as a value ranging from 0 - 100
        /// </summary>
        public int Lightness => (int) (_floatLightness * 100.0f);

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
        public float FloatRed => ColorSwatch.FloatArgbFromAhsl(_floatHue, _floatSaturation, _floatLightness, _floatAlpha)[1];

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float FloatGreen => ColorSwatch.FloatArgbFromAhsl(_floatHue, _floatSaturation, _floatLightness, _floatAlpha)[2];

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float FloatBlue => ColorSwatch.FloatArgbFromAhsl(_floatHue, _floatSaturation, _floatLightness, _floatAlpha)[3];

        /// <summary>
        /// Gets or sets the alpha component as a value ranging from 0 - 1
        /// </summary>
        public float FloatAlpha => _floatAlpha;

        /// <summary>
        /// Gets or sets the hue component as a value ranging from 0 - 1
        /// </summary>
        public float FloatHue => _floatHue;

        /// <summary>
        /// Gets or sets the saturation component as a value ranging from 0 - 1
        /// </summary>
        public float FloatSaturation => _floatSaturation;

        /// <summary>
        /// Gets or sets the lightness component as a value ranging from 0 - 1
        /// </summary>
        public float FloatLightness => _floatLightness;

        /// <summary>
        /// The alpha component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _floatAlpha;
        /// <summary>
        /// The hue component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _floatHue;
        /// <summary>
        /// The saturation component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _floatSaturation;
        /// <summary>
        /// The lightness component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _floatLightness;
        
        /// <summary>
        /// Creates a new AHSL color
        /// </summary>
        /// <param name="a">The Alpha component, ranging from 0-255</param>
        /// <param name="h">The Hue component, ranging from 0-360</param>
        /// <param name="s">The Saturation component, ranging from 0-100</param>
        /// <param name="l">The Lightness component, ranging from 0-100</param>
        public AhslColor(int a, int h, int s, int l)
            : this(a / 255.0f, h / 360.0f, s / 100.0f, l / 100.0f) { }

        /// <summary>
        /// Creates a new AHSL color
        /// </summary>
        /// <param name="a">The Alpha component, ranging from 0-1</param>
        /// <param name="h">The Hue component, ranging from 0-1</param>
        /// <param name="s">The Saturation component, ranging from 0-1</param>
        /// <param name="l">The Lightness component, ranging from 0-1</param>
        public AhslColor(float a, float h, float s, float l)
        {
            _floatAlpha = Math.Max(0, Math.Min(1, a));
            _floatHue = Math.Max(0, Math.Min(1, h));
            _floatSaturation = Math.Max(0, Math.Min(1, s));
            _floatLightness = Math.Max(0, Math.Min(1, l));
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
            return (Math.Abs(color1._floatAlpha - color2._floatAlpha) < float.Epsilon &&
                    Math.Abs(color1._floatHue - color2._floatHue) < float.Epsilon &&
                    Math.Abs(color1._floatSaturation - color2._floatSaturation) < float.Epsilon &&
                    Math.Abs(color1._floatLightness - color2._floatLightness) < float.Epsilon);
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
            return ColorSwatch.ArgbFromAhsl(_floatHue, _floatSaturation, _floatLightness, _floatAlpha, revertByteOrder);
        }

        /// <summary>
        /// Returns whether this AHSL color object equals another AHSL color
        /// </summary>
        /// <param name="other">The other color to test</param>
        /// <returns>Whether this AHSL color object equals another AHSL color</returns>
        public bool Equals(AhslColor other)
        {
            return _floatAlpha.Equals(other._floatAlpha) && _floatHue.Equals(other._floatHue) && _floatSaturation.Equals(other._floatSaturation) && _floatLightness.Equals(other._floatLightness);
        }

        // Override Equals
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AhslColor && Equals((AhslColor)obj);
        }

        // Overrided GetHashCode
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _floatAlpha.GetHashCode();
                hashCode = (hashCode * 397) ^ _floatHue.GetHashCode();
                hashCode = (hashCode * 397) ^ _floatSaturation.GetHashCode();
                hashCode = (hashCode * 397) ^ _floatLightness.GetHashCode();
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
                h = (((g - b) / d) % 6) * 60;
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

            var l = (M + m) / 2;

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
        public static AhslColor LinearInterpolate(AhslColor start, AhslColor end, float factor)
        {
            // Clamp
            factor = Math.Max(0, Math.Min(1, factor));

            float Lerp(float min, float max, float v)
            {
                return min * (1 - v) + max * v;
            }
            
            var a = Lerp(start.FloatAlpha, end.FloatAlpha, factor);
            var h = Lerp(start.FloatHue, end.FloatHue, factor);
            var s = Lerp(start.FloatSaturation, end.FloatSaturation, factor);
            var l = Lerp(start.FloatLightness, end.FloatLightness, factor);

            return new AhslColor(a, h, s, l);
        }
    }

    #region Default Color Definitions

    /// <summary>
    /// Specifies default color definitions
    /// </summary>
    public struct AhslColors
    {
        /// <summary>
        /// A: 1, H: 0, S: 1, L: 1
        /// </summary>
        public static AhslColor White = new AhslColor(1.0f, 0, 1, 1);

        /// <summary>
        /// A: 1, H: 0, S: 0, L: 0
        /// </summary>
        public static AhslColor Black = new AhslColor(1.0f, 0, 0, 0);

        /// <summary>
        /// A: 1, H: 0, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Red = new AhslColor(1.0f, 0, 1, 0.5f);

        /// <summary>
        /// A: 1, H: 0.333, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Green = AhslColor.FromArgb(1.0f, 0, 1, 0);

        /// <summary>
        /// A: 1, H: 0.666, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Blue = AhslColor.FromArgb(1.0f, 0, 0, 1);

        /// <summary>
        /// A: 1, H: 0.166, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Yellow = AhslColor.FromArgb(1.0f, 1, 1, 0);

        /// <summary>
        /// A: 1, H: 0.5, S: 1, L: 0.5
        /// </summary>
        public static AhslColor Cyan = new AhslColor(1.0f, 0.5f, 1, 1);
    }

    #endregion

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

            if (_service == null || !(value is AhslColor color))
                return value;

            using (var colorPicker = new ColorPickerDialog {SelectedColor = color})
            {
                if (_service.ShowDialog(colorPicker) != DialogResult.OK)
                    return value;

                if (colorPicker.SelectedColor != color)
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

    #endregion
}
using System;
using System.Drawing;
using Pixelaria.Views.Controls.ColorControls;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Represents an HSL color with an alpha channel
    /// </summary>
    public struct AhslColor : IEquatable<AhslColor>
    {
        /// <summary>
        /// Gets or sets the alpha component as a value ranging from 0 - 255
        /// </summary>
        public int A
        {
            get { return (int) (_af * 255.0f); }
        }

        /// <summary>
        /// Gets or sets the hue component as a value ranging from 0 - 360
        /// </summary>
        public int H
        {
            get { return (int) (_hf * 360.0f); }
        }

        /// <summary>
        /// Gets or sets the saturation component as a value ranging from 0 - 100
        /// </summary>
        public int S
        {
            get { return (int) (_sf * 100.0f); }
        }

        /// <summary>
        /// Gets or sets the lightness component as a value ranging from 0 - 100
        /// </summary>
        public int L
        {
            get { return (int) (_lf * 100.0f); }
        }

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public int R
        {
            get { return ToColor().R; }
        }

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public int G
        {
            get { return ToColor().G; }

        }

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public int B
        {
            get { return ToColor().B; }
        }

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float Rf
        {
            get { return ColorSwatch.FloatArgbFromAHSL(_hf, _sf, _lf, _af)[1]; }
        }

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float Gf
        {
            get { return ColorSwatch.FloatArgbFromAHSL(_hf, _sf, _lf, _af)[2]; }
        }

        /// <summary>
        /// Gets the Red component value for this AHSL color
        /// </summary>
        public float Bf
        {
            get { return ColorSwatch.FloatArgbFromAHSL(_hf, _sf, _lf, _af)[3]; }
        }

        /// <summary>
        /// Gets or sets the alpha component as a value ranging from 0 - 1
        /// </summary>
        public float Af
        {
            get { return _af; }
        }

        /// <summary>
        /// Gets or sets the hue component as a value ranging from 0 - 1
        /// </summary>
        public float Hf
        {
            get { return _hf; }
        }

        /// <summary>
        /// Gets or sets the saturation component as a value ranging from 0 - 1
        /// </summary>
        public float Sf
        {
            get { return _sf; }
        }

        /// <summary>
        /// Gets or sets the lightness component as a value ranging from 0 - 1
        /// </summary>
        public float Lf
        {
            get { return _lf; }
        }

        /// <summary>
        /// The alpha component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _af;
        /// <summary>
        /// The hue component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _hf;
        /// <summary>
        /// The saturation component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _sf;
        /// <summary>
        /// The lightness component as a value ranging from 0 - 1
        /// </summary>
        private readonly float _lf;
        
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
            _af = Math.Max(0, Math.Min(1, a));
            _hf = Math.Max(0, Math.Min(1, h));
            _sf = Math.Max(0, Math.Min(1, s));
            _lf = Math.Max(0, Math.Min(1, l));
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
            return (Math.Abs(color1._af - color2._af) < float.Epsilon &&
                    Math.Abs(color1._hf - color2._hf) < float.Epsilon &&
                    Math.Abs(color1._sf - color2._sf) < float.Epsilon &&
                    Math.Abs(color1._lf - color2._lf) < float.Epsilon);
        }

        /// <summary>
        /// Converts this AHSL color to a Color object
        /// </summary>
        /// <returns>The Color object that represents this AHSL color</returns>
        public Color ToColor()
        {
            return Color.FromArgb(ToArgb());
        }

        /// <summary>
        /// Converts this AHSL color to a ARGB color
        /// </summary>
        /// <param name="revertByteOrder">Whether to revert the byte order so the alpha component is the most significant and the blue component the least</param>
        /// <returns>The ARGB color that represents this AHSL color</returns>
        public int ToArgb(bool revertByteOrder = false)
        {
            return ColorSwatch.ArgbFromAHSL(_hf, _sf, _lf, _af, revertByteOrder);
        }

        /// <summary>
        /// Returns whether this AHSL color object equals another AHSL color
        /// </summary>
        /// <param name="other">The other color to test</param>
        /// <returns>Whether this AHSL color object equals another AHSL color</returns>
        public bool Equals(AhslColor other)
        {
            return _af.Equals(other._af) && _hf.Equals(other._hf) && _sf.Equals(other._sf) && _lf.Equals(other._lf);
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
                var hashCode = _af.GetHashCode();
                hashCode = (hashCode * 397) ^ _hf.GetHashCode();
                hashCode = (hashCode * 397) ^ _sf.GetHashCode();
                hashCode = (hashCode * 397) ^ _lf.GetHashCode();
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
        // ReSharper disable once InconsistentNaming
        public static AhslColor FromAHSL(int a, int h, int s, int l)
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
        public static AhslColor FromArgb(int a, int r, int g, int b)
        {
            return Utilities.ToAHSL((a << 24) | (r << 16) | (g << 8) | b);
        }

        /// <summary>
        /// Creates an AHSL object from the given ARGB color
        /// </summary>
        /// <param name="a">The Alpha component, ranging from 0-1</param>
        /// <param name="r">The Red component, ranging from 0-1</param>
        /// <param name="g">The Green component, ranging from 0-1</param>
        /// <param name="b">The Blue component, ranging from 0-1</param>
        /// <returns>The AHSL color representing the given ARGB value</returns>
        public static AhslColor FromArgb(float a, float r, float g, float b)
        {
            return Utilities.ToAHSL(a, r, g, b);
        }

        /// <summary>
        /// Creates an AHSL object from the given ARGB color
        /// </summary>
        /// <param name="argb">The ARGB color to convert to AHSL</param>
        /// <returns>The AHSL color representing the given ARGB value</returns>
        public static AhslColor FromArgb(int argb)
        {
            return Utilities.ToAHSL(argb);
        }
    }
}
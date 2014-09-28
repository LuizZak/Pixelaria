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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Utils;

namespace Pixelaria.Views.Controls.ColorControls
{
    /// <summary>
    /// A control that displays a slider with a colored background used to pick a single color component from a composed color
    /// </summary>
    public partial class ColorSlider : UserControl
    {
        /// <summary>
        /// The active color for this ColorSlider
        /// </summary>
        private Color activeColor = Color.FromArgb(0xFF, 0xAE, 0xB0, 0x11);

        /// <summary>
        /// The color component this ColorSlider is currently manipulating
        /// </summary>
        private ColorSliderComponent colorComponent;

        /// <summary>
        /// Gets or sets the color component this ColorSlider is currently manipulating
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The color component this ColorSlider is currently manipulating")]
        [DefaultValue(ColorSliderComponent.Alpha)]
        public ColorSliderComponent ColorComponent
        {
            get
            {
                return colorComponent;
            }
            set
            {
                colorComponent = value;
                RecalculateValue();
                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the active color for this ColorSlider
        /// </summary>
        public Color ActiveColor
        {
            get
            {
                return activeColor;
            }
            set
            {
                activeColor = value;
                this.RecalculateValue();
                this.Invalidate();
            }
        }

        /// <summary>
        /// Iniitalizes a new instance of the ColorSlider class
        /// </summary>
        public ColorSlider()
        {
            InitializeComponent();

            ColorComponent = ColorSliderComponent.Alpha;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        #region Position/value-related calculations

        /// <summary>
        /// Recalculates this color slider's value property from the current
        /// color's composition
        /// </summary>
        private void RecalculateValue()
        {
            this.rtb_value.Text = "" + GetColorComponentValueRaw();
        }

        /// <summary>
        /// Returns an integer number that represents the offset for the slider that represents the
        /// current active color's component's value on this slider
        /// </summary>
        /// <returns>
        /// An integer number that represents the offset for the slider that represents the
        /// current active color's component's value on this slider
        /// </returns>
        private int GetSliderXOffset()
        {
            return (int)(7 + GetColorComponentValue() * (this.Width - 14));
        }

        /// <summary>
        /// <para>
        /// Returns a value that represents the value of the component currently being manipulated by this
        /// ColorSlider on the current active color.
        /// </para>
        /// <para>
        /// The value always ranges from [0 - 1] inclusive
        /// </para>
        /// </summary>
        /// <returns>
        /// <para>
        /// Avalue that represents the value of the component currently being manipulated by this
        /// ColorSlider on the current active color.
        /// </para>
        /// <para>
        /// The value always ranges from [0 - 1] inclusive
        /// </para>
        /// </returns>
        private float GetColorComponentValue()
        {
            switch (this.ColorComponent)
            {
                case ColorSliderComponent.Alpha:
                    return activeColor.A / 255.0f;
                case ColorSliderComponent.Red:
                    return activeColor.R / 255.0f;
                case ColorSliderComponent.Green:
                    return activeColor.G / 255.0f;
                case ColorSliderComponent.Blue:
                    return activeColor.B / 255.0f;
                case ColorSliderComponent.Hue:
                    return activeColor.ToAHSL().H / 360.0f;
                case ColorSliderComponent.Saturation:
                    return activeColor.ToAHSL().S / 100.0f;
                case ColorSliderComponent.Lightness:
                    return activeColor.ToAHSL().L / 100.0f;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// <para>
        /// Returns a value that represents the value of the component currently being manipulated by this
        /// ColorSlider on the current active color.
        /// </para>
        /// </summary>
        /// <returns>
        /// <para>
        /// Avalue that represents the value of the component currently being manipulated by this
        /// ColorSlider on the current active color.
        /// </para>
        /// </returns>
        private int GetColorComponentValueRaw()
        {
            switch (this.ColorComponent)
            {
                case ColorSliderComponent.Alpha:
                    return activeColor.A;
                case ColorSliderComponent.Red:
                    return activeColor.R;
                case ColorSliderComponent.Green:
                    return activeColor.G;
                case ColorSliderComponent.Blue:
                    return activeColor.B;
                case ColorSliderComponent.Hue:
                    return activeColor.ToAHSL().H;
                case ColorSliderComponent.Saturation:
                    return activeColor.ToAHSL().S;
                case ColorSliderComponent.Lightness:
                    return activeColor.ToAHSL().L;

                default:
                    return 0;
            }
        }

        #endregion

        #region Drawing

        //
        // OnPaint event handler
        //
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            DrawLabel(g);

            // Setup the Graphic's properties
            GraphicsState state = g.Save();
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;

            // Draw the slider components
            DrawSlider(g);
            DrawKnob(g);

            // Restore the graphic's state
            g.Restore(state);
        }

        /// <summary>
        /// Draws the slider's label on a given Graphics object
        /// </summary>
        /// <param name="g">A valid Graphics object to draw the label to</param>
        private void DrawLabel(Graphics g)
        {
            // Draw the string
            g.DrawString(GetNameForSliderComponent(this.ColorComponent) + ":", this.Font, Brushes.Black, new PointF(6, 1.5f));
        }

        /// <summary>
        /// Draw the slider's contents on a given Graphics object
        /// </summary>
        /// <param name="g">A valid Graphics object to draw the slider to</param>
        private void DrawSlider(Graphics g)
        {
            GraphicsPath path = GenerateSliderGraphicsPath();

            // Draw a background
            Brush backBrush = new TextureBrush(ImageUtilities.GetDefaultTile());
            g.FillPath(backBrush, path);
            backBrush.Dispose();

            // Draw the fill
            Brush brush = GenerateSliderGradient();
            g.FillPath(brush, path);
            brush.Dispose();

            // Draw the outline
            Pen pen = (Pen)Pens.DarkGray.Clone();
            pen.Width = 2;
            g.DrawPath(pen, path);

            path.Dispose();
        }

        /// <summary>
        /// Draw the slider's knob on a given Graphics object
        /// </summary>
        /// <param name="g">A valid Graphics object to draw the knob to</param>
        private void DrawKnob(Graphics g)
        {
            float xOffset = GetSliderXOffset();

            Matrix knobMatrix = new Matrix();
            knobMatrix.Translate(xOffset - 6, GetSliderRectangleBounds().Top + 10);

            // Create the knob base graphics path
            GraphicsPath knobBase = new GraphicsPath();
            knobBase.AddLine(6, 0, 12, 8);
            knobBase.AddLine(12, 8, 0, 8);
            knobBase.AddLine(0, 8, 6, 0);
            knobBase.CloseAllFigures();

            knobBase.Transform(knobMatrix);

            // Draw the knob line
            Pen linePen = new Pen(activeColor.Invert());
            g.DrawLine(linePen, xOffset, GetSliderRectangleBounds().Top, xOffset, GetSliderRectangleBounds().Top + 10);
            linePen.Dispose();

            // Draw the knob base
            Brush knobBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Control));

            g.FillPath(knobBrush, knobBase);
            g.DrawPath(Pens.DarkGray, knobBase);

            knobBrush.Dispose();
        }

        /// <summary>
        /// Generates and returns a GraphicsPath that represents the outline of the slider's area
        /// </summary>
        /// <returns>A GraphicsPath that represents the outline of the slider's area</returns>
        private GraphicsPath GenerateSliderGraphicsPath()
        {
            GraphicsPath path = new GraphicsPath();

            path.AddRoundedRectangle(GetSliderRectangleBounds(), 15);

            return path;
        }

        /// <summary>
        /// Generates and returns a LinearGradientBrush that is used to draw the slider's background area
        /// </summary>
        /// <returns>A LinearGradientBrush that is used to draw the slider's background area</returns>
        private LinearGradientBrush GenerateSliderGradient()
        {
            ColorBlend blend;
            LinearGradientBrush brush;

            Rectangle rec = GetSliderRectangleBounds();

            // Hue is a special case
            if (this.colorComponent == ColorSliderComponent.Hue)
            {
                int steps = 54;

                // Create the color blends for the brush
                blend = new ColorBlend(steps);

                List<Color> colors = new List<Color>();
                List<float> positions = new List<float>();

                // Create the colors now
                for (int i = 0; i < steps; i++)
                {
                    float v = (float)i / (steps - 1);
                    colors.Add(GetColorWithActiveComponentSet(v));
                    positions.Add(v);
                }

                blend.Colors = colors.ToArray();
                blend.Positions = positions.ToArray();

                // Create the brush
                brush = new LinearGradientBrush(rec, Color.White, Color.White, LinearGradientMode.Horizontal);
                brush.InterpolationColors = blend;
                brush.WrapMode = WrapMode.TileFlipXY;

                return brush;
            }

            Color zeroColor = GetColorWithActiveComponentSet(0);
            Color halfColor = GetColorWithActiveComponentSet(0.5f);
            Color fullColor = GetColorWithActiveComponentSet(1);

            rec.X += 7;
            rec.Width -= 14;

            // Create the color blends for the brush
            blend = new ColorBlend(3);
            blend.Colors = new Color[] { zeroColor, halfColor, fullColor };
            blend.Positions = new float[] { 0.0f, 0.5f, 1.0f };

            // Create the brush
            brush = new LinearGradientBrush(rec, zeroColor, fullColor, LinearGradientMode.Horizontal);
            brush.InterpolationColors = blend;
            brush.WrapMode = WrapMode.TileFlipXY;

            return brush;
        }

        /// <summary>
        /// Gets a color with the current active component set to be of the given value
        /// </summary>
        /// <param name="componentValue">The component value, ranging from [0 - 1]</param>
        /// <returns>A color with the current active component set to be of the given value</returns>
        private Color GetColorWithActiveComponentSet(float componentValue)
        {
            Color retColor = this.activeColor;

            switch (this.ColorComponent)
            {
                case ColorSliderComponent.Alpha:
                    retColor = Color.FromArgb((int)(255 * componentValue), retColor.R, retColor.G, retColor.B);
                    break;
                case ColorSliderComponent.Red:
                    retColor = Color.FromArgb(retColor.A, (int)(255 * componentValue), retColor.G, retColor.B);
                    break;
                case ColorSliderComponent.Green:
                    retColor = Color.FromArgb(retColor.A, retColor.R, (int)(255 * componentValue), retColor.B);
                    break;
                case ColorSliderComponent.Blue:
                    retColor = Color.FromArgb(retColor.A, retColor.R, retColor.G, (int)(255 * componentValue));
                    break;
                case ColorSliderComponent.Hue:
                    retColor = AHSL.FromAHSL(retColor.A, (int)(360 * componentValue), (int)(retColor.GetSaturation() * 100), (int)(retColor.GetLightness() * 100)).ToColor();
                    break;
                case ColorSliderComponent.Saturation:
                    retColor = AHSL.FromAHSL(retColor.A, (int)(retColor.GetHue()), (int)(100 * componentValue), (int)(retColor.GetLightness() * 100)).ToColor();
                    break;
                case ColorSliderComponent.Lightness:
                    retColor = AHSL.FromAHSL(retColor.A, (int)(retColor.GetHue()), (int)(retColor.GetSaturation() * 100), (int)(100 * componentValue)).ToColor();
                    break;
            }

            return retColor;
        }

        /// <summary>
        /// Returns a Rectangle object that represents the slider's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider's bounds</returns>
        private Rectangle GetSliderRectangleBounds()
        {
            return new Rectangle(0, 19, this.Width - 1, 15);
        }

        #endregion

        //
        // SetBoundsCore override used to lock the control's height
        //
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // EDIT: ADD AN EXTRA HEIGHT VALIDATION TO AVOID INITIALIZATION PROBLEMS
            // BITWISE 'AND' OPERATION: IF ZERO THEN HEIGHT IS NOT INVOLVED IN THIS OPERATION
            if ((specified & BoundsSpecified.Height) == 0 || (specified & BoundsSpecified.Width) == 0 || height == 38)
            {
                base.SetBoundsCore(x, y, Math.Max(80, width), 38, specified);
            }
            else
            {
                return; // RETURN WITHOUT DOING ANY RESIZING
            }
        }

        /// <summary>
        /// Returns a string that represents the given ColorSliderComponent enum value
        /// </summary>
        /// <param name="component">A valid ColorSliderComponent value</param>
        /// <returns>A string that represents the given ColorSliderComponent enum value</returns>
        private static string GetNameForSliderComponent(ColorSliderComponent component)
        {
            switch (component)
            {
                case ColorSliderComponent.Alpha:
                    return "Alpha";
                case ColorSliderComponent.Red:
                    return "Red";
                case ColorSliderComponent.Green:
                    return "Green";
                case ColorSliderComponent.Blue:
                    return "Blue";
                case ColorSliderComponent.Hue:
                    return "Hue";
                case ColorSliderComponent.Saturation:
                    return "Saturation";
                case ColorSliderComponent.Lightness:
                    return "Lightness";

                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Defines what color component a ColorSlider is supposed to manipulate
    /// </summary>
    public enum ColorSliderComponent
    {
        /// <summary>
        /// Specifies an alpha channel modification
        /// </summary>
        Alpha,
        /// <summary>
        /// Specifies a red component modification
        /// </summary>
        Red,
        /// <summary>
        /// Specifies a green component modification
        /// </summary>
        Green,
        /// <summary>
        /// Specifies a blue component modification
        /// </summary>
        Blue,
        /// <summary>
        /// Specifies a hue component modification
        /// </summary>
        Hue,
        /// <summary>
        /// Specifies a saturation component modification
        /// </summary>
        Saturation,
        /// <summary>
        /// Specifies a lightness component modification
        /// </summary>
        Lightness
    }
}
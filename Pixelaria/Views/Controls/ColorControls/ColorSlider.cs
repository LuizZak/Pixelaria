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
using System.Linq;
using System.Windows.Forms;

using Pixelaria.Utils;

namespace Pixelaria.Views.Controls.ColorControls
{
    /// <summary>
    /// A control that displays a slider with a colored background used to pick a single color component from a composed color
    /// </summary>
    [DefaultEvent("ColorChanged")]
    public partial class ColorSlider : UserControl
    {
        /// <summary>
        /// The active color for this ColorSlider
        /// </summary>
        protected AhslColor activeColor = Color.Black.ToAhsl();

        /// <summary>
        /// The color component this ColorSlider is currently manipulating
        /// </summary>
        protected ColorSliderComponent colorComponent;

        /// <summary>
        /// Whether the mouse is currently over the slider area
        /// </summary>
        protected bool mouseOverSlider;
        
        /// <summary>
        /// Whether the mouse is currently over the knob area
        /// </summary>
        protected bool mouseOverKnob;

        /// <summary>
        /// Whether the mouse is currently dragging the knob on this ColorSlider
        /// </summary>
        protected bool mouseDragging;

        /// <summary>
        /// An offset applied to the mouse position while dragging the knob
        /// </summary>
        protected float knobDraggingOffset;

        /// <summary>
        /// The current value specified by this ColorSlider, ranging from [0 - 1]
        /// </summary>
        protected float currentValue;

        /// <summary>
        /// The fixed height for this color slider
        /// </summary>
        protected int fixedControlHeight;

        /// <summary>
        /// Whether to render the label
        /// </summary>
        protected bool drawLabel = true;

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
        /// Gets the current value specified by this ColorSlider, ranging from [0 - 1]
        /// </summary>
        [Browsable(false)]
        public float CurrentValue
        {
            get
            {
                return currentValue;
            }
            set
            {
                float newValue = Math.Max(0, Math.Min(1, value));

                if(Math.Abs(newValue - currentValue) > float.Epsilon)
                    SetColorComponentValue(value);
            }
        }

        /// <summary>
        /// Delegate for a ColorChanged event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void ColorChangedEventHandler(object sender, ColorChangedEventArgs eventArgs);

        /// <summary>
        /// Occurs whenever the current active color component is changed by the user
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the current active color component is changed by the user")]
        public event ColorChangedEventHandler ColorChanged;

        /// <summary>
        /// Gets or sets the active color for this ColorSlider
        /// </summary>
        public AhslColor ActiveColor
        {
            get
            {
                return activeColor;
            }
            set
            {
                SetActiveColor(value);
                RecalculateValue();
            }
        }

        /// <summary>
        /// Iniitalizes a new instance of the ColorSlider class
        /// </summary>
        public ColorSlider()
        {
            fixedControlHeight = 38;

            InitializeComponent();

            ColorComponent = ColorSliderComponent.Alpha;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        #region Mouse dragging handling methods

        //
        // OnMousedown event handler
        //
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Rectangle rect = GetSliderRectangleBounds();

            if (rect.Contains(e.Location))
            {
                // Test agains the current knob position, if the mouse is over the knob, setup an offset so
                // the mouse drags relative to the current knob's position
                Rectangle knobBounds = GetKnobRectangleBounds();
                knobBounds.Inflate(4, 0);
                
                knobDraggingOffset = 0;

                if (knobBounds.Contains(e.Location))
                {
                    knobDraggingOffset = e.Location.X - GetSliderXOffset();
                }

                UpdateValueForMouseEvent(e);
                mouseDragging = true;

                InvalidateSlider();
            }
        }

        // 
        // OnMouseMove event handler
        //
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDragging)
            {
                UpdateValueForMouseEvent(e);
            }
            else
            {
                if (GetSliderRectangleBounds().Contains(e.Location) && !mouseOverSlider)
                {
                    mouseOverSlider = true;
                    InvalidateSlider();
                }
                else if (!GetSliderRectangleBounds().Contains(e.Location))
                {
                    if(mouseOverSlider)
                    {
                        mouseOverSlider = false;
                        InvalidateSlider();
                    }
                }

                Rectangle knobRect = GetKnobRectangleBounds();
                knobRect.Inflate(4, 0);

                if (knobRect.Contains(e.Location) && !mouseOverKnob)
                {
                    mouseOverKnob = true;
                    InvalidateKnob();
                }
                else if (!knobRect.Contains(e.Location))
                {
                    if (mouseOverKnob)
                    {
                        mouseOverKnob = false;
                        InvalidateKnob();
                    }
                }
            }
        }

        //
        // OnMouseUp event handler
        //
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (mouseDragging)
            {
                InvalidateSlider();
                mouseDragging = false;
            }
        }

        //
        // OnMouseLeave event handler
        //
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (mouseOverSlider)
            {
                mouseOverSlider = false;
                InvalidateSlider();
            }

            if (mouseOverKnob)
            {
                mouseOverKnob = false;
                InvalidateKnob();
            }
        }

        /// <summary>
        /// Updates the value of this slider based on a given mouse event args. The arguments are used to get the
        /// position of the mouse during the event, and assign a value based on this position relative to the slider
        /// </summary>
        /// <param name="e">The mouse event args to use to manipulate the mouse</param>
        private void UpdateValueForMouseEvent(MouseEventArgs e)
        {
            float value = GetValueForXOffset(e.X - knobDraggingOffset);

            SetColorComponentValue(value);
        }

        #endregion

        #region TextBox input handling

        bool _ignoreTextField;
        //
        // Value rich text box text changed
        //
        private void rtb_value_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreTextField)
                return;

            int rawValue;
            int maxValue = GetColorComponentMaxValue();
            float value = 0;
            string valueString = txt_value.Text;

            // If the caret is not at the end of the current value string, trim the digits that are out of the range
            if (txt_value.SelectionStart != txt_value.TextLength)
            {
                valueString = valueString.Substring(0, Math.Min(valueString.Length, maxValue.ToString().Length));
            }

            // Parse the value
            if (int.TryParse(valueString, out rawValue))
            {
                if (rawValue > maxValue)
                {
                    rawValue = maxValue;
                }

                value = (float)rawValue / maxValue;
            }

            SetColorComponentValue(value);
        }
        
        //
        // Value rich text box key down
        //
        private void rtb_value_KeyDown(object sender, KeyEventArgs e)
        {
                // Numeric keys above letters
            if (!((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                // Numpad
                  (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) ||
                // Backspace and delete
                  (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete) ||
                // Directional keys
                  (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)))
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        #endregion

        #region Position/value-related calculations

        /// <summary>
        /// Recalculates this color slider's value property from the current color's composition
        /// </summary>
        private void RecalculateValue()
        {
            string valueString = "" + GetColorComponentValueRaw();

            currentValue = GetColorComponentValue();
            _ignoreTextField = true;
            txt_value.Text = valueString;
            _ignoreTextField = false;

            if (txt_value.SelectionStart > valueString.Length)
                txt_value.SelectionStart = valueString.Length;
        }

        /// <summary>
        /// Sets a given color as the current active color of this ColorSlider
        /// </summary>
        /// <param name="color">The color to set as the new active color for this ColorSlider</param>
        private void SetActiveColor(AhslColor color)
        {
            // Check if any redraw is required
            if (IgnoreActiveColorAlpha())
            {
                if (Math.Abs(activeColor.Hf - color.Hf) < float.Epsilon && Math.Abs(activeColor.Sf - color.Sf) < float.Epsilon && Math.Abs(activeColor.Lf - color.Lf) < float.Epsilon)
                {
                    activeColor = color;

                    return;
                }
            }

            activeColor = color;

            Rectangle invalidateRect = GetSliderRectangleBounds();
            invalidateRect.Height = Height - invalidateRect.Top;

            Invalidate(invalidateRect);
            InvalidateKnob();
        }

        /// <summary>
        /// Returns a float value ranging from [0 - 1] that indicates the value represented by a given X offset of the slider
        /// </summary>
        /// <param name="xOffset">An X offset of the slider's total size</param>
        /// <returns>A float value ranging from [0 - 1] that indicates the value represented by a given X offset of the slider</returns>
        private float GetValueForXOffset(float xOffset)
        {
            // Get the slider rectangle and move it to offset 0
            Rectangle rect = GetSliderRectangleBounds();

            // Move the offset by the ammount the rectangle was moved too
            xOffset -= rect.X + rect.Height / 2;

            rect.X = 0;
            rect.Width -= rect.Height;

            float value = xOffset / rect.Width;

            return Math.Max(0, Math.Min(1, value));
        }

        /// <summary>
        /// Returns an integer number that represents the offset for the slider that represents the
        /// current active color's component's value on this slider
        /// </summary>
        /// <returns>
        /// An integer number that represents the offset for the slider that represents the
        /// current active color's component's value on this slider
        /// </returns>
        protected int GetSliderXOffset()
        {
            Rectangle rec = GetSliderRectangleBounds();

            return (int)(rec.Left + rec.Height / 2 + currentValue * (rec.Width - rec.Height));
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
            switch (ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    return activeColor.Af;
                // RGB
                case ColorSliderComponent.Red:
                    return activeColor.Rf;
                case ColorSliderComponent.Green:
                    return activeColor.Gf;
                case ColorSliderComponent.Blue:
                    return activeColor.Bf;
                // HSL
                case ColorSliderComponent.Hue:
                    return activeColor.Hf;
                case ColorSliderComponent.Saturation:
                    return activeColor.Sf;
                case ColorSliderComponent.Lightness:
                    return activeColor.Lf;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Sets the active color component to be of the given value.
        /// The value must range from [0 - 1]
        /// </summary>
        /// <param name="value">The value to set. The value must range from [0 - 1]</param>
        private void SetColorComponentValue(float value)
        {
            // Pre-invalidate the knob area
            InvalidateKnob();

            //
            AhslColor oldColor = activeColor;

            currentValue = value;

            AhslColor newColor = activeColor;

            switch (ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    newColor = new AhslColor(value, newColor.Hf, newColor.Sf, newColor.Lf);
                    break;
                // RGB
                case ColorSliderComponent.Red:
                    newColor = AhslColor.FromArgb(activeColor.Af, value, activeColor.Gf, activeColor.Bf);
                    break;
                case ColorSliderComponent.Green:
                    newColor = AhslColor.FromArgb(activeColor.Af, activeColor.Rf, value, activeColor.Bf);
                    break;
                case ColorSliderComponent.Blue:
                    newColor = AhslColor.FromArgb(activeColor.Af, activeColor.Rf, activeColor.Gf, value);
                    break;
                // HSL
                case ColorSliderComponent.Hue:
                    newColor = new AhslColor(newColor.Af, value, newColor.Sf, newColor.Lf);
                    break;
                case ColorSliderComponent.Saturation:
                    newColor = new AhslColor(newColor.Af, newColor.Hf, value, newColor.Lf);
                    break;
                case ColorSliderComponent.Lightness:
                    newColor = new AhslColor(newColor.Af, newColor.Hf, newColor.Sf, value);
                    break;
            }

            SetActiveColor(newColor);

            // Fire the event now
            if (ColorChanged != null)
            {
                ColorChanged(this, new ColorChangedEventArgs(oldColor, activeColor, colorComponent));
            }
        }

        /// <summary>
        /// Returns a value that represents the value of the component currently being manipulated by this
        /// ColorSlider on the current active color.
        /// </summary>
        /// <returns>
        /// Avalue that represents the value of the component currently being manipulated by this
        /// ColorSlider on the current active color.
        /// </returns>
        private int GetColorComponentValueRaw()
        {
            switch (ColorComponent)
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
                    return activeColor.H;
                case ColorSliderComponent.Saturation:
                    return activeColor.S;
                case ColorSliderComponent.Lightness:
                    return activeColor.L;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Returns a value that represents the maximum accepted value for the current component
        /// being manipulated by this ColorSlider
        /// </summary>
        /// <returns>A value that represents the maximum accepted value for the current component
        /// being manipulated by this ColorSlider</returns>
        private int GetColorComponentMaxValue()
        {
            switch (ColorComponent)
            {
                // ARGB
                case ColorSliderComponent.Alpha:
                case ColorSliderComponent.Red:
                case ColorSliderComponent.Green:
                case ColorSliderComponent.Blue:
                    return 255;
                // Hue
                case ColorSliderComponent.Hue:
                    return 360;
                // Saturation and Lightness
                case ColorSliderComponent.Saturation:
                case ColorSliderComponent.Lightness:
                    return 100;

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

            if(drawLabel)
            {
                DrawLabel(g);
            }

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
        /// Invalidates the region associated with this slider's knob
        /// </summary>
        private void InvalidateKnob()
        {
            RectangleF bounds = GenerateKnobGraphicsPath().GetBounds();
            bounds.Inflate(2, 2);

            Invalidate(new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height));
        }

        /// <summary>
        /// Invalidates the region associated with this slider's slider rectangle
        /// </summary>
        private void InvalidateSlider()
        {
            RectangleF bounds = GetSliderRectangleBounds();
            bounds.Inflate(2, 2);

            Invalidate(new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height));
        }

        /// <summary>
        /// Draws the slider's label on a given Graphics object
        /// </summary>
        /// <param name="g">A valid Graphics object to draw the label to</param>
        private void DrawLabel(Graphics g)
        {
            // Draw the string
            g.DrawString(GetNameForSliderComponent(ColorComponent) + ":", Font, Brushes.Black, new PointF(6, 1.5f));
        }

        /// <summary>
        /// Draw the slider's contents on a given Graphics object
        /// </summary>
        /// <param name="g">A valid Graphics object to draw the slider to</param>
        private void DrawSlider(Graphics g)
        {
            GraphicsPath path = GenerateSliderGraphicsPath();

            // Get the fill brush
            LinearGradientBrush fillBrush = GenerateSliderGradient();

            if (!IgnoreActiveColorAlpha())
            {
                // Iterate through the colors and check if there's any that isn't fully opaque
                // Draw the background image then
                if (fillBrush.InterpolationColors.Colors.Any(color => color.A != 255))
                {
                    Brush backBrush = new TextureBrush(ImageUtilities.GetDefaultTile());

                    var compositingQuality = g.CompositingQuality;
                    var interpolationMode = g.InterpolationMode;

                    g.CompositingQuality = CompositingQuality.HighSpeed;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;

                    g.FillPath(backBrush, path);

                    g.CompositingQuality = compositingQuality;
                    g.InterpolationMode = interpolationMode;

                    backBrush.Dispose();
                }
            }

            // Draw the fill
            g.FillPath(fillBrush, path);
            fillBrush.Dispose();

            // Draw the outline
            Pen pen = (Pen)(mouseOverSlider ? (mouseDragging ? Pens.Black : Pens.Gray) : Pens.DarkGray).Clone();
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
            Pen basePen = (Pen)Pens.DarkGray.Clone();
            basePen.Width = 4;

            GraphicsPath knobPath = GenerateKnobGraphicsPath();

            g.DrawPath(basePen, knobPath);

            basePen.Color = Color.White;
            basePen.Width = (mouseOverKnob || mouseDragging ? 3 : 2);
            g.DrawPath(basePen, knobPath);

            knobPath.Dispose();
        }

        /// <summary>
        /// Generates and returns a GraphicsPath that represents the outline of the slider's area
        /// </summary>
        /// <returns>A GraphicsPath that represents the outline of the slider's area</returns>
        protected GraphicsPath GenerateSliderGraphicsPath()
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = GetSliderRectangleBounds();

            path.AddRoundedRectangle(rect, rect.Height);

            return path;
        }

        /// <summary>
        /// Generates and returns a GraphicsPath that represents the outline of the slider's knob
        /// </summary>
        /// <returns>A GraphicsPath that represents the outline of the slider's knob</returns>
        protected virtual GraphicsPath GenerateKnobGraphicsPath()
        {
            GraphicsPath path = new GraphicsPath();
            RectangleF rec = GetKnobRectangleFBounds();

            path.AddEllipse(rec);

            return path;
        }

        /// <summary>
        /// Generates and returns a LinearGradientBrush that is used to draw the slider's background area
        /// </summary>
        /// <returns>A LinearGradientBrush that is used to draw the slider's background area</returns>
        protected LinearGradientBrush GenerateSliderGradient()
        {
            ColorBlend blend;
            LinearGradientBrush brush;

            Rectangle rec = GetSliderRectangleBounds();

            // Offset the rectangle so it starts within the round corners of the graphic path
            rec.X += rec.Height / 2;
            rec.Width -= rec.Height;

            // Hue is a special case
            if (colorComponent == ColorSliderComponent.Hue)
            {
                // 7 steps chosen for the 7 colors of the spectrum. This produces a gradient that is precise enough to display the whole hue range for the slider
                const int steps = 7;

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
                brush = new LinearGradientBrush(rec, Color.White, Color.White, LinearGradientMode.Horizontal)
                {
                    InterpolationColors = blend,
                    WrapMode = WrapMode.TileFlipXY
                };
                
                return brush;
            }

            Color zeroColor = GetColorWithActiveComponentSet(0);
            Color halfColor = GetColorWithActiveComponentSet(0.5f);
            Color fullColor = GetColorWithActiveComponentSet(1);

            // Create the color blends for the brush
            blend = new ColorBlend(3)
            {
                Colors = new[] {zeroColor, halfColor, fullColor},
                Positions = new[] {0.0f, 0.5f, 1.0f}
            };

            // Create the brush
            brush = new LinearGradientBrush(rec, zeroColor, fullColor, LinearGradientMode.Horizontal)
            {
                InterpolationColors = blend,
                WrapMode = WrapMode.TileFlipXY
            };

            return brush;
        }

        /// <summary>
        /// Gets a color with the current active component set to be of the given value
        /// </summary>
        /// <param name="componentValue">The component value, ranging from [0 - 1]</param>
        /// <returns>A color with the current active component set to be of the given value</returns>
        protected Color GetColorWithActiveComponentSet(float componentValue)
        {
            AhslColor retColor = activeColor;

            switch (ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    retColor = new AhslColor(componentValue, retColor.Hf, retColor.Sf, retColor.Lf);
                    break;
                // RGB
                case ColorSliderComponent.Red:
                    retColor = AhslColor.FromArgb(retColor.Af, componentValue, retColor.Gf, retColor.Bf);
                    break;
                case ColorSliderComponent.Green:
                    retColor = AhslColor.FromArgb(retColor.Af, retColor.Rf, componentValue, retColor.Bf);
                    break;
                case ColorSliderComponent.Blue:
                    retColor = AhslColor.FromArgb(retColor.Af, retColor.Rf, retColor.Gf, componentValue);
                    break;
                // HSL
                case ColorSliderComponent.Hue:
                    retColor = new AhslColor(retColor.Af, componentValue, retColor.Sf, retColor.Lf);
                    break;
                case ColorSliderComponent.Saturation:
                    retColor = new AhslColor(retColor.Af, retColor.Hf, componentValue, retColor.Lf);
                    break;
                case ColorSliderComponent.Lightness:
                    retColor = new AhslColor(retColor.Af, retColor.Hf, retColor.Sf, componentValue);
                    break;
            }

            if (IgnoreActiveColorAlpha())
            {
                retColor = new AhslColor(1.0f, retColor.Hf, retColor.Sf, retColor.Lf);
            }

            /*switch (ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    retColor.Af = componentValue;
                    break;

                // RGB
                case ColorSliderComponent.Red:
                    retColor = AhslColor.FromArgb(retColor.Af, componentValue, retColor.Gf, retColor.Bf);
                    break;
                case ColorSliderComponent.Green:
                    retColor = AhslColor.FromArgb(retColor.Af, retColor.Rf, componentValue, retColor.Bf);
                    break;
                case ColorSliderComponent.Blue:
                    retColor = AhslColor.FromArgb(retColor.Af, retColor.Rf, retColor.Gf, componentValue);
                    break;

                // HSL
                case ColorSliderComponent.Hue:
                    retColor.Hf = componentValue;
                    break;
                case ColorSliderComponent.Saturation:
                    retColor.Sf = componentValue;
                    break;
                case ColorSliderComponent.Lightness:
                    retColor.Lf = componentValue;
                    break;
            }

            if (IgnoreActiveColorAlpha())
            {
                retColor.Af = 1.0f;
            }*/

            return retColor.ToColor();
        }

        /// <summary>
        /// Returns a Rectangle object that represents the slider's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider's bounds</returns>
        protected virtual Rectangle GetSliderRectangleBounds()
        {
            return new Rectangle(2, 19, Width - 4, 15);
        }
        
        /// <summary>
        /// Returns a Rectangle object that represents the slider knob's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider knob's bounds</returns>
        protected virtual Rectangle GetKnobRectangleBounds()
        {
            return Rectangle.Truncate(GetKnobRectangleFBounds());
        }

        /// <summary>
        /// Returns a Rectangle object that represents the slider knob's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider knob's bounds</returns>
        protected virtual RectangleF GetKnobRectangleFBounds()
        {
            float xOffset = GetSliderXOffset();
            Rectangle sliderRect = GetSliderRectangleBounds();

            RectangleF rec = new RectangleF(xOffset - sliderRect.Height / 2.0f, sliderRect.Top, sliderRect.Height, sliderRect.Height);

            return rec;
        }

        #endregion

        /// <summary>
        /// Returns whether this color slider should ignore updates to the alpha transparency of the active color and always render it as 1.0
        /// </summary>
        /// <returns>Whether this color slider should ignore updates to the alpha transparency of the active color and always render it as 1.0</returns>
        protected bool IgnoreActiveColorAlpha()
        {
            return colorComponent != ColorSliderComponent.Alpha;
        }

        //
        // SetBoundsCore override used to lock the control's height
        //
        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            // If no fixed control height is specified, allow free resizing
            if (fixedControlHeight == -1)
            {
                base.SetBoundsCore(x, y, Math.Max(80, width), height, specified);
            }

            // EDIT: ADD AN EXTRA HEIGHT VALIDATION TO AVOID INITIALIZATION PROBLEMS
            // BITWISE 'AND' OPERATION: IF ZERO THEN HEIGHT IS NOT INVOLVED IN THIS OPERATION
            if ((specified & BoundsSpecified.Height) == 0 || (specified & BoundsSpecified.Width) == 0 || height == fixedControlHeight)
            {
                base.SetBoundsCore(x, y, Math.Max(80, width), fixedControlHeight, specified);
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
    /// Event arguments for a ColorChanged event 
    /// </summary>
    public class ColorChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the original color before the component was updated
        /// </summary>
        public AhslColor OldColor { get; private set; }

        /// <summary>
        /// Gets the new color after the component was updated
        /// </summary>
        public AhslColor NewColor { get; private set; }

        /// <summary>
        /// Gets the color component that was modified
        /// </summary>
        public ColorSliderComponent ComponentChanged { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ColorChangedEventArgs class
        /// </summary>
        /// <param name="oldColor">The original color before the component was updated</param>
        /// <param name="newColor">The new color after the component was updated</param>
        /// <param name="componentChanged">The color component that was modified</param>
        public ColorChangedEventArgs(AhslColor oldColor, AhslColor newColor, ColorSliderComponent componentChanged)
        {
            OldColor = oldColor;
            NewColor = newColor;
            ComponentChanged = componentChanged;
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
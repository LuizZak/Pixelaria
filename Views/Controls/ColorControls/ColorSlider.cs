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
    [DefaultEvent("ColorChanged")]
    public partial class ColorSlider : UserControl
    {
        /// <summary>
        /// The active color for this ColorSlider
        /// </summary>
        private AHSL activeColor = Color.White.ToAHSL();

        /// <summary>
        /// The color component this ColorSlider is currently manipulating
        /// </summary>
        private ColorSliderComponent colorComponent;

        /// <summary>
        /// Whether the mouse is currently over the slider area
        /// </summary>
        private bool mouseOverSlider = false;

        /// <summary>
        /// Whether the mouse is currently dragging the knob on this ColorSlider
        /// </summary>
        private bool mouseDragging = false;

        /// <summary>
        /// An offset applied to the mouse position while dragging the knob
        /// </summary>
        private float knobDraggingOffset = 0;

        /// <summary>
        /// The current value specified by this ColorSlider, ranging from [0 - 1]
        /// </summary>
        private float currentValue = 0;

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
        public float CurrentValue
        {
            get
            {
                return currentValue;
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
        public AHSL ActiveColor
        {
            get
            {
                return activeColor;
            }
            set
            {
                activeColor = value;
                this.SetActiveColor(activeColor);
                this.RecalculateValue();
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
                RectangleF bounds = this.GenerateKnobGraphicsPath().GetBounds();
                bounds.Inflate(2, 2);
                knobDraggingOffset = 0;

                if (bounds.Contains(e.Location))
                {
                    knobDraggingOffset = e.Location.X - (bounds.Right + bounds.Left) / 2;
                }

                UpdateValueForMouseEvent(e);
                mouseDragging = true;
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
                    InvalidateKnob();
                    InvalidateSlider();
                }
                else if (!GetSliderRectangleBounds().Contains(e.Location))
                {
                    mouseOverSlider = false;
                    InvalidateKnob();
                    InvalidateSlider();
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
                InvalidateKnob();
                InvalidateSlider();
            }
        }

        /// <summary>
        /// Updates the value of this slider based on a given mouse event args. The arguments are used to get the
        /// position of the mouse during the event, and assign a value based on this position relative to the slider
        /// </summary>
        /// <param name="e">The mouse event args to use to manipulate the mouse</param>
        private void UpdateValueForMouseEvent(MouseEventArgs e)
        {
            float value = GetValueForXOffset(e.X - (int)knobDraggingOffset);

            SetColorComponentValue(value);
        }

        #endregion

        #region TextBox input handling

        bool ignoreTextField = false;
        //
        // Value rich text box text changed
        //
        private void rtb_value_TextChanged(object sender, EventArgs e)
        {
            if (ignoreTextField)
                return;

            int rawValue = 0;
            int maxValue = GetColorComponentMaxValue();
            float value = 0;
            string valueString = rtb_value.Text;

            // If the caret is not at the end of the current value string, trim the digits that are out of the range
            if (rtb_value.SelectionStart != rtb_value.TextLength)
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
        /// Recalculates this color slider's value property from the current
        /// color's composition
        /// </summary>
        private void RecalculateValue()
        {
            int selectionStart = this.rtb_value.SelectionStart;

            this.currentValue = GetColorComponentValue();
            ignoreTextField = true;
            this.rtb_value.Text = "" + GetColorComponentValueRaw();
            ignoreTextField = false;
            this.rtb_value.SelectionStart = Math.Min(this.rtb_value.TextLength, selectionStart);
        }

        /// <summary>
        /// Sets a given color as the current active color of this ColorSlider
        /// </summary>
        /// <param name="color">The color to set as the new active color for this ColorSlider</param>
        private void SetActiveColor(AHSL color)
        {
            this.activeColor = color;

            Rectangle invalidateRect = GetSliderRectangleBounds();
            invalidateRect.Height = this.Height - invalidateRect.Top;

            this.Invalidate(invalidateRect);
            this.InvalidateKnob();
        }

        /// <summary>
        /// Returns a float value ranging from [0 - 1] that indicates the value represented by a given X offset of the slider
        /// </summary>
        /// <param name="xOffset">An X offset of the slider's total size</param>
        /// <returns>A float value ranging from [0 - 1] that indicates the value represented by a given X offset of the slider</returns>
        private float GetValueForXOffset(int xOffset)
        {
            float value = 0;

            // Get the slider rectangle and move it to offset 0
            Rectangle rect = GetSliderRectangleBounds();

            // Move the offset by the ammount the rectangle was moved too
            xOffset -= rect.X + rect.Height / 2;

            rect.X = 0;
            rect.Width -= rect.Height;

            value = (float)(xOffset) / rect.Width;

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
        private int GetSliderXOffset()
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
            switch (this.ColorComponent)
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
            AHSL oldColor = activeColor;

            currentValue = value;

            AHSL newColor = activeColor;

            switch (this.ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    newColor.Af = value;
                    break;
                // RGB
                case ColorSliderComponent.Red:
                    newColor = AHSL.FromArgb(activeColor.Af, value, activeColor.Gf, activeColor.Bf);
                    break;
                case ColorSliderComponent.Green:
                    newColor = AHSL.FromArgb(activeColor.Af, activeColor.Rf, value, activeColor.Bf);
                    break;
                case ColorSliderComponent.Blue:
                    newColor = AHSL.FromArgb(activeColor.Af, activeColor.Rf, activeColor.Gf, value);
                    break;
                // HSL
                case ColorSliderComponent.Hue:
                    newColor.Hf = value;
                    break;
                case ColorSliderComponent.Saturation:
                    newColor.Sf = value;
                    break;
                case ColorSliderComponent.Lightness:
                    newColor.Lf = value;
                    break;
            }

            SetActiveColor(newColor);

            // Fire the event now
            if (this.ColorChanged != null)
            {
                this.ColorChanged(this, new ColorChangedEventArgs(oldColor, activeColor, this.colorComponent));
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
            switch (this.ColorComponent)
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
        /// Invalidates the region associated with this slider's knob
        /// </summary>
        private void InvalidateKnob()
        {
            RectangleF bounds = this.GenerateKnobGraphicsPath().GetBounds();
            bounds.Inflate(2, 2);

            this.Invalidate(new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height));
        }

        /// <summary>
        /// Invalidates the region associated with this slider's slider rectangle
        /// </summary>
        private void InvalidateSlider()
        {
            RectangleF bounds = this.GetSliderRectangleBounds();
            bounds.Inflate(2, 2);

            this.Invalidate(new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height));
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

            // Get the fill brush
            LinearGradientBrush fillBrush = GenerateSliderGradient();

            // Iterate through the colors and check if there's any that isn't fully opaque
            foreach (Color color in fillBrush.InterpolationColors.Colors)
            {
                if (color.A != 255)
                {
                    // Draw a background
                    Brush backBrush = new TextureBrush(ImageUtilities.GetDefaultTile());
                    g.FillPath(backBrush, path);
                    backBrush.Dispose();
                    break;
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

            GraphicsPath knobPath = this.GenerateKnobGraphicsPath();

            g.DrawPath(basePen, knobPath);

            basePen.Color = Color.White;
            basePen.Width = (mouseOverSlider || mouseDragging ? 3 : 2);
            g.DrawPath(basePen, knobPath);

            knobPath.Dispose();
        }

        /// <summary>
        /// Generates and returns a GraphicsPath that represents the outline of the slider's area
        /// </summary>
        /// <returns>A GraphicsPath that represents the outline of the slider's area</returns>
        private GraphicsPath GenerateSliderGraphicsPath()
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
        private GraphicsPath GenerateKnobGraphicsPath()
        {
            float xOffset = GetSliderXOffset();

            Rectangle sliderRect = GetSliderRectangleBounds();

            GraphicsPath knobPath = new GraphicsPath();
            knobPath.AddEllipse(new RectangleF(xOffset - sliderRect.Height / 2, sliderRect.Top, sliderRect.Height, sliderRect.Height));

            return knobPath;
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
            AHSL retColor = this.activeColor;

            switch (this.ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    retColor.Af = componentValue;
                    break;

                // RGB
                case ColorSliderComponent.Red:
                    retColor = AHSL.FromArgb(retColor.Af, componentValue, retColor.Gf, retColor.Bf);
                    break;
                case ColorSliderComponent.Green:
                    retColor = AHSL.FromArgb(retColor.Af, retColor.Rf, componentValue, retColor.Bf);
                    break;
                case ColorSliderComponent.Blue:
                    retColor = AHSL.FromArgb(retColor.Af, retColor.Rf, retColor.Gf, componentValue);
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

            return retColor.ToColor();
        }

        /// <summary>
        /// Returns a Rectangle object that represents the slider's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider's bounds</returns>
        private Rectangle GetSliderRectangleBounds()
        {
            return new Rectangle(2, 19, this.Width - 4, 15);
        }
        
        /// <summary>
        /// Returns a Rectangle object that represents the slider knob's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider knob's bounds</returns>
        private Rectangle GetKnobRectangleBounds()
        {
            RectangleF bounds = this.GenerateKnobGraphicsPath().GetBounds();
            bounds.Inflate(4, 0);

            return new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
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
    /// Event arguments for a ColorChanged event 
    /// </summary>
    public class ColorChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the original color before the component was updated
        /// </summary>
        public AHSL OldColor { get; private set; }

        /// <summary>
        /// Gets the new color after the component was updated
        /// </summary>
        public AHSL NewColor { get; private set; }

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
        public ColorChangedEventArgs(AHSL oldColor, AHSL newColor, ColorSliderComponent componentChanged)
        {
            this.OldColor = oldColor;
            this.NewColor = newColor;
            this.ComponentChanged = componentChanged;
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
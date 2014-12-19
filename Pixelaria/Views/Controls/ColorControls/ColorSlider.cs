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
        private AHSL _activeColor = Color.White.ToAHSL();

        /// <summary>
        /// The color component this ColorSlider is currently manipulating
        /// </summary>
        private ColorSliderComponent _colorComponent;

        /// <summary>
        /// Whether the mouse is currently over the slider area
        /// </summary>
        private bool _mouseOverSlider;
        
        /// <summary>
        /// Whether the mouse is currently over the knob area
        /// </summary>
        private bool _mouseOverKnob;

        /// <summary>
        /// Whether the mouse is currently dragging the knob on this ColorSlider
        /// </summary>
        private bool _mouseDragging;

        /// <summary>
        /// An offset applied to the mouse position while dragging the knob
        /// </summary>
        private int _knobDraggingOffset;

        /// <summary>
        /// The current value specified by this ColorSlider, ranging from [0 - 1]
        /// </summary>
        private float _currentValue;

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
                return _colorComponent;
            }
            set
            {
                _colorComponent = value;
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
                return _currentValue;
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
                return _activeColor;
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
                Rectangle bounds = GetKnobRectangleBounds();
                
                _knobDraggingOffset = 0;

                if (bounds.Contains(e.Location))
                {
                    _knobDraggingOffset = e.Location.X - (bounds.Right + bounds.Left) / 2;
                }

                UpdateValueForMouseEvent(e);
                _mouseDragging = true;
            }
        }

        // 
        // OnMouseMove event handler
        //
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_mouseDragging)
            {
                UpdateValueForMouseEvent(e);
            }
            else
            {
                if (GetSliderRectangleBounds().Contains(e.Location) && !_mouseOverSlider)
                {
                    _mouseOverSlider = true;
                    InvalidateSlider();
                }
                else if (!GetSliderRectangleBounds().Contains(e.Location))
                {
                    if(_mouseOverSlider)
                    {
                        _mouseOverSlider = false;
                        InvalidateSlider();
                    }
                }

                if (GetKnobRectangleBounds().Contains(e.Location) && !_mouseOverKnob)
                {
                    _mouseOverKnob = true;
                    InvalidateKnob();
                }
                else if (!GetKnobRectangleBounds().Contains(e.Location))
                {
                    if (_mouseOverKnob)
                    {
                        _mouseOverKnob = false;
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

            if (_mouseDragging)
            {
                InvalidateSlider();
                _mouseDragging = false;
            }
        }

        //
        // OnMouseLeave event handler
        //
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_mouseOverSlider)
            {
                _mouseOverSlider = false;
                InvalidateSlider();
            }

            if (_mouseOverKnob)
            {
                _mouseOverKnob = false;
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
            float value = GetValueForXOffset(e.X - _knobDraggingOffset);

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
            int selectionStart = rtb_value.SelectionStart;

            _currentValue = GetColorComponentValue();
            _ignoreTextField = true;
            rtb_value.Text = "" + GetColorComponentValueRaw();
            _ignoreTextField = false;
            rtb_value.SelectionStart = Math.Min(rtb_value.TextLength, selectionStart);
        }

        /// <summary>
        /// Sets a given color as the current active color of this ColorSlider
        /// </summary>
        /// <param name="color">The color to set as the new active color for this ColorSlider</param>
        private void SetActiveColor(AHSL color)
        {
            // Check if any redraw is required
            if (IgnoreActiveColorAlpha())
            {
                if (Math.Abs(_activeColor.Hf - color.Hf) < float.Epsilon && Math.Abs(_activeColor.Sf - color.Sf) < float.Epsilon && Math.Abs(_activeColor.Lf - color.Lf) < float.Epsilon)
                {
                    _activeColor = color;

                    return;
                }
            }

            _activeColor = color;

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
        private float GetValueForXOffset(int xOffset)
        {
            float value;

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

            return (int)(rec.Left + rec.Height / 2 + _currentValue * (rec.Width - rec.Height));
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
                    return _activeColor.Af;
                // RGB
                case ColorSliderComponent.Red:
                    return _activeColor.Rf;
                case ColorSliderComponent.Green:
                    return _activeColor.Gf;
                case ColorSliderComponent.Blue:
                    return _activeColor.Bf;
                // HSL
                case ColorSliderComponent.Hue:
                    return _activeColor.Hf;
                case ColorSliderComponent.Saturation:
                    return _activeColor.Sf;
                case ColorSliderComponent.Lightness:
                    return _activeColor.Lf;

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
            AHSL oldColor = _activeColor;

            _currentValue = value;

            AHSL newColor = _activeColor;

            switch (ColorComponent)
            {
                // Global alpha channel
                case ColorSliderComponent.Alpha:
                    newColor.Af = value;
                    break;
                // RGB
                case ColorSliderComponent.Red:
                    newColor = AHSL.FromArgb(_activeColor.Af, value, _activeColor.Gf, _activeColor.Bf);
                    break;
                case ColorSliderComponent.Green:
                    newColor = AHSL.FromArgb(_activeColor.Af, _activeColor.Rf, value, _activeColor.Bf);
                    break;
                case ColorSliderComponent.Blue:
                    newColor = AHSL.FromArgb(_activeColor.Af, _activeColor.Rf, _activeColor.Gf, value);
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
            if (ColorChanged != null)
            {
                ColorChanged(this, new ColorChangedEventArgs(oldColor, _activeColor, _colorComponent));
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
                    return _activeColor.A;
                case ColorSliderComponent.Red:
                    return _activeColor.R;
                case ColorSliderComponent.Green:
                    return _activeColor.G;
                case ColorSliderComponent.Blue:
                    return _activeColor.B;
                case ColorSliderComponent.Hue:
                    return _activeColor.H;
                case ColorSliderComponent.Saturation:
                    return _activeColor.S;
                case ColorSliderComponent.Lightness:
                    return _activeColor.L;

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
            Pen pen = (Pen)(_mouseOverSlider ? (_mouseDragging ? Pens.Black : Pens.Gray) : Pens.DarkGray).Clone();
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
            basePen.Width = (_mouseOverKnob || _mouseDragging ? 3 : 2);
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
            knobPath.AddEllipse(new RectangleF(xOffset - sliderRect.Height / 2.0f, sliderRect.Top, sliderRect.Height, sliderRect.Height));

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
            if (_colorComponent == ColorSliderComponent.Hue)
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
            blend.Colors = new [] { zeroColor, halfColor, fullColor };
            blend.Positions = new [] { 0.0f, 0.5f, 1.0f };

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
            AHSL retColor = _activeColor;

            switch (ColorComponent)
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

            if (IgnoreActiveColorAlpha())
            {
                retColor.Af = 1.0f;
            }

            return retColor.ToColor();
        }

        /// <summary>
        /// Returns a Rectangle object that represents the slider's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider's bounds</returns>
        private Rectangle GetSliderRectangleBounds()
        {
            return new Rectangle(2, 19, Width - 4, 15);
        }
        
        /// <summary>
        /// Returns a Rectangle object that represents the slider knob's bounds
        /// </summary>
        /// <returns>A Rectangle object that represents the slider knob's bounds</returns>
        private Rectangle GetKnobRectangleBounds()
        {
            RectangleF bounds = GenerateKnobGraphicsPath().GetBounds();
            bounds.Inflate(4, 0);

            return new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
        }

        #endregion

        /// <summary>
        /// Returns whether this color slider should ignore updates to the alpha transparency of the active color and always render it as 1.0
        /// </summary>
        /// <returns>Whether this color slider should ignore updates to the alpha transparency of the active color and always render it as 1.0</returns>
        protected bool IgnoreActiveColorAlpha()
        {
            return _colorComponent != ColorSliderComponent.Alpha;
        }

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
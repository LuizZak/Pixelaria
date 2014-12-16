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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Represents a Numerid Up and Down control that has an assisting
    /// bar bellow for quick value picking
    /// </summary>
    [DefaultEvent("ValueChanged")]
    [Description("Represents a Numerid Up and Down control that has an assisting bar bellow for quick value picking")]
    public partial class AssistedNumericUpDown : UserControl
    {
        /// <summary>
        /// Whether the mouse is currently being held down on this control
        /// </summary>
        private bool mouseDown;

        /// <summary>
        /// The color to fill the assist bar with
        /// </summary>
        private Color assistBarColor;
        
        /// <summary>
        /// Gets or sets the minimum value for the assisted numeric up down
        /// </summary>
        [Browsable(true)]
        [Category("Data")]
        [Description("The minimum value for the assisted numeric up down")]
        public decimal Minimum { get { return nud_controlNud.Minimum; } set { if (nud_controlNud.Minimum != value) { Invalidate(); } nud_controlNud.Minimum = value; } }

        /// <summary>
        /// Gets or sets the maximum value for the assisted numeric up down
        /// </summary>
        [Browsable(true)]
        [Category("Data")]
        [Description("The maximum value for the assisted numeric up down")]
        public decimal Maximum { get { return nud_controlNud.Maximum; } set { if (nud_controlNud.Maximum != value) { Invalidate(); } nud_controlNud.Maximum = value; } }

        /// <summary>
        /// Gets or sets the value to be incremented when the up or down arrows are clicked
        /// </summary>
        [Browsable(true)]
        [Category("Data")]
        [Description("The value to be incremented when the up or down arrows are clicked")]
        public decimal Increment { get { return nud_controlNud.Increment; } set { nud_controlNud.Increment = value; } }

        /// <summary>
        /// Gets or sets the current value for the assisted numeric up down
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("The current value for the assisted numeric up down")]
        public decimal Value
        {
            get { return nud_controlNud.Value; }
            set
            {
                if (nud_controlNud.Value != value)
                {
                    decimal oldValue = nud_controlNud.Value;

                    if (!AllowDecimalOnMouse)
                    {
                        value = Math.Floor(value);
                    }
                    else
                    {
                        // Clamp the mouse value to the increments
                        value = Math.Floor(value / Increment) * Increment;
                    }

                    value = Math.Min(Maximum, Math.Max(Minimum, value));

                    mouseDown = true;
                    nud_controlNud.Value = value;
                    mouseDown = false;

                    if (oldValue > value)
                    {
                        decimal temp = oldValue;
                        oldValue = value;
                        value = temp;
                    }

                    Rectangle invalidateBox = new Rectangle((int)((oldValue - Minimum) / (Maximum - Minimum) * Width) - 1, nud_controlNud.Height, (int)((value - Minimum) / (Maximum - Minimum) * Width) + 1, Height - nud_controlNud.Height);
                    this.Invalidate(invalidateBox);
                }
            }
        }

        /// <summary>
        /// Gets or sets the number of decimal places to display in the assisted numeric up down
        /// </summary>
        [Category("Data")]
        [DefaultValue(0)]
        [Description("the number of decimal places to display in the assisted numeric up down")]
        public int DecimalPlaces { get { return nud_controlNud.DecimalPlaces; } set { nud_controlNud.DecimalPlaces = value; } } 

        /// <summary>
        /// Gets or sets whether to allow the precise mouse input to produce decimal values.
        /// Setting to false will round down all inputs from the mouse precise input
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(false)]
        [Description("whether to allow the precise mouse input to produce decimal values.\nSetting to false will round down all inputs from the mouse precise input")]
        public bool AllowDecimalOnMouse { get; set; }

        /// <summary>
        /// Gets or sets the color to fill the assist bar with
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [Description("The color to fill the assist bar with")]
        public Color AssistBarColor { get { return assistBarColor; } set { if(assistBarColor != value) { assistBarColor = value; Invalidate(); } } }

        /// <summary>
        /// Occurs when the Value property has been changed in some way
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs when the Value property has been changed in some way.")]
        public event EventHandler ValueChanged
        {
            add
            {
                nud_controlNud.ValueChanged += value;
            }

            remove
            {
                nud_controlNud.ValueChanged -= value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AssistedNumericUpDown class
        /// </summary>
        public AssistedNumericUpDown()
        {
            InitializeComponent();

            assistBarColor = Color.CornflowerBlue;
        }

        // 
        // Control NUD value changed
        // 
        private void nud_controlNud_ValueChanged(object sender, EventArgs e)
        {
            if (!mouseDown)
                Invalidate();
        }

        // 
        // OnPaint event handler
        // 
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Draw the value range
            Rectangle rangeBox = new Rectangle(0, nud_controlNud.Height, (int)((Value - Minimum) / (Maximum - Minimum) * Width), Height - nud_controlNud.Height);

            if (rangeBox.Width > 0)
            {
                Brush drawBrush = Brushes.CornflowerBlue;

                LinearGradientBrush gradientBrush = new LinearGradientBrush(rangeBox, assistBarColor, assistBarColor, 90);

                e.Graphics.FillRectangle(gradientBrush, rangeBox);
            }
        }

        // 
        // OnMouseDown event handler
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            mouseDown = true;

            decimal value = Minimum + ((Maximum - Minimum) * ((decimal)Math.Max(0, Math.Min(Width, e.X)) / Width));

            if (!AllowDecimalOnMouse)
            {
                value = Math.Round(value);
            }
            else
            {
                // Clamp the mouse value to the increments
                value = Math.Round(value / Increment) * Increment;
            }

            value = Math.Min(Maximum, Math.Max(Minimum, value));

            decimal oldValue = nud_controlNud.Value;

            nud_controlNud.Value = value;

            if (oldValue > value)
            {
                decimal temp = oldValue;
                oldValue = value;
                value = temp;
            }

            Rectangle invalidateBox = new Rectangle((int)((oldValue - Minimum) / (Maximum - Minimum) * Width) - 1, nud_controlNud.Height, (int)((value - Minimum) / (Maximum - Minimum) * Width) + 1, Height - nud_controlNud.Height);
            this.Invalidate(invalidateBox);
        }

        // 
        // OnMouseMove event handler
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (mouseDown)
            {
                decimal value = Minimum + ((Maximum - Minimum) * ((decimal)Math.Max(0, Math.Min(Width, e.X)) / Width));
                decimal oldValue = nud_controlNud.Value;

                if (!AllowDecimalOnMouse)
                {
                    value = Math.Round(value);
                }
                else
                {
                    // Clamp the mouse value to the increments
                    value = Math.Round(value / Increment) * Increment;
                }

                value = Math.Min(Maximum, Math.Max(Minimum, value));

                nud_controlNud.Value = value;

                if (oldValue > value)
                {
                    decimal temp = oldValue;
                    oldValue = value;
                    value = temp;
                }

                Rectangle invalidateBox = new Rectangle((int)((oldValue - Minimum) / (Maximum - Minimum) * Width) - 1, nud_controlNud.Height, (int)((value - Minimum) / (Maximum - Minimum) * Width) + 1, Height - nud_controlNud.Height);
                this.Invalidate(invalidateBox);
            }
        }

        // 
        // OnMouseUp event handler
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            mouseDown = false;
        }
    }
}
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
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Pixelaria.Utils;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// A control where the user can pick colors
    /// </summary>
    [DefaultEvent("ColorPick")]
    public partial class ColorPicker : UserControl
    {
        /// <summary>
        /// The first color of the control
        /// </summary>
        private Color firstColor;

        /// <summary>
        /// The second color of the control
        /// </summary>
        private Color secondColor;

        /// <summary>
        /// Gets or sets the currently selected color
        /// </summary>
        private ColorPickerColor selectedColor;

        /// <summary>
        /// Whether the mouse is currently held down on the palette bitmap
        /// </summary>
        private bool mouseDown;

        /// <summary>
        /// Delegate for a ColorPick event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void ColorPickEventHandler(object sender, ColorPickEventArgs eventArgs);

        /// <summary>
        /// Occurs whenever the user changes the currently selected color
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user changes the currently selected color")]
        public event ColorPickEventHandler ColorPick;

        /// <summary>
        /// Gets or sets the first color of the control
        /// </summary>
        public Color FirstColor { get { return firstColor; } set { firstColor = value; pnl_firstColor.BackColor = value; if (SelectedColor == ColorPickerColor.FirstColor) UpdateSliders(); } }

        /// <summary>
        /// Gets or sets the second color of the control
        /// </summary>
        public Color SecondColor { get { return secondColor; } set { secondColor = value; pnl_secondColor.BackColor = value; if (SelectedColor == ColorPickerColor.SecondColor) UpdateSliders(); } }

        /// <summary>
        /// Gets or sets which color the ColorPicker is supposed to be currently displaying
        /// </summary>
        public ColorPickerColor SelectedColor
        {
            get { return selectedColor; }
            set
            {
                selectedColor = value;

                Color color = firstColor;

                switch (selectedColor)
                {
                    case ColorPickerColor.FirstColor:
                        pnl_firstColor.BorderStyle = BorderStyle.Fixed3D;
                        pnl_secondColor.BorderStyle = BorderStyle.FixedSingle;
                        color = firstColor;
                        break;
                    case ColorPickerColor.SecondColor:
                        pnl_firstColor.BorderStyle = BorderStyle.FixedSingle;
                        pnl_secondColor.BorderStyle = BorderStyle.Fixed3D;
                        color = secondColor;
                        break;
                }

                // Update the color components sliders
                anud_transparency.Value = color.A;
                anud_redComonent.Value = color.R;
                anud_greenComponent.Value = color.G;
                anud_blueComponent.Value = color.B;
            }
        }

        /// <summary>
        /// Initializes a new instance of the ColorPicker class
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();

            firstColor = Color.Black;
            secondColor = Color.White;

            pnl_firstColor.BackColor = firstColor;
            pnl_secondColor.BackColor = secondColor;
        }

        int updateSliders = -1;
        /// <summary>
        /// Sets the currently selected color
        /// </summary>
        /// <param name="color">The new value for the currently selected color</param>
        /// <param name="keepTransparency">Whether to keep the current alpha channel unmodified</param>
        public void SetCurrentColor(Color color, bool keepTransparency = false)
        {
            Color oldColor = Color.White;

            if (keepTransparency)
            {
                color = Color.FromArgb(GetCurrentColor().A, color.R, color.G, color.B);
            }

            switch (selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    oldColor = firstColor;
                    FirstColor = color;
                    break;
                case ColorPickerColor.SecondColor:
                    oldColor = secondColor;
                    SecondColor = color;
                    break;
            }

            UpdateSliders();

            if (ColorPick != null)
            {
                ColorPick.Invoke(this, new ColorPickEventArgs(oldColor, color, selectedColor));
            }
        }

        /// <summary>
        /// Updates the color sliders
        /// </summary>
        public void UpdateSliders()
        {
            Color color = Color.White;

            switch (selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    color = firstColor;
                    break;
                case ColorPickerColor.SecondColor:
                    color = secondColor;
                    break;
            }

            anud_transparency.Value = color.A;

            if (updateSliders != 0)
            {
                // Update the color components sliders
                anud_redComonent.Value = color.R;
                anud_greenComponent.Value = color.G;
                anud_blueComponent.Value = color.B;
            }
            if (updateSliders != 1)
            {
                AHSL ahsl = color.ToAHSL();

                anud_h.Value = ahsl.H;
                anud_s.Value = ahsl.S;
                anud_l.Value = ahsl.L;
            }
        }

        /// <summary>
        /// Gets the currently selected color
        /// </summary>
        /// <returns>The value for the currently selected color</returns>
        public Color GetCurrentColor()
        {
            switch (selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    return FirstColor;
                case ColorPickerColor.SecondColor:
                    return SecondColor;
            }

            return Color.White;
        }

        /// <summary>
        /// Updates the current color using the RGB components to get the new color value
        /// </summary>
        private void UpdateColorRGB()
        {
            if (updateSliders != -1)
                return;

            Color c = Color.FromArgb((int)anud_transparency.Value, (int)anud_redComonent.Value, (int)anud_greenComponent.Value, (int)anud_blueComponent.Value);

            updateSliders = 0;
            SetCurrentColor(c);
            updateSliders = -1;
        }

        /// <summary>
        /// Updates the current color using the HSL components to get the new color value
        /// </summary>
        private void UpdateColorHSL()
        {
            if (updateSliders != -1)
                return;

            Color c = new AHSL((int)anud_transparency.Value, (int)anud_h.Value, (int)anud_s.Value, (int)anud_l.Value).ToARGB();

            updateSliders = 1;
            SetCurrentColor(c);
            updateSliders = -1;
        }

        // 
        // First Color Panel mouse down event handler
        // 
        private void pnl_firstColor_MouseDown(object sender, MouseEventArgs e)
        {
            this.SelectedColor = ColorPickerColor.FirstColor;
        }

        // 
        // Second Color Panel mouse down event handler
        // 
        private void pnl_secondColor_MouseDown(object sender, MouseEventArgs e)
        {
            this.SelectedColor = ColorPickerColor.SecondColor;
        }

        // 
        // Palette Bitmap mouse down
        // 
        private void pb_palette_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;

            SelectColorOnPalette(e.Location);
        }

        // 
        // Palette Bitmap mouse move
        // 
        private void pb_palette_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                SelectColorOnPalette(e.Location);
            }
        }

        // 
        // Palette Bitmap mouse up
        // 
        private void pb_palette_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        // 
        // Transparency NUD value changed event handler
        // 
        private void anud_transparency_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorRGB();
        }

        // 
        // Red NUD value changed event handler
        // 
        private void anud_redComonent_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorRGB();
        }
        // 
        // Green NUD value changed event handler
        // 
        private void anud_greenComponent_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorRGB();
        }
        // 
        // Blue NUD value changed event handler
        // 
        private void anud_blueComponent_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorRGB();
        }

        // 
        // Hue NUD value changed event handler
        // 
        private void anud_h_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorHSL();
        }
        // 
        // Saturation NUD value changed event handler
        // 
        private void anud_s_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorHSL();
        }
        // 
        // Lightness NUD value changed event handler
        // 
        private void anud_l_ValueChanged(object sender, EventArgs e)
        {
            UpdateColorHSL();
        }

        /// <summary>
        /// Sets the currently highlighted color to be the color on the given position on the palette bitmap
        /// </summary>
        /// <param name="point">The point to get the color at the palette</param>
        private void SelectColorOnPalette(Point point)
        {
            if (point.X < 0 || point.Y < 0 || point.X >= pb_palette.Width || point.Y >= pb_palette.Height)
                return;

            point.X = (int)(point.X / ((float)pb_palette.Width / pb_palette.Image.Width));
            point.Y = (int)(point.Y / ((float)pb_palette.Height / pb_palette.Image.Height));

            Color color = (pb_palette.Image as Bitmap).GetPixel(point.X, point.Y);

            SetCurrentColor(color, true);
        }
    }

    /// <summary>
    /// Event arguments for a ColorPick event
    /// </summary>
    public class ColorPickEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the color value before the change
        /// </summary>
        public Color OldColor { get; private set; }

        /// <summary>
        /// Gets the new color value
        /// </summary>
        public Color NewColor { get; private set; }

        /// <summary>
        /// Gets the ColorPicker color index that was changed
        /// </summary>
        public ColorPickerColor TargetColor { get; private set; }

        /// <summary>
        /// Initializes a new instance of the using Pixelaria.Views.ModelViews;
        /// </summary>
        /// <param name="oldColor">Gets the color value before the change</param>
        /// <param name="penColor">Gets the new color value</param>
        /// <param name="targetColor">Gets the ColorPicker color index that was changed</param>
        public ColorPickEventArgs(Color oldColor, Color newColor, ColorPickerColor targetColor)
        {
            this.OldColor = oldColor;
            this.NewColor = newColor;
            this.TargetColor = targetColor;
        }
    }

    /// <summary>
    /// Specifies one of the two colors on a ColorPicker control
    /// </summary>
    public enum ColorPickerColor
    {
        /// <summary>
        /// The first color of a ColorPicker
        /// </summary>
        FirstColor,
        /// <summary>
        /// The second color of a ColorPicker
        /// </summary>
        SecondColor,
        /// <summary>
        /// The currently selected color of a ColorPicker
        /// </summary>
        CurrentColor
    }
}
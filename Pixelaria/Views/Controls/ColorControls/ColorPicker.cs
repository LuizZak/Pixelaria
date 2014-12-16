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
using System.Windows.Forms;
using Pixelaria.Utils;

namespace Pixelaria.Views.Controls.ColorControls
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
        private AHSL firstColor;

        /// <summary>
        /// The second color of the control
        /// </summary>
        private AHSL secondColor;

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
        public Color FirstColor
        {
            get { return firstColor.ToColor(); }
            set
            {
                if (firstColor.ToColor() == value)
                    return;

                firstColor = value.ToAHSL();
                pnl_firstColor.BackColor = value;
                if (SelectedColor == ColorPickerColor.FirstColor)
                    UpdateSliders();
            }
        }

        /// <summary>
        /// Gets or sets the second color of the control
        /// </summary>
        public Color SecondColor
        {
            get { return secondColor.ToColor(); }
            set
            {
                if (secondColor.ToColor() == value)
                    return;

                secondColor = value.ToAHSL();
                pnl_secondColor.BackColor = value;
                if (SelectedColor == ColorPickerColor.SecondColor)
                    UpdateSliders();
            }
        }

        /// <summary>
        /// Gets or sets the first color of the control in AHSL format
        /// </summary>
        public AHSL FirstAHSLColor
        {
            get { return firstColor; }
            set
            {
                if (firstColor == value)
                    return;

                firstColor = value;
                pnl_firstColor.BackColor = value.ToColor();
                if (SelectedColor == ColorPickerColor.FirstColor)
                    UpdateSliders();
            }
        }

        /// <summary>
        /// Gets or sets the second color of the control in AHSL format
        /// </summary>
        public AHSL SecondAHSLColor
        {
            get { return secondColor; }
            set
            {
                if (firstColor == value)
                    return;

                secondColor = value;
                pnl_secondColor.BackColor = value.ToColor();
                if (SelectedColor == ColorPickerColor.SecondColor)
                    UpdateSliders();
            }
        }

        /// <summary>
        /// Gets or sets which color the ColorPicker is supposed to be currently displaying
        /// </summary>
        public ColorPickerColor SelectedColor
        {
            get { return selectedColor; }
            set
            {
                selectedColor = value;

                Color color = FirstColor;

                switch (selectedColor)
                {
                    case ColorPickerColor.FirstColor:
                        pnl_firstColor.BorderStyle = BorderStyle.Fixed3D;
                        pnl_secondColor.BorderStyle = BorderStyle.FixedSingle;
                        color = FirstColor;
                        break;
                    case ColorPickerColor.SecondColor:
                        pnl_firstColor.BorderStyle = BorderStyle.FixedSingle;
                        pnl_secondColor.BorderStyle = BorderStyle.Fixed3D;
                        color = SecondColor;
                        break;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the ColorPicker class
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();

            firstColor = Color.Black.ToAHSL();
            secondColor = Color.White.ToAHSL();

            pnl_firstColor.BackColor = FirstColor;
            pnl_secondColor.BackColor = SecondColor;

            // Hookup the events
            this.cs_alpha.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
            this.cs_red.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
            this.cs_green.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
            this.cs_blue.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
            this.cs_hue.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
            this.cs_saturation.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
            this.cs_lightness.ColorChanged += new ColorControls.ColorSlider.ColorChangedEventHandler(cs_colorChanged);
        }

        /// <summary>
        /// Sets the currently selected color
        /// </summary>
        /// <param name="color">The new value for the currently selected color</param>
        /// <param name="keepTransparency">Whether to keep the current alpha channel unmodified</param>
        public void SetCurrentColor(Color color, bool keepTransparency = false)
        {
            SetCurrentColor(color.ToAHSL(), keepTransparency);
        }

        /// <summary>
        /// Sets the currently selected color
        /// </summary>
        /// <param name="color">The new value for the currently selected color</param>
        /// <param name="keepTransparency">Whether to keep the current alpha channel unmodified</param>
        public void SetCurrentColor(AHSL color, bool keepTransparency = false)
        {
            AHSL oldColor = Color.White.ToAHSL();

            if (keepTransparency)
            {
                color.A = GetCurrentColor().A;
            }

            switch (selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    oldColor = firstColor;
                    FirstAHSLColor = color;
                    break;
                case ColorPickerColor.SecondColor:
                    oldColor = secondColor;
                    SecondAHSLColor = color;
                    break;
            }

            UpdateSliders();

            if (ColorPick != null)
            {
                ColorPick(this, new ColorPickEventArgs(oldColor.ToColor(), color.ToColor(), selectedColor));
            }
        }

        /// <summary>
        /// Updates the color sliders
        /// </summary>
        public void UpdateSliders()
        {
            AHSL color = Color.White.ToAHSL();

            switch (selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    color = firstColor;
                    break;
                case ColorPickerColor.SecondColor:
                    color = secondColor;
                    break;
            }

            // Global alpha channel
            this.cs_alpha.ActiveColor = color;
            // RGB
            this.cs_red.ActiveColor = color;
            this.cs_green.ActiveColor = color;
            this.cs_blue.ActiveColor = color;
            // HSL
            this.cs_hue.ActiveColor = color;
            this.cs_saturation.ActiveColor = color;
            this.cs_lightness.ActiveColor = color;
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
        // Color Sliders component changed event handler
        //
        private void cs_colorChanged(object sender, ColorControls.ColorChangedEventArgs eventArgs)
        {
            this.SetCurrentColor(eventArgs.NewColor);
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
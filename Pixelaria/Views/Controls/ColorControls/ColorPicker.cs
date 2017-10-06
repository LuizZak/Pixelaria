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
using JetBrains.Annotations;
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
        private AhslColor _firstColor;

        /// <summary>
        /// The second color of the control
        /// </summary>
        private AhslColor _secondColor;

        /// <summary>
        /// Gets or sets the currently selected color
        /// </summary>
        private ColorPickerColor _selectedColor;

        /// <summary>
        /// Whether the mouse is currently held down on the palette bitmap
        /// </summary>
        private bool _mouseDown;

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
            get => _firstColor.ToColor();
            set
            {
                if (_firstColor.ToColor().ToArgb() == value.ToArgb())
                    return;

                _firstColor = value.ToAhsl();
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
            get => _secondColor.ToColor();
            set
            {
                if (_secondColor.ToColor().ToArgb() == value.ToArgb())
                    return;

                _secondColor = value.ToAhsl();
                pnl_secondColor.BackColor = value;
                if (SelectedColor == ColorPickerColor.SecondColor)
                    UpdateSliders();
            }
        }

        /// <summary>
        /// Gets or sets the first color of the control in AHSL format
        /// </summary>
        public AhslColor FirstAhslColor
        {
            get => _firstColor;
            set
            {
                if (_firstColor == value)
                    return;

                _firstColor = value;
                pnl_firstColor.BackColor = value.ToColor();
                if (SelectedColor == ColorPickerColor.FirstColor)
                    UpdateSliders();
            }
        }

        /// <summary>
        /// Gets or sets the second color of the control in AHSL format
        /// </summary>
        public AhslColor SecondAhslColor
        {
            get => _secondColor;
            set
            {
                if (_secondColor == value)
                    return;

                _secondColor = value;
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
            get => _selectedColor;
            set
            {
                if (_selectedColor == value)
                    return;

                _selectedColor = value;

                switch (_selectedColor)
                {
                    case ColorPickerColor.FirstColor:
                        pnl_firstColor.BorderStyle = BorderStyle.Fixed3D;
                        pnl_secondColor.BorderStyle = BorderStyle.FixedSingle;
                        break;
                    case ColorPickerColor.SecondColor:
                        pnl_firstColor.BorderStyle = BorderStyle.FixedSingle;
                        pnl_secondColor.BorderStyle = BorderStyle.Fixed3D;
                        break;
                }

                UpdateSliders();
            }
        }

        /// <summary>
        /// Initializes a new instance of the ColorPicker class
        /// </summary>
        public ColorPicker()
        {
            InitializeComponent();

            _firstColor = Color.Black.ToAhsl();
            _secondColor = Color.White.ToAhsl();

            pnl_firstColor.BackColor = FirstColor;
            pnl_secondColor.BackColor = SecondColor;

            // Hookup the events
            cs_alpha.ColorChanged += cs_colorChanged;
            cs_red.ColorChanged += cs_colorChanged;
            cs_green.ColorChanged += cs_colorChanged;
            cs_blue.ColorChanged += cs_colorChanged;
            cs_hue.ColorChanged += cs_colorChanged;
            cs_saturation.ColorChanged += cs_colorChanged;
            cs_lightness.ColorChanged += cs_colorChanged;

            UpdateSliders();
        }

        /// <summary>
        /// Sets the currently selected color
        /// </summary>
        /// <param name="color">The new value for the currently selected color</param>
        /// <param name="keepTransparency">Whether to keep the current alpha channel unmodified</param>
        public void SetCurrentColor(Color color, bool keepTransparency = false)
        {
            SetCurrentColor(color.ToAhsl(), keepTransparency);
        }

        /// <summary>
        /// Sets the currently selected color
        /// </summary>
        /// <param name="color">The new value for the currently selected color</param>
        /// <param name="keepTransparency">Whether to keep the current alpha channel unmodified</param>
        public void SetCurrentColor(AhslColor color, bool keepTransparency = false)
        {
            AhslColor oldColor = Color.White.ToAhsl();

            if (keepTransparency)
            {
                color = new AhslColor(GetCurrentColor().A, color.Hf, color.Sf, color.Lf);
            }

            switch (_selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    oldColor = _firstColor;
                    FirstAhslColor = color;
                    break;
                case ColorPickerColor.SecondColor:
                    oldColor = _secondColor;
                    SecondAhslColor = color;
                    break;
            }

            UpdateSliders();

            ColorPick?.Invoke(this, new ColorPickEventArgs(oldColor.ToColor(), color.ToColor(), _selectedColor));
        }

        /// <summary>
        /// Updates the color sliders
        /// </summary>
        public void UpdateSliders()
        {
            AhslColor color = Color.White.ToAhsl();

            switch (_selectedColor)
            {
                case ColorPickerColor.FirstColor:
                    color = _firstColor;
                    break;
                case ColorPickerColor.SecondColor:
                    color = _secondColor;
                    break;
            }

            // Global alpha channel
            cs_alpha.ActiveColor = color;
            // RGB
            cs_red.ActiveColor = color;
            cs_green.ActiveColor = color;
            cs_blue.ActiveColor = color;
            // HSL
            cs_hue.ActiveColor = color;
            cs_saturation.ActiveColor = color;
            cs_lightness.ActiveColor = color;
        }

        /// <summary>
        /// Gets the currently selected color
        /// </summary>
        /// <returns>The value for the currently selected color</returns>
        public Color GetCurrentColor()
        {
            switch (_selectedColor)
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
            SelectedColor = ColorPickerColor.FirstColor;
        }

        // 
        // Second Color Panel mouse down event handler
        // 
        private void pnl_secondColor_MouseDown(object sender, MouseEventArgs e)
        {
            SelectedColor = ColorPickerColor.SecondColor;
        }

        // 
        // Palette Bitmap mouse down
        // 
        private void pb_palette_MouseDown(object sender, [NotNull] MouseEventArgs e)
        {
            _mouseDown = true;

            SelectColorOnPalette(e.Location);
        }

        // 
        // Palette Bitmap mouse move
        // 
        private void pb_palette_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseDown)
            {
                SelectColorOnPalette(e.Location);
            }
        }

        // 
        // Palette Bitmap mouse up
        // 
        private void pb_palette_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseDown = false;
        }

        //
        // Color Sliders component changed event handler
        //
        private void cs_colorChanged(object sender, [NotNull] ColorChangedEventArgs eventArgs)
        {
            SetCurrentColor(eventArgs.NewColor);
        }

        /// <summary>
        /// Sets the currently highlighted color to be the color on the given position on the palette bitmap
        /// </summary>
        /// <param name="point">The point to get the color at the palette</param>
        private void SelectColorOnPalette(Point point)
        {
            point.X = Math.Max(0, Math.Min(pb_palette.Width - 1, point.X));
            point.Y = Math.Max(0, Math.Min(pb_palette.Height - 1, point.Y));

            point.X = (int)(point.X / ((float)pb_palette.Width / pb_palette.Image.Width));
            point.Y = (int)(point.Y / ((float)pb_palette.Height / pb_palette.Image.Height));

            if (pb_palette.Image is Bitmap bitmap)
            {
                Color color = bitmap.GetPixel(point.X, point.Y);
                SetCurrentColor(color, true);
            }
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
        public Color OldColor { get; }

        /// <summary>
        /// Gets the new color value
        /// </summary>
        public Color NewColor { get; }

        /// <summary>
        /// Gets the ColorPicker color index that was changed
        /// </summary>
        public ColorPickerColor TargetColor { get; }

        /// <summary>
        /// Initializes a new instance of the using Pixelaria.Views.ModelViews;
        /// </summary>
        /// <param name="oldColor">Gets the color value before the change</param>
        /// <param name="newColor">Gets the new color value</param>
        /// <param name="targetColor">Gets the ColorPicker color index that was changed</param>
        public ColorPickEventArgs(Color oldColor, Color newColor, ColorPickerColor targetColor)
        {
            OldColor = oldColor;
            NewColor = newColor;
            TargetColor = targetColor;
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
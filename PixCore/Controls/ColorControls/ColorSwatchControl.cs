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
using System.Windows.Forms;

namespace PixCore.Controls.ColorControls
{
    /// <summary>
    /// A control that is used to display a color swatch to the user and
    /// allow them pick colors as well as add new custom colors to it.
    /// </summary>
    [DefaultEvent("ColorSelect")]
    public partial class ColorSwatchControl : UserControl
    {
        /// <summary>
        /// The color swatch for this ColorSwatchControl
        /// </summary>
        private readonly ColorSwatch _colorSwatch;

        /// <summary>
        /// The last mouse cell area the mouse was hovering over
        /// </summary>
        private Rectangle _lastMouseCellArea;

        /// <summary>
        /// The horizontal cell the mouse is currently over at the moment
        /// </summary>
        private int _mouseCellX;

        /// <summary>
        /// The vertical cell the mouse is currently over at the moment
        /// </summary>
        private int _mouseCellY;

        /// <summary>
        /// The width of the color cells
        /// </summary>
        private readonly int _cellWidth;
        /// <summary>
        /// The height of the color cells
        /// </summary>
        private readonly int _cellHeight;

        /// <summary>
        /// The number of colors to place at each row
        /// before jumping to the next row
        /// </summary>
        private readonly int _cellsPerRow;

        /// <summary>
        /// Whether the mouse is currently over this control
        /// </summary>
        private bool _mouseOver;

        /// <summary>
        /// Whether the mouse is currently pressed down on this control
        /// </summary>
        private bool _mouseDown;

        /// <summary>
        /// Delegate for a ColorSelect event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The arguments for the event</param>
        public delegate void ColorSelectEventHandler(object sender, ColorSelectEventArgs e);

        /// <summary>
        /// Occurs whenever the user selects a color on the swatch
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user selects a color on the swatch")]
        public event ColorSelectEventHandler ColorSelect;

        /// <summary>
        /// Initializes a new ColorSwatches control
        /// </summary>
        public ColorSwatchControl()
        {
            InitializeComponent();

            // Initialize with the default swatch
            _colorSwatch = ColorSwatch.MakeDefault();

            _mouseCellX = _mouseCellY = 0;

            _cellWidth = 13;
            _cellHeight = 13;

            _cellsPerRow = 13;

            _mouseOver = false;

            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
        }

        // 
        // OnPaint event handler
        // 
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(Color.Black);

            // Draw the swatch colors as 20x20 squares that wrap at every 13 colors
            int x = 0;
            int y = 0;

            foreach (var color in _colorSwatch.Colors)
            {
                Rectangle region = new Rectangle(x * _cellWidth, y * _cellHeight, _cellWidth, _cellHeight);

                if (e.ClipRectangle.IntersectsWith(region))
                {
                    var brush = new SolidBrush(color);

                    e.Graphics.FillRectangle(brush, region);

                    brush.Dispose();
                }

                x ++;

                if (x >= _cellsPerRow)
                {
                    x = 0;
                    y++;
                }
            }

            if (_mouseOver)
            {
                // Draw the mouse cell
                Rectangle cellRect = GetMouseCellArea();

                e.Graphics.DrawRectangle(Pens.White, cellRect);
            }
        }

        // 
        // OnMouseDown event handler
        // 
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button != MouseButtons.Left)
                return;

            // Gets the color the user clicked on
            ColorSelect?.Invoke(this, new ColorSelectEventArgs(GetColorUnderMouse()));

            _mouseDown = true;
        }

        // 
        // OnMouseMove event handler
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Keep track of the current mouse position so we can invalidate the region
            _mouseCellX = Math.Max(0, Math.Min(_cellsPerRow - 1, e.X / _cellWidth));
            _mouseCellY = Math.Max(0, Math.Min((int)Math.Ceiling(_colorSwatch.ColorCount / (float)(_cellsPerRow)) - 1, e.Y / _cellHeight));

            // Invalidate the mouse region
            Rectangle newCellRect = GetMouseCellArea();

            if (newCellRect != _lastMouseCellArea)
            {
                Rectangle oldRec = _lastMouseCellArea;
                Rectangle newRec = newCellRect;

                oldRec.Inflate(2, 2);
                newRec.Inflate(2, 2);

                Invalidate(oldRec);
                Invalidate(newRec);

                if (_mouseDown && _mouseOver)
                {
                    // Gets the color the user clicked on
                    ColorSelect?.Invoke(this, new ColorSelectEventArgs(GetColorUnderMouse()));
                }
            }

            _lastMouseCellArea = newCellRect;
        }

        // 
        // OnMouseEnter event handler
        // 
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            _mouseOver = true;

            Invalidate();
        }

        // 
        // OnMouseLeave event handler
        // 
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _mouseOver = false;

            Rectangle newCellRect = GetMouseCellArea();
            newCellRect.Offset(-1, -1);
            newCellRect.Inflate(2, 2);
            Invalidate(newCellRect);
        }

        // 
        // OnMouseUp event handler
        // 
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown = false;
        }

        /// <summary>
        /// Returns a Rectangle that represents the area of the cell the mouse is currently hovering over
        /// </summary>
        /// <returns>A Rectangle that represents the area of the cell the mouse is currently hovering over</returns>
        private Rectangle GetMouseCellArea()
        {
            return new Rectangle(_mouseCellX * _cellWidth, _mouseCellY * _cellHeight, _cellWidth, _cellHeight);
        }

        /// <summary>
        /// Gets the color currently under the mouse cursor
        /// </summary>
        /// <returns>The color currently under the mouse cursor</returns>
        private Color GetColorUnderMouse()
        {
            int index = _mouseCellX % 13 + 13 * (_mouseCellY);

            if (index >= _colorSwatch.ColorCount)
            {
                return Color.Black;
            }

            return _colorSwatch[index];
        }
    }

    /// <summary>
    /// Event arguments for a ColorSelect event
    /// </summary>
    public class ColorSelectEventArgs: EventArgs
    {
        /// <summary>
        /// Gets the color value
        /// </summary>
        public Color Color { get; }

        /// <summary>
        /// Initializes a new instance of the ColorChangeEventArgs
        /// </summary>
        /// <param name="color">Gets the color value</param>
        public ColorSelectEventArgs(Color color)
        {
            Color = color;
        }
    }

    /// <summary>
    /// Specifies a set of color
    /// </summary>
    public class ColorSwatch
    {
        /// <summary>
        /// List of colors in this swatch
        /// </summary>
        protected List<Color> colorList;

        /// <summary>
        /// Gets the array of colors currently on this ColorSwatch
        /// </summary>
        public Color[] Colors => colorList.ToArray();

        /// <summary>
        /// The number of colors on this ColorSwatch
        /// </summary>
        public int ColorCount => colorList.Count;

        /// <summary>
        /// Gets the color at the given index on this swatch
        /// </summary>
        /// <param name="index">The index of the color to get</param>
        /// <returns>The color at the given index on this swatch</returns>
        public Color this[int index] => colorList[index];

        /// <summary>
        /// Initializes a new instance of the ColorSwatch class
        /// </summary>
        public ColorSwatch()
        {
            colorList = new List<Color>();
        }

        /// <summary>
        /// Creates the default color swatch
        /// </summary>
        /// <returns>A ColorSwatches object with the default colors</returns>
        public static ColorSwatch MakeDefault()
        {
            var colorSwatches = new ColorSwatch();

            // Code generated swatch that fades a rainbow swatch darker as you go down the rows
            uint[] colors = { 0xFFFF0000, 0xFFFF7F00, 0xFFFFFF00, 0xFF7FFF00, 0xFF00FF00, 0xFF00FF7F, 0xFF00FFFF, 0xFF007FFF, 0xFF0000FF, 0xFF7F00FF, 0xFFFF00FF, 0xFFFF007F, 0xFFFF0000, 
                              0xFFE50000, 0xFFE57200, 0xFFE5E500, 0xFF72E500, 0xFF00E500, 0xFF00E572, 0xFF00E5E5, 0xFF0072E5, 0xFF0000E5, 0xFF7200E5, 0xFFE500E5, 0xFFE50072, 0xFFE50000, 
                              0xFFCC0000, 0xFFCC6600, 0xFFCCCC00, 0xFF66CC00, 0xFF00CC00, 0xFF00CC66, 0xFF00CCCC, 0xFF0066CC, 0xFF0000CC, 0xFF6600CC, 0xFFCC00CC, 0xFFCC0066, 0xFFCC0000, 
                              0xFFB20000, 0xFFB25900, 0xFFB2B200, 0xFF59B200, 0xFF00B200, 0xFF00B259, 0xFF00B2B2, 0xFF0059B2, 0xFF0000B2, 0xFF5900B2, 0xFFB200B2, 0xFFB20059, 0xFFB20000, 
                              0xFF990000, 0xFF994C00, 0xFF999900, 0xFF4C9900, 0xFF009900, 0xFF00994C, 0xFF009999, 0xFF004C99, 0xFF000099, 0xFF4C0099, 0xFF990099, 0xFF99004C, 0xFF990000, 
                              0xFF7F0000, 0xFF7F3F00, 0xFF7F7F00, 0xFF3F7F00, 0xFF007F00, 0xFF007F3F, 0xFF007F7F, 0xFF003F7F, 0xFF00007F, 0xFF3F007F, 0xFF7F007F, 0xFF7F003F, 0xFF7F0000, 
                              0xFF660000, 0xFF663300, 0xFF666600, 0xFF336600, 0xFF006600, 0xFF006633, 0xFF006666, 0xFF003366, 0xFF000066, 0xFF330066, 0xFF660066, 0xFF660033, 0xFF660000, 
                              0xFF4C0000, 0xFF4C2600, 0xFF4C4C00, 0xFF264C00, 0xFF004C00, 0xFF004C26, 0xFF004C4C, 0xFF00264C, 0xFF00004C, 0xFF26004C, 0xFF4C004C, 0xFF4C0026, 0xFF4C0000, 
                              0xFF330000, 0xFF331900, 0xFF333300, 0xFF193300, 0xFF003300, 0xFF003319, 0xFF003333, 0xFF001933, 0xFF000033, 0xFF190033, 0xFF330033, 0xFF330019, 0xFF330000, 
                              0xFF190000, 0xFF190C00, 0xFF191900, 0xFF0C1900, 0xFF001900, 0xFF00190C, 0xFF001919, 0xFF000C19, 0xFF000019, 0xFF0C0019, 0xFF190019, 0xFF19000C, 0xFF190000,
                              0xFF000000, 0xFF151515, 0xFF2A2A2A, 0xFF3F3F3F, 0xFF555555, 0xFF6A6A6A, 0xFF7F7F7F, 0xFF949494, 0xFFAAAAAA, 0xFFBFBFBF, 0xFFD4D4D4, 0xFFE9E9E9, 0xFFFFFFFF };

            // Add the colors to the swatch and return it
            foreach (uint color in colors)
            {
                colorSwatches.AddColor(Color.FromArgb(unchecked((int)color)));
            }

            return colorSwatches;
        }

        /// <summary>
        /// Adds the given color to this ColorSwatches
        /// </summary>
        /// <param name="color">The color to add to this swatch</param>
        public void AddColor(Color color)
        {
            colorList.Add(color);
        }

        /// <summary>
        /// Sets a color on this ColorSwatches to be of the given color
        /// </summary>
        /// <param name="color">The color to set</param>
        /// <param name="index">The position to set the color at</param>
        public void SetColor(Color color, int index)
        {
            colorList[index] = color;
        }
    }
}
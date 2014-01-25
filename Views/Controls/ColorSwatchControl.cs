using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
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
        private ColorSwatch colorSwatch;

        /// <summary>
        /// The last mouse cell area the mouse was hovering over
        /// </summary>
        private Rectangle lastMouseCellArea;

        /// <summary>
        /// The horizontal cell the mouse is currently over at the moment
        /// </summary>
        private int mouseCellX;

        /// <summary>
        /// The vertical cell the mouse is currently over at the moment
        /// </summary>
        private int mouseCellY;

        /// <summary>
        /// The width of the color cells
        /// </summary>
        private int cellWidth;
        /// <summary>
        /// The height of the color cells
        /// </summary>
        private int cellHeight;

        /// <summary>
        /// The number of colors to place at each row
        /// before jumping to the next row
        /// </summary>
        private int cellsPerRow;

        /// <summary>
        /// Whether the mouse is currently over this control
        /// </summary>
        private bool mouseOver;

        /// <summary>
        /// Delegate for a ColorSelect event
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The arguments for the event</param>
        public delegate void ColorSelectEventHandler(object sender, ColorSelectEventArgs eventArgs);

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
            colorSwatch = ColorSwatch.MakeDefault();

            mouseCellX = mouseCellY = 0;

            cellWidth = 17;
            cellHeight = 17;

            cellsPerRow = 13;

            mouseOver = false;

            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
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

            foreach (Color color in colorSwatch.Colors)
            {
                Rectangle region = new Rectangle(x * cellWidth, y * cellHeight, cellWidth, cellHeight);


                if (e.ClipRectangle.IntersectsWith(region))
                {
                    SolidBrush brush = new SolidBrush(color);

                    e.Graphics.FillRectangle(brush, region);

                    brush.Dispose();
                }

                x ++;

                if (x >= cellsPerRow)
                {
                    x = 0;
                    y++;
                }
            }

            if (mouseOver)
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

            // Gets the color the user clicked on
            if (ColorSelect != null)
            {
                ColorSelect.Invoke(this, new ColorSelectEventArgs(GetColorOverMouse()));
            }
        }

        // 
        // OnMouseMove event handler
        // 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Keep track of the current mouse position so we can invalidate the region
            mouseCellX = Math.Max(0, Math.Min(cellsPerRow - 1, e.X / cellWidth));
            mouseCellY = Math.Max(0, e.Y / cellHeight);

            // Invalidate the mouse region
            Rectangle newCellRect = GetMouseCellArea();

            if (newCellRect != lastMouseCellArea)
            {
                Rectangle oldRec = lastMouseCellArea;
                Rectangle newRec = newCellRect;

                oldRec.Offset(-1, -1);
                oldRec.Inflate(2, 2);

                newRec.Offset(-1, -1);
                newRec.Inflate(2, 2);

                Invalidate(oldRec);
                Invalidate(newRec);
            }

            lastMouseCellArea = newCellRect;
        }

        // 
        // OnMouseEnter event handler
        // 
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            mouseOver = true;

            Invalidate();
        }

        // 
        // OnMouseLeave event handler
        // 
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            mouseOver = false;

            Rectangle newCellRect = GetMouseCellArea();
            newCellRect.Offset(-1, -1);
            newCellRect.Inflate(2, 2);
            Invalidate(newCellRect);
        }

        /// <summary>
        /// Returns a Rectangle that represents the area of the cell the mouse is currently hovering over
        /// </summary>
        /// <returns>A Rectangle that represents the area of the cell the mouse is currently hovering over</returns>
        private Rectangle GetMouseCellArea()
        {
            return new Rectangle(mouseCellX * cellWidth, mouseCellY * cellHeight, cellWidth, cellHeight);
        }

        /// <summary>
        /// Gets the color currently over the mouse cursor
        /// </summary>
        /// <returns>The color currently over the mouse cursor</returns>
        private Color GetColorOverMouse()
        {
            int index = mouseCellX % 13 + 13 * (mouseCellY);

            if (index >= colorSwatch.ColorCount)
            {
                return Color.Black;
            }

            return colorSwatch[index];
        }
    }

    /// <summary>
    /// Event arguments for a ColorSelect event
    /// </summary>
    public class ColorSelectEventArgs
    {
        /// <summary>
        /// Gets the color value
        /// </summary>
        public Color Color { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ColorChangeEventArgs
        /// </summary>
        /// <param name="color">Gets the color value</param>
        public ColorSelectEventArgs(Color color)
        {
            this.Color = color;
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
        public Color[] Colors { get { return colorList.ToArray(); } }
        
        /// <summary>
        /// The number of colors on this ColorSwatch
        /// </summary>
        public int ColorCount { get { return colorList.Count; } }

        /// <summary>
        /// Gets the color at the given index on this swatch
        /// </summary>
        /// <param name="index">The index of the color to get</param>
        /// <returns>The color at the given index on this swatch</returns>
        public Color this[int index] { get { return this.colorList[index]; } }

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
            ColorSwatch colorSwatches = new ColorSwatch();

            // Code generated swatch that fades a rainbow swatch darker as you go down the rows
            uint[] colors = new uint[] { 0xFFFF0000, 0xFFFF7F00, 0xFFFFFF00, 0xFF7FFF00, 0xFF00FF00, 0xFF00FF7F, 0xFF00FFFF, 0xFF007FFF, 0xFF0000FF, 0xFF7F00FF, 0xFFFF00FF, 0xFFFF007F, 0xFFFF0000, 
                                         0xFFE50000, 0xFFE57200, 0xFFE5E500, 0xFF72E500, 0xFF00E500, 0xFF00E572, 0xFF00E5E5, 0xFF0072E5, 0xFF0000E5, 0xFF7200E5, 0xFFE500E5, 0xFFE50072, 0xFFE50000, 
                                         0xFFCC0000, 0xFFCC6600, 0xFFCCCC00, 0xFF66CC00, 0xFF00CC00, 0xFF00CC66, 0xFF00CCCC, 0xFF0066CC, 0xFF0000CC, 0xFF6600CC, 0xFFCC00CC, 0xFFCC0066, 0xFFCC0000, 
                                         0xFFB20000, 0xFFB25900, 0xFFB2B200, 0xFF59B200, 0xFF00B200, 0xFF00B259, 0xFF00B2B2, 0xFF0059B2, 0xFF0000B2, 0xFF5900B2, 0xFFB200B2, 0xFFB20059, 0xFFB20000, 
                                         0xFF990000, 0xFF994C00, 0xFF999900, 0xFF4C9900, 0xFF009900, 0xFF00994C, 0xFF009999, 0xFF004C99, 0xFF000099, 0xFF4C0099, 0xFF990099, 0xFF99004C, 0xFF990000, 
                                         0xFF7F0000, 0xFF7F3F00, 0xFF7F7F00, 0xFF3F7F00, 0xFF007F00, 0xFF007F3F, 0xFF007F7F, 0xFF003F7F, 0xFF00007F, 0xFF3F007F, 0xFF7F007F, 0xFF7F003F, 0xFF7F0000, 
                                         0xFF660000, 0xFF663300, 0xFF666600, 0xFF336600, 0xFF006600, 0xFF006633, 0xFF006666, 0xFF003366, 0xFF000066, 0xFF330066, 0xFF660066, 0xFF660033, 0xFF660000, 
                                         0xFF4C0000, 0xFF4C2600, 0xFF4C4C00, 0xFF264C00, 0xFF004C00, 0xFF004C26, 0xFF004C4C, 0xFF00264C, 0xFF00004C, 0xFF26004C, 0xFF4C004C, 0xFF4C0026, 0xFF4C0000, 
                                         0xFF330000, 0xFF331900, 0xFF333300, 0xFF193300, 0xFF003300, 0xFF003319, 0xFF003333, 0xFF001933, 0xFF000033, 0xFF190033, 0xFF330033, 0xFF330019, 0xFF330000, 
                                         0xFF190000, 0xFF190C00, 0xFF191900, 0xFF0C1900, 0xFF001900, 0xFF00190C, 0xFF001919, 0xFF000C19, 0xFF000019, 0xFF0C0019, 0xFF190019, 0xFF19000C, 0xFF190000 };

            // Add the colors to the swatch and return it
            foreach (uint color in colors)
            {
                colorSwatches.AddColor(Color.FromArgb((int)((color >> 24) & 0xFF), (int)((color >> 16) & 0xFF), (int)((color >> 8) & 0xFF), (int)((color) & 0xFF)));
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

        #region Helper Methods

        /// <summary>
        /// Creates an ARGB color form the given HSL color components
        /// </summary>
        /// <param name="h">The hue</param>
        /// <param name="s">The saturation</param>
        /// <param name="l">The lightness</param>
        /// <param name="alpha">An optional alpha component</param>
        /// <returns>An ARGB color from the given HSL color components</returns>
        public static Color FromHSL(float h, float s, float l, float alpha = 1)
        {
            if (h < 0) h = 0;
            if (s < 0) s = 0;
            if (l < 0) l = 0;
            if (h >= 360) h = 359;
            if (s > 100) s = 100;
            if (l > 100) l = 100;
            s /= 100;
            l /= 100;
            float C = (1 - Math.Abs(2 * l - 1)) * s;
            float hh = h / 60;
            float X = C * (1 - Math.Abs(hh % 2 - 1));

            float r = 0, g = 0, b = 0;

            if (hh >= 0 && hh < 1)
            {
                r = C;
                g = X;
            }
            else if (hh >= 1 && hh < 2)
            {
                r = X;
                g = C;
            }
            else if (hh >= 2 && hh < 3)
            {
                g = C;
                b = X;
            }
            else if (hh >= 3 && hh < 4)
            {
                g = X;
                b = C;
            }
            else if (hh >= 4 && hh < 5)
            {
                r = X;
                b = C;
            }
            else
            {
                r = C;
                b = X;
            }
            float m = l - C / 2;

            r += m;
            g += m;
            b += m;

            r *= 255;
            g *= 255;
            b *= 255;

            return Color.FromArgb((int)(alpha * 255), (int)r, (int)g, (int)b);
        }

        #endregion
    }

    /// <summary>
    /// Represents an HSL color with an alpha channel
    /// </summary>
    public struct AHSL
    {
        /// <summary>
        /// The Alpha component
        /// </summary>
        int a;

        /// <summary>
        /// The Hue component
        /// </summary>
        int h;

        /// <summary>
        /// The Saturation component
        /// </summary>
        int s;

        /// <summary>
        /// The Lightness component
        /// </summary>
        int l;

        /// <summary>
        /// Gets or sets the alpha component
        /// </summary>
        public int A { get { return a; } set { this.a = value; } }

        /// <summary>
        /// Gets or sets the hue component
        /// </summary>
        public int H { get { return h; } set { this.h = value; } }

        /// <summary>
        /// Gets or sets the saturation component
        /// </summary>
        public int S { get { return s; } set { this.s = value; } }

        /// <summary>
        /// Gets or sets the lightness component
        /// </summary>
        public int L { get { return l; } set { this.l = value; } }

        /// <summary>
        /// Creates a new AHSL color
        /// </summary>
        /// <param name="a">The Alpha component</param>
        /// <param name="h">The Hue component</param>
        /// <param name="s">The Saturation component</param>
        /// <param name="l">The Lightness component</param>
        public AHSL(int a, int h, int s, int l)
        {
            this.a = a;
            this.h = h;
            this.s = s;
            this.l = l;
        }

        /// <summary>
        /// Converts this AHSL color to a ARGB color
        /// </summary>
        /// <returns>The ARGB color that represents this AHSL color</returns>
        public Color ToARGB()
        {
            return ColorSwatch.FromHSL(this.h, this.s, this.l, (this.a / 255.0f));
        }
    }
}
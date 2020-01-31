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

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using PixRendering;

namespace PixDirectX.Rendering.Gdi
{
    public class GdiFontManager : IFontManager
    {
        public IReadOnlyList<IFontFamily> GetFontFamilies()
        {
            return FontFamily.Families.Select(family => new GdiFontFamily(family)).ToArray();
        }

        public IFont DefaultFont(float size)
        {
            return new GdiFont(new Font(FontFamily.GenericSansSerif, size));
        }
    }

    public class GdiFontFamily : IFontFamily
    {
        public string FamilyName => FontFamily.Name;

        public FontFamily FontFamily { get; }

        public GdiFontFamily(FontFamily fontFamily)
        {
            FontFamily = fontFamily;
        }
    }

    public class GdiFont : IFont
    {
        public Font Font { get; }

        public string FamilyName => Font.FontFamily.Name;

        public float FontSize => Font.Size;

        public string Name => Font.Name;

        public GdiFont(Font font)
        {
            Font = font;
        }

        public void Dispose()
        {
            Font.Dispose();
        }
    }
}

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
using System.Drawing.Text;
using System.IO;
using System.Linq;
using Blend2DCS;
using JetBrains.Annotations;
using PixRendering;

namespace PixDirectX.Rendering.Blend2D
{
    internal class Blend2DFontManager : IFontManager
    {
        private readonly Dictionary<float, Blend2DFont> _defaultFontMap = new Dictionary<float, Blend2DFont>();
        private IReadOnlyList<FontInformation> _sansSerifFontFiles;
        private BLFontFace _defaultFontFace;
        private Dictionary<string, List<FontInformation>> _fontNameToFiles;

        public IReadOnlyList<IFontFamily> GetFontFamilies()
        {
            return new IFontFamily[0];
        }

        public IFont DefaultFont(float size)
        {
            if (_sansSerifFontFiles == null)
            {
                _sansSerifFontFiles = GetFilesForFont("Sans Serif");
            }

            if (_defaultFontFace == null)
            {
                _defaultFontFace = new BLFontFace(_sansSerifFontFiles[0].FileName, 0);
            }

            if (_defaultFontMap.TryGetValue(size, out var font))
            {
                return font;
            }

            var blend2DFont = new Blend2DFont(new BLFont(_defaultFontFace, size), _sansSerifFontFiles[0].FontName, _sansSerifFontFiles[0].FamilyName, size);
            _defaultFontMap[size] = blend2DFont;

            return blend2DFont;
        }

        /// <summary>
        /// This is a brute force way of finding the files that represent a particular
        /// font family.
        /// The first call may be quite slow.
        /// Only finds font files that are installed in the standard directory.
        /// Will not discover font files installed after the first call.
        /// </summary>
        /// <returns>enumeration of file paths (possibly none) that contain data
        /// for the specified font name</returns>
        private IReadOnlyList<FontInformation> GetFilesForFont([NotNull] string fontName)
        {
            if (_fontNameToFiles == null)
            {
                _fontNameToFiles = new Dictionary<string, List<FontInformation>>();
                foreach (string fontFile in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)))
                {
                    if (!fontFile.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var fc = new PrivateFontCollection();
                    try
                    {
                        fc.AddFontFile(fontFile);
                    }
                    catch (FileNotFoundException)
                    {
                        continue; // not sure how this can happen but I've seen it.
                    }

                    string name = fc.Families[0].Name;
                    // If you care about bold, italic, etc, you can filter here.
                    if (!_fontNameToFiles.TryGetValue(name, out var files))
                    {
                        files = new List<FontInformation>();
                        _fontNameToFiles[name] = files;
                    }

                    var fontInformation = new FontInformation(fontFile, name, name);

                    files.Add(fontInformation);
                }
            }

            var list = new List<FontInformation>();
            foreach (var keyValuePair in _fontNameToFiles.Where(keyValuePair => keyValuePair.Key.ToLower().Contains(fontName.ToLower())))
            {
                list.AddRange(keyValuePair.Value);
            }

            return list;
        }

        private struct FontInformation
        {
            public string FileName { get; }

            public string FontName { get; }

            public string FamilyName { get; }

            public FontInformation(string fileName, string fontName, string familyName)
            {
                FileName = fileName;
                FontName = fontName;
                FamilyName = familyName;
            }
        }
    }

    public class Blend2DFont : IFont
    {
        public BLFont Font { get; }

        public string FamilyName { get; }

        public float FontSize { get; }

        public string Name { get; }

        public Blend2DFont(BLFont font, string fontName, string fontFamily, float fontSize)
        {
            Font = font;
            FamilyName = fontFamily;
            Name = fontName;
            FontSize = fontSize;
        }

        public void Dispose()
        {
            Font.Dispose();
        }
    }
}

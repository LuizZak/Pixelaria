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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.PaintTools;

namespace PixelariaTests.Views.Controls.PaintTools
{
    [TestClass]
    public class PencilPaintToolTests
    {
        [TestMethod]
        public void TestPaintToolPerformance()
        {
            using (var bitmap = new Bitmap(1024, 1024))
            {
                var panel = new ImageEditPanel {Size = new Size(1024, 1024)};
                panel.LoadBitmap(bitmap);
                var sut = new PencilPaintTool(Color.Black, Color.White, 20);
                sut.Initialize(panel.PictureBox);

                var sw = Stopwatch.StartNew();

                sut.MouseDown(new MouseEventArgs(MouseButtons.Left, 0, 50, 50, 0));
                sut.MouseMove(new MouseEventArgs(MouseButtons.Left, 0, 950, 950, 0));
                sut.MouseUp(new MouseEventArgs(MouseButtons.Left, 0, 950, 950, 0));

                sw.Stop();
                Console.WriteLine($@"{sw.ElapsedMilliseconds:F2}ms");
            }
        }
    }
}
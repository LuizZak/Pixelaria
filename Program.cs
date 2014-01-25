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
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Pixelaria.Views;

namespace Pixelaria
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            /*
            const int BUFFW = 16;

            int[][] buffer = new int[BUFFW][];

            buffer = new int[][] { 
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 1, 1, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }
            };

            PrintBuff(buffer, BUFFW, BUFFW);

            Console.WriteLine();

            //floodFillScanlineStack(Extend(buffer), 2, 2, BUFFW, BUFFW, 1, 0);
            //IntPtr p = new IntPtr();
            //buffer.

            int[] a = Extend(buffer);

            //PrintBuff(Pack(a, BUFFW), BUFFW, BUFFW);

            floodFillScanlineStackArray(a, 2, 2, BUFFW, BUFFW, 1, 0);

            PrintBuff(Pack(a, BUFFW), BUFFW, BUFFW);

            //floodFillScanlineStackArray(a, a.Length, 1);

            //PrintBuff(a);

            //PrintBuff(buffer, BUFFW, BUFFW);

            Console.ReadKey();
            */

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(args));
        }

        static void PrintBuff(int[][] buff, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Console.Write(buff[y][x] + " ");
                }

                Console.WriteLine("");
            }
        }

        static void PrintBuff(int[] buff)
        {
            for (int y = 0; y < buff.Length; y++)
            {
                Console.Write(buff[y] + " ");
            }
            Console.WriteLine("");
        }

        static int[] Extend(int[][] array)
        {
            int w = array[0].Length;

            int[] extended = new int[array.Length * w];

            for (int i = 0; i < extended.Length; i++)
            {
                extended[i] = array[i / array.Length][i % w];
            }

            return extended;
        }

        static int[][] Pack(int[] array, int w)
        {
            int[][] pack = new int[w][];

            for (int i = 0; i < pack.Length; i++)
            {
                pack[i] = new int[array.Length / w];

                for (int x = 0; x < w; x++)
                {
                    pack[i][x] = array[i * w + x];
                }
            }

            return pack;
        }

        [DllImport("PixelariaC.DLL", CallingConvention = CallingConvention.Cdecl)]
        static extern int floodFillScanlineStackArray(int[] screenBuffer, int x, int y, int w, int h, int newColor, int oldColor);

        [DllImport("PixelariaC.DLL", CallingConvention = CallingConvention.Cdecl)]
        static extern int floodFillScanlineStack(int[] screenBuffer, int x, int y, int w, int h, int newColor, int oldColor);
    }
}
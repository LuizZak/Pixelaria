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
using JetBrains.Annotations;

namespace Pixelaria.Utils
{
    /// <summary>
    /// A basic static class to support runtime logging
    /// </summary>
    public static class Logging
    {
        private static readonly Stack<string> ModuleStack = new Stack<string>();

        static Logging()
        {
            Trace.IndentSize = 2;
            Trace.AutoFlush = true;
        }

        public static void Error(string message, string module = null)
        {
            WriteEntry(message, "error", module);
        }

        public static void Error([NotNull] Exception ex, string module = null)
        {
            WriteEntry(ex.Message, "error", module);
        }

        public static void Warning(string message, string module = null)
        {
            WriteEntry(message, "warning", module);
        }

        public static void Info(string message, string module = null)
        {
            WriteEntry(message, "info", module);
        }

        /// <summary>
        /// To allow indentation during logging, use a block that will ident and then auto-deident
        /// afterwards.
        /// </summary>
        public static void Indenting([NotNull] Action perform)
        {
            Indenting(null, perform);
        }

        /// <summary>
        /// To allow indentation during logging, use a block that will ident and then auto-deident
        /// afterwards.
        /// </summary>
        public static void Indenting(string moduleName, [NotNull] Action perform)
        {
            if (moduleName != null)
                ModuleStack.Push(moduleName);

            Trace.Indent();

            try
            {
                perform();
            }
            catch (Exception e)
            {
                Error(e, "Logging");
            }
            finally
            {
                Trace.Unindent();

                if (moduleName != null)
                    ModuleStack.Pop();
            }
        }

        private static void WriteEntry(string message, string type = null, string module = null)
        {
            var finalModule = module;
            if (ModuleStack.Count > 0)
            {
                if (finalModule == null)
                    finalModule = "";

                foreach (var mod in ModuleStack)
                {
                    if (finalModule.Length != 0)
                        finalModule += "/";

                    finalModule += $"{mod}";
                }
            }

            var trace = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            if (module != null)
            {
                trace += $" - [{module}]";
            }
            if (module != null)
            {
                trace += $" - [{type}]:";
            }
            trace += $" {message}";

#if DEBUG
            Debug.WriteLine(trace);
#elif TRACE
            Trace.WriteLine(trace);
#endif
        }
    }
}

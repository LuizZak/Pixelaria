using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace PixelariaLib.Utils
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

            Trace.WriteLine(trace);
        }
    }
}

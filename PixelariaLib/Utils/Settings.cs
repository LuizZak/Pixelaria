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
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace PixelariaLib.Utils
{
    /// <summary>
    /// Provides settings that can be loaded and saved from files
    /// Settings allow changes such as graphics and control changes
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// The main configuration file reference
        /// </summary>
        private readonly IniReader _iniFile;

        /// <summary>
        /// Initializes the Settings class
        /// </summary>
        private Settings(string settingsFile)
        {
            _iniFile = new IniReader(settingsFile);
            _iniFile.LoadSettings();
        }

        /// <summary>
        /// Ensures that a specific value is present on the Settings object, and initializes it with a default value it of doesn't
        /// </summary>
        /// <param name="value">The path to the settings value</param>
        /// <param name="type">The type the value must have in order to be valid</param>
        /// <param name="defaultValue">The default value to be set if the value does not exists or does not matches the provided type</param>
        /// <returns>True when the value was asserted, false if it was not present or not valid</returns>
        public bool EnsureValue([NotNull] string value, EnsureValueType type, string defaultValue)
        {
            if (GetValue(value) == null)
            {
                SetValue(value, defaultValue);
                return false;
            }

            switch (type)
            {
                case EnsureValueType.Boolean:
                    if (GetValue(value) != "true" && GetValue(value) != "false")
                    {
                        SetValue(value, defaultValue);
                        return false;
                    }
                    break;
                case EnsureValueType.Int:
                    int a;
                    if (!int.TryParse(GetValue(value), out a))
                    {
                        SetValue(value, defaultValue);
                        return false;
                    }
                    break;
            }

            return true;
        }

        /// <summary>
        /// Saves the current settings to the disk
        /// </summary>
        public void SaveSettings()
        {
            _iniFile.SaveSettings();
        }

        /// <summary>
        /// Gets the given value from the values list
        /// </summary>
        /// <param name="valueName">A string representing the value saved. Returns null if the value is not currently present</param>
        [CanBeNull]
        public string GetValue([NotNull] params string[] valueName)
        {
            return _iniFile.GetValue(string.Join("\\", valueName));
        }

        /// <summary>
        /// Sets the given value on the values list
        /// </summary>
        /// <param name="valueName">Thev value name to sabe</param>
        /// <param name="value">The value to save</param>
        public void SetValue([NotNull] string valueName, string value)
        {
            _iniFile.SetValue(valueName, value);
            _iniFile.SaveSettings();
        }

        /// <summary>
        /// Returns the current settings singleton class
        /// </summary>
        /// <param name="path">The path for the settings file</param>
        /// <returns>The current settings singleton class</returns>
        public static Settings GetSettings(string path = "settings.ini")
        {
            if (_settings == null)
                return _settings = new Settings(path);

            return _settings;
        }

        /// <summary>
        /// The main settings singleton class
        /// </summary>
        private static Settings _settings;
    }

    /// <summary>
    /// Allows reading/saving of .ini files
    /// </summary>
    public class IniReader
    {
        /// <summary>
        /// Creates a new instance of the IniReader class
        /// </summary>
        /// <param name="path">A path to a .ini file</param>
        public IniReader(string path)
        {
            _filePath = path;
            _values = new Dictionary<string, string>();
        }

        /// <summary>
        /// Loads the settings from the .ini file
        /// </summary>
        public void LoadSettings()
        {
            // Clear the values before adding the new ones
            _values.Clear();

            // Create the file if it does not exists
            if (!File.Exists(_filePath))
                File.Create(_filePath).Close();

            // Load the file line by line
            using (var reader = new StreamReader(_filePath, Encoding.UTF8))
            {
                string currentPath = "";
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                
                    if (line == null)
                        break;

                    if (line.Trim() != "")
                    {
                        // Path block, change current path block
                        if (line[0] == '[')
                        {
                            currentPath = "";
                            string localPath = "";
                            // Use a parser to guarantee consistency
                            MiniParser parser = new MiniParser(line);
                            parser.Next(); // Skip the starting '['
                            parser.SkipWhiteSpace();

                            if (parser.Peek() == ']')
                                continue;
                            bool afterSeparator = false;
                            while (!parser.EOF())
                            {
                                if (parser.Peek() == ']')
                                {
                                    currentPath = localPath;
                                    break;
                                }

                                if (parser.Peek() == '\\')
                                {
                                    parser.Next();
                                    afterSeparator = true;
                                }
                                else
                                {
                                    try
                                    {
                                        if (afterSeparator)
                                            localPath += "\\";

                                        afterSeparator = false;

                                        // Buffer the path until a '\' sign
                                        while (!parser.EOF())
                                        {
                                            if (parser.Peek() != '\\' && parser.Peek() != ']')
                                            {
                                                localPath += parser.Next();
                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        continue;
                                    }
                                }

                                parser.SkipWhiteSpace();
                            }
                        }
                        // Comment line, ignore
                        else if (line.Trim()[0] == '\'')
                        {
                        
                        }
                        // Normal line, read the settings
                        else
                        {
                            // Parse the value
                            MiniParser parser = new MiniParser(line);
                            parser.SkipWhiteSpace();

                            try
                            {
                                // VAR = VALUE

                                // Read the settings identifier
                                string valueName = parser.ReadIdent(false);
                                parser.SkipWhiteSpace();

                                // If there's no '=', the line is not correctly formated
                                if (parser.Next() != '=')
                                    continue;

                                // Skip to the value
                                parser.SkipWhiteSpace();

                                // Read the value
                                StringBuilder builder = new StringBuilder();
                                while (!parser.EOF() && (parser.Peek() != '\n' || parser.Peek() != '\r'))
                                {
                                    builder.Append(parser.Next());
                                }
                                string value = builder.ToString();

                                // 
                                _values[currentPath + "\\" + valueName] = value;
                            }
                            // ReSharper disable once EmptyGeneralCatchClause
                            catch (Exception) { }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Saves the current settings to the .ini file
        /// </summary>
        public void SaveSettings()
        {
            // Start by organizing the values into a tree
            var baseNode = new SettingsNode();

            foreach (string value in _values.Keys)
            {
                string path = value;

                // Base settings, create values at the base node
                if (path.IndexOf("\\", StringComparison.Ordinal) == -1)
                {
                    baseNode[value] = _values[value];
                }
                else
                {
                    string[] subPath = path.Split('\\');

                    path = "";

                    for (int i = 0; i < subPath.Length - 1; i++)
                    {
                        if (path == "")
                            path += subPath[i];
                        else
                            path += "\\" + subPath[i];
                    }

                    baseNode.CreateNode(path)[subPath[subPath.Length - 1]] = _values[value];
                }
            }

            // Sort the nodes after they are created
            baseNode.Sort();

            // Mount the settings string
            var output = new StringBuilder();

            baseNode.SaveToString(output);

            using (var stream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);

                // Save the settings to the settings file now
                var writer = new StreamWriter(stream, Encoding.UTF8, 512, true);
                writer.Write(output.ToString().Trim());
            }

            baseNode.Clear();
        }

        /// <summary>
        /// Gets the given value from the values list
        /// </summary>
        /// <param name="valueName">A string representing the value saved. Returns null if the value is not currently present</param>
        [CanBeNull]
        public string GetValue([NotNull] params string[] valueName)
        {
            var collapsed = string.Join("\\", valueName);
            return _values.ContainsKey(collapsed) ? _values[collapsed] : null;
        }

        /// <summary>
        /// Sets the given value on the values list
        /// </summary>
        /// <param name="valueName">Thev value name to sabe</param>
        /// <param name="value">The value to save</param>
        public void SetValue([NotNull] string valueName, string value)
        {
            _values[valueName] = value;
        }

        /// <summary>
        /// The values stored in the .ini file
        /// </summary>
        private readonly Dictionary<string, string> _values;

        /// <summary>
        /// The filepath to the .ini file
        /// </summary>
        private readonly string _filePath;

        /// <summary>
        /// Specifies a node that contains settings and subnodes
        /// </summary>
        private class SettingsNode
        {
            /// <summary>
            /// The name of this SettingsNode
            /// </summary>
            private string _name = "";

            /// <summary>
            /// List of subnodes
            /// </summary>
            private readonly List<SettingsNode> _subNodes = new List<SettingsNode>();

            /// <summary>
            /// The parent node that owns this node
            /// </summary>
            private SettingsNode _parentNode;

            /// <summary>
            /// Dictionary of settings and their respective values
            /// </summary>
            private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

            /// <summary>
            /// Gets or sets a value from this SettingsNode
            /// </summary>
            /// <param name="valueName">The name of the value to modify</param>
            /// <returns>The string contents of the value</returns>
            public string this[string valueName]
            {
                set => _values[valueName] = value;
            }

            /// <summary>
            /// Creates and returns the given node path
            /// If the node path already exists, it is not re-created
            /// </summary>
            /// <param name="path">The path to the node, separated by '\'</param>
            /// <returns>The topmost node of the node list</returns>
            public SettingsNode CreateNode([NotNull] string path)
            {
                if (path == "")
                    return this;

                SettingsNode returnNode = null;
                string[] nodes = new string[1];

                if (path.IndexOf("\\", StringComparison.Ordinal) != -1)
                    nodes = path.Split('\\');
                else
                    nodes[0] = path;

                foreach (var node in _subNodes)
                {
                    if (node._name == nodes[0])
                    {
                        if (nodes.Length == 0)
                        {
                            returnNode = node;
                        }
                        else
                        {
                            string nextPath = "";
                            for (int i = 1; i < nodes.Length; i++)
                            {
                                if (nextPath == "")
                                    nextPath += nodes[i];
                                else
                                    nextPath += "\\" + nodes[i];
                            }

                            returnNode = node.CreateNode(nextPath);
                        }
                        break;
                    }
                }

                // If no node was found, create a new one
                if (returnNode == null)
                {
                    string nextPath = "";
                    for (int i = 1; i < nodes.Length; i++)
                    {
                        if (nextPath == "")
                            nextPath += nodes[i];
                        else
                            nextPath += "\\" + nodes[i];
                    }

                    var newNode = new SettingsNode { _parentNode = this, _name = nodes[0] };

                    _subNodes.Add(newNode);

                    returnNode = nextPath == "" ? newNode : newNode.CreateNode(nextPath);
                }

                return returnNode;
            }

            /// <summary>
            /// Saves this SettingsNode into the provided StringBuilder object
            /// </summary>
            /// <param name="builder">The StringBuilder to save the data to</param>
            public void SaveToString(StringBuilder builder)
            {
                string path = _name;
                var curNode = _parentNode;
                while (curNode != null)
                {
                    if (curNode._name != "")
                        path = curNode._name + "\\" + path;

                    curNode = curNode._parentNode;
                }

                if (path != "" && _values.Count > 0)
                {
                    builder.Append("["); builder.Append(path); builder.AppendLine("]");
                }

                // Save the values now
                foreach (string value in _values.Keys)
                {
                    builder.Append(value); builder.Append(" = "); builder.AppendLine(_values[value]);
                }

                if (_values.Count > 0)
                    builder.AppendLine();

                foreach (var childNode in _subNodes)
                {
                    childNode.SaveToString(builder);
                }
            }

            /// <summary>
            /// Clears this node and all subsequent nodes on the tree
            /// </summary>
            public void Clear()
            {
                // Start by clearing all the children nodes
                foreach (SettingsNode node in _subNodes)
                {
                    node.Clear();
                }

                // Clear the lists
                _subNodes.Clear();
                _values.Clear();

                // Clear the parent node reference
                _parentNode = null;
            }

            /// <summary>
            /// Sorts this and all children SettingsNode alphabetically
            /// </summary>
            public void Sort()
            {
                // Sort the subnodes
                _subNodes.Sort((lhs, rhs) => string.Compare(lhs._name.ToLower(), rhs._name, StringComparison.Ordinal));
                
                // Sort children nodes now
                foreach (var node in _subNodes)
                {
                    node.Sort();
                }
            }
        }

        /// <summary>
        /// A class that is able to minimally parse strings of text, used to parse a .ini file structure
        /// </summary>
        private class MiniParser
        {
            /// <summary>
            /// Initializes a new instance of the MiniParser class with a string to use as a
            /// buffer and optionally skipping past the start of the buffer to a certain position
            /// </summary>
            /// <param name="text">The string that will be used as a buffer</param>
            /// <param name="offset">The offset to start reading the buffer string from</param>
            public MiniParser(string text, int offset = 0)
            {
                _buffer = text;

                _length = _buffer.Length;
                _curChar = offset;
            }

            /// <summary>
            /// Reads an identifier from the buffer
            /// </summary>
            /// <param name="failSilently">Whether to not raise exceptions on fails, but instead return a null string</param>
            /// <returns>An identifier from the buffer</returns>
            [CanBeNull]
            public string ReadIdent(bool failSilently)
            {
                if (failSilently && EOF())
                    return null;

                char peek = Next();

                var ident = new StringBuilder();

                if (!((peek >= 65 && peek <= 90) || (peek >= 97 && peek <= 122)) && peek != '_')
                {
                    if (failSilently)
                        return null;

                    throw new Exception("Expected identifier");
                }
                _curChar--;
                ident.Append(Next());

                // Read all alphanumeric character until a non-alphanumeric character is hit
                while (!EOF())
                {
                    peek = Peek();

                    if (!((peek >= 65 && peek <= 90) || (peek >= 97 && peek <= 122) || (peek >= 48 && peek <= 57)) && peek != '_')
                    {
                        break;
                    }

                    ident.Append(peek);
                    _curChar++; // Move to the next character
                }

                return ident.ToString();
            }

            /// <summary>
            /// Skips all the whitespace that may be lying on the current position of the string
            /// </summary>
            public void SkipWhiteSpace()
            {
                char c;
                while (!EOF() && ((c = Peek()) == ' ' || c == '\t' || c == '\r' || c == '\n' || c == 160))
                {
                    _curChar++;
                }
            }

            /// <summary>
            /// Returns whether the buffer position is at the end of the buffer length
            /// </summary>
            /// <returns>Whether the buffer position is at the end of the buffer length</returns>
            // ReSharper disable once InconsistentNaming
            public bool EOF()
            {
                return _curChar >= _length;
            }

            /// <summary>
            /// Returns the next char in the buffer and advances the current buffer position
            /// </summary>
            /// <returns>The next char int the buffer</returns>
            public char Next()
            {
                // Throw an error when the end of the file has been reached prematurely
                if (EOF())
                {
                    throw new Exception("Unexpected end of file reached");
                }

                return _buffer[_curChar++];
            }

            /// <summary>
            /// Peeks the next char in the buffer
            /// </summary>
            /// <returns>The next char in the buffer</returns>
            public char Peek()
            {
                // Throw an error when the end of the file has been reached prematurely
                if (EOF())
                {
                    throw new Exception("Unexpected end of file reached");
                }

                return _buffer[_curChar];
            }

            /// <summary>
            /// The input string's length
            /// </summary>
            private readonly int _length;

            /// <summary>
            /// The current character being processed
            /// </summary>
            private int _curChar;

            /// <summary>
            /// The scene script
            /// </summary>
            private readonly string _buffer;
        }
    }

    /// <summary>
    /// Enumerator used to specify a type when asserting initial values
    /// </summary>
    public enum EnsureValueType
    {
        /// <summary>
        /// Specifies a string value type
        /// </summary>
        String,
        /// <summary>
        /// Specifies an integer value type
        /// </summary>
        Int,
        /// <summary>
        /// Specifies a boolean value type, that can either be 'true' or 'false'
        /// </summary>
        Boolean
    }
}
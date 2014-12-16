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

namespace Pixelaria.Utils
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
        private IniReader IniFile;

        /// <summary>
        /// Initializes the Settings class
        /// </summary>
        private Settings(string settingsFile)
        {
            IniFile = new IniReader(settingsFile);
            IniFile.LoadSettings();
        }

        /// <summary>
        /// Asserts that a specific value is present on the Settings object, and initializes it with a default value it of doesn't
        /// </summary>
        /// <param name="value">The path to the settings value</param>
        /// <param name="type">The type the value must have in order to be valid</param>
        /// <param name="defaultValue">The default value to be set if the value does not exists or does not matches the provided type</param>
        /// <returns>True when the value was asserted, false if it was not present or not valid</returns>
        public bool Assert(string value, AssertType type, string defaultValue)
        {
            if (GetValue(value) == null)
            {
                SetValue(value, defaultValue);
                return false;
            }

            switch (type)
            {
                case AssertType.Boolean:
                    if (GetValue(value) != "true" && GetValue(value) != "false")
                    {
                        SetValue(value, defaultValue);
                        return false;
                    }
                    break;
                case AssertType.Int:
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
            IniFile.SaveSettings();
        }

        /// <summary>
        /// Gets the given value from the values list
        /// </summary>
        /// <param name="value">A string representing the value saved. Returns null if the value is not currently present</param>
        public string GetValue(string valueName)
        {
            return IniFile.GetValue(valueName);
        }

        /// <summary>
        /// Sets the given value on the values list
        /// </summary>
        /// <param name="valueName">Thev value name to sabe</param>
        /// <param name="value">The value to save</param>
        public void SetValue(string valueName, string value)
        {
            IniFile.SetValue(valueName, value);
            IniFile.SaveSettings();
        }

        /// <summary>
        /// Returns the current settings singleton class
        /// </summary>
        /// <param name="path">The path for the settings file</param>
        /// <returns>The current settings singleton class</returns>
        public static Settings GetSettings(string path = "settings.ini")
        {
            if (settings == null)
                return settings = new Settings(path);
            else
                return settings;
        }

        /// <summary>
        /// The main settings singleton class
        /// </summary>
        private static Settings settings;
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
            FilePath = path;
            Values = new Dictionary<string, string>();
        }

        /// <summary>
        /// Loads the settings from the .ini file
        /// </summary>
        public void LoadSettings()
        {
            // Clear the values before adding the new ones
            Values.Clear();

            // Create the file if it does not exists
            if (!File.Exists(FilePath))
                File.Create(FilePath).Close();

            // Load the file line by line
            StreamReader reader = new StreamReader(FilePath, Encoding.UTF8);

            string currentPath = "";
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

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
                            else
                            {
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
                    }
                    // Comment line, ignore
                    else if (line.Trim()[0] == '\'')
                    {
                        continue;
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
                            Values[currentPath + "\\" + valueName] = value;
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
            }

            reader.Close();
            reader.Dispose();
        }

        /// <summary>
        /// Saves the current settings to the .ini file
        /// </summary>
        public void SaveSettings()
        {
            // Start by organizing the values into a tree
            SettingsNode baseNode = new SettingsNode();

            foreach (string value in Values.Keys)
            {
                string path = value;

                // Base settings, create values at the base node
                if (path.IndexOf("\\") == -1)
                {
                    baseNode[value] = Values[value];
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

                    baseNode.CreateNode(path)[subPath[subPath.Length - 1]] = Values[value];
                }
            }

            // Sort the nodes after they are created
            baseNode.Sort();

            // Mount the settings string
            StringBuilder output = new StringBuilder();

            baseNode.SaveToString(output);

            using (FileStream stream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);

                // Save the settings to the settings file now
                StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(output.ToString().Trim());
                writer.Close();
                writer.Dispose();
            }
        }

        /// <summary>
        /// Gets the given value from the values list
        /// </summary>
        /// <param name="value">A string representing the value saved. Returns null if the value is not currently present</param>
        public string GetValue(string valueName)
        {
            if (Values.ContainsKey(valueName))
            {
                return Values[valueName];
            }

            return null;
        }

        /// <summary>
        /// Sets the given value on the values list
        /// </summary>
        /// <param name="valueName">Thev value name to sabe</param>
        /// <param name="value">The value to save</param>
        public void SetValue(string valueName, string value)
        {
            Values[valueName] = value;
        }

        /// <summary>
        /// The values stored in the .ini file
        /// </summary>
        Dictionary<string, string> Values;

        /// <summary>
        /// The filepath to the .ini file
        /// </summary>
        string FilePath = "";

        /// <summary>
        /// Specifies a node that contains settings and subnodes
        /// </summary>
        private class SettingsNode
        {
            /// <summary>
            /// The name of this SettingsNode
            /// </summary>
            string Name = "";

            /// <summary>
            /// List of subnodes
            /// </summary>
            List<SettingsNode> SubNodes = new List<SettingsNode>();

            /// <summary>
            /// The parent node that owns this node
            /// </summary>
            SettingsNode ParentNode = null;

            /// <summary>
            /// Dictionary of settings and their respective values
            /// </summary>
            Dictionary<string, string> Values = new Dictionary<string, string>();

            /// <summary>
            /// Gets or sets a value from this SettingsNode
            /// </summary>
            /// <param name="valueName">The name of the value to modify</param>
            /// <returns>The string contents of the value</returns>
            public string this[string valueName]
            {
                get { return Values[valueName]; }
                set { Values[valueName] = value; }
            }

            /// <summary>
            /// Creates and returns the given node path
            /// If the node path already exists, it is not re-created
            /// </summary>
            /// <param name="path">The path to the node, separated by '\'</param>
            /// <returns>The topmost node of the node list</returns>
            public SettingsNode CreateNode(string path)
            {
                if (path == "")
                    return this;

                SettingsNode returnNode = null;
                string[] nodes = new string[1];

                if (path.IndexOf("\\") != -1)
                    nodes = path.Split('\\');
                else
                    nodes[0] = path;

                foreach (SettingsNode node in SubNodes)
                {
                    if (node.Name == nodes[0])
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

                    SettingsNode newNode = new SettingsNode();
                    newNode.ParentNode = this;
                    newNode.Name = nodes[0];

                    SubNodes.Add(newNode);

                    if (nextPath == "")
                    {
                        returnNode = newNode;
                    }
                    else
                    {
                        returnNode = newNode.CreateNode(nextPath);
                    }
                }

                return returnNode;
            }

            /// <summary>
            /// Saves this SettingsNode into the provided StringBuilder object
            /// </summary>
            /// <param name="builder">The StringBuilder to save the data to</param>
            public void SaveToString(StringBuilder builder)
            {
                string path = Name;
                SettingsNode curNode = ParentNode;
                while (curNode != null)
                {
                    if (curNode.Name != "")
                        path = curNode.Name + "\\" + path;

                    curNode = curNode.ParentNode;
                }

                if (path != "" && Values.Count > 0)
                {
                    builder.Append("["); builder.Append(path); builder.AppendLine("]");
                }

                // Save the values now
                foreach (string value in Values.Keys)
                {
                    builder.Append(value); builder.Append(" = "); builder.AppendLine(Values[value]);
                }

                if (Values.Count > 0)
                    builder.AppendLine();

                foreach (SettingsNode childNode in SubNodes)
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
                foreach (SettingsNode node in SubNodes)
                {
                    node.Clear();
                }

                // Clear the lists
                SubNodes.Clear();
                Values.Clear();

                // Clear the parent node reference
                ParentNode = null;
            }

            /// <summary>
            /// Sorts this and all children SettingsNode alphabetically
            /// </summary>
            public void Sort()
            {
                // Sort the subnodes
                for (int i = 0; i < SubNodes.Count; i++)
                {
                    for (int j = 0; j < SubNodes.Count - i - 1; j++)
                    {
                        if (SubNodes[j].Name.ToLower().CompareTo(SubNodes[j + 1].Name) > 0)
                        {
                            SettingsNode aux = SubNodes[j];
                            SubNodes[j] = SubNodes[j + 1];
                            SubNodes[j + 1] = aux;
                        }
                    }
                }

                // Sort children nodes now
                foreach (SettingsNode node in SubNodes)
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
                buffer = text;

                length = buffer.Length;
                curChar = offset;
            }

            /// <summary>
            /// Reads an identifier from the buffer
            /// </summary>
            /// <param name="failSilently">Whether to not raise exceptions on fails, but instead return a null string</param>
            /// <returns>An identifier from the buffer</returns>
            public string ReadIdent(bool failSilently)
            {
                if (failSilently && EOF())
                    return null;

                int charStart = curChar;
                char peek = Next();

                StringBuilder ident = new StringBuilder();

                if (!((peek >= 65 && peek <= 90) || (peek >= 97 && peek <= 122)) && peek != '_')
                {
                    if (failSilently)
                        return null;

                    throw new Exception("Expected identifier");
                }
                curChar--;
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
                    curChar++; // Move to the next character
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
                    curChar++;
                }
            }

            /// <summary>
            /// Returns whether the buffer position is at the end of the buffer length
            /// </summary>
            /// <returns>Whether the buffer position is at the end of the buffer length</returns>
            public bool EOF()
            {
                return (curChar >= length);
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

                return buffer[curChar++];
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

                return buffer[curChar];
            }

            /// <summary>
            /// The input string's length
            /// </summary>
            int length = 0;

            /// <summary>
            /// The current character being processed
            /// </summary>
            public int curChar = 0;

            /// <summary>
            /// The scene script
            /// </summary>
            string buffer;
        }
    }

    /// <summary>
    /// Enumerator used to specify a type when asserting initial values
    /// </summary>
    public enum AssertType
    {
        String,
        Int,
        Boolean
    }
}
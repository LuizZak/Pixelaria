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

using System.IO;
using JetBrains.Annotations;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Helper class used to manage recent files list
    /// </summary>
    public class RecentFileList
    {
        /// <summary>
        /// The list of files
        /// </summary>
        private readonly string[] _fileList;

        /// <summary>
        /// Returns the list of files currently in this RecentFileList object
        /// </summary>
        public int FileCount => _fileList.Length;

        /// <summary>
        /// Returns the file path at the given index
        /// </summary>
        /// <param name="index">The index to get the file path</param>
        /// <returns>The file path at the given index</returns>
        public string this[int index] => _fileList[index];

        /// <summary>
        /// Initializes a new instance of the RecentFileList class
        /// </summary>
        /// <param name="fileCount">The number of values to store at the list</param>
        public RecentFileList(int fileCount)
        {
            _fileList = new string[fileCount];

            // Load the values from the settings file
            var settings = Settings.GetSettings();

            for (int i = 0; i < fileCount; i++)
            {
                // Assert first to make sure the value exists
                settings.EnsureValue("Recent Files\\File" + i, EnsureValueType.String, "");

                _fileList[i] = settings.GetValue("Recent Files\\File" + i);
            }
        }

        /// <summary>
        /// Stores the given file to the recent file list, and automatically
        /// saves the list down to the settings file
        /// </summary>
        /// <param name="file">The file to store on this list</param>
        public void StoreFile([NotNull] string file)
        {
            var settings = Settings.GetSettings();

            string fullPath = Path.GetFullPath(file);

            // Don't do anything if the file is already at the top of the file list
            if (fullPath == _fileList[0])
                return;

            // Search if the file is not already in the list, and push it to the top
            int index = 0;

            for (int i = 1; i < _fileList.Length; i++)
            {
                if (fullPath == _fileList[i] || i == _fileList.Length - 1)
                {
                    index = (i == _fileList.Length - 1 ? i : i - 1);
                    break;
                }
            }

            // Push all current values down
            for (int i = index; i >= 0; i--)
            {
                if (i + 1 >= _fileList.Length)
                    continue;

                _fileList[i + 1] = _fileList[i];

                settings.SetValue("Recent Files\\File" + (i + 1), _fileList[i + 1]);
            }

            _fileList[0] = fullPath;

            settings.SetValue("Recent Files\\File" + 0, fullPath);
        }

        /// <summary>
        /// Removes an index from this RecentFileList
        /// </summary>
        /// <param name="index">The index of the item to remove</param>
        public void RemoveFromList(int index)
        {
            var settings = Settings.GetSettings();

            if (index == _fileList.Length - 1)
            {
                _fileList[index] = "";
                settings.SetValue("Recent Files\\File" + index, "");
                return;
            }

            // Push all current values down
            for (int i = index; i < _fileList.Length - 1; i++)
            {
                _fileList[i] = _fileList[i + 1];

                settings.SetValue("Recent Files\\File" + i, _fileList[i]);
            }

            _fileList[_fileList.Length - 1] = "";
            settings.SetValue("Recent Files\\File" + (_fileList.Length - 1), "");
        }
    }
}
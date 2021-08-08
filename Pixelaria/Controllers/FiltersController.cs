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
using System.Linq;
using JetBrains.Annotations;
using Pixelaria.Filters;
using PixLib.Filters;

namespace Pixelaria.Controllers
{
    /// <summary>
    /// A singleton class for controlling filters during runtime of the application
    /// </summary>
    internal class FiltersController
    {
        private readonly List<FilterPreset> _presets;

        /// <summary>
        /// Gets the singleton instance of this class
        /// </summary>
        public static FiltersController Instance { get; } = new FiltersController();

        /// <summary>
        /// Gets the latest filter presets for the filters controller
        /// </summary>
        public FilterPreset[] Presets => _presets.ToArray();

        /// <summary>
        /// Gets or sets the maximum number of presets for this filters controller
        /// </summary>
        public int MaxPresets { get; set; } = 10;

        /// <summary>
        /// Event fired whenever the preset data set has changed
        /// </summary>
        public event EventHandler FiltersChanged;

        private FiltersController()
        {
            _presets = new List<FilterPreset>();
        }

        /// <summary>
        /// Clears all filter presets currently recorded
        /// </summary>
        public void ClearFilterPresets()
        {
            _presets.Clear();

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Adds a set of filters to this filters controller
        /// </summary>
        /// <param name="filters">The filters to save</param>
        public void AddFilters([NotNull] IFilter[] filters)
        {
            if (filters.Length == 0)
                return;

            // Generate a new name for the filters
            string presetName = filters.Select(f => f.Name).Aggregate((res, name) => res + ", " + name);

            AddFilterPreset(new FilterPreset(presetName, filters));
        }

        /// <summary>
        /// Adds a new filter preset to this filters controller
        /// </summary>
        /// <param name="preset">The filter preset to save</param>
        public void AddFilterPreset([NotNull] FilterPreset preset)
        {
            var matching = MatchingPreset(preset);

            if (matching != null)
            {
                int index = _presets.IndexOf(matching);

                _presets.Insert(index, preset);
                _presets.Remove(matching);

                return;
            }

            while (_presets.Count > MaxPresets)
            {
                _presets.RemoveAt(_presets.Count - 1);
            }

            _presets.Insert(0, preset);

            FiltersChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns the first filter preset that matches the given preset.
        /// Returns null, if none are found
        /// </summary>
        /// <param name="preset">The filter preset to search using the .Equals(FilterPreset) method</param>
        /// <returns>A FilterPreset matching the given preset by using the .Equals(FilterPreset) method, or null, if none are found</returns>
        [CanBeNull]
        public FilterPreset MatchingPreset([NotNull] FilterPreset preset)
        {
            return _presets.FirstOrDefault(preset.Equals);
        }
    }
}

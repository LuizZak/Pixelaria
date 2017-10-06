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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Singleton class used to store the program's filters
    /// </summary>
    public class FilterStore
    {
        /// <summary>
        /// The list of filter items of the program
        /// </summary>
        List<FilterItem> _filterItems;

        /// <summary>
        /// The list of filter presets of the program
        /// </summary>
        List<FilterPreset> _filterPresets;

        /// <summary>
        /// Gets the list of filters of the program
        /// </summary>
        public string[] FiltersList => GetFilterList();

        /// <summary>
        /// Gets the list of icons for the filters
        /// </summary>
        public Image[] FilterIconList => GetFilterIconList();

        /// <summary>
        /// Gets the list of filter presets of the program
        /// </summary>
        public FilterPreset[] FilterPrests => _filterPresets.ToArray();

        /// <summary>
        /// Gets the singleton instance of the FilterStore for the program
        /// </summary>
        public static FilterStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FilterStore();
                    _instance.LoadFilterPresets();
                }
                
                return _instance;
            }
        }

        /// <summary>
        /// Initializes a new instance of the FolterStore class
        /// </summary>
        private FilterStore()
        {
            InitList();
        }

        /// <summary>
        /// Initializes the list of filters of the program
        /// </summary>
        private void InitList()
        {
            _filterItems = new List<FilterItem>();
            _filterPresets = new List<FilterPreset>();

            RegisterFilter("Transparency", Properties.Resources.filter_transparency_icon, typeof(TransparencyFilter), typeof(TransparencyControl));
            RegisterFilter("Scale", Properties.Resources.filter_scale_icon, typeof(ScaleFilter), typeof(ScaleControl));
            RegisterFilter("Offset", Properties.Resources.filter_offset_icon, typeof(OffsetFilter), typeof(OffsetControl));
            RegisterFilter("Fade Color", Properties.Resources.filter_fade_icon, typeof(FadeFilter), typeof(FadeControl));
            RegisterFilter("Rotation", Properties.Resources.filter_rotation_icon, typeof(RotationFilter), typeof(RotationControl));

            RegisterFilter("Hue", Properties.Resources.filter_hue, typeof(HueFilter), typeof(HueControl));
            RegisterFilter("Saturation", Properties.Resources.filter_saturation, typeof(SaturationFilter), typeof(SaturationControl));
            RegisterFilter("Lightness", Properties.Resources.filter_lightness, typeof(LightnessFilter), typeof(LightnessControl));

            RegisterFilter("Stroke", Properties.Resources.filter_stroke, typeof(StrokeFilter), typeof(StrokeControl));
        }

        /// <summary>
        /// Register a filter on this FilterStore
        /// </summary>
        /// <param name="filterName">The display name of the filter item</param>
        /// <param name="filterIcon">The icon for the filter</param>
        /// <param name="filterType">The type to use when creating an instance of the filter</param>
        /// <param name="filterControlType">The type to use when creating an instance of the filter's control</param>
        public void RegisterFilter(string filterName, Image filterIcon, Type filterType, Type filterControlType)
        {
            FilterItem item = new FilterItem { FilterName = filterName, FilterIcon = filterIcon, FilterType = filterType, FilterControlType = filterControlType };
            _filterItems.Add(item);
        }

        /// <summary>
        /// Creates a new instance of the specified filter with the given filter name
        /// </summary>
        /// <param name="filterName">The name of the filter to create</param>
        /// <returns>The filter instance</returns>
        [CanBeNull]
        public IFilter CreateFilter(string filterName)
        {
            IFilter filter = null;

            foreach (FilterItem item in _filterItems)
            {
                if (item.FilterName == filterName)
                {
                    var constructorInfo = item.FilterType.GetConstructor(Type.EmptyTypes);

                    if (constructorInfo != null)
                        filter = constructorInfo.Invoke(null) as IFilter;

                    break;
                }
            }

            return filter;
        }

        /// <summary>
        /// Creates the FilterControl for a specified filter
        /// </summary>
        /// <param name="filterName">The name of the filter to create the FilterControl out of</param>
        /// <returns>The created FilterControl, or null if the filter does not exists</returns>
        [CanBeNull]
        public FilterControl CreateFilterControl(string filterName)
        {
            FilterControl filterControl = null;

            foreach (var item in _filterItems)
            {
                if (item.FilterName == filterName)
                {
                    var constructorInfo = item.FilterControlType.GetConstructor(Type.EmptyTypes);
                    if (constructorInfo != null)
                        filterControl = constructorInfo.Invoke(null) as FilterControl;

                    break;
                }
            }

            return filterControl;
        }

        /// <summary>
        /// Records a filter preset of the given name with the given filters.
        /// If a filter preset with the given name already exists, it is overriden
        /// </summary>
        /// <param name="name">The name to give to the filter preset</param>
        /// <param name="filters">The filters to save on the filter preset</param>
        public void RecordFilterPreset(string name, IFilter[] filters)
        {
            // Search for a filter preset with the given name
            for (int i = 0; i < _filterPresets.Count; i++)
            {
                if (_filterPresets[i].Name == name)
                {
                    _filterPresets[i] = new FilterPreset(name, filters);
                    return;
                }
            }

            _filterPresets.Add(new FilterPreset(name, filters));

            // Save automatically after each Record call
            SaveFilterPresets();
        }

        /// <summary>
        /// Removes the FilterPreset that matches the given name.
        /// If no FilterPreset matches the name, no action is taken
        /// </summary>
        /// <param name="name">The name of the FilterPreset to remove</param>
        public void RemoveFilterPresetByName(string name)
        {
            foreach (FilterPreset preset in _filterPresets)
            {
                if (preset.Name == name)
                {
                    _filterPresets.Remove(preset);

                    SaveFilterPresets();

                    return;
                }
            }
        }

        /// <summary>
        /// Returns a FilterPreset stored on this FilterStore that matches the given name.
        /// If no FilterPreset object is found, null is returned instead
        /// </summary>
        /// <param name="name">The name of the filter preset to match</param>
        /// <returns>A FilterPreset stored on this FilterStore that matches the given name. If no FilterPreset object is found, null is returned instead</returns>
        [CanBeNull]
        public FilterPreset GetFilterPresetByName(string name)
        {
            return _filterPresets.FirstOrDefault(preset => preset.Name == name);
        }

        /// <summary>
        /// Returns an image binded to the given filter name
        /// </summary>
        /// <param name="name">The name of the filter</param>
        /// <returns>An image, binded to the given filter name</returns>
        public Image GetIconForFilter(string name)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (FilterItem item in _filterItems)
            {
                if (item.FilterName == name)
                    return item.FilterIcon;
            }

            return null;
        }

        /// <summary>
        /// Gets an array of all the filter display names of the program
        /// </summary>
        /// <returns>An array of all the filter display names of the program</returns>
        private string[] GetFilterList()
        {
            string[] filterNames = new string[_filterItems.Count];

            for (int i = 0; i < _filterItems.Count; i++)
            {
                filterNames[i] = _filterItems[i].FilterName;
            }

            return filterNames;
        }

        /// <summary>
        /// Gets an array of all the filter icons of the program
        /// </summary>
        /// <returns>An array of all the filter icons of the program</returns>
        private Image[] GetFilterIconList()
        {
            Image[] filterIcons = new Image[_filterItems.Count];

            for (int i = 0; i < _filterItems.Count; i++)
            {
                filterIcons[i] = _filterItems[i].FilterIcon;
            }

            return filterIcons;
        }

        /// <summary>
        /// Saves all the filter presets of the program to the disk
        /// </summary>
        private void SaveFilterPresets()
        {
            string savePath = Path.GetDirectoryName(Application.LocalUserAppDataPath) + "\\filterpresets.bin";

            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(_filterPresets.Count);

                foreach (FilterPreset preset in _filterPresets)
                {
                    preset.SaveToStream(stream);
                }
            }
        }

        /// <summary>
        /// Loads the filter presets from disk
        /// </summary>
        private void LoadFilterPresets()
        {
            _filterPresets.Clear();

            string savePath = Path.GetDirectoryName(Application.LocalUserAppDataPath) + "\\filterpresets.bin";

            using (FileStream stream = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                // No filters saved
                if (stream.Length == 0)
                    return;

                BinaryReader reader = new BinaryReader(stream);

                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    _filterPresets.Add(FilterPreset.FromStream(stream));
                }
            }
        }

        /// <summary>
        /// The singleton instance for the main FilterStore
        /// </summary>
        static FilterStore _instance;

        /// <summary>
        /// Provides a unified structure to store information about a filter
        /// </summary>
        private struct FilterItem
        {
            /// <summary>
            /// The display name of the filter item
            /// </summary>
            public string FilterName;

            /// <summary>
            /// The icon for the filter
            /// </summary>
            public Image FilterIcon;

            /// <summary>
            /// The type to use when creating an instance of the filter
            /// </summary>
            public Type FilterType;

            /// <summary>
            /// The type to use when creating an instance of the filter's control
            /// </summary>
            public Type FilterControlType;
        }
    }

    /// <summary>
    /// Specifies a filter preset object that holds information about a set of filters
    /// and their parameters that can be serialized from and to binary streams
    /// </summary>
    public class FilterPreset
    {
        /// <summary>
        /// The internal array of filter objects that compose this filter preset
        /// </summary>
        IFilter[] _filters;

        /// <summary>
        /// Gets or sets the display name for this FilterPreset
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the FilterPreset class
        /// </summary>
        private FilterPreset()
        {

        }

        /// <summary>
        /// Initializes a new instance of the FilterPreset class with a name and an array
        /// of IFilter objects to utilize as a preset
        /// </summary>
        /// <param name="name">A name for the preset</param>
        /// <param name="filters">An array of IFilter objects to utilize as a preset</param>
        public FilterPreset(string name, IFilter[] filters)
        {
            Name = name;
            _filters = filters;
        }

        /// <summary>
        /// Makes an array of filter controls based on the data stored on this FilterPreset
        /// </summary>
        /// <returns>An array of filter controls based on the data stored on this FilterPreset</returns>
        public FilterControl[] MakeFilterControls()
        {
            var filterControls = new FilterControl[_filters.Length];

            for(int i = 0; i < _filters.Length; i++)
            {
                var control = FilterStore.Instance.CreateFilterControl(_filters[i].Name);
                Debug.Assert(control != null, "control != null");
                control.SetFilter(_filters[i]);

                filterControls[i] = control;
            }

            return filterControls;
        }

        /// <summary>
        /// Returns whether this filter preset matches the given filter preset.
        /// Filter preset comparision is made by filters, the preset name and filter order is ignored
        /// </summary>
        /// <param name="other">Anoher filter preset to compare</param>
        /// <returns>true if all filters match the filters on the given preset; false otherwise</returns>
        public bool Equals([NotNull] FilterPreset other)
        {
            if (_filters.Length != other._filters.Length)
                return false;

            return _filters.Select(f1 => other._filters.Any(f1.Equals)).All(found => found);
        }

        /// <summary>
        /// Saves this FilterPreset to a stream
        /// </summary>
        /// <param name="stream">A stream to save this filter preset to</param>
        public void SaveToStream([NotNull] Stream stream)
        {
            var writer = new BinaryWriter(stream);

            writer.Write(Name);

            writer.Write(_filters.Length);

            foreach (var filter in _filters)
            {
                writer.Write(filter.Name);
                filter.SaveToStream(stream);
            }
        }

        /// <summary>
        /// Loads this FilterPreset from a stream
        /// </summary>
        /// <param name="stream">A stream to load this filter preset from</param>
        public void LoadFromStream([NotNull] Stream stream)
        {
            var reader = new BinaryReader(stream);

            Name = reader.ReadString();

            int count = reader.ReadInt32();

            _filters = new IFilter[count];

            for (int i = 0; i < count; i++)
            {
                var filter = FilterStore.Instance.CreateFilter(reader.ReadString());
                Debug.Assert(filter != null, "filter != null");
                filter.LoadFromStream(stream, 1);

                _filters[i] = filter;
            }
        }

        /// <summary>
        /// Reads a FilterPreset that was serialized from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the filter preset from</param>
        /// <returns>A FilterPreset that was read from the stream</returns>
        public static FilterPreset FromStream([NotNull] Stream stream)
        {
            var preset = new FilterPreset();

            preset.LoadFromStream(stream);

            return preset;
        }
    }
}
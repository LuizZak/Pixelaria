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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Filters
{
    /// <summary>
    /// Singleton class used to store the program's filters
    /// </summary>
    public class FilterStore
    {
        /// <summary>
        /// The list of filters of the program
        /// </summary>
        List<string> filterList;

        /// <summary>
        /// The list of icons for the filters
        /// </summary>
        List<Image> filterIconList;

        /// <summary>
        /// The list of filter presets of the program
        /// </summary>
        List<FilterPreset> filterPresets;

        /// <summary>
        /// Gets the list of filters of the program
        /// </summary>
        public string[] FiltersList { get { return filterList.ToArray(); } }

        /// <summary>
        /// Gets the list of icons for the filters
        /// </summary>
        public Image[] FilterIconList { get { return filterIconList.ToArray(); } }

        /// <summary>
        /// Gets the list of filter presets of the program
        /// </summary>
        public FilterPreset[] FilterPrests { get { return filterPresets.ToArray(); } }

        /// <summary>
        /// Gets the singleton instance of the FilterStore for the program
        /// </summary>
        public static FilterStore Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new FilterStore();
                    instance.LoadFilterPresets();
                }
                
                return instance;
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
            filterList = new List<string>();
            filterIconList = new List<Image>();

            filterList.Add("Transparency");
            filterIconList.Add(Pixelaria.Properties.Resources.filter_transparency_icon);

            filterList.Add("Scale");
            filterIconList.Add(Pixelaria.Properties.Resources.filter_scale_icon);

            filterList.Add("Offset");
            filterIconList.Add(Pixelaria.Properties.Resources.filter_offset_icon);

            filterList.Add("Fade Color");
            filterIconList.Add(Pixelaria.Properties.Resources.filter_fade_icon);
        }

        /// <summary>
        /// Creates a new instance of the specified filter with the given filter name
        /// </summary>
        /// <param name="filterName">The name of the filter to create</param>
        /// <returns>The filter instance</returns>
        public IFilter CreateFilter(string filterName)
        {
            IFilter filter = null;

            switch (filterName)
            {
                case "Transparency":
                    return new TransparencyFilter();

                case "Scale":
                    return new ScaleFilter();

                case "Offset":
                    return new OffsetFilter();

                case "Fade Color":
                    return new FadeFilter();
            }

            return filter;
        }

        /// <summary>
        /// Creates the FilterControl for a specified filter
        /// </summary>
        /// <param name="filterName">The name of the filter to create the FilterControl out of</param>
        /// <returns>The created FilterControl, or null if the filter does not exists</returns>
        public FilterControl CreateFilterControl(string filterName)
        {
            FilterControl filterControl = null;

            switch (filterName)
            {
                case "Transparency":
                    filterControl = new TransparencyControl();
                    break;
                case "Scale":
                    filterControl = new ScaleControl();
                    break;
                case "Offset":
                    filterControl = new OffsetControl();
                    break;
                case "Fade Color":
                    filterControl = new FadeControl();
                    break;
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
            for (int i = 0; i < filterPresets.Count; i++)
            {
                if (filterPresets[i].Name == name)
                {
                    filterPresets[i] = new FilterPreset(name, filters);
                    return;
                }
            }

            filterPresets.Add(new FilterPreset(name, filters));

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
            foreach (FilterPreset preset in filterPresets)
            {
                if (preset.Name == name)
                {
                    filterPresets.Remove(preset);

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
        public FilterPreset GetFilterPresetByName(string name)
        {
            foreach (FilterPreset preset in filterPresets)
            {
                if (preset.Name == name)
                    return preset;
            }

            return null;
        }

        /// <summary>
        /// Saves all the filter presets of the program to the disk
        /// </summary>
        private void SaveFilterPresets()
        {
            string savePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\filterpresets.bin";

            using (FileStream stream = new FileStream(savePath, FileMode.Create))
            {
                BinaryWriter writer = new BinaryWriter(stream);

                writer.Write(filterPresets.Count);

                foreach (FilterPreset preset in filterPresets)
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
            filterPresets = new List<FilterPreset>();

            string savePath = Path.GetDirectoryName(Application.ExecutablePath) + "\\filterpresets.bin";

            using (FileStream stream = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                // No filters saved
                if (stream.Length == 0)
                    return;

                BinaryReader reader = new BinaryReader(stream);

                int count = reader.ReadInt32();

                for (int i = 0; i < count; i++)
                {
                    filterPresets.Add(FilterPreset.FromStream(stream));
                }
            }
        }

        /// <summary>
        /// The singleton instance for the main FilterStore
        /// </summary>
        static FilterStore instance;
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
        IFilter[] filters;

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
            this.Name = name;
            this.filters = filters;
        }

        /// <summary>
        /// Makes an array of filter controls based on the data stored on this FilterPreset
        /// </summary>
        /// <returns>An array of filter controls based on the data stored on this FilterPreset</returns>
        public FilterControl[] MakeFilterControls()
        {
            FilterControl[] filterControls = new FilterControl[filters.Length];

            for(int i = 0; i < filters.Length; i++)
            {
                filterControls[i] = FilterStore.Instance.CreateFilterControl(filters[i].Name);
                filterControls[i].SetFilter(filters[i]);
            }

            return filterControls;
        }

        /// <summary>
        /// Saves this FilterPreset to a stream
        /// </summary>
        /// <param name="stream">A stream to save this filter preset to</param>
        public void SaveToStream(Stream stream)
        {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(Name);

            writer.Write(filters.Length);

            foreach (IFilter filter in filters)
            {
                writer.Write(filter.Name);
                filter.SaveToStream(stream);
            }
        }

        /// <summary>
        /// Loads this FilterPreset from a stream
        /// </summary>
        /// <param name="stream">A stream to load this filter preset from</param>
        public void LoadFromStream(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            Name = reader.ReadString();

            int count = reader.ReadInt32();

            filters = new IFilter[count];

            for (int i = 0; i < count; i++)
            {
                filters[i] = FilterStore.Instance.CreateFilter(reader.ReadString());
                filters[i].LoadFromStream(stream);
            }
        }

        /// <summary>
        /// Reads a FilterPreset that was serialized from the given stream
        /// </summary>
        /// <param name="stream">The stream to load the filter preset from</param>
        /// <returns>A FilterPreset that was read from the stream</returns>
        public static FilterPreset FromStream(Stream stream)
        {
            FilterPreset preset = new FilterPreset();

            preset.LoadFromStream(stream);

            return preset;
        }
    }
}
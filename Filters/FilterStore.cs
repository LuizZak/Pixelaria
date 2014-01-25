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
using System.Drawing;
using System.Linq;
using System.Text;

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
        /// Gets the list of filters of the program
        /// </summary>
        public string[] FiltersList { get { return filterList.ToArray(); } }

        /// <summary>
        /// Gets the list of filter icons of the program
        /// </summary>
        public Image[] FilterIconList { get { return filterIconList.ToArray(); } }

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

            filterList.Add("Fade");
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
                case "Fade":
                    filterControl = new FadeControl();
                    break;
            }

            return filterControl;
        }

        /// <summary>
        /// The singleton instance for the main FilterStore
        /// </summary>
        static FilterStore instance;
    }
}
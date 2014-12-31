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
using System.Drawing;
using System.Windows.Forms;

using Pixelaria.Data;
using Pixelaria.Filters;

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Implements an interface that the user can use to tweak settings of and apply a filter to an animation
    /// </summary>
    public partial class AnimationFilterView : Form
    {
        /// <summary>
        /// The animation to modify
        /// </summary>
        private readonly Animation _animation;

        /// <summary>
        /// The current frame bitmap being displayed
        /// </summary>
        private Bitmap _currentFrameBitmap;

        /// <summary>
        /// Initializes a new instance of the AnimationFilterView class
        /// </summary>
        /// <param name="animation">The animation to show the filter to</param>
        public AnimationFilterView(Animation animation)
        {
            InitializeComponent();

            _animation = animation;

            tc_timeline.Minimum = 1;
            tc_timeline.Maximum = animation.FrameCount;

            tc_timeline.Range = new Point(0, animation.FrameCount);

            SetDisplayFrame(animation.GetFrameAtIndex(0));

            pnl_errorPanel.Visible = false;

            btn_ok.Enabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="filters">The array of FilterControls to use as interface to mediate the interaction between the filters to be applied and the user</param>
        /// <param name="animation">The animation to apply the filter to</param>
        public AnimationFilterView(FilterControl[] filters, Animation animation)
            : this(animation)
        {
            fs_filters.LoadFilters(filters);
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="preset">A FilterPreset that contains data about filters to load on this BaseFilterView</param>
        /// <param name="animation">The animation to apply the filter to</param>
        public AnimationFilterView(FilterPreset preset, Animation animation)
            : this(animation)
        {
            fs_filters.LoadFilterPreset(preset);
        }

        /// <summary>
        /// Sets the current frame  being displayed on the filter view
        /// </summary>
        /// <param name="frame">The </param>
        private void SetDisplayFrame(IFrame frame)
        {
            if (_currentFrameBitmap != null)
            {
                _currentFrameBitmap.Dispose();
            }

            _currentFrameBitmap = frame.GetComposedBitmap();

            fs_filters.SetImage(_currentFrameBitmap);
        }

        /// <summary>
        /// Returns whether the current filter configuration can make any significant changes to the bitmap loaded
        /// </summary>
        /// <returns>Whether the current filter configuration can make any significant changes to the bitmap loaded</returns>
        public bool ChangesDetected()
        {
            return fs_filters.ChangesDetected();
        }

        /// <summary>
        /// Applies the filter to the animation
        /// </summary>
        public void ApplyFilter()
        {
            if (MessageBox.Show(@"Applying filters results in the flattening of the layers of all the frames. Do you wish to continue?", @"Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
            {
                return;
            }

            if (fs_filters.ChangesDetected())
            {
                Point range = tc_timeline.GetRange();

                for (int i = range.X - 1; i < range.X + range.Y; i++)
                {
                    Frame frame = _animation[i] as Frame;

                    if(frame != null)
                    {
                        foreach (FilterContainer container in fs_filters.FilterContainers)
                        {
                            Bitmap bitmap = frame.GetComposedBitmap();
                            container.ApplyFilter(bitmap);

                            // Remove all the layers from the frame
                            while (frame.LayerCount > 1)
                            {
                                frame.RemoveLayerAt(frame.LayerCount - 1);
                            }

                            frame.SetFrameBitmap(bitmap);
                        }
                    }
                }
            }
        }

        // 
        // Timeline frame changed
        // 
        private void tc_timeline_FrameChanged(object sender, FrameChangedEventArgs eventArgs)
        {
            SetDisplayFrame(_animation.GetFrameAtIndex(eventArgs.NewFrame - 1));
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            ApplyFilter();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
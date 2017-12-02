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
using JetBrains.Annotations;
using Pixelaria.Controllers;
using Pixelaria.Controllers.DataControllers;

using Pixelaria.Filters;

using Pixelaria.Views.Controls;
using Pixelaria.Views.Controls.Filters;

namespace Pixelaria.Views.ModelViews
{
    /// <summary>
    /// Implements an interface that the user can use to tweak settings of and apply a filter to an animation
    /// </summary>
    internal partial class AnimationFilterView : Form
    {
        /// <summary>
        /// The animation to modify
        /// </summary>
        private readonly AnimationController _animation;

        /// <summary>
        /// The current frame bitmap being displayed
        /// </summary>
        private Bitmap _currentFrameBitmap;

        /// <summary>
        /// Initializes a new instance of the AnimationFilterView class
        /// </summary>
        /// <param name="animation">The animation to show the filter to</param>
        public AnimationFilterView([NotNull] AnimationController animation)
        {
            InitializeComponent();

            _animation = animation;

            tc_timeline.Minimum = 1;
            tc_timeline.Maximum = animation.FrameCount;

            tc_timeline.Range = new Point(0, animation.FrameCount);

            SetDisplayFrame(_animation.GetFrameController(_animation.GetFrameAtIndex(0)).GetComposedBitmap());

            pnl_errorPanel.Visible = false;

            btn_ok.Enabled = true;
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="filters">The array of FilterControls to use as interface to mediate the interaction between the filters to be applied and the user</param>
        /// <param name="animation">The animation to apply the filter to</param>
        public AnimationFilterView([NotNull] IFilterControl[] filters, [NotNull] AnimationController animation)
            : this(animation)
        {
            fs_filters.LoadFilters(filters);
        }

        /// <summary>
        /// Initializes a new instance of the BaseFilterView class
        /// </summary>
        /// <param name="preset">A FilterPreset that contains data about filters to load on this BaseFilterView</param>
        /// <param name="animation">The animation to apply the filter to</param>
        public AnimationFilterView([NotNull] FilterPreset preset, [NotNull] AnimationController animation)
            : this(animation)
        {
            fs_filters.LoadFilterPreset(preset);
        }

        /// <summary>
        /// Sets the current frame  being displayed on the filter view
        /// </summary>
        /// <param name="frame">The </param>
        private void SetDisplayFrame(Bitmap frame)
        {
            _currentFrameBitmap?.Dispose();

            _currentFrameBitmap = frame;

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
        protected void ApplyFilter()
        {
            if (!fs_filters.ChangesDetected())
                return;

            // Verify if any of the frames on the animation have > 1 layer.
            // If so, present an alert warning changes will flatten the layers.
            bool hasLayersToFlatten = false;

            var range = tc_timeline.GetRange();

            for (int i = range.X - 1; i < range.X + range.Y; i++)
            {
                var frame = _animation.GetFrameAtIndex(i);
                var controller = _animation.GetFrameController(frame);

                if (controller.LayerCount > 1)
                {
                    hasLayersToFlatten = true;
                    break;
                }
            }

            if (hasLayersToFlatten)
            {
                if (MessageBox.Show(
                        @"Applying filters to the selected frames will result in flattening of all layers to a single layer per frame. Do you wish to continue?",
                        @"Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                {
                    return;
                }
            }
            
            for (int i = range.X - 1; i < range.X + range.Y; i++)
            {
                var frame = _animation.GetFrameAtIndex(i);
                var controller = _animation.GetFrameController(frame);

                foreach (var container in fs_filters.FilterContainers)
                {
                    var bitmap = controller.GetComposedBitmap();
                    container.ApplyFilter(bitmap);

                    // Remove all the layers from the frame
                    while (controller.LayerCount > 1)
                    {
                        controller.RemoveLayerAt(controller.LayerCount - 1);
                    }

                    controller.SetFrameBitmap(bitmap);
                }
            }

            // Save the preset
            FiltersController.Instance.AddFilters(fs_filters.Filters);
        }

        // 
        // Timeline frame changed
        // 
        private void tc_timeline_FrameChanged(object sender, [NotNull] FrameChangedEventArgs eventArgs)
        {
            SetDisplayFrame(_animation.GetFrameController(_animation.GetFrameAtIndex(eventArgs.NewFrame - 1)).GetComposedBitmap());
        }

        // 
        // Ok button click
        // 
        private void btn_ok_Click(object sender, EventArgs e)
        {
            if (!fs_filters.ChangesDetected())
            {
                MessageBox.Show(
                    @"No changes will be made with the filter presets present (i.e. all frames will look the same!)",
                    @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplyFilter();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
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
using JetBrains.Annotations;
using PixUI.LayoutSystem;

namespace PixUI.Controls.ToolStrip
{
    /// <summary>
    /// A tool-strip menu bar that presents items in a linear menu anchored to a window or view
    /// </summary>
    public class ToolStripMenu : ControlView
    {
        /// <summary>
        /// Default size of tool strip bar depth.
        ///
        /// Can either be used as a width or a height, depending on the bar's orientation while anchored in a view.
        /// </summary>
        public const float BarSize = 25;

        /// <summary>
        /// Gets the orientation of this tool strip menu within its parent view.
        /// </summary>
        public ToolStripOrientation Orientation { get; internal set; }

        /// <summary>
        /// Creates a new instance of <see cref="ToolStripMenu"/>
        /// </summary>
        public static ToolStripMenu Create()
        {
            var menu = new ToolStripMenu();

            menu.Initialize();

            return menu;
        }

        protected ToolStripMenu()
        {

        }

        protected virtual void Initialize()
        {
            // Layout and colors
        }

        /// <summary>
        /// Anchors this tool strip to a given view, on a given side of its bounds.
        ///
        /// This method also adds the tool strip as a child view to <see cref="view"/>.
        /// </summary>
        /// <param name="view">A view to anchor to</param>
        /// <param name="position">The side of the view to anchor to</param>
        public void AnchorIntoView([NotNull] ControlView view, ToolStripAnchorPosition position)
        {
            RemoveConstraints();

            view.AddChild(this);
            
            switch (position)
            {
                case ToolStripAnchorPosition.Top:
                    Orientation = ToolStripOrientation.Horizontal;

                    LayoutConstraint.Create(Anchors.Height, constant: BarSize);

                    LayoutConstraint.Create(Anchors.Top, view.Anchors.Top);
                    LayoutConstraint.Create(Anchors.Left, view.Anchors.Left);
                    LayoutConstraint.Create(Anchors.Right, view.Anchors.Right);
                    break;

                case ToolStripAnchorPosition.Left:
                    Orientation = ToolStripOrientation.Vertical;

                    LayoutConstraint.Create(Anchors.Width, constant: BarSize);

                    LayoutConstraint.Create(Anchors.Left, view.Anchors.Left);
                    LayoutConstraint.Create(Anchors.Top, view.Anchors.Top);
                    LayoutConstraint.Create(Anchors.Bottom, view.Anchors.Bottom);
                    break;

                case ToolStripAnchorPosition.Right:
                    Orientation = ToolStripOrientation.Vertical;

                    LayoutConstraint.Create(Anchors.Width, constant: BarSize);

                    LayoutConstraint.Create(Anchors.Right, view.Anchors.Right);
                    LayoutConstraint.Create(Anchors.Top, view.Anchors.Top);
                    LayoutConstraint.Create(Anchors.Bottom, view.Anchors.Bottom);
                    break;

                case ToolStripAnchorPosition.Bottom:
                    Orientation = ToolStripOrientation.Horizontal;

                    LayoutConstraint.Create(Anchors.Height, constant: BarSize);

                    LayoutConstraint.Create(Anchors.Bottom, view.Anchors.Bottom);
                    LayoutConstraint.Create(Anchors.Left, view.Anchors.Left);
                    LayoutConstraint.Create(Anchors.Right, view.Anchors.Right);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }
        }
    }

    public enum ToolStripAnchorPosition
    {
        Top,
        Left,
        Right,
        Bottom
    }

    /// <summary>
    /// Defines the orientation of a <see cref="ToolStripMenu"/>
    /// </summary>
    public enum ToolStripOrientation
    {
        /// <summary>
        /// The tool strip is laid horizontally across the top or bottom side of a view
        /// </summary>
        Horizontal,
        /// <summary>
        /// The tool strip is laid vertically across the left or right side of a view
        /// </summary>
        Vertical
    }
}

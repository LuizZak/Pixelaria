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
using JetBrains.Annotations;
using PixUI.LayoutSystem;

namespace PixUI.Controls.ToolStrip
{
    /// <summary>
    /// A tool-strip menu bar that presents items in a linear menu anchored to a window or view
    /// </summary>
    public class ToolStripMenu : ControlView
    {
        private readonly StatedValueStore<ToolStripMenuVisualStyleParameters> _statesStyles = new StatedValueStore<ToolStripMenuVisualStyleParameters>();

        /// <summary>
        /// Default size of tool strip bar depth.
        ///
        /// Can either be used as a width or a height, depending on the bar's orientation while anchored in a view.
        /// </summary>
        public const float BarSize = 25;

        /// <summary>
        /// Gets the orientation of this tool strip menu within its parent view.
        ///
        /// Defaults to <see cref="ToolStripOrientation.Horizontal"/>
        /// </summary>
        public ToolStripOrientation Orientation { get; internal set; } = ToolStripOrientation.Horizontal;

        /// <summary>
        /// Gets the current active style for this tool strip menu.
        /// </summary>
        public ToolStripMenuVisualStyleParameters Style { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="ToolStripMenu"/>
        /// </summary>
        public static ToolStripMenu Create(bool darkTheme = true)
        {
            var menu = new ToolStripMenu();

            menu.Initialize();

            if (darkTheme)
            {
                menu.SetStyleForState(ToolStripMenuVisualStyleParameters.DefaultDarkStyle(), ControlViewState.Normal);
            }
            else
            {
                menu.SetStyleForState(ToolStripMenuVisualStyleParameters.DefaultLightStyle(), ControlViewState.Normal);
            }

            return menu;
        }

        protected ToolStripMenu()
        {

        }

        protected virtual void Initialize()
        {
            // Layout and colors
        }

        #region Visual Style Settings

        /// <summary>
        /// Sets the visual style of this tool strip menu when it's under a given view state.
        /// </summary>
        public void SetStyleForState(ToolStripMenuVisualStyleParameters visualStyle, ControlViewState state)
        {
            _statesStyles.SetValue(visualStyle, state);

            if (CurrentState == state)
            {
                ApplyStyle(visualStyle);
            }
        }

        /// <summary>
        /// Removes the special style for a given control view state.
        /// 
        /// Note that <see cref="ControlViewState.Normal"/> styles are the default styles and cannot be removed.
        /// </summary>
        public void RemoveStyleForState(ControlViewState state)
        {
            if (state == ControlViewState.Normal)
                return;

            _statesStyles.RemoveValueForState(state);
        }

        /// <summary>
        /// Gets the visual style for a given state.
        /// 
        /// If no custom visual style is specified for the state, the normal state style is returned instead.
        /// </summary>
        public ToolStripMenuVisualStyleParameters GetStyleForState(ControlViewState state)
        {
            return _statesStyles.GetValue(state);
        }

        private void ApplyStyle(ToolStripMenuVisualStyleParameters style)
        {
            Style = style;
            
            BackColor = style.BackgroundColor;
            
            Invalidate();
        }

        #endregion

        /// <summary>
        /// Anchors this tool strip to a given view, on a given side of its bounds.
        ///
        /// This method also adds the tool strip as a child view to <see cref="view"/>.
        /// </summary>
        /// <param name="view">A view to anchor to</param>
        /// <param name="position">The side of the view to anchor to</param>
        public void AnchorIntoView([NotNull] BaseView view, ToolStripAnchorPosition position)
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

    /// <summary>
    /// Defines the visual style parameters for a <see cref="ToolStripMenu"/>
    /// </summary>
    public struct ToolStripMenuVisualStyleParameters
    {
        public Color BackgroundColor { get; set; }

        public ToolStripMenuVisualStyleParameters(Color backgroundColor)
        {
            BackgroundColor = backgroundColor;
        }

        public static ToolStripMenuVisualStyleParameters DefaultDarkStyle()
        {
            return new ToolStripMenuVisualStyleParameters(Color.Black);
        }

        public static ToolStripMenuVisualStyleParameters DefaultLightStyle()
        {
            return new ToolStripMenuVisualStyleParameters(Color.FromKnownColor(KnownColor.Control));
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

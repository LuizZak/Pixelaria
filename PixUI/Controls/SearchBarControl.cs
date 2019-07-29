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
using PixCore.Geometry;

namespace PixUI.Controls
{
    public partial class SearchBarControl: ControlView
    {
        private TextField _textField;
        private ButtonControl _cancelButton;
        
        /// <summary>
        /// Creates a new instance of <see cref="SearchBarControl"/>
        /// </summary>
        public static SearchBarControl Create(bool darkStyle = true)
        {
            var searchBar = new SearchBarControl();
            searchBar.Initialize(darkStyle);

            return searchBar;
        }

        protected SearchBarControl()
        {

        }

        protected virtual void Initialize(bool darkStyle)
        {
            _textField = TextField.Create(darkStyle);
            _cancelButton = ButtonControl.Create();
        }

        protected override void OnResize()
        {
            _textField.Size = Size;
            _cancelButton.SetFrame(AABB.FromRectangle(Width - 25, Height / 2 - 10, 20, 20));
        }
    }

    // Reactive bindings for SearchBarControl
    public partial class SearchBarControl
    {
        /// <summary>
        /// On subscription, returns the current text value and receives updates
        /// of next subsequent text values as the user updates it.
        /// </summary>
        public IObservable<string> RxSearchTextUpdated => _textField.RxTextUpdated;
    }
}

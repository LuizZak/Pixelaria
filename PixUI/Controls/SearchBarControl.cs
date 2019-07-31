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
using PixCore.Geometry;

namespace PixUI.Controls
{
    public partial class SearchBarControl: ControlView
    {
        private TextField _textField;
        private ButtonControl _cancelButton;

        /// <summary>
        /// An optional placeholder text which is printed when the search bar text field's contents are empty.
        /// </summary>
        [CanBeNull]
        public string PlaceholderText
        {
            get => _textField.PlaceholderText;
            set => _textField.PlaceholderText = value;
        }
        
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
            _cancelButton.ManagedImage = darkStyle ? DefaultResources.Images.CancelButton : DefaultResources.Images.CancelButtonDark;
            _cancelButton.Text = "";
            _cancelButton.ColorMode = ButtonControl.ButtonColorMode.TintImage;
            _cancelButton.NormalColor = Color.Gray;
            _cancelButton.HighlightColor = Color.DarkGray;
            _cancelButton.SelectedColor = Color.Gray;
            _cancelButton.BackColor = Color.Transparent;

            AddChild(_textField);
            AddChild(_cancelButton);

            OnResize();
            SetupReactiveBindings();
        }

        protected override void OnResize()
        {
            _textField.Size = Size;
            _cancelButton.SetFrame(AABB.FromRectangle(Width - 25, Height / 2 - 10, 20, 20));
        }

        private void SetupReactiveBindings()
        {
            RxSearchTextUpdated
                .Subscribe(s =>
                {
                    _cancelButton.Visible = !string.IsNullOrEmpty(s);
                });

            _cancelButton.Rx.MouseClick
                .Subscribe(_ =>
                {
                    _textField.Text = "";
                    _textField.ResignFirstResponder();
                });
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

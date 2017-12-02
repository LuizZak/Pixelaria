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

using System.Collections;
using System.Collections.Generic;

namespace Pixelaria.PixUI.Controls.ContextMenu
{
    /// <summary>
    /// An item for a <see cref="ContextMenuControl"/> which is capable of containing
    /// more subitems itself.
    /// </summary>
    internal class ContextMenuDropDownItem : ContextMenuItem
    {
        /// <summary>
        /// Gets or sets the display name for this context menu item
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets the collection of sub-items on this context menu item
        /// </summary>
        public ContextMenuItemCollection DropDownItems { get; }

        public ContextMenuDropDownItem()
        {
            DropDownItems = new ContextMenuItemCollection(this);
        }
        
        protected void ItemsCollectionChanged()
        {
            
        }

        /// <summary>
        /// A collection of drop down menu items on this drop down item
        /// </summary>
        internal class ContextMenuItemCollection : IList<ContextMenuItem>
        {
            private readonly ContextMenuDropDownItem _dropDownItem;
            private readonly List<ContextMenuItem> _items = new List<ContextMenuItem>();

            public ContextMenuItem this[int index]
            {
                get => _items[index];
                set => _items[index] = value;
            }

            public int Count => _items.Count;
            public bool IsReadOnly => false;

            public ContextMenuItemCollection(ContextMenuDropDownItem dropDownItem)
            {
                _dropDownItem = dropDownItem;
            }

            public IEnumerator<ContextMenuItem> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(ContextMenuItem dropDownItem)
            {
                _items.Add(dropDownItem);
                if (dropDownItem != null)
                    dropDownItem.DropDownItem = _dropDownItem;

                _dropDownItem.ItemsCollectionChanged();
            }

            public void Clear()
            {
                _items.Clear();
                _dropDownItem.ItemsCollectionChanged();
            }

            public bool Contains(ContextMenuItem dropDownItem)
            {
                return _items.Contains(dropDownItem);
            }

            public void CopyTo(ContextMenuItem[] array, int arrayIndex)
            {
                _items.CopyTo(array, arrayIndex);
            }

            public bool Remove(ContextMenuItem dropDownItem)
            {
                var removed = _items.Remove(dropDownItem);

                _dropDownItem.ItemsCollectionChanged();

                return removed;
            }

            public int IndexOf(ContextMenuItem item)
            {
                return _items.IndexOf(item);
            }

            public void Insert(int index, ContextMenuItem item)
            {
                _items.Insert(index, item);
            }

            public void RemoveAt(int index)
            {
                _items.RemoveAt(index);
            }
        }
    }
}
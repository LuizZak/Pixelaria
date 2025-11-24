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

using PixCore.Geometry;
using PixCore.Text;
using PixCore.Text.Attributes;
using PixUI;
using PixUI.Controls;
using PixUI.Controls.ContextMenu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Pixelaria.Utils
{
    /// <summary>
    /// Provides scaffolding for generating a searching context menu control.
    /// </summary>
    internal class SearchContextMenuManager
    {
        public delegate void ItemClickEventHandler(object sender, SearchContextMenuItemSelectedEventArgs e);
        public delegate void ItemSelectedEventHandler(object sender, SearchContextMenuItemSelectedEventArgs e);
        public delegate void ItemMouseEnterEventHandler(object sender, SearchContextMenuItemSelectedEventArgs e);
        public delegate void ItemMouseLeaveEventHandler(object sender, SearchContextMenuItemSelectedEventArgs e);

        private readonly string[] _items;

        /// <summary>
        /// Event issued when an item from the context menu is clicked.
        /// 
        /// Is also issued when the 'enter' key is pressed while an item is highlighted.
        /// </summary>
        public event ItemClickEventHandler ItemClick;

        /// <summary>
        /// Event issued when an item has been selected, either by highlighting with the mouse, or with the
        /// arrow keys.
        /// </summary>
        public event ItemSelectedEventHandler ItemSelected;

        /// <summary>
        /// Event issued when the mouse has entered a particular toolstrip menu item.
        /// 
        /// This event is not raised for the search textbox item itself.
        /// </summary>
        public event ItemMouseEnterEventHandler ItemMouseEnter;

        /// <summary>
        /// Event issued when the mouse has left a particular toolstrip menu item.
        /// 
        /// This event is not raised for the search textbox item itself.
        /// </summary>
        public event ItemMouseLeaveEventHandler ItemMouseLeave;

        public SearchContextMenuManager(IEnumerable<string> items)
        {
            _items = items.ToArray();
        }

        public SearchContextMenuManager(params string[] items)
        {
            _items = items;
        }

        /// <summary>
        /// Generates the context menu control to display.
        /// </summary>
        public ContextMenuControl GenerateContextMenuControl()
        {
            var _dropDown = new ContextMenuDropDownItem("root");

            var allItems = new List<ContextMenuDropDownItem>();
            var visibleItems = new List<ContextMenuDropDownItem>();

            var searchBox = TextField.Create(true);
            searchBox.Layout();
            searchBox.Size = new Vector(100, 26);
            searchBox.TextChanged += (sender, args) =>
            {
                visibleItems.Clear();

                if (string.IsNullOrEmpty(args.Text))
                {
                    foreach (var item in allItems)
                    {
                        item.Visible = true;
                        item.AttributedName = new AttributedText(item.Name);
                        visibleItems.Add(item);
                    }
                }
                else
                {
                    foreach (var item in allItems)
                    {
                        var index = item.Name.IndexOf(args.Text, StringComparison.InvariantCultureIgnoreCase);
                        var textBuilder = new AttributedTextBuilder(item.Name);

                        if (index != -1)
                        {
                            item.Visible = true;
                            textBuilder.SetAttributes(new TextRange(index, args.Text.Length), new ITextAttribute[]
                            {
                                new BackgroundColorAttribute(Color.Blue),
                                new TextFontAttribute(new Font(ContextMenuControl.DefaultItemFont, FontStyle.Bold)),
                            });

                            visibleItems.Add(item);
                        }
                        else
                        {
                            item.Visible = false;
                        }

                        item.AttributedName = textBuilder.MakeAttributedText();
                    }
                }
            };
            searchBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Down)
                {
                    args.Handled = true;
                    args.SuppressKeyPress = true;

                    int selectedIndex = -1;

                    for (int i = 0; i < visibleItems.Count; i++)
                    {
                        if (visibleItems[i].Selected)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    if (selectedIndex < visibleItems.Count - 1)
                        selectedIndex++;

                    if (selectedIndex > -1 && selectedIndex < visibleItems.Count)
                    {
                        var item = visibleItems[selectedIndex];

                        item.Select();
                    }
                }
                else if (args.KeyCode == Keys.Up)
                {
                    args.Handled = true;
                    args.SuppressKeyPress = true;
                    int selectedIndex = -1;

                    for (int i = 0; i < visibleItems.Count; i++)
                    {
                        if (visibleItems[i].Selected)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    if (selectedIndex > 0)
                        selectedIndex--;

                    if (selectedIndex > -1 && visibleItems.Count > 0)
                    {
                        var item = visibleItems[selectedIndex];

                        item.Select();
                    }
                }
                else if (args.KeyCode == Keys.Enter)
                {
                    args.Handled = true;
                    args.SuppressKeyPress = true;

                    foreach (var item in allItems)
                    {
                        if (item.Visible && item.Selected)
                        {
                            item.PerformClick();
                            args.SuppressKeyPress = true;
                            args.Handled = true;
                            break;
                        }
                    }
                }
            };

            _dropDown.DropDownItems.Add(new ContextMenuControlHostItem(searchBox) { CreateConstraints = false });

            _dropDown.DropDownItems.Add(new ContextMenuSeparatorItem());

            for (int i = 0; i < _items.Length; i++)
            {
                int index = i;
                var potentialNode = _items[i];
                var item = _dropDown.DropDownItems.Add(potentialNode);

                allItems.Add(item);
                visibleItems.Add(item);

                item.SelectChange += (sender, e) =>
                {
                    if (item.Selected)
                        ItemSelected?.Invoke(this, new SearchContextMenuItemSelectedEventArgs(index));
                };
                item.MouseEnter += (sender, e) =>
                {
                    ItemMouseEnter?.Invoke(this, new SearchContextMenuItemSelectedEventArgs(index));
                };
                item.MouseLeave += (sender, e) =>
                {
                    ItemMouseLeave?.Invoke(this, new SearchContextMenuItemSelectedEventArgs(index));
                };
                item.Click += (sender, e) =>
                {
                    ItemClick?.Invoke(this, new SearchContextMenuItemSelectedEventArgs(index));
                };
            }

            var contextMenu = ContextMenuControl.Create(_dropDown);
            contextMenu.AreaIntoConstraintsMask = BoundsConstraintMask.Size;
            contextMenu.Layout();
            contextMenu.Opened += (sender, e) =>
            {
                searchBox.BecomeFirstResponder();
            };

            return contextMenu;
        }

        /// <summary>
        /// Generates the context menu to display.
        /// </summary>
        public ContextMenuStrip GenerateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            var searchBox = new ToolStripTextBox();
            contextMenu.Items.Add(searchBox);

            var allItems = new List<ToolStripRichTextLabel>();
            var visibleItems = new List<ToolStripRichTextLabel>();

            // Add individual items
            for (int i = 0; i < _items.Length; i++)
            {
                string item = _items[i];
                var index = i;
                var menuItem = new ToolStripRichTextLabel(item);
                contextMenu.Items.Add(menuItem);

                allItems.Add(menuItem);
                visibleItems.Add(menuItem);

                menuItem.MouseEnter += (sender, args) =>
                {
                    ItemMouseEnter?.Invoke(sender, new SearchContextMenuItemSelectedEventArgs(index));
                };
                menuItem.MouseLeave += (sender, args) =>
                {
                    ItemMouseLeave?.Invoke(sender, new SearchContextMenuItemSelectedEventArgs(index));
                };
                menuItem.Click += (sender, args) =>
                {
                    ItemClick?.Invoke(sender, new SearchContextMenuItemSelectedEventArgs(index));
                };
            }

            int toAllItemsIndex(int visibleItemIndex)
            {
                for (int i = 0; i < allItems.Count; i++)
                {
                    if (allItems[i].Visible)
                        visibleItemIndex -= 1;

                    if (visibleItemIndex < 0)
                        return i;
                }

                return -1;
            }

            // Auto-focus search box on open
            contextMenu.Opened += (sender, args) =>
            {
                searchBox.TextBox.Focus();
            };

            searchBox.TextChanged += (sender, args) =>
            {
                var searchTerm = searchBox.Text;
                visibleItems.Clear();

                foreach (var item in allItems)
                {
                    var visible = item.Text.ToLower().Contains(searchTerm.ToLower());
                    
                    if (visible)
                    {
                        visibleItems.Add(item);
                    }

                    item.Visible = visible;

                    var textBuilder = new AttributedTextBuilder(item.AttributedText);
                    textBuilder.ClearAttributes();

                    var index = item.Text.IndexOf(searchTerm, System.StringComparison.InvariantCultureIgnoreCase);
                    if (searchTerm.Length > 0 && index > -1)
                    {
                        textBuilder.SetAttributes(new TextRange(index, searchTerm.Length), new ITextAttribute[] {
                            new BackgroundColorAttribute(Color.Cyan)
                        });
                    }

                    item.AttributedText = textBuilder.MakeAttributedText();
                }
            };
            searchBox.TextBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode == Keys.Escape)
                {
                    args.SuppressKeyPress = true;
                    args.Handled = true;
                    contextMenu.Close();
                }
                else if (args.KeyCode == Keys.Down)
                {
                    args.SuppressKeyPress = true;
                    args.Handled = true;

                    int selectedIndex = -1;

                    for (int i = 0; i < visibleItems.Count; i++)
                    {
                        if (visibleItems[i].Selected)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    if (selectedIndex < visibleItems.Count - 1)
                        selectedIndex++;
                    
                    if (selectedIndex > -1 && selectedIndex < visibleItems.Count)
                    {
                        var item = visibleItems[selectedIndex];

                        item.Select();

                        var allItemsIndex = toAllItemsIndex(selectedIndex);
                        if (allItemsIndex != -1)
                            ItemSelected?.Invoke(sender, new SearchContextMenuItemSelectedEventArgs(allItemsIndex));
                    }
                }
                else if (args.KeyCode == Keys.Up)
                {
                    args.SuppressKeyPress = true;
                    args.Handled = true;

                    int selectedIndex = -1;

                    for (int i = 0; i < visibleItems.Count; i++)
                    {
                        if (visibleItems[i].Selected)
                        {
                            selectedIndex = i;
                            break;
                        }
                    }

                    if (selectedIndex > 0)
                        selectedIndex--;

                    if (selectedIndex > -1 && visibleItems.Count > 0)
                    {
                        var item = visibleItems[selectedIndex];

                        item.Select();

                        var allItemsIndex = toAllItemsIndex(selectedIndex);
                        if (allItemsIndex != -1)
                            ItemSelected?.Invoke(sender, new SearchContextMenuItemSelectedEventArgs(allItemsIndex));
                    }
                }
                else if (args.KeyCode == Keys.Enter)
                {
                    foreach (var item in allItems)
                    {
                        if (item.Visible && item.Selected)
                        {
                            item.PerformClick();
                            args.SuppressKeyPress = true;
                            args.Handled = true;
                            break;
                        }
                    }
                }
            };

            return contextMenu;
        }

        private class ToolStripRichTextLabel : ToolStripMenuItem
        {
            AttributedText _attributedText;

            public AttributedText AttributedText
            {
                get
                {
                    return _attributedText;
                }
                set
                {
                    _attributedText = value;
                    Invalidate();
                }
            }

            public ToolStripRichTextLabel() : base()
            {
                _attributedText = new AttributedText();
            }

            public ToolStripRichTextLabel(string text) : this()
            {
                _attributedText = new AttributedText(text);
                Text = text;
            }

            public ToolStripRichTextLabel(AttributedText text) : this()
            {
                AttributedText = text;
                Text = text.String;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (Selected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.LightBlue), e.ClipRectangle);
                }

                var lastPoint = PointF.Empty;
                lastPoint.X += 34;
                lastPoint.Y += 2;

                var text = AttributedText.String;

                // TODO: Improve character ranges construction to include full background color spans instead of all individual characters.
                var characterRanges = new CharacterRange[text.Length];
                for (int i = 0; i < text.Length; i++)
                {
                    characterRanges[i] = new CharacterRange(i, 1);
                }

                var stringFormat = new StringFormat();
                stringFormat.FormatFlags = StringFormatFlags.NoClip;
                stringFormat.SetMeasurableCharacterRanges(characterRanges);

                var charRegions = e.Graphics.MeasureCharacterRanges(text, Font, new RectangleF(PointF.Empty, Size), stringFormat);
                var lastCharIndex = 0;

                var backBoxPoint = lastPoint;

                // Draw background
                foreach (var segment in AttributedText.GetTextSegments())
                {
                    var backColorAttribute = segment.GetAttributeNullable<BackgroundColorAttribute>();
                    if (backColorAttribute != null)
                    {
                        var range = charRegions.Skip(lastCharIndex).Take(segment.Text.Length).ToArray();
                        
                        var sumRegion = range[0].Clone();
                        foreach (var charBox in range)
                        {
                            sumRegion.Union(charBox);
                        }

                        var sumRect = sumRegion.GetBounds(e.Graphics);

                        var backBrush = new SolidBrush(backColorAttribute.Value.BackColor);
                        var inflatedSize = sumRect.Size + new SizeF(backColorAttribute.Value.Inflation.X, backColorAttribute.Value.Inflation.Y);
                        var inflatedPoint = sumRect.Location + new SizeF(backBoxPoint) - new SizeF(backColorAttribute.Value.Inflation.X / 2, backColorAttribute.Value.Inflation.Y / 2);

                        e.Graphics.FillRectangle(backBrush, new RectangleF(inflatedPoint, inflatedSize));
                    }

                    lastCharIndex += segment.Text.Length;
                }

                e.Graphics.DrawString(AttributedText.String, Font, new SolidBrush(Color.Black), lastPoint);
            }

            public override Size GetPreferredSize(Size constrainingSize)
            {
                return base.GetPreferredSize(constrainingSize);
            }
        }
    }

    /// <summary>
    /// Event args for a <see cref="SearchContextMenuManager.ItemClicked"/> event.
    /// </summary>
    internal class SearchContextMenuItemSelectedEventArgs
    {
        public int Index { get; }

        public SearchContextMenuItemSelectedEventArgs(int index)
        {
            Index = index;
        }
    }
}

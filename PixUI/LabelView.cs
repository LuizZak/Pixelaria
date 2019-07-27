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
using PixCore.Text;
using PixDirectX.Rendering;

namespace PixUI
{
    /// <summary>
    /// A backing class for label views which stores display properties and keeps track of event
    /// dispatching when these properties change.
    /// </summary>
    public sealed class LabelViewBacking : IDisposable
    {
        [NotNull]
        private Font _font = new Font(FontFamily.GenericSansSerif.Name, 10);
        
        private InsetBounds _textInsetBounds;
        private Color _backgroundColor;

        /// <summary>
        /// Event triggered whenever the bound-related properties for this label view
        /// backing are changed and require bounds recalculation/rendering.
        /// </summary>
        public event EventHandler BoundsInvalidated;
        
        [NotNull]
        public AttributedText AttributedText { get; } = new AttributedText();

        /// <summary>
        /// Gets or sets the background color that is drawn around the label.
        /// </summary>
        public Color BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor.ToArgb() == value.ToArgb())
                    return;

                _backgroundColor = value;
                BoundsInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the string this label represents
        /// </summary>
        [NotNull]
        public string Text
        {
            get => AttributedText.String;
            set
            {
                if (!AttributedText.HasAttributes && AttributedText.String == value)
                    return;

                AttributedText.SetText(value);
            }
        }

        /// <summary>
        /// Gets or sets the text font for the text of this label view backing
        /// </summary>
        [NotNull]
        public Font TextFont
        {
            get => _font;
            set
            {
                _font = value;

                BoundsInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Gets or sets the text color of this label view backing
        /// </summary>
        public Color TextColor { get; set; } = Color.White;

        /// <summary>
        /// <see cref="InsetBounds"/> that expands the four corners of the text bounds from
        /// within this view.
        /// 
        /// The final text label has the size of its text string expanded by this
        /// value.
        /// </summary>
        public InsetBounds TextInsetBounds
        {
            get => _textInsetBounds;
            set
            {
                if (TextInsetBounds.Equals(value))
                    return;
                
                _textInsetBounds = value;
                BoundsInvalidated?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the size provider for this label view backing.
        /// </summary>
        [CanBeNull]
        public ITextSizeProvider SizeProvider { get; set; }

        public LabelViewBacking()
        {
            AttributedText.Modified += (sender, args) =>
            {
                BoundsInvalidated?.Invoke(this, args);
            };
        }
        
        /// <summary>
        /// Calculates the size for this label view backing using a given size provider
        /// </summary>
        public Vector CalculateSize([NotNull] ITextSizeProvider sizeProvider)
        {
            var textBounds = new RectangleF(PointF.Empty, sizeProvider.CalculateTextSize(AttributedText, TextFont));
            Vector size = textBounds.Size;
            size += new Vector(TextInsetBounds.Left + TextInsetBounds.Right, TextInsetBounds.Top + TextInsetBounds.Bottom);

            return size;
        }

        public void Dispose()
        {
            _font.Dispose();
        }
    }

    /// <summary>
    /// A basic view that represents a text label
    /// </summary>
    public sealed class LabelView : BaseView, IDisposable
    {
        private readonly LabelViewBacking _labelViewBacking = new LabelViewBacking();

        [NotNull]
        public static ITextSizeProvider defaultTextSizeProvider = new GdiTextSizeSizeProvider();

        /// <summary>
        /// Gets or sets the background color that is drawn around the label.
        /// </summary>
        public Color BackgroundColor
        {
            get => _labelViewBacking.BackgroundColor;
            set => _labelViewBacking.BackgroundColor = value;
        }

        /// <summary>
        /// Gets or sets the string this label represents
        /// </summary>
        [NotNull]
        public string Text
        {
            get => _labelViewBacking.Text;
            set => _labelViewBacking.Text = value;
        }

        /// <summary>
        /// Gets the attributed text for this label view
        /// </summary>
        [NotNull]
        public IAttributedText AttributedText => _labelViewBacking.AttributedText;

        /// <summary>
        /// Gets or sets the text color of this label view
        /// </summary>
        public Color TextColor
        {
            get => _labelViewBacking.TextColor;
            set => _labelViewBacking.TextColor = value;
        }

        /// <summary>
        /// Gets or sets the size provider for this label view.
        /// 
        /// If null, label view defaults to using <see cref="defaultTextSizeProvider"/>
        /// </summary>
        [CanBeNull]
        public ITextSizeProvider SizeProvider
        {
            get => _labelViewBacking.SizeProvider;
            set => _labelViewBacking.SizeProvider = value;
        }

        /// <summary>
        /// Gets or sets the text font for the text of this label view
        /// </summary>
        [NotNull]
        public Font TextFont
        {
            get => _labelViewBacking.TextFont;
            set => _labelViewBacking.TextFont = value;
        }

        /// <summary>
        /// An AABB that expands the four corners of the text bounds from
        /// within this view.
        /// 
        /// The final text label has the size of its text string expanded by this
        /// value.
        /// </summary>
        public InsetBounds TextInsetBounds
        {
            get => _labelViewBacking.TextInsetBounds;
            set => _labelViewBacking.TextInsetBounds = value;
        }

        /// <summary>
        /// Gets or sets if this label should be visible on screen.
        /// </summary>
        public bool Visible { get; set; } = true;
        
        /// <summary>
        /// Returns the exact AABB from within this label view where the text will
        /// appear in when inset by <see cref="TextInsetBounds"/>.
        /// </summary>
        public AABB TextBounds
        {
            get
            {
                var final = Bounds;
                final = TextInsetBounds.Inset(final);
                return final;
            }
        }

        public LabelView()
        {
            _labelViewBacking.AttributedText.Modified += AttributedTextModified;
        }
        
        public void Dispose()
        {
            _labelViewBacking.Dispose();
        }

        private void AttributedTextModified(object sender, EventArgs eventArgs)
        {
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            Size = _labelViewBacking.CalculateSize(SizeProvider ?? defaultTextSizeProvider);
        }
    }
}

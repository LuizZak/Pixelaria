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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using Color = System.Drawing.Color;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A Text Field that accepts user inputs via keyboard to alter a text content within.
    /// </summary>
    internal class TextField : ControlView, IKeyboardEventHandler
    {
        private readonly CursorBlinker _blinker = new CursorBlinker();

        private readonly ControlView _labelContainer = new ControlView();
        private readonly LabelViewControl _label = new LabelViewControl();

        private InsetBounds _contentInset = new InsetBounds(8, 8, 8, 8);

        private TextRange CaretRange = new TextRange(0, 0);

        /// <summary>
        /// Whether to allow line breaks when pressing the enter key.
        /// </summary>
        public bool AllowLineBreaks { get; set; } = false;

        /// <summary>
        /// Inset for text of textfield
        /// </summary>
        public InsetBounds ContentInset
        {
            get => _contentInset;
            set
            {
                _contentInset = value;
                Layout();
            }
        }

        /// <summary>
        /// Gets or sets the text of this textfield.
        /// 
        /// As keyboard input is received, this value is updated accordingly.
        /// </summary>
        [NotNull]
        public string Text
        {
            get => _label.Text;
            set => _label.Text = value;
        }

        public override bool CanBecomeFirstResponder => true;

        public TextField()
        {
            _labelContainer.InteractionEnabled = false;
            _labelContainer.AddChild(_label);

            _labelContainer.BackColor = Color.Transparent;
            _labelContainer.StrokeColor = Color.Transparent;

            _label.TextFont = new Font(FontFamily.GenericSansSerif, 11);
            
            _label.BackColor = Color.Transparent;
            _label.ForeColor = Color.Black;
            _label.StrokeColor = Color.Transparent;
            _label.VerticalTextAlignment = VerticalTextAlignment.Center;

            _blinker.BlinkInterval = TimeSpan.FromSeconds(1);

            AddChild(_labelContainer);
        }

        protected override void OnResize()
        {
            base.OnResize();

            Layout();
        }

        public override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            BecomeFirstResponder();
        }

        public void OnKeyPress(KeyPressEventArgs e)
        {
            if (!AllowLineBreaks)
            {
                if (e.KeyChar == '\n' || e.KeyChar == '\r')
                    return;
            }

            if (e.KeyChar == '\b')
            {
                BackspaceText();
                return;
            }

            InsertText(e.KeyChar.ToString());
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Home)
            {
                MoveToStart();
            }
            else if (e.KeyCode == Keys.End)
            {
                MoveToEnd();
            }
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            
        }

        public void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            // Caret movement
            if (e.KeyCode == Keys.Left)
                MoveLeft();
            else if (e.KeyCode == Keys.Right)
                MoveRight();
        }

        public override bool BecomeFirstResponder()
        {
            bool isFirstResponder = base.BecomeFirstResponder();

            if(isFirstResponder)
                _blinker.Restart();

            return isFirstResponder;
        }

        public override void RenderForeground(ControlRenderingContext context)
        {
            base.RenderForeground(context);

            if (IsFirstResponder)
                RenderCaret(context);
        }

        private void RenderCaret([NotNull] ControlRenderingContext context)
        {
            float transparency = _blinker.BlinkState;

            if (Math.Abs(transparency) < float.Epsilon)
                return;

            var caretLocation = AABB.FromRectangle(0, 0, 1, 10);

            string text = Text;

            var attributes = new TextAttributes(_label.TextFont.Name, _label.TextFont.Size)
            {
                HorizontalTextAlignment = _label.HorizontalTextAlignment,
                VerticalTextAlignment = _label.VerticalTextAlignment,
                AvailableWidth = _label.Width,
                AvailableHeight = _label.Height,
                WordWrap = _label.TextWordWrap
            };

            var provider = context.Renderer.LabelViewTextMetricsProvider;
            var bounds = provider.LocationOfCharacter(CaretRange.Start, new AttributedText(text), attributes);

            caretLocation = _label.ConvertTo(caretLocation, this);
            
            caretLocation = caretLocation.OffsetBy(bounds.Minimum + new Vector(0, -2));
            caretLocation = caretLocation.WithSize(new Vector(caretLocation.Size.X, bounds.Height + 2));

            var color = Color4.Black;

            color.Alpha = transparency;

            using (var brush = new SolidColorBrush(context.RenderTarget, color))
            {
                context.RenderTarget.FillRectangle(caretLocation, brush);
            }
        }

        public override bool CanHandle(IEventRequest eventRequest)
        {
            if (eventRequest is IKeyboardEventRequest)
                return true;

            return base.CanHandle(eventRequest);
        }

        private void MoveLeft()
        {
            if (CaretRange.Start == 0)
                return;

            CaretRange = new TextRange(CaretRange.Start - 1, 0);
            _blinker.Restart();
        }

        private void MoveRight()
        {
            if (CaretRange.Start == Text.Length)
                return;

            CaretRange = new TextRange(CaretRange.Start + 1, 0);

            _blinker.Restart();
        }

        private void MoveToStart()
        {
            if (CaretRange.Start == 0)
                return;

            CaretRange = new TextRange(0, 0);
            _blinker.Restart();
        }

        private void MoveToEnd()
        {
            if (CaretRange.Start == Text.Length)
                return;

            CaretRange = new TextRange(Text.Length, 0);
            _blinker.Restart();
        }

        /// <summary>
        /// Inserts the specified text on top of the current caret position.
        /// 
        /// Replaces text, if caret's length is > 0 and text is available on selection;
        /// </summary>
        private void InsertText(string text)
        {
            if (CaretRange.Start == Text.Length)
            {
                Text += text;
            }
            else
            {
                Text = Text.Insert(CaretRange.Start, text);
            }
            
            CaretRange = new TextRange(CaretRange.Start + 1, 0);
            _blinker.Restart();
        }

        /// <summary>
        /// Deletes the text before the starting position of the caret.
        /// </summary>
        private void BackspaceText()
        {
            if (CaretRange.Start == 0)
                return;
            
            Text = Text.Remove(CaretRange.Start - 1, CaretRange.Length == 0 ? 1 : CaretRange.Length);
            CaretRange = new TextRange(CaretRange.Start - 1, 0);
            _blinker.Restart();
        }

        private void Layout()
        {
            var bounds = Bounds.Inset(ContentInset);
            _labelContainer.SetFrame(bounds);
        }

        /// <summary>
        /// A small class to handle cursor blinker timer
        /// </summary>
        private class CursorBlinker
        {
            private readonly Stopwatch _stopwatch = new Stopwatch();

            /// <summary>
            /// Blink interval; time going from fully opaque to transparent, right
            /// up to before the cursor goes fully opaque again.
            /// </summary>
            public TimeSpan BlinkInterval { get; set; } = TimeSpan.FromSeconds(1);

            /// <summary>
            /// Cursor blink state, from 0 to 1.
            /// 
            /// 0 is fully transparent, and 1 is fully opaque.
            /// </summary>
            public float BlinkState => GetBlinkState();

            public CursorBlinker()
            {
                _stopwatch.Start();
            }

            public void Restart()
            {
                _stopwatch.Restart();
            }

            private float GetBlinkState()
            {
                double state = _stopwatch.Elapsed.TotalMilliseconds % BlinkInterval.TotalMilliseconds;

                if (state < BlinkInterval.TotalMilliseconds / 2)
                    return 1.0f;

                return 0;
            }
        }
    }
}

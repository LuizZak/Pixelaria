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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Forms;
using JetBrains.Annotations;
using Pixelaria.Utils;

using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;

namespace Pixelaria.Views.ExportPipeline.PipelineView.Controls
{
    /// <summary>
    /// A Text Field that accepts user inputs via keyboard to alter a text content within.
    /// </summary>
    internal partial class TextField : ControlView, IKeyboardEventHandler
    {
        private readonly Subject<string> _textUpdated = new Subject<string>();

        private readonly TextEngine _textEngine;

        private readonly CursorBlinker _blinker = new CursorBlinker();

        private readonly ControlView _labelContainer = new ControlView();
        private readonly LabelViewControl _label = new LabelViewControl();

        private InsetBounds _contentInset = new InsetBounds(8, 8, 8, 8);

        // TODO: Collapse these into an external handler or into ReactiveX to lower the state clutter here

        private DateTime _lastMouseDown = DateTime.MinValue;
        private Vector _lastMouseDownVec = Vector.Zero;
        private bool _selectingWordSpan;
        private int _wordSpanStartPosition;
        private bool _mouseDown;

        /// <summary>
        /// Gets or sets the color to use when drawing the background of selected
        /// regions of this text field.
        /// </summary>
        public Color SelectionBackColor { get; set; } = Color.LightBlue;

        /// <summary>
        /// Gets or sets the color to use when drawing the caret.
        /// </summary>
        public Color CaretColor { get; set; } = Color.Black;

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
            var buffer = new LabelViewTextBuffer(_label);

            buffer.Changed += TextBufferOnChanged;

            _textEngine = new TextEngine(buffer);
            _textEngine.CaretChanged += TextEngineOnCaretChanged;

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

        private void TextBufferOnChanged(object sender, EventArgs eventArgs)
        {
            _blinker.Restart();
            _textUpdated.OnNext(Text);
        }

        protected override void OnResize()
        {
            base.OnResize();

            Layout();
        }

        #region Mouse Handlers
        
        public override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!BecomeFirstResponder())
                return;

            _mouseDown = true;

            Cursor.Current = Cursors.IBeam;
            _blinker.Restart();

            int offset = OffsetUnder(e.Location);
            _textEngine.SetCaret(offset);

            // Double click selects word
            if (_lastMouseDownVec.Distance(e.Location) < 10 && DateTime.Now.Subtract(_lastMouseDown).TotalMilliseconds < SystemInformation.DoubleClickTime)
            {
                _wordSpanStartPosition = OffsetUnder(e.Location);

                var segment = _textEngine.WordSegmentIn(_wordSpanStartPosition);
                _textEngine.SetCaret(segment);

                _selectingWordSpan = true;
            }

            _lastMouseDown = DateTime.Now;
            _lastMouseDownVec = e.Location;
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();

            Cursor.Current = Cursors.IBeam;
        }

        public override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            Cursor.Current = Cursors.IBeam;

            if (!_mouseDown)
                return;

            _blinker.Restart();

            if (_selectingWordSpan)
            {
                int offset = OffsetUnder(e.Location);

                var original = _textEngine.WordSegmentIn(_wordSpanStartPosition);
                var newSeg = _textEngine.WordSegmentIn(offset);

                _textEngine.SetCaret(original.Union(newSeg), offset <= _wordSpanStartPosition ? CaretPosition.Start : CaretPosition.End);
            }
            else
            {
                int offset = OffsetUnder(e.Location);
                _textEngine.MoveCaretSelecting(offset);
            }
        }

        public override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            _mouseDown = false;
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();

            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region Keyboard Handlers

        public void OnKeyPress(KeyPressEventArgs e)
        {
            if (!AllowLineBreaks)
            {
                if (e.KeyChar == '\n' || e.KeyChar == '\r')
                    return;
            }
            
            if (char.IsControl(e.KeyChar))
                return;

            _textEngine.InsertText(e.KeyChar.ToString());
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            // Selection
            if (e.Modifiers.HasFlag(Keys.Shift))
            {
                if (e.KeyCode == Keys.Home)
                {
                    _textEngine.SelectToStart();
                }
                else if (e.KeyCode == Keys.End)
                {
                    _textEngine.SelectToEnd();
                }
            }
            else
            {
                // Navigation
                if (e.KeyCode == Keys.Home)
                {
                    _textEngine.MoveToStart();
                }
                else if (e.KeyCode == Keys.End)
                {
                    _textEngine.MoveToEnd();
                }
            }

            // Delete/backspace
            if (e.KeyCode == Keys.Back)
            {
                // When control is held down, erase previous word
                if (e.Modifiers == Keys.Control)
                {
                    _textEngine.SelectLeftWord();
                }

                _textEngine.BackspaceText();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                // When control is held down, erase next word
                if (e.Modifiers == Keys.Control)
                {
                    _textEngine.SelectRightWord();
                }

                _textEngine.DeleteText();
            }
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            
        }

        public void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            // Caret selection/movement
            if (e.Modifiers.HasFlag(Keys.Shift))
            {
                if (e.Modifiers.HasFlag(Keys.Control))
                {
                    if (e.KeyCode == Keys.Left)
                        _textEngine.SelectLeftWord();
                    else if (e.KeyCode == Keys.Right)
                        _textEngine.SelectRightWord();
                }
                else
                {
                    if (e.KeyCode == Keys.Left)
                        _textEngine.SelectLeft();
                    else if (e.KeyCode == Keys.Right)
                        _textEngine.SelectRight();
                }
            }
            else
            {
                if (e.Modifiers.HasFlag(Keys.Control))
                {
                    if (e.KeyCode == Keys.Left)
                        _textEngine.MoveLeftWord();
                    else if (e.KeyCode == Keys.Right)
                        _textEngine.MoveRightWord();
                }
                else
                {
                    if (e.KeyCode == Keys.Left)
                        _textEngine.MoveLeft();
                    else if (e.KeyCode == Keys.Right)
                        _textEngine.MoveRight();
                }
            }
        }

        #endregion
        
        public override bool BecomeFirstResponder()
        {
            if (IsFirstResponder)
                return true;

            bool isFirstResponder = base.BecomeFirstResponder();

            if(isFirstResponder)
                _blinker.Restart();

            return isFirstResponder;
        }
        
        public override void RenderBackground(ControlRenderingContext context)
        {
            base.RenderBackground(context);
            
            if (_textEngine.Caret.Length == 0)
                return;
            
            _label.WithTextLayout(layout =>
            {
                var metrics = layout.HitTestTextRange(_textEngine.Caret.Start, _textEngine.Caret.Length, 0, 0);

                using (var brush = new SolidColorBrush(context.RenderTarget, SelectionBackColor.ToColor4()))
                {
                    foreach (var metric in metrics)
                    {
                        var aabb = AABB.FromRectangle(metric.Left, metric.Top, metric.Width, metric.Height);
                        aabb = _label.ConvertTo(aabb, this);

                        context.RenderTarget.FillRectangle(aabb, brush);
                    }
                }
            });
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
            var bounds = provider.LocationOfCharacter(_textEngine.Caret.Location, new AttributedText(text), attributes);

            caretLocation = _label.ConvertTo(caretLocation, this);
            
            caretLocation = caretLocation.OffsetBy(bounds.Minimum);
            caretLocation = caretLocation.WithSize(new Vector(caretLocation.Size.X, bounds.Height));

            // Round caret location to avoid aliasing
            caretLocation = caretLocation.OffsetTo(Vector.Round(caretLocation.Minimum));

            var color = CaretColor.ToColor4();
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
        
        private void TextEngineOnCaretChanged(object sender, TextEngineCaretChangedEventArgs textEngineCaretChangedEventArgs)
        {
            _blinker.Restart();
        }
        
        private void Layout()
        {
            var bounds = Bounds.Inset(ContentInset);
            _labelContainer.SetFrame(bounds);
        }

        /// <summary>
        /// Returns string offset at a given point on this text field.
        /// </summary>
        private int OffsetUnder(Vector point)
        {
            if (DirectWriteFactory == null)
                return 0;

            int offset = 0;
            var converted = _label.ConvertFrom(point, this);

            _label.WithTextLayout(layout =>
            {
                var metrics = layout.HitTestPoint(converted.X, converted.Y, out RawBool isTrailing, out RawBool _);
                offset = metrics.TextPosition + (isTrailing ? 1 : 0);
            });

            return Math.Min(offset, _label.Text.Length);
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
            public TimeSpan BlinkInterval { private get; set; } = TimeSpan.FromSeconds(1);

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
                double state = _stopwatch.Elapsed.TotalSeconds % BlinkInterval.TotalSeconds;
                
                if (state < BlinkInterval.TotalSeconds / 2)
                    return 1.0f;

                return 0;
            }
        }

        private class LabelViewTextBuffer : ITextEngineTextualBuffer
        {
            private readonly LabelViewControl _label;
            public int TextLength => _label.Text.Length;
            
            public event EventHandler Changed;

            public LabelViewTextBuffer(LabelViewControl label)
            {
                _label = label;
            }

            public string TextInRange(TextRange range)
            {
                return _label.Text.Substring(range.Start, range.End);
            }

            public char CharacterAtOffset(int offset)
            {
                return _label.Text[offset];
            }

            public void Delete(int index, int length)
            {
                _label.Text = _label.Text.Remove(index, length);
                Changed?.Invoke(this, EventArgs.Empty);
            }

            public void Insert(int index, string text)
            {
                _label.Text = _label.Text.Insert(index, text);
                Changed?.Invoke(this, EventArgs.Empty);
            }

            public void Append(string text)
            {
                _label.Text += text;
                Changed?.Invoke(this, EventArgs.Empty);
            }

            public void Replace(int index, int length, string text)
            {
                _label.Text = _label.Text.Remove(index, length).Insert(index, text);
                Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    // Reactive bindings for TextField

    internal partial class TextField
    {
        /// <summary>
        /// On subscription, returns the current text value and receives updates
        /// of next subsequent text values as the user updates it.
        /// </summary>
        public IObservable<string> RxTextUpdated => _textUpdated.StartWith(Text);
    }
}

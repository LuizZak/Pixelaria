﻿/*
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
using PixCore.Geometry;
using PixCore.Text;
using PixDirectX.Rendering;
using PixDirectX.Utils;
using PixUI.Text;
using SharpDX.Direct2D1;
using Color = System.Drawing.Color;

namespace PixUI.Controls
{
    /// <summary>
    /// A Text Field that accepts user inputs via keyboard to alter a text content within.
    /// </summary>
    public partial class TextField : ControlView, IKeyboardEventHandler
    {
        private readonly TextEngine _textEngine;
        private readonly Subject<string> _textUpdated = new Subject<string>();
        private readonly CursorBlinker _blinker = new CursorBlinker();
        private readonly ControlView _labelContainer = new ControlView();
        private readonly LabelViewControl _label = LabelViewControl.Create();
        private readonly StatedValueStore<TextFieldVisualStyleParameters> _statesStyles = new StatedValueStore<TextFieldVisualStyleParameters>();

        private InsetBounds _contentInset = new InsetBounds(8, 8, 8, 8);

        // TODO: Collapse these into an external handler or into ReactiveX to lower the state clutter here

        private DateTime _lastMouseDown = DateTime.MinValue;
        private Vector _lastMouseDownVec = Vector.Zero;
        private bool _selectingWordSpan;
        private int _wordSpanStartPosition;
        private bool _mouseDown;
        private bool _editable;

        public override Color ForeColor
        {
            get => base.ForeColor;
            set
            {
                base.ForeColor = value;
                _label.ForeColor = value;
            }
        }

        /// <summary>
        /// If true, when the enter key is pressed a <see cref="EnterKey"/> event is raised for this text field.
        /// </summary>
        public bool AcceptsEnterKey { get; set; }
        
        /// <summary>
        /// Gets the current active style for this textfield.
        /// </summary>
        public TextFieldVisualStyleParameters Style { get; private set; }

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
                Invalidate();
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
            set
            {
                _label.Text = value;
                _textEngine.UpdateCaretFromTextBuffer();
            }
        }

        /// <summary>
        /// Gets or sets the caret position for this textfield.
        /// </summary>
        public Caret Caret
        {
            get => _textEngine.Caret;
            set => _textEngine.SetCaret(value);
        }

        /// <summary>
        /// Gets or sets a value specifying whether the string contents of this <see cref="TextField"/> are editable.
        /// </summary>
        public bool Editable
        {
            get => _editable;
            set
            {
                _editable = value;
                if (IsFirstResponder)
                {
                    ResignFirstResponder();
                }
            }
        }

        /// <summary>
        /// Event fired whenever the text contents of this text field are updated.
        /// </summary>
        public event TextFieldTextChangedEventHandler TextChanged;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler EnterKey;

        public override bool CanBecomeFirstResponder => true;

        /// <summary>
        /// Creates a new instance of <see cref="TextField"/>
        /// </summary>
        public static TextField Create(bool darkStyle = true)
        {
            var textField = new TextField();

            textField.Initialize();

            if (darkStyle)
            {
                textField.SetStyleForState(TextFieldVisualStyleParameters.DefaultDarkStyle(), ControlViewState.Normal);
            }
            else
            {
                textField.SetStyleForState(TextFieldVisualStyleParameters.DefaultLightStyle(), ControlViewState.Normal);
            }

            return textField;
        }

        protected TextField()
        {
            var buffer = new LabelViewTextBuffer(_label);

            buffer.Changed += TextBufferOnChanged;

            _textEngine = new TextEngine(buffer);
        }

        protected virtual void Initialize()
        {
            Editable = true;
            
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

            TextChanged?.Invoke(this, new TextFieldTextChangedEventArgs(Text));
        }

        protected override void OnChangedState(ControlViewState newState)
        {
            base.OnChangedState(newState);

            var style = GetStyleForState(newState);
            ApplyStyle(style);
        }

        public override void OnFixedFrame(FixedFrameEventArgs e)
        {
            base.OnFixedFrame(e);

            // For caret blinking rendering
            if(IsFirstResponder)
                Invalidate();
        }

        #region Visual Style Settings

        /// <summary>
        /// Sets the visual style of this text field when it's under a given view state.
        /// </summary>
        public void SetStyleForState(TextFieldVisualStyleParameters visualStyle, ControlViewState state)
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
        public TextFieldVisualStyleParameters GetStyleForState(ControlViewState state)
        {
            return _statesStyles.GetValue(state);
        }

        private void ApplyStyle(TextFieldVisualStyleParameters style)
        {
            Style = style;
            Invalidate();

            ForeColor = style.TextColor;
            StrokeWidth = style.StrokeWidth;
            StrokeColor = style.StrokeColor;
            BackColor = style.BackgroundColor;
        }

        #endregion

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
            
            Invalidate();
            
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
            
            Invalidate();

            Cursor.Current = Cursors.Default;
        }

        #endregion

        #region Keyboard Handlers

        public void OnKeyPress(KeyPressEventArgs e)
        {
            if (!Editable)
                return;

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
            // Enter key
            if (e.KeyCode == Keys.Enter && AcceptsEnterKey)
            {
                EnterKey?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
                return;
            }

            // Copy/cut/paste + undo/redo
            if (e.Modifiers == Keys.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.C:
                        Copy();
                        e.Handled = true;
                        break;
                    case Keys.X:
                        if (Editable)
                        {
                            Cut();
                            e.Handled = true;
                        }
                        break;
                    case Keys.V:
                        if (Editable)
                        {
                            Paste();
                            e.Handled = true;
                        }
                        break;
                    case Keys.Z:
                        if (Editable)
                        {
                            Undo();
                            e.Handled = true;
                        }
                        break;
                    case Keys.Y:
                        if (Editable)
                            Redo();
                        e.Handled = true;
                        break;
                    case Keys.A:
                        SelectAll();
                        e.Handled = true;
                        break;
                }
            }
            
            // Ctrl+Shift+Z as alternative for Ctrl+Y (redo)
            if (Editable && e.Modifiers == (Keys.Control | Keys.Shift) && e.KeyCode == Keys.Z)
            {
                Redo();
                e.Handled = true;
                return;
            }
            
            // Selection
            if (e.Modifiers.HasFlag(Keys.Shift))
            {
                if (e.KeyCode == Keys.Home)
                {
                    _textEngine.SelectToStart();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.End)
                {
                    _textEngine.SelectToEnd();
                    e.Handled = true;
                }
            }
            else
            {
                // Navigation
                if (e.KeyCode == Keys.Home)
                {
                    _textEngine.MoveToStart();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.End)
                {
                    _textEngine.MoveToEnd();
                    e.Handled = true;
                }
            }

            // Delete/backspace
            if (Editable)
            {
                if (e.KeyCode == Keys.Back)
                {
                    // When control is held down, erase previous word
                    if (e.Modifiers == Keys.Control)
                    {
                        _textEngine.SelectLeftWord();
                    }

                    _textEngine.BackspaceText();

                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Delete)
                {
                    // When control is held down, erase next word
                    if (e.Modifiers == Keys.Control)
                    {
                        _textEngine.SelectRightWord();
                    }

                    _textEngine.DeleteText();

                    e.Handled = true;
                }
            }

            if (HandleCaretMoveEvent(e.KeyCode, e.Modifiers))
            {
                e.Handled = true;
            }
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            
        }

        public void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            HandleCaretMoveEvent(e.KeyCode, e.Modifiers);
        }

        private bool HandleCaretMoveEvent(Keys keyCode, Keys modifiers)
        {
            // Caret selection/movement
            if (modifiers.HasFlag(Keys.Shift))
            {
                if (modifiers.HasFlag(Keys.Control))
                {
                    if (keyCode == Keys.Left)
                    {
                        _textEngine.SelectLeftWord();
                        return true;
                    }
                    if (keyCode == Keys.Right)
                    {
                        _textEngine.SelectRightWord();
                        return true;
                    }
                }
                else
                {
                    if (keyCode == Keys.Left)
                    {
                        _textEngine.SelectLeft();
                        return true;
                    }
                    if (keyCode == Keys.Right)
                    {
                        _textEngine.SelectRight();
                        return true;
                    }
                }
            }
            else
            {
                if (modifiers.HasFlag(Keys.Control))
                {
                    if (keyCode == Keys.Left)
                    {
                        _textEngine.MoveLeftWord();
                        return true;
                    }
                    if (keyCode == Keys.Right)
                    {
                        _textEngine.MoveRightWord();
                        return true;
                    }
                }
                else
                {
                    if (keyCode == Keys.Left)
                    {
                        _textEngine.MoveLeft();
                        return true;
                    }
                    if (keyCode == Keys.Right)
                    {
                        _textEngine.MoveRight();
                        return true;
                    }
                }
            }

            return false;
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

        public override void ResignFirstResponder()
        {
            base.ResignFirstResponder();

            Invalidate();
        }

        public override void RenderBackground(ControlRenderingContext context)
        {
            base.RenderBackground(context);
            
            if (_textEngine.Caret.Length == 0)
                return;
            
            _label.WithTextLayout(layout =>
            {
                context.State.WithTemporaryClipping(_labelContainer.FrameOnParent, () =>
                {
                    var metrics = layout.HitTestTextRange(_textEngine.Caret.Start, _textEngine.Caret.Length, 0, 0);

                    using (var brush = new SolidColorBrush(context.RenderTarget, Style.SelectionColor.ToColor4()))
                    {
                        foreach (var metric in metrics)
                        {
                            var aabb = AABB.FromRectangle(metric.Left, metric.Top, metric.Width, metric.Height);
                            aabb = _label.ConvertTo(aabb, this);

                            context.RenderTarget.FillRectangle(aabb.ToRawRectangleF(), brush);
                        }
                    }
                });
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

            var caretLocation = GetCaretBounds(context);

            var color = Style.CaretColor.ToColor4();
            color.Alpha = transparency;

            using (var brush = new SolidColorBrush(context.RenderTarget, color))
            {
                context.RenderTarget.FillRectangle(caretLocation.ToRawRectangleF(), brush);
            }
        }

        private AABB GetCaretBounds([NotNull] ControlRenderingContext context)
        {
            var caretLocation = AABB.FromRectangle(0, 0, 1, 10);

            string text = Text;

            var attributes = new TextLayoutAttributes(_label.TextFont.Name, _label.TextFont.Size)
            {
                HorizontalTextAlignment = _label.HorizontalTextAlignment,
                VerticalTextAlignment = _label.VerticalTextAlignment,
                AvailableWidth = _label.Width,
                AvailableHeight = _label.Height,
                WordWrap = _label.TextWordWrap
            };

            var provider = context.TextMetricsProvider;
            var bounds = provider.LocationOfCharacter(_textEngine.Caret.Location, new AttributedText(text), attributes);

            caretLocation = _label.ConvertTo(caretLocation, this);

            caretLocation = caretLocation.OffsetBy(bounds.Minimum);
            caretLocation = caretLocation.WithSize(new Vector(caretLocation.Size.X, bounds.Height));

            // Round caret location to avoid aliasing
            return caretLocation.OffsetTo(Vector.Round(caretLocation.Minimum));
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

            ScrollLabel();
        }

        /// <summary>
        /// Scrolls the label positioning so the current caret location is always visible
        /// </summary>
        private void ScrollLabel()
        {
            var loc = LocationForOffset(_textEngine.Caret.Location);
            
            var locInContainer = _labelContainer.ConvertFrom(loc, this);
            if (_labelContainer.Contains(locInContainer))
                return;

            var labelOffset = _label.Location;

            if (locInContainer.X > _labelContainer.Width)
                labelOffset = labelOffset - new Vector(locInContainer.X - _labelContainer.Width, 0);
            else if (locInContainer.X < 0)
                labelOffset = labelOffset - new Vector(locInContainer.X, 0);

            _label.Location = labelOffset;
        }

        public override void Layout()
        {
            base.Layout();

            var bounds = Bounds.Inset(ContentInset);
            _labelContainer.SetFrame(bounds);
            _label.Center = new Vector(_label.Center.X, _labelContainer.Height / 2);
        }

        /// <summary>
        /// Selects the entire text available on this text field
        /// </summary>
        public void SelectAll()
        {
            _textEngine.SelectAll();
        }

        #region Copy/cut/paste + undo/redo

        private void Copy()
        {
            _textEngine.Copy();
        }

        private void Cut()
        {
            _textEngine.Cut();
        }

        private void Paste()
        {
            _textEngine.Paste();
        }

        private void Undo()
        {
            _textEngine.UndoSystem.Undo();
        }

        private void Redo()
        {
            _textEngine.UndoSystem.Redo();
        }

        #endregion

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
                var metrics = layout.HitTestPoint(converted.X, converted.Y, out var isTrailing, out var _);
                offset = metrics.TextPosition + (isTrailing ? 1 : 0);
            });

            return Math.Min(offset, _label.Text.Length);
        }

        /// <summary>
        /// Returns the point for a given string offset, locally on this text field's coordinates.
        /// </summary>
        private Vector LocationForOffset(int offset)
        {
            if (DirectWriteFactory == null)
                return Vector.Zero;
            
            var position = Vector.Zero;

            _label.WithTextLayout(layout =>
            {
                layout.HitTestTextPosition(offset, false, out var x, out var y);
                position = new Vector(x, y);
            });

            position = _label.ConvertTo(position, this);

            return position;
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
                return _label.Text.Substring(range.Start, range.Length);
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

    /// <summary>
    /// Specfiies the presentation style for a text field.
    /// 
    /// Used to specify separate visual styles depending on the first-responding state of the textfield.
    /// </summary>
    public struct TextFieldVisualStyleParameters
    {
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
        public Color StrokeColor { get; set; }
        public float StrokeWidth { get; set; }
        public Color CaretColor { get; set; }
        public Color SelectionColor { get; set; }

        public TextFieldVisualStyleParameters(Color textColor, Color backgroundColor, Color strokeColor, float strokeWidth, Color caretColor, Color selectionColor)
        {
            TextColor = textColor;
            StrokeColor = strokeColor;
            StrokeWidth = strokeWidth;
            BackgroundColor = backgroundColor;
            CaretColor = caretColor;
            SelectionColor = selectionColor;
        }

        public static TextFieldVisualStyleParameters DefaultDarkStyle()
        {
            return new TextFieldVisualStyleParameters(Color.White, Color.Black, Color.FromArgb(50, 50, 50), 1, Color.White, Color.SteelBlue);
        }

        public static TextFieldVisualStyleParameters DefaultLightStyle()
        {
            return new TextFieldVisualStyleParameters(Color.Black, Color.White, Color.Black, 1, Color.Black, Color.LightBlue);
        }
    }

    /// <summary>
    /// Event arguments for a <see cref="TextFieldTextChangedEventHandler"/> event.
    /// </summary>
    public class TextFieldTextChangedEventArgs : EventArgs
    {
        public string Text { get; }

        public TextFieldTextChangedEventArgs(string text)
        {
            Text = text;
        }
    }

    /// <summary>
    /// Delegate for a <see cref="TextField.TextChanged"/> event.
    /// </summary>
    public delegate void TextFieldTextChangedEventHandler(object sender, TextFieldTextChangedEventArgs e);

    // Reactive bindings for TextField
    public partial class TextField
    {
        /// <summary>
        /// On subscription, returns the current text value and receives updates
        /// of next subsequent text values as the user updates it.
        /// </summary>
        public IObservable<string> RxTextUpdated => _textUpdated.StartWith(Text);
    }
}

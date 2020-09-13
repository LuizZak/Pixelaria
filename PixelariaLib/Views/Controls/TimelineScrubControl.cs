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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace PixelariaLib.Views.Controls
{
    /// <summary>
    /// Describes a timeline control with two knobs,
    /// used to set a starting and ending point in a range
    /// </summary>
    [DefaultEvent("RangeChanged")]
    public class TimelineScrubControl : Control
    {
        /// <summary>
        /// The height of the timeline to draw:
        /// </summary>
        protected int timelineHeight = 10;

        /// <summary>
        /// The behavior type of this timeline
        /// </summary>
        protected TimelineBehaviorType behaviorType;

        /// <summary>
        /// The frame display type of this timeline
        /// </summary>
        protected TimelineFrameDisplayType frameDisplayType;

        /// <summary>
        /// Whether to display the frame under the mouse when it hovers over the control
        /// </summary>
        protected bool displayFrameUnderMouse;

        /// <summary>
        /// Whether to disable selection of frames that are out of range
        /// </summary>
        protected bool disableFrameSelectionOutOfRange;

        /// <summary>
        /// ToolTip instance associated with this timeline. Used to show the frames the knobs are pointing to
        /// </summary>
        protected ToolTip toolTip;

        /// <summary>
        /// The control's X scroll
        /// </summary>
        protected double scrollX;
        /// <summary>
        /// The control's Width scale modifier
        /// </summary>
        protected double scrollScaleWidth = 1;

        /// <summary>
        /// The knob currently being dragged
        /// </summary>
        protected Knob drag;

        /// <summary>
        /// Whether the user is dragging the view
        /// </summary>
        protected bool draggingView;
        /// <summary>
        /// Whether the user is dragging the current frame
        /// </summary>
        protected bool draggingFrame;
        /// <summary>
        /// Whether the user is dragging the timeline
        /// </summary>
        protected bool draggingTimeline;
        /// <summary>
        /// Whether the user is hovering the mouse over the currently selected range
        /// </summary>
        protected bool mouseOverCurrentRange;
        /// <summary>
        /// Whether the user is hovering the mouse over the timeline
        /// </summary>
        protected bool mouseOverTimeline;
        /// <summary>
        /// Whether the mouse is currently inside the control
        /// </summary>
        protected bool mouseInsideControl;
        /// <summary>
        /// Point representing the range before the user started dragging the timeline
        /// </summary>
        protected Point draggingTimelineRange;

        /// <summary>
        /// States the current frame in the seekbar. Set to -1 to highlight no frame.
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(-1)]
        [Description("The current playing frame on the timeline")]
        public int CurrentFrame
        {
            get => currentFrame;
            set
            {
                // Invalidate last playhead position:
                InvalidatePlayhead();

                currentFrame = value;

                // Invalidate new playhead position:
                InvalidatePlayhead();
            }
        }

        /// <summary>
        /// Gets or sets this TimelineControl's first knob
        /// </summary>
        [Browsable(false)]
        public Knob FirstKnob
        {
            get => firstKnob;
            protected set => firstKnob = value;
        }

        /// <summary>
        /// Gets or sets this TimelineControl's second knob
        /// </summary>
        [Browsable(false)]
        public Knob SecondKnob
        {
            get => secondKnob;
            protected set => secondKnob = value;
        }

        /// <summary>
        /// Gets or sets the currently selected value range of this TimelineControl
        /// </summary>
        [Browsable(false)]
        public Point Range
        {
            get => GetRange();
            set
            {
                if (value == GetRange())
                    return;

                if (value.X < minimum)
                    value.X = minimum;
                if (value.Y > maximum - value.X)
                    value.Y = maximum - value.X;

                firstKnob.Value = value.X;
                secondKnob.Value = value.Y + value.X;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets whether the user is currently dragging the frame
        /// </summary>
        [Browsable(false)]
        public bool DraggingFrame => draggingFrame;

        /// <summary>
        /// Gets or sets this TimelineControl's minimum value
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The minimum value on the available range")]
        public int Minimum
        {
            get => minimum;
            set
            {
                // Don't update if the values are the same
                if (minimum == value)
                    return;

                minimum = value;

                // Caches the text size:
                var g = CreateGraphics();

                // Set a format flag:
                const TextFormatFlags flags = TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;

                SizeF stringSize = TextRenderer.MeasureText(g, "" + (maximum), _font, new Size(), flags);
                _maxSize = stringSize.Width;

                // Draw the middle frame (only if there are more than 1 frames):
                if (maximum > 1)
                {
                    stringSize = TextRenderer.MeasureText(g, "" + (maximum + minimum) / 2, _font, new Size(), flags);
                    _medSize = stringSize.Width;
                }

                if (DesignMode || firstKnob.Value < minimum)
                {
                    firstKnob.Value = minimum;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets this TimelineControl's maximum value
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The maximum value on the available range")]
        public int Maximum
        {
            get => maximum;
            set
            {
                // Don't update if the values are the same
                if (maximum == value)
                    return;

                maximum = value;

                // Caches the text size:
                var g = CreateGraphics();

                // Set a format flag:
                const TextFormatFlags flags = TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;

                SizeF stringSize = TextRenderer.MeasureText(g, "" + (maximum), _font, new Size(), flags);
                _maxSize = stringSize.Width;

                // Draw the middle frame (only if there are more than 1 frames):
                if (maximum > 1)
                {
                    stringSize = TextRenderer.MeasureText(g, "" + (maximum + minimum) / 2, _font, new Size(), flags);
                    _medSize = stringSize.Width;
                }

                if (DesignMode || secondKnob.Value > maximum)
                {
                    secondKnob.Value = maximum;
                }

                Invalidate();
            }
        }

        /// <summary>
        /// Gets or sets the height of the timeline
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(10)]
        [Description("The height of the timeline in pixels")]
        public int TimelineHeight
        {
            get => timelineHeight;
            set { if (timelineHeight != value) { timelineHeight = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets the control's X scroll
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(0)]
        [Description("The control's X scroll")]
        public double ScrollX
        {
            get => scrollX;
            set { if (Math.Abs(scrollX - value) > float.Epsilon) { scrollX = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets the control's Width scale modifier 
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(1)]
        [Description("The control's Width scale modifier")]
        public double ScrollScaleWidth
        {
            get => scrollScaleWidth;
            set { if (Math.Abs(scrollScaleWidth - value) > float.Epsilon) { value = value < 0 ? 0.1f : value; scrollScaleWidth = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets the behavior type of this timeline
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(TimelineBehaviorType.RangeSelector)]
        [Description("The behavior type of this timeline")]
        public TimelineBehaviorType BehaviorType
        {
            get => behaviorType;
            set { if (behaviorType != value) { behaviorType = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets the frame display type of this timeline
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(TimelineFrameDisplayType.Tick)]
        [Description("The frame display type of this timeline")]
        public TimelineFrameDisplayType FrameDisplayType
        {
            get => frameDisplayType;
            set { if (frameDisplayType != value) { frameDisplayType = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets whether to display the frame under the mouse when it hovers over the control
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Whether to display the frame under the mouse when it hovers over the control")]
        public bool DisplayFrameUnderMouse
        {
            get => displayFrameUnderMouse;
            set
            {
                if (displayFrameUnderMouse != value)
                {
                    displayFrameUnderMouse = value;

                    InvalidateUnderMouse();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to disable selection of frames that are out of range
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Whether to disable selection of frames that are out of range")]
        public bool DisableFrameSelectionOutOfRange
        {
            get => disableFrameSelectionOutOfRange;
            set => disableFrameSelectionOutOfRange = value;
        }

        /// <summary>
        /// Event handler for the RangeChangedEvent
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void RangeChangedEventHandler(object sender, RangeChangedEventArgs e);
        /// <summary>
        /// Event fired every time the frame range has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs when the selection range was changed")]
        public event RangeChangedEventHandler RangeChanged;

        /// <summary>
        /// Event handler for the FrameChanged
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="e">The event arguments for the event</param>
        public delegate void FrameChangedEventHandler(object sender, FrameChangedEventArgs e);
        /// <summary>
        /// Event fired every time the frame range has changed
        /// </summary>
        [Browsable(true)]
        [Category("Action")]
        [Description("Occurs whenever the user changes the current frame")]
        public event FrameChangedEventHandler FrameChanged;

        /// <summary>
        /// Initializes a new instance of the TimelineControl
        /// </summary>
        public TimelineScrubControl()
        {
            // Initialize the internal fields
            behaviorType = TimelineBehaviorType.RangeSelector;

            // Create the knobs:
            firstKnob = new Knob(this);
            secondKnob = new Knob(this);

            // Set the bounds:
            Minimum = 0;
            Maximum = 1;
            currentFrame = -1;
            disableFrameSelectionOutOfRange = true;

            // Set the knobs' values:
            firstKnob.Value = minimum;
            secondKnob.Value = maximum;

            // Set the control's style so it won't flicker at every draw call:
            SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);

            // Create the tooltip object:
            toolTip = new ToolTip();

            drawLast = firstKnob;

            ScrollX = 0;
        }

        /// <summary>
        /// Gets the range of the timeline knobs, in a Point, being the X component
        /// the starting value, and the Y component, the range of the selection
        /// </summary>
        /// <returns></returns>
        public Point GetRange()
        {
            // Setup the knobs temps:

            // Select the knobs by checking which one is coming first in the timeline:
            var firstKnobRange = firstKnob.Value <= secondKnob.Value ? firstKnob : SecondKnob;
            var secondKnobRange = firstKnobRange == firstKnob ? secondKnob : firstKnob;

            // Return a newly created point:
            return new Point(firstKnobRange.Value, secondKnobRange.Value - firstKnobRange.Value);
        }

        /// <summary>
        /// OnPaint event for this TimelineControl that paints the whole timeline and the knobs
        /// </summary>
        /// <param name="e">The PaintEventArgs for this event</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // Enabling flags
            bool knobsEnabled = behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange;
            bool rangeEnabled = behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange;

            // Setup the graphics object:
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            base.OnPaint(e);

            // Setup the knobs temps:

            // Select the knobs by checking which one is coming first in the timeline:
            var firstKnobRange = firstKnob.Value <= secondKnob.Value ? firstKnob : SecondKnob;
            var secondKnobRange = firstKnobRange == firstKnob ? secondKnob : firstKnob;

            // Calculate the timeline size to draw:
            Rectangle timelineRect = GetTimelineRect();

            // Fill out the timeline rectangle:
            e.Graphics.FillRectangle(Brushes.LightGray, timelineRect);

            // Draw the timeline's outline:
            e.Graphics.DrawRectangle(Pens.DarkGray, timelineRect);

            if (maximum - minimum > 0)
            {
                double mult = (Width * ScrollScaleWidth) - firstKnobRange.KnobThickness - 2;

                // Draw ticks:
                for (float i = 0; i <= (maximum - minimum); i++)
                {
                    // Calculate the X portion of the line:
                    float x1 = i / (maximum - minimum) * (float)mult + firstKnobRange.KnobThickness / 2.0f - (float)ScrollX;

                    // Check to see if the newly calculated X portion is inside the clip rectangle:
                    if (e.ClipRectangle.X - 1 > x1)
                        continue;

                    // No need to keep drawing if off of bounds:
                    if (e.ClipRectangle.X + e.ClipRectangle.Width < x1)
                        break;

                    // Draw the first line (inside the timeline):
                    if (i < maximum - minimum)
                        e.Graphics.DrawLine(Pens.DarkGray, x1, 2, x1, timelineHeight - 2);

                    // Draw the second line (outside the timeline):
                    e.Graphics.DrawLine(Pens.DarkGray, x1, timelineHeight + 2, x1, timelineHeight + firstKnobRange.KnobHeight);
                }

                if (rangeEnabled)
                {
                    // Draw selected region:            
                    int x = (int)firstKnobRange.GetRealX();
                    int width = (int)secondKnobRange.GetRealX() - x;

                    // Calculate the new size for the selected area:
                    timelineRect = new Rectangle(firstKnobRange.KnobThickness / 2 + x + 2, 2, width - 3, timelineHeight - 3);

                    var baseColor = !Enabled ? Color.LightGray : (behaviorType == TimelineBehaviorType.TimelineWithRange ? Color.CornflowerBlue : Color.DarkCyan);
                    var lightColor = Color.FromArgb(Math.Min(baseColor.R + 40, 255), Math.Min(baseColor.G + 40, 255), Math.Min(baseColor.B + 40, 255));

                    Brush baseBrush = new SolidBrush(baseColor);
                    Brush lightBrush = new SolidBrush(lightColor);

                    // Draw out the new area:
                    if (draggingTimeline && behaviorType == TimelineBehaviorType.RangeSelector)
                        e.Graphics.FillRectangle(lightBrush, timelineRect);
                    else if (mouseOverCurrentRange && behaviorType == TimelineBehaviorType.RangeSelector)
                        e.Graphics.FillRectangle(lightBrush, timelineRect);
                    else
                        e.Graphics.FillRectangle(baseBrush, timelineRect);

                    baseBrush.Dispose();
                    lightBrush.Dispose();
                }

                // Draw the number indicator:
                {
                    int textY = timelineHeight + 12;

                    // Draw the first frame:
                    e.Graphics.DrawString(minimum + "", Font, Brushes.DarkGray, -(float)ScrollX, textY);

                    // Draw the last frame:
                    e.Graphics.DrawString("" + maximum, _font, Brushes.DarkGray, (float)(Width * ScrollScaleWidth - firstKnobRange.KnobThickness / 2.0f - 1) - _maxSize - (float)ScrollX, textY);

                    // Draw the middle frame (only if there is more than 1 frame):
                    if (maximum - minimum > 1)
                    {
                        e.Graphics.DrawString("" + (maximum + minimum) / 2, _font, Brushes.DarkGray, (float)(Width * ScrollScaleWidth / 2) - _medSize / 2 - (float)ScrollX, textY);
                    }
                }

                if (knobsEnabled)
                {
                    // Draw the indicators (black lines at the top of the knobs):
                    firstKnobRange.DrawIndicator(e.Graphics);
                    secondKnobRange.DrawIndicator(e.Graphics);
                }

                // Draw the frame playhead indicator:
                if (currentFrame >= minimum)
                {
                    DrawFrameIndicator(e, CurrentFrame);
                }
            }

            if (mouseOverTimeline)
            {
                if (displayFrameUnderMouse)
                {
                    DrawFrameIndicator(e, currentFrameUnderMouse, 0.5f);
                }
            }

            if (knobsEnabled)
            {
                // Draw the knobs:
                if (drawLast == secondKnobRange || drawLast == null)
                {
                    // First knob first:
                    firstKnobRange.Draw(e.Graphics);
                    secondKnobRange.Draw(e.Graphics);
                }
                else
                {
                    // Second knob first:
                    secondKnobRange.Draw(e.Graphics);
                    firstKnobRange.Draw(e.Graphics);
                }
            }
        }
        /// <summary>
        /// OnMouseDown event called whenever the user clicks down on this control
        /// </summary>
        /// <param name="e">The PaintEventArgs for this event</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            Focus();

            if (!Enabled)
                return;

            // Enabling flags
            bool knobsEnabled = behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange;
            bool rangeEnabled = behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange;
            bool frameDrag = behaviorType == TimelineBehaviorType.Timeline || behaviorType == TimelineBehaviorType.TimelineWithRange;
            bool rangeDrag = behaviorType == TimelineBehaviorType.RangeSelector;

            if (e.Button == MouseButtons.Left)
            {
                // Flag that specifies whether the next procedure has successfully captured a knob under the mouse to drag
                bool succeeded = false;

                if (knobsEnabled || rangeEnabled)
                {
                    // Get the distance between the mouse and the knobs:
                    double fx = Math.Abs(e.X - firstKnob.ScaledX - firstKnob.KnobThickness / 2.0f);
                    double sx = Math.Abs(e.X - secondKnob.ScaledX - secondKnob.KnobThickness / 2.0f);

                    bool overY = behaviorType == TimelineBehaviorType.RangeSelector || e.Y > timelineHeight + 2;

                    // Set the dragging knob here:
                    if (fx < sx)
                    {
                        // Drag only if in the range of the knob:
                        if (fx <= firstKnob.KnobThickness / 2.0f && overY)
                        {
                            // Set this knob as the one currently being dragged:
                            drag = firstKnob;

                            succeeded = true;
                        }
                    }
                    else
                    {
                        // Drag only if in the range of the knob:
                        if (sx <= secondKnob.KnobThickness / 2.0f && overY)
                        {
                            // Set this knob as the one currently being dragged:
                            drag = secondKnob;

                            succeeded = true;
                        }
                    }
                }

                // If a knob is being dragged
                if (succeeded)
                {
                    _dragOffset.X = (float)drag.ScaledX - e.X + drag.KnobThickness / 2.0f;

                    // Show the tooltip:
                    toolTip.Show("" + drag.Value, this, (int)drag.ScaledX, -25, 1000);
                }
                // If no knob is being dragged, try grabbing the timeline instead
                else if (rangeDrag)
                {
                    if (e.X > Math.Min(firstKnob.ScaledX, secondKnob.ScaledX) && e.X < Math.Max(firstKnob.ScaledX, secondKnob.ScaledX) && e.Y < timelineHeight)
                    {
                        _dragOffset.X = (float)((e.X + ScrollX - firstKnob.KnobThickness / 2.0f) / ScrollScaleWidth) / (float)((Width - firstKnob.KnobThickness / ScrollScaleWidth) / (maximum - minimum));
                        drag = firstKnob;
                        draggingTimeline = true;
                        draggingTimelineRange = GetRange();
                        Invalidate();
                    }
                }
                // If frame dragging is enabled, try dragging the frame instead
                else if (frameDrag)
                {
                    if (IsMouseOnTimeline())
                    {
                        draggingFrame = true;

                        ChangeFrame(GetFrameUnderMouse());
                    }
                }
            }
            // Reset scaling with the middle mouse button
            else if (e.Button == MouseButtons.Middle)
            {
                bool redraw = false;
                
                if (Math.Abs(ScrollX) > float.Epsilon)
                {
                    ScrollX = 0;
                    redraw = true;
                }

                if (Math.Abs(ScrollScaleWidth - 1) > float.Epsilon)
                {
                    ScrollScaleWidth = 1;
                    redraw = true;
                }

                if (redraw)
                {
                    Invalidate();
                }
            }
            // Start scrolling with the right mouse button
            else if (e.Button == MouseButtons.Right)
            {
                _dragOffset.X = (int)ScrollX + e.X;
                draggingView = true;
            }
        }
        /// <summary>
        /// OnMouseMove event called whenever the user moves the mouse on this control
        /// </summary>
        /// <param name="e">The MouseEventArgs for this event</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!Enabled)
                return;

            // Enabling flags
            bool knobsEnabled = behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange;
            bool rangeEnabled = behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange;

            // Whether to redraw the knobs:
            bool redrawKnobs = false;

            // If the current behavior is set to display under mouse, invalidate the old (and new) mouse position
            if (displayFrameUnderMouse)
            {
                UpdateFrameUnderMouseDisplay();
            }

            // Check whether the mouse is currently over the timeline
            UpdateMouseOverTimeline();

            // The user is dragging the timeline
            if (draggingTimeline)
            {
                double eX = ((e.X + ScrollX - firstKnob.KnobThickness / 2.0f) / ScrollScaleWidth) / ((Width - firstKnob.KnobThickness / ScrollScaleWidth) / (maximum - minimum));
                int diff = (int)(eX - _dragOffset.X);

                // Clamp the movement delta
                int moveDelta = diff + draggingTimelineRange.X;

                if (moveDelta < 1)
                    moveDelta = 1;
                if (moveDelta + draggingTimelineRange.Y > maximum)
                    moveDelta = maximum - draggingTimelineRange.Y;

                // Change the range
                if (moveDelta != _temp)
                {
                    _temp = moveDelta;

                    // Move the knobs
                    firstKnob.Value = moveDelta;
                    secondKnob.Value = moveDelta + draggingTimelineRange.Y;

                    // Clip the current frame to be in between the range
                    if (currentFrame < firstKnob.Value)
                    {
                        ChangeFrame(firstKnob.Value);
                    }

                    // Clip the current frame to be in between the range
                    if (currentFrame > secondKnob.Value)
                    {
                        ChangeFrame(secondKnob.Value);
                    }

                    // Redraw the control
                    Invalidate();

                    // Invoke the change event
                    RangeChanged?.Invoke(this, new RangeChangedEventArgs(GetRange()));
                }
            }
            // The user is dragging the view
            else if (draggingView)
            {
                double lastScroll = ScrollX;

                ScrollX = -(e.X - _dragOffset.X);

                if (Math.Abs(lastScroll - ScrollX) > float.Epsilon)
                    Invalidate();
            }
            // The user is dragging the current frame
            else if (draggingFrame)
            {
                int newFrame = GetFrameUnderMouse();

                if (newFrame != currentFrame)
                {
                    ChangeFrame(newFrame);
                }
            }
            // The user is hovering the mouse over the control
            else if (drag != null)
            {
                double eX = e.X + _dragOffset.X + (int)ScrollX;

                // Calculate new value:
                double newValue = minimum + Math.Round((eX - drag.KnobThickness / 2.0f) / ((Width * ScrollScaleWidth) - drag.KnobThickness - 1) * (maximum - minimum));

                // Whether to redraw:
                bool redraw = Math.Abs(newValue - drag.Value) > float.Epsilon;

                // Check for redrawing. Should only redraw when the new value is different from the last value in the knob:

                // Set the last X position of the knob. Used to set the redraw rectangle.
                double lastX = drag.ScaledX;

                // Set the knob's new value:
                drag.Value = (int)newValue;

                // Whether to redraw or not:
                if (redraw)
                {
                    // Clip the current frame to be in between the range
                    if (currentFrame < firstKnob.Value)
                    {
                        ChangeFrame(firstKnob.Value);
                    }

                    // Clip the current frame to be in between the range
                    if (currentFrame > secondKnob.Value)
                    {
                        ChangeFrame(secondKnob.Value);
                    }

                    // Calculate the redraw rectangle:
                    double x = Math.Min(drag.ScaledX, lastX) - drag.KnobThickness;
                    double width = Math.Max(drag.ScaledX, lastX) - x + drag.KnobThickness * 2;

                    // Set the redraw rectangle:
                    Invalidate(new Rectangle((int)x, 0, (int)width, timelineHeight + drag.KnobHeight + 1 + (int)drag.DrawOffset.Y));

                    // Show the tooltip:
                    toolTip.Show("" + (drag.Value), this, (int)(drag.ScaledX), -25, 1000);

                    RangeChanged?.Invoke(this, new RangeChangedEventArgs(GetRange()));
                }

                // Set the knob mouse over setting:
                drag.MouseOver = true;
            }
            else
            {
                if (knobsEnabled)
                {
                    // Get the distance between the mouse and the knobs:
                    double fx = Math.Abs(e.X - firstKnob.ScaledX - firstKnob.KnobThickness / 2.0f);
                    double sx = Math.Abs(e.X - secondKnob.ScaledX - secondKnob.KnobThickness / 2.0f);
                    bool overY = (behaviorType == TimelineBehaviorType.RangeSelector || e.Y > timelineHeight + 2);

                    // I tried optimizing this bit as much as I could, and right now, it behaves pretty fast:
                    if (fx < sx)
                    {
                        // If the mouse is near enough:
                        if (fx <= firstKnob.KnobThickness / 2.0f && overY)
                        {
                            // Show the tooltip:
                            if (!firstKnob.MouseOver)
                                toolTip.Show("" + (firstKnob.Value), this, (int)firstKnob.ScaledX, -25, 1000);

                            // Change the state of the knobs:
                            firstKnob.MouseOver = true;
                            secondKnob.MouseOver = false;

                            // Set to redraw:
                            redrawKnobs = true;

                            // Set this knob to draw over the other knob:
                            drawLast = firstKnob;

                            if (mouseOverCurrentRange)
                                Invalidate();

                            // Reset the mouse over timeline flag
                            mouseOverCurrentRange = false;
                        }
                        // If not, un-highlight it:
                        else if (firstKnob.MouseOver)
                        {
                            // Change the state of the knobs:
                            firstKnob.MouseOver = false;
                            secondKnob.MouseOver = false;

                            // Set to redraw:
                            redrawKnobs = true;

                            // Hide the tooltip:
                            toolTip.Hide(this);
                        }
                    }
                    // Second knob check:
                    else
                    {
                        // If the mouse is near enough:
                        if (sx <= secondKnob.KnobThickness / 2.0f && overY)
                        {
                            // Show the tooltip:
                            if (!secondKnob.MouseOver)
                                toolTip.Show("" + (secondKnob.Value), this, (int)secondKnob.ScaledX, -25, 1000);

                            // Change the state of the knobs:
                            firstKnob.MouseOver = false;
                            secondKnob.MouseOver = true;

                            // Set to redraw:
                            redrawKnobs = true;

                            // Set this knob to draw over the other knob:
                            drawLast = secondKnob;

                            if (mouseOverCurrentRange)
                                Invalidate();

                            // Reset the mouse over timeline flag
                            mouseOverCurrentRange = false;
                        }
                        // If not, un-highlight it:
                        else if (secondKnob.MouseOver)
                        {
                            // Change the state of the knobs:
                            firstKnob.MouseOver = false;
                            secondKnob.MouseOver = false;

                            // Set to redraw:
                            redrawKnobs = true;

                            // Hide the tooltip:
                            toolTip.Hide(this);
                        }
                    }
                }

                if (rangeEnabled && !firstKnob.MouseOver && !secondKnob.MouseOver)
                {
                    if (e.X > Math.Min(firstKnob.ScaledX, secondKnob.ScaledX) && e.X < Math.Max(firstKnob.ScaledX, secondKnob.ScaledX) && e.Y < timelineHeight)
                    {
                        mouseOverCurrentRange = true;
                        Invalidate();
                    }
                    else if (mouseOverCurrentRange)
                    {
                        mouseOverCurrentRange = false;
                        Invalidate();
                    }
                }
            }

            // If set to redraw:
            if (redrawKnobs)
            {
                // Redraw the knobs:
                Invalidate(new Rectangle((int)firstKnob.ScaledX, timelineHeight - 3, (firstKnob.KnobThickness * 2), timelineHeight * 2 + (int)firstKnob.DrawOffset.Y));
                Invalidate(new Rectangle((int)secondKnob.ScaledX, timelineHeight - 3, (secondKnob.KnobThickness * 2), timelineHeight * 2 + (int)secondKnob.DrawOffset.Y));
            }
        }
        /// <summary>
        /// OnMouseUp event called whenever the user releases the mouse on this control
        /// </summary>
        /// <param name="e">The MouseEventArgs for this event</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (draggingTimeline)
            {
                Invalidate();
            }

            // Set the drag to a null:
            drag = null;
            draggingFrame = false;
            draggingTimeline = false;
            draggingView = false;
        }
        /// <summary>
        /// OnMouseCaptureChanged event called whenever the capture target has changed
        /// </summary>
        /// <param name="e">The EventArgs for this event</param>
        protected override void OnMouseCaptureChanged(EventArgs e)
        {
            base.OnMouseCaptureChanged(e);

            if (draggingTimeline)
            {
                Invalidate();
            }

            // Set the drag to a null:
            drag = null;
            draggingFrame = false;
            draggingTimeline = false;
            draggingView = false;
        }
        /// <summary>
        /// OnMouseUp event called whenever the user leaves the mouse out of this control's bounds
        /// </summary>
        /// <param name="e">The MouseEventArgs for this event</param>
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Enabled)
                return;

            mouseInsideControl = true;

            // Check whether the mouse is currently over the timeline
            UpdateMouseOverTimeline();

            // If the current behavior is set to display under mouse, invalidate the old (and new) mouse position
            if (displayFrameUnderMouse)
            {
                UpdateFrameUnderMouseDisplay();
            }
        }
        /// <summary>
        /// OnMouseUp event called whenever the user leaves the mouse out of this control's bounds
        /// </summary>
        /// <param name="e">The MouseEventArgs for this event</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!Enabled)
                return;

            mouseInsideControl = false;
            mouseOverTimeline = false;

            // If the current behavior is set to display under mouse, invalidate the old (and new) mouse position
            if (displayFrameUnderMouse)
            {
                UpdateFrameUnderMouseDisplay();
            }

            // Redraw the knobs:
            if (firstKnob.MouseOver)
                Invalidate(new Rectangle((int)firstKnob.X - firstKnob.KnobThickness - (int)ScrollX, timelineHeight - 3, (int)firstKnob.X + firstKnob.KnobThickness, timelineHeight * 2));
            if (secondKnob.MouseOver)
                Invalidate(new Rectangle((int)secondKnob.X - secondKnob.KnobThickness - (int)ScrollX, timelineHeight - 3, (int)secondKnob.X + secondKnob.KnobThickness, timelineHeight * 2));

            // Set both knobs' MouseOver property to false:
            firstKnob.MouseOver = false;
            secondKnob.MouseOver = false;

            if (mouseOverCurrentRange)
            {
                mouseOverCurrentRange = false;
                Invalidate();
            }
        }
        /// <summary>
        /// OnMouseWheel event called whenever the user scrolls with the mouse wheel inside this control
        /// </summary>
        /// <param name="e">The MouseEventArgs for this event</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!Enabled)
                return;

            // Scroll the view
            if (e.X > 0 && e.X < Width && e.Y > 0 && e.Y < Height)
            {
                double oldX = (e.X * ((double)Width / (Width - firstKnob.KnobThickness)) - (double)firstKnob.KnobThickness / 2.0f) * ScrollScaleWidth;

                ScrollScaleWidth += (float)e.Delta / 120 / 3;

                if (ScrollScaleWidth < 1f / 3)
                {
                    ScrollScaleWidth = 1f / 3;
                }
                else if (ScrollScaleWidth > 30f)
                {
                    ScrollScaleWidth = 30f;
                }

                double newX = (e.X * ((double)Width / (Width - firstKnob.KnobThickness)) - (double)firstKnob.KnobThickness / 2.0f) * ScrollScaleWidth;

                ScrollX += (newX - oldX);

                Invalidate();
            }
        }
        /// <summary>
        /// OnSizeChanged event called whenever the control changes size and redraws the control
        /// </summary>
        /// <param name="e">The EventArgs for this event</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            // Invalidate:
            Invalidate();

            // Change the scroll position to match the new position
            if (_lastWidth != 0)
            {
                ScrollX /= (float)_lastWidth / Width;
            }

            // Set the knobs to calculate new X properties:
            firstKnob.Update();
            secondKnob.Update();

            // Update the lastWidth variable
            _lastWidth = Width;
        }

        /// <summary>
        /// OnEnabledChanged event called whenever the Enabled property is changed
        /// </summary>
        /// <param name="e">The EventArgs for this event</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);

            Invalidate();

            // Set the drag to a null:
            drag = null;
            draggingFrame = false;
            draggingTimeline = false;
            draggingView = false;
        }

        /// <summary>
        /// Draws the frame indicator on the given frame with the given paint args
        /// </summary>
        /// <param name="e">The paint args to draw the frame indicator on</param>
        /// <param name="frame">The frame index to draw</param>
        /// <param name="alpha">The alpha modifier to use during the drawing process</param>
        private void DrawFrameIndicator(PaintEventArgs e, int frame, float alpha = 1)
        {
            // Create the playhead translation point:
            var playheadPos1 = Point.Empty;

            var backColor = Color.FromArgb((int)(255 * alpha), 255, 255, 255);
            var textColor = Color.FromArgb((int)(255 * alpha), 0, 0, 0);

            // Set the playhead translation point:
            playheadPos1.X = (firstKnob.KnobThickness / 2) + (int)((float)(frame - minimum) / Math.Max(1, maximum - minimum) * ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 1)) - (int)ScrollX;
            playheadPos1.Y = 0;

            if (frameDisplayType == TimelineFrameDisplayType.Tick)
            {
                // Create the playhead size point:
                var playheadPos2 = new Point(playheadPos1.X, timelineHeight);

                // Draw the playhead:
                e.Graphics.DrawLine(new Pen(backColor, 4), playheadPos1, playheadPos2);
            }
            else if (frameDisplayType == TimelineFrameDisplayType.FrameNumber)
            {
                string frameText = frame + "";
                var frameTextSize = e.Graphics.MeasureString(frameText, _font);

                Brush backBrush = new SolidBrush(backColor);
                Brush textBrush = new SolidBrush(textColor);

                e.Graphics.FillRectangle(backBrush, new RectangleF(playheadPos1.X - frameTextSize.Width / 2, playheadPos1.Y, frameTextSize.Width, timelineHeight + 1));
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                e.Graphics.DrawString(frame + "", _font, textBrush, playheadPos1.X - frameTextSize.Width / 2, playheadPos1.Y - 1);

                backBrush.Dispose();
                textBrush.Dispose();
            }
        }

        /// <summary>
        /// Gets a Rectangle that represents the timeline display area
        /// </summary>
        /// <returns>A Rectangle that represents the timeline display area</returns>
        protected Rectangle GetTimelineRect()
        {
            return new Rectangle(firstKnob.KnobThickness / 2 - (int)ScrollX, 0, (int)(Width * ScrollScaleWidth) - firstKnob.KnobThickness - 2, timelineHeight);
        }

        /// <summary>
        /// Changes the current frame being displayed
        /// </summary>
        /// <param name="newFrame">The new frame to display</param>
        public void ChangeFrame(int newFrame)
        {
            if (currentFrame == newFrame)
                return;

            int oldFrame = currentFrame;

            if (FrameChanged != null)
            {
                var evArgs = new FrameChangedEventArgs(oldFrame, newFrame);

                FrameChanged.Invoke(this, evArgs);

                if (evArgs.Cancel)
                    return;
            }

            CurrentFrame = newFrame;
        }

        /// <summary>
        /// Gets the frame currently under the mouse pointer.
        /// If a range is currently visible to the user, the value is truncated
        /// between [MinimumRange - MaximumRange] inclusive, if not, the value is
        /// truncated between [Minimum - Maximum] inclusive
        /// </summary>
        /// <param name="clipOnRange">Whether to clip on the current range if available</param>
        /// <returns>The frame under the mouse pointer</returns>
        protected int GetFrameUnderMouse(bool clipOnRange = true)
        {
            double totalWidth = ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 2);
            double mx = (PointToClient(MousePosition).X + ScrollX - firstKnob.KnobThickness / 2.0f) / totalWidth;

            int f = Math.Max(minimum, Math.Min(maximum, minimum + (int)Math.Round(mx * (maximum - minimum))));

            var range = GetRange();

            if (clipOnRange && behaviorType == TimelineBehaviorType.RangeSelector || behaviorType == TimelineBehaviorType.TimelineWithRange && disableFrameSelectionOutOfRange)
            {
                f = Math.Max(range.X, Math.Min(range.X + range.Y, f));
            }

            return f;
        }

        /// <summary>
        /// Updates the mouseOverTimeline flag
        /// </summary>
        private void UpdateMouseOverTimeline()
        {
            mouseOverTimeline = IsMouseOnTimeline();
        }

        /// <summary>
        /// Updates the display of the frame currently under the mouse
        /// </summary>
        protected void UpdateFrameUnderMouseDisplay()
        {
            InvalidateUnderMouse();

            currentFrameUnderMouse = GetFrameUnderMouse();

            InvalidateUnderMouse();
        }

        /// <summary>
        /// Returns whether the mouse is currently over the timeline
        /// </summary>
        /// <returns>Whether the mouse is currently over the timeline</returns>
        protected bool IsMouseOnTimeline()
        {
            double x = firstKnob.KnobThickness / 2.0f - ScrollX;
            double w = Width * ScrollScaleWidth - firstKnob.KnobThickness - 2;

            int mx = PointToClient(MousePosition).X;
            int my = PointToClient(MousePosition).Y;

            return mx > x && mx < x + w && my < timelineHeight;
        }

        /// <summary>
        /// Invalidates the playhead position
        /// </summary>
        protected void InvalidatePlayhead()
        {
            var g = CreateGraphics();

            string frameText = currentFrame + "";
            var frameTextSize = g.MeasureString(frameText, _font);

            Invalidate(new Rectangle(firstKnob.KnobThickness / 2 + (int)((float)(currentFrame - minimum) / Math.Max(1, maximum - minimum) * (Width * ScrollScaleWidth - firstKnob.KnobThickness - 1)) - (int)ScrollX - (int)frameTextSize.Width / 2 - 1, 0, (int)frameTextSize.Width + 2, timelineHeight + 1));

            g.Dispose();
        }

        /// <summary>
        /// Invalidates the region under the mouse. Utilized when the mouse is hovering over the control and DisplayFrameUnderMouse is set to true
        /// </summary>
        protected void InvalidateUnderMouse()
        {
            int frame = currentFrameUnderMouse;

            var g = CreateGraphics();

            string frameText = frame + "";
            var frameTextSize = g.MeasureString(frameText, _font);

            Invalidate(new Rectangle(firstKnob.KnobThickness / 2 + (int)((float)(frame - minimum) / Math.Max(1, maximum - minimum) * (Width * ScrollScaleWidth - firstKnob.KnobThickness - 1)) - (int)ScrollX - (int)frameTextSize.Width / 2 - 1, 0, (int)frameTextSize.Width + 2, timelineHeight + 1));

            g.Dispose();
        }

        /// <summary>
        /// The first timeline knob
        /// </summary>
        protected Knob firstKnob;
        /// <summary>
        /// The second timeline knob
        /// </summary>
        protected Knob secondKnob;

        /// <summary>
        /// The minimum value this TimelineControl will display
        /// </summary>
        protected int minimum;

        /// <summary>
        /// The maximum value this TimelineControl will display
        /// </summary>
        protected int maximum;

        /// <summary>
        /// The current frame being displayed
        /// </summary>
        protected int currentFrame;
        /// <summary>
        /// The current frame under the mouse
        /// </summary>
        protected int currentFrameUnderMouse;
        /// <summary>
        /// The knob to draw the last
        /// </summary>
        protected Knob drawLast;

        /// <summary>
        /// The mouse drag offset
        /// </summary>
        private PointF _dragOffset = PointF.Empty;

        /// <summary>
        /// This control's last width before resizing
        /// </summary>
        private int _lastWidth;

        /// <summary>
        /// The pre-calculated middle label width
        /// </summary>
        private float _medSize;
        /// <summary>
        /// The pre-calculated end label width
        /// </summary>
        private float _maxSize;

        /// <summary>
        /// Create the font object this control will use to draw the texts:
        /// </summary>
        private readonly Font _font = new Font("Segoi UI", 8.1f);

        /// <summary>
        /// Temporary integer used in various calculations
        /// </summary>
        private int _temp;
    }

    /// <summary>
    /// Event arguments for the RangeChanged event
    /// </summary>
    public class RangeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new range
        /// </summary>
        public Point NewRange { get; }

        /// <summary>
        /// Initializes a new instance of the RangeChangedEventArgs class
        /// </summary>
        /// <param name="newRange">The new range for the timeline selection</param>
        public RangeChangedEventArgs(Point newRange)
        {
            NewRange = newRange;
        }
    }

    /// <summary>
    /// The event arguments for a FrameChanged event
    /// </summary>
    public class FrameChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new frame selected
        /// </summary>
        public int NewFrame { get; }

        /// <summary>
        /// The previous frame selected
        /// </summary>
        public int OldFrame { get; }

        /// <summary>
        /// Whether to cancel this event and not modify the frame
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the FrameChangedEventArgs class
        /// </summary>
        /// <param name="oldFrame">The previous frame selected</param>
        /// <param name="newFrame">The new frame selected</param>
        public FrameChangedEventArgs(int oldFrame, int newFrame)
        {
            OldFrame = oldFrame;
            NewFrame = newFrame;
        }
    }

    /// <summary>
    /// Represents a TimelineControl knob
    /// </summary>
    public class Knob: IDisposable
    {
        /// <summary>
        /// This knob's tick offset
        /// </summary>
        protected int offset;
        /// <summary>
        /// This knob's value
        /// </summary>
        protected int value;
        /// <summary>
        /// The parent TimelineControl that's hosting this knob
        /// </summary>
        protected TimelineScrubControl parent;
        /// <summary>
        /// This knob's X position
        /// </summary>
        protected double x;
        /// <summary>
        /// This knob's X scale
        /// </summary>
        protected double scaledX;
        /// <summary>
        /// This knob's drawing offset
        /// </summary>
        protected PointF drawOffset;

        /// <summary>
        /// Gets or sets this knob's tick offset
        /// </summary>
        public int Offset
        {
            get => offset;
            set => offset = Math.Max(0, Math.Min(parent.Maximum - parent.Minimum, value));
        }

        /// <summary>
        /// Gets or sets this knob's value
        /// </summary>
        public int Value
        {
            get => value;
            set { this.value = Math.Max(parent.Minimum, Math.Min(parent.Maximum, value)); Update(); }
        }

        /// <summary>
        /// Gets or sets the parent TimelineControl that's hosting this knob
        /// </summary>
        public TimelineScrubControl Parent
        {
            get => parent;
            protected set => parent = value;
        }

        /// <summary>
        /// Gets or sets this knob's X position
        /// </summary>
        public double X
        {
            get => x;
            set => x = value;
        }

        /// <summary>
        /// Gets this knob's scaled X component based on the parent TimelineControl's size
        /// </summary>
        public double ScaledX => GetRealX();

        /// <summary>
        /// Gets or sets this knob's drawing offset
        /// </summary>
        public PointF DrawOffset
        {
            get => drawOffset;
            set => drawOffset = value;
        }

        /// <summary>
        /// Thickness of this knob, in pixels
        /// </summary>
        public int KnobThickness = 10;

        /// <summary>
        /// The knob height on screen, in pixels
        /// </summary>
        public int KnobHeight = 7;

        /// <summary>
        /// Whether the mouse is hovering over this knob
        /// </summary>
        public bool MouseOver;

        /// <summary>
        /// Initializes a new instance of the Knob control, binding it to a TimelineControl
        /// </summary>
        /// <param name="parent">A TimelineControl to bind to this KNob</param>
        public Knob(TimelineScrubControl parent)
        {
            this.parent = parent;

            value = 0;

            drawOffset = new PointF(0, 4);
        }

        ~Knob()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _b.Dispose();
            _path.Dispose();
        }

        /// <summary>
        /// Draws this knob into a graphics object
        /// </summary>
        /// <param name="e">The graphics to draw this knob on</param>
        public void Draw([NotNull] Graphics e)
        {
            var realDrawOffset = new PointF(drawOffset.X + (float)ScaledX,drawOffset.Y + parent.TimelineHeight);

            _b?.Dispose();

            // Fill it with a gray color if the parent is disabled
            if (!parent.Enabled)
            {
                _b = new SolidBrush(Color.LightGray);
            }
            // If the mouse is over the knob, fill it with a lighter color:
            else if (MouseOver)
            {
                _b = new LinearGradientBrush(realDrawOffset, new Point(KnobHeight * 2 + (int)realDrawOffset.X, KnobThickness + (int)realDrawOffset.Y), Color.LightGray, Color.DarkGray);
            }
            else
            {
                _b = new LinearGradientBrush(realDrawOffset, new Point(KnobHeight * 2 + (int)realDrawOffset.X, KnobThickness + (int)realDrawOffset.Y), Color.LightGray, Color.Gray);
            }

            _path.Reset();

            // Fill in the lines array:
            _lines[0] = new PointF(0, 0);
            _lines[1] = new PointF(KnobThickness / 2.0f, -3);
            _lines[2] = new PointF(KnobThickness, 0);
            _lines[3] = new PointF(KnobThickness, KnobHeight);
            _lines[4] = new PointF(0, KnobHeight);
            _lines[5] = _lines[0];

            // Offset all the lines:
            for (int i = 0; i < _lines.Length; i++)
            {
                _lines[i].X = (int)(_lines[i].X + realDrawOffset.X);
                _lines[i].Y = (int)(_lines[i].Y + realDrawOffset.Y);
            }

            // Add the lines to the path:
            _path.AddLines(_lines);

            // Fill the path:
            e.FillPath(_b, _path);

            // Draw the outline, and the line marker:
            e.DrawPath(Pens.Black, _path);
        }

        /// <summary>
        /// Draws this knob's indicator
        /// </summary>
        /// <param name="e">The graphics to draw the indicator on</param>
        public void DrawIndicator([NotNull] Graphics e)
        {
            var realDrawOffset = new PointF(drawOffset.X + (float)ScaledX, drawOffset.Y + parent.TimelineHeight);

            // Draw the indicator:
            e.DrawLine(Pens.Black, KnobThickness / 2 + (int)realDrawOffset.X, -3 + (int)realDrawOffset.Y, KnobThickness / 2 + (int)realDrawOffset.X, -(int)realDrawOffset.Y);
        }

        /// <summary>
        /// Update the positioning of this knob
        /// </summary>
        public void Update()
        {
            // Calculate the new position:
            x = KnobThickness / 2.0f + (float)value / Math.Max(parent.Maximum - parent.Minimum, 1) * (parent.Width - KnobThickness - 1);

            scaledX = GetRealX();
        }

        /// <summary>
        /// Gets the real X component of this knob
        /// </summary>
        /// <returns>The real X component of this knob, adjusted for the parent TimelineControl's scale</returns>
        public double GetRealX()
        {
            return (double)(value - parent.Minimum) / Math.Max(parent.Maximum - parent.Minimum, 1) * (parent.Width * parent.ScrollScaleWidth - KnobThickness - 1) - parent.ScrollX;
        }

        /// <summary>
        /// Linear gradient brush, used to draw the knob: 
        /// </summary>
        private Brush _b;
        /// <summary>
        /// Create the graphics path used to fill out the knob:
        /// </summary>
        private readonly GraphicsPath _path = new GraphicsPath();
        /// <summary>
        /// Create a bunch of lines used to fill out the knob:
        /// </summary>
        private readonly PointF[] _lines = new PointF[6];
    }

    /// <summary>
    /// Defines the type of interaction the timeline has with the user
    /// </summary>
    public enum TimelineBehaviorType
    {
        /// <summary>
        /// Specifies that the timeline is supposed to be a range selector,
        /// disabling the modification of the current frame and dragging the 
        /// timeline range when the user clicks over it
        /// </summary>
        RangeSelector,
        /// <summary>
        /// Specifies that the timeline is supposed to be a time selector,
        /// enabling the modification of the current frame and modifying it
        /// when the user clicks over it
        /// </summary>
        Timeline,
        /// <summary>
        /// Specifies that the timeline is supposed to be a time selector
        /// with the ability to select a range as well, enabling the modification
        /// of both the range and the current frame. When the user clicks the
        /// timeline area, the current frame is modified instead of dragging the
        /// range
        /// </summary>
        TimelineWithRange
    }

    /// <summary>
    /// Defines the type of drawing to use when displaying the current frame of a TimelineControl
    /// </summary>
    public enum TimelineFrameDisplayType
    {
        /// <summary>
        /// Displays a single white tick along the timeline bar
        /// </summary>
        Tick,
        /// <summary>
        /// Displays a frame number, surrounded by a blue box on the timeline
        /// </summary>
        FrameNumber
    }
}
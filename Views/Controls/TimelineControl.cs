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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Pixelaria.Views.Controls
{
    /// <summary>
    /// Describes a timeline control with two knobs,
    /// used to set a starting and ending point in a range
    /// </summary>
    [DefaultEvent("RangeChanged")]
    public class TimelineControl : Control
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
        /// Whether to disable selection of frames that are out of range
        /// </summary>
        protected bool disableFrameSelectionOutOfRange;

        /// <summary>
        /// ToolTip instance associated with this timeline. Used to show the frames the knobs are pointing to
        /// </summary>
        protected ToolTip ToolTip;

        /// <summary>
        /// The control's X scroll
        /// </summary>
        protected float scrollX = 0;
        /// <summary>
        /// The control's Width scale modifier
        /// </summary>
        protected float scrollScaleWidth = 1;

        /// <summary>
        /// The knob currently being dragged
        /// </summary>
        protected Knob drag;

        /// <summary>
        /// Whether the user is dragging the view
        /// </summary>
        protected bool draggingView = false;
        /// <summary>
        /// Whether the user is dragging the current frame
        /// </summary>
        protected bool draggingFrame = false;
        /// <summary>
        /// Whether the user is dragging the timeline
        /// </summary>
        protected bool draggingTimeline = false;
        /// <summary>
        /// Whether the user is hovering the mouse over the timeline
        /// </summary>
        protected bool mouseOverTimeline = false;
        /// <summary>
        /// Point representing the range before the user started dragging the timeline
        /// </summary>
        protected Point draggingTimelineRange = new Point();

        /// <summary>
        /// States the current frame in the seekbar. Set to -1 to highlight no frame.
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [DefaultValue(-1)]
        [Description("The current playing frame on the timeline")]
        public int CurrentFrame
        {
            get { return currentFrame; }
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
            get { return this.firstKnob; }
            protected set { this.firstKnob = value; }
        }

        /// <summary>
        /// Gets or sets this TimelineControl's second knob
        /// </summary>
        [Browsable(false)]
        public Knob SecondKnob
        {
            get { return this.secondKnob; }
            protected set { this.secondKnob = value; }
        }

        /// <summary>
        /// Gets or sets the currently selected value range of this TimelineControl
        /// </summary>
        [Browsable(false)]
        public Point Range
        {
            get { return GetRange(); }
            set
            {
                if (value == GetRange())
                    return;

                if (value.X < minimum)
                    value.X = minimum;
                if (value.Y > maximum - value.X)
                    value.Y = maximum - value.X;

                this.firstKnob.Value = value.X;
                this.secondKnob.Value = value.Y + value.X;

                Invalidate();
            }
        }

        /// <summary>
        /// Gets whether the user is currently dragging the frame
        /// </summary>
        [Browsable(false)]
        public bool DraggingFrame
        {
            get { return draggingFrame; }
        }

        /// <summary>
        /// Gets or sets this TimelineControl's minimum value
        /// </summary>
        [Browsable(true)]
        [Category("Behavior")]
        [Description("The minimum value on the available range")]
        public int Minimum
        {
            get { return minimum; }
            set
            {
                // Don't update if the values are the same
                if (minimum == value)
                    return;

                minimum = value;

                // Caches the text size:
                Graphics g = this.CreateGraphics();

                // Set a foramt flag:
                TextFormatFlags flags = TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;

                SizeF stringSize = TextRenderer.MeasureText(g, "" + (minimum), font, new Size(), flags);
                minSize = stringSize.Width;

                stringSize = TextRenderer.MeasureText(g, "" + (maximum), font, new Size(), flags);
                maxSize = stringSize.Width;

                // Draw the middle frame (only if there are more than 1 frames):
                if (maximum > 1)
                {
                    stringSize = TextRenderer.MeasureText(g, "" + (maximum + minimum) / 2, font, new Size(), flags);
                    medSize = stringSize.Width;
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
            get { return maximum; }
            set
            {
                // Don't update if the values are the same
                if (maximum == value)
                    return;

                maximum = value;

                // Caches the text size:
                Graphics g = this.CreateGraphics();

                // Set a foramt flag:
                TextFormatFlags flags = TextFormatFlags.GlyphOverhangPadding | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding;

                SizeF stringSize = TextRenderer.MeasureText(g, "" + (maximum), font, new Size(), flags);
                maxSize = stringSize.Width;

                // Draw the middle frame (only if there are more than 1 frames):
                if (maximum > 1)
                {
                    stringSize = TextRenderer.MeasureText(g, "" + (maximum + minimum) / 2, font, new Size(), flags);
                    medSize = stringSize.Width;
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
            get { return timelineHeight; }
            set { if (timelineHeight != value) { timelineHeight = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets the control's X scroll
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(0)]
        [Description("The control's X scroll")]
        public float ScrollX
        {
            get { return scrollX; }
            set { if (scrollX != value) { scrollX = value; Invalidate(); } }
        }

        /// <summary>
        /// Gets or sets the control's Width scale modifier 
        /// </summary>
        [Browsable(true)]
        [Category("Appearance")]
        [DefaultValue(1)]
        [Description("The control's Width scale modifier")]
        public float ScrollScaleWidth
        {
            get { return scrollScaleWidth; }
            set { if (scrollScaleWidth != value) { value = value < 0 ? 0.1f : value; scrollScaleWidth = value; Invalidate(); } }
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
            get { return behaviorType; }
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
            get { return frameDisplayType; }
            set { if (frameDisplayType != value) { frameDisplayType = value; Invalidate(); } }
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
            get { return disableFrameSelectionOutOfRange; }
            set { disableFrameSelectionOutOfRange = value; }
        }

        /// <summary>
        /// Event handler for the RangeChangedEvent
        /// </summary>
        /// <param name="sender">The object that fired this event</param>
        /// <param name="eventArgs">The event arguments for the event</param>
        public delegate void RangeChangedEventHandler(object sender, RangeChangedEventArgs eventArgs);
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
        /// <param name="eventArgs">The event arguments for the event</param>
        public delegate void FrameChangedEventHandler(object sender, FrameChangedEventArgs eventArgs);
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
        public TimelineControl()
        {
            // Initialize the internal fields
            behaviorType = TimelineBehaviorType.RangeSelector;

            // Create the knobs:
            firstKnob = new Knob(this);
            secondKnob = new Knob(this);

            // Set the bounds:
            this.Minimum = 0;
            this.Maximum = 1;
            this.currentFrame = -1;
            this.disableFrameSelectionOutOfRange = true;

            // Set the knobs' values:
            firstKnob.Value = this.minimum;
            secondKnob.Value = this.maximum;

            // Set the control's style so it won't flicker at every draw call:
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.UserPaint, true);

            // Create the tooltip object:
            ToolTip = new ToolTip();

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
            Knob firstKnob, secondKnob;

            // Select the knobs by checking which one is coming first in the timeline:
            firstKnob = this.firstKnob.Value <= this.secondKnob.Value ? this.firstKnob : this.SecondKnob;
            secondKnob = firstKnob == this.firstKnob ? this.secondKnob : this.firstKnob;

            // Return a newly created point:
            return new Point(firstKnob.Value, secondKnob.Value - firstKnob.Value);
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
            Knob firstKnob, secondKnob;

            // Select the knobs by checking which one is coming first in the timeline:
            firstKnob = this.firstKnob.Value <= this.secondKnob.Value ? this.firstKnob : this.SecondKnob;
            secondKnob = firstKnob == this.firstKnob ? this.secondKnob : this.firstKnob;

            // Calculate the timeline size to draw:
            Rectangle timelineRect = GetTimelineRect();

            // Fill out the timeline rectangle:
            e.Graphics.FillRectangle(Brushes.LightGray, timelineRect);

            // Draw the timeline's outline:
            e.Graphics.DrawRectangle(Pens.DarkGray, timelineRect);

            if (maximum - minimum > 0)
            {
                float mult = ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 2);

                // Draw ticks:
                for (float i = 0; i <= (maximum - minimum); i++)
                {
                    // Calculate the X portion of the line:
                    float x1 = (i / (maximum - minimum)) * mult + firstKnob.KnobThickness / 2 - ScrollX;

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
                    e.Graphics.DrawLine(Pens.DarkGray, x1, timelineHeight + 2, x1, timelineHeight + firstKnob.KnobHeigth);
                }

                if (rangeEnabled)
                {
                    // Draw selected region:            
                    int x = (int)firstKnob.GetRealX();
                    int width = (int)secondKnob.GetRealX() - x;

                    // Calculate the new size for the selected area:
                    timelineRect = new Rectangle(firstKnob.KnobThickness / 2 + x + 2, 2, width - 3, timelineHeight - 3);

                    Color baseColor = !Enabled ? Color.LightGray : (behaviorType == TimelineBehaviorType.TimelineWithRange ? Color.CornflowerBlue : Color.DarkCyan);
                    Color lightColor = Color.FromArgb(Math.Min(baseColor.R + 40, 255), Math.Min(baseColor.G + 40, 255), Math.Min(baseColor.B + 40, 255));

                    Brush baseBrush = new SolidBrush(baseColor);
                    Brush lightBrush = new SolidBrush(lightColor);

                    // Draw out the new area:
                    if (draggingTimeline && behaviorType == TimelineBehaviorType.RangeSelector)
                        e.Graphics.FillRectangle(lightBrush, timelineRect);
                    else if (mouseOverTimeline && behaviorType == TimelineBehaviorType.RangeSelector)
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
                    e.Graphics.DrawString(minimum + "", this.Font, Brushes.DarkGray, -ScrollX, textY);

                    // Draw the last frame:
                    e.Graphics.DrawString("" + (maximum), font, Brushes.DarkGray, ((Width * ScrollScaleWidth) - firstKnob.KnobThickness / 2 - 1) - (maxSize) - ScrollX, textY);

                    // Draw the middle frame (only if there is more than 1 frame):
                    if ((maximum - minimum) > 1)
                    {
                        e.Graphics.DrawString("" + (maximum + minimum) / 2, font, Brushes.DarkGray, ((Width * ScrollScaleWidth) / 2) - medSize / 2 - ScrollX, textY);
                    }
                }

                if (knobsEnabled)
                {
                    // Draw the indicators (black lines at the top of the knobs):
                    firstKnob.DrawIndicator(e.Graphics);
                    secondKnob.DrawIndicator(e.Graphics);
                }

                // Draw the frame playhead indicator:
                if (currentFrame >= minimum)
                {
                    // Create the playhead translation point:
                    Point playheadPos1 = Point.Empty;

                    // Set the playhead translation point:
                    playheadPos1.X = (firstKnob.KnobThickness / 2) + (int)((float)(currentFrame - minimum) / Math.Max(1, maximum - minimum) * ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 1)) - (int)ScrollX;
                    playheadPos1.Y = 0;

                    if (frameDisplayType == TimelineFrameDisplayType.Tick)
                    {
                        // Create the playhead size point:
                        Point playheadPos2 = new Point(playheadPos1.X, playheadPos1.Y);

                        // Set the playhead size point:
                        playheadPos2.Y = timelineHeight;

                        // Draw the playhead:
                        e.Graphics.DrawLine(new Pen(Color.White, 4), playheadPos1, playheadPos2);
                    }
                    else if (frameDisplayType == TimelineFrameDisplayType.FrameNumber)
                    {
                        string frameText = currentFrame + "";
                        SizeF frameTextSize = e.Graphics.MeasureString(frameText, this.font);

                        Brush brush = new SolidBrush(Color.White);

                        e.Graphics.FillRectangle(brush, new RectangleF(playheadPos1.X - frameTextSize.Width / 2, playheadPos1.Y, frameTextSize.Width, timelineHeight + 1));

                        brush.Dispose();

                        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                        e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                        e.Graphics.DrawString(currentFrame + "", this.font, Brushes.Black, playheadPos1.X - frameTextSize.Width / 2, playheadPos1.Y - 1);
                    }
                }
            }

            if (knobsEnabled)
            {
                // Draw the knobs:
                if (drawLast == secondKnob || drawLast == null)
                {
                    // First knob first:
                    firstKnob.Draw(e.Graphics);
                    secondKnob.Draw(e.Graphics);
                }
                else
                {
                    // Second knob first:
                    secondKnob.Draw(e.Graphics);
                    firstKnob.Draw(e.Graphics);
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
                // Flag that specifies whether the next proceedure has sucessfully captured a knob under the mouse to drag
                bool succeded = false;

                if (knobsEnabled)
                {
                    // Get the distance between the mouse and the knobs:
                    float fx = Math.Abs(e.X - firstKnob.ScaledX - firstKnob.KnobThickness / 2);
                    float sx = Math.Abs(e.X - secondKnob.ScaledX - secondKnob.KnobThickness / 2);

                    bool overY = (behaviorType == TimelineBehaviorType.RangeSelector || e.Y > timelineHeight + 2);

                    // Set the dragging knob here:
                    if (fx < sx)
                    {
                        // Drag only if in the range of the knob:
                        if (fx <= firstKnob.KnobThickness / 2 && overY)
                        {
                            // Set this knob as the one currently being dragged:
                            drag = firstKnob;

                            succeded = true;
                        }
                    }
                    else
                    {
                        // Drag only if in the range of the knob:
                        if (sx <= secondKnob.KnobThickness / 2 && overY)
                        {
                            // Set this knob as the one currently being dragged:
                            drag = secondKnob;

                            succeded = true;
                        }
                    }
                }

                // If a knob is being dragged
                if (succeded)
                {
                    dragOffset.X = drag.ScaledX - e.X + drag.KnobThickness / 2;

                    // Show the tooltip:
                    ToolTip.Show("" + (drag.Value), this, (int)drag.ScaledX, -25, 1000);
                }
                // If no knob is being dragged, try grabbing the timeline instead
                else if (rangeDrag)
                {
                    if (e.X > Math.Min(firstKnob.ScaledX, secondKnob.ScaledX) && e.X < Math.Max(firstKnob.ScaledX, secondKnob.ScaledX) && e.Y < timelineHeight)
                    {
                        dragOffset.X = ((e.X + ScrollX - firstKnob.KnobThickness / 2) / ScrollScaleWidth) / ((float)(Width - firstKnob.KnobThickness / ScrollScaleWidth) / (maximum - minimum));
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

                if (ScrollX != 0)
                {
                    ScrollX = 0;
                    redraw = true;
                }

                if (ScrollScaleWidth != 1)
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
                dragOffset.X = (int)ScrollX + e.X;
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

            // The user is dragging the timeline
            if (draggingTimeline)
            {
                float eX = ((e.X + ScrollX - firstKnob.KnobThickness / 2) / ScrollScaleWidth) / ((float)(Width - firstKnob.KnobThickness / ScrollScaleWidth) / (maximum - minimum));
                int diff = (int)(eX - dragOffset.X);

                // Clamp the movement delta
                int moveDelta = diff + draggingTimelineRange.X;

                if (moveDelta < 1)
                    moveDelta = 1;
                if (moveDelta + draggingTimelineRange.Y > maximum)
                    moveDelta = maximum - draggingTimelineRange.Y;

                // Change the range
                if (moveDelta != temp)
                {
                    temp = moveDelta;

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
                    if (RangeChanged != null)
                        RangeChanged.Invoke(this, new RangeChangedEventArgs(GetRange()));
                }
            }
            // The user is dragging the view
            else if (draggingView)
            {
                float lastScroll = ScrollX;

                ScrollX = -(e.X - dragOffset.X);

                if (lastScroll != ScrollX)
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
                float eX = e.X + dragOffset.X + (int)ScrollX;

                // Calculate new value:
                float newValue = minimum + (float)Math.Round((float)(eX - drag.KnobThickness / 2) / ((Width * ScrollScaleWidth) - drag.KnobThickness - 1) * (maximum - minimum));

                // Whether to redraw:
                bool redraw = false;

                // Check for redrawing. Should only redraw when the new value is different from the last value in the knob:
                if (newValue != drag.Value)
                {
                    redraw = true;
                }

                // Set the last X position of the knob. Used to set the redraw rectangle.
                float lastX = drag.ScaledX;

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
                    float x = Math.Min(drag.ScaledX, lastX) - drag.KnobThickness;
                    float width = Math.Max(drag.ScaledX, lastX) - x + drag.KnobThickness * 2;

                    // Set the redraw rectangle:
                    Invalidate(new Rectangle((int)x, 0, (int)width, timelineHeight + drag.KnobHeigth + 1 + (int)drag.DrawOffset.Y));

                    // Show the tooltip:
                    ToolTip.Show("" + (drag.Value), this, (int)(drag.ScaledX), -25, 1000);

                    if (RangeChanged != null)
                        RangeChanged.Invoke(this, new RangeChangedEventArgs(GetRange()));
                }

                // Set the knob mouse over setting:
                drag.MouseOver = true;
            }
            else
            {
                if (knobsEnabled)
                {
                    // Get the distance between the mouse and the knobs:
                    float fx = Math.Abs(e.X - firstKnob.ScaledX - firstKnob.KnobThickness / 2);
                    float sx = Math.Abs(e.X - secondKnob.ScaledX - secondKnob.KnobThickness / 2);
                    bool overY = (behaviorType == TimelineBehaviorType.RangeSelector || e.Y > timelineHeight + 2);

                    // I tried optimizing this bit as much as I could, and right now, it behaves pretty fast:
                    if (fx < sx)
                    {
                        // If the mouse is near enough:
                        if (fx <= firstKnob.KnobThickness / 2 && overY)
                        {
                            // Show the tooltip:
                            if (!firstKnob.MouseOver)
                                ToolTip.Show("" + (firstKnob.Value), this, (int)firstKnob.ScaledX, -25, 1000);

                            // Change the state of the knobs:
                            firstKnob.MouseOver = true;
                            secondKnob.MouseOver = false;

                            // Set to redraw:
                            redrawKnobs = true;

                            // Set this knob to draw over the other knob:
                            drawLast = firstKnob;

                            if (mouseOverTimeline)
                                Invalidate();

                            // Reset the mouse over timeline flag
                            mouseOverTimeline = false;
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
                            ToolTip.Hide(this);
                        }
                    }
                    // Second knob check:
                    else
                    {
                        // If the mouse is near enough:
                        if (sx <= secondKnob.KnobThickness / 2 && overY)
                        {
                            // Show the tooltip:
                            if (!secondKnob.MouseOver)
                                ToolTip.Show("" + (secondKnob.Value), this, (int)secondKnob.ScaledX, -25, 1000);

                            // Change the state of the knobs:
                            firstKnob.MouseOver = false;
                            secondKnob.MouseOver = true;

                            // Set to redraw:
                            redrawKnobs = true;

                            // Set this knob to draw over the other knob:
                            drawLast = secondKnob;

                            if (mouseOverTimeline)
                                Invalidate();

                            // Reset the mouse over timeline flag
                            mouseOverTimeline = false;
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
                            ToolTip.Hide(this);
                        }
                    }
                }

                if (rangeEnabled && !firstKnob.MouseOver && !secondKnob.MouseOver)
                {
                    if (e.X > Math.Min(firstKnob.ScaledX, secondKnob.ScaledX) && e.X < Math.Max(firstKnob.ScaledX, secondKnob.ScaledX) && e.Y < timelineHeight)
                    {
                        mouseOverTimeline = true;
                        Invalidate();
                    }
                    else
                    {
                        if (mouseOverTimeline)
                        {
                            mouseOverTimeline = false;
                            Invalidate();
                        }
                    }
                }
            }

            // If set to redraw:
            if (redrawKnobs)
            {
                // Redraw the knobs:
                Invalidate(new Rectangle((int)firstKnob.ScaledX, timelineHeight - 3, (int)(firstKnob.KnobThickness * 2), timelineHeight * 2 + (int)firstKnob.DrawOffset.Y));
                Invalidate(new Rectangle((int)secondKnob.ScaledX, timelineHeight - 3, (int)(secondKnob.KnobThickness * 2), timelineHeight * 2 + (int)secondKnob.DrawOffset.Y));
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
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (!Enabled)
                return;

            // Redraw the knobs:
            if (firstKnob.MouseOver)
                Invalidate(new Rectangle((int)firstKnob.X - firstKnob.KnobThickness - (int)ScrollX, timelineHeight - 3, (int)firstKnob.X + firstKnob.KnobThickness, timelineHeight * 2));
            if (secondKnob.MouseOver)
                Invalidate(new Rectangle((int)secondKnob.X - secondKnob.KnobThickness - (int)ScrollX, timelineHeight - 3, (int)secondKnob.X + secondKnob.KnobThickness, timelineHeight * 2));

            // Set both knobs' MouseOver property to false:
            firstKnob.MouseOver = false;
            secondKnob.MouseOver = false;

            if (mouseOverTimeline)
            {
                mouseOverTimeline = false;
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
            if ((e.X > 0 && e.X < Width) &&
                (e.Y > 0 && e.Y < Height))
            {
                float oldX = (e.X * ((float)Width / (Width - firstKnob.KnobThickness)) - firstKnob.KnobThickness / 2) * ScrollScaleWidth;

                ScrollScaleWidth += (float)e.Delta / 120 / 3;

                if (ScrollScaleWidth < 1f / 3)
                {
                    ScrollScaleWidth = 1f / 3;
                }
                else if (ScrollScaleWidth > 30f)
                {
                    ScrollScaleWidth = 30f;
                }

                float newX = (e.X * ((float)Width / (Width - firstKnob.KnobThickness)) - firstKnob.KnobThickness / 2) * ScrollScaleWidth;

                ScrollX += (newX - oldX);

                Invalidate();

                return;
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
            if (lastWidth != 0)
            {
                ScrollX /= ((float)lastWidth / Width);
            }

            // Set the knobs to calculate new X properties:
            firstKnob.Update();
            secondKnob.Update();

            // Update the lastWidth variable
            lastWidth = Width;
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
        /// Changes the current frame being displayed
        /// </summary>
        /// <param name="oldFrame">The new frame to display</param>
        protected void ChangeFrame(int newFrame)
        {
            if (currentFrame == newFrame)
                return;

            int oldFrame = currentFrame;

            if (FrameChanged != null)
            {
                FrameChangedEventArgs evArgs = new FrameChangedEventArgs(oldFrame, newFrame);

                FrameChanged.Invoke(this, evArgs);

                if (evArgs.Cancel)
                    return;
            }

            CurrentFrame = newFrame;
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
        /// Gets the frame currently under the mouse pointer.
        /// If a range is currently visible to the user, the value is truncated
        /// between [MinimumRange - MaximumRange] incluse, if not, the value is
        /// truncated between [Minimum - Maximum] inclusive
        /// </summary>
        /// <param name="clipOnRange">Whether to clip on the current range if available</param>
        /// <returns>The frame under the mouse pointer</returns>
        protected int GetFrameUnderMouse(bool clipOnRange = true)
        {
            float totalWidth = ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 2);
            float mx = (this.PointToClient(MousePosition).X + ScrollX - firstKnob.KnobThickness / 2) / totalWidth;

	        int f = Math.Max(minimum, Math.Min(maximum, minimum + (int)Math.Round(mx * (maximum - minimum))));

            if (clipOnRange && behaviorType == TimelineBehaviorType.RangeSelector)
            {
                Point range = GetRange();
                f = Math.Max(range.X, Math.Min(range.X + range.Y, f));
            }
            else if (behaviorType == TimelineBehaviorType.TimelineWithRange && disableFrameSelectionOutOfRange)
            {
                Point range = GetRange();
                f = Math.Max(range.X, Math.Min(range.X + range.Y, f));
            }

            return f;
        }

        /// <summary>
        /// Returns whether the mouse is currently over the timeline
        /// </summary>
        /// <returns>Whether the mouse is currently over the timeline</returns>
        protected bool IsMouseOnTimeline()
        {
            float x = firstKnob.KnobThickness / 2 - ScrollX;
            float w = ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 2);

            int mx = this.PointToClient(MousePosition).X;
            int my = this.PointToClient(MousePosition).Y;

            return mx > x && mx < x + w && my < timelineHeight;
        }

        /// <summary>
        /// Invalidates the playhead position
        /// </summary>
        protected void InvalidatePlayhead()
        {
            Graphics g = this.CreateGraphics();

            string frameText = currentFrame + "";
            SizeF frameTextSize = g.MeasureString(frameText, this.font);

            Invalidate(new Rectangle((firstKnob.KnobThickness / 2) + (int)((float)(currentFrame - minimum) / Math.Max(1, maximum - minimum) * ((Width * ScrollScaleWidth) - firstKnob.KnobThickness - 1)) - (int)ScrollX - (int)frameTextSize.Width / 2 - 1, 0, (int)frameTextSize.Width + 2, timelineHeight + 1));

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
        protected int minimum = 0;

        /// <summary>
        /// The maximum value this TimelineControl will display
        /// </summary>
        protected int maximum = 0;

        /// <summary>
        /// The current frame being displayed
        /// </summary>
        protected int currentFrame;
        /// <summary>
        /// The knob to draw the last
        /// </summary>
        protected Knob drawLast;

        /// <summary>
        /// The mouse drag offset
        /// </summary>
        private PointF dragOffset = PointF.Empty;

        /// <summary>
        /// This control's last width before resizing
        /// </summary>
        private int lastWidth = 0;

        /// <summary>
        /// The pre-calculated start label width
        /// </summary>
        private float minSize = 0;
        /// <summary>
        /// The pre-calculated middle label width
        /// </summary>
        private float medSize = 0;
        /// <summary>
        /// The pre-calculated end label width
        /// </summary>
        private float maxSize = 0;

        /// <summary>
        /// Create the font object this control will use to draw the texts:
        /// </summary>
        private Font font = new System.Drawing.Font("Segoi UI", 8.1f);

        /// <summary>
        /// Temporary integer used in various calculations
        /// </summary>
        private int temp = 0;
    }

    /// <summary>
    /// Event arguments for the RangeChanged event
    /// </summary>
    public class RangeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The new range
        /// </summary>
        public Point NewRange { get; private set; }

        /// <summary>
        /// Initializes a new instance of the RangeChangedEventArgs class
        /// </summary>
        /// <param name="newRange">The new range for the timeline selection</param>
        public RangeChangedEventArgs(Point newRange)
        {
            this.NewRange = newRange;
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
        public int NewFrame { get; private set; }

        /// <summary>
        /// The previous frame selected
        /// </summary>
        public int OldFrame { get; private set; }

        /// <summary>
        /// Whether to cancel this event and not modify the frame
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Initializes a new instance of the FrameChangedEventArgs class
        /// </summary>
        /// <param name="oldFrame">The previous frame selected</param>
        /// <param name="oldFrame">The new frame selected</param>
        public FrameChangedEventArgs(int oldFrame, int newFrame)
        {
            this.OldFrame = oldFrame;
            this.NewFrame = newFrame;
        }
    }

    /// <summary>
    /// Represents a TimelineControl knob
    /// </summary>
    public class Knob
    {
        /// <summary>
        /// This knob's tick offset
        /// </summary>
        protected int offset;
        /// <summary>
        /// This knob'x value
        /// </summary>
        protected int value = 0;
        /// <summary>
        /// The parent TimelineControl that's hosting this knob
        /// </summary>
        protected TimelineControl parent;
        /// <summary>
        /// This knob's X position
        /// </summary>
        protected float x;
        /// <summary>
        /// This knob's X scale
        /// </summary>
        protected float scaledX;
        /// <summary>
        /// This knob's drawing offset
        /// </summary>
        protected PointF drawOffset;

        /// <summary>
        /// Gets or sets this knob's tick offset
        /// </summary>
        public int Offset
        {
            get { return offset; }
            set { offset = Math.Max(0, Math.Min(parent.Maximum - parent.Minimum, value)); }
        }

        /// <summary>
        /// Gets or sets this knob'x value
        /// </summary>
        public int Value
        {
            get { return this.value; }
            set { this.value = Math.Max(parent.Minimum, Math.Min(parent.Maximum, value)); Update(); }
        }

        /// <summary>
        /// Gets or sets the parent TimelineControl that's hosting this knob
        /// </summary>
        public TimelineControl Parent
        {
            get { return parent; }
            protected set { this.parent = value; }
        }

        /// <summary>
        /// Gets or sets this knob's X position
        /// </summary>
        public float X
        {
            get { return x; }
            set { this.x = X; }
        }

        /// <summary>
        /// Gets this knob's scaled X component based on the parent TimelineControl's size
        /// </summary>
        public float ScaledX
        {
            get { return (int)GetRealX(); }
        }

        /// <summary>
        /// Gets or sets this knob's drawing offset
        /// </summary>
        public PointF DrawOffset
        {
            get { return drawOffset; }
            set
            {
                if (drawOffset != value)
                {
                    drawOffset = value;
                }
            }
        }

        /// <summary>
        /// Thickness of this knob, in pixels
        /// </summary>
        public int KnobThickness = 10;

        /// <summary>
        /// The knob height on screen, in pixels
        /// </summary>
        public int KnobHeigth = 7;

        /// <summary>
        /// Whether the mouse is hovering over this knob
        /// </summary>
        public bool MouseOver = false;

        /// <summary>
        /// Initializes a new instance of the Knob control, binding it to a TimelineControl
        /// </summary>
        /// <param name="Parent">A TimelineControl to bind to this KNob</param>
        public Knob(TimelineControl Parent)
        {
            this.parent = Parent;

            this.value = 0;

            this.drawOffset = new PointF(0, 4);
        }

        /// <summary>
        /// Draws this knob into a graphics object
        /// </summary>
        /// <param name="e">The graphics to draw this knob on</param>
        public void Draw(Graphics e)
        {
            PointF drawOffset = new PointF(this.drawOffset.X + ScaledX, this.drawOffset.Y + parent.TimelineHeight);

            if (b != null)
                b.Dispose();

            // Fill it with a gray color if the parent is disabled
            if (!parent.Enabled)
            {
                b = new SolidBrush(Color.LightGray);
            }
            // If the mouse is over the knob, fill it with a lighter color:
            else if (MouseOver)
            {
                b = new LinearGradientBrush(drawOffset, new Point(KnobHeigth * 2 + (int)drawOffset.X, KnobThickness + (int)drawOffset.Y), Color.LightGray, Color.DarkGray);
            }
            else
            {
                b = new LinearGradientBrush(drawOffset, new Point(KnobHeigth * 2 + (int)drawOffset.X, KnobThickness + (int)drawOffset.Y), Color.LightGray, Color.Gray);
            }

            path.Reset();

            // Fill in the lines array:
            lines[0] = new PointF(0, 0);
            lines[1] = new PointF(KnobThickness / 2, -3);
            lines[2] = new PointF(KnobThickness, 0);
            lines[3] = new PointF(KnobThickness, KnobHeigth);
            lines[4] = new PointF(0, KnobHeigth);
            lines[5] = lines[0];

            // Offset all the lines:
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].X = (int)(lines[i].X + drawOffset.X);
                lines[i].Y = (int)(lines[i].Y + drawOffset.Y);
            }

            // Add the lines to the path:
            path.AddLines(lines);

            // Fill the path:
            e.FillPath(b, path);

            // Draw the outline, and the line marker:
            e.DrawPath(Pens.Black, path);
        }

        /// <summary>
        /// Draws this knob's indicator
        /// </summary>
        /// <param name="e">The graphics to draw the indicator on</param>
        public void DrawIndicator(Graphics e)
        {
            PointF drawOffset = new PointF(this.drawOffset.X + ScaledX, this.drawOffset.Y + parent.TimelineHeight);

            // Draw the indicator:
            e.DrawLine(Pens.Black, KnobThickness / 2 + (int)drawOffset.X, -3 + (int)drawOffset.Y, KnobThickness / 2 + (int)drawOffset.X, -(int)drawOffset.Y);
        }

        /// <summary>
        /// Update the positioning of this knob
        /// </summary>
        public void Update()
        {
            // Calculate the new position:
            this.x = (KnobThickness / 2) + ((float)this.value / Math.Max(parent.Maximum - parent.Minimum, 1)) * (parent.Width - KnobThickness - 1);

            this.scaledX = GetRealX();
        }

        /// <summary>
        /// Gets the real X component of this knob
        /// </summary>
        /// <returns>The real X component of this knob, adjusted for the parent TimelineControl's scale</returns>
        public float GetRealX()
        {
            return ((float)(value - parent.Minimum) / Math.Max(parent.Maximum - parent.Minimum, 1)) * ((parent.Width * parent.ScrollScaleWidth) - KnobThickness - 1) - parent.ScrollX;
        }

        /// <summary>
        /// Linear gradient brush, used to draw the knob: 
        /// </summary>
        private Brush b;
        /// <summary>
        /// Create the graphics path used to fill out the knob:
        /// </summary>
        private GraphicsPath path = new GraphicsPath();
        /// <summary>
        /// Create a bunch of lines used to fill out the knob:
        /// </summary>
        private PointF[] lines = new PointF[6];
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

namespace Pixelaria.Views.ModelViews
{
    partial class TimelineTestView
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            PixelariaLib.Timeline.TimelineController timelineController1 = new PixelariaLib.Timeline.TimelineController();
            this.tc_timeline = new Pixelaria.Views.Controls.TimelineControl();
            this.SuspendLayout();
            // 
            // tc_timeline
            // 
            this.tc_timeline.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tc_timeline.Location = new System.Drawing.Point(0, 283);
            this.tc_timeline.Name = "tc_timeline";
            this.tc_timeline.Size = new System.Drawing.Size(914, 299);
            this.tc_timeline.TabIndex = 0;
            this.tc_timeline.Text = "timelineControl1";
            this.tc_timeline.TimelineController = timelineController1;
            // 
            // TimelineTestView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 582);
            this.Controls.Add(this.tc_timeline);
            this.Name = "TimelineTestView";
            this.Text = "TimelineTestView";
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.TimelineControl tc_timeline;
    }
}
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PixelariaLib.Timeline;

namespace Pixelaria.Views.ModelViews
{
    public partial class TimelineTestView : Form
    {
        public TimelineTestView()
        {
            InitializeComponent();

            SetupTimeline();
        }

        private void SetupTimeline()
        {
            var layerController = new EmptyTimelineLayerController();

            tc_timeline.TimelineController.AddLayer(layerController);
            tc_timeline.TimelineController.AddKeyframe(0, 0);
            tc_timeline.TimelineController.AddKeyframe(15, 0);
        }
    }
}

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

            tc_timeline.ShowExtendedTimeline = true;

            tc_timeline.TimelineController.AddLayer("layer 1", layerController);
            tc_timeline.TimelineController.AddLayer("layer 2", layerController);
            tc_timeline.TimelineController.AddLayer("layer 3", layerController);
            tc_timeline.TimelineController.AddLayer("layer 4", layerController);
            tc_timeline.TimelineController.AddLayer("layer 5", layerController);
            tc_timeline.TimelineController.AddLayer("layer 6", layerController);
            tc_timeline.TimelineController.AddLayer("layer 7", layerController);
            tc_timeline.TimelineController.AddLayer("layer 8", layerController);
            tc_timeline.TimelineController.AddLayer("layer 9", layerController);
            tc_timeline.TimelineController.AddLayer("layer 10", layerController);
            tc_timeline.TimelineController.AddLayer("layer 11", layerController);
            tc_timeline.TimelineController.AddLayer("layer 12", layerController);
            tc_timeline.TimelineController.AddKeyframe(0, 5, 0);
            tc_timeline.TimelineController.AddKeyframe(15, 0);

            tc_timeline.TimelineController.AddKeyframe(20, 2, 1);
            tc_timeline.TimelineController.AddKeyframe(50, 1);

            tc_timeline.TimelineController.AddKeyframe(5, 3);
            tc_timeline.TimelineController.AddKeyframe(10, 3);
        }
    }
}

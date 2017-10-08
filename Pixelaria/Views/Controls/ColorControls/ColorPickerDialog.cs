using System;
using System.Windows.Forms;
using Pixelaria.Utils;

namespace Pixelaria.Views.Controls.ColorControls
{
    public partial class ColorPickerDialog : Form
    {
        public AhslColor SelectedColor { get; set; }

        public ColorPickerDialog()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            colorPicker1.FirstAhslColor = SelectedColor;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            SelectedColor = colorPicker1.FirstAhslColor;

            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}

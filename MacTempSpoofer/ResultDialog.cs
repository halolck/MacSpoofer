using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MacTempSpoofer
{
    public partial class ResultDialog : Form
    {
        public ResultDialog(string text)
        {
            int h = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
            int w = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            InitializeComponent();
            this.Location = new Point(w - (this.Width + 30),(this.Height -50));
            label1.Text = text;
            SleepEnd();
        }

        private async void SleepEnd()
        {

        }

        private void ResultDialog_Load(object sender, EventArgs e)
        {

        }

        private void ResultDialog_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();

        }
    }
}

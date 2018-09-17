using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EngooSchedule
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            ScheduleManagement.Instance.Start(null);
            textBox1.Text = "服務已啟動";
            btnStart.Enabled = false;
            btnStop.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            ScheduleManagement.Instance.Stop();
            textBox1.Text = "服務已停止";
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EngooSchedule
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ScheduleManagement.Instance.Start(args);
        }

        protected override void OnStop()
        {
            ScheduleManagement.Instance.Stop();
        }

        public void Start(string[] args) {
            this.OnStart(args);
        }

        public void Stop() {
            this.OnStop();
        }
    }
}

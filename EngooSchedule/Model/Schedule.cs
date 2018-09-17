using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EngooSchedule.Model
{
    public class Schedule
    {
        public string lesson_id { get; set; }
        public string lesson_date { get; set; }
        public string scheduled_start_time { get; set; }
        public string status { get; set; }
        public bool time_passed { get; set; }
        public bool locked_lesson { get; set; }

    }
}

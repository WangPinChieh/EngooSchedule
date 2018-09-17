using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace EngooSchedule.Model
{
    public class SystemVars
    {
        private static SystemVars m_SystemVars;
        private static Configs m_Configs;
        public static Configs Configs
        {
            get
            {
                if (m_Configs == null)
                {
                    m_Configs = new Configs();
                    m_Configs.TeachersID = ConfigurationManager.AppSettings["TeacherIDs"].ToString().Split('|').ToList();
                    m_Configs.RefreshMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["RefreshMinutes"]);
                    m_Configs.EngooUrl = ConfigurationManager.AppSettings["EngooUrl"];
                }

                return m_Configs;
            }
        }        
    }
    public class Configs {
        public List<string> TeachersID { get; set; }
        public int RefreshMinutes { get; set; }
        public string EngooUrl { get; set; }
    }
}

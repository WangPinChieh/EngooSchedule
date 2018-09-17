using EngooSchedule.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EngooSchedule
{
    public class ScheduleManagement
    {
        private static Thread m_ScanThread;
        private static Thread m_SendEmailThread;
        private static ScheduleManagement m_Instance;
        private static object m_CountersLock = new object();
        private static object m_NotificationMessagesLock = new object();
        private static Dictionary<object, int> m_Counters;
        private static Dictionary<object, List<Schedule>> m_NotificationMessages;
        private static DateTime m_CurrentDate;

        public static ScheduleManagement Instance
        {
            get
            {
                if (m_Instance == null) {
                    m_Instance = new ScheduleManagement();
                }
                return m_Instance;
            }
        }

        public void Start(string[] args)
        {
            m_Counters = new Dictionary<object, int>();
            m_NotificationMessages = new Dictionary<object, List<Schedule>>();
            m_ScanThread = new Thread(doScanSchedule);
            m_SendEmailThread = new Thread(doSendEmail);
            m_ScanThread.Start();
            m_SendEmailThread.Start();
        }
        public void Stop()
        {
            if (m_ScanThread != null)
            {
                m_ScanThread.Abort();
                m_ScanThread = null;
            }
            if (m_SendEmailThread != null)
            {
                m_SendEmailThread.Abort();
                m_SendEmailThread = null;
            }
        }

        private void doScanSchedule()
        {
            try
            {
                List<string> _TeachersID = SystemVars.Configs.TeachersID;
                if (_TeachersID != null && _TeachersID.Count > 0)
                {
                    while (true)
                    {
                        if (m_CurrentDate != DateTime.Now.Date)
                        {
                            m_Counters = new Dictionary<object, int>();
                            m_NotificationMessages = new Dictionary<object, List<Schedule>>();
                            m_CurrentDate = DateTime.Now.Date;
                        }
                        foreach (string teacherID in _TeachersID)
                        {
                            object _DictionaryKey = new { Date = DateTime.Now.Date.ToString("yyyy/MM/dd"), TeacherID = teacherID };
                            lock (m_CountersLock)
                            {
                                if (m_Counters != null && m_Counters.ContainsKey(_DictionaryKey) && m_Counters[_DictionaryKey] == 3)
                                    continue;
                            }

                            string _Url = string.Format("{0}{1}.json", SystemVars.Configs.EngooUrl, teacherID);
                            HttpWebRequest _Request = (HttpWebRequest)WebRequest.Create(_Url);

                            using (HttpWebResponse _Response = (HttpWebResponse)_Request.GetResponse())
                            {
                                using (Stream _Stream = _Response.GetResponseStream())
                                {
                                    using (StreamReader _StreamReader = new StreamReader(_Stream))
                                    {
                                        string _JsonResult = _StreamReader.ReadToEnd();
                                        dynamic _Result = JsonConvert.DeserializeObject(_JsonResult);
                                        if (_Result != null && _Result.schedules != null && _Result.schedules.result != null)
                                        {
                                            string _SerializedSchedule = JsonConvert.SerializeObject(_Result.schedules.result);
                                            List<Schedule> _Schedules = JsonConvert.DeserializeObject<List<Schedule>>(_SerializedSchedule);
                                            if (_Schedules != null && _Schedules.Count > 0)
                                            {
                                                lock (m_NotificationMessagesLock)
                                                {
                                                    if (!m_NotificationMessages.ContainsKey(_DictionaryKey))
                                                    {
                                                        m_NotificationMessages[_DictionaryKey] = _Schedules;
                                                        lock (m_CountersLock)
                                                        {
                                                            if (!m_Counters.ContainsKey(_DictionaryKey))
                                                                m_Counters[_DictionaryKey] = 1;
                                                            else
                                                                m_Counters[_DictionaryKey]++;
                                                        }
                                                    }
                                                }

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        Thread.Sleep(SystemVars.Configs.RefreshMinutes * 60 * 1000);
                    }

                }
                else
                    return;
            }
            catch (Exception exp) { }
        }

        private void doSendEmail()
        {
            try
            {
                while (true)
                {
                    lock (m_NotificationMessagesLock)
                    {
                        if (m_NotificationMessages.Count > 0)
                        {
                            foreach (dynamic key in m_NotificationMessages.Keys)
                            {
                                SmtpClient _SmtpClient = new SmtpClient();
                                _SmtpClient.Port = 587;
                                _SmtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                                _SmtpClient.UseDefaultCredentials = false;
                                _SmtpClient.Host = "smtp.gmail.com";
                                _SmtpClient.Credentials = new NetworkCredential("engooschedule@gmail.com", "apex8500");
                                _SmtpClient.Timeout = 10000;
                                _SmtpClient.EnableSsl = true;
                                MailAddress _From = new MailAddress("engooschedule@gmail.com", "EngooSchedule");
                                MailMessage _MailMessage = new MailMessage();
                                _MailMessage.From = _From;
                                _MailMessage.To.Add(new MailAddress("pp8101313@hotmail.com"));
                                _MailMessage.Subject = string.Format("{0}-{1} 課程表已更新", key.Date, key.TeacherID);
                                _MailMessage.Body = ConvertToHtml(m_NotificationMessages[key]);
                                _MailMessage.BodyEncoding = UTF8Encoding.UTF8;
                                _MailMessage.IsBodyHtml = true;
                                _MailMessage.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                                _SmtpClient.Send(_MailMessage);
                                Thread.Sleep(2000);
                            }

                            m_NotificationMessages = new Dictionary<object, List<Schedule>>();
                        }
                    }
                    Thread.Sleep(5000);
                }
            }
            catch (Exception exp) { }
        }

        private string ConvertToHtml(List<Schedule> schedules)
        {
            if (schedules != null && schedules.Count > 0)
            {
                string _Html = string.Empty;
                _Html += "<table><tr><td>課程時間</td><td>是否可預約</td><td>是否超過時間</td></tr>";
                foreach (Schedule schedule in schedules)
                {
                    _Html += "<tr>";
                    _Html += string.Format("<td>{0}</td>", schedule.lesson_date + " " + schedule.scheduled_start_time);
                    if (schedule.status == "1")
                        _Html += string.Format("<td style='color: red;'>X</td>");
                    else
                        _Html += string.Format("<td style='color: green;'>V</td>");

                    if (schedule.time_passed)
                        _Html += string.Format("<td style='color: red;'>X</td>");
                    else
                        _Html += string.Format("<td style='color: green;'>V</td>");

                    _Html += "</tr>";
                }
                _Html += "</table>";

                return _Html;
            }
            else
                return string.Empty;
        }

    }
}

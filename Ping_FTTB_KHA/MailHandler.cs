using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using S22.Imap;
using HtmlAgilityPack;
using System.IO;
using System.Threading;
using System.Xml;

namespace Ping_FTTB_KHA
{
    class MailHandler : IMailFinderIMAP, IMailSenderSMTP
    {
        //default IP
        public string IP { get; set; } = "10.203.150.1";
        public string TempBodyForSend { get; set; } = "Непрочитанных писем не обнаружено. Это запрос по default <b>ping_kha 10.203.150.1</b><br>";
        public int CountUnseenMails { get; set; } = 0;

        public string HostSMTP
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                return xElem.FirstChild.Attributes["HostSMTP"].Value;
            }
            
        }
        public string LoginSMTP
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                return xElem.FirstChild.Attributes["LoginSMTP"].Value;
            }

        }
        public string HostIMAP
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                return xElem.FirstChild.Attributes["HostIMAP"].Value;
            }

        }

        public string LoginIMAP {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                return xElem.FirstChild.Attributes["Name"].Value;
            } 
        } 
        public string PasswordIMAP
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                return xElem.FirstChild.Attributes["MyKey"].Value;
            }
        }
        public int Port /*{ get; set; } = 993;*/
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                int port = 587;
                if (int.TryParse(xElem.FirstChild.Attributes["Port"].Value, out port))
                {
                    return port;
                }
                return port;
            }
            set { }
        }
        public bool Ssl /*{ get; set; } = true;*/
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                bool ssl = false;
                if (bool.TryParse(xElem.FirstChild.Attributes["Ssl"].Value, out ssl))
                {
                    return ssl;
                }
                return ssl;
            }
            set { }
        }
        public MailAddress ToAnswer
        {
            get
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(Environment.CurrentDirectory + "/App_Data/userSetting.xml");
                XmlElement xElem = xDoc.DocumentElement;
                string to = xElem.FirstChild.Attributes["ToAnswer"].Value;
                return new MailAddress(to);
            }
            set { }
        }

    public MailAddress FindAddress()
        {
            for (int i = 0; i < 50; i++)
            {
                Console.Write(".");
                Thread.Sleep(150);
            }
            Console.WriteLine("");
            int count = 0;
            ImapClient client = new ImapClient(HostIMAP, Port, LoginIMAP, PasswordIMAP, AuthMethod.Login, Ssl);
            Console.WriteLine("Connected to server {0}!", HostIMAP);
            client.DefaultMailbox = "INBOX/Pinger";
            Console.WriteLine("Work directory is {0}", client.DefaultMailbox);

            //getting uids
            
            IEnumerable<uint> uids = client.Search(SearchCondition.Unseen());
            
                foreach (var item in uids)
                {
                ++count;
                }
            Console.WriteLine("Finded " + count + " mails!");

            //getting mails
            IEnumerable<MailMessage> messages = client.GetMessages(uids, FetchOptions.HeadersOnly);
            List<MailMessage> listMailMes;
            listMailMes = messages.ToList();
            if (listMailMes != null & listMailMes.Count >= 1)
            {
                CountUnseenMails = listMailMes.Count;
                    if (listMailMes[listMailMes.Count-1].Subject.Length > 4)
                    {
                            if (listMailMes[listMailMes.Count - 1].From.Address.Contains("@DOMAIN.net"))
                            { ToAnswer = listMailMes[listMailMes.Count - 1].From; }
                        TempBodyForSend = "<div style=\"font-size: 9pt;\">Информацию запросил " + listMailMes[listMailMes.Count - 1].From.DisplayName + " (" + listMailMes[listMailMes.Count - 1].From.Address + ")" + "<br>" + listMailMes[listMailMes.Count - 1].Subject + " " + "в " + listMailMes[listMailMes.Count - 1].Date().ToString() + "</div><br><br>";
                        Console.WriteLine(listMailMes[listMailMes.Count - 1].Subject + " from " + listMailMes[listMailMes.Count - 1].From);
                        int index = listMailMes[listMailMes.Count - 1].Subject.IndexOf("10.");
                        IP = listMailMes[listMailMes.Count - 1].Subject.Substring(index, listMailMes[listMailMes.Count - 1].Subject.Length - index).Trim();
                    }
 
                } 
            return ToAnswer;
        }

        public void SendMail(MailAddress to)
        {
            
            MailMessage message1 = new MailMessage();
            message1.Subject = "Ping response (" + IP + ") from " + DateTime.Now.ToString();
            message1.From = new MailAddress(LoginSMTP, "AutoPinger");
            message1.Body = TempBodyForSend;
            message1.IsBodyHtml = true;
            message1.BodyEncoding = Encoding.UTF8;
            message1.To.Add(to);
            SmtpClient client1 = new SmtpClient();
            client1.Host = HostSMTP;
            try
            {
                client1.Send(message1);
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.WriteLine("Mail hasn't send success :(");
            }
        }
    }
}

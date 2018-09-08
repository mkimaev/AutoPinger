using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using S22.Imap;
using HtmlAgilityPack;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml;

namespace Ping_FTTB_KHA
{
    class Program
    {
        static string ipForException = "";
        static bool isAvailableDataLinks = true;
        static string ColorText = "Green";
        //web source #1
        static string WebPortaGetInfo { get; set; } = "http://webportal.ks/src/util_info.php?getinfo&hide=true&ip=";

        static void Main(string[] args)
        {
            for (int i = 0; i < 50; i++)
            {
                Console.Write(".");
                Thread.Sleep(100);
            }
            Console.WriteLine();
            try
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Waiting for replacing mail to folder (Pinger)");
                MailHandler mailHandler = new MailHandler();
                //find all email data
                mailHandler.FindAddress();
                ipForException = mailHandler.IP;
                HtmlDocument html = new HtmlDocument();
                WebRequest request = WebRequest.Create(WebPortaGetInfo + mailHandler.IP);
                request.Credentials = new NetworkCredential(mailHandler.LoginIMAP, mailHandler.PasswordIMAP);
                WebResponse response;
                HtmlNode node;
                using ( response = request.GetResponse())
                {
                    if (response != null)
                    {
                        html.Load(response.GetResponseStream(), Encoding.UTF8); 
                    } 
                }
                
                //if node is not available
                if (!html.ParsedText.Contains("No response from remote host"))
                {
                    node = html.DocumentNode.SelectSingleNode("//table[3]");
                    if (node.OuterHtml != null)
                    {
                        mailHandler.TempBodyForSend += node.OuterHtml;
                        mailHandler.TempBodyForSend = mailHandler.TempBodyForSend.Replace("align=\"center\"", "style=\"font-size: 9pt\"");
                    }
                    
                }
                else
                {
                    isAvailableDataLinks = false;
                    mailHandler.TempBodyForSend += "Не получилось получиь данные по линкам!<br>";
                }

                //web source #2
                request = WebRequest.Create("http://webportal.ks/src/ping.php?gw=fttb-bridge&ip=" + mailHandler.IP);
                request.Credentials = new NetworkCredential(mailHandler.LoginIMAP, mailHandler.PasswordIMAP);
                using (response = request.GetResponse())
                {
                    if (response != null)
                    {
                        html.Load(response.GetResponseStream(), Encoding.UTF8);  
                    }
                }
                node = html.DocumentNode.SelectSingleNode("//html/body");
                if (node.OuterHtml != null & isAvailableDataLinks)
                { 
                mailHandler.TempBodyForSend += "<br>" + "<div style=\"color:" + ColorText +"; font-size: 9pt\">" + node.OuterHtml + "</div>";
                mailHandler.TempBodyForSend += "<br><br><span style=\"color:Gray; font-size: 8pt\">Scripted by Max Kimaev</span></div>";
                }
                else
                {
                    ColorText = "Red";
                    mailHandler.TempBodyForSend += "<br>" + "<div style=\"color:" + ColorText + "; font-size: 9pt\">" + node.OuterHtml + "</div>";
                }
                mailHandler.SendMail(mailHandler.ToAnswer);

                Thread.Sleep(4000);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Sended");
                for (int i = 0; i < 50; i++)
                {
                    Console.Write(".");
                    Thread.Sleep(100);
                }

            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("Was IP in request " + ipForException + "'\n"); ;

                StringBuilder sb = new StringBuilder();
                sb.Append(DateTime.Now.ToString() + "'\r");
                sb.Append(e.Message + "'\r");
                sb.Append(e.StackTrace + "'\r");
                sb.Append( "Was IP in request " + ipForException + "'\r");
                sb.Append("**************************************************************" + "'\n");
                using (StreamWriter sw = new StreamWriter(File.Open(Environment.CurrentDirectory + "/App_Data/logError.txt", FileMode.OpenOrCreate), Encoding.UTF8))
                {
                    sw.WriteLine(sb);
                }
                Console.ReadKey();
                
            }
        }
    }
}

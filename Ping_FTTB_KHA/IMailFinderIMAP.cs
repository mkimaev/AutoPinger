using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Ping_FTTB_KHA
{
    interface IMailFinderIMAP
    {
        string HostIMAP { get; }
        string LoginIMAP { get; }
        string PasswordIMAP { get; }
        int Port { get;  }
        bool Ssl { get; set; }
        MailAddress ToAnswer { get; set; }
        MailAddress FindAddress();
    }
}

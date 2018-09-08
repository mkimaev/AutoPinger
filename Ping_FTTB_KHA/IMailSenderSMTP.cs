using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;

namespace Ping_FTTB_KHA
{
    interface IMailSenderSMTP
    {
        string HostSMTP { get; }
        string LoginSMTP { get; }
        void SendMail(MailAddress to);

    }
}

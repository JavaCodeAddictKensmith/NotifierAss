using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tingt.UnsualSpendingNotifier.Models
{
    public class NotificationMessage
    {
        public string Subject { get; }
        public string Body { get; }

        public NotificationMessage(string subject, string body)
        {
            Subject = subject;
            Body = body;
        }
    }
}

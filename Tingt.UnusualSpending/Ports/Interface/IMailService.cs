using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;

namespace Tingt.UnusualSpending.Ports.Interface
{
    public interface IMailServices
    {
        public void SendEmail(EmailReceiver emailReceiver);
    }
}

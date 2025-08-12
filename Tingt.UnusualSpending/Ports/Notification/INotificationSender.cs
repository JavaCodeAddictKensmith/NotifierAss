using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;

namespace Tingt.UnusualSpending.Ports.Notification
{
    public interface INotificationSender
    {
        /// <summary>
        /// Sends the provided subject & body to the user's preferred channel.
        /// Implementations should throw an exception or return a failed Task for unrecoverable send errors.
        /// </summary>
        Task SendAsync(User user, string subject, string body);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Enums;
using Tingt.UnsualSpendingNotifier.Models;

namespace Tingt.UnsualSpendingNotifier.Services.Interface
{
    public interface INotificationSender
    {
        Task SendAsync(User user, NotificationMessage message, NotificationChannel channel);
    }
}

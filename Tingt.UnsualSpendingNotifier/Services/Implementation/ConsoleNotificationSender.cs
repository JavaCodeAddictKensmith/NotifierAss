using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Enums;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services.Interface;

namespace Tingt.UnsualSpendingNotifier.Services.Implementation
{

    public class ConsoleNotificationSender : INotificationSender
    {
        public Task SendAsync(User user, NotificationMessage message, NotificationChannel channel)
        {
            Console.WriteLine($"[Sending via {channel}] To {user.FullName}: {message.Subject}");
            Console.WriteLine(message.Body);
            Console.WriteLine(new string('-', 50));
            return Task.CompletedTask;
        }
    }
}

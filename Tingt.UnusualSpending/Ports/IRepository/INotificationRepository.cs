using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tingt.UnusualSpending.Ports.IRepository
{
    public interface INotificationHistoryRepository
    {
        Task<bool> HasNotificationBeenSentAsync(Guid userId, int year, int month);
        Task MarkNotificationAsSentAsync(Guid userId, int year, int month);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Services.@struct;

namespace Tingt.UnsualSpendingNotifier.Services.Interface
{
    public interface INotificationLogRepository
    {
        Task<bool> HasNotificationBeenSentAsync(Guid userId, YearMonth period);
        Task MarkNotificationSentAsync(Guid userId, YearMonth period);
    }
}

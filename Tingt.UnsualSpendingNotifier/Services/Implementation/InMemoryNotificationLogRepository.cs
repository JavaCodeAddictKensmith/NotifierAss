using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Services.Interface;
using Tingt.UnsualSpendingNotifier.Services.@struct;

namespace Tingt.UnsualSpendingNotifier.Services.Implementation
{

    public class InMemoryNotificationLogRepository : INotificationLogRepository
    {
        private readonly ConcurrentDictionary<(Guid, YearMonth), bool> _sent = new();

        public Task<bool> HasNotificationBeenSentAsync(Guid userId, YearMonth period)
        {
            return Task.FromResult(_sent.ContainsKey((userId, period)));
        }

        public Task MarkNotificationSentAsync(Guid userId, YearMonth period)
        {
            _sent.TryAdd((userId, period), true);
            return Task.CompletedTask;
        }
    }
   

}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Ports.IRepository;
using Tingt.UnusualSpending.Ports.Notification;
using Tingt.UnusualSpending.Infrastructure;

namespace Tingt.UnusualSpending.Infrastructure.InMemory
{
    public class InMemoryNotificationHistoryRepository : INotificationHistoryRepository
    {
        private readonly ConcurrentDictionary<string, bool> _sent = new();

        public Task<bool> HasNotificationBeenSentAsync(Guid userId, int year, int month)
        {
            var key = KeyFor(userId, year, month);
            return Task.FromResult(_sent.ContainsKey(key));
        }

        public Task MarkNotificationAsSentAsync(Guid userId, int year, int month)
        {
            var key = KeyFor(userId, year, month);
            _sent[key] = true;
            return Task.CompletedTask;
        }

        private string KeyFor(Guid userId, int year, int month) => $"{userId:N}-{year}-{month}";
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Ports.IRepository;

namespace Tingt.UnusualSpending.Infrastructure.InMemory
{
    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly Dictionary<Guid, List<Payment>> _paymentsByUser;
        public InMemoryPaymentRepository(Dictionary<Guid, List<Payment>> paymentsByUser) => _paymentsByUser = paymentsByUser;

        public Task<IEnumerable<Payment>> GetPaymentsForUserAsync(Guid userId, DateTime fromUtc, DateTime toUtc)
        {
            _paymentsByUser.TryGetValue(userId, out var list);
            list ??= new List<Payment>();
            var filtered = list.Where(p => p.TimestampUtc >= fromUtc && p.TimestampUtc <= toUtc);
            return Task.FromResult(filtered);
        }
    }
}

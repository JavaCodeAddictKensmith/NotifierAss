using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services.Interface;

namespace Tingt.UnsualSpendingNotifier.Services.Implementation
{

    public class InMemoryPaymentRepository : IPaymentRepository
    {
        private readonly List<Payment> _payments;

        public InMemoryPaymentRepository(IEnumerable<Payment>? initial = null)
        {
            _payments = (initial ?? Enumerable.Empty<Payment>()).ToList();
        }

        public Task<IEnumerable<Payment>> GetPaymentsAsync(Guid userId, DateTime fromInclusive, DateTime toExclusive)
        {
            var result = _payments
                .Where(p => p.UserId == userId && p.Timestamp >= fromInclusive && p.Timestamp < toExclusive);
            return Task.FromResult<IEnumerable<Payment>>(result);
        }
    }
   
}

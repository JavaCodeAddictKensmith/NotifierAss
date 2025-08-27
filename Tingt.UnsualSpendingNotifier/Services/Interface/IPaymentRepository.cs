using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Models;

namespace Tingt.UnsualSpendingNotifier.Services.Interface
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetPaymentsAsync(Guid userId, DateTime fromInclusive, DateTime toExclusive);
    }
}

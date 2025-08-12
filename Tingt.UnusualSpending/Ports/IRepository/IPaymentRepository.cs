using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;

namespace Tingt.UnusualSpending.Ports.IRepository
{
    public interface IPaymentRepository
    {
        /// <summary>
        /// Returns payments for a given user within [fromUtc, toUtc] inclusive
        /// </summary>
        Task<IEnumerable<Payment>> GetPaymentsForUserAsync(Guid userId, DateTime fromUtc, DateTime toUtc);
    }
}

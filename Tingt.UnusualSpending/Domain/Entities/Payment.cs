using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tingt.UnusualSpending.Domain.Entities
{
    using Enums;

    public sealed class Payment
    {
        public decimal Price { get; }
        public string Description { get; }
        public Category Category { get; }
        public DateTime TimestampUtc { get; }

        public Payment(decimal price, string description, Category category, DateTime timestampUtc)
        {
            if (price < 0) throw new ArgumentOutOfRangeException(nameof(price));
            Price = price;
            Description = description ?? string.Empty;
            Category = category;
            TimestampUtc = timestampUtc;
        }
    }
}

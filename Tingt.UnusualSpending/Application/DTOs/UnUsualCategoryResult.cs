using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Enums;

namespace Tingt.UnusualSpending.Application.DTOs
{
    public sealed record UnusualCategoryResult(Category Category, decimal Amount);
}

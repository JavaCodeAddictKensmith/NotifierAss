using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tingt.UnsualSpendingNotifier.Services.@struct
{

    public readonly struct YearMonth : IEquatable<YearMonth>
    {
        public int Year { get; }
        public int Month { get; }

        public YearMonth(int year, int month)
        {
            Year = year;
            Month = month;
        }

        public static YearMonth FromDate(DateTime date) => new YearMonth(date.Year, date.Month);

        public override string ToString() => $"{Year:D4}-{Month:D2}";

        public bool Equals(YearMonth other) => Year == other.Year && Month == other.Month;

        public override bool Equals(object? obj) => obj is YearMonth other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Year, Month);
    }
   
}

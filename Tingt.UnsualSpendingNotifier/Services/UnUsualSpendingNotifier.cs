using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Enums;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services.Interface;
using Tingt.UnsualSpendingNotifier.Services.@struct;

namespace Tingt.UnsualSpendingNotifier.Services
{



    public class UnusualSpendingNotifier
    {
        private readonly IUserRepository _userRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly INotificationSender _sender;
        private readonly INotificationLogRepository _logRepo;

        public UnusualSpendingNotifier(
            IUserRepository userRepo,
            IPaymentRepository paymentRepo,
            INotificationSender sender,
            INotificationLogRepository logRepo)
        {
            _userRepo = userRepo;
            _paymentRepo = paymentRepo;
            _sender = sender;
            _logRepo = logRepo;
        }

        /// <summary>
        /// Run the monthly process for the month that contains referenceDate (typically DateTime.UtcNow).
        /// </summary>
        public async Task NotifyAllAsync(DateTime referenceDate)
        {
            var users = await _userRepo.GetAllUsersAsync();
            var currentMonthStart = new DateTime(referenceDate.Year, referenceDate.Month, 1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart; // exclusive

            var period = new YearMonth(currentMonthStart.Year, currentMonthStart.Month);

            foreach (var user in users)
            {
                // idempotency
                if (await _logRepo.HasNotificationBeenSentAsync(user.Id, period))
                    continue;

                var currentPayments = await _paymentRepo.GetPaymentsAsync(user.Id, currentMonthStart, currentMonthStart.AddMonths(1));
                var previousPayments = await _paymentRepo.GetPaymentsAsync(user.Id, previousMonthStart, previousMonthEnd);

                var currTotals = currentPayments.GroupBy(p => p.Category)
                                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                var prevTotals = previousPayments.GroupBy(p => p.Category)
                                    .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

                // union of categories
                var categories = new HashSet<Category>(currTotals.Keys);
                categories.UnionWith(prevTotals.Keys);

                var unusual = new List<(Category Category, decimal PreviousAmount, decimal CurrentAmount)>();

                foreach (var cat in categories)
                {
                    prevTotals.TryGetValue(cat, out var prev);
                    currTotals.TryGetValue(cat, out var curr);

                    if (prev == 0m)
                    {
                        if (curr > 0m)
                        {
                            // Assumption: any new spending after zero last-month is considered unusual
                            unusual.Add((cat, prev, curr));
                        }
                    }
                    else
                    {
                        if (curr >= prev * 1.5m)
                        {
                            unusual.Add((cat, prev, curr));
                        }
                    }
                }

                if (!unusual.Any())
                    continue; // nothing to notify

                var totalUnusual = unusual.Sum(x => x.CurrentAmount);
                var subject = $"Unusual spending of ${Math.Round(totalUnusual, 0)} detected!";

                var bodyLines = new List<string>
                {
                    "Hello Tingtel user!",
                    "",
                    "We have detected unusually high spending on your account in these categories:",
                    ""
                };
                bodyLines.AddRange(unusual.Select(u => $"* You spent ${Math.Round(u.CurrentAmount, 0)} on {u.Category.ToString().ToLower()}"));
                bodyLines.Add("");
                bodyLines.Add("Love,");
                bodyLines.Add("Tingtel");

                var message = new NotificationMessage(subject, string.Join(Environment.NewLine, bodyLines));

                bool anySuccess = false;
                foreach (var channel in user.PreferredChannels)
                {
                    try
                    {
                        await _sender.SendAsync(user, message, channel);
                        anySuccess = true;
                    }
                    catch (Exception)
                    {
                        // In real app: log the failure, potentially record per channel. For this exercise we swallow,
                        // and proceed to attempt other channels.
                    }
                }

                if (anySuccess)
                {
                    await _logRepo.MarkNotificationSentAsync(user.Id, period);
                }
                // else: do not mark as sent (so reprocessing will retry).
            }
        }
    }
}

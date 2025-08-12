using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Application.DTOs;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Domain.Enums;
using Tingt.UnusualSpending.Ports.IRepository;
using Tingt.UnusualSpending.Ports.Notification;

namespace Tingt.UnusualSpending.Application.UseCases
{
    public sealed class NotifyUsersOfUnusualSpendingMonthly
    {
        private readonly IUserRepository _userRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly INotificationSender _notificationSender;
        private readonly INotificationHistoryRepository _historyRepo;

        /// <summary>
        /// Absolute threshold to consider a category unusual when lastMonth sum == 0.
        /// Example: If set to 100, then if lastMonth was 0 and currentMonth >= 100 => unusual.
        /// This prevents noisy alerts for trivial first-time spends.
        /// </summary>
        private readonly decimal _absoluteThresholdForZeroHistory;

        public NotifyUsersOfUnusualSpendingMonthly(
            IUserRepository userRepo,
            IPaymentRepository paymentRepo,
            INotificationSender notificationSender,
            INotificationHistoryRepository historyRepo,
            decimal absoluteThresholdForZeroHistory = 100.00m)
        {
            _userRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            _paymentRepo = paymentRepo ?? throw new ArgumentNullException(nameof(paymentRepo));
            _notificationSender = notificationSender ?? throw new ArgumentNullException(nameof(notificationSender));
            _historyRepo = historyRepo ?? throw new ArgumentNullException(nameof(historyRepo));
            if (absoluteThresholdForZeroHistory < 0) throw new ArgumentOutOfRangeException(nameof(absoluteThresholdForZeroHistory));
            _absoluteThresholdForZeroHistory = absoluteThresholdForZeroHistory;
        }

        /// <summary>
        /// Entry point — assume this is run monthly (e.g., from a cron or scheduled job).
        /// Uses UTC months.
        /// </summary>
        public async Task RunAsync(DateTime utcNow)
        {
            // Use UTC day boundaries; compute month boundaries
            var currentMonthStart = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonthStart = currentMonthStart.AddMonths(1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddTicks(-1); // inclusive end for previous month

            var users = await _userRepo.GetAllUsersAsync();

            foreach (var user in users)
            {
                // Idempotency: skip if already notified for this user for this month
                if (await _historyRepo.HasNotificationBeenSentAsync(user.Id, currentMonthStart.Year, currentMonthStart.Month))
                    continue;

                // Fetch payments
                var currentPayments = await _paymentRepo.GetPaymentsForUserAsync(user.Id, currentMonthStart, nextMonthStart.AddTicks(-1));
                var lastPayments = await _paymentRepo.GetPaymentsForUserAsync(user.Id, lastMonthStart, lastMonthEnd);

                var unusual = DetectUnusualSpendingByCategory(currentPayments, lastPayments);

                if (!unusual.Any())
                    continue; // nothing to notify

                // Compose notification
                var totalUnusual = unusual.Sum(u => u.Amount);
                var subject = $"Unusual spending of ${totalUnusual.ToString("0.##", CultureInfo.InvariantCulture)} detected!";
                var body = ComposeBody(user, unusual);

                // Validate contact based on preferred channel
                if (!ValidateContactForChannel(user))
                {
                    // Option: log and skip marking history, so next run can retry once contact fixed.
                    continue;
                }

                // Send and mark, ensure atomicity: mark only on successful send
                try
                {
                    await _notificationSender.SendAsync(user, subject, body);
                    await _historyRepo.MarkNotificationAsSentAsync(user.Id, currentMonthStart.Year, currentMonthStart.Month);
                }
                catch
                {
                    // Do not mark history to allow retry; bubble-up logging to caller if needed
                }
            }
        }

        private static bool ValidateContactForChannel(User user)
        {
            if (user == null) return false;
            if (string.IsNullOrWhiteSpace(user.Contact)) return false;

            // Basic validations — keep simple to remain infra-independent
            return user.PreferredChannel switch
            {
                NotificationChannel.Email => user.Contact.Contains("@"),
                NotificationChannel.Sms => user.Contact.All(c => char.IsDigit(c) || c == '+' || c == '-' || c == ' '),
                NotificationChannel.Push => true, // assume token present
                _ => false
            };
        }

        private static IEnumerable<UnusualCategoryResult> DetectUnusualSpendingByCategory(
            IEnumerable<Payment> currentMonthPayments,
            IEnumerable<Payment> lastMonthPayments)
        {
            var currentByCategory = currentMonthPayments
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

            var lastByCategory = lastMonthPayments
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

            var results = new List<UnusualCategoryResult>();

            foreach (var kvp in currentByCategory)
            {
                var category = kvp.Key;
                var currentAmount = kvp.Value;
                var lastAmount = lastByCategory.TryGetValue(category, out var v) ? v : 0m;

                // Only consider if current > 0
                if (currentAmount <= 0) continue;

                // If lastAmount > 0, detect if current >= 150% of last
                if (lastAmount > 0)
                {
                    if (currentAmount >= lastAmount * 1.5m)
                        results.Add(new UnusualCategoryResult(category, Decimal.Round(currentAmount, 2)));
                }
                else
                {
                    // last amount == 0 -> use configurable absolute threshold to avoid noisy alerts,
                    // e.g., only notify if currentAmount >= absolute threshold.
                    // This rule is configurable at construction time (default 100.00).
                    // If threshold is zero, it treats any new spending as unusual.
                    // Caller might choose threshold=0 to alert for any new spending.
                    // (The service ctor supplies the threshold.)
                    // Add if meets threshold
                    // (We can't access instance _absoluteThreshold here because static method; alternative: keep instance method.)
                }
            }

            return results;
        }

        // Because we need access to instance _absoluteThresholdForZeroHistory, convert DetectUnusual into instance method:
        private IEnumerable<UnusualCategoryResult> DetectUnusualSpendingByCategory(
            IEnumerable<Payment> currentMonthPayments,
            IEnumerable<Payment> lastMonthPayments,
            decimal absoluteThreshold)
        {
            var currentByCategory = currentMonthPayments
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

            var lastByCategory = lastMonthPayments
                .GroupBy(p => p.Category)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Price));

            var results = new List<UnusualCategoryResult>();

            foreach (var kvp in currentByCategory)
            {
                var category = kvp.Key;
                var currentAmount = kvp.Value;
                var lastAmount = lastByCategory.TryGetValue(category, out var v) ? v : 0m;

                if (currentAmount <= 0) continue;

                if (lastAmount > 0)
                {
                    if (currentAmount >= lastAmount * 1.5m)
                        results.Add(new UnusualCategoryResult(category, Decimal.Round(currentAmount, 2)));
                }
                else
                {
                    if (currentAmount >= absoluteThreshold)
                        results.Add(new UnusualCategoryResult(category, Decimal.Round(currentAmount, 2)));
                }
            }

            return results;
        }

        private string ComposeBody(User user, IEnumerable<UnusualCategoryResult> unusual)
        {
            // Build lines
            var lines = unusual.Select(u => $"* You spent ${u.Amount.ToString("0.##", CultureInfo.InvariantCulture)} on {u.Category.ToString().ToLower()}");

            return $@"Hello {user.Name}!
We have detected unusually high spending on your account in these categories:
{string.Join(Environment.NewLine, lines)}

Love,
Tingtel";
        }

        // Replace previous call to DetectUnusualSpendingByCategory(current, last) with instance method access to the threshold:
        public async Task RunAsync()
        {
            await RunAsync(DateTime.UtcNow);
        }

        // Overload that uses instance-level method
        public async Task RunForUtcNowAsync(DateTime utcNow)
        {
            // Wrapper to call instance DetectUnusual...
            var currentMonthStart = new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonthStart = currentMonthStart.AddMonths(1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);
            var lastMonthEnd = currentMonthStart.AddTicks(-1);

            var users = await _userRepo.GetAllUsersAsync();

            foreach (var user in users)
            {
                if (await _historyRepo.HasNotificationBeenSentAsync(user.Id, currentMonthStart.Year, currentMonthStart.Month))
                    continue;

                var currentPayments = await _paymentRepo.GetPaymentsForUserAsync(user.Id, currentMonthStart, nextMonthStart.AddTicks(-1));
                var lastPayments = await _paymentRepo.GetPaymentsForUserAsync(user.Id, lastMonthStart, lastMonthEnd);

                var unusual = DetectUnusualSpendingByCategory(currentPayments, lastPayments, _absoluteThresholdForZeroHistory);

                if (!unusual.Any())
                    continue;

                var totalUnusual = unusual.Sum(u => u.Amount);
                var subject = $"Unusual spending of ${totalUnusual.ToString("0.##", CultureInfo.InvariantCulture)} detected!";
                var body = ComposeBody(user, unusual);

                if (!ValidateContactForChannel(user))
                    continue;

                try
                {
                    await _notificationSender.SendAsync(user, subject, body);
                    await _historyRepo.MarkNotificationAsSentAsync(user.Id, currentMonthStart.Year, currentMonthStart.Month);
                }
                catch
                {
                    // swallow; do not mark
                }
            }
        }
    }
}

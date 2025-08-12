using Moq;
using Tingt.UnusualSpending.Application.UseCases;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Domain.Enums;
using Tingt.UnusualSpending.Ports.IRepository;
using Tingt.UnusualSpending.Ports.Notification;

namespace Tingtel.UnusualSpending.Tests
{
    public class NotifyUsersOfUnusualSpendingTests
    {
        [Fact]
        public async Task WhenUserHasUnusualSpending_ShouldSendNotification_AndMarkHistory()
        {
            var userId = Guid.NewGuid();
            var user = new User(userId, "Ting", NotificationChannel.Email, "user@example.com");

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new[] { user });

            var now = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);
            // last month: groceries 100 -> this month 160 => 60% rise (unusual)
            var payments = new Dictionary<Guid, List<Payment>>
            {
                [userId] = new List<Payment>
                {
                    new Payment(100m, "G last", Category.Groceries, now.AddMonths(-1).AddDays(1)),
                    new Payment(160m, "G this", Category.Groceries, now.AddDays(1))
                }
            };

            var paymentRepo = new Mock<IPaymentRepository>();
            paymentRepo.Setup(p => p.GetPaymentsForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                       .ReturnsAsync((Guid id, DateTime from, DateTime to) => payments[id].Where(x => x.TimestampUtc >= from && x.TimestampUtc <= to));

            var historyRepo = new Mock<INotificationHistoryRepository>();
            historyRepo.Setup(h => h.HasNotificationBeenSentAsync(userId, now.Year, now.Month)).ReturnsAsync(false);

            var sender = new Mock<INotificationSender>();
            sender.Setup(s => s.SendAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);

            var service = new NotifyUsersOfUnusualSpendingMonthly(
                userRepo.Object,
                paymentRepo.Object,
                sender.Object,
                historyRepo.Object,
                absoluteThresholdForZeroHistory: 50m);

            await service.RunForUtcNowAsync(now);

            sender.Verify(s => s.SendAsync(user, It.Is<string>(sub => sub.Contains("Unusual spending")), It.IsAny<string>()), Times.Once);
            historyRepo.Verify(h => h.MarkNotificationAsSentAsync(userId, now.Year, now.Month), Times.Once);
        }

        [Fact]
        public async Task WhenNoUnusualSpending_ShouldNotSend()
        {
            var userId = Guid.NewGuid();
            var user = new User(userId, "Ting", NotificationChannel.Email, "user@example.com");

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new[] { user });

            var now = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

            var payments = new Dictionary<Guid, List<Payment>>
            {
                [userId] = new List<Payment>
                {
                    new Payment(100m, "G last", Category.Groceries, now.AddMonths(-1).AddDays(1)),
                    new Payment(110m, "G this", Category.Groceries, now.AddDays(1)) // only 10% increase
                }
            };

            var paymentRepo = new Mock<IPaymentRepository>();
            paymentRepo.Setup(p => p.GetPaymentsForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                       .ReturnsAsync((Guid id, DateTime from, DateTime to) => payments[id].Where(x => x.TimestampUtc >= from && x.TimestampUtc <= to));

            var historyRepo = new Mock<INotificationHistoryRepository>();
            historyRepo.Setup(h => h.HasNotificationBeenSentAsync(userId, now.Year, now.Month)).ReturnsAsync(false);

            var sender = new Mock<INotificationSender>();

            var service = new NotifyUsersOfUnusualSpendingMonthly(
                userRepo.Object, paymentRepo.Object, sender.Object, historyRepo.Object, 50m);

            await service.RunForUtcNowAsync(now);

            sender.Verify(s => s.SendAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            historyRepo.Verify(h => h.MarkNotificationAsSentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task WhenLastMonthZero_AndCurrentAboveThreshold_ShouldSend()
        {
            var userId = Guid.NewGuid();
            var user = new User(userId, "Ting", NotificationChannel.Email, "user@example.com");

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new[] { user });

            var now = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

            var payments = new Dictionary<Guid, List<Payment>>
            {
                [userId] = new List<Payment>
                {
                    // Last month no groceries
                    new Payment(250m, "G this", Category.Groceries, now.AddDays(1)), // above threshold of 200
                }
            };

            var paymentRepo = new Mock<IPaymentRepository>();
            paymentRepo.Setup(p => p.GetPaymentsForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                       .ReturnsAsync((Guid id, DateTime from, DateTime to) => payments[id].Where(x => x.TimestampUtc >= from && x.TimestampUtc <= to));

            var historyRepo = new Mock<INotificationHistoryRepository>();
            historyRepo.Setup(h => h.HasNotificationBeenSentAsync(userId, now.Year, now.Month)).ReturnsAsync(false);

            var sender = new Mock<INotificationSender>();

            // set threshold to 200
            var service = new NotifyUsersOfUnusualSpendingMonthly(
                userRepo.Object, paymentRepo.Object, sender.Object, historyRepo.Object, absoluteThresholdForZeroHistory: 200m);

            await service.RunForUtcNowAsync(now);

            sender.Verify(s => s.SendAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task WhenContactMissing_ShouldNotSend_AndNotMarkHistory()
        {
            var userId = Guid.NewGuid();
            // missing contact
            var user = new User(userId, "Ting", NotificationChannel.Email, "");

            var userRepo = new Mock<IUserRepository>();
            userRepo.Setup(r => r.GetAllUsersAsync()).ReturnsAsync(new[] { user });

            var now = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

            var payments = new Dictionary<Guid, List<Payment>>
            {
                [userId] = new List<Payment>
                {
                    new Payment(100m, "G last", Category.Groceries, now.AddMonths(-1).AddDays(1)),
                    new Payment(160m, "G this", Category.Groceries, now.AddDays(1))
                }
            };

            var paymentRepo = new Mock<IPaymentRepository>();
            paymentRepo.Setup(p => p.GetPaymentsForUserAsync(userId, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                       .ReturnsAsync((Guid id, DateTime from, DateTime to) => payments[id].Where(x => x.TimestampUtc >= from && x.TimestampUtc <= to));

            var historyRepo = new Mock<INotificationHistoryRepository>();
            historyRepo.Setup(h => h.HasNotificationBeenSentAsync(userId, now.Year, now.Month)).ReturnsAsync(false);

            var sender = new Mock<INotificationSender>();

            var service = new NotifyUsersOfUnusualSpendingMonthly(
                userRepo.Object,
                paymentRepo.Object,
                sender.Object,
                historyRepo.Object,
                absoluteThresholdForZeroHistory: 50m);

            await service.RunForUtcNowAsync(now);

            sender.Verify(s => s.SendAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            historyRepo.Verify(h => h.MarkNotificationAsSentAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}
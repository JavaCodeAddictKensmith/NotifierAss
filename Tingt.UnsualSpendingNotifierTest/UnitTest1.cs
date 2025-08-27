using Tingt.UnsualSpendingNotifier.Enums;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services;
using Tingt.UnsualSpendingNotifier.Services.Implementation;
using Tingt.UnsualSpendingNotifier.Services.Interface;
using Tingt.UnsualSpendingNotifier.Services.@struct;

namespace Tingt.UnsualSpendingNotifierTest
{
    //public class UnitTest1
    //{
    //    [Fact]
    //    public void Test1()
    //    {

    //    }
    //}

    public class UnusualSpendingNotifierTests
    {
        private class TestNotificationSender : INotificationSender
        {
            public List<(User User, NotificationMessage Message, NotificationChannel Channel)> Sent { get; } = new();

            public bool ThrowOnSend { get; set; }

            public Task SendAsync(User user, NotificationMessage message, NotificationChannel channel)
            {
                if (ThrowOnSend)
                {
                    throw new InvalidOperationException("Simulated send failure");
                }
                Sent.Add((user, message, channel));
                return Task.CompletedTask;
            }
        }

        private User CreateTestUser(string email = "test@example.com")
        {
            return new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Email = email,
                PhoneNumber = "12345",
                PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email }
            };
        }

        [Fact]
        public async Task NotifyAllAsync_WhenUnusualSpendingExists_SendsNotification()
        {
            // Arrange
            var user = CreateTestUser();
            var now = new DateTime(2025, 8, 1);
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);

            var payments = new List<Payment>
            {
                new Payment { UserId = user.Id, Category = Category.Groceries, Amount = 100m, Timestamp = lastMonthStart.AddDays(3) },
                new Payment { UserId = user.Id, Category = Category.Groceries, Amount = 220m, Timestamp = currentMonthStart.AddDays(5) } // +120% increase
            };

            var userRepo = new InMemoryUserRepository(new[] { user });
            var paymentRepo = new InMemoryPaymentRepository(payments);
            var logRepo = new InMemoryNotificationLogRepository();
            var sender = new TestNotificationSender();

            var notifier = new UnusualSpendingNotifier(userRepo, paymentRepo, sender, logRepo);

            // Act
            await notifier.NotifyAllAsync(now);

            // Assert
            Assert.Single(sender.Sent);
            Assert.Contains("groceries", sender.Sent[0].Message.Body.ToLower());
        }

        [Fact]
        public async Task NotifyAllAsync_WhenNoUnusualSpending_DoesNotSendNotification()
        {
            var user = CreateTestUser();
            var now = new DateTime(2025, 8, 1);
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);

            var payments = new List<Payment>
            {
                new Payment { UserId = user.Id, Category = Category.Groceries, Amount = 100m, Timestamp = lastMonthStart.AddDays(3) },
                new Payment { UserId = user.Id, Category = Category.Groceries, Amount = 120m, Timestamp = currentMonthStart.AddDays(5) } // only +20%
            };

            var userRepo = new InMemoryUserRepository(new[] { user });
            var paymentRepo = new InMemoryPaymentRepository(payments);
            var logRepo = new InMemoryNotificationLogRepository();
            var sender = new TestNotificationSender();

            var notifier = new UnusualSpendingNotifier(userRepo, paymentRepo, sender, logRepo);

            await notifier.NotifyAllAsync(now);

            Assert.Empty(sender.Sent);
        }

        [Fact]
        public async Task NotifyAllAsync_WhenNotificationFails_DoesNotMarkAsSent()
        {
            var user = CreateTestUser();
            var now = new DateTime(2025, 8, 1);
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);

            var payments = new List<Payment>
            {
                new Payment { UserId = user.Id, Category = Category.Travel, Amount = 0m, Timestamp = lastMonthStart.AddDays(3) },
                new Payment { UserId = user.Id, Category = Category.Travel, Amount = 100m, Timestamp = currentMonthStart.AddDays(5) } // new category spend
            };

            var userRepo = new InMemoryUserRepository(new[] { user });
            var paymentRepo = new InMemoryPaymentRepository(payments);
            var logRepo = new InMemoryNotificationLogRepository();
            var sender = new TestNotificationSender { ThrowOnSend = true };

            var notifier = new UnusualSpendingNotifier(userRepo, paymentRepo, sender, logRepo);

            await notifier.NotifyAllAsync(now);

            // Assert: nothing sent, and log should remain empty
            Assert.Empty(sender.Sent);
            Assert.False(await logRepo.HasNotificationBeenSentAsync(user.Id, new YearMonth(2025, 8)));
        }

        [Fact]
        public async Task NotifyAllAsync_WhenPreviousSpendZeroAndNowNonZero_FlagsAsUnusual()
        {
            var user = CreateTestUser();
            var now = new DateTime(2025, 8, 1);
            var currentMonthStart = new DateTime(now.Year, now.Month, 1);
            var lastMonthStart = currentMonthStart.AddMonths(-1);

            var payments = new List<Payment>
            {
                new Payment { UserId = user.Id, Category = Category.Travel, Amount = 0m, Timestamp = lastMonthStart.AddDays(3) },
                new Payment { UserId = user.Id, Category = Category.Travel, Amount = 200m, Timestamp = currentMonthStart.AddDays(10) }
            };

            var userRepo = new InMemoryUserRepository(new[] { user });
            var paymentRepo = new InMemoryPaymentRepository(payments);
            var logRepo = new InMemoryNotificationLogRepository();
            var sender = new TestNotificationSender();

            var notifier = new UnusualSpendingNotifier(userRepo, paymentRepo, sender, logRepo);

            await notifier.NotifyAllAsync(now);

            Assert.Single(sender.Sent);
            Assert.Contains("travel", sender.Sent[0].Message.Body.ToLower());
        }
    }
}
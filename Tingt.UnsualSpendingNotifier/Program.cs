using System.Net;
using System.Net.Mail;
using Tingt.UnsualSpendingNotifier.Enums;
using Tingt.UnsualSpendingNotifier.Infra;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services;
using Tingt.UnsualSpendingNotifier.Services.Implementation;
using Tingt.UnsualSpendingNotifier.Services.Interface;


class Program
{
    static async Task Main()
    {
        // Seed sample data
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            FullName = "Kensmith",
            Email = "kensmithonyeoma@gmail.com",
            PhoneNumber = "07088561077",
            PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email/* NotificationChannel.Sms*/ }
        };
        //var user2 = new User
        //{
        //    Id = Guid.NewGuid(),
        //    FullName = "kenneth Ajah",
        //    Email = "ajahkenneth2018@gmail.com",
        //    PhoneNumber = "08010000001",
        //    PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email /*NotificationChannel.Sms*/ }
        //};
        //var user3 = new User
        //{
        //    Id = Guid.NewGuid(),
        //    FullName = "kenneth Ajah",
        //    Email = "kennethajah.u@gmail.com",
        //    PhoneNumber = "08010000001",
        //    PreferredChannels = new List<NotificationChannel> { NotificationChannel.Email /*NotificationChannel.Sms*/ }
        //};
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = currentMonthStart.AddMonths(-1);

        var payments = new List<Payment>
        {
            // last month - groceries 100
            new Payment { UserId = user1.Id, Amount = 100m, Category = Category.Groceries, Timestamp = lastMonthStart.AddDays(5)},
            // current month - groceries 220 (>=50% increase)
            new Payment { UserId = user1.Id, Amount = 220m, Category = Category.Groceries, Timestamp = currentMonthStart.AddDays(2)},
            // current month - travel 50 (previous 0 -> flagged)
            new Payment { UserId = user1.Id, Amount = 50m, Category = Category.Travel, Timestamp = currentMonthStart.AddDays(7)},
            // last month - groceries 100
            //new Payment { UserId = user2.Id, Amount = 100m, Category = Category.Groceries, Timestamp = lastMonthStart.AddDays(5)},
            //// current month - groceries 220 (>=50% increase)
            //new Payment { UserId = user2.Id, Amount = 220m, Category = Category.Groceries, Timestamp = currentMonthStart.AddDays(2)},
            //// current month - travel 50 (previous 0 -> flagged)
            //new Payment { UserId = user2.Id, Amount = 50m, Category = Category.Travel, Timestamp = currentMonthStart.AddDays(7)},
            // last month - groceries 100
            //new Payment { UserId = user3.Id, Amount = 100m, Category = Category.Groceries, Timestamp = lastMonthStart.AddDays(5)},
            //// current month - groceries 220 (>=50% increase)
            //new Payment { UserId = user3.Id, Amount = 220m, Category = Category.Groceries, Timestamp = currentMonthStart.AddDays(2)},
            //// current month - travel 50 (previous 0 -> flagged)
            //new Payment { UserId = user3.Id, Amount = 50m, Category = Category.Travel, Timestamp = currentMonthStart.AddDays(7)}
        };

        var userRepo = new InMemoryUserRepository(new[] { user1
            
            //user2,
            //user3
        });
        var paymentRepo = new InMemoryPaymentRepository(payments);
        var logRepo = new InMemoryNotificationLogRepository();
        var sender = new DummyNotificationSender();

        var notifier = new UnusualSpendingNotifier(userRepo, paymentRepo, sender, logRepo);

        Console.WriteLine("Running unusual spending notifier...");
        await notifier.NotifyAllAsync(now);

        Console.WriteLine("Done.");

       
    }

}
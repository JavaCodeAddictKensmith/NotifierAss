// Seed mock users
using Tingt.UnusualSpending.Application.UseCases;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Domain.Enums;
using Tingt.UnusualSpending.Infrastructure;
using Tingt.UnusualSpending.Infrastructure.InMemory;

var users = new List<User>
{
    new User(Guid.NewGuid(), "kennethajah.u@gmail.com", NotificationChannel.Email,"kennethajah.u@gmail.com"),
    new User(Guid.NewGuid(), "kennethajah.u@gmail.com", NotificationChannel.Sms, "07088561077")
};

// Seed mock payments for users
var paymentsByUser = new Dictionary<Guid, List<Payment>>
{
    [users[0].Id] = new List<Payment>
    {
        new Payment(150, "Groceries at Walmart", Category.Groceries, DateTime.UtcNow.AddDays(-5)),
        new Payment(900, "Travel to Bahamas", Category.Travel, DateTime.UtcNow.AddDays(-10)),
        // Last month for comparison
        new Payment(50, "Groceries at Walmart", Category.Groceries, DateTime.UtcNow.AddMonths(-1).AddDays(-5)),
        new Payment(400, "Travel to Bahamas", Category.Travel, DateTime.UtcNow.AddMonths(-1).AddDays(-10))
    },
    [users[1].Id] = new List<Payment>
    {
        new Payment(300, "Dinner at Italian Place", Category.Restaurants, DateTime.UtcNow.AddDays(-3)),
        // Last month smaller spend
        new Payment(100, "Dinner at Italian Place", Category.Restaurants, DateTime.UtcNow.AddMonths(-1).AddDays(-3))
    }
};

// Set up repositories with seeded data
var userRepo = new InMemoryUserRepository(users);
var paymentRepo = new InMemoryPaymentRepository(paymentsByUser);
var historyRepo = new InMemoryNotificationHistoryRepository();
//var user = new User { Contact = "+15551234567" };
var sender = new ConsoleNotificationSender(
    accountSid: "YOUR_TWILIO_ACCOUNT_SID",
    authToken: "YOUR_TWILIO_AUTH_TOKEN",
    fromPhoneNumber: users[0].Contact,
     smtpHost: "smtp.gmail.com",     // Example: Gmail SMTP
    smtpPort: 587,
    smtpUser: "your-email@gmail.com",
    smtpPass: "your-email-password-or-app-password",
    fromEmail: "your-email@gmail.com"
// Your Twilio number
);
//new ConsoleNotificationSender();

// Use case service
var service = new NotifyUsersOfUnusualSpendingMonthly(userRepo, paymentRepo, sender, historyRepo);

// Run the process
service.RunAsync();

Console.WriteLine("Unusual spending detection process completed.");


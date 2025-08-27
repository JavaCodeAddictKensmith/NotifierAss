using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Enums;
using Tingt.UnsualSpendingNotifier.Models;
using Tingt.UnsualSpendingNotifier.Services.Implementation;
using Tingt.UnsualSpendingNotifier.Services.Interface;

namespace Tingt.UnsualSpendingNotifier.Infra
{
    /// <summary>
    /// Dummy sender that succeeds only when the user has the necessary contact detail for the channel.
    /// Throws InvalidOperationException when missing contact detail.
    /// </summary>
    /// 
    /// 
    /// 
    public class DummyNotificationSender : INotificationSender
    {
        private readonly EmailSettings _emailSettings;

        public DummyNotificationSender()
        {
            _emailSettings = new EmailSettings(); // Uses default values
        }

        public async Task SendAsync(User user, NotificationMessage message, NotificationChannel channel)
        {
            switch (channel)
            {
                case NotificationChannel.Email:
                    if (string.IsNullOrWhiteSpace(user.Email))
                        throw new InvalidOperationException("No email available for user.");

                    // Always log to console first
                    Console.WriteLine($"[EMAIL] To:{user.Email} Subject:{message.Subject}\n{message.Body}\n");

                    var sendMail = new SmtpNotificationSender(
                        _emailSettings.Host,
                        _emailSettings.Port,
                        _emailSettings.Email,
                        _emailSettings.Password,
                        _emailSettings.Email);

                    try
                    {
                        await sendMail.SendAsync(user, message, channel);
                        Console.WriteLine($"[SMTP SUCCESS] Email sent to {user.Email}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[SMTP ERROR] Could not send to {user.Email}: {ex.Message}");
                    }
                    break;

                case NotificationChannel.Sms:
                    if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                        throw new InvalidOperationException("No phone number available for user.");
                    Console.WriteLine($"[SMS] To:{user.PhoneNumber} {message.Subject}\n");
                    break;

                case NotificationChannel.Push:
                    if (string.IsNullOrWhiteSpace(user.DeviceId))
                        throw new InvalidOperationException("No device id available for user.");
                    Console.WriteLine($"[PUSH] To Device:{user.DeviceId} {message.Subject}\n");
                    break;

                default:
                    throw new NotSupportedException("Unknown channel");
            }
        }
    }


   

}

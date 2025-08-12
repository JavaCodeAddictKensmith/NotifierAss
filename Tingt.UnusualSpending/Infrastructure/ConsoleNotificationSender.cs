using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnusualSpending.Domain.Entities;
using Tingt.UnusualSpending.Domain.Enums;
using Tingt.UnusualSpending.Ports.Notification;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Tingt.UnusualSpending.Infrastructure
{
    public class ConsoleNotificationSender : INotificationSender
    {
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _fromPhoneNumber;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly string _fromEmail;

        public ConsoleNotificationSender(string accountSid, string authToken, string fromPhoneNumber, string smtpHost,
            int smtpPort,
            string smtpUser,
            string smtpPass,
            string fromEmail)
        {
            _twilioAccountSid = accountSid;
            _twilioAuthToken = authToken;
            _fromPhoneNumber = fromPhoneNumber;
            _smtpHost = smtpHost;
            _smtpPort = smtpPort;
            _smtpUser = smtpUser;
            _smtpPass = smtpPass;
            _fromEmail = fromEmail;

        }

        public Task SendAsync(User user, string subject, string body)
        {


            Console.WriteLine($"Sending [{user.PreferredChannel}] to {user.Contact}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine(body);
            Console.WriteLine("----");
            if (user.PreferredChannel == NotificationChannel.Sms && !string.IsNullOrWhiteSpace(user.Contact))
            {
                return SendSmsAsync(user.Contact, $"{subject}\n{body}");
            }

            else if (user.PreferredChannel == NotificationChannel.Email && !string.IsNullOrWhiteSpace(user.Contact))
            {
                return SendEmailAsync(user.Contact, subject, body);
            }
           
            return Task.CompletedTask;
        }

        private Task SendSmsAsync(string toPhoneNumber, string message)
        {
            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);

            var msg = MessageResource.Create(
                body: message,
                from: new Twilio.Types.PhoneNumber(_fromPhoneNumber),
                to: new Twilio.Types.PhoneNumber(toPhoneNumber)
            );

            Console.WriteLine($"[SMS] Sent to {toPhoneNumber} - SID: {msg.Sid}");
            return Task.CompletedTask;
        }



        private Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var smtp = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage(_fromEmail, toEmail, subject, body);

            smtp.Send(mail);
            Console.WriteLine($"[EMAIL] Sent to {toEmail} - Subject: {subject}");
            return Task.CompletedTask;
        }
    }
}

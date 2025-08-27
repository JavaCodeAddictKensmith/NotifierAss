/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;*/

    using global::Tingt.UnsualSpendingNotifier.Enums;
    using global::Tingt.UnsualSpendingNotifier.Models;
    using global::Tingt.UnsualSpendingNotifier.Services.Interface;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using Tingt.UnsualSpendingNotifier.Enums;
    using Tingt.UnsualSpendingNotifier.Models;
    using Tingt.UnsualSpendingNotifier.Services.Interface;
namespace Tingt.UnsualSpendingNotifier.Services.Implementation
{
        public class SmtpNotificationSender : INotificationSender
        {
            private readonly string _smtpHost;
            private readonly int _smtpPort;
            private readonly string _smtpUser;
            private readonly string _smtpPass;
            private readonly string _fromAddress;

            public SmtpNotificationSender(string smtpHost, int smtpPort, string smtpUser, string smtpPass, string fromAddress)
            {
                _smtpHost = smtpHost;
                _smtpPort = smtpPort;
                _smtpUser = smtpUser;
                _smtpPass = smtpPass;
                _fromAddress = fromAddress;
            }

            public async Task SendAsync(User user, NotificationMessage message, NotificationChannel channel)
            {
                if (channel != NotificationChannel.Email)
                    throw new NotSupportedException("Only email channel is supported by SmtpNotificationSender.");

                if (string.IsNullOrWhiteSpace(user.Email))
                    throw new InvalidOperationException("No email available for user.");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
                {
                    Credentials = new NetworkCredential(_smtpUser, _smtpPass),
                    EnableSsl = true
            };

            //var mailMessage = new MailMessage(_fromAddress, user.Email, message.Subject, message.Body);
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromAddress),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = false
            };
            mailMessage.To.Add(user.Email);



            await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }
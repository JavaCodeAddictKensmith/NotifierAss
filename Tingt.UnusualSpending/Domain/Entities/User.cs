using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tingt.UnusualSpending.Domain.Entities
{
    using Enums;

    public sealed class User
    {
        public Guid Id { get; }
        public string Name { get; }
        public NotificationChannel PreferredChannel { get; }
        public string? Contact { get; } // email or phone or device token depending on channel

        public User(Guid id, string name, NotificationChannel preferredChannel, string contact)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PreferredChannel = preferredChannel;
            Contact = contact ?? string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Enums;

namespace Tingt.UnsualSpendingNotifier.Models
{
    public class User
    {
        [Required(ErrorMessage = "User Id is required.")]
        public Guid Id { get; init; }

        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
        [MinLength(2, ErrorMessage = "Full name must be at least 2 characters long.")]
        public string FullName { get; init; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        [StringLength(16, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 16 digits.")]
        public string PhoneNumber { get; init; } = string.Empty;

        [MaxLength(100, ErrorMessage = "Device ID cannot exceed 100 characters.")]
        public string? DeviceId { get; init; }

        [Required(ErrorMessage = "Preferred channels are required.")]
        [MinLength(1, ErrorMessage = "At least one preferred channel must be selected.")]
        public List<NotificationChannel> PreferredChannels { get; init; } = new();
    }
    //public record NotificationMessage(string Subject, string Body);

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tingt.UnsualSpendingNotifier.Models
{
    public class EmailSettings
    {
        public string Email { get; set; } = "ajahkenneth2018@gmail.com"; // Default to empty string if not set
        public string Password { get; set; }= "ytxh epbd elwt isil"; // Default to empty string if not set
        public string Host { get; set; } = "smtp.gmail.com"; // Default to Gmail SMTP server
        public string DisplayName { get; set; } = "Unusualaccount"; // Default to empty string if not set
        public int Port { get; set; } = 587; // Default SMTP port for TLS
    }
}

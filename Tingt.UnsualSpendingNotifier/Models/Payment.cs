using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tingt.UnsualSpendingNotifier.Enums;

namespace Tingt.UnsualSpendingNotifier.Models
{
    public class Payment
    {
        [Required(ErrorMessage = "UserId is required.")]
        public Guid UserId { get; init; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; init; }

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? Description { get; init; }

        [Required(ErrorMessage = "Category is required.")]
        [EnumDataType(typeof(Category), ErrorMessage = "Invalid category.")]
        public Category Category { get; init; }

        [Required(ErrorMessage = "Timestamp is required.")]
        [DataType(DataType.DateTime)]
        //[CustomValidation(typeof(Payment)]
        public DateTime Timestamp { get; init; }

        
    }
}

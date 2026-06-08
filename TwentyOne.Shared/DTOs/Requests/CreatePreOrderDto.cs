using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class CreatePreOrderDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "bKash Transaction ID is required")]
        [MaxLength(100, ErrorMessage = "Transaction ID too long")]
        public string BkashTransactionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "bKash sender number is required")]
        [MaxLength(20, ErrorMessage = "Phone number too long")]
        public string BkashSenderNumber { get; set; } = string.Empty;

        [Required]
        public bool TermsAccepted { get; set; }

    }
}

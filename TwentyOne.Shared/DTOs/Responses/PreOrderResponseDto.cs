using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.Shared.DTOs.Responses
{
    public class PreOrderResponseDto
    {
        public object PreOrderDeposit;

        public int Id { get; set; }
        public string PreOrderNumber { get; set; } = string.Empty;
        public decimal DepositAmount { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal RemainingAmount { get; set; }
        public string BkashTransactionId { get; set; } = string.Empty;
        public string BkashSenderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public bool TermsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeadlineDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public bool IsDeadlinePassed => DeadlineDate.HasValue &&
            DateTime.UtcNow > DeadlineDate;
        public int DaysRemaining => DeadlineDate.HasValue
            ? Math.Max(0, (DeadlineDate.Value - DateTime.UtcNow).Days)
            : 0;
    }
}

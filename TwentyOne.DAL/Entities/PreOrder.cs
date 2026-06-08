using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TwentyOne.Shared.Enums;

namespace TwentyOne.DAL.Entities
{
    public class PreOrder
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string PreOrderNumber { get; set; } = string.Empty;

        public decimal DepositAmount { get; set; }
        public decimal ProductPrice { get; set; }
        public decimal RemainingAmount { get; set; }

        [MaxLength(100)]
        public string BkashTransactionId { get; set; } = string.Empty;

        [MaxLength(20)]
        public string BkashSenderNumber { get; set; } = string.Empty;

        public PreOrderStatus Status { get; set; } = PreOrderStatus.Pending;

        [MaxLength(500)]
        public string? AdminNotes { get; set; }

        public bool TermsAccepted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeadlineDate { get; set; }

        // Foreign Keys
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public Product Product { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}

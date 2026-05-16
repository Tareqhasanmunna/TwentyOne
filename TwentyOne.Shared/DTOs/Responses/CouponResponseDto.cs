using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.Shared.DTOs.Responses
{
    public class CouponResponseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string DiscountType { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MinimumOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageCount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryDate;
    }
}

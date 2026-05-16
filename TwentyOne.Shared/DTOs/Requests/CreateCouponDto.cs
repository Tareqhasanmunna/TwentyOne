using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TwentyOne.Shared.Enums;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class CreateCouponDto
    {
        [Required(ErrorMessage = "Coupon code is required")]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required]
        public DiscountType DiscountType { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }

        public decimal? MinimumOrderAmount { get; set; }

        public int? UsageLimit { get; set; }

        [Required(ErrorMessage = "Expiry date is required")]
        public DateTime ExpiryDate { get; set; }
    }
}

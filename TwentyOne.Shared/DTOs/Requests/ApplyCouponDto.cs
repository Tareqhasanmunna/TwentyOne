using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class ApplyCouponDto
    {
        [Required(ErrorMessage = "Coupon code is required")]
        public string Code { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal OrderTotal { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class CreateOrderDto
    {
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery address is required")]
        [MaxLength(500, ErrorMessage = "Address too long")]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Notes too long")]
        public string? Notes { get; set; }
        
        [MaxLength(50)]
        public string? CouponCode { get; set; }
        public decimal DeliveryCharge { get; set; } = 0;
        public decimal DiscountAmount { get; set; } = 0;

        [Required(ErrorMessage = "Order must have at least one item")]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}

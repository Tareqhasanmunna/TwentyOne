using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Delivery address is required")]
        [MaxLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public string? CouponCode { get; set; }

        [Required(ErrorMessage = "Order must have at least one item")]
        public List<CreateOrderItemDto> Items { get; set; } = new List<CreateOrderItemDto>();
    }
}

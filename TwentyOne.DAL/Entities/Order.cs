using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TwentyOne.Shared.Enums;

namespace TwentyOne.DAL.Entities
{
    public class Order
    {
        public int Id { get; set; }

        [MaxLength(50)]
        public string OrderNumber { get; set; } = string.Empty;

        [MaxLength(500)]
        public string DeliveryAddress { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        [MaxLength(50)]
        public string? CouponCode { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign Key
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}

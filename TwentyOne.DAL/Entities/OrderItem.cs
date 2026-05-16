using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.DAL.Entities
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }
        public int ProductId { get; set; }

        // Navigation properties
        public Order Order { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}

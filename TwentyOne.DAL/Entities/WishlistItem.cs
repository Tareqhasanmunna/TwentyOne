using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.DAL.Entities
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}

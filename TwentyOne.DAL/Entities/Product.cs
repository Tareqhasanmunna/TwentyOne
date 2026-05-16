using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.DAL.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        [MaxLength(20)]
        public string Scale { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Condition { get; set; }

        public int? ReleaseYear { get; set; }
        public bool IsLimitedEdition { get; set; } = false;
        public bool IsPreOrder { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}

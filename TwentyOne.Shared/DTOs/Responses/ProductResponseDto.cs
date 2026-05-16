using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.Shared.DTOs.Responses
{
    public class ProductResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool InStock => StockQuantity > 0;
        public string Scale { get; set; } = string.Empty;
        public int? ReleaseYear { get; set; }
        public bool IsLimitedEdition { get; set; }
        public bool IsPreOrder { get; set; }
        public bool IsArchived { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}

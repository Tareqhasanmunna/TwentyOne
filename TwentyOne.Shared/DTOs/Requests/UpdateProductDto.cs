using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 999999.99, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Scale is required")]
        [MaxLength(20)]
        public string Scale { get; set; } = string.Empty;

        public int? ReleaseYear { get; set; }

        public bool IsLimitedEdition { get; set; } = false;

        public bool IsPreOrder { get; set; } = false;

        public bool IsArchived { get; set; } = false;

        [Required(ErrorMessage = "Brand is required")]
        public int BrandId { get; set; }

        public List<string>? ImageUrls { get; set; }
    }
}

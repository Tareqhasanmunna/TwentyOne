using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product Name is required.")]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stock Quantity is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be a non-negative value.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Scale is required.")]
        [MaxLength(20)]
        public string Scale { get; set; }= string.Empty;

        public bool IsLimitedEdition { get; set; }
        public bool IsPreOrder { get; set; }

        [Required(ErrorMessage = "Brand is required.")]
        public int BrandId { get; set; }

        public List<string>? ImageUrls { get; set; }
        public int? ReleaseYear { get; set; }
    }
}

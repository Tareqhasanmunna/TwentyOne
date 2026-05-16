using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class ProductFilterDto
    {
        public string? Search { get; set; }
        public int? BrandId { get; set; }
        public string? Scale { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsLimitedEdition { get; set; }
        public bool? IsPreOrder { get; set; }
        public bool? InStockOnly { get; set; }
        public string SortBy { get; set; } = "newest";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}

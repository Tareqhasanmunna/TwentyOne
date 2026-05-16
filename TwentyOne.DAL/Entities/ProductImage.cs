using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.DAL.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
        public int DisplayOrder { get; set; } = 0;

        // Foreign Key
        public int ProductId { get; set; }

        // Navigation property
        public Product Product { get; set; } = null!;
    }
}

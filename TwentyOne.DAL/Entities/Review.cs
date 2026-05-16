using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.DAL.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign Keys
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }

        // Navigation properties
        public ApplicationUser User { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}

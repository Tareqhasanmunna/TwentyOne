using System;
using System.Collections.Generic;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class BrandResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
    }
}

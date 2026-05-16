using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class CreateBrandDto
    {
        [Required(ErrorMessage ="Brand name is required")]
        [MaxLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? LogoUrl { get; set; }
    }
}

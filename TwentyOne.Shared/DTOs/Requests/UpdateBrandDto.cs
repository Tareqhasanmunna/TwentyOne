using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class UpdateBrandDto
    {
        [Required(ErrorMessage = "Brand name is required")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(300)]
        public string? LogoUrl { get; set; }

        public bool IsActive { get; set; } = true;
    }
}

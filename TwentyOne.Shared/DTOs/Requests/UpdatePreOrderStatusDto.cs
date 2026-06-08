using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using TwentyOne.Shared.Enums;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class UpdatePreOrderStatusDto
    {
        [Required]
        public PreOrderStatus Status { get; set; }

        public string? AdminNotes { get; set; }
    }
}

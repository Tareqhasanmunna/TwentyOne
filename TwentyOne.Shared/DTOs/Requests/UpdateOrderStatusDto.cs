using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TwentyOne.Shared.Enums;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class UpdateOrderStatusDto
    {
        [Required]
        public OrderStatus Status { get; set; }
    }
}

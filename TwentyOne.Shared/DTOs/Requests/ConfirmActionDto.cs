using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class ConfirmActionDto
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

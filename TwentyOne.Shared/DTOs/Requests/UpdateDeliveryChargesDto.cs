
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TwentyOne.Shared.DTOs.Requests
{
    public class UpdateDeliveryChargesDto
    {
        [Required]
        [Range(0, 10000)]
        public int InsideDhaka { get; set; }

        [Required]
        [Range(0, 10000)]
        public int OutsideDhaka { get; set; }
    }
}

namespace TwentyOne.Shared.DTOs.Responses
{
    public class ApplyCouponResponseDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OriginalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string DiscountDescription { get; set; } = string.Empty;
    }
}

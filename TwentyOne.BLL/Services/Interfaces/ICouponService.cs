using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Interfaces
{
    public interface ICouponService
    {
        Task<ApiResponse<List<CouponResponseDto>>> GetAllAsync();
        Task<ApiResponse<CouponResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<CouponResponseDto>> CreateAsync(CreateCouponDto dto);
        Task<ApiResponse<string>> DeleteAsync(int id);
        Task<ApiResponse<string>> ToggleActiveAsync(int id);
        Task<ApiResponse<ApplyCouponResponseDto>> ApplyCouponAsync(ApplyCouponDto dto);
    }
}

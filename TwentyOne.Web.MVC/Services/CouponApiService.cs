using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class CouponApiService: BaseApiService
    {
        public CouponApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<List<CouponResponseDto>>?> GetAllAsync()
        {
            return await GetAsync<ApiResponse<List<CouponResponseDto>>>(
                "api/coupons");
        }

        public async Task<ApiResponse<CouponResponseDto>?> CreateAsync(
            CreateCouponDto dto)
        {
            return await PostAsync<ApiResponse<CouponResponseDto>>(
                "api/coupons", dto);
        }

        public async Task<ApiResponse<string>?> RemoveAsync(int id)
        {
            return await DeleteAsync<ApiResponse<string>>(
                $"api/coupons/{id}");
        }

        public async Task<ApiResponse<string>?> ToggleAsync(int id)
        {
            return await PostAsync<ApiResponse<string>>(
                $"api/coupons/{id}/toggle", new { });
        }

        public async Task<ApiResponse<ApplyCouponResponseDto>?> ApplyCouponAsync(
    ApplyCouponDto dto)
        {
            return await PostAsync<ApiResponse<ApplyCouponResponseDto>>(
                "api/coupons/apply", dto);
        }
    }
}

using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class PreOrderApiService: BaseApiService
    {
        public PreOrderApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<List<PreOrderResponseDto>>?>
            GetAllAsync()
        {
            return await GetAsync<ApiResponse<List<PreOrderResponseDto>>>(
                "api/preorders");
        }

        public async Task<ApiResponse<List<PreOrderResponseDto>>?>
            GetMyPreOrdersAsync()
        {
            return await GetAsync<ApiResponse<List<PreOrderResponseDto>>>(
                "api/preorders/my");
        }

        public async Task<ApiResponse<PreOrderResponseDto>?>
            GetByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<PreOrderResponseDto>>(
                $"api/preorders/{id}");
        }

        public async Task<ApiResponse<PreOrderResponseDto>?> CreateAsync(
            CreatePreOrderDto dto)
        {
            return await PostAsync<ApiResponse<PreOrderResponseDto>>(
                "api/preorders", dto);
        }

        public async Task<ApiResponse<PreOrderResponseDto>?>
            UpdateStatusAsync(int id, UpdatePreOrderStatusDto dto)
        {
            return await PutAsync<ApiResponse<PreOrderResponseDto>>(
                $"api/preorders/{id}/status", dto);
        }

        public async Task<ApiResponse<string>?> CancelAsync(int id)
        {
            return await PostAsync<ApiResponse<string>>(
                $"api/preorders/{id}/cancel", new { });
        }

        public async Task<ApiResponse<string>?> GetBkashNumberAsync()
        {
            return await GetAsync<ApiResponse<string>>(
                "api/preorders/bkash");
        }
    }
}

using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class OrderApiService: BaseApiService
    {
        public OrderApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<List<OrderResponseDto>>?> GetAllAsync()
        {
            return await GetAsync<ApiResponse<List<OrderResponseDto>>>(
                "api/orders");
        }

        public async Task<ApiResponse<OrderResponseDto>?> GetByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<OrderResponseDto>>(
                $"api/orders/{id}");
        }

        public async Task<ApiResponse<OrderResponseDto>?> UpdateStatusAsync(
            int id, UpdateOrderStatusDto dto)
        {
            return await PutAsync<ApiResponse<OrderResponseDto>>(
                $"api/orders/{id}/status", dto);
        }
    }
}

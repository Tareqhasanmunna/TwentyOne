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

        public async Task<ApiResponse<OrderResponseDto>?> UpdateStatusAsync(int id, UpdateOrderStatusDto dto)
        {
            return await PutAsync<ApiResponse<OrderResponseDto>>(
                $"api/orders/{id}/status", dto);
        }

        public async Task<ApiResponse<List<OrderResponseDto>>?> GetMyOrdersAsync()
        {
            return await GetAsync<ApiResponse<List<OrderResponseDto>>>(
                "api/orders/my-orders");
        }

        public async Task<ApiResponse<OrderResponseDto>?> PlaceOrderAsync(CreateOrderDto dto)
        {
            return await PostAsync<ApiResponse<OrderResponseDto>>(
                "api/orders", dto);
        }

        public async Task<ApiResponse<OrderResponseDto>?> TrackOrderAsync(string orderNumber)
        {
            // Default endpoint for regular orders (TW-)
            string endpoint = $"api/orders/track/{orderNumber}";

            // Check if it's a pre-order (PO-)
            if (!string.IsNullOrEmpty(orderNumber) && orderNumber.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
            {
                endpoint = $"api/preorders/track/{orderNumber}";

                //If backend returns a PreOrderResponseDto instead of OrderResponseDto, 
                // need to map it or change the generic type depending on your backend models.
            }

            return await GetPublicAsync<ApiResponse<OrderResponseDto>>(endpoint);
        }

        public async Task<byte[]?> DownloadOrderPdfAsync(int id)
        {
            AttachToken();
            var response = await _httpClient
                .GetAsync($"api/orders/{id}/pdf");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<byte[]?> DownloadPdfByOrderNumberAsync(
            string orderNumber)
        {
            var response = await _httpClient
                .GetAsync($"api/orders/pdf/{orderNumber}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsByteArrayAsync();
        }
    }
}

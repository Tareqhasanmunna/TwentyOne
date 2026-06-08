using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ApiResponse<List<OrderResponseDto>>> GetAllAsync();
        Task<ApiResponse<List<OrderResponseDto>>> GetMyOrdersAsync(string userId);
        Task<ApiResponse<OrderResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<OrderResponseDto>> PlaceOrderAsync(
            string? userId, CreateOrderDto dto);
        Task<ApiResponse<OrderResponseDto>> UpdateStatusAsync(
            int id, UpdateOrderStatusDto dto);
        Task<ApiResponse<string>> CancelOrderAsync(int id, string userId);
        Task<ApiResponse<OrderResponseDto>> GetByOrderNumberAsync(string orderNumber);
    }
}

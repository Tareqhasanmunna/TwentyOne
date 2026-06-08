using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Interfaces
{
    public interface IPreOrderService
    {
        Task<ApiResponse<List<PreOrderResponseDto>>> GetAllAsync();
        Task<ApiResponse<List<PreOrderResponseDto>>> GetMyPreOrdersAsync(
            string userId);
        Task<ApiResponse<PreOrderResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<PreOrderResponseDto>> CreateAsync(
            string userId, CreatePreOrderDto dto);
        Task<ApiResponse<PreOrderResponseDto>> UpdateStatusAsync(
            int id, UpdatePreOrderStatusDto dto);
        Task<ApiResponse<string>> CancelAsync(int id, string userId);
        Task<ApiResponse<string>> GetBkashNumberAsync();
    }
}

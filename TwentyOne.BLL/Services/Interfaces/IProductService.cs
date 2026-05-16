using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Interfaces
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResponseDto<ProductResponseDto>>> GetAllAsync(
            ProductFilterDto filter);
        Task<ApiResponse<ProductResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<ProductResponseDto>> GetBySlugAsync(string slug);
        Task<ApiResponse<ProductResponseDto>> CreateAsync(CreateProductDto dto);
        Task<ApiResponse<ProductResponseDto>> UpdateAsync(int id, UpdateProductDto dto);
        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}

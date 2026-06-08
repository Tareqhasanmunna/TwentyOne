using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Interfaces
{
    public interface IBrandService
    {
        Task<ApiResponse<List<BrandResponseDto>>> GetAllAsync();
        Task<ApiResponse<BrandResponseDto>> GetByIdAsync(int id);
        Task<ApiResponse<BrandResponseDto>> CreateAsync(CreateBrandDto dto);
        Task<ApiResponse<BrandResponseDto>> UpdateAsync(int id, UpdateBrandDto dto);
        Task<ApiResponse<string>> DeleteAsync(int id);
    }
}

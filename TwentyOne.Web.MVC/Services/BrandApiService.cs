using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class BrandApiService: BaseApiService
    {
        public BrandApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<List<BrandResponseDto>>?> GetAllAsync()
        {
            return await GetAsync<ApiResponse<List<BrandResponseDto>>>(
                "api/brands");
        }

        public async Task<ApiResponse<BrandResponseDto>?> GetByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<BrandResponseDto>>(
                $"api/brands/{id}");
        }

        public async Task<ApiResponse<BrandResponseDto>?> CreateAsync(
            CreateBrandDto dto)
        {
            return await PostAsync<ApiResponse<BrandResponseDto>>(
                "api/brands", dto);
        }

        public async Task<ApiResponse<BrandResponseDto>?> UpdateAsync(
            int id, UpdateBrandDto dto)
        {
            return await PutAsync<ApiResponse<BrandResponseDto>>(
                $"api/brands/{id}", dto);
        }

        public async Task<ApiResponse<string>?> RemoveAsync(int id)
        {
            return await DeleteAsync<ApiResponse<string>>(
                $"api/brands/{id}");
        }
    }
}

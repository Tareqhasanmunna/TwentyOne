using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.MVC.Services
{
    public class ProductApiService : BaseApiService
    {
        public ProductApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<ApiResponse<PagedResponseDto<ProductResponseDto>>?>
            GetAllAsync(ProductFilterDto filter)
        {
            var query = $"api/products?page={filter.Page}" +
                $"&pageSize={filter.PageSize}";

            if (!string.IsNullOrEmpty(filter.Search))
                query += $"&search={filter.Search}";

            if (filter.BrandId.HasValue)
                query += $"&brandId={filter.BrandId}";

            var result = await GetAsync<ApiResponse<PagedResponseDto<ProductResponseDto>>>(query);
            return result;
        }

        public async Task<ApiResponse<ProductResponseDto>?> GetByIdAsync(int id)
        {
            return await GetAsync<ApiResponse<ProductResponseDto>>(
                $"api/products/{id}");
        }

        public async Task<ApiResponse<ProductResponseDto>?> CreateAsync(
            CreateProductDto dto)
        {
            return await PostAsync<ApiResponse<ProductResponseDto>>(
                "api/products", dto);
        }

        public async Task<ApiResponse<ProductResponseDto>?> UpdateAsync(
            int id, UpdateProductDto dto)
        {
            return await PutAsync<ApiResponse<ProductResponseDto>>(
                $"api/products/{id}", dto);
        }

        public async Task<ApiResponse<string>?> RemoveAsync(int id)
        {
            return await DeleteAsync<ApiResponse<string>>(
                $"api/products/{id}");
        }
    }
}
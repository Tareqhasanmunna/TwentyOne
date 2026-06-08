using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.BLL.Helpers;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Implementations
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;
        private readonly CacheService _cache;
        private readonly SanitizationService _sanitizer;

        private const string CacheKey = "brands_all";

        public BrandService(IBrandRepository brandRepository, CacheService cache, SanitizationService sanitizer)
        {
            _brandRepository = brandRepository;
            _cache = cache;
            _sanitizer = sanitizer;
        }

        public async Task<ApiResponse<List<BrandResponseDto>>> GetAllAsync()
        {
            var cached = _cache.Get<List<BrandResponseDto>>(CacheKey);
            if (cached != null)
            {
                return ApiResponse<List<BrandResponseDto>>.SuccessResponse(cached);
            }
            
            var brands = await _brandRepository.GetAllAsync();

            var result = brands.Select(b => new BrandResponseDto
            {
                Id = b.Id,
                Name = b.Name,
                Description = b.Description,
                LogoUrl = b.LogoUrl,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                ProductCount = b.Products.Count
            }).ToList();

            _cache.Set(CacheKey, result);

            return ApiResponse<List<BrandResponseDto>>
                .SuccessResponse(result);
        }

        public async Task<ApiResponse<BrandResponseDto>> GetByIdAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);

            if (brand == null)
                return ApiResponse<BrandResponseDto>
                    .FailResponse("Brand not found");

            var result = new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                IsActive = brand.IsActive,
                CreatedAt = brand.CreatedAt,
                ProductCount = brand.Products.Count
            };

            return ApiResponse<BrandResponseDto>.SuccessResponse(result);
        }

        public async Task<ApiResponse<BrandResponseDto>> CreateAsync(CreateBrandDto dto)
        {
            // Sanitize inputs
            dto.Name = _sanitizer.SanitizePlainText(dto.Name);
            dto.Description = _sanitizer.Sanitize(dto.Description);

            // Check if brand name already exists
            var existing = await _brandRepository.GetByNameAsync(dto.Name);
            if (existing != null)
                return ApiResponse<BrandResponseDto>
                    .FailResponse("A brand with this name already exists");

            var brand = new Brand
            {
                Name = dto.Name,
                Description = dto.Description,
                LogoUrl = dto.LogoUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var created = await _brandRepository.CreateAsync(brand);

            _cache.Remove(CacheKey);

            var result = new BrandResponseDto
            {
                Id = created.Id,
                Name = created.Name,
                Description = created.Description,
                LogoUrl = created.LogoUrl,
                IsActive = created.IsActive,
                CreatedAt = created.CreatedAt,
                ProductCount = 0
            };

            return ApiResponse<BrandResponseDto>
                .SuccessResponse(result, "Brand created successfully");
        }

        public async Task<ApiResponse<BrandResponseDto>> UpdateAsync(
            int id, UpdateBrandDto dto)
        {
            // Sanitize inputs
            dto.Name = _sanitizer.SanitizePlainText(dto.Name);
            dto.Description = _sanitizer.Sanitize(dto.Description);

            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return ApiResponse<BrandResponseDto>
                    .FailResponse("Brand not found");

            // Check name conflict with other brands
            var existing = await _brandRepository.GetByNameAsync(dto.Name);
            if (existing != null && existing.Id != id)
                return ApiResponse<BrandResponseDto>
                    .FailResponse("A brand with this name already exists");

            brand.Name = dto.Name;
            brand.Description = dto.Description;
            brand.LogoUrl = dto.LogoUrl;
            brand.IsActive = dto.IsActive;

            var updated = await _brandRepository.UpdateAsync(brand);

            _cache.Remove(CacheKey);

            var result = new BrandResponseDto
            {
                Id = updated.Id,
                Name = updated.Name,
                Description = updated.Description,
                LogoUrl = updated.LogoUrl,
                IsActive = updated.IsActive,
                CreatedAt = updated.CreatedAt,
                ProductCount = updated.Products.Count
            };

            return ApiResponse<BrandResponseDto>
                .SuccessResponse(result, "Brand updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var brand = await _brandRepository.GetByIdAsync(id);
            if (brand == null)
                return ApiResponse<string>.FailResponse("Brand not found");

            // Prevent deletion if brand has products
            var hasProducts = await _brandRepository.HasProductsAsync(id);
            if (hasProducts)
                return ApiResponse<string>.FailResponse(
                    "Cannot delete brand that has products. " +
                    "Remove or reassign products first");

            await _brandRepository.DeleteAsync(brand);

            _cache.Remove(CacheKey);

            return ApiResponse<string>
                .SuccessResponse("Brand deleted successfully");
        }

    }
}

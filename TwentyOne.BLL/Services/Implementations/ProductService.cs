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
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;

        public ProductService(
            IProductRepository productRepository,
            IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
        }

        public async Task<ApiResponse<PagedResponseDto<ProductResponseDto>>>
            GetAllAsync(ProductFilterDto filter)
        {
            var (items, totalCount) = await _productRepository
                .GetAllAsync(filter);

            var result = new PagedResponseDto<ProductResponseDto>
            {
                Items = items.Select(MapToDto).ToList(),
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedResponseDto<ProductResponseDto>>
                .SuccessResponse(result);
        }

        public async Task<ApiResponse<ProductResponseDto>> GetByIdAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Product not found");

            return ApiResponse<ProductResponseDto>
                .SuccessResponse(MapToDto(product));
        }

        public async Task<ApiResponse<ProductResponseDto>> GetBySlugAsync(
            string slug)
        {
            var product = await _productRepository.GetBySlugAsync(slug);
            if (product == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Product not found");

            return ApiResponse<ProductResponseDto>
                .SuccessResponse(MapToDto(product));
        }

        public async Task<ApiResponse<ProductResponseDto>> CreateAsync(
            CreateProductDto dto)
        {
            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(dto.BrandId);
            if (brand == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Brand not found");

            // Generate unique slug
            var slug = SlugHelper.GenerateSlug(dto.Name);
            var originalSlug = slug;
            var counter = 1;

            while (await _productRepository.SlugExistsAsync(slug))
            {
                slug = $"{originalSlug}-{counter}";
                counter++;
            }

            var product = new Product
            {
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                Scale = dto.Scale,
                ReleaseYear = dto.ReleaseYear,
                IsLimitedEdition = dto.IsLimitedEdition,
                IsPreOrder = dto.IsPreOrder,
                BrandId = dto.BrandId,
                CreatedAt = DateTime.UtcNow,
                Images = dto.ImageUrls?.Select((url, index) =>
                    new ProductImage
                    {
                        ImageUrl = url,
                        IsPrimary = index == 0,
                        DisplayOrder = index
                    }).ToList() ?? new List<ProductImage>()
            };

            var created = await _productRepository.CreateAsync(product);

            return ApiResponse<ProductResponseDto>
                .SuccessResponse(MapToDto(created), "Product created successfully");
        }

        public async Task<ApiResponse<ProductResponseDto>> UpdateAsync(
            int id, UpdateProductDto dto)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Product not found");

            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(dto.BrandId);
            if (brand == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Brand not found");

            // Regenerate slug if name changed
            if (product.Name != dto.Name)
            {
                var slug = SlugHelper.GenerateSlug(dto.Name);
                var originalSlug = slug;
                var counter = 1;

                while (await _productRepository.SlugExistsAsync(slug) &&
                    slug != product.Slug)
                {
                    slug = $"{originalSlug}-{counter}";
                    counter++;
                }
                product.Slug = slug;
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.Scale = dto.Scale;
            product.ReleaseYear = dto.ReleaseYear;
            product.IsLimitedEdition = dto.IsLimitedEdition;
            product.IsPreOrder = dto.IsPreOrder;
            product.IsArchived = dto.IsArchived;
            product.BrandId = dto.BrandId;
            product.UpdatedAt = DateTime.UtcNow;

            // Update images
            if (dto.ImageUrls != null)
            {
                product.Images = dto.ImageUrls.Select((url, index) =>
                    new ProductImage
                    {
                        ImageUrl = url,
                        IsPrimary = index == 0,
                        DisplayOrder = index,
                        ProductId = product.Id
                    }).ToList();
            }

            var updated = await _productRepository.UpdateAsync(product);

            return ApiResponse<ProductResponseDto>
                .SuccessResponse(MapToDto(updated), "Product updated successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<string>.FailResponse("Product not found");

            await _productRepository.DeleteAsync(product);

            return ApiResponse<string>
                .SuccessResponse("Product deleted successfully");
        }

        // Private helper to map Product entity to DTO
        private static ProductResponseDto MapToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Scale = product.Scale,
                ReleaseYear = product.ReleaseYear,
                IsLimitedEdition = product.IsLimitedEdition,
                IsPreOrder = product.IsPreOrder,
                IsArchived = product.IsArchived,
                CreatedAt = product.CreatedAt,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
                ImageUrls = product.Images?
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList() ?? new List<string>()
            };
        }
    }
}

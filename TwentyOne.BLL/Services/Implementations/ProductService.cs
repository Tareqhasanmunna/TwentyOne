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
        private readonly SanitizationService _sanitizer;

        public ProductService(
            IProductRepository productRepository,
            SanitizationService sanitizer,
            IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _sanitizer = sanitizer;
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
            // Sanitize inputs
            dto.Name = _sanitizer.SanitizePlainText(dto.Name);
            dto.Description = _sanitizer.Sanitize(dto.Description);

            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(dto.BrandId);
            if (brand == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Brand not found");

            // If product is pre-order, ignore any discount inputs
            if (dto.IsPreOrder)
            {
                dto.DiscountPercentage = null;
                dto.DiscountAmount = null;
            }

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
                DiscountPercentage = dto.DiscountPercentage,
                DiscountAmount = dto.DiscountAmount,
                StockQuantity = dto.StockQuantity,
                Scale = dto.Scale,
                ReleaseYear = dto.ReleaseYear,
                IsLimitedEdition = dto.IsLimitedEdition,
                IsPreOrder = dto.IsPreOrder,
                PreOrderDeadline = dto.PreOrderDeadline,
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
            // Sanitize inputs
            dto.Name = _sanitizer.SanitizePlainText(dto.Name);
            dto.Description = _sanitizer.Sanitize(dto.Description);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Product not found");

            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(dto.BrandId);
            if (brand == null)
                return ApiResponse<ProductResponseDto>
                    .FailResponse("Brand not found");

            // If product is pre-order, ignore any discount inputs
            if (dto.IsPreOrder)
            {
                dto.DiscountPercentage = null;
                dto.DiscountAmount = null;
            }

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
            product.DiscountPercentage = dto.DiscountPercentage;
            product.DiscountAmount = dto.DiscountAmount;
            product.StockQuantity = dto.StockQuantity;
            product.Scale = dto.Scale;
            product.ReleaseYear = dto.ReleaseYear;
            product.IsLimitedEdition = dto.IsLimitedEdition;
            product.IsPreOrder = dto.IsPreOrder;
            product.PreOrderDeadline = dto.PreOrderDeadline;
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
            var deposit = product.Price >= 10000 ? 500 : 200;
            decimal discountedPrice = product.Price;
            string discountLabel = string.Empty;
            bool hasDiscount = false;

            if (product.DiscountPercentage > 0)
            {
                discountedPrice = product.Price -
                    (product.Price * product.DiscountPercentage.Value / 100);
                discountLabel = $"{product.DiscountPercentage}% OFF";
                hasDiscount = true;
            }
            else if (product.DiscountAmount > 0)
            {
                discountedPrice = product.Price - product.DiscountAmount.Value;
                discountLabel = $"Save ৳{product.DiscountAmount:N0}";
                hasDiscount = true;
            }
            if (discountedPrice < 0) discountedPrice = 0;

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                Price = product.Price,
                DiscountPercentage = product.DiscountPercentage,
                DiscountAmount = product.DiscountAmount,
                DiscountedPrice = discountedPrice,
                HasDiscount = hasDiscount,
                DiscountLabel = discountLabel,
                StockQuantity = product.StockQuantity,
                Scale = product.Scale,
                ReleaseYear = product.ReleaseYear,
                IsLimitedEdition = product.IsLimitedEdition,
                IsPreOrder = product.IsPreOrder,
                IsArchived = product.IsArchived,
                CreatedAt = product.CreatedAt,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
                PreOrderDeadline = product.PreOrderDeadline,
                PreOrderDeposit = deposit,
                ImageUrls = product.Images?
                    .OrderBy(i => i.DisplayOrder)
                    .Select(i => i.ImageUrl)
                    .ToList() ?? new List<string>()
            };
        }


    }
}

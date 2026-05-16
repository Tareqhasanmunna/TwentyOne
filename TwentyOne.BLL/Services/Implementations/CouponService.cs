using System;
using System.Collections.Generic;
using System.Text;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Enums;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Implementations
{
    public class CouponService: ICouponService
    {
        private readonly ICouponRepository _couponRepository;

        public CouponService(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<ApiResponse<List<CouponResponseDto>>> GetAllAsync()
        {
            var coupons = await _couponRepository.GetAllAsync();
            return ApiResponse<List<CouponResponseDto>>
                .SuccessResponse(coupons.Select(MapToDto).ToList());
        }

        public async Task<ApiResponse<CouponResponseDto>> GetByIdAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return ApiResponse<CouponResponseDto>
                    .FailResponse("Coupon not found");

            return ApiResponse<CouponResponseDto>
                .SuccessResponse(MapToDto(coupon));
        }

        public async Task<ApiResponse<CouponResponseDto>> CreateAsync(
            CreateCouponDto dto)
        {
            // Check if code already exists
            var existing = await _couponRepository.GetByCodeAsync(dto.Code);
            if (existing != null)
                return ApiResponse<CouponResponseDto>
                    .FailResponse("Coupon code already exists");

            // Validate percentage discount
            if (dto.DiscountType == DiscountType.Percentage &&
                dto.DiscountValue > 100)
                return ApiResponse<CouponResponseDto>
                    .FailResponse("Percentage discount cannot exceed 100%");

            var coupon = new Coupon
            {
                Code = dto.Code.ToUpper(),
                DiscountType = dto.DiscountType,
                DiscountValue = dto.DiscountValue,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                UsageLimit = dto.UsageLimit,
                UsageCount = 0,
                ExpiryDate = dto.ExpiryDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _couponRepository.CreateAsync(coupon);

            return ApiResponse<CouponResponseDto>
                .SuccessResponse(MapToDto(created), "Coupon created successfully");
        }

        public async Task<ApiResponse<string>> DeleteAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return ApiResponse<string>.FailResponse("Coupon not found");

            await _couponRepository.DeleteAsync(coupon);

            return ApiResponse<string>
                .SuccessResponse("Coupon deleted successfully");
        }

        public async Task<ApiResponse<string>> ToggleActiveAsync(int id)
        {
            var coupon = await _couponRepository.GetByIdAsync(id);
            if (coupon == null)
                return ApiResponse<string>.FailResponse("Coupon not found");

            coupon.IsActive = !coupon.IsActive;
            await _couponRepository.UpdateAsync(coupon);

            var status = coupon.IsActive ? "activated" : "deactivated";
            return ApiResponse<string>
                .SuccessResponse($"Coupon {status} successfully");
        }

        public async Task<ApiResponse<ApplyCouponResponseDto>> ApplyCouponAsync(
            ApplyCouponDto dto)
        {
            var coupon = await _couponRepository.GetByCodeAsync(dto.Code);

            // Validate coupon exists
            if (coupon == null)
                return ApiResponse<ApplyCouponResponseDto>
                    .FailResponse("Invalid coupon code");

            // Validate coupon is active
            if (!coupon.IsActive)
                return ApiResponse<ApplyCouponResponseDto>
                    .FailResponse("This coupon is no longer active");

            // Validate coupon is not expired
            if (DateTime.UtcNow > coupon.ExpiryDate)
                return ApiResponse<ApplyCouponResponseDto>
                    .FailResponse("This coupon has expired");

            // Validate usage limit
            if (coupon.UsageLimit.HasValue &&
                coupon.UsageCount >= coupon.UsageLimit)
                return ApiResponse<ApplyCouponResponseDto>
                    .FailResponse("This coupon has reached its usage limit");

            // Validate minimum order amount
            if (coupon.MinimumOrderAmount.HasValue &&
                dto.OrderTotal < coupon.MinimumOrderAmount)
                return ApiResponse<ApplyCouponResponseDto>
                    .FailResponse($"Minimum order amount for this coupon is " +
                        $"{coupon.MinimumOrderAmount:C}");

            // Calculate discount
            decimal discountAmount = 0;
            string description = string.Empty;

            if (coupon.DiscountType == DiscountType.Percentage)
            {
                discountAmount = dto.OrderTotal * (coupon.DiscountValue / 100);
                description = $"{coupon.DiscountValue}% off";
            }
            else
            {
                discountAmount = coupon.DiscountValue;
                description = $"{coupon.DiscountValue:C} off";

                // Discount cannot exceed order total
                if (discountAmount > dto.OrderTotal)
                    discountAmount = dto.OrderTotal;
            }

            var finalAmount = dto.OrderTotal - discountAmount;

            // Increment usage count
            coupon.UsageCount++;
            await _couponRepository.UpdateAsync(coupon);

            var result = new ApplyCouponResponseDto
            {
                Code = coupon.Code,
                OriginalAmount = dto.OrderTotal,
                DiscountAmount = discountAmount,
                FinalAmount = finalAmount,
                DiscountDescription = description
            };

            return ApiResponse<ApplyCouponResponseDto>
                .SuccessResponse(result, "Coupon applied successfully");
        }

        private static CouponResponseDto MapToDto(Coupon coupon)
        {
            return new CouponResponseDto
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType.ToString(),
                DiscountValue = coupon.DiscountValue,
                MinimumOrderAmount = coupon.MinimumOrderAmount,
                UsageLimit = coupon.UsageLimit,
                UsageCount = coupon.UsageCount,
                ExpiryDate = coupon.ExpiryDate,
                IsActive = coupon.IsActive
            };
        }
    }
}

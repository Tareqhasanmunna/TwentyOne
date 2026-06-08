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
    public class PreOrderService: IPreOrderService
    {
        private readonly IPreOrderRepository _preOrderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ISiteSettingRepository _siteSettingRepository;

        public PreOrderService(
            IPreOrderRepository preOrderRepository,
            IProductRepository productRepository,
            ISiteSettingRepository siteSettingRepository)
        {
            _preOrderRepository = preOrderRepository;
            _productRepository = productRepository;
            _siteSettingRepository = siteSettingRepository;
        }

        public async Task<ApiResponse<List<PreOrderResponseDto>>>
            GetAllAsync()
        {
            var preOrders = await _preOrderRepository.GetAllAsync();
            return ApiResponse<List<PreOrderResponseDto>>
                .SuccessResponse(preOrders.Select(MapToDto).ToList());
        }

        public async Task<ApiResponse<List<PreOrderResponseDto>>>
            GetMyPreOrdersAsync(string userId)
        {
            var preOrders = await _preOrderRepository
                .GetByUserIdAsync(userId);
            return ApiResponse<List<PreOrderResponseDto>>
                .SuccessResponse(preOrders.Select(MapToDto).ToList());
        }

        public async Task<ApiResponse<PreOrderResponseDto>>
            GetByIdAsync(int id)
        {
            var preOrder = await _preOrderRepository.GetByIdAsync(id);
            if (preOrder == null)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse("Pre-order not found");

            return ApiResponse<PreOrderResponseDto>
                .SuccessResponse(MapToDto(preOrder));
        }

        public async Task<ApiResponse<PreOrderResponseDto>> CreateAsync(
            string userId, CreatePreOrderDto dto)
        {
            // Validate terms accepted
            if (!dto.TermsAccepted)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse(
                        "You must accept the terms and conditions");

            // Get product
            var product = await _productRepository
                .GetByIdAsync(dto.ProductId);
            if (product == null)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse("Product not found");

            // Check if product is pre-order
            if (!product.IsPreOrder)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse("This product is not available for pre-order");

            // Check if pre-order deadline has passed
            if (product.PreOrderDeadline.HasValue &&
                DateTime.UtcNow > product.PreOrderDeadline.Value)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse("Pre-order deadline has passed");

            // Check if user already pre-ordered this product
            var alreadyPreOrdered = await _preOrderRepository
                .HasUserPreOrderedAsync(userId, dto.ProductId);
            if (alreadyPreOrdered)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse(
                        "You have already pre-ordered this product");

            // Calculate deposit based on price
            var deposit = product.Price >= 10000 ? 500 : 200;
            var remaining = product.Price - deposit;

            // Generate pre-order number
            var preOrderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-" +
                $"{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var preOrder = new PreOrder
            {
                PreOrderNumber = preOrderNumber,
                UserId = userId,
                ProductId = dto.ProductId,
                BkashTransactionId = dto.BkashTransactionId,
                BkashSenderNumber = dto.BkashSenderNumber,
                DepositAmount = deposit,
                ProductPrice = product.Price,
                RemainingAmount = remaining,
                Status = PreOrderStatus.Pending,
                TermsAccepted = dto.TermsAccepted,
                DeadlineDate = product.PreOrderDeadline,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _preOrderRepository.CreateAsync(preOrder);
            var result = await _preOrderRepository
                .GetByIdAsync(created.Id);

            return ApiResponse<PreOrderResponseDto>
                .SuccessResponse(MapToDto(result!),
                    "Pre-order placed successfully! " +
                    "We will verify your payment shortly.");
        }

        public async Task<ApiResponse<PreOrderResponseDto>>
            UpdateStatusAsync(int id, UpdatePreOrderStatusDto dto)
        {
            var preOrder = await _preOrderRepository.GetByIdAsync(id);
            if (preOrder == null)
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse("Pre-order not found");

            // Validate status transition
            if (!IsValidTransition(preOrder.Status, dto.Status))
                return ApiResponse<PreOrderResponseDto>
                    .FailResponse($"Cannot change status from " +
                        $"{preOrder.Status} to {dto.Status}");

            preOrder.Status = dto.Status;
            preOrder.AdminNotes = dto.AdminNotes;
            preOrder.UpdatedAt = DateTime.UtcNow;

            var updated = await _preOrderRepository.UpdateAsync(preOrder);

            return ApiResponse<PreOrderResponseDto>
                .SuccessResponse(MapToDto(updated),
                    "Pre-order status updated");
        }

        public async Task<ApiResponse<string>> CancelAsync(
            int id, string userId)
        {
            var preOrder = await _preOrderRepository.GetByIdAsync(id);
            if (preOrder == null)
                return ApiResponse<string>
                    .FailResponse("Pre-order not found");

            if (preOrder.UserId != userId)
                return ApiResponse<string>
                    .FailResponse("Unauthorized");

            if (preOrder.Status != PreOrderStatus.Pending)
                return ApiResponse<string>
                    .FailResponse(
                        "Only pending pre-orders can be cancelled");

            preOrder.Status = PreOrderStatus.Cancelled;
            preOrder.UpdatedAt = DateTime.UtcNow;
            await _preOrderRepository.UpdateAsync(preOrder);

            return ApiResponse<string>
                .SuccessResponse("Pre-order cancelled");
        }

        public async Task<ApiResponse<string>> GetBkashNumberAsync()
        {
            var number = await _siteSettingRepository
                .GetValueAsync("BkashNumber");
            return ApiResponse<string>
                .SuccessResponse(number ?? "01749028100");
        }

        private static bool IsValidTransition(
            PreOrderStatus current, PreOrderStatus next)
        {
            return (current, next) switch
            {
                (PreOrderStatus.Pending,
                 PreOrderStatus.Confirmed) => true,
                (PreOrderStatus.Confirmed,
                 PreOrderStatus.Ready) => true,
                (PreOrderStatus.Ready,
                 PreOrderStatus.Delivered) => true,
                (PreOrderStatus.Pending,
                 PreOrderStatus.Cancelled) => true,
                _ => false
            };
        }

        private static PreOrderResponseDto MapToDto(PreOrder preOrder)
        {
            return new PreOrderResponseDto
            {
                Id = preOrder.Id,
                PreOrderNumber = preOrder.PreOrderNumber,
                DepositAmount = preOrder.DepositAmount,
                ProductPrice = preOrder.ProductPrice,
                RemainingAmount = preOrder.RemainingAmount,
                BkashTransactionId = preOrder.BkashTransactionId,
                BkashSenderNumber = preOrder.BkashSenderNumber,
                Status = preOrder.Status.ToString(),
                AdminNotes = preOrder.AdminNotes,
                TermsAccepted = preOrder.TermsAccepted,
                CreatedAt = preOrder.CreatedAt,
                UpdatedAt = preOrder.UpdatedAt,
                DeadlineDate = preOrder.DeadlineDate,
                ProductId = preOrder.ProductId,
                ProductName = preOrder.Product?.Name ?? string.Empty,
                ProductSlug = preOrder.Product?.Slug ?? string.Empty,
                ProductImageUrl = preOrder.Product?.Images
                    .FirstOrDefault()?.ImageUrl,
                CustomerName = preOrder.User?.FullName ?? string.Empty,
                CustomerEmail = preOrder.User?.Email ?? string.Empty
            };
        }

    }
}

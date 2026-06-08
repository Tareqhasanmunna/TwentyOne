using Microsoft.Extensions.Logging;
using TwentyOne.BLL.Helpers;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Enums;
using TwentyOne.Shared.Models;

namespace TwentyOne.BLL.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly SanitizationService _sanitizer;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            SanitizationService sanitizer,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _sanitizer = sanitizer;
            _logger = logger;
        }

        public async Task<ApiResponse<List<OrderResponseDto>>> GetAllAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return ApiResponse<List<OrderResponseDto>>
                .SuccessResponse(orders.Select(MapToDto).ToList());
        }

        public async Task<ApiResponse<List<OrderResponseDto>>> GetMyOrdersAsync(
            string userId)
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return ApiResponse<List<OrderResponseDto>>
                .SuccessResponse(orders.Select(MapToDto).ToList());
        }

        public async Task<ApiResponse<OrderResponseDto>> GetByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return ApiResponse<OrderResponseDto>
                    .FailResponse("Order not found");

            return ApiResponse<OrderResponseDto>
                .SuccessResponse(MapToDto(order));
        }

        public async Task<ApiResponse<OrderResponseDto>> PlaceOrderAsync(
            string? userId, CreateOrderDto dto)
        {
            // Sanitize inputs
            dto.DeliveryAddress = _sanitizer
                .SanitizePlainText(dto.DeliveryAddress);
            dto.Notes = _sanitizer
                .SanitizePlainText(dto.Notes);
            dto.CouponCode = _sanitizer
                .SanitizePlainText(dto.CouponCode);

            // Validate all items and calculate total
            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0;

            foreach (var item in dto.Items)
            {
                var product = await _productRepository
                    .GetByIdAsync(item.ProductId);

                if (product == null)
                    return ApiResponse<OrderResponseDto>
                        .FailResponse(
                            $"Product {item.ProductId} not found");

                if (product.IsArchived)
                    return ApiResponse<OrderResponseDto>
                        .FailResponse(
                            $"{product.Name} is no longer available");

                if (product.StockQuantity < item.Quantity)
                    return ApiResponse<OrderResponseDto>
                        .FailResponse(
                            $"Insufficient stock for {product.Name}");

                // Calculate actual selling price
                decimal sellingPrice = product.Price;

                if (product.DiscountPercentage > 0)
                    sellingPrice = product.Price -
                        (product.Price *
                         product.DiscountPercentage.Value / 100);
                else if (product.DiscountAmount > 0)
                    sellingPrice = product.Price -
                        product.DiscountAmount.Value;

                if (sellingPrice < 0) sellingPrice = 0;

                var subtotal = sellingPrice * item.Quantity;
                totalAmount += subtotal;

                orderItems.Add(new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = sellingPrice,
                    Subtotal = subtotal
                });

                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Generate unique order number
            var orderNumber = $"TW-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var order = new Order
            {
                OrderNumber = orderNumber,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                UserId = userId,
                DeliveryAddress = dto.DeliveryAddress,
                Notes = dto.Notes,
                CouponCode = dto.CouponCode,
                DeliveryCharge = dto.DeliveryCharge,
                TotalAmount = totalAmount,
                DiscountAmount = dto.DiscountAmount,
                FinalAmount = totalAmount + dto.DeliveryCharge - dto.DiscountAmount,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            var created = await _orderRepository.CreateAsync(order);
            var result = await _orderRepository.GetByIdAsync(created.Id);

            _logger.LogInformation("Order placed: {OrderNumber} for user: {UserId}", order.OrderNumber, userId);

            return ApiResponse<OrderResponseDto>
                .SuccessResponse(MapToDto(result!), "Order placed successfully");
        }


        public async Task<ApiResponse<OrderResponseDto>> UpdateStatusAsync(
            int id, UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return ApiResponse<OrderResponseDto>
                    .FailResponse("Order not found");

            // Validate status transition
            var validTransition = IsValidTransition(order.Status, dto.Status);
            if (!validTransition)
                return ApiResponse<OrderResponseDto>
                    .FailResponse($"Cannot change status from " +
                        $"{order.Status} to {dto.Status}");

            order.Status = dto.Status;
            order.UpdatedAt = DateTime.UtcNow;

            var updated = await _orderRepository.UpdateAsync(order);

            return ApiResponse<OrderResponseDto>
                .SuccessResponse(MapToDto(updated), "Order status updated");
        }

        public async Task<ApiResponse<string>> CancelOrderAsync(
            int id, string userId)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return ApiResponse<string>.FailResponse("Order not found");

            // Only the owner can cancel their order
            if (string.IsNullOrEmpty(order.UserId) || order.UserId != userId)
                return ApiResponse<string>.FailResponse("Unauthorized");

            // Can only cancel pending orders
            if (order.Status != OrderStatus.Pending)
                return ApiResponse<string>.FailResponse(
                    "Only pending orders can be cancelled");

            // Can only cancel pending orders
            if (order.Status != OrderStatus.Pending)
                return ApiResponse<string>.FailResponse("Only pending orders can be cancelled");

            // Restore stock
            foreach (var item in order.OrderItems)
            {
                var product = await _productRepository
                    .GetByIdAsync(item.ProductId);

                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            return ApiResponse<string>
                .SuccessResponse("Order cancelled successfully");
        }

        // Validates allowed status transitions
        private static bool IsValidTransition(
            OrderStatus current, OrderStatus next)
        {
            return (current, next) switch
            {
                (OrderStatus.Pending, OrderStatus.Confirmed) => true,
                (OrderStatus.Confirmed, OrderStatus.Shipped) => true,
                (OrderStatus.Shipped, OrderStatus.Delivered) => true,
                (OrderStatus.Pending, OrderStatus.Cancelled) => true,
                (OrderStatus.Delivered, OrderStatus.Returned) => true,
                _ => false
            };
        }

        private static OrderResponseDto MapToDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.User?.FullName ?? order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                CustomerEmail = order.User?.Email ?? string.Empty,
                OrderNumber = order.OrderNumber,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                DeliveryCharge = order.DeliveryCharge,
                FinalAmount = order.FinalAmount,
                Status = order.Status.ToString(),
                CouponCode = order.CouponCode,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                CreatedAt = order.CreatedAt,
                Items = order.OrderItems?.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? string.Empty,
                    ProductSlug = oi.Product?.Slug ?? string.Empty,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Subtotal = oi.Subtotal
                }).ToList() ?? new()
            };
        }

        public async Task<ApiResponse<OrderResponseDto>>
        GetByOrderNumberAsync(string orderNumber)
        {
            var order = await _orderRepository
                .GetByOrderNumberAsync(orderNumber);

            if (order == null)
                return ApiResponse<OrderResponseDto>
                    .FailResponse("Order not found. " +
                        "Please check your order number.");

            return ApiResponse<OrderResponseDto>
                .SuccessResponse(MapToDto(order));
        }
    }
}

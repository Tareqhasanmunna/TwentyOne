using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;
using TwentyOne.Web.MVC.Services;

namespace TwentyOne.Web.MVC.Controllers
{
    public class ShopController : BaseController
    {
        private readonly ProductApiService _productApiService;
        private readonly OrderApiService _orderApiService;
        private readonly AuthApiService _authApiService;
        private readonly CouponApiService _couponApiService;
        private readonly BannerApiService _bannerApiService;
        private readonly PreOrderApiService _preOrderApiService;

        public ShopController(
            ProductApiService productApiService,
            OrderApiService orderApiService,
            AuthApiService authApiService,
            CouponApiService couponApiService,
            BannerApiService bannerApiService,
            PreOrderApiService preOrderApiService) : base(bannerApiService)
        {
            _productApiService = productApiService;
            _orderApiService = orderApiService;
            _authApiService = authApiService;
            _couponApiService = couponApiService;
            _bannerApiService = bannerApiService;
            _preOrderApiService = preOrderApiService;
        }

        // Buy Now

        [HttpPost]
        public async Task<IActionResult> BuyNow(
            int productId, int quantity = 1)
        {
            var cart = GetCart();
            cart[productId] = quantity;
            SaveCart(cart);

            return RedirectToAction("Checkout");
        }

        // Cart

        public async Task<IActionResult> Cart()
        {
            var cart = GetCart();
            var cartItems = new List<CartItemViewModel>();

            foreach (var item in cart)
            {
                var product = await _productApiService
                    .GetByIdAsync(item.Key);
                if (product?.Data != null)
                {
                    // ✅ Use discounted price
                    var price = product.Data.HasDiscount
                        ? product.Data.DiscountedPrice
                        : product.Data.Price;

                    cartItems.Add(new CartItemViewModel
                    {
                        ProductId = item.Key,
                        ProductName = product.Data.Name,
                        ProductSlug = product.Data.Slug,
                        Price = price,
                        Quantity = item.Value,
                        ImageUrl = product.Data.ImageUrls
                            .FirstOrDefault(),
                        OriginalPrice = product.Data.Price,
                        HasDiscount = product.Data.HasDiscount
                    });
                }
            }

            var (insideDhaka, outsideDhaka) = await _bannerApiService
                .GetDeliveryChargesAsync();
            ViewBag.InsideDhakaCharge = insideDhaka;
            ViewBag.OutsideDhakaCharge = outsideDhaka;

            return View(cartItems);
        }

        // Add to Cart

        [HttpPost]
        public async Task<IActionResult> AddToCart(
            int productId, int quantity = 1)
        {
            var cart = GetCart();

            if (cart.ContainsKey(productId))
                cart[productId] += quantity;
            else
                cart[productId] = quantity;

            SaveCart(cart);

            decimal cartTotal = 0;
            int cartCount = cart.Values.Sum();

            foreach (var item in cart)
            {
                var product = await _productApiService
                    .GetByIdAsync(item.Key);
                if (product?.Data != null)
                {
                    
                    var price = product.Data.HasDiscount
                        ? product.Data.DiscountedPrice
                        : product.Data.Price;
                    cartTotal += price * item.Value;
                }
            }

            if (Request.Headers["X-Requested-With"] ==
                "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    cartCount = cartCount,
                    cartTotal = cartTotal,
                    message = "Added to cart!"
                });
            }

            return RedirectToAction("Cart");
        }

        // Update Cart

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();

            if (quantity <= 0)
                cart.Remove(productId);
            else
                cart[productId] = quantity;

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // Remove from Cart

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.Remove(productId);
            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // Checkout
        public async Task<IActionResult> Checkout()
        {

            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Cart");

            var cartItems = new List<CartItemViewModel>();
            decimal total = 0;

            foreach (var item in cart)
            {
                var product = await _productApiService.GetByIdAsync(item.Key);
                if (product?.Data != null)
                {
                    var price = product.Data.HasDiscount
                        ? product.Data.DiscountedPrice
                        : product.Data.Price;

                    var cartItem = new CartItemViewModel
                    {
                        ProductId = item.Key,
                        ProductName = product.Data.Name,
                        Price = price,
                        Quantity = item.Value,
                        ImageUrl = product.Data.ImageUrls.FirstOrDefault()
                    };
                    cartItems.Add(cartItem);
                    total += price * item.Value;
                }
            }

            var (insideDhaka, outsideDhaka) = await _bannerApiService
                .GetDeliveryChargesAsync();

            ViewBag.CartItems = cartItems;
            ViewBag.Total = total;
            ViewBag.InsideDhakaCharge = insideDhaka;
            ViewBag.OutsideDhakaCharge = outsideDhaka;
            var token = HttpContext.Session.GetString("JwtToken");
            ViewBag.IsGuest = string.IsNullOrEmpty(token);

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
            ViewBag.UserPhone = HttpContext.Session.GetString("UserPhone");

            return View(cartItems);
        }

        // Place order

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(
            string deliveryAddress, string? notes,
            string? couponCode, int deliveryCharge = 0,
            string? deliveryLocation = null,
            string? guestName = null,
            string? guestPhone = null,
            decimal discountAmount = 0)
        {
            var cart = GetCart();
            if (!cart.Any())
                return RedirectToAction("Cart");

            var token = HttpContext.Session.GetString("JwtToken");
            var isGuest = string.IsNullOrEmpty(token);

            // Get correct name and phone
            string customerName = isGuest
                ? guestName ?? "Guest"
                : HttpContext.Session.GetString("UserName") ?? "Customer";

            string customerPhone = isGuest
                ? guestPhone ?? ""
                : HttpContext.Session.GetString("UserPhone") ?? "";

            // Build clean address
            var fullAddress = deliveryAddress ?? "";
            if (!string.IsNullOrEmpty(deliveryLocation))
                fullAddress += $" | {deliveryLocation}";

            var dto = new CreateOrderDto
            {
                CustomerName = customerName,
                CustomerPhone = customerPhone,
                DeliveryAddress = fullAddress,
                Notes = notes,
                CouponCode = string.IsNullOrEmpty(couponCode)
                    ? null : couponCode,
                DeliveryCharge = deliveryCharge,
                DiscountAmount = discountAmount,
                Items = cart.Select(c => new CreateOrderItemDto
                {
                    ProductId = c.Key,
                    Quantity = c.Value
                }).ToList()
            };

            var result = await _orderApiService.PlaceOrderAsync(dto);

            if (result == null || !result.Success)
            {
                TempData["Error"] = result?.Message ??
                    "Failed to place order";
                return RedirectToAction("Checkout");
            }

            HttpContext.Session.Remove("Cart");
            TempData["OrderNumber"] = result.Data?.OrderNumber;
            return RedirectToAction("OrderSuccess");
        }

        // Order Success
        [HttpGet]
        public IActionResult OrderSuccess()
        {
            // Retrieve the order number passed from the PlaceOrder method
            var orderNumber = TempData["OrderNumber"] as string;

            // Safety check: if someone navigates directly to this URL without placing an order, 
            // redirect them back to the home page.
            if (string.IsNullOrEmpty(orderNumber))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.OrderNumber = orderNumber;
            return View();
        }

        // Cart helpers

        private Dictionary<int, int> GetCart()
        {
            var cartJson = HttpContext.Session.GetString("Cart");
            if (string.IsNullOrEmpty(cartJson))
                return new Dictionary<int, int>();

            return JsonSerializer.Deserialize<Dictionary<int, int>>(cartJson)
                ?? new Dictionary<int, int>();
        }

        private void SaveCart(Dictionary<int, int> cart)
        {
            HttpContext.Session.SetString("Cart",
                JsonSerializer.Serialize(cart));
        }

        // Validate coupon code (AJAX)
        [HttpPost]
        public async Task<IActionResult> ValidateCoupon(
        string code, decimal orderTotal)
        {
            var result = await _couponApiService.ApplyCouponAsync(
                new TwentyOne.Shared.DTOs.Requests.ApplyCouponDto
                {
                    Code = code,
                    OrderTotal = orderTotal
                });

            return Json(result);
        }

        // Get cart summary (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetCartSummary()
        {
            var cart = GetCart();
            int count = cart.Values.Sum();
            decimal total = 0;

            foreach (var item in cart)
            {
                var product = await _productApiService
                    .GetByIdAsync(item.Key);
                if (product?.Data != null)
                {
                    var price = product.Data.HasDiscount
                        ? product.Data.DiscountedPrice
                        : product.Data.Price;
                    total += price * item.Value;
                }
            }

            return Json(new { count, total });
        }

        // Track order
        [HttpGet]
        public async Task<IActionResult> TrackOrder(string? orderNumber)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            var isLoggedIn = !string.IsNullOrEmpty(token);

            ViewBag.MyOrders = new List<OrderResponseDto>();
            ViewBag.SearchedNumber = orderNumber;

            if (isLoggedIn)
            {
                var myOrdersResult = await _orderApiService.GetMyOrdersAsync();
                if (myOrdersResult?.Success == true)
                {
                    ViewBag.MyOrders = myOrdersResult.Data ?? new List<OrderResponseDto>();
                }
            }

            if (!string.IsNullOrEmpty(orderNumber))
            {
                orderNumber = orderNumber.Trim();

                // 🛠️ Detect and handle Pre-Orders dynamically
                if (orderNumber.StartsWith("PO-", StringComparison.OrdinalIgnoreCase))
                {
                    if (!isLoggedIn)
                    {
                        TempData["Error"] = "Please log in to track pre-orders.";
                        return View();
                    }

                    // Fetch the user's pre-orders matching their active context session token
                    var preOrdersResult = await _preOrderApiService.GetMyPreOrdersAsync();

                    var matchingPreOrder = preOrdersResult?.Data?
                        .FirstOrDefault(p => string.Equals(p.PreOrderNumber, orderNumber, StringComparison.OrdinalIgnoreCase));

                    if (matchingPreOrder == null)
                    {
                        TempData["Error"] = "Pre-order not found. Please check your tracking token number.";
                    }
                    else
                    {
                        // 🚀 Project PreOrderResponseDto into OrderResponseDto seamlessly for your View layout
                        ViewBag.Order = new OrderResponseDto
                        {
                            OrderNumber = matchingPreOrder.PreOrderNumber,
                            Status = matchingPreOrder.Status, // Maps cleanly to your switch logic ("Pending", "Confirmed", etc.)
                            CreatedAt = matchingPreOrder.CreatedAt,
                            CustomerName = matchingPreOrder.CustomerName ?? HttpContext.Session.GetString("UserName") ?? "Customer",
                            CustomerPhone = HttpContext.Session.GetString("UserPhone") ?? "N/A",
                            DeliveryAddress = "Pre-Order Deposit Confirmed",
                            TotalAmount = matchingPreOrder.ProductPrice,
                            DiscountAmount = 0,
                            DeliveryCharge = 0,
                            FinalAmount = matchingPreOrder.DepositAmount // Shows what has been paid for tracking verification
                        };
                    }
                }
                else
                {
                    // Regular Standard Order Pipeline
                    var result = await _orderApiService.TrackOrderAsync(orderNumber);

                    if (result == null || !result.Success)
                    {
                        TempData["Error"] = result?.Message ?? "Order not found. Please check your order number.";
                    }
                    else
                    {
                        ViewBag.Order = result.Data;
                    }
                }
            }

            return View();
        }

        // Download PDF

        [HttpGet]
        public async Task<IActionResult> DownloadPdf(string orderNumber)
        {
            if (string.IsNullOrEmpty(orderNumber))
                return RedirectToAction("Index", "Home");

            var pdf = await _orderApiService
                .DownloadPdfByOrderNumberAsync(orderNumber);

            if (pdf == null)
            {
                TempData["Error"] = "Could not generate PDF";
                return RedirectToAction("TrackOrder",
                    new { orderNumber });
            }

            return File(pdf, "application/pdf",
                $"Order-{orderNumber}.pdf");
        }


    }
    // Cart item view model
    public class CartItemViewModel
    {
        public decimal OriginalPrice { get; set; }
        public bool HasDiscount { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
using Microsoft.AspNetCore.Mvc;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Web.MVC.Services;
using TwentyOne.Web.MVC.ViewModels;

namespace TwentyOne.Web.MVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly AuthApiService _authApiService;
        private readonly BrandApiService _brandApiService;
        private readonly ProductApiService _productApiService;
        private readonly OrderApiService _orderApiService;
        private readonly CouponApiService _couponApiService;

        public AdminController(
            AuthApiService authApiService,
            BrandApiService brandApiService,
            ProductApiService productApiService,
            OrderApiService orderApiService,
            CouponApiService couponApiService)
        {
            _authApiService = authApiService;
            _brandApiService = brandApiService;
            _productApiService = productApiService;
            _orderApiService = orderApiService;
            _couponApiService = couponApiService;
        }

        // Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // If already logged in redirect to dashboard
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                return RedirectToAction("Dashboard");

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authApiService.LoginAsync(new LoginDto
            {
                Email = model.Email,
                Password = model.Password
            });

            if (result == null || !result.Success)
            {
                model.ErrorMessage = result?.Message ?? "Login failed";
                return View(model);
            }

            // Check if user is Admin or SuperAdmin
            var roles = result.Data?.Roles ?? new List<string>();
            if (!roles.Contains("Admin") && !roles.Contains("SuperAdmin"))
            {
                model.ErrorMessage = "You don't have permission to access the admin panel";
                return View(model);
            }

            // Store token and user info in session
            HttpContext.Session.SetString("JwtToken", result.Data!.Token);
            HttpContext.Session.SetString("UserName", result.Data.FullName);
            HttpContext.Session.SetString("UserEmail", result.Data.Email);
            HttpContext.Session.SetString("UserRoles",
                string.Join(",", result.Data.Roles));

            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Dashboard");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var products = await _productApiService
                .GetAllAsync(new Shared.DTOs.Requests.ProductFilterDto());
            var orders = await _orderApiService.GetAllAsync();
            var brands = await _brandApiService.GetAllAsync();

            ViewBag.TotalProducts = products?.Data?.TotalCount ?? 0;
            ViewBag.TotalOrders = orders?.Data?.Count ?? 0;
            ViewBag.TotalBrands = brands?.Data?.Count ?? 0;
            ViewBag.PendingOrders = orders?.Data?
                .Count(o => o.Status == "Pending") ?? 0;
            ViewBag.RecentOrders = orders?.Data?.Take(5).ToList();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View();
        }

        // Brands
        public async Task<IActionResult> Brands()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _brandApiService.GetAllAsync();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(result?.Data ?? new List<BrandResponseDto>());
        }

        [HttpGet]
        public IActionResult CreateBrand()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBrand(CreateBrandDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _brandApiService.CreateAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to create brand");
                return View(dto);
            }

            TempData["Success"] = "Brand created successfully!";
            return RedirectToAction("Brands");
        }

        [HttpGet]
        public async Task<IActionResult> EditBrand(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _brandApiService.GetByIdAsync(id);
            if (result == null || !result.Success)
                return RedirectToAction("Brands");

            var dto = new UpdateBrandDto
            {
                Name = result.Data!.Name,
                Description = result.Data.Description,
                LogoUrl = result.Data.LogoUrl,
                IsActive = result.Data.IsActive
            };

            ViewBag.BrandId = id;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditBrand(int id, UpdateBrandDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BrandId = id;
                return View(dto);
            }

            var result = await _brandApiService.UpdateAsync(id, dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to update brand");
                ViewBag.BrandId = id;
                return View(dto);
            }

            TempData["Success"] = "Brand updated successfully!";
            return RedirectToAction("Brands");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            await _brandApiService.RemoveAsync(id);
            TempData["Success"] = "Brand deleted successfully!";
            return RedirectToAction("Brands");
        }

        // Products
        public async Task<IActionResult> Products(
            string? search, int? brandId, int page = 1)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var filter = new ProductFilterDto
            {
                Search = search,
                BrandId = brandId,
                Page = page,
                PageSize = 10
            };

            var products = await _productApiService.GetAllAsync(filter);
            var brands = await _brandApiService.GetAllAsync();

            ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentBrandId = brandId;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View(products?.Data);
        }

        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var brands = await _brandApiService.GetAllAsync();
            ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ??
                    new List<BrandResponseDto>();
                return View(dto);
            }

            var result = await _productApiService.CreateAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create product");
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ??
                    new List<BrandResponseDto>();
                return View(dto);
            }

            TempData["Success"] = "Product created successfully!";
            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _productApiService.GetByIdAsync(id);
            if (result == null || !result.Success)
                return RedirectToAction("Products");

            var dto = new UpdateProductDto
            {
                Name = result.Data!.Name,
                Description = result.Data.Description,
                Price = result.Data.Price,
                StockQuantity = result.Data.StockQuantity,
                Scale = result.Data.Scale,
                ReleaseYear = result.Data.ReleaseYear,
                IsLimitedEdition = result.Data.IsLimitedEdition,
                IsPreOrder = result.Data.IsPreOrder,
                IsArchived = result.Data.IsArchived,
                BrandId = result.Data.BrandId
            };

            var brands = await _brandApiService.GetAllAsync();
            ViewBag.Brands = brands?.Data ??
                new List<BrandResponseDto>();
            ViewBag.ProductId = id;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, UpdateProductDto dto)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ??
                    new List<BrandResponseDto>();
                ViewBag.ProductId = id;
                return View(dto);
            }

            var result = await _productApiService.UpdateAsync(id, dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to update product");
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ??
                    new List<BrandResponseDto>();
                ViewBag.ProductId = id;
                return View(dto);
            }

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction("Products");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productApiService.RemoveAsync(id);
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction("Products");
        }

        // Orders
        public async Task<IActionResult> Orders()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _orderApiService.GetAllAsync();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(result?.Data ??
                new List<Shared.DTOs.Responses.OrderResponseDto>());
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _orderApiService.GetByIdAsync(id);
            if (result == null || !result.Success)
                return RedirectToAction("Orders");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(
            int id, int status)
        {
            var dto = new UpdateOrderStatusDto
            {
                Status = (Shared.Enums.OrderStatus)status
            };

            await _orderApiService.UpdateStatusAsync(id, dto);
            TempData["Success"] = "Order status updated!";
            return RedirectToAction("OrderDetail", new { id });
        }

        // Coupons
        public async Task<IActionResult> Coupons()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _couponApiService.GetAllAsync();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(result?.Data ??
                new List<Shared.DTOs.Responses.CouponResponseDto>());
        }

        [HttpGet]
        public IActionResult CreateCoupon()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCoupon(CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _couponApiService.CreateAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create coupon");
                return View(dto);
            }

            TempData["Success"] = "Coupon created successfully!";
            return RedirectToAction("Coupons");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            await _couponApiService.RemoveAsync(id);
            TempData["Success"] = "Coupon deleted!";
            return RedirectToAction("Coupons");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCoupon(int id)
        {
            await _couponApiService.ToggleAsync(id);
            return RedirectToAction("Coupons");
        }
    }
}

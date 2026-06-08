using Microsoft.AspNetCore.Mvc;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Enums;
using TwentyOne.Web.MVC.Services;
using TwentyOne.Web.MVC.ViewModels;
using static System.Net.Mime.MediaTypeNames;

namespace TwentyOne.Web.MVC.Controllers
{
    public class AdminController : BaseController
    {
        private readonly AuthApiService _authApiService;
        private readonly BrandApiService _brandApiService;
        private readonly ProductApiService _productApiService;
        private readonly OrderApiService _orderApiService;
        private readonly CouponApiService _couponApiService;
        private readonly ImageApiService _imageApiService;
        private readonly BannerApiService _bannerApiService;
        private readonly PreOrderApiService _preOrderApiService;
        

        public AdminController(
            AuthApiService authApiService,
            BrandApiService brandApiService,
            ProductApiService productApiService,
            OrderApiService orderApiService,
            CouponApiService couponApiService,
            ImageApiService imageApiService,
            BannerApiService bannerApiService,
            
            PreOrderApiService preOrderApiService): base(bannerApiService)
        {
            _authApiService = authApiService;
            _brandApiService = brandApiService;
            _productApiService = productApiService;
            _orderApiService = orderApiService;
            _couponApiService = couponApiService;
            _imageApiService = imageApiService;
            _bannerApiService = bannerApiService;
            
            _preOrderApiService = preOrderApiService;
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

            var rolesString = string.Join(",", result.Data.Roles);
            HttpContext.Session.SetString("UserRoles", rolesString);

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
            var productList = products?.Data?.Items ??
                new List<ProductResponseDto>();
            
            var lowStock = productList
                .Where(p => p.StockQuantity <= 5 &&
                    p.StockQuantity > 0 &&
                    !p.IsPreOrder)
                .OrderBy(p => p.StockQuantity)
                .ToList();

            var outOfStock = productList
                .Where(p => p.StockQuantity == 0 &&
                            !p.IsPreOrder)
                .ToList();

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
        public async Task<IActionResult> CreateBrand(
            CreateBrandDto dto, IFormFile? logoImage)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // Upload logo if provided
            if (logoImage != null && logoImage.Length > 0)
            {
                var uploadResult = await _imageApiService
                    .UploadBrandLogoAsync(logoImage);

                if (uploadResult?.Success == true && uploadResult.Data != null)
                    dto.LogoUrl = uploadResult.Data;
                else
                {
                    ModelState.AddModelError("",
                        "Logo upload failed. Please try again.");
                    return View(dto);
                }
            }

            var result = await _brandApiService.CreateAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create brand");
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
        public async Task<IActionResult> EditBrand(
            int id, UpdateBrandDto dto, IFormFile? logoImage)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BrandId = id;
                return View(dto);
            }

            // Upload new logo if provided
            if (logoImage != null && logoImage.Length > 0)
            {
                var uploadResult = await _imageApiService
                    .UploadBrandLogoAsync(logoImage);

                if (uploadResult?.Success == true && uploadResult.Data != null)
                    dto.LogoUrl = uploadResult.Data;
                else
                {
                    ModelState.AddModelError("",
                        "Logo upload failed. Please try again.");
                    ViewBag.BrandId = id;
                    return View(dto);
                }
            }

            var result = await _brandApiService.UpdateAsync(id, dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to update brand");

                if (result?.Errors != null && result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error);
                }

                ViewBag.BrandId = id;
                return View(dto);
            }

            TempData["Success"] = "Brand updated successfully!";
            return RedirectToAction("Brands");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var result = await _brandApiService.RemoveAsync(id);

            if (result == null || !result.Success)
            {
                TempData["Error"] = result?.Message ?? "Failed to delete brand";
                return RedirectToAction("Brands");
            }

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
        public async Task<IActionResult> CreateProduct(
            CreateProductDto dto, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();
                return View(dto);
            }

            // Upload images if provided
            if (images != null && images.Any())
            {
                var imageUrls = new List<string>();
                foreach (var image in images)
                {
                    var uploadResult = await _imageApiService.UploadImageAsync(image);
                    if (uploadResult?.Success == true && uploadResult.Data != null)
                        imageUrls.Add(uploadResult.Data);
                }
                dto.ImageUrls = imageUrls;
            }

            var result = await _productApiService.CreateAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("", result?.Message ?? "Failed to create product");
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();
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
            ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();
            ViewBag.ProductId = id;
            ViewBag.ExistingImages = result.Data.ImageUrls;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(int id, UpdateProductDto dto, List<IFormFile>? images)
        {
            if (!ModelState.IsValid)
            {
                var brands = await _brandApiService.GetAllAsync();
                ViewBag.Brands = brands?.Data ??
                    new List<BrandResponseDto>();
                ViewBag.ProductId = id;
                return View(dto);
            }

            if (images != null && images.Any())
            {
                var imageUrls = new List<string>();
                foreach (var image in images)
                {
                    var uploadResult = await _imageApiService.UploadImageAsync(image);
                    if (uploadResult?.Success == true && uploadResult.Data != null)
                        imageUrls.Add(uploadResult.Data);
                }
                dto.ImageUrls = imageUrls;
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
        public async Task<IActionResult> UpdateStock(
    int productId, int newStock)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var product = await _productApiService
                .GetByIdAsync(productId);
            if (product?.Data == null)
                return RedirectToAction("Products");

            var dto = new UpdateProductDto
            {
                Name = product.Data.Name,
                Description = product.Data.Description,
                Price = product.Data.Price,
                StockQuantity = newStock,
                Scale = product.Data.Scale,
                ReleaseYear = product.Data.ReleaseYear,
                IsLimitedEdition = product.Data.IsLimitedEdition,
                IsPreOrder = product.Data.IsPreOrder,
                IsArchived = product.Data.IsArchived,
                BrandId = product.Data.BrandId,
                DiscountPercentage = product.Data.DiscountPercentage,
                DiscountAmount = product.Data.DiscountAmount
            };

            await _productApiService.UpdateAsync(productId, dto);
            TempData["Success"] = "Stock updated!";
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
            return View("OrderDetail", result.Data);
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

        // Interface

        public async Task<IActionResult> Interface()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var banners = await _bannerApiService.GetAllAsync();
            var logo = await _bannerApiService.GetLogoUrlAsync();
            var (insideDhaka, outsideDhaka) = await _bannerApiService
                .GetDeliveryChargesAsync();
            var bkashNumber = await _bannerApiService 
                .GetBkashNumberAsync();

            ViewBag.Banners = banners?.Data ?? new List<BannerResponseDto>();
            ViewBag.LogoUrl = logo;
            ViewBag.InsideDhakaCharge = insideDhaka;
            ViewBag.OutsideDhakaCharge = outsideDhaka;
            ViewBag.BkashNumber = bkashNumber;
            ViewBag.UserName = HttpContext.Session.GetString("UserName");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBkashNumber(
            string bkashNumber)
        {
            if (!string.IsNullOrEmpty(bkashNumber))
            {

                await _bannerApiService.UpdateBkashNumberAsync(bkashNumber);
                TempData["Success"] = "bKash number updated!";
            }
            return RedirectToAction("Interface");
        }

        [HttpPost]
        public async Task<IActionResult> UploadBanner(
            IFormFile bannerImage, string? title,
            string? subtitle, int displayOrder = 0)
        {
            if (bannerImage != null)
            {
                var result = await _bannerApiService.CreateBannerAsync(
                    title, subtitle, displayOrder, bannerImage);

                if (result?.Success == true)
                    TempData["Success"] = "Banner uploaded successfully!";
                else
                    TempData["Error"] = result?.Message ?? "Upload failed";
            }

            return RedirectToAction("Interface");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            await _bannerApiService.DeleteAsync(id);
            TempData["Success"] = "Banner deleted!";
            return RedirectToAction("Interface");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleBanner(int id)
        {
            await _bannerApiService.ToggleAsync(id);
            return RedirectToAction("Interface");
        }


        [HttpPost]
        public async Task<IActionResult> UploadSiteLogo(IFormFile logoFile)
        {
            if (logoFile != null)
            {
                var result = await _bannerApiService.UploadLogoAsync(logoFile);
                if (result?.Success == true)
                    TempData["Success"] = "Logo updated successfully!";
                else
                    TempData["Error"] = "Logo upload failed";
            }

            return RedirectToAction("Interface");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDeliveryCharges(
            int insideDhaka, int outsideDhaka)
        {
            var result = await _bannerApiService
                .UpdateDeliveryChargesAsync(insideDhaka, outsideDhaka);

            if (result?.Success == true)
                TempData["Success"] = "Delivery charges updated!";
            else
                TempData["Error"] = "Failed to update charges";

            return RedirectToAction("Interface");
        }

        // Pre-Orders

        public async Task<IActionResult> PreOrders()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _preOrderApiService.GetAllAsync();
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View(result?.Data ??
                new List<PreOrderResponseDto>());
        }

        public async Task<IActionResult> PreOrderDetail(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _preOrderApiService.GetByIdAsync(id);
            if (result == null || !result.Success)
                return RedirectToAction("PreOrders");

            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View("AdminPreOrderDetail", result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePreOrderStatus(
            int id, int status, string? adminNotes)
        {
            var dto = new UpdatePreOrderStatusDto
            {
                Status = (PreOrderStatus)status,
                AdminNotes = adminNotes
            };

            var result = await _preOrderApiService.UpdateStatusAsync(id, dto);

            if (result?.Success == true)
                TempData["Success"] = "Pre-order status updated!";
            else
                TempData["Error"] = result?.Message ?? "Update failed";

            return RedirectToAction("PreOrderDetail", new { id });
        }

        // Create Admin

        [HttpGet]
        public IActionResult CreateAdmin()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            // Check if SuperAdmin
            var roles = HttpContext.Session
                .GetString("UserRoles") ?? "";
            if (!roles.Contains("SuperAdmin"))
                return RedirectToAction("Dashboard");

            ViewBag.UserName = HttpContext.Session
                .GetString("UserName");
            return View();
        }

        // Only SuperAdmin can create new admin accounts

        [HttpPost]
        public async Task<IActionResult> CreateAdmin(
            CreateAdminDto dto)
        {
            var roles = HttpContext.Session
                .GetString("UserRoles") ?? "";
            if (!roles.Contains("SuperAdmin"))
                return RedirectToAction("Dashboard");

            if (!ModelState.IsValid)
            {
                ViewBag.UserName = HttpContext.Session
                    .GetString("UserName");
                return View(dto);
            }

            var result = await _authApiService
                .CreateAdminAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Failed to create admin");
                ViewBag.UserName = HttpContext.Session
                    .GetString("UserName");
                return View(dto);
            }

            TempData["Success"] = "Admin account created successfully!";
            return RedirectToAction("CreateAdmin");
        }

        // List all admins - only SuperAdmin can access
        public async Task<IActionResult> AdminList()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var roles = HttpContext.Session
                .GetString("UserRoles") ?? "";
            if (!roles.Contains("SuperAdmin"))
                return RedirectToAction("Dashboard");

            var result = await _authApiService.GetAdminsAsync();
            ViewBag.UserName = HttpContext.Session
                .GetString("UserName");

            return View(result?.Data);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAdmin(
            string id, string password)
        {
            var result = await _authApiService
                .ToggleAdminAsync(id, password);

            if (result?.Success == true)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result?.Message ??
                    "Action failed";

            return RedirectToAction("AdminList");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdmin(
            string id, string password)
        {
            var result = await _authApiService
                .DeleteAdminAsync(id, password);

            if (result?.Success == true)
                TempData["Success"] = "Admin deleted successfully";
            else
                TempData["Error"] = result?.Message ??
                    "Action failed";

            return RedirectToAction("AdminList");
        }

        [HttpGet]
        public async Task<IActionResult> GetAdminsJson()
        {
            var result = await _authApiService.GetAdminsAsync();
            return Json(result?.Data);
        }
    }
}

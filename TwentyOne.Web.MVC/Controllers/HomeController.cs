using Microsoft.AspNetCore.Mvc;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Web.MVC.Services;

namespace TwentyOne.Web.MVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ProductApiService _productApiService;
        private readonly BrandApiService _brandApiService;
        private readonly BannerApiService _bannerApiService;
        private readonly PreOrderApiService _preOrderApiService;

        public HomeController(
            ProductApiService productApiService,
            BrandApiService brandApiService,
            BannerApiService bannerApiService,
            PreOrderApiService preOrderApiService) : base(bannerApiService)
        {
            _productApiService = productApiService;
            _brandApiService = brandApiService;
            _bannerApiService = bannerApiService;
            _preOrderApiService = preOrderApiService;
        }

        // Homepage

        public async Task<IActionResult> Index()
        {
            var featuredFilter = new ProductFilterDto
            {
                Page = 1,
                PageSize = 4,
                SortBy = "newest",
                
            };

            var limitedFilter = new ProductFilterDto
            {
                Page = 1,
                PageSize = 4,
                IsLimitedEdition = true,
                
            };

            var preOrderFilter = new ProductFilterDto
            {
                Page = 1,
                PageSize = 4,
                IsPreOrder = true,
                
            };

            var featured = await _productApiService.GetAllAsync(featuredFilter);
            var limited = await _productApiService.GetAllAsync(limitedFilter);
            var preOrders = await _productApiService.GetAllAsync(preOrderFilter);
            var brands = await _brandApiService.GetAllAsync();
            var banners = await _bannerApiService.GetActiveAsync();
            var logo = await _bannerApiService.GetLogoUrlAsync();

            ViewBag.Banners = banners?.Data ?? new List<BannerResponseDto>();
            ViewBag.SiteLogo = logo;

            ViewBag.FeaturedProducts = featured?.Data?.Items?
                .Where(p => !p.IsLimitedEdition && !p.IsPreOrder)
                .ToList() ?? new List<ProductResponseDto>();

            ViewBag.LimitedProducts = limited?.Data?.Items?
                .Where(p => p.IsLimitedEdition && !p.IsPreOrder)
                .ToList() ?? new List<ProductResponseDto>();

            ViewBag.PreOrderProducts = preOrders?.Data?.Items?
                .Where(p => p.IsPreOrder)
                .ToList() ?? new List<ProductResponseDto>();

            ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();

            return View();
        }

        // Shop / Product Listing

        public async Task<IActionResult> Shop(
            string? search, int? brandId,
            string? scale, decimal? minPrice,
            decimal? maxPrice, bool? isLimitedEdition, bool? isPreOrder,
            bool? inStockOnly, string sortBy = "newest",
            int page = 1)
        {
            var filter = new ProductFilterDto
            {
                Search = search,
                BrandId = brandId,
                Scale = scale,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                IsLimitedEdition = isLimitedEdition,
                IsPreOrder = isPreOrder,
                InStockOnly = inStockOnly,
                SortBy = sortBy,
                Page = page,
                PageSize = 12
            };

            var products = await _productApiService.GetAllAsync(filter);
            var brands = await _brandApiService.GetAllAsync();

            ViewBag.Brands = brands?.Data ?? new List<BrandResponseDto>();
            ViewBag.Filter = filter;

            return View(products?.Data);
        }

        //Brand Detail

        public async Task<IActionResult> Brands()
        {
            var brands = await _brandApiService.GetAllAsync();
            return View(brands?.Data ?? new List<BrandResponseDto>());
        }


        // Product Detail

        public async Task<IActionResult> Product(string slug)
        {
            var result = await _productApiService.GetBySlugAsync(slug);
            if (result == null || !result.Success)
                return RedirectToAction("Shop");

            // Get related products from same brand
            var relatedFilter = new ProductFilterDto
            {
                BrandId = result.Data!.BrandId,
                Page = 1,
                PageSize = 4
            };

            var related = await _productApiService.GetAllAsync(relatedFilter);
            ViewBag.RelatedProducts = related?.Data?.Items
                .Where(p => p.Id != result.Data.Id)
                .Take(3)
                .ToList() ?? new List<ProductResponseDto>();

            return View(result.Data);
        }

        // Pre-Order Detail

        public async Task<IActionResult> PreOrder(int productId)
        {
            // Must be logged in
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account",
                    new { returnUrl = $"/Home/PreOrder?productId={productId}" });

            var product = await _productApiService.GetByIdAsync(productId);
            if (product == null || !product.Success)
                return RedirectToAction("Shop");

            // Check if product is still pre-orderable
            if (!product.Data!.IsPreOrder || !product.Data.IsPreOrderOpen)
            {
                TempData["Error"] = "Pre-order is no longer available " +
                    "for this product";
                return RedirectToAction("Product",
                    new { slug = product.Data.Slug });
            }

            var bkash = await _preOrderApiService.GetBkashNumberAsync();

            ViewBag.BkashNumber = bkash?.Data ?? "01749028100";
            ViewBag.Product = product.Data;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SubmitPreOrder(
            CreatePreOrderDto dto)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!dto.TermsAccepted)
            {
                TempData["Error"] = "You must accept the terms and conditions";
                return RedirectToAction("PreOrder",
                    new { productId = dto.ProductId });
            }

            var result = await _preOrderApiService.CreateAsync(dto);

            if (result == null || !result.Success)
            {
                TempData["Error"] = result?.Message ?? "Failed to place pre-order";
                return RedirectToAction("PreOrder",
                    new { productId = dto.ProductId });
            }

            TempData["PreOrderNumber"] = result.Data?.PreOrderNumber;
            return RedirectToAction("PreOrderSuccess");
        }

        public IActionResult PreOrderSuccess()
        {
            ViewBag.PreOrderNumber = TempData["PreOrderNumber"];
            return View();
        }

        public async Task<IActionResult> Terms()
        {
            return View();
        }
    }
}
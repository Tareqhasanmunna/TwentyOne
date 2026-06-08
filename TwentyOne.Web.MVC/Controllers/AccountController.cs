using Microsoft.AspNetCore.Mvc;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Web.MVC.Services;
using TwentyOne.Web.MVC.ViewModels;

namespace TwentyOne.Web.MVC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AuthApiService _authApiService;
        private readonly OrderApiService _orderApiService;
        private readonly PreOrderApiService _preOrderApiService;

        public AccountController(
            AuthApiService authApiService,
            OrderApiService orderApiService,
            PreOrderApiService preOrderApiService,
            BannerApiService bannerApiService) : base(bannerApiService)
        {
            _authApiService = authApiService;
            _orderApiService = orderApiService;
            _preOrderApiService = preOrderApiService;
        }

        // Login

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                return RedirectToAction("Index", "Home");

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

            HttpContext.Session.SetString("JwtToken", result.Data!.Token);
            HttpContext.Session.SetString("UserName", result.Data.FullName);
            HttpContext.Session.SetString("UserEmail", result.Data.Email);
            HttpContext.Session.SetString("UserRoles",
                string.Join(",", result.Data.Roles));

            var roles = result.Data.Roles;
            if (roles.Contains("Admin") || roles.Contains("SuperAdmin"))
                return RedirectToAction("Dashboard", "Admin");

            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        // Register

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var result = await _authApiService.RegisterAsync(dto);

            if (result == null || !result.Success)
            {
                ModelState.AddModelError("",
                    result?.Message ?? "Registration failed");
                return View(dto);
            }

            TempData["Success"] = "Account created! Please login.";
            return RedirectToAction("Login");
        }

        // Logout

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // My Orders

        public async Task<IActionResult> MyOrders()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var orders = await _orderApiService.GetMyOrdersAsync();
            var preOrders = await _preOrderApiService.GetMyPreOrdersAsync();

            ViewBag.Orders = orders?.Data ??
                new List<OrderResponseDto>();
            ViewBag.PreOrders = preOrders?.Data ??
                new List<PreOrderResponseDto>();

            return View();
        }

        public async Task<IActionResult> OrderDetail(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _orderApiService.GetByIdAsync(id);
            if (result == null || !result.Success)
                return RedirectToAction("MyOrders");

            return View(result.Data);
        }

        public async Task<IActionResult> DownloadOrderPdf(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var pdf = await _orderApiService.DownloadOrderPdfAsync(id);
            if (pdf == null)
            {
                TempData["Error"] = "Could not generate PDF";
                return RedirectToAction("MyOrders");
            }

            // Get order number for filename
            var order = await _orderApiService.GetByIdAsync(id);
            var fileName = $"Order-{order?.Data?.OrderNumber ?? id.ToString()}.pdf";

            return File(pdf, "application/pdf", fileName);
        }

        //Pre Order

        public async Task<IActionResult> MyPreOrders()
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var result = await _preOrderApiService.GetMyPreOrdersAsync();
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
                return RedirectToAction("MyPreOrders");

            return View(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> CancelPreOrder(int id)
        {
            var result = await _preOrderApiService.CancelAsync(id);
            if (result?.Success == true)
                TempData["Success"] = "Pre-order cancelled";
            else
                TempData["Error"] = result?.Message ?? "Cannot cancel";

            return RedirectToAction("MyPreOrders");
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TwentyOne.Web.MVC.Services;

namespace TwentyOne.Web.MVC.Controllers
{
    public class BaseController : Controller
    {
        private readonly BannerApiService _bannerApiService;

        public BaseController(BannerApiService bannerApiService)
        {
            _bannerApiService = bannerApiService;
        }

        // Runs before every action in any controller
        // that inherits from BaseController
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var logo = await _bannerApiService.GetLogoUrlAsync();
            ViewBag.SiteLogo = logo;
            await next();
        }
    }
}

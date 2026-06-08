using Microsoft.AspNetCore.Mvc;

namespace TwentyOne.Web.MVC.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController(
            TwentyOne.Web.MVC.Services.BannerApiService bannerApiService) : base(bannerApiService)
        { 
        }

        [Route("Error/404")]
        public IActionResult NotFound404()
        {
            return View("NotFound");
        }

        [Route("Error/{statusCode}")]
        public IActionResult HandleError(int statusCode) {
            ViewBag.StatusCode = statusCode;
            ViewBag.Message = statusCode switch
            {
                404 => "Page not found",
                403 =>"Access denied",
                500 => "Internal server error",
                _ => "Something went wrong"
            };
            return View("Error");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyOne.Shared.Models;
using TwentyOne.Web.API.Services;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ImagesController : ControllerBase
    {
        private readonly ImageUploadService _imageUploadService;

        public ImagesController(ImageUploadService imageUploadService)
        {
            _imageUploadService = imageUploadService;
        }

        // POST api/images/upload
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>
                    .FailResponse("No file provided"));

            var url = await _imageUploadService.UploadImageAsync(file);

            if (url == null)
                return BadRequest(ApiResponse<string>
                    .FailResponse(
                        "Invalid file. Only JPG, PNG, WEBP under 5MB allowed"));

            return Ok(ApiResponse<string>.SuccessResponse(url, "Image uploaded"));
        }

        // POST api/images/upload-logo
        [HttpPost("upload-logo")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>
                    .FailResponse("No file provided"));

            var url = await _imageUploadService.UploadBrandLogoAsync(file);

            if (url == null)
                return BadRequest(ApiResponse<string>
                    .FailResponse(
                        "Invalid file. Only JPG, PNG, WEBP under 2MB allowed"));

            return Ok(ApiResponse<string>.SuccessResponse(url, "Logo uploaded"));
        }

        // DELETE api/images/delete
        [HttpDelete("delete")]
        public IActionResult Delete([FromQuery] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return BadRequest(ApiResponse<string>
                    .FailResponse("Image URL required"));

            _imageUploadService.DeleteImage(imageUrl);
            return Ok(ApiResponse<string>
                .SuccessResponse("Image deleted"));
        }
    }
}

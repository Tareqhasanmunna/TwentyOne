using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TwentyOne.BLL.Helpers;
using TwentyOne.DAL.Entities;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.DTOs.Responses;
using TwentyOne.Shared.Models;
using TwentyOne.Web.API.Services;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("PublicCors")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly ISiteSettingRepository _siteSettingRepository;
        private readonly ImageUploadService _imageUploadService;
        private readonly CacheService _cache;

        public BannersController(
            IBannerRepository bannerRepository,
            ISiteSettingRepository siteSettingRepository,
            ImageUploadService imageUploadService,
            CacheService cache)
        {
            _bannerRepository = bannerRepository;
            _siteSettingRepository = siteSettingRepository;
            _imageUploadService = imageUploadService;
            _cache = cache;
        }

        // GET api/banners/active
        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
        {
            var banners = await _bannerRepository.GetActiveAsync();
            var result = banners.Select(b => new BannerResponseDto
            {
                Id = b.Id,
                ImageUrl = b.ImageUrl,
                Title = b.Title,
                Subtitle = b.Subtitle,
                DisplayOrder = b.DisplayOrder,
                IsActive = b.IsActive
            }).ToList();

            _cache.Set("banners_active", result,
                TimeSpan.FromMinutes(10));

            return Ok(ApiResponse<List<BannerResponseDto>>
                .SuccessResponse(result));
        }

        // GET api/banners
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var banners = await _bannerRepository.GetAllAsync();
            var result = banners.Select(b => new BannerResponseDto
            {
                Id = b.Id,
                ImageUrl = b.ImageUrl,
                Title = b.Title,
                Subtitle = b.Subtitle,
                DisplayOrder = b.DisplayOrder,
                IsActive = b.IsActive
            }).ToList();

            return Ok(ApiResponse<List<BannerResponseDto>>
                .SuccessResponse(result));
        }

        // POST api/banners
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Create(
            [FromForm] CreateBannerDto dto, IFormFile image)
        {
            if (image == null)
                return BadRequest(ApiResponse<string>
                    .FailResponse("Banner image is required"));

            var imageUrl = await _imageUploadService
                .UploadImageAsync(image);

            if (imageUrl == null)
                return BadRequest(ApiResponse<string>
                    .FailResponse("Image upload failed"));

            var banner = new Banner
            {
                ImageUrl = imageUrl,
                Title = dto.Title,
                Subtitle = dto.Subtitle,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _bannerRepository.CreateAsync(banner);

            return Ok(ApiResponse<BannerResponseDto>
                .SuccessResponse(new BannerResponseDto
                {
                    Id = created.Id,
                    ImageUrl = created.ImageUrl,
                    Title = created.Title,
                    Subtitle = created.Subtitle,
                    DisplayOrder = created.DisplayOrder,
                    IsActive = created.IsActive
                }, "Banner created successfully"));
        }

        // DELETE api/banners/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                return NotFound(ApiResponse<string>
                    .FailResponse("Banner not found"));

            _imageUploadService.DeleteImage(banner.ImageUrl);
            await _bannerRepository.DeleteAsync(banner);

            return Ok(ApiResponse<string>
                .SuccessResponse("Banner deleted"));
        }

        // POST api/banners/1/toggle
        [HttpPost("{id}/toggle")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Toggle(int id)
        {
            var banner = await _bannerRepository.GetByIdAsync(id);
            if (banner == null)
                return NotFound(ApiResponse<string>
                    .FailResponse("Banner not found"));

            banner.IsActive = !banner.IsActive;
            await _bannerRepository.UpdateAsync(banner);

            return Ok(ApiResponse<string>
                .SuccessResponse("Banner toggled"));
        }

        // GET api/banners/logo
        [HttpGet("logo")]
        public async Task<IActionResult> GetLogo()
        {
            var logo = await _siteSettingRepository
                .GetValueAsync("SiteLogo");
            return Ok(ApiResponse<string>.SuccessResponse(logo ?? ""));
        }

        // POST api/banners/logo
        [HttpPost("logo")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UploadLogo(IFormFile file)
        {
            if (file == null)
                return BadRequest(ApiResponse<string>
                    .FailResponse("Logo file is required"));

            var url = await _imageUploadService
                .UploadBrandLogoAsync(file);

            if (url == null)
                return BadRequest(ApiResponse<string>
                    .FailResponse("Upload failed"));

            await _siteSettingRepository.SetValueAsync("SiteLogo", url);

            return Ok(ApiResponse<string>
                .SuccessResponse(url, "Logo updated"));
        }

        // GET api/banners/delivery-charges
        [HttpGet("delivery-charges")]
        public async Task<IActionResult> GetDeliveryCharges()
        {
            var inside = await _siteSettingRepository
                .GetValueAsync("DeliveryCharge_InsideDhaka");
            var outside = await _siteSettingRepository
                .GetValueAsync("DeliveryCharge_OutsideDhaka");

            var result = new
            {
                insideDhaka = int.Parse(inside ?? "80"),
                outsideDhaka = int.Parse(outside ?? "130")
            };

            _cache.Set("delivery_charges", result,
                TimeSpan.FromMinutes(10));

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }

        // POST api/banners/delivery-charges (Admin only)
        [HttpPost("delivery-charges")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateDeliveryCharges(
            [FromBody] UpdateDeliveryChargesDto dto)
        {
            await _siteSettingRepository
                .SetValueAsync("DeliveryCharge_InsideDhaka",
                    dto.InsideDhaka.ToString());
            await _siteSettingRepository
                .SetValueAsync("DeliveryCharge_OutsideDhaka",
                    dto.OutsideDhaka.ToString());

            _cache.Remove("delivery_charges");

            return Ok(ApiResponse<string>
                .SuccessResponse("Delivery charges updated"));
        }

        // GET api/banners/bkash-number
        [HttpGet("bkash-number")]
        public async Task<IActionResult> GetBkashNumber()
        {
            var number = await _siteSettingRepository
                .GetValueAsync("BkashNumber");
            return Ok(ApiResponse<string>
                .SuccessResponse(number ?? "01749028100"));
        }

        // POST api/banners/bkash
        [HttpPost("bkash")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateBkashNumber(
            [FromBody] string number)
        {
            if (string.IsNullOrEmpty(number))
                return BadRequest(ApiResponse<string>
                    .FailResponse("Number required"));

            await _siteSettingRepository
                .SetValueAsync("BkashNumber", number);

            return Ok(ApiResponse<string>
                .SuccessResponse("bKash number updated"));
        }
    }
}

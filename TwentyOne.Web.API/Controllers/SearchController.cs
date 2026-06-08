using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TwentyOne.DAL.Repositories.Interfaces;
using TwentyOne.Shared.DTOs.Requests;
using TwentyOne.Shared.Models;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("PublicCors")]
    public class SearchController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;

        public SearchController(
            IProductRepository productRepository,
            IBrandRepository brandRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
        }

        // GET api/search?q=hot+wheels
        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
                return Ok(ApiResponse<object>.SuccessResponse(new
                {
                    products = new List<object>(),
                    brands = new List<object>()
                }));

            // Search products
            var productFilter = new ProductFilterDto
            {
                Search = q,
                Page = 1,
                PageSize = 5
            };

            var (products, _) = await _productRepository
                .GetAllAsync(productFilter);

            // Search brands
            var brands = await _brandRepository.GetAllAsync();
            var matchedBrands = brands
                .Where(b => b.Name.Contains(q,
                    StringComparison.OrdinalIgnoreCase))
                .Take(3)
                .ToList();

            var result = new
            {
                products = products.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Slug,
                    p.Price,
                    p.Scale,
                    BrandName = p.Brand?.Name,
                    ImageUrl = p.Images.FirstOrDefault()?.ImageUrl
                }),
                brands = matchedBrands.Select(b => new
                {
                    b.Id,
                    b.Name,
                    b.LogoUrl,

                })
            };

            return Ok(ApiResponse<object>.SuccessResponse(result));
        }
    }
}

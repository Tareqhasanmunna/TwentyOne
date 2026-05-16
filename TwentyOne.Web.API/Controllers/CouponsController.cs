using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.Shared.DTOs.Requests;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly ICouponService _couponService;

        public CouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        // GET api/coupons (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _couponService.GetAllAsync();
            return Ok(result);
        }

        // GET api/coupons/1 (Admin only)
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _couponService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        // POST api/coupons (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _couponService.CreateAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // DELETE api/coupons/1 (Admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _couponService.DeleteAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // POST api/coupons/1/toggle (Admin only)
        [HttpPost("{id}/toggle")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Toggle(int id)
        {
            var result = await _couponService.ToggleActiveAsync(id);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // POST api/coupons/apply (Public)
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] ApplyCouponDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _couponService.ApplyCouponAsync(dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }
    }
}

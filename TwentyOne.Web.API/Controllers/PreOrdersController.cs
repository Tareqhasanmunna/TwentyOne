using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.Shared.DTOs.Requests;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreOrdersController : ControllerBase
    {
        private readonly IPreOrderService _preOrderService;

        public PreOrdersController(IPreOrderService preOrderService)
        {
            _preOrderService = preOrderService;
        }

        // GET api/preorders (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _preOrderService.GetAllAsync();
            return Ok(result);
        }

        // GET api/preorders/my
        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMine()
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;
            var result = await _preOrderService
                .GetMyPreOrdersAsync(userId!);
            return Ok(result);
        }

        // GET api/preorders/1
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _preOrderService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        // POST api/preorders
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(
            [FromBody] CreatePreOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;
            var result = await _preOrderService
                .CreateAsync(userId!, dto);

            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // PUT api/preorders/1/status (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdatePreOrderStatusDto dto)
        {
            var result = await _preOrderService
                .UpdateStatusAsync(id, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // POST api/preorders/1/cancel
        [HttpPost("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;
            var result = await _preOrderService
                .CancelAsync(id, userId!);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // GET api/preorders/bkash
        [HttpGet("bkash")]
        public async Task<IActionResult> GetBkashNumber()
        {
            var result = await _preOrderService.GetBkashNumberAsync();
            return Ok(result);
        }

        // POST api/preorders/bkash (Admin only)
        [HttpPost("bkash")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateBkashNumber(
            [FromBody] string number)
        {
            if (string.IsNullOrEmpty(number))
                return BadRequest();

            // Store in site settings
            return Ok();
        }
    }
}

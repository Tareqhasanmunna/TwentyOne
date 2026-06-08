using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TwentyOne.BLL.Services.Interfaces;
using TwentyOne.Shared.DTOs.Requests;

namespace TwentyOne.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly OrderPdfService _pdfService;

        public OrdersController(IOrderService orderService, OrderPdfService pdfService)
        {
            _orderService = orderService;
            _pdfService = pdfService;
        }

        // GET api/orders (Admin only)
        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _orderService.GetAllAsync();
            return Ok(result);
        }

        // GET api/orders/my-orders
        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.GetMyOrdersAsync(userId!);
            return Ok(result);
        }

        // GET api/orders/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _orderService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound(result);
            return Ok(result);
        }

        // POST api/orders
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> PlaceOrder([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string? userId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            var result = await _orderService.PlaceOrderAsync(userId!, dto);

            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // PUT api/orders/1/status (Admin only)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UpdateStatus(
            int id, [FromBody] UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateStatusAsync(id, dto);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // POST api/orders/1/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _orderService.CancelOrderAsync(id, userId!);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);
        }

        // GET api/orders/track/{orderNumber}
        [HttpGet("track/{orderNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackOrder(string orderNumber)
        {
            var result = await _orderService
                .GetByOrderNumberAsync(orderNumber);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        // GET api/orders/1/pdf
        [HttpGet("{id}/pdf")]
        
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var result = await _orderService.GetByIdAsync(id);
            if (!result.Success)
                return NotFound();

            var pdf = _pdfService.GenerateOrderPdf(result.Data!);

            return File(pdf, "application/pdf",
                $"Order-{result.Data!.OrderNumber}.pdf");
        }

        [HttpGet("pdf/{orderNumber}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadPdfByNumber(
            string orderNumber)
        {
            var result = await _orderService
                .GetByOrderNumberAsync(orderNumber);

            if (!result.Success || result.Data == null)
                return NotFound();

            var pdf = _pdfService.GenerateOrderPdf(result.Data);

            return File(pdf, "application/pdf",
                $"Order-{orderNumber}.pdf");
        }
    }
}

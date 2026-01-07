using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderControllerAPI : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderControllerAPI(OrderService orderService)
        {
            _orderService = orderService;
        }

        // Przykład: POST /api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] ZamowienieAddDto dto)
        {
            var result = await _orderService.CreateOrderAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Zamowienie);
        }

        // Przykład: GET /api/orders/15
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var result = await _orderService.GetOrderAsync(id);

            if (!result.Success || result.Zamowienie == null)
                return NotFound(new { message = result.ErrorMessage });

            return Ok(result.Zamowienie);
        }

        // Przykład: GET /api/orders/user/abc123
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersForUser(string userId)
        {
            var orders = await _orderService.GetOrdersForUserAsync(userId);
            return Ok(orders);
        }

        // Przykład: GET /api/orders/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // Przykład: GET /api/orders/search/15
        [HttpGet("search/{id}")]
        public async Task<IActionResult> SearchOrder(int id)
        {
            var result = await _orderService.GetOrderAsync(id);

            if (!result.Success || result.Zamowienie == null)
                return NotFound(new { message = result.ErrorMessage });

            return Ok(result.Zamowienie);
        }
    }
}

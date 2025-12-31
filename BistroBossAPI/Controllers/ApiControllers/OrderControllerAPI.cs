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
    }
}

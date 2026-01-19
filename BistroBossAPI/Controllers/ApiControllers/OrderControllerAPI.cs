using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Diagnostics;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/orders")]
    [ApiController]
    [AllowAnonymous]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrderControllerAPI : ControllerBase
    {
        private readonly OrderService _orderService;
        private readonly UserManager<Uzytkownik> _userManager;

        public OrderControllerAPI(OrderService orderService, UserManager<Uzytkownik> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // Przykład: POST /api/orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] ZamowienieAddDto dto)
        {
            if (dto.UserId != "GUEST" && _userManager.IsNotAllowedInEndpoint(dto.UserId, User))
            {
                return Unauthorized();
            }

            var result = await _orderService.CreateOrderAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(result.Zamowienie);
        }

        // Przykład: GET /api/orders/15
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            if(!await _userManager.IsAdminAsync(User) && !await _orderService.IsOrderForUser(_userManager.GetUserId(User)))
            {
                return Unauthorized();
            }

            var result = await _orderService.GetOrderAsync(id);

            if (!result.Success || result.Zamowienie == null)
                return NotFound(new { message = result.ErrorMessage });

            return Ok(result.Zamowienie);
        }

        // Przykład: GET /api/orders/user/abc123
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetOrdersForUser(string userId)
        {
            if (userId != "GUEST" && _userManager.IsNotAllowedInEndpoint(userId, User))
            {
                return Unauthorized();
            }

            var orders = await _orderService.GetOrdersForUserAsync(userId);
            return Ok(orders);
        }

        // Przykład: GET /api/orders/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
        {
            if(!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // Przykład: GET /api/orders/search/15
        [HttpGet("search/{id}")]
        public async Task<IActionResult> SearchOrder(int id)
        {
            if (!await _userManager.IsAdminAsync(User))
            {
                return Unauthorized();
            }

            var result = await _orderService.GetOrderAsync(id);

            if (!result.Success || result.Zamowienie == null)
                return NotFound(new { message = result.ErrorMessage });

            return Ok(result.Zamowienie);
        }
    }
}

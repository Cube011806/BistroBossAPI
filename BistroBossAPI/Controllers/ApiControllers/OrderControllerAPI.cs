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
        // POST /api/orders/reorder/15
        [HttpPost("reorder/{id}")]
        public async Task<IActionResult> ReOrder(int id, [FromBody] ReOrderRequestDto dto)
        {
            var userId = _userManager.GetUserId(User);

            if (userId == null)
                return Unauthorized();

            // Sprawdzenie czy użytkownik ma aktywne zamówienie
            if (await _orderService.HasActiveOrderAsync(userId))
            {
                return BadRequest(new { message = "Żeby ponownie coś zamówić, nie możesz mieć zamówienia aktualnie w realizacji!" });
            }

            // Pobranie starego zamówienia
            var oldOrderResult = await _orderService.GetOrderEntityAsync(id);

            if (!oldOrderResult.Success || oldOrderResult.Zamowienie == null)
                return NotFound(new { message = oldOrderResult.ErrorMessage });

            var oldOrder = oldOrderResult.Zamowienie;

            // Tworzenie nowego zamówienia
            var newOrder = await _orderService.ReOrderAsync(oldOrder, userId, dto);

            if (!newOrder.Success)
                return BadRequest(new { message = newOrder.ErrorMessage });

            return Ok(new { id = newOrder.Zamowienie.Id });
        }
        // POST /api/orders/review
        [HttpPost("review")]
        public async Task<IActionResult> AddReview([FromBody] AddReviewDto dto)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            if (dto.Ocena < 1 || dto.Ocena > 5 || string.IsNullOrWhiteSpace(dto.Komentarz))
                return BadRequest(new { message = "Wszystkie pola muszą być wypełnione!" });

            var result = await _orderService.AddReviewAsync(dto, userId);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Pomyślnie dodano opinię!" });
        }

    }
}

using BistroBossAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BistroBossAPI.Controllers.ApiControllers
{
    [Route("api/baskets")]
    [ApiController]
    public class BasketControllerAPI : ControllerBase
    {
        private readonly BasketService _basketService;

        // Wstrzykujemy Serwis
        public BasketControllerAPI(BasketService basketService)
        {
            _basketService = basketService;
        }

        //Przykład: GET /api/baskets/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBasket(string userId)
        {
            var basket = await _basketService.GetBasketForUserAsync(userId);
            return Ok(basket);
        }

        //Przykład: POST /api/baskets?userId=123&produktId=123
        [HttpPost]
        public async Task<IActionResult> AddToBasket([FromQuery] string userId, [FromQuery] int produktId)
        {
            var result = await _basketService.AddToBasketAsync(userId, produktId);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = "Dodano do koszyka" });
        }

    }
}

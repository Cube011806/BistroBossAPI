using BistroBossAPI.Models;
using BistroBossAPI.Models.Dto;
using BistroBossAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            return Ok(new { message = "Dodano produkt do koszyka!" });
        }

        //Przykład: DELETE /api/baskets/{userId}/products/{koszykProduktId}
        [HttpDelete("{userId}/products/{koszykProduktId}")]
        public async Task<IActionResult> RemoveFromBasket(string userId, int koszykProduktId)
        {
            var result = await _basketService.RemoveFromBasketAsync(userId, koszykProduktId);

            if (!result.Success)
                return BadRequest(new { message = result.ErrorMessage });

            return Ok(new { message = result.Message });
        }

        //Przykład: POST /api/baskets/guest
        [HttpPost("guest")]
        public async Task<IActionResult> SetGuestBasket([FromBody] KoszykGuestDto dto)
        {
            var success = await _basketService.SetGuestBasketAsync(dto);

            if (!success)
                return BadRequest(new { message = "Nie udało się zapisać koszyka gościa." });

            return Ok();
        }
    }
}
